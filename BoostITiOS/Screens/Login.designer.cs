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
    [Register ("Login")]
    partial class Login
    {
        [Outlet]
        UIKit.UIButton btnLogin { get; set; }


        [Outlet]
        UIKit.UILabel lblRemember { get; set; }


        [Outlet]
        UIKit.UISwitch swRemember { get; set; }


        [Outlet]
        UIKit.UITextField txtPassword { get; set; }


        [Outlet]
        UIKit.UITextField txtUserName { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (btnLogin != null) {
                btnLogin.Dispose ();
                btnLogin = null;
            }

            if (lblRemember != null) {
                lblRemember.Dispose ();
                lblRemember = null;
            }

            if (swRemember != null) {
                swRemember.Dispose ();
                swRemember = null;
            }

            if (txtPassword != null) {
                txtPassword.Dispose ();
                txtPassword = null;
            }

            if (txtUserName != null) {
                txtUserName.Dispose ();
                txtUserName = null;
            }
        }
    }
}