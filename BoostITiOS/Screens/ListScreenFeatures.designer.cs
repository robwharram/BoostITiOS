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
    [Register ("ListScreenFeatures")]
    partial class ListScreenFeatures
    {
        [Outlet]
        UIKit.UIButton btnDone { get; set; }


        [Outlet]
        UIKit.UITableView tvFeatures { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (btnDone != null) {
                btnDone.Dispose ();
                btnDone = null;
            }

            if (tvFeatures != null) {
                tvFeatures.Dispose ();
                tvFeatures = null;
            }
        }
    }
}