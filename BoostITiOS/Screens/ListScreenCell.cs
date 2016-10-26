
using System;
using System.Drawing;

using Foundation;
using UIKit;

namespace BoostITiOS
{
	public class ListScreenCell : UITableViewCell
	{
		public static readonly NSString Key = new NSString ("ListScreenCell");

		public string LabelText { 
			get {
				return TextLabel.Text;
			}
			set {
				TextLabel.Text = value;
			}
		}

		public ListScreenCell () : base (UITableViewCellStyle.Value1, Key)
		{
			this.BackgroundColor = UIColor.Black;
			TextLabel.TextColor = UIColor.White;

			UIView selectedColor = new UIView ();
			selectedColor.BackgroundColor = UIColor.Red;
			selectedColor.Layer.MasksToBounds = true;
			this.SelectedBackgroundView = selectedColor;
		}
	}
}

