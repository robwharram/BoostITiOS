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
    [Register ("UploadListDealers")]
    partial class UploadListDealers
    {
        [Outlet]
        UIKit.UIBarButtonItem btnActions { get; set; }


        [Outlet]
        UIKit.UIBarButtonItem btnCancel { get; set; }


        [Outlet]
        UIKit.UITableView tvDealers { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (btnActions != null) {
                btnActions.Dispose ();
                btnActions = null;
            }

            if (btnCancel != null) {
                btnCancel.Dispose ();
                btnCancel = null;
            }

            if (tvDealers != null) {
                tvDealers.Dispose ();
                tvDealers = null;
            }
        }
    }
}