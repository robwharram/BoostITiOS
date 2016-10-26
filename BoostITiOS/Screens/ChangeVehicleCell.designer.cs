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
    [Register ("ChangeVehicleCell")]
    partial class ChangeVehicleCell
    {
        [Outlet]
        UIKit.UILabel lblPrice { get; set; }


        [Outlet]
        UIKit.UILabel lblStock { get; set; }


        [Outlet]
        UIKit.UILabel lblVIN { get; set; }


        [Outlet]
        UIKit.UILabel lblYearMakeModel { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (lblPrice != null) {
                lblPrice.Dispose ();
                lblPrice = null;
            }

            if (lblStock != null) {
                lblStock.Dispose ();
                lblStock = null;
            }

            if (lblVIN != null) {
                lblVIN.Dispose ();
                lblVIN = null;
            }

            if (lblYearMakeModel != null) {
                lblYearMakeModel.Dispose ();
                lblYearMakeModel = null;
            }
        }
    }
}