using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;

namespace BoostITiOS
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the
	// User Interface of the application, as well as listening (and optionally responding) to
	// application events from iOS.
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		// class-level declarations
		UIWindow window;
		UINavigationController _nav;

		public bool RestrictRotation { get; set; }

		//
		// This method is invoked when the application has loaded and is ready to run. In this
		// method you should instantiate the window, load the UI into it and then make the window
		// visible.
		//
		// You have 17 seconds to return from this method, or iOS will terminate your application.
		//
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			// create a new window instance based on the screen size
			window = new UIWindow (UIScreen.MainScreen.Bounds);
			
			_nav = new UINavigationController ();
			_nav.SetNavigationBarHidden (true, false);
			//_nav.PushViewController (new CreateVehicle (), false);
			_nav.PushViewController(new Login(), false);
			window.RootViewController = _nav;

			// make the window visible
			window.MakeKeyAndVisible ();
			
			return true;
		}

		public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations (UIApplication application, UIWindow forWindow)
		{
			if (this.RestrictRotation)
				return UIInterfaceOrientationMask.Portrait;
			else
				return UIInterfaceOrientationMask.AllButUpsideDown;
		}

	}
}

