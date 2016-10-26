
using System;

using Foundation;
using UIKit;
using SQLite;
using BoostIT.DataAccess;
using System.Collections.Generic;
using BoostIT.Models;
using ObjCRuntime;
using System.Linq;

namespace BoostITiOS
{
	public partial class UploadListDealers : UIViewController
	{
		private int UploadID;

		public UploadListDealers (int UploadID) : base ("UploadListDealers", null)
		{
			this.UploadID = UploadID;
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
			
			LoadDealers ();

			btnActions.Clicked += btnActions_Clicked;
			btnCancel.Clicked += btnCancel_Clicked;
		}

		void btnCancel_Clicked (object sender, EventArgs e)
		{
			Controls.YesNoDialog ("Confirm Cancel", "Are you sure that you want to cancel?", delegate {
				NavigationController.PushViewController(new Home(), true);
			}, delegate {
				return;
			});

		}

		void btnActions_Clicked (object sender, EventArgs e)
		{
			var actionSheet = new UIActionSheet("Select an Action", null, "Cancel", "Upload More Vehicles", "Start Upload") {
				Style = UIActionSheetStyle.Default
			};
			actionSheet.Clicked += delegate (object sheetsender, UIButtonEventArgs args) {
				if (args.ButtonIndex == 0) 
					NavigationController.PopViewController(true);
				else if (args.ButtonIndex == 1)
					NavigationController.PushViewController(new UploadProgress(UploadID), true);
			};
			actionSheet.ShowInView (View);
		}

		private void LoadDealers()
		{
			List<UploadDealerVehiclesList> listOfDealers = new List<UploadDealerVehiclesList> ();
			using (Connection sqlConn = new Connection(SQLiteBoostDB.GetDBPath()))
				listOfDealers = new UploadDB(sqlConn).GetDealersToUpload(UploadID);

			tvDealers.Delegate = new TableViewDelegate (this, listOfDealers);
			tvDealers.DataSource = new TableViewDataSource (this, listOfDealers);
			tvDealers.ReloadData ();
		}

		private class TableViewDelegate : UITableViewDelegate
		{
			private List<UploadDealerVehiclesList> list;
			private UploadListDealers controller;

			public TableViewDelegate(UploadListDealers Controller, List<UploadDealerVehiclesList> list)
			{
				this.controller = Controller;
				this.list = list;
			}

			public override nfloat GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
			{
				return 50f;
			}

			public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
			{				
				UploadDealerVehiclesList dealer = list[indexPath.Row];
				Controls.YesNoDialog ("Confirm Delete", "Would you like to remove all of the vehicles for " + dealer.DealerName + "?", delegate {
					using (Connection sqlConn = new Connection (SQLiteBoostDB.GetDBPath ()))
						new UploadDB (sqlConn).RemoveDealerFromUpload (dealer.UploadID, dealer.DealershipID);
					controller.LoadDealers ();
				}, delegate {
					return;
				});					
			}
		}

		private class TableViewDataSource : UITableViewDataSource
		{
			static NSString kCellIdentifier = new NSString ("MyIdentifier");
			public List<UploadDealerVehiclesList> list;
			public UploadListDealers controller;

			public TableViewDataSource (UploadListDealers parentController, List<UploadDealerVehiclesList> list)
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
				UITableViewCell cell = tableView.DequeueReusableCell(kCellIdentifier);

				if (cell == null) {
					cell = new UITableViewCell (UITableViewCellStyle.Subtitle, kCellIdentifier);
					//cell.Layer.CornerRadius = 7.5f;
					cell.BackgroundColor = UIColor.FromRGB ((176f / 255f), (10f / 255f), (10f / 255f));
					cell.Layer.BorderColor = UIColor.White.CGColor;
					cell.Layer.BorderWidth = 0.5f;
					cell.TextLabel.Font = UIFont.FromName ("Arial", 14f);
					cell.TextLabel.TextColor = UIColor.White;
					cell.DetailTextLabel.Font = UIFont.FromName ("Arial", 12f);
					cell.DetailTextLabel.TextColor = UIColor.White;
				}

				cell.Tag = indexPath.Row;

				UploadDealerVehiclesList dealer = list [indexPath.Row];
				int vehicleCount = dealer.VehicleIDs.Count ();
				cell.TextLabel.Text = dealer.DealerName;
				cell.DetailTextLabel.Text = (vehicleCount == 1) ? vehicleCount + " vehicle" : vehicleCount + " vehicles";

				return cell;
			}
		}
	}
}

