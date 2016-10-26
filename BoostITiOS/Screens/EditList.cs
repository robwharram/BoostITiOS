
using System;
using System.Drawing;

using Foundation;
using UIKit;

namespace BoostITiOS
{
	public partial class EditList : EditItem
	{
		private string labelText = "";
		private string valueText = "";
		private string[] listData;
		private string controlName = "";
		public delegate void OpenListDelegate(string itemName, string[] data, bool isSearchable);
		private OpenListDelegate openListClicked;
		private bool isSearchable;

		public EditList (OpenListDelegate OpenListClicked, string ControlName, string LabelText, string[] ListData, bool IsSearchable = false, string ValueText = "required") : base ("EditList")
		{
			labelText = LabelText;
			valueText = ValueText;
			controlName = ControlName;
			openListClicked = OpenListClicked;
			listData = ListData;
			isSearchable = IsSearchable;
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
			
			lblTitle.Text = labelText;
			lblValue.Text = valueText;

			this.View.Layer.CornerRadius = 7.5f;
			this.View.Layer.BorderColor = UIColor.White.CGColor;
			this.View.Layer.BorderWidth = 0.5f;

			btnDisclosure.TouchDown += (object sender, EventArgs e) => {
				itemClicked();
			};
		}

		public override void TouchesEnded (NSSet touches, UIEvent evt)
		{
			base.TouchesEnded (touches, evt);
			UITouch touch = touches.AnyObject as UITouch;
			if (touch != null) {
				itemClicked ();
			}
		}

		public override void TouchesBegan (NSSet touches, UIEvent evt)
		{
			base.TouchesBegan (touches, evt);
		}

		public override void TouchesMoved (NSSet touches, UIEvent evt)
		{
			base.TouchesMoved (touches, evt);
		}

		public override void TouchesCancelled (NSSet touches, UIEvent evt)
		{
			base.TouchesCancelled (touches, evt);
		}

		private void itemClicked()
		{
			//controller.OpenListScreen (labelText, listData, isSearchable);
			openListClicked(controlName, listData, isSearchable);
		}

		public override string GetValue()
		{
			return lblValue.Text;
		}

		public override void SetValue(string value)
		{
			lblValue.Text = value;
		}
	}
}

