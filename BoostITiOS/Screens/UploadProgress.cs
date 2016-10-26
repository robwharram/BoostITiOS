
using System;

using Foundation;
using UIKit;
using BoostIT.Tasks;
using System.Threading;
using BoostITPortableLibrary.Models;
using SQLite;
using BoostIT.DataAccess;
using BoostIT.Models;
using System.Collections.Generic;
using BoostIT;
using System.IO;
using System.Linq;

namespace BoostITiOS
{
	public partial class UploadProgress : UIViewController
	{
		private List<UploadDealerVehiclesList> listDealerVehicles;
		private bool paused = false, skipVehicle = false, overwriteAll = false;
		private int userId = 0, mobileUploadID = 0, mobileVehicleId = 0, UploadID = 0;
		private long currentFileSize = 0, maxValue = 0, totalBytesCompleted = 0, maxImageSize = 0;
		private ManualResetEvent pauseEvent = new ManualResetEvent(true);
		private ManualResetEvent stopEvent = new ManualResetEvent(false);
		private UploadVehicle uploader;
		private Thread uploadThread;
		private Duplicate dupeResponse;
		private List<ImageForUpload> listOfImagesToCheck;
		private ToastOverlay currentToast;
		private LoadingOverlay loadingOverlay;

		public UploadProgress (int UploadID) : base ("UploadProgress", null)
		{
			this.UploadID = UploadID;
			userId = (int)NSUserDefaults.StandardUserDefaults.IntForKey("UserID");
		}

		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			Controls.RestrictRotation (true);

			//events
			btnPause.Clicked += btnPause_Clicked;
			btnCancel.Clicked += btnCancel_Clicked;
			uploader = new UploadVehicle(onDuplicateResponse, onVehicleInsertSuccess, onImageFileError, onUploadWorkCompleted, onMobileUploadIDRetrieved, onUploadError);
			uploader.progressChanged += new UploadVehicle.progressChangedHandler(progressHasChanged);

			//initialize progress bars
			pbPrepareImage.SetProgress(0f, false);
			pbTotal.SetProgress (0f, false);
			pbCurrentVehicle.SetProgress (0f, false);

			//load dealers/vehicles from database
			using (Connection sqlConn = new Connection(SQLiteBoostDB.GetDBPath()))
				listDealerVehicles = new UploadDB(sqlConn).GetDealersToUpload(UploadID);

			//Load Images to check - this has to run on the UI thread for Orientation to work...
			listOfImagesToCheck = CalculateMaxForPrepageImages();

			//start the upload
			uploadThread = new Thread(StartUpload);

			//check for duplicates
			DuplicateCheck();
		}

		void btnCancel_Clicked (object sender, EventArgs e)
		{
			PauseUpload();
			Controls.YesNoDialog("Confirm Cancel", "Are you sure you want to cancel?", StopUpload, ResumeUpload);
		}

		void btnPause_Clicked (object sender, EventArgs e)
		{
			if (paused) {
				paused = false;
				ResumeUpload();
				btnPause.Title = "Pause";
				currentToast = Controls.ShowToast (this, currentToast, "Upload Resumed");
			}
			else {
				paused = true;
				PauseUpload();
				btnPause.Title = "Resume";
				currentToast = Controls.ShowToast (this, currentToast, "Upload Paused");
			}
		}

		void DuplicateCheck()
		{
			//Check for duplicates
			int numberOfDuplicates = getTotalDuplicateCount();
			if (numberOfDuplicates > 0) {      
				UIAlertView alert = new UIAlertView ("Duplicates Detected", "We detected " + numberOfDuplicates + " duplicates in your upload.  Please choose one of the two options below.",
					null, "Confirm Each Dupe", "Overwrite All");
				alert.Clicked += (object sender, UIButtonEventArgs e) => {
					if (e.ButtonIndex == 1) 
						goAheadWithUpload(true);
					else 
						goAheadWithUpload(false);
				};
				alert.Show ();
			}
			else
				goAheadWithUpload(false);     
		}

		private void goAheadWithUpload(bool overwrite)
		{
			overwriteAll = overwrite;

			//start upload
			Controls.WifiDialog("You are not connected to WIFI, are you sure you want to continue?", delegate { uploadThread.Start(); });
		}

		public void PauseUpload()
		{
			pauseEvent.Reset();
			uploader.PauseUpload();
		}

		public void ResumeUpload()
		{
			if (uploader != null) {
				pauseEvent.Set();
				uploader.ResumeUpload();
			}
		}

		public void StopUpload()
		{
			//stop the image upload
			uploader.StopUpload();

			//signal the stop event
			stopEvent.Set();

			//resume if paused so that it can get to the stop event
			pauseEvent.Set();

			//show the user dialog
			this.InvokeOnMainThread(delegate {
				Controls.OkDialog("Cancel Complete", "The upload has been cancelled.  Any vehicles that were successfully uploaded before the cancellation have been uploaded successfully.", Done);
			});
		}

		private void progressHasChanged(object sender, progressChangedEventArgs args)
		{
			this.InvokeOnMainThread(delegate {
				updateTotalProgress(args.bytesRead);
				//pbIVtotal.Progress = (int)args.uploadedBytes;
				float percentComplete = (float)args.uploadedBytes / (float)maxImageSize;
				Console.WriteLine("CurrentVehicleMax=" + maxImageSize);
				Console.WriteLine("CurrentVehicleProgress=" + args.uploadedBytes);
				Console.WriteLine("CurrentVehiclepercentComplete=" + percentComplete);
				pbCurrentVehicle.Progress = percentComplete;
				lblCurrentVehicle.Text = "Image Upload Progress: " + args.uploadedBytes.getByteSize() + "/" + currentFileSize.getByteSize();
				//tvIVupload.SetText("Image Upload Progress: " + args.uploadedBytes.getByteSize() + "/" + currentFileSize.getByteSize(), null);
			});
		}

		private void updateTotalProgress(long bytesCompleted)
		{
			this.InvokeOnMainThread(delegate {
				totalBytesCompleted += bytesCompleted;
				float percentComplete = (float)totalBytesCompleted / (float)maxValue;
				Console.WriteLine("TotalMax=" + maxValue);
				Console.WriteLine("TotalProgress=" + totalBytesCompleted);
				Console.WriteLine("TotalPercentComplete=" + percentComplete);
				//pbtotal.Progress = (int)totalBytesCompleted;
				pbTotal.Progress = percentComplete;
				//tvTotalUpload.SetText("Total Upload Progress: " + totalBytesCompleted.getByteSize() + "/" + maxValue.getByteSize(), null);
				lblTotal.Text = "Total Upload Progress: " + totalBytesCompleted.getByteSize() + "/" + maxValue.getByteSize();
			});
		}

		private void onImageFileError(string message)
		{
			this.InvokeOnMainThread(delegate {
				Controls.YesNoDialog("Error", "There was an error during the upload, please check your connection.  The error was: " + message + ".  Would you like to retry?", delegate {
					uploader.ResumeUpload();
				}, delegate {
					StopUpload();
				});
			});
		}

		private void onUploadError(string message, out bool retryYesNo)
		{
			pauseEvent.Reset(); //wait for user response..

			bool retry = false;
			this.InvokeOnMainThread(delegate {
				Controls.YesNoDialog("Error", "There was an error during the upload, please check your connection.  The error was: " + message + ".  Would you like to retry?", delegate
					{
						retry = true;
						pauseEvent.Set();
					}, delegate
					{
						StopUpload();
						pauseEvent.Set();
					});
			});

			retryYesNo = false;

			pauseEvent.WaitOne(Timeout.Infinite);
			if (stopEvent.WaitOne(0))
				return;            

			retryYesNo = retry;
			uploader.ResumeUpload();

		}

		private void onVehicleInsertSuccess(int vehicleId)
		{
			mobileVehicleId = vehicleId;
		}

		private void onDuplicateResponse(Duplicate duplicate)
		{
			dupeResponse = duplicate;
		}

		private void onUploadWorkCompleted()
		{
			if (!paused) //make sure the user hasn't paused the upload
				ResumeUpload();
		}

		private void onMobileUploadIDRetrieved(int UploadID)
		{
			mobileUploadID = UploadID;
		}

		private int GetTotalVehicles()
		{
			int count = 0;
			foreach (UploadDealerVehiclesList dealerVehicles in listDealerVehicles)
				foreach (int vehicleId in dealerVehicles.VehicleIDs)
					count++;

			return count;
		}

		private int getTotalDuplicateCount()
		{
			using (Connection sqlConn = new Connection(SQLiteBoostDB.GetDBPath())) {
				VehicleDB vdb = new VehicleDB(sqlConn);
				List<DuplicateCheck> vehiclesToCheck = new List<DuplicateCheck>();

				foreach (UploadDealerVehiclesList dealerVehicles in listDealerVehicles) {
					foreach (int id in dealerVehicles.VehicleIDs) {
						Vehicle vehicle = vdb.GetVehicle(id);
						vehiclesToCheck.Add(new DuplicateCheck() { DealershipID = dealerVehicles.DealershipID, StockNumber = vehicle.StockNumber, VIN = vehicle.VIN });
					}
				}

				if (vehiclesToCheck.Count > 0) 
					return uploader.GetTotalDuplicatesFound(vehiclesToCheck);
			}

			return 0;
		}

		private List<ImageForUpload> prepareImagesForUpload()
		{
			this.InvokeOnMainThread(delegate { nbUpload.TopItem.Title = "Preparing Images"; });    

			List<ImageForUpload> list = new List<ImageForUpload>();

			int numberOfImages = listOfImagesToCheck.Count;

			int counter = 0;
			foreach (ImageForUpload imgToCheck in listOfImagesToCheck) {
				pauseEvent.WaitOne(Timeout.Infinite);
				if (stopEvent.WaitOne(0))
					return null;

				counter++;
				this.InvokeOnMainThread(delegate { lblPrepareImage.Text = "Preparing image " + counter + " of " + numberOfImages; });

				if (File.Exists(imgToCheck.filePath)) {
					ImageForUpload ifu = Graphics.CopyAndResizeFile(imgToCheck, imgToCheck.dealershipId);
					if (ifu != null)
						list.Add(ifu);
				}

				this.InvokeOnMainThread(delegate { pbPrepareImage.Progress = (float)counter/(float)numberOfImages; });
			}            

			this.InvokeOnMainThread(delegate {
				lblPrepareImage.Text = "Image Preparation Complete";
				pbPrepareImage.Progress = 100f;
				nbUpload.TopItem.Title = "Start Uploading";                
			});

			return list; 
		}

		private List<ImageForUpload> CalculateMaxForPrepageImages()
		{
			List<ImageForUpload> list = new List<ImageForUpload>();
			using (Connection sqlConn = new Connection(SQLiteBoostDB.GetDBPath())) {
				VehicleImagesDB vidb = new VehicleImagesDB(sqlConn);
				DamageDB ddb = new DamageDB(sqlConn);

				foreach (UploadDealerVehiclesList dealerVehicles in listDealerVehicles) {
					foreach (int VehicleId in dealerVehicles.VehicleIDs) {
						List<Image> listOfImages = vidb.GetImagesList(VehicleId);
						List<Damage> listOfDamagesWithImages = ddb.GetDamagesWithImages(VehicleId);

						if ((listOfImages == null || listOfImages.Count <= 0) && (listOfDamagesWithImages == null || listOfDamagesWithImages.Count <= 0))
							continue;

						foreach (Image image in listOfImages)
							if (!image.FileName.Contains("http://"))
								list.Add(new ImageForUpload() { vehicleId = VehicleId, dealershipId = dealerVehicles.DealershipID, fileNumber = image.FileNumber, filePath = image.FileName, damageId = 0 });

						foreach (Damage dmg in listOfDamagesWithImages)
							if (!dmg.FileName.Contains("http://"))
								list.Add(new ImageForUpload() { vehicleId = VehicleId, dealershipId = dealerVehicles.DealershipID, filePath = dmg.FileName, damageId = dmg.ID });
					}
				}

			}
			return list;
		}

		private long CalculateMaxProgressValue(List<ImageForUpload> listImages)
		{
			long retval = 0;

			using (Connection sqlConn = new Connection(SQLiteBoostDB.GetDBPath())) {
				DamageDB ddb = new DamageDB(sqlConn);
				TireDB tdb = new TireDB(sqlConn);
				PaintDB pdb = new PaintDB(sqlConn);

				foreach (UploadDealerVehiclesList dealerVehicles in listDealerVehicles) {
					foreach (int VehicleId in dealerVehicles.VehicleIDs) {
						//add 20kB for each car (ws call to insert vehicle 10k + ws call to commit vehicle 10k + ws call to insert features 10k)
						retval += 30720; //30k - 30*1024

						//only 1 service call is made for tires/paint/damages - add 10k for each
						if (ddb.GetDamageCount(VehicleId) >= 1)
							retval += 10240;
						if (tdb.GetTireCount(VehicleId) >= 1)
							retval += 10240;
						if (pdb.GetPaintCount(VehicleId) >= 1)
							retval += 10240;

						List<ImageForUpload> listImagesForVehicle = listImages.Where(i => i.vehicleId == VehicleId).ToList();
						foreach (ImageForUpload ifu in listImagesForVehicle)
						{
							//add 10kB for each image (ws call to insert image)
							retval += 10240; //10k - 10*1024
							retval += ifu.fileSize;
						}
					}
				}
			}            

			return retval;
		}

		private void CheckSkipVehicle(Duplicate duplicateData)
		{
			PauseUpload();
			string message = "";
			string secondButton = "Allow Duplicate";
			if (duplicateData.TotalDuplicates == 1) {
				secondButton = "Overwrite";
				message = "We detected the following conflict with another vehicle in your inventory.";
				if (duplicateData.NumberOfStockDupes > 0)
					message += System.Environment.NewLine + "-Stock Number \"" + duplicateData.StockNumber + "\" already exists.";
				if (duplicateData.NumberOfVINDupes > 0)
					message += System.Environment.NewLine + "-VIN \"" + duplicateData.VIN + "\" already exists.";
			}
			else {
				message = "We detected the following conflicts with other vehicles in your inventory.";
				if (duplicateData.NumberOfStockDupes > 0) {
					if (duplicateData.NumberOfStockDupes == 1)
						message += System.Environment.NewLine + "-A vehicle already uses Stock Number \"" + duplicateData.StockNumber + "\".";
					else
						message += System.Environment.NewLine + "-" + duplicateData.NumberOfStockDupes + " vehicles already use Stock Number \"" + duplicateData.StockNumber + "\".";
				}
				if (duplicateData.NumberOfVINDupes > 0) {
					if (duplicateData.NumberOfVINDupes == 1)
						message += System.Environment.NewLine + "-A vehicle already uses VIN \"" + duplicateData.VIN + "\".";
					else
						message += System.Environment.NewLine + "-" + duplicateData.NumberOfVINDupes + " vehicles already use VIN \"" + duplicateData.VIN + "\".";
				}
			}

			this.InvokeOnMainThread(delegate {
				var alert = new UIAlertView("Duplicate Detected",
					message,null, "Skip", secondButton);

				alert.Clicked += (object sender, UIButtonEventArgs e) => {
					if (e.ButtonIndex == 1) {
						this.skipVehicle = false;
						ResumeUpload();
					} else {
						this.skipVehicle = true;
						ResumeUpload();
					}
				};
			});
		}

		public void Upload_Complete()
		{
			this.InvokeOnMainThread(delegate {
				lblTotal.Text="Upload Complete";
				lblCurrentVehicle.Text="Upload Complete";
				pbTotal.Progress = 100f;
				Controls.OkDialog("Upload Complete", "Upload Complete", Done);
			});
		}

		public void Upload_Failed(string errorMessage)
		{
			this.InvokeOnMainThread(delegate {
				lblTotal.Text="Upload Failed";
				lblCurrentVehicle.Text="Upload Failed";
				pbTotal.Progress = 100f;
				Controls.OkDialog("Upload Failed", "Upload Failed. Error=" + errorMessage, Done);
			});
		}

		private void Done()
		{
			this.InvokeOnMainThread(delegate { 
				loadingOverlay = Controls.ProgressDialog(this, "Cleaning Temporary Images");
				Async.BackgroundProcess(bw_CleanTemp, bw_CleanTempCompleted);            
			});            
		}

		private void bw_CleanTemp(object sender, System.ComponentModel.DoWorkEventArgs e)
		{
			//Clean up the temporary images...
			try {
				Graphics.CleanUpTempImages();
			}
			catch {} //ignore errors here
		}

		private void bw_CleanTempCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
		{
			this.InvokeOnMainThread(delegate {
				uploadThread.Join();
				loadingOverlay.Hide();
				NavigationController.PushViewController(new Home(), true);
			});            
		}

		private void StartUpload()
		{
			//prepare all images to upload
			List<ImageForUpload> listOfImagesToUpload = prepareImagesForUpload();
			if (stopEvent.WaitOne(0))
				return;

			//prepare counters and progress bar
			int totalVehicleCount = GetTotalVehicles();
			maxValue = CalculateMaxProgressValue(listOfImagesToUpload); //calculate max value for total progress (total kB of images)
			//this.InvokeOnMainThread(delegate { pbTotal.Max = (int)maxValue; });

			using (Connection sqlConn = new Connection(SQLiteBoostDB.GetDBPath()))
			{
				//initialize vars
				VehicleDB vdb = new VehicleDB(sqlConn);
				VehicleFeaturesDB vfdb = new VehicleFeaturesDB(sqlConn);
				DamageDB ddb = new DamageDB(sqlConn);
				TireDB tdb = new TireDB(sqlConn);
				PaintDB pdb = new PaintDB(sqlConn);
				int vehCounter = 0;

				foreach (UploadDealerVehiclesList dealerVehicles in listDealerVehicles) {
					mobileUploadID = 0;

					PauseUpload();
					uploader.GetMobileUploadID(userId, dealerVehicles.DealershipID);
					pauseEvent.WaitOne(Timeout.Infinite);
					if (stopEvent.WaitOne(0))
						return;

					if (mobileUploadID <= 0) {
						Upload_Failed("Unable to retrieve a valid upload key.");
						return;
					}

					foreach (int vehicleId in dealerVehicles.VehicleIDs)
					{
						pauseEvent.WaitOne(Timeout.Infinite);
						if (stopEvent.WaitOne(0))
							return;

						//initialize variables for next vehicle
						skipVehicle = false;
						mobileVehicleId = 0;
						dupeResponse = null;
						vehCounter++;
						Vehicle vehicle = vdb.GetVehicle(vehicleId);

						//reset progress for images and set the vehicle counter
						this.InvokeOnMainThread(delegate { nbUpload.TopItem.Title = "Uploading Vehicle " + vehCounter + "/" + totalVehicleCount; });

						//check if vehicle already exists
						if (!overwriteAll && (!string.IsNullOrEmpty(vehicle.StockNumber) || !string.IsNullOrEmpty(vehicle.VIN)))
						{
							PauseUpload();
							uploader.GetDuplicateData(dealerVehicles.DealershipID, vehicle.StockNumber, vehicle.VIN);
							pauseEvent.WaitOne(Timeout.Infinite);
							if (stopEvent.WaitOne(0))
								return;

							if (dupeResponse != null && dupeResponse.TotalDuplicates > 0)
							{
								CheckSkipVehicle(dupeResponse);

								//wait until the user responds
								pauseEvent.WaitOne(Timeout.Infinite);
								if (skipVehicle)
									continue;
							}
						}

						//Upload vehicle data
						PauseUpload();

						uploader.UploadVehicleData(mobileUploadID, vehicle);

						//wait for upload of data to complete
						pauseEvent.WaitOne(Timeout.Infinite);
						if (stopEvent.WaitOne(0))
							return;
						if (skipVehicle || mobileVehicleId <= 0)
							continue;

						//update the progress for the vehicle data upload
						updateTotalProgress(10240);

						//upload feature data
						PauseUpload();
						uploader.UploadFeatureData(mobileVehicleId, vfdb.GetSelectedFeatures(vehicle.ID));

						//wait for upload of features to complete
						pauseEvent.WaitOne(Timeout.Infinite);
						if (stopEvent.WaitOne(0))
							return;
						if (skipVehicle)
							continue;

						//update the progress for the features data upload
						updateTotalProgress(10240);

						//damages
						PauseUpload();
						uploader.UploadDamageData(mobileVehicleId, ddb.GetDamageList(vehicle.ID));
						pauseEvent.WaitOne(Timeout.Infinite);//wait for upload of damages to complete
						if (stopEvent.WaitOne(0))
							return;
						if (skipVehicle)
							continue;
						updateTotalProgress(10240);  //add damage progress

						//paint
						PauseUpload();
						uploader.UploadPaintData(mobileVehicleId, pdb.GetPaintList(vehicle.ID));
						pauseEvent.WaitOne(Timeout.Infinite);//wait for upload of paint to complete
						if (stopEvent.WaitOne(0))
							return;
						if (skipVehicle)
							continue;
						updateTotalProgress(10240);  //add paint progress

						//tires
						PauseUpload();
						uploader.UploadTireData(mobileVehicleId, tdb.GetTireList(vehicle.ID));
						pauseEvent.WaitOne(Timeout.Infinite);//wait for upload of tires to complete
						if (stopEvent.WaitOne(0))
							return;
						if (skipVehicle)
							continue;
						updateTotalProgress(10240);  //add tire progress

						//loop images to upload
						if (listOfImagesToUpload != null && listOfImagesToUpload.Count > 0)
						{
							List<ImageForUpload> listOfImagesForVehicle = listOfImagesToUpload.Where(i => i.vehicleId == vehicleId && i.dealershipId == dealerVehicles.DealershipID).ToList();

							int imgCounter = 0;
							foreach (ImageForUpload img in listOfImagesForVehicle)
							{
								pauseEvent.WaitOne(Timeout.Infinite);
								if (stopEvent.WaitOne(0))
									return;

								//initialize image variables
								imgCounter++;
								currentFileSize = (int)img.fileSize;
								this.InvokeOnMainThread(delegate {
									pbCurrentVehicle.Progress = 0f;
									//pbIVtotal.Max = (int)img.fileSize;
									maxImageSize = (int)img.fileSize;
									lblCurrentVehicle.Text = "Uploading image " + imgCounter + " of " + listOfImagesForVehicle.Count() + " for this vehicle.";
								});

								if (img.damageId > 0) {
									pauseEvent.Reset();//don't want to pause the "uploader" object, only local - don't use "PauseUpload()"
									uploader.UploadDamageFile(mobileUploadID, mobileVehicleId, img.damageId, File.ReadAllBytes(img.filePath));
									pauseEvent.WaitOne(Timeout.Infinite);
									if (stopEvent.WaitOne(0))
										return;
									if (skipVehicle)
										break;

									continue;
								}

								pauseEvent.Reset();//don't want to pause the "uploader" object, only local - don't use "PauseUpload()"
								uploader.UploadFile(mobileUploadID, mobileVehicleId, img.fileNumber, File.ReadAllBytes(img.filePath));

								pauseEvent.WaitOne(Timeout.Infinite);
								if (stopEvent.WaitOne(0))
									return;
								if (skipVehicle)
									break;

								//insert image to database
								PauseUpload();
								uploader.UploadImageData(mobileVehicleId, img.fileNumber);

								pauseEvent.WaitOne(Timeout.Infinite);
								if (stopEvent.WaitOne(0))
									return;
								if (skipVehicle)
									break;

								//update the progress for the image update
								updateTotalProgress(10240);
							}
						}

						if (stopEvent.WaitOne(0)) //check if the upload has been cancelled before committing data
							return;
						if (skipVehicle) //check if the vehicle is skipped                    
							continue;

						//commit vehicle
						PauseUpload();
						uploader.CommitVehicle(mobileVehicleId);

						pauseEvent.WaitOne(Timeout.Infinite);
						if (stopEvent.WaitOne(0))
							return;
						if (skipVehicle)
							continue;

						//Delete the vehicle from the device	
						vdb.DeleteVehicle(vehicle.ID);

						//update the progress for the commit
						updateTotalProgress(10240);
					}
				}
			}

			if (!stopEvent.WaitOne(0))
				Upload_Complete();
		}
	}
}

