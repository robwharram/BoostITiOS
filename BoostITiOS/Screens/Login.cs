
using System;
using System.Drawing;

using Foundation;
using UIKit;
using BoostIT.Tasks;
using System.Threading;
using System.ComponentModel;
using System.Collections.Generic;
using BoostIT.Models;
using System.Linq;

namespace BoostITiOS
{
	public partial class Login : UIViewController
	{
		public Login () : base ("Login", null)
		{
		}

		private LoadingOverlay loadingOverlay;

		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			Controls.RestrictRotation (true);

			btnLogin.Font = UIFont.FromName ("Good Times", 20f);
			btnLogin.BackgroundColor = UIColor.FromRGB ((231f / 255f), (77f / 255f), (62f / 255f));
			txtPassword.SecureTextEntry = true;

			btnLogin.TouchDown += (object sender, EventArgs e) => {
				Controls.WifiDialog("You are not connected to a WiFi network.  Continue?", login);
			};

			string userName = NSUserDefaults.StandardUserDefaults.StringForKey ("UserName");
			string password = NSUserDefaults.StandardUserDefaults.StringForKey ("Password");
			if (!string.IsNullOrWhiteSpace (userName) && !string.IsNullOrWhiteSpace (password)) {
				txtUserName.Text = userName;
				txtPassword.Text = password;
			}

			var g = new UITapGestureRecognizer (() => View.EndEditing (true));
			g.CancelsTouchesInView = false;
			View.AddGestureRecognizer (g);
		}

		public void login()
		{
			loadingOverlay = Controls.ProgressDialog (this, "Attempting Login...");
			//this.View.Add (loadingOverlay);

			Async.BackgroundProcess (bw_Login, bw_LoginCompleted, new loginCredentials(txtUserName.Text, txtPassword.Text));
		}

		private void bw_Login(object sender, DoWorkEventArgs e)
		{
			loginCredentials cred = (loginCredentials)e.Argument;
			int userId = new LoginService().GetUserID(cred.usermame, cred.password);
			if (userId <= 0)
				throw new Exception ("Invalid user name or password.");

			e.Result = userId;
		}

		private void bw_LoginCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			loadingOverlay.Hide ();

			if (e.Error != null) {
				Controls.OkDialog ("Invalid Credentials", e.Error.Message);
				return;
			}

			//Controls.OkDialog ("Success", "Logged in successfully!");
			SaveLogin((int)e.Result);
			NavigationController.PushViewController (new Home (), true);
		}

		private void SaveLogin(int userId)
		{
			NSUserDefaults.StandardUserDefaults.SetInt (userId, "UserID");
			List<Dealership> listOfDealers = new GetDealers().GetDealersFromService(userId);
			if (listOfDealers.Count == 1) {
				NSUserDefaults.StandardUserDefaults.SetInt(listOfDealers[0].DealershipID, "SelectedDealershipID");
				NSUserDefaults.StandardUserDefaults.SetString(listOfDealers[0].DealershipName, "SelectedDealershipName");
			}

			int selectedDealershipID = (int)NSUserDefaults.StandardUserDefaults.IntForKey("SelectedDealershipID");
			if (selectedDealershipID > 0) {
				Dealership validatedDealer = listOfDealers.FirstOrDefault(d => d.DealershipID == selectedDealershipID);
				if (validatedDealer == null) {
					NSUserDefaults.StandardUserDefaults.SetInt(0, "SelectedDealershipID");
					NSUserDefaults.StandardUserDefaults.SetString("", "SelectedDealershipName");
				}
			}

			if (swRemember.On) {
				NSUserDefaults.StandardUserDefaults.SetString (txtUserName.Text, "UserName");
				NSUserDefaults.StandardUserDefaults.SetString (txtPassword.Text, "Password");
			} else {
				NSUserDefaults.StandardUserDefaults.SetString ("", "UserName");
				NSUserDefaults.StandardUserDefaults.SetString ("", "Password");
			}				
		}
	
		public class loginCredentials {

			public string usermame { get; set; }
			public string password { get; set; }

			public loginCredentials(string UserName, string Password)
			{
				this.usermame = UserName;
				this.password = Password;
			}

		}

	}
}

