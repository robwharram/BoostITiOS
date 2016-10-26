
using System;

using Foundation;
using UIKit;
using System.IO;
using SQLite;
using BoostIT.Models;
using System.Collections.Generic;
using BoostIT.DataAccess;
using System.Linq;

namespace BoostITiOS
{
	public partial class Photos : UIViewController
	{
		private int vehicleid, currentFileNumber=0;
		private string imagePath="";
		private UIButton[] btnArray;
		private List<Image> listVehicleImages;
		private bool swap=false, imagesInitiallyLoaded = false;
		private MultiSelectController picker;
		private LoadingOverlay loadingOverlay;

		public Photos (int VehicleID) : base ("Photos", null)
		{
			vehicleid = VehicleID;
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

			btnDone.Clicked += (object sender, EventArgs e) => { Done(); };
			btnMultiSelect.Clicked += BtnMultiSelect_Clicked;

			svPhotos.ContentSize = new CoreGraphics.CGSize (svPhotos.Bounds.Width, 1350f);

			BuildArray ();

			imagePath = Graphics.GetImagePath (vehicleid);

		}

		void BtnMultiSelect_Clicked (object sender, EventArgs e)
		{
			listVehicleImages.Clear ();
			for (int i = 1; i <= 32; i++)
				DisplayThumbnail (i);

			GC.Collect ();
			
			picker = MultiSelectController.Instance;
			picker.MaximumImagesCount = 32;
			picker.vehicleId = vehicleid;
			picker.Completion.ContinueWith (t => {
				//Console.WriteLine("hellow continuewith");
				InvokeOnMainThread(delegate {
					LoadImagesFromDatabase();
					for (int i=1; i<=32; i++)
						DisplayThumbnail (i);	
				});
			});
			PresentModalViewController (picker, true);
		}

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);

			if (!imagesInitiallyLoaded) {
				//load all images from the database and then show the thumbnails
				LoadImagesFromDatabase();
				foreach (Image image in listVehicleImages) 
					DisplayThumbnail(image.FileNumber);  
				imagesInitiallyLoaded = true;
			}
		}

		private void Done()
		{
			DismissModalViewController(true);
		}

		private void LoadImagesFromDatabase()
		{
			using (Connection sqlConn = new Connection(SQLiteBoostDB.GetDBPath()))
				listVehicleImages =  new VehicleImagesDB(sqlConn).GetImagesList(vehicleid);

			foreach (Image image in listVehicleImages)
				Console.WriteLine ("filePath = " + image.FileName);
		}

		private void DisplayThumbnail(int fileNumber)
		{
			Image image = listVehicleImages.FirstOrDefault(i => i.FileNumber == fileNumber);
			if (image == null) {
				btnArray [fileNumber].SetBackgroundImage (UIImage.FromBundle ("nophoto.png"), UIControlState.Normal);
				return;
			}

			UIView loadingView = showLoadingOverlay (fileNumber);

			Async.BackgroundProcess(bw_getThumb, bw_getThumbCompleted, new ThumbHolder() { image = image, LoadingView = loadingView });
		}

		private void bw_getThumb(object sender, System.ComponentModel.DoWorkEventArgs e)
		{
			if (e.Argument == null)
				return;

			ThumbHolder holder = (ThumbHolder)e.Argument;
			if (holder == null)
				return;

			if (holder.image.FileName.StartsWith ("https://"))
				holder.thumbNail = Graphics.FromUrl (holder.image.FileName);
			else {
				string thumbPath = Path.Combine (imagePath, "thumb_" + holder.image.FileNumber + ".jpg");
				if (File.Exists (thumbPath))
					holder.thumbNail = UIImage.FromFile (thumbPath);
			}
			e.Result = holder;
		}

		private void bw_getThumbCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
		{
			if (e.Error != null) 
				return;

			ThumbHolder holder = (ThumbHolder)e.Result;

			Graphics.HideActivitySpinner (holder.LoadingView);

			if (holder.thumbNail == null) {
				btnArray [holder.image.FileNumber].SetBackgroundImage (UIImage.FromBundle ("nophoto.png"), UIControlState.Normal);
				return;
			}

			btnArray [holder.image.FileNumber].SetBackgroundImage (holder.thumbNail, UIControlState.Normal);

			holder = null;
		}

		private void imageClicked (int fileNumber)
		{
			Console.WriteLine ("You clicked file number: " + fileNumber);
			if (swap) {
				swapimage(currentFileNumber, fileNumber);
				swap = false;
				LoadImagesFromDatabase();
				return;
			}

			currentFileNumber = fileNumber;
			if (listVehicleImages.Count(i => i.FileNumber == fileNumber) <= 0)
				TakeOrSelect();
			else
				SelectPhotoAction();
		}

		private void TakeOrSelect()
		{
			var actionSheet = new UIActionSheet("Select a source", null, "Cancel", "Take a Picture", "Open Camera Roll"){
				Style = UIActionSheetStyle.Default
			};
			actionSheet.Clicked += delegate (object sender, UIButtonEventArgs args) {
				if (args.ButtonIndex == 0) 
					TakePicture();
				else if (args.ButtonIndex == 1)
					SelectPicture();				
			};
			actionSheet.ShowInView (View);
		}

		private void SelectPhotoAction()
		{
			var actionSheet = new UIActionSheet("Select an Action", null, "Cancel", "Take a Picture", "Open Camera Roll", "Move Image", "Delete Image", "Enlarge Image") {
				Style = UIActionSheetStyle.Default
			};
			actionSheet.Clicked += delegate (object sender, UIButtonEventArgs args) {
				if (args.ButtonIndex == 0) 
					TakePicture();
				else if (args.ButtonIndex == 1)
					SelectPicture();				
				else if (args.ButtonIndex == 2)
					MovePicture();
				else if (args.ButtonIndex == 3)
					deleteImage();
				else if (args.ButtonIndex == 4)
					enlargeImage();					
			};
			actionSheet.ShowInView (View);
		}

		private UIView showLoadingOverlay(int fileNumber)
		{
			return Graphics.GetActivitySpinner (svPhotos, btnArray [fileNumber].Frame);
		}

		public void CameraCallBack (NSDictionary dic)
		{
			Console.WriteLine("camera callback");

			Async.BackgroundProcess (bw_CameraSaveImage, bw_CameraSaveImageCompleted, new CameraHolder() { CameraImage = (UIImage)dic.Values[0], LoadingView = showLoadingOverlay (currentFileNumber), FileNumber = currentFileNumber });
		}

		private void bw_CameraSaveImage(object sender, System.ComponentModel.DoWorkEventArgs e)
		{
			CameraHolder holder = (CameraHolder)e.Argument;

			string filePath = Path.Combine(imagePath,"image_" + holder.FileNumber + ".jpg");
			string thumbPath = Path.Combine(imagePath,"thumb_" + holder.FileNumber + ".jpg");
			NSError fileError, thumbError;

			try
			{
				if (!Directory.Exists (imagePath))
					Directory.CreateDirectory (imagePath);
				if (File.Exists (filePath))
					File.Delete (filePath);
				if (File.Exists (thumbPath))
					File.Delete (thumbPath);
				
				using (NSData jpegData = holder.CameraImage.AsJPEG ()) {
					jpegData.Save (filePath, NSDataWritingOptions.Atomic, out fileError);
					UIImage thumb = Graphics.ResizeImage (UIImage.LoadFromData (jpegData), 100f, 75f);
					thumb.AsJPEG ().Save (thumbPath, NSDataWritingOptions.Atomic, out thumbError);

					if (fileError!=null)
						throw new Exception("Error saving image. " + fileError.ToString());
					if (thumbError!=null)
						throw new Exception("Error saving thumbnail. " + thumbError.ToString());

					Graphics.SaveImage(holder.FileNumber, filePath, vehicleid);
				}
			}
			catch (Exception ex) {
				holder.ErrorMsg = ex.Message;
			}

			e.Result = holder;
		}

		private void bw_CameraSaveImageCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
		{
			CameraHolder holder = (CameraHolder)e.Result;

			Graphics.HideActivitySpinner (holder.LoadingView);

			if (!string.IsNullOrWhiteSpace (holder.ErrorMsg))
				Controls.OkDialog ("Error Saving Image", "There was an error saving the image from the camera. The error was: " + holder.ErrorMsg);
			
			LoadImagesFromDatabase();
			DisplayThumbnail(holder.FileNumber);
		}

		private void TakePicture()
		{            
			Camera.TakePicture (this, CameraCallBack);
		}

		private void SelectPicture()
		{
			Camera.SelectPicture (this, CameraCallBack);
		}

		private void MovePicture()
		{            
			swap = true; 
			Controls.OkDialog ("Move Image", "Select where you would like to move the image to", null);
		}

		private void deleteImage()
		{
			using (Connection sqlConn = new Connection(SQLiteBoostDB.GetDBPath()))
				new VehicleImagesDB(sqlConn).RemoveImage(vehicleid, currentFileNumber);

			btnArray [currentFileNumber].SetBackgroundImage (UIImage.FromBundle ("nophoto.png"), UIControlState.Normal);
			LoadImagesFromDatabase();
		}

		private void enlargeImage()
		{
			string filePath = Path.Combine (imagePath, "image_" + currentFileNumber + ".jpg");
			if (!File.Exists (filePath))
				return;

			UIImageViewClickable iv = new UIImageViewClickable ();
			iv.Frame = this.View.Frame;
			iv.OnClick += () => { 
				iv.RemoveFromSuperview();
				if (UIDevice.CurrentDevice.Orientation != UIDeviceOrientation.Portrait)
					UIDevice.CurrentDevice.SetValueForKey (NSObject.FromObject(UIInterfaceOrientation.Portrait), new NSString("orientation"));
				
				Controls.RestrictRotation(true);
			};
			iv.ContentMode = UIViewContentMode.ScaleAspectFit;
			iv.AutosizesSubviews = true;
			iv.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
			iv.Image = UIImage.FromFile (filePath);

			Controls.RestrictRotation (false);
			this.View.AddSubview (iv);
		}

		private void swapimage(int oldFileNumber, int newFileNumber)
		{            
			using (Connection sqlConn = new Connection(SQLiteBoostDB.GetDBPath())) {
				VehicleImagesDB vidb = new VehicleImagesDB(sqlConn);
				Image oldImage = vidb.GetImage(vehicleid, oldFileNumber);
				Image newImage = vidb.GetImage(vehicleid, newFileNumber);
				vidb.RemoveImage(vehicleid, oldFileNumber);
				vidb.RemoveImage(vehicleid, newFileNumber);

				oldImage.FileNumber = newFileNumber;
				vidb.AddImage(oldImage);

				if (newImage != null) {
					newImage.FileNumber = oldFileNumber;
					vidb.AddImage(newImage);
				}
			}            

			string filePathMoving = Path.Combine(imagePath,"image_" + oldFileNumber + ".jpg");
			string thumbPathMoving = Path.Combine(imagePath,"thumb_" + oldFileNumber + ".jpg");
			string filePathMovedTo = Path.Combine(imagePath,"image_" + newFileNumber + ".jpg");
			string thumbPathMovedTo = Path.Combine(imagePath,"thumb_" + newFileNumber + ".jpg");

			try
			{
				if (File.Exists(filePathMoving))
				{
					File.Move(filePathMoving,filePathMovedTo.Replace("image_","movedimage_"));
					if (File.Exists(thumbPathMoving))
						File.Move(thumbPathMoving,thumbPathMovedTo.Replace("thumb_","movedthumb_"));
				}

				if (File.Exists(filePathMovedTo))
				{
					File.Move(filePathMovedTo,filePathMoving.Replace("image_","movedimage_"));
					if (File.Exists(thumbPathMovedTo))
						File.Move(thumbPathMovedTo,thumbPathMoving.Replace("thumb_","movedthumb_"));
				}

				if (File.Exists(filePathMoving.Replace("image_","movedimage_")))
				{
					File.Move(filePathMoving.Replace("image_","movedimage_"),filePathMoving.Replace("moveimage_","image_"));
					if (File.Exists(thumbPathMoving.Replace("thumb_","movedthumb_")))
						File.Move(thumbPathMoving.Replace("thumb_","movedthumb_"),thumbPathMoving.Replace("movedthumb_","thumb_"));
				}

				if (File.Exists(filePathMovedTo.Replace("image_","movedimage_")))
				{
					File.Move(filePathMovedTo.Replace("image_","movedimage_"),filePathMovedTo.Replace("moveimage_","image_"));
					if (File.Exists(thumbPathMovedTo.Replace("thumb_","movedthumb_")))
						File.Move(thumbPathMovedTo.Replace("thumb_","movedthumb_"),thumbPathMovedTo.Replace("movedthumb_","thumb_"));
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("error moving images: " + ex.ToString());
			}

			LoadImagesFromDatabase();
			DisplayThumbnail(oldFileNumber);
			DisplayThumbnail(newFileNumber);
		}

		private void BuildArray()
		{
			btnArray = new UIButton[33];

			btnArray [1] = btnImage1;
			btnArray [2] = btnImage2;
			btnArray [3] = btnImage3;
			btnArray [4] = btnImage4;
			btnArray [5] = btnImage5;
			btnArray [6] = btnImage6;
			btnArray [7] = btnImage7;
			btnArray [8] = btnImage8;
			btnArray [9] = btnImage9;
			btnArray [10] = btnImage10;
			btnArray [11] = btnImage11;
			btnArray [12] = btnImage12;
			btnArray [13] = btnImage13;
			btnArray [14] = btnImage14;
			btnArray [15] = btnImage15;
			btnArray [16] = btnImage16;
			btnArray [17] = btnImage17;
			btnArray [18] = btnImage18;
			btnArray [19] = btnImage19;
			btnArray [20] = btnImage20;
			btnArray [21] = btnImage21;
			btnArray [22] = btnImage22;
			btnArray [23] = btnImage23;
			btnArray [24] = btnImage24;
			btnArray [25] = btnImage25;
			btnArray [26] = btnImage26;
			btnArray [27] = btnImage27;
			btnArray [28] = btnImage28;
			btnArray [29] = btnImage29;
			btnArray [30] = btnImage30;
			btnArray [31] = btnImage31;
			btnArray [32] = btnImage32;

			btnArray [1].TouchDown += delegate { imageClicked (1); };
			btnArray [2].TouchDown += delegate { imageClicked (2); };
			btnArray [3].TouchDown += delegate { imageClicked (3); };
			btnArray [4].TouchDown += delegate { imageClicked (4); };
			btnArray [5].TouchDown += delegate { imageClicked (5); };
			btnArray [6].TouchDown += delegate { imageClicked (6); };
			btnArray [7].TouchDown += delegate { imageClicked (7); };
			btnArray [8].TouchDown += delegate { imageClicked (8); };
			btnArray [9].TouchDown += delegate { imageClicked (9); };
			btnArray [10].TouchDown += delegate { imageClicked (10); };
			btnArray [11].TouchDown += delegate { imageClicked (11); };
			btnArray [12].TouchDown += delegate { imageClicked (12); };
			btnArray [13].TouchDown += delegate { imageClicked (13); };
			btnArray [14].TouchDown += delegate { imageClicked (14); };
			btnArray [15].TouchDown += delegate { imageClicked (15); };
			btnArray [16].TouchDown += delegate { imageClicked (16); };
			btnArray [17].TouchDown += delegate { imageClicked (17); };
			btnArray [18].TouchDown += delegate { imageClicked (18); };
			btnArray [19].TouchDown += delegate { imageClicked (19); };
			btnArray [20].TouchDown += delegate { imageClicked (20); };
			btnArray [21].TouchDown += delegate { imageClicked (21); };
			btnArray [22].TouchDown += delegate { imageClicked (22); };
			btnArray [23].TouchDown += delegate { imageClicked (23); };
			btnArray [24].TouchDown += delegate { imageClicked (24); };
			btnArray [25].TouchDown += delegate { imageClicked (25); };
			btnArray [26].TouchDown += delegate { imageClicked (26); };
			btnArray [27].TouchDown += delegate { imageClicked (27); };
			btnArray [28].TouchDown += delegate { imageClicked (28); };
			btnArray [29].TouchDown += delegate { imageClicked (29); };
			btnArray [30].TouchDown += delegate { imageClicked (30); };
			btnArray [31].TouchDown += delegate { imageClicked (31); };
			btnArray [32].TouchDown += delegate { imageClicked (32); };
		}
	}
}

