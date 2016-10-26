
using System;
using System.Drawing;

using Foundation;
using UIKit;
using System.Collections.Generic;

namespace BoostITiOS
{
	public class ListScreenController : UITableViewController
	{
		private string[] list;
		private string editItemName;
		//private CreateVehicle controller;
		public delegate void ListScreenItemClicked(string itemName, string itemValue);
		private ListScreenItemClicked listItemClicked;

		public ListScreenController (ListScreenItemClicked ListItemClicked, string[] ListOfData, string LabelToChange) : base (UITableViewStyle.Grouped)
		{
			list = ListOfData;
			editItemName = LabelToChange;
			//controller = createVehicleController;
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

			// Register the TableView's data source
			TableView.Source = new ListScreenSource (list, editItemName, SetItemResult);
		}

		public void SetItemResult(string itemName, string itemValue)
		{
			//controller.SetEditItemValue (itemName, itemValue);
			listItemClicked(itemName, itemValue);
			DismissModalViewController (true);
		}
	}
}

