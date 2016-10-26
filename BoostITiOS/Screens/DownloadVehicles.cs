
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
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.ObjectModel;

namespace BoostITiOS
{
	public partial class DownloadVehicles : UIViewController
	{
		private List<int> selectedVehicleIds;
		private List<VehicleForDownloadWithImage> listOfVehicles;
		private LoadingOverlay loadingOverlay;
		private int dealershipId=0;
		public bool onlyLGM = false, onlyBlue = false, menuShowing = false;
		public UITableView tvMenu;

		public DownloadVehicles (int DealershipID) : base ("DownloadVehicles", null)
		{			
			dealershipId = DealershipID;
		}

		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();

			foreach (VehicleForDownloadWithImage v in listOfVehicles)
				v.image = null;
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			Controls.RestrictRotation (true);

			selectedVehicleIds = new List<int> ();

			this.View.BackgroundColor = UIColor.FromPatternImage (UIImage.FromBundle("bg.jpg"));

			LoadVehicles ();

			btnDownload.Clicked += DownloadClicked;
			btnMenu.Clicked += MenuClicked;
		}

		void MenuClicked (object sender, EventArgs e)
		{
			if (menuShowing) {
				tvMenu.RemoveFromSuperview ();
				menuShowing = false;
				return;
			}

			tvMenu = new UITableView (new CoreGraphics.CGRect (0f, 60f, 240f, 130f));
			tvMenu.DataSource = new menuDataSource (this);
			tvMenu.Delegate = new menuDelegate (this);
				
			this.View.AddSubview (tvMenu);

			menuShowing = true;
		}

		private class menuDelegate : UITableViewDelegate
		{
			private DownloadVehicles controller;

			public menuDelegate(DownloadVehicles Controller)
			{
				this.controller = Controller;
			}

			public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
			{				
				if (indexPath.Row == 0) {
					if (controller.onlyBlue == true)
						controller.onlyBlue = false;
					else
						controller.onlyBlue = true;
					controller.tvMenu.RemoveFromSuperview ();
					controller.LoadVehicles ();
				} else if (indexPath.Row == 1) {
					if (controller.onlyLGM == true)
						controller.onlyLGM = false;
					else
						controller.onlyLGM = true;
					controller.tvMenu.RemoveFromSuperview ();
					controller.LoadVehicles ();
				} else if (indexPath.Row == 2) {
					controller.NavigationController.PopViewController (true);
				}

				tableView.ReloadData ();					
			}
		}

		private class menuDataSource : UITableViewDataSource
		{
			static NSString kCellIdentifier = new NSString ("MyIdentifier");
			private DownloadVehicles controller;

			public menuDataSource (DownloadVehicles Controller)
			{
				controller = Controller;
			}

			public override nint RowsInSection (UITableView tableview, nint section)
			{
				return 3;
			}

			public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
			{
				UITableViewCell cell = tableView.DequeueReusableCell(kCellIdentifier);
				if (cell == null) {
					cell = new UITableViewCell (UITableViewCellStyle.Default, kCellIdentifier);
				}

				cell.Tag = indexPath.Row;
				if (indexPath.Row == 0) {
					cell.TextLabel.Text = "only show Blue Status";
					if (controller.onlyBlue)
						cell.Accessory = UITableViewCellAccessory.Checkmark;
					else
						cell.Accessory = UITableViewCellAccessory.None;
				}
				else if (indexPath.Row == 1) {
					cell.TextLabel.Text = "only LGM";
					if (controller.onlyLGM)
						cell.Accessory = UITableViewCellAccessory.Checkmark;
					else
						cell.Accessory = UITableViewCellAccessory.None;
				}
				else if (indexPath.Row == 2)
					cell.TextLabel.Text = "Home";
				
				return cell;
			}
		}

		private void LoadVehicles()
		{
			loadingOverlay = Controls.ProgressDialog(this, "Loading Vehicles");
			Async.BackgroundProcess(bw_LoadVehicles, bw_LoadVehiclesCompleted);
		}

		private void bw_LoadVehicles(object sender, System.ComponentModel.DoWorkEventArgs e)
		{
			e.Result = Download.GetVehicleList(dealershipId, onlyLGM, onlyBlue);
		}

		private void bw_LoadVehiclesCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
		{
			loadingOverlay.Hide ();

			if (e.Error != null) {
				Controls.OkDialog("Error", "There was an error downloading the vehicles. Error: " + e.Error.Message, delegate { return; });
				return;
			}

			listOfVehicles = AddImages((List<VehicleForDownload>)e.Result);
			if (listOfVehicles.Count == 1)
				nbDownload.TopItem.Title = "1 Vehicle";
			else
				nbDownload.TopItem.Title = listOfVehicles.Count + " Vehicles";
			
			tvDownload.Delegate = new TableViewDelegate (this, listOfVehicles);
			tvDownload.DataSource = new TableViewDataSource (this, listOfVehicles);
			tvDownload.ReloadData ();
		}

		private List<VehicleForDownloadWithImage> AddImages(List<VehicleForDownload> list)
		{
			List<VehicleForDownloadWithImage> returnList = new List<VehicleForDownloadWithImage>();
			foreach (VehicleForDownload vehicle in list)
				returnList.Add(new VehicleForDownloadWithImage() { vehicle = vehicle });

			return returnList;
		}

		private void DownloadClicked(object sender, EventArgs e)
		{
			if (selectedVehicleIds.Count () <= 0) {
				Controls.OkDialog ("No Vehicles Selected", "You must select at least 1 vehicle to download.");
				return;
			}

			foreach (int id in selectedVehicleIds)
				Console.WriteLine("selectedId=" + id);

			loadingOverlay = Controls.ProgressDialog (this, "Downloading Vehicles");
			Async.BackgroundProcess (bw_DownloadVehicles, bw_DownloadVehiclesCompleted);
		}

		private void bw_DownloadVehicles(object sender, System.ComponentModel.DoWorkEventArgs e)
		{
			if (selectedVehicleIds.Count <= 0)
				throw new Exception("No vehicles selected");

			using (Connection sqlConn = new Connection(SQLiteBoostDB.GetDBPath())) {               
				foreach (int vehicleId in selectedVehicleIds) 
					Download.DownloadVehicle(sqlConn, vehicleId, dealershipId);               
			}            
		}

		private void bw_DownloadVehiclesCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
		{
			loadingOverlay.Hide ();

			Controls.OkDialog("Download Complete", "Download Complete", delegate { 
				NavigationController.PopViewController(true);
			});
		}

		private class TableViewDelegate : UITableViewDelegate
		{
			private List<VehicleForDownloadWithImage> list;
			private DownloadVehicles controller;

			public TableViewDelegate(DownloadVehicles Controller, List<VehicleForDownloadWithImage> list)
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
				int vehicleId = list [indexPath.Row].vehicle.BoostVehicleID;

				if (controller.selectedVehicleIds.Contains (vehicleId))
					controller.selectedVehicleIds.Remove (vehicleId);
				else
					controller.selectedVehicleIds.Add (vehicleId);

				foreach (int vehicleid in controller.selectedVehicleIds)
					Console.WriteLine ("vehicleID=" + vehicleid);

				tableView.ReloadData ();					
			}
		}

		private class TableViewDataSource : UITableViewDataSource
		{
			static NSString kCellIdentifier = new NSString ("MyIdentifier");
			public List<VehicleForDownloadWithImage> list;
			public DownloadVehicles controller;
			private UIImage placeHolder;

			public TableViewDataSource (DownloadVehicles parentController, List<VehicleForDownloadWithImage> list)
			{
				this.list = list;
				controller = parentController;
				placeHolder = UIImage.FromBundle("nophoto.png");
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

				VehicleForDownloadWithImage dv = list [indexPath.Row];

				string imagePath = dv.vehicle.thumbURL;
				string YearMakeModel = dv.vehicle.Year + " " + dv.vehicle.Make + " " + dv.vehicle.Model;
				string Price = dv.vehicle.Price.ToString ("C0").ToNullableString (string.Empty);

				cell.UpdateCell (YearMakeModel, dv.vehicle.StockNumber, dv.vehicle.VIN, Price);
				if (dv.image == null) {
					dv.image = placeHolder;
					if (!string.IsNullOrWhiteSpace (imagePath))
						BeginDownloadingImage (dv, indexPath);
				}
				cell.ImageView.Image = dv.image;

				if (controller.selectedVehicleIds.Contains (dv.vehicle.BoostVehicleID))
					cell.Accessory = UITableViewCellAccessory.Checkmark;
				else
					cell.Accessory = UITableViewCellAccessory.None;

				return cell;
			}

			async void BeginDownloadingImage (VehicleForDownloadWithImage dv, NSIndexPath path)
			{
				// Queue the image to be downloaded. This task will execute
				// as soon as the existing ones have finished.
				byte[] data = null;

				data = await GetImageData (dv);
				if (data != null)
					dv.image = UIImage.LoadFromData (NSData.FromArray (data));
				else
					dv.image = placeHolder;

				InvokeOnMainThread (() => {
					var cell = controller.tvDownload.VisibleCells.Where (c => c.Tag == controller.listOfVehicles.IndexOf (dv)).FirstOrDefault ();
					if (cell != null)
						cell.ImageView.Image = dv.image;
				});
			}

			async Task<byte[]> GetImageData(VehicleForDownloadWithImage dv)
			{
				byte[] data = null;
				try {
					UIApplication.SharedApplication.NetworkActivityIndicatorVisible = true;
					using (var c = new GzipWebClient ())
						data = await c.DownloadDataTaskAsync (dv.vehicle.thumbURL);					
				} 
				catch
				{
					return null;
				}
				finally {
					UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false;
				}

				return data;
			}

			private void bw_LoadThumb(object sender, System.ComponentModel.DoWorkEventArgs e)
			{
				ThumbHolder holder = (ThumbHolder)e.Argument;

				using (var httpClient = new HttpClient ()) {
					byte[] contents = httpClient.GetByteArrayAsync (holder.filePath).Result;
					holder.thumbNail = UIImage.LoadFromData (NSData.FromArray (contents));  
				}

				e.Result = holder;
			}

			private void bw_LoadThumbCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
			{
				ThumbHolder holder = (ThumbHolder)e.Result;

				Graphics.HideActivitySpinner (holder.LoadingView);
				if (holder.thumbNail == null) {
					holder.imageView.Image = UIImage.FromBundle ("nophoto.png");
					return;
				}
				holder.imageView.Image = holder.thumbNail;
				holder = null;
			}

		}
	}
}

