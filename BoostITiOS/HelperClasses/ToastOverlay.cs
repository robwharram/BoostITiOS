using System;
using UIKit;
using System.Drawing;
using CoreGraphics;
using Foundation;

namespace BoostITiOS
{
	public class ToastOverlay : UIView {
		// control declarations
		UILabel loadingLabel;

		public ToastOverlay (CGRect frame, string LabelText) : base (frame)
		{	
			// configurable bits
			BackgroundColor = UIColor.Black;
			Alpha = 0.75f;
			AutoresizingMask = UIViewAutoresizing.FlexibleDimensions;

			nfloat labelHeight = 22;
			nfloat labelWidth = Frame.Width;

			// derive the center x and y
			//nfloat centerX = Frame.Width / 2;
			//nfloat centerY = Frame.Height / 2;

			// create and configure the "Loading Data" label
			loadingLabel = new UILabel(new CGRect (
				0f,0f,
				labelWidth ,
				labelHeight
			));
			loadingLabel.BackgroundColor = UIColor.Clear;
			loadingLabel.TextColor = UIColor.White;
			loadingLabel.Font = UIFont.FromName ("Arial", 14f);
			loadingLabel.Text = LabelText;
			loadingLabel.TextAlignment = UITextAlignment.Center;
			loadingLabel.AutoresizingMask = UIViewAutoresizing.FlexibleMargins;
			AddSubview (loadingLabel);

			NSTimer fadeout = NSTimer.CreateTimer(2, timer => Hide());

			NSRunLoop.Main.AddTimer(fadeout, NSRunLoopMode.Common);
		}

		/// <summary>
		/// Fades out the control and then removes it from the super view
		/// </summary>
		public void Hide ()
		{
			UIView.Animate (
				1.0, // duration
				() => { Alpha = 0; },
				() => { RemoveFromSuperview(); }
			);
		}

		public void Remove ()
		{
			RemoveFromSuperview ();
		}
	};
}

