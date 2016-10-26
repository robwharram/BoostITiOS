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
    [Register ("CreateVehicle")]
    partial class CreateVehicle
    {
        [Outlet]
        UIKit.UIBarButtonItem btnCancel { get; set; }


        [Outlet]
        UIKit.UIBarButtonItem btnSave { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (btnCancel != null) {
                btnCancel.Dispose ();
                btnCancel = null;
            }

            if (btnSave != null) {
                btnSave.Dispose ();
                btnSave = null;
            }
        }
    }
}