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
    [Register ("UploadProgress")]
    partial class UploadProgress
    {
        [Outlet]
        UIKit.UIBarButtonItem btnCancel { get; set; }


        [Outlet]
        UIKit.UIBarButtonItem btnPause { get; set; }


        [Outlet]
        UIKit.UILabel lblCurrentVehicle { get; set; }


        [Outlet]
        UIKit.UILabel lblPrepareImage { get; set; }


        [Outlet]
        UIKit.UILabel lblTotal { get; set; }


        [Outlet]
        UIKit.UINavigationBar nbUpload { get; set; }


        [Outlet]
        UIKit.UIProgressView pbCurrentVehicle { get; set; }


        [Outlet]
        UIKit.UIProgressView pbPrepareImage { get; set; }


        [Outlet]
        UIKit.UIProgressView pbTotal { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (btnCancel != null) {
                btnCancel.Dispose ();
                btnCancel = null;
            }

            if (btnPause != null) {
                btnPause.Dispose ();
                btnPause = null;
            }

            if (lblCurrentVehicle != null) {
                lblCurrentVehicle.Dispose ();
                lblCurrentVehicle = null;
            }

            if (lblPrepareImage != null) {
                lblPrepareImage.Dispose ();
                lblPrepareImage = null;
            }

            if (lblTotal != null) {
                lblTotal.Dispose ();
                lblTotal = null;
            }

            if (nbUpload != null) {
                nbUpload.Dispose ();
                nbUpload = null;
            }

            if (pbCurrentVehicle != null) {
                pbCurrentVehicle.Dispose ();
                pbCurrentVehicle = null;
            }

            if (pbPrepareImage != null) {
                pbPrepareImage.Dispose ();
                pbPrepareImage = null;
            }

            if (pbTotal != null) {
                pbTotal.Dispose ();
                pbTotal = null;
            }
        }
    }
}