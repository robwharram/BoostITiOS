
using System;

using Foundation;
using UIKit;
using BoostIT.Models;
using System.Collections.Generic;
using System.Linq;
using SQLite;
using BoostIT.DataAccess;
using System.IO;
using ObjCRuntime;
using BoostIT;
using BoostIT.Tasks;

namespace BoostITiOS
{
	public partial class UploadList : UIViewController
	{
		private bool initialLoad = true;
		private List<int> selectedVehicleIds;
		//private LoadingOverlay loadingOverlay;
		private int UploadID;
		private int selectedDealershipID;

		public UploadList (int UploadID, int DealershipID) : base ("UploadList", null)
		{
			this.UploadID = UploadID;
			this.selectedDealershipID = DealershipID;
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

			selectedVehicleIds = new List<int> ();

			this.View.BackgroundColor = UIColor.FromPatternImage (UIImage.FromBundle("bg.jpg"));

			LoadVehicles ();

			btnDone.Clicked += (object sender, EventArgs e) => { NavigationController.PopViewController(true); };	
			btnUpload.Clicked += UploadClicked;
		}

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);

			if (!initialLoad) 
				LoadVehicles ();
			else
				initialLoad = false;
		}

		private void UploadClicked(object sender, EventArgs e)
		{
			if (selectedVehicleIds.Count () <= 0) {
				Controls.OkDialog ("No Vehicles Selected", "You must select at least 1 vehicle to upload.");
				return;
			}

			foreach (int id in selectedVehicleIds)
				Console.WriteLine("selectedId=" + id);

			string selectedDealershipName = NSUserDefaults.StandardUserDefaults.StringForKey("SelectedDealershipName");

			LaunchUploadVehiclesProgress(selectedDealershipID, selectedDealershipName, false);

			//int UserID = (int)NSUserDefaults.StandardUserDefaults.IntForKey("UserID");
			//Console.WriteLine ("UserID=" + UserID);

			//loadingOverlay = Controls.ProgressDialog (this, "Loading Dealers...");
			//Async.BackgroundProcess (bw_GetDealers, bw_GetDealersCompleted, UserID);
		}

		/*public void dealershipSelected(string name, string value)
		{
			Console.WriteLine ("selected dealer=" + value);
			Dealership dealer = listOfDealers.FirstOrDefault (d => d.DealershipName == value);
			if (dealer == null) {
				Controls.OkDialog ("Invalid Dealer", "Selected dealer does not exist, please select a different dealer.");
				return;
			}

			LaunchUploadVehiclesProgress (dealer.DealershipID, dealer.DealershipName, (listOfDealers.Count() == 1) ? false : true);
		}*/

		private void LaunchUploadVehiclesProgress(int DealershipID, string DealershipName, bool showDealerScreen = false)
		{
			//insert into UploadDealer table
			using (Connection sqlConn = new Connection(SQLiteBoostDB.GetDBPath()))
				new UploadDB(sqlConn).InsertUploadDealer(UploadID, DealershipID, DealershipName, selectedVehicleIds);

			if (showDealerScreen)
				NavigationController.PushViewController (new UploadListDealers (UploadID), true);
			else
				NavigationController.PushViewController (new UploadProgress (UploadID), true);
		}

		private void LoadVehicles()
		{
			List<VehicleWithImages> listOfVehicles = new List<VehicleWithImages> ();
			using (Connection sqlConn = new Connection(SQLiteBoostDB.GetDBPath()))
				listOfVehicles = new VehicleDB(sqlConn).GetVehicleList(selectedDealershipID);

			tvUpload.Delegate = new TableViewDelegate (this, listOfVehicles);
			tvUpload.DataSource = new TableViewDataSource (this, listOfVehicles);
			tvUpload.ReloadData ();
		}

		/*private void bw_GetDealers(object sender, System.ComponentModel.DoWorkEventArgs e)
		{
			int UserID = (int)e.Argument;
			if (UserID <= 0) {
				e.Result = listOfDealers;
				return;
			}

			listOfDealers = new GetDealers().GetDealersFromService(UserID);

			e.Result = listOfDealers;
		}

		private void bw_GetDealersCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
		{
			loadingOverlay.Hide ();

			if (e.Error != null) {
				Controls.OkDialog("Error", "There was an error getting the dealers. Error was: " + e.Error.Message);
				return;
			}

			if (listOfDealers == null || listOfDealers.Count <= 0) {
				Controls.OkDialog("No Dealers Found", "We were unable to find any dealers that you have access to, please login again.");
				return;
			}

			if (listOfDealers.Count == 1) {
				dealershipSelected("Dealership",listOfDealers[0].DealershipName);
				return;
			}

			PresentModalViewController (new ListScreenSearch (dealershipSelected, listOfDealers.Select(d => d.DealershipName).ToArray(), "Dealership"), true);
		}*/

		private class TableViewDelegate : UITableViewDelegate
		{
			private List<VehicleWithImages> list;
			private UploadList controller;

			public TableViewDelegate(UploadList Controller, List<VehicleWithImages> list)
			{
				this.controller = Controller;
				this.list = list;
			}

			public override nfloat GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
			{
				return 75f;
			}

			public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
			{				
				int vehicleId = list [indexPath.Row].vehicle.ID;

				if (controller.selectedVehicleIds.Contains (vehicleId))
					controller.selectedVehicleIds.Remove (vehicleId);
				else
					controller.selectedVehicleIds.Add (vehicleId);
									
				tableView.ReloadData ();					
			}
		}

		private class TableViewDataSource : UITableViewDataSource
		{
			static NSString kCellIdentifier = new NSString ("MyIdentifier");
			public List<VehicleWithImages> list;
			public UploadList controller;

			public TableViewDataSource (UploadList parentController, List<VehicleWithImages> list)
			{
				this.list = list;
				controller = parentController;
			}

			public override nint RowsInSection (UITableView tableview, nint section)
			{
				return list.Count();
			}

			public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
			{
				ChangeVehicleCell cell = tableView.DequeueReusableCell(kCellIdentifier) as ChangeVehicleCell;

				if (cell == null) {
					var views = NSBundle.MainBundle.LoadNib ("ChangeVehicleCell", tableView, null);
					cell = Runtime.GetNSObject (views.ValueAt (0)) as ChangeVehicleCell;
					//cell.Layer.CornerRadius = 7.5f;
					cell.Layer.BorderColor = UIColor.White.CGColor;
					cell.Layer.BorderWidth = 0.5f;
				}

				cell.Tag = indexPath.Row;

				VehicleWithImages vehicleWithImages = list [indexPath.Row];
				Vehicle vehicle = vehicleWithImages.vehicle;

				string imagePath = Graphics.GetImagePath (vehicle.ID);
				string thumbPath = "";

				foreach (Image img in vehicleWithImages.images.OrderBy(i => i.FileNumber)) {
					if (!string.IsNullOrWhiteSpace (img.FileName)) {
						if (img.FileName.StartsWith ("https://")) {
							thumbPath = img.FileName;
							break;
						} else {
							thumbPath = Path.Combine (imagePath, "thumb_" + img.FileNumber + ".jpg");
							if (File.Exists (thumbPath)) {
								Console.WriteLine ("thumbpath exists");
								break;
							}
						}
						thumbPath = ""; //Thumb file does not exist - set to empty variable.
					}
				}

				string YearMakeModel = vehicle.Year + " " + vehicle.Make + " " + vehicle.Model;
				string Price = (vehicle.Price.HasValue) ? vehicle.Price.Value.ToString ("C0").ToNullableString (string.Empty) : string.Empty;

				cell.UpdateCell (YearMakeModel, vehicle.StockNumber, vehicle.VIN, Price);

				if (string.IsNullOrWhiteSpace (thumbPath))
					cell.ImageView.Image = UIImage.FromBundle ("nophoto.png");
				else if (thumbPath.StartsWith ("https://"))
					cell.ImageView.Image = Graphics.FromUrl (thumbPath);
				else
					cell.ImageView.Image = UIImage.FromFile(thumbPath);

				if (controller.selectedVehicleIds.Contains (vehicle.ID))
					cell.Accessory = UITableViewCellAccessory.Checkmark;
				else
					cell.Accessory = UITableViewCellAccessory.None;

				return cell;
			}
		}
	}
}

