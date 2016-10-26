
using System;

using Foundation;
using UIKit;

namespace BoostITiOS
{
	public partial class ChangeVehicleCell : UITableViewCell
	{
		public static readonly UINib Nib = UINib.FromName ("ChangeVehicleCell", NSBundle.MainBundle);
		public static readonly NSString Key = new NSString ("ChangeVehicleCell");

		public ChangeVehicleCell (IntPtr handle) : base (handle)
		{
		}

		public static ChangeVehicleCell Create ()
		{
			return (ChangeVehicleCell)Nib.Instantiate (null, null) [0];
		}

		public void UpdateCell(string YearMakeModel, string Stock, string VIN, string Price)
		{
			lblYearMakeModel.Text = YearMakeModel;
			lblStock.Text = Stock;
			lblVIN.Text = VIN;
			lblPrice.Text = Price;
		}
	}
}

