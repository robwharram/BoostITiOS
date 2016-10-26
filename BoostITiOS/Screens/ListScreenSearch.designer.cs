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
    [Register ("ListScreenSearch")]
    partial class ListScreenSearch
    {
        [Outlet]
        UIKit.UIButton btnDone { get; set; }


        [Outlet]
        UIKit.UISearchBar searchBar { get; set; }


        [Outlet]
        UIKit.UITableView tableView { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (btnDone != null) {
                btnDone.Dispose ();
                btnDone = null;
            }

            if (searchBar != null) {
                searchBar.Dispose ();
                searchBar = null;
            }

            if (tableView != null) {
                tableView.Dispose ();
                tableView = null;
            }
        }
    }
}