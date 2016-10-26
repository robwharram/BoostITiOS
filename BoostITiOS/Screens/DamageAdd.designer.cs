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
    [Register ("DamageAdd")]
    partial class DamageAdd
    {
        [Outlet]
        UIKit.UIBarButtonItem btnCancel { get; set; }


        [Outlet]
        UIKit.UIButton btnImage { get; set; }


        [Outlet]
        UIKit.UIBarButtonItem btnSaVE { get; set; }


        [Outlet]
        UIKit.UITextView tvComments { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (btnCancel != null) {
                btnCancel.Dispose ();
                btnCancel = null;
            }

            if (btnImage != null) {
                btnImage.Dispose ();
                btnImage = null;
            }

            if (btnSaVE != null) {
                btnSaVE.Dispose ();
                btnSaVE = null;
            }

            if (tvComments != null) {
                tvComments.Dispose ();
                tvComments = null;
            }
        }
    }
}