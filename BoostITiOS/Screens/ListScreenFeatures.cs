
using System;

using Foundation;
using UIKit;
using System.Collections.Generic;
using SQLite;
using BoostIT.DataAccess;
using BoostIT.Models;
using System.Linq;

namespace BoostITiOS
{
	public partial class ListScreenFeatures : UIViewController
	{
		private string[] list;
		public List<string> selectedFeatures = new List<string>();
		private int vehicleid=0, categoryid=0;
		private List<AvailableFeature> listFeatures;

		public ListScreenFeatures (string[] ListOfData, int VehicleID, int CategoryID, List<AvailableFeature> listOfFeatures) : base ("ListScreenFeatures", null)
		{
			list = ListOfData;
			vehicleid = VehicleID;
			categoryid = CategoryID;
			listFeatures = listOfFeatures;
			List<Feature> listSelectedFeatures = new List<Feature>();
			using (Connection sqlConn = new Connection(SQLiteBoostDB.GetDBPath()))
				listSelectedFeatures= new VehicleFeaturesDB(sqlConn).GetSelectedFeatures(vehicleid, categoryid);

			foreach (Feature feature in listSelectedFeatures)
				selectedFeatures.Add (feature.FeatureName);
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
			
			tvFeatures.Source = new ListScreenFeaturesSource (this, list);
			btnDone.TouchUpInside += btnDoneTouchUpInside;
		}

		void btnDoneTouchUpInside (object sender, EventArgs e)
		{
			using (Connection sqlConn = new Connection(SQLiteBoostDB.GetDBPath())) {
				VehicleFeaturesDB vfdb = new VehicleFeaturesDB(sqlConn);                
				vfdb.RemoveAllFeaturesByCategory(vehicleid, categoryid);
				if (selectedFeatures != null && selectedFeatures.Count > 0) {
					foreach (string selectedFeature in selectedFeatures) {
						AvailableFeature af = listFeatures.FirstOrDefault(f => f.FeatureCategoryID == categoryid && f.Feature == selectedFeature);
						if (af != null)
							vfdb.AddFeature(new Feature() { FeatureID = af.ID, FeatureCategoryID = af.FeatureCategoryID, FeatureName = af.Feature, VehicleID = vehicleid });
					}
				}                
			}      
			DismissModalViewController (true);
		}

		public class ListScreenFeaturesSource : UITableViewSource
		{
			private string[] list;
			private ListScreenFeatures lsf;

			public ListScreenFeaturesSource (ListScreenFeatures listScreenFeatures, string[] ListOfData)
			{
				lsf = listScreenFeatures;
				list = ListOfData;
			}

			public override nint NumberOfSections (UITableView tableView)
			{
				return 1;
			}

			public override nint RowsInSection (UITableView tableview, nint section)
			{
				return list.Length;
			}

			public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
			{
				UITableViewCell cell = tableView.CellAt (indexPath);
				string feature = list [indexPath.Row];
				if (lsf.selectedFeatures.Contains (feature)) {
					lsf.selectedFeatures.Remove (feature);
					cell.Accessory = UITableViewCellAccessory.None;
					return;
				}

				lsf.selectedFeatures.Add (feature);
				cell.Accessory = UITableViewCellAccessory.Checkmark;
			}

			public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
			{
				var cell = tableView.DequeueReusableCell (ListScreenFeaturesCell.Key) as ListScreenFeaturesCell;
				if (cell == null)
					cell = new ListScreenFeaturesCell ();

				string feature = list [indexPath.Row];
				cell.LabelText = feature;

				cell.Accessory = UITableViewCellAccessory.None;
				if (lsf.selectedFeatures.Contains (feature))
					cell.Accessory = UITableViewCellAccessory.Checkmark;

				return cell;
			}

			public class ListScreenFeaturesCell : UITableViewCell
			{
				public static readonly NSString Key = new NSString ("ListScreenFeaturesCell");

				public string LabelText { 
					get {
						return TextLabel.Text;
					}
					set {
						TextLabel.Text = value;
					}
				}

				public ListScreenFeaturesCell () : base (UITableViewCellStyle.Value1, Key)
				{

				}
			}
		}
	}
}

