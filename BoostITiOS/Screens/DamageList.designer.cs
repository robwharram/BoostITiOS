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
    [Register ("DamageList")]
    partial class DamageList
    {
        [Outlet]
        UIKit.UIBarButtonItem btnAdd { get; set; }


        [Outlet]
        UIKit.UIBarButtonItem btnDone { get; set; }


        [Outlet]
        UIKit.UITableView tvDamages { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (btnAdd != null) {
                btnAdd.Dispose ();
                btnAdd = null;
            }

            if (btnDone != null) {
                btnDone.Dispose ();
                btnDone = null;
            }

            if (tvDamages != null) {
                tvDamages.Dispose ();
                tvDamages = null;
            }
        }
    }
}