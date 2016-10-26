// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace BoostITiOS
{
    [Register ("ChangeVehicle")]
    partial class ChangeVehicle
    {
        [Outlet]
        UIKit.UIBarButtonItem btnDone { get; set; }


        [Outlet]
        UIKit.UITableView tvChangeVehicles { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (btnDone != null) {
                btnDone.Dispose ();
                btnDone = null;
            }

            if (tvChangeVehicles != null) {
                tvChangeVehicles.Dispose ();
                tvChangeVehicles = null;
            }
        }
    }
}