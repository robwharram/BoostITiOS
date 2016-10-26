
using System;

using Foundation;
using UIKit;
using System.IO;

namespace BoostITiOS
{
	public partial class DamageListCell : UITableViewCell
	{
		public static readonly UINib Nib = UINib.FromName ("DamageListCell", NSBundle.MainBundle);
		public static readonly NSString Key = new NSString ("DamageListCell");



		public UITableViewCell cell {
			get { return this; }
		}



		public DamageListCell (IntPtr handle) : base (handle)
		{
		}

		public static DamageListCell Create ()
		{
			return (DamageListCell)Nib.Instantiate (null, null) [0];
		}

		public void UpdateCell(string area, string type, string length)
		{
			lblArea.Text = area;
			lblType.Text = type;
			lblLength.Text = length;
		}
	}
}

