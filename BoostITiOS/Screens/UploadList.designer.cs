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
    [Register ("UploadList")]
    partial class UploadList
    {
        [Outlet]
        UIKit.UIBarButtonItem btnDone { get; set; }


        [Outlet]
        UIKit.UIBarButtonItem btnUpload { get; set; }


        [Outlet]
        UIKit.UITableView tvUpload { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (btnDone != null) {
                btnDone.Dispose ();
                btnDone = null;
            }

            if (btnUpload != null) {
                btnUpload.Dispose ();
                btnUpload = null;
            }

            if (tvUpload != null) {
                tvUpload.Dispose ();
                tvUpload = null;
            }
        }
    }
}