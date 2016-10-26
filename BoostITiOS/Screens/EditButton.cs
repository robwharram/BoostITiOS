
using System;
using System.Drawing;

using Foundation;
using UIKit;

namespace BoostITiOS
{
	public partial class EditButton : UIViewController
	{
		private string label;
		private Action action;

		public EditButton (string labelText, Action actionToCall) : base ("EditButton", null)
		{
			label = labelText;
			action = actionToCall;
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
			
			lblTitle.Text = label;
			this.View.Layer.CornerRadius = 7.5f;
			this.View.Layer.BorderColor = UIColor.White.CGColor;
			this.View.Layer.BorderWidth = 0.5f;

			btnDisclosure.TouchDown += (object sender, EventArgs e) => {
				action.Invoke();
			};
		}

		public override void TouchesEnded (NSSet touches, UIEvent evt)
		{
			base.TouchesEnded (touches, evt);
			UITouch touch = touches.AnyObject as UITouch;
			if (touch != null) {
				action.Invoke ();
			}
		}
	}
}

