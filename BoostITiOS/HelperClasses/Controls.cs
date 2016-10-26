using System;
using System.Net;
using SystemConfiguration;
using CoreFoundation;
using UIKit;

namespace BoostITiOS
{
	public static class Controls
	{
		public static void WifiDialog(string message, Action callback)
		{
			NetworkStatus remoteHostStatus = Reachability.RemoteHostStatus();
			if (remoteHostStatus == NetworkStatus.ReachableViaWiFiNetwork) {
				callback ();
			} else if (remoteHostStatus == NetworkStatus.ReachableViaCarrierDataNetwork) {
				UIAlertView alert = new UIAlertView ("No WiFi Connection Found", message, null, "No", "Yes");
				alert.Clicked += (object sender, UIButtonEventArgs e) => {
					if (e.ButtonIndex == 0)
						return;
					else
						callback ();
				};
				alert.Show ();
			} else {
				new UIAlertView ("No Data Connection", "We were unable to detect a network connection and are unable to log you in.", null, "Ok").Show ();
			}				
		}

		public static LoadingOverlay ProgressDialog(UIViewController controller, string loadingMessage)
		{
			var bounds = UIScreen.MainScreen.Bounds;
			if (UIApplication.SharedApplication.StatusBarOrientation == UIInterfaceOrientation.LandscapeLeft || UIApplication.SharedApplication.StatusBarOrientation == UIInterfaceOrientation.LandscapeRight) 
				bounds.Size = new CoreGraphics.CGSize (bounds.Size.Height, bounds.Size.Width);

			LoadingOverlay overlay = new LoadingOverlay (bounds, loadingMessage);
			controller.View.Add (overlay);

			return overlay;
		}

		public static ToastOverlay ShowToast(UIViewController controller, ToastOverlay currentToast, string loadingMessage)
		{
			if (currentToast != null)
				currentToast.Remove ();
			
			var bounds = UIScreen.MainScreen.Bounds;
			if (UIApplication.SharedApplication.StatusBarOrientation == UIInterfaceOrientation.LandscapeLeft || UIApplication.SharedApplication.StatusBarOrientation == UIInterfaceOrientation.LandscapeRight) 
				bounds.Size = new CoreGraphics.CGSize (bounds.Size.Height, bounds.Size.Width);
							
			nfloat labelHeight = 22;
			nfloat labelWidth = bounds.Width;

			// derive the center x and y
			//nfloat centerX = bounds.Width / 2;
			nfloat centerY = bounds.Height / 2;

			var frameToSend = new CoreGraphics.CGRect (0f, centerY, labelWidth, labelHeight);

			ToastOverlay overlay = new ToastOverlay (frameToSend, loadingMessage);
			controller.View.Add (overlay);

			return overlay;
		}

		public static void OkDialog(string title, string message, Action callback = null)
		{
			UIAlertView alert = new UIAlertView (title, message, null, "Ok", null);
			if (callback != null) {
				alert.Clicked += (object sender, UIButtonEventArgs e) => {
					callback();
				};
			}
			alert.Show ();				
		}

		public static void YesNoDialog(string title, string message, Action YesCallBack, Action NoCallBack)
		{
			UIAlertView alert = new UIAlertView (title, message, null, "No", "Yes");
			alert.Clicked += (object sender, UIButtonEventArgs e) => {
				if (e.ButtonIndex == 1) {
					YesCallBack();
					return;
				}
				NoCallBack();
			};

			alert.Show ();	
		}

		public static void RestrictRotation(bool restriction)
		{
			AppDelegate app = (AppDelegate)UIApplication.SharedApplication.Delegate;
			app.RestrictRotation = restriction;
		} 

		/*public static void YesNoDialog(Context c, string message, Action YesCallBack, Action NoCallBack)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(c);
            builder.SetMessage(message)
                .SetPositiveButton("Yes", delegate { YesCallBack(); })
                .SetNegativeButton("No", delegate { NoCallBack(); })
                .Show();
        }*/
	}
}

