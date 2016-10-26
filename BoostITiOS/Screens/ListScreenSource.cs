
using System;
using System.Drawing;

using Foundation;
using UIKit;
using System.Collections.Generic;

namespace BoostITiOS
{
	public class ListScreenSource : UITableViewSource
	{
		private string[] list;
		public delegate void ItemClickedDelegate (string itemName, string itemValue);
		private ItemClickedDelegate itemClicked;
		private string itemName;

		public ListScreenSource (string[] ListOfData, string ItemName, ItemClickedDelegate ItemClicked)
		{
			list = ListOfData;
			itemClicked = ItemClicked;
			itemName = ItemName;
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
			Console.WriteLine ("selected=" + list [indexPath.Row]);
			//controller.SetItemResult (list [indexPath.Row]);
			itemClicked (itemName, list [indexPath.Row]);
			 
		}

		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			var cell = tableView.DequeueReusableCell (ListScreenCell.Key) as ListScreenCell;
			if (cell == null)
				cell = new ListScreenCell ();

			cell.LabelText = list [indexPath.Row];
			//cell.Accessory = UITableViewCellAccessory.Checkmark;

			return cell;
		}
	}
}

