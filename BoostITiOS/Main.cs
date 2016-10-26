using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;

namespace BoostITiOS
{
	public class Application
	{
		// This is the main entry point of the application.
		static void Main (string[] args)
		{
			// if you want to use a different Application Delegate class from "AppDelegate"
			// you can specify it here.
			//Xamarin.Insights.Initialize("35f406cba9eeab6650f8eee8565942a36bdba1ba");
			UIApplication.Main (args, null, "AppDelegate");

		}
	}
}
