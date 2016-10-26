
using System;
using System.Drawing;

using Foundation;
using UIKit;
using System.Collections.Generic;

namespace BoostITiOS
{
	public partial class EditGroup : UIViewController
	{
		private List<UIView> views;

		public EditGroup (List<UIView> viewsForGroup) : base ("EditGroup", null)
		{
			views = viewsForGroup;
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

			//this.View.BackgroundColor = UIColor.Black;
			this.View.BackgroundColor = UIColor.FromPatternImage (UIImage.FromBundle ("editsectionbackground.png"));
			//this.View.Frame = new CoreGraphics.CGRect(this.View.Bounds.X, this.View.Bounds.Y, this.View.Bounds.Width, ((30f * views.Count) + 5f));
			this.View.Frame = new CoreGraphics.CGRect (this.View.Bounds.X + 5f, this.View.Bounds.Y, this.View.Bounds.Width, ((30f * views.Count) + 5f));
			//this.View.Tag = 69;
			this.View.Layer.CornerRadius = 7.5f;

			float pos = 5f;

			foreach(UIView view in views) {
				this.View.AddSubview (view);
				view.Frame = new CoreGraphics.CGRect (view.Bounds.X + 5f, pos, this.View.Bounds.Width - 10f, view.Bounds.Height);
				pos += 30f;
			}

			//this.View.Frame = new CoreGraphics.CGRect(this.View.Bounds.X, this.View.Bounds.Y, this.View.Bounds.Width, pos);
		}
	}
}

