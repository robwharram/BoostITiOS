
using System;

using Foundation;
using UIKit;
using BoostIT.Models;
using System.Collections.Generic;
using SQLite;
using BoostIT.DataAccess;
using System.Threading.Tasks;
using System.IO;
using ObjCRuntime;


namespace BoostITiOS
{
	public partial class DamageList : UIViewController
	{
		private List<Damage> listOfDamages;
		private int vehicleId, categoryId;
		private bool initialLoad = true;

		public DamageList (int VehicleID, int CategoryID) : base ("DamageList", null)
		{
			vehicleId = VehicleID;
			categoryId = CategoryID;
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

			//if there are no damages to list, go straight to add damage activity
			LoadDamages();
			if (listOfDamages.Count <= 0)
				AddEditDamage (0);

			btnDone.Clicked += BtnDone_Clicked;
			btnAdd.Clicked += (object sender, EventArgs e) => {
				AddEditDamage(0);
			};

			tvDamages.Delegate = new TableViewDelegate(this,listOfDamages);
			tvDamages.DataSource = new TableViewDataSource(this, listOfDamages);
		}

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);

			if (!initialLoad) {
				//reload table
				LoadDamages ();
				tvDamages.Delegate = new TableViewDelegate(this,listOfDamages);
				tvDamages.DataSource = new TableViewDataSource(this, listOfDamages);
				tvDamages.ReloadData ();
			} else
				initialLoad = false;
		}

		public  void AddEditDamage(int damageId)
		{
			PresentModalViewController (new DamageAdd (vehicleId, categoryId, damageId), true);
		}

		public void DeleteDamage(int damageId)
		{
			using (Connection sqlConn = new Connection (SQLiteBoostDB.GetDBPath ()))
				new DamageDB (sqlConn).DeleteDamage (damageId);
		}

		private void LoadDamages()
		{
			using (Connection sqlConn = new Connection (SQLiteBoostDB.GetDBPath ())) 
				listOfDamages = new DamageDB (sqlConn).GetDamageList (vehicleId, categoryId);
		}

		void BtnDone_Clicked (object sender, EventArgs e)
		{
			NavigationController.PopViewController(true);
		}

		private class TableViewDelegate : UITableViewDelegate
		{
			private List<Damage> list;
			private DamageList controller;

			public TableViewDelegate(DamageList Controller, List<Damage> list)
			{
				this.controller = Controller;
				this.list = list;
			}

			public override nfloat GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
			{
				// TODO: Implement - see: http://go-mono.com/docs/index.aspx?link=T%3aMonoTouch.Foundation.ModelAttribute
				return 74f;
			}

			public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
			{				
				controller.AddEditDamage (list [indexPath.Row].ID);
			}
		}

		private class TableViewDataSource : UITableViewDataSource
		{
			static NSString kCellIdentifier = new NSString ("MyIdentifier");
			public List<Damage> list;
			private DamageList controller;

			public TableViewDataSource (DamageList damageList, List<Damage> list)
			{
				this.list = list;
				this.controller = damageList;
			}

			public override nint RowsInSection (UITableView tableview, nint section)
			{
				return list.Count;
			}

			public override void CommitEditingStyle (UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
			{
				switch (editingStyle) {
				case UITableViewCellEditingStyle.Delete:
					controller.DeleteDamage (list [indexPath.Row].ID);
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
				DamageListCell cell = tableView.DequeueReusableCell(kCellIdentifier) as DamageListCell;

				if (cell == null) {
					var views = NSBundle.MainBundle.LoadNib ("DamageListCell", tableView, null);
					cell = Runtime.GetNSObject (views.ValueAt (0)) as DamageListCell;
				}

				cell.Tag = indexPath.Row;

				Damage damage = list [indexPath.Row];

				string thumbPath = "";
				if (!string.IsNullOrWhiteSpace (damage.FileName)) {
					string pthumbPath = Path.Combine (Path.GetDirectoryName(damage.FileName), Path.GetFileNameWithoutExtension (damage.FileName) + "_thumb.jpg");
					if (File.Exists (pthumbPath))
						thumbPath = pthumbPath;
				}

				cell.UpdateCell (damage.Area, damage.TypeName, (damage.Length > 0) ? damage.Length.ToString () + " inches" : "N/A");
				if (string.IsNullOrWhiteSpace(thumbPath))
					cell.ImageView.Image = UIImage.FromBundle ("nophoto.png");
				else
					cell.ImageView.Image = UIImage.FromFile(thumbPath);

				return cell;
			}
		}
	}
}

