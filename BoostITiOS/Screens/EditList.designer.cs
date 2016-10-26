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
    [Register ("EditList")]
    partial class EditList
    {
        [Outlet]
        UIKit.UIButton btnDisclosure { get; set; }


        [Outlet]
        UIKit.UILabel lblTitle { get; set; }


        [Outlet]
        UIKit.UILabel lblValue { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (btnDisclosure != null) {
                btnDisclosure.Dispose ();
                btnDisclosure = null;
            }

            if (lblTitle != null) {
                lblTitle.Dispose ();
                lblTitle = null;
            }

            if (lblValue != null) {
                lblValue.Dispose ();
                lblValue = null;
            }
        }
    }
}