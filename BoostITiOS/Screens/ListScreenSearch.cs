
using System;
using System.Drawing;

using Foundation;
using UIKit;
using System.Linq;

namespace BoostITiOS
{
	public partial class ListScreenSearch : UIViewController
	{
		private string[] list;
		private string editItemName;
		public delegate void ListScreenItemClicked(string itemName, string itemValue);
		private ListScreenItemClicked listItemClicked;

		public ListScreenSearch (ListScreenItemClicked ListItemClicked, string[] ListOfData, string LabelToChange) : base ("ListScreenSearch", null)
		{
			list = ListOfData;
			editItemName = LabelToChange;
			listItemClicked = ListItemClicked;
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

			tableView.Source = new ListScreenSource (list, editItemName, SetItemResult);

			searchBar.TextChanged += delegate {
				string[] newList = list.Where (o => o.ToLower ().Contains (searchBar.Text.ToLower ())).ToArray ();
				tableView.Source = new ListScreenSource (newList, editItemName, SetItemResult);
				tableView.ReloadData();
			};

			btnDone.TouchUpInside += btnDoneTouchUpInside;
		}

		void btnDoneTouchUpInside (object sender, EventArgs e)
		{
			//controller.SetEditItemValue (editItemName, searchBar.Text);
			listItemClicked(editItemName, searchBar.Text);
			DismissModalViewController (true);
		}

		public void SetItemResult(string itemName, string itemValue)
		{
			//controller.SetEditItemValue (itemName, itemValue);
			//DismissModalViewController (true);
			searchBar.Text = itemValue;
			listItemClicked(itemName, itemValue);
			DismissModalViewController (true);
		}

		
	}
}

