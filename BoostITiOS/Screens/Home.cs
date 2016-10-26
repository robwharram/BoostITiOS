
using System;
using System.Drawing;

using Foundation;
using UIKit;
using SQLite;
using BoostIT.DataAccess;
using System.ComponentModel;
using BoostIT.Models;
using System.Collections.Generic;
using BoostIT.Tasks;
using System.Linq;

namespace BoostITiOS
{
	public partial class Home : UIViewController
	{
		private bool assetsRequireDownload = false;
		private LoadingOverlay loadingOverlay;
		private List<Dealership> listOfDealers;
		private UILabel labelSelectedDealer;


		public Home () : base ("Home", null)
		{
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

			SQLiteBoostDB.CheckAndCreateDatabase();

			this.View.BackgroundColor = UIColor.FromPatternImage (UIImage.FromBundle("bg.jpg"));

			labelSelectedDealer = GetLabel("No Dealer Selected");

			UIScrollView scrollView = new UIScrollView (new CoreGraphics.CGRect(this.View.Bounds.X, 60f, this.View.Bounds.Width, this.View.Bounds.Height));
			scrollView.AddSubview(new EditGroup(GetDealershipGroup()).View);
			scrollView.AddSubview (new EditGroup (GetButtons ()).View);

			nfloat ypos = 0f;
			foreach (UIView view in scrollView  .Subviews) {
				view.Frame = new CoreGraphics.CGRect (view.Bounds.X + 5f, ypos + 10f, this.View.Bounds.Width - 10f, view.Bounds.Height);
				ypos += view.Bounds.Height + 10f;
			}

			//makes height of scroller the total ypos of groups + 60 for the top bar + 10 for margin at bottom
			scrollView.ContentSize = new CoreGraphics.CGSize (scrollView.Bounds.Width, ypos + 60f + 10f);

			this.View.AddSubview (scrollView);

			assetsRequireDownload = getAssetsRequired();

			if (assetsRequireDownload)
				showAssetsDialog ();

			bool dealershipWarningShown = (bool)NSUserDefaults.StandardUserDefaults.BoolForKey("DealershipWarningShown");

			if (!dealershipWarningShown) {
				List<Vehicle> listAllVehicles = new List<Vehicle>();
				using (Connection sqlConn = new Connection(SQLiteBoostDB.GetDBPath()))
					listAllVehicles = new VehicleDB(sqlConn).GetAllVehicles();

				if (listAllVehicles.Count(v => v.DealershipID == null || v.DealershipID == 0) > 0) 
					Controls.OkDialog("Vehicles Without Dealership", "Boost IT has recently changed so that you must select a dealership before creating a vehicle.  You have vehicles that were created without a dealership assigned to them on your device.  You will not be able to upload these vehicles until you go into \"Change a Vehicle\" and select a dealer for each vehicle.\n\n Vehicles not assigned to a dealership will be marked with \"(**)\" on the \"Change a Vehicle\" page.", delegate { NSUserDefaults.StandardUserDefaults.SetBool(true, "DealershipWarningShown"); });
			}
		}

		private List<UIView> GetDealershipGroup()
		{
			SetDealershipLabel();
			List<UIView> listViews = new List<UIView>();
			//scrollView.AddSubview(GetLabel("SELECTED DEALER:"));
			//listViews.Add(GetLabel("SELECTED DEALER:"));
			listViews.Add(new EditButton("Change Dealer", ChangeDealershipClicked).View);
			listViews.Add(labelSelectedDealer);

			return listViews;
		}

		private List<UIView> GetButtons()
		{
			List<UIView> listViews = new List<UIView> ();
			listViews.Add (new EditButton("Create a Vehicle", CreateVehicleClick).View);
			listViews.Add (new EditButton("Change a Vehicle", ChangeVehicleClick).View);
			listViews.Add (new EditButton("Upload", UploadClick).View);
			listViews.Add (new EditButton("Download", DownloadClick).View);
			listViews.Add (new EditButton("Download Assets", downloadAssets).View);
			listViews.Add (new EditButton("Logout", LogoutClick).View);

			return listViews;
		}

		private UILabel GetLabel(string labelText)
		{
			UILabel label = new UILabel(new CoreGraphics.CGRect(0f, 0f, this.View.Bounds.Width, 10f));
			label.Font = UIFont.FromName("Good Times", 14f);
			label.TextColor = UIColor.White;
			label.Text = labelText;

			return label;
		}

		private void SetDealershipLabel()
		{
			int selectedDealershipID = (int)NSUserDefaults.StandardUserDefaults.IntForKey("SelectedDealershipID");
			string selectedDealershipName = NSUserDefaults.StandardUserDefaults.StringForKey("SelectedDealershipName");
			if (selectedDealershipID <= 0 || string.IsNullOrWhiteSpace(selectedDealershipName)) {
				labelSelectedDealer.Text = "No Dealer Selected";
				return;
			}

			labelSelectedDealer.Text = selectedDealershipName;
		}

		void LogoutClick()
		{
			NSUserDefaults.StandardUserDefaults.SetInt (0, "UserID");
			NavigationController.PushViewController (new Login (), true);
		}

		private void ChangeDealershipClicked()
		{
			int UserID = (int)NSUserDefaults.StandardUserDefaults.IntForKey("UserID");
			Console.WriteLine("UserID=" + UserID);
			loadingOverlay = Controls.ProgressDialog(this, "Loading Dealers...");
			Async.BackgroundProcess(bw_GetDealers, bw_GetDealersCompleted, UserID);
		}

		public void dealershipSelected(string name, string value)
		{
			Console.WriteLine("selected dealer=" + value);
			Dealership dealer = listOfDealers.FirstOrDefault(d => d.DealershipName == value);
			if (dealer == null) {
				Controls.OkDialog("Invalid Dealer", "Selected dealer does not exist, please select a different dealer.");
				return;
			}

			NSUserDefaults.StandardUserDefaults.SetInt(dealer.DealershipID, "SelectedDealershipID");
			NSUserDefaults.StandardUserDefaults.SetString(dealer.DealershipName, "SelectedDealershipName");

			SetDealershipLabel();
		}

		private void CreateVehicleClick()
		{
			int selectedDealershipID = (int)NSUserDefaults.StandardUserDefaults.IntForKey("SelectedDealershipID");
			if (assetsRequireDownload)
				showAssetsDialog();
			else if (selectedDealershipID <= 0)
				Controls.OkDialog("No Dealer Selected", "You must select a dealer before you can create a vehicle.", null);
			else {
				int vehicleId = 0;
				using (Connection sqlConn = new Connection(SQLiteBoostDB.GetDBPath()))
					vehicleId = new VehicleDB(sqlConn).AddVehicle(selectedDealershipID);

				this.NavigationController.PushViewController(new CreateVehicle(vehicleId), true);
			}
		}

		void ChangeVehicleClick ()
		{
			int selectedDealershipID = (int)NSUserDefaults.StandardUserDefaults.IntForKey("SelectedDealershipID");

			if (assetsRequireDownload)
				showAssetsDialog ();
			else if (selectedDealershipID <= 0)
				Controls.OkDialog("No Dealer Selected", "You must select a dealer before you can change vehicles.", null);
			else
				NavigationController.PushViewController (new ChangeVehicle (selectedDealershipID), true);
		}

		void UploadClick ()
		{
			int selectedDealershipID = (int)NSUserDefaults.StandardUserDefaults.IntForKey("SelectedDealershipID");

			if (assetsRequireDownload)
				showAssetsDialog ();
			else if (selectedDealershipID <= 0)
				Controls.OkDialog("No Dealer Selected", "You must select a dealer before you can upload vehicles.", null);
			else {
				int uploadID = 0;
				using (Connection sqlConn = new Connection(SQLiteBoostDB.GetDBPath()))
					uploadID = new UploadDB(sqlConn).CreateUpload();

				NavigationController.PushViewController (new UploadList (uploadID, selectedDealershipID), true);
			}
		}

		void DownloadClick()
		{
			int selectedDealershipID = (int)NSUserDefaults.StandardUserDefaults.IntForKey("SelectedDealershipID");

			if (assetsRequireDownload)
				showAssetsDialog ();
			else if (selectedDealershipID <= 0)
				Controls.OkDialog("No Dealer Selected", "You must select a dealer before you can download vehicles.", null);
			else 
				NavigationController.PushViewController(new DownloadVehicles(selectedDealershipID), true);
		}

		private void bw_GetDealers(object sender, System.ComponentModel.DoWorkEventArgs e)
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
		}

		private bool getAssetsRequired()
		{
			using (Connection sqlConn = new Connection(SQLiteBoostDB.GetDBPath()))
				return new VehicleAssetsDB(sqlConn).AssetsRequireDownloading();
		}

		private void showAssetsDialog()
		{
			Controls.YesNoDialog ("Assets Download Required",
				"You must download the vehicle assets to create a vehicle. Would you like to download them now?",
				delegate {
					Controls.WifiDialog ("This is a large download and you are not connected to Wifi. Continue?", downloadAssets);
				},
				delegate {
					return;
				});		
		}

		private void downloadAssets()
		{
			loadingOverlay = Controls.ProgressDialog (this, "Downloading Assets...");

			Async.BackgroundProcess(bw_GetVehicleAssets, bw_GetVehicleAssetsCompleted);
		}        

		private void bw_GetVehicleAssets(object sender, DoWorkEventArgs e)
		{
			using (Connection sqlConn = new Connection(SQLiteBoostDB.GetDBPath()))
				new BoostIT.Tasks.GetVehicleAssets(sqlConn).LoadAssetsIntoDatabase();
		}

		private void bw_GetVehicleAssetsCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			loadingOverlay.Hide ();
			if (e.Error != null) {
				//there was an error, show error message
				Controls.OkDialog("Error Loading Assets", "There was an error loading the assets.  The error message was: " + e.Error.Message);    
				return;
			}

			//make sure the assets have all been downloaded.
			assetsRequireDownload = getAssetsRequired();            
		}


	}
}

