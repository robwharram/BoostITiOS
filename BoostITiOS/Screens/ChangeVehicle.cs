
using System;

using Foundation;
using UIKit;
using BoostIT.Models;
using System.Collections.Generic;
using ObjCRuntime;
using BoostIT;
using System.IO;
using SQLite;
using BoostIT.DataAccess;
using System.Linq;

namespace BoostITiOS
{
	public partial class ChangeVehicle : UIViewController
	{
		private bool initialLoad = true;
		private int selectedDealershipID;

		public ChangeVehicle (int DealershipID) : base ("ChangeVehicle", null)
		{
			selectedDealershipID = DealershipID;
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

			this.View.BackgroundColor = UIColor.FromPatternImage (UIImage.FromBundle("bg.jpg"));

			LoadVehicles ();
			
			btnDone.Clicked += (object sender, EventArgs e) => { NavigationController.PopViewController(true); };	
			//tvChangeVehicles.SectionHeaderHeight = 10f;
			//tvChangeVehicles.BackgroundColor = UIColor.FromPatternImage (UIImage.FromBundle("bg.jpg"));

		}

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);

			if (!initialLoad) 
				LoadVehicles ();
			else
				initialLoad = false;
		}

		private void LoadVehicles()
		{
			List<VehicleWithImages> listOfVehicles = new List<VehicleWithImages> ();
			using (Connection sqlConn = new Connection(SQLiteBoostDB.GetDBPath()))
				listOfVehicles = new VehicleDB(sqlConn).GetVehicleList(selectedDealershipID, true);

			tvChangeVehicles.Delegate = new TableViewDelegate (this, listOfVehicles);
			tvChangeVehicles.DataSource = new TableViewDataSource (this, listOfVehicles);
			tvChangeVehicles.ReloadData ();
		}

		public void DeleteVehicle(int vehicleId)
		{
			using (Connection sqlConn = new Connection (SQLiteBoostDB.GetDBPath ()))
				new VehicleDB(sqlConn).DeleteVehicle(vehicleId);
		}

		public void GoToCreateVehicle(int VehicleID)
		{
			NavigationController.PushViewController(new CreateVehicle(VehicleID, true),true);
		}

		private class TableViewDelegate : UITableViewDelegate
		{
			private List<VehicleWithImages> list;
			private ChangeVehicle controller;

			public TableViewDelegate(ChangeVehicle Controller, List<VehicleWithImages> list)
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
				controller.GoToCreateVehicle (list [indexPath.Row].vehicle.ID);
			}
				
		}

		private class TableViewDataSource : UITableViewDataSource
		{
			static NSString kCellIdentifier = new NSString ("MyIdentifier");
			public List<VehicleWithImages> list;
			private ChangeVehicle controller;

			public TableViewDataSource (ChangeVehicle changeVehicle, List<VehicleWithImages> list)
			{
				this.list = list;
				this.controller = changeVehicle;
			}

			public override nint RowsInSection (UITableView tableview, nint section)
			{
				return list.Count();
			}

			public override void CommitEditingStyle (UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
			{
				switch (editingStyle) {
				case UITableViewCellEditingStyle.Delete:
					controller.DeleteVehicle (list [indexPath.Row].vehicle.ID);
					list.RemoveAt (indexPath.Row);
					tableView.DeleteRows (new NSIndexPath[] { indexPath }, UITableViewRowAnimation.Fade);
					break;
				}
			}

			public override bool CanEditRow (UITableView tableView, NSIndexPath indexPath)
			{
				return true;
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
							if (File.Exists (thumbPath))
								break;
						}
						thumbPath = ""; //Thumb file does not exist - set to empty variable.
					}
				}

				string MissingDealershipID = "";
				if (vehicle.DealershipID == null || vehicle.DealershipID == 0)
					MissingDealershipID = "(**)";
				string YearMakeModel = vehicle.Year + " " + vehicle.Make + " " + vehicle.Model + " " + MissingDealershipID;
				string Price = (vehicle.Price.HasValue) ? vehicle.Price.Value.ToString ("C0").ToNullableString (string.Empty) : string.Empty;
				cell.UpdateCell (YearMakeModel, vehicle.StockNumber, vehicle.VIN, Price);
				if (string.IsNullOrWhiteSpace (thumbPath))
					cell.ImageView.Image = UIImage.FromBundle ("nophoto.png");
				else if (thumbPath.StartsWith ("https://"))
					cell.ImageView.Image = Graphics.FromUrl (thumbPath);
				else
					cell.ImageView.Image = UIImage.FromFile(thumbPath);

				return cell;
			}
		}
	}
}

