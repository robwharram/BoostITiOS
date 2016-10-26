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
    [Register ("DownloadVehicles")]
    partial class DownloadVehicles
    {
        [Outlet]
        UIKit.UIBarButtonItem btnDownload { get; set; }


        [Outlet]
        UIKit.UIBarButtonItem btnMenu { get; set; }


        [Outlet]
        UIKit.UINavigationBar nbDownload { get; set; }


        [Outlet]
        UIKit.UITableView tvDownload { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (btnDownload != null) {
                btnDownload.Dispose ();
                btnDownload = null;
            }

            if (btnMenu != null) {
                btnMenu.Dispose ();
                btnMenu = null;
            }

            if (nbDownload != null) {
                nbDownload.Dispose ();
                nbDownload = null;
            }

            if (tvDownload != null) {
                tvDownload.Dispose ();
                tvDownload = null;
            }
        }
    }
}