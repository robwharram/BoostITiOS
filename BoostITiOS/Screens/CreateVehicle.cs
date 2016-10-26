
using System;
using System.Drawing;

using Foundation;
using UIKit;
using System.Collections.Generic;
using BoostIT.Models;
using SQLite;
using BoostIT.DataAccess;
using System.Linq;
using BoostIT;
using BoostIT.Tasks;

namespace BoostITiOS
{
	public partial class CreateVehicle : UIViewController
	{
		private Dictionary<string, EditItem> dicEditItems;
		private int vehicleid = 0;
		private int vehicleSelectedDealershipID = 0;
		private List<Make> listMakes;
		private List<Model> listModels;
		private List<Body> listBodies;
		private List<Drivetrain> listDrivetrains;
		private List<AvailableFeature> listFeatures;
		private string[] listTransmissions, listExteriorColours, listInteriorColours, listDoors, listSeats, listInteriorFeatures, listExteriorFeatures, listSafetyFeatures, listMechanicalFeatures, listYears, listEngine;
		private UIScrollView scrollView;
		private nfloat keyboardHeight = 0f;
		private bool keyboardShowing = false;
		private bool originChangeVehicle = false;
		private LoadingOverlay loadingOverlay;
		private List<Dealership> listOfDealers;

		public CreateVehicle (int vehicleId, bool isFromChangeVehicle = false) : base ("CreateVehicle", null)
		{
			vehicleid = vehicleId;
			originChangeVehicle = isFromChangeVehicle;
		}

		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			LoadDataFromDB ();

			GenerateLayout ();

			CheckDealershipID();

			setElementValues ();

			NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.DidShowNotification,KeyBoardUpNotification);
			NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.DidHideNotification,KeyBoardHideNotification);
			NSNotificationCenter.DefaultCenter.AddObserver (UITextField.TextDidBeginEditingNotification, TextFieldOnFocus);

			var g = new UITapGestureRecognizer (() => View.EndEditing (true));
			g.CancelsTouchesInView = false;
			View.AddGestureRecognizer (g);
		}

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);

			if (UIDevice.CurrentDevice.Orientation != UIDeviceOrientation.Portrait)
				UIDevice.CurrentDevice.SetValueForKey (NSObject.FromObject(UIInterfaceOrientation.Portrait), new NSString("orientation"));

			Controls.RestrictRotation (true);	
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
		}

		private void validateVehicle()
		{
			//checking that all the required fields are filled out
			if (string.IsNullOrWhiteSpace(GetEditItemValue("YEAR")))
				Controls.OkDialog("Required Fields Missing" ,"You must fill in all the required fields");
			else if (string.IsNullOrWhiteSpace(GetEditItemValue("MAKE")))
				Controls.OkDialog("Required Fields Missing", "You must fill in all the required fields");
			else if (string.IsNullOrWhiteSpace(GetEditItemValue("MODEL")))
				Controls.OkDialog("Required Fields Missing", "You must fill in all the required fields");
			else if (string.IsNullOrWhiteSpace(GetEditItemValue("BODY")))
				Controls.OkDialog("Required Fields Missing", "You must fill in all the required fields");
			else if (string.IsNullOrWhiteSpace(GetEditItemValue("MILEAGE")))
				Controls.OkDialog("Required Fields Missing", "You must fill in all the required fields");
			else if (string.IsNullOrWhiteSpace(GetEditItemValue("PRICE")))
				Controls.OkDialog("Required Fields Missing", "You must fill in all the required fields");
			else if (!string.IsNullOrWhiteSpace(GetEditItemValue("VIN")) && GetEditItemValue("VIN").Length != 17)
				Controls.OkDialog("VIN Validation Failed", "Vin must be exactly 17 characters");
			else {			
				SaveVehicle(); //if all required fields are filled out then save the vehicle

				NavigationController.PopViewController (true);
				//if (root.Equals("change")) 
				//	StartActivity(new Intent(this, typeof(ChangeVehicle)));                    

				//Finish();
			}
		}

		private void SaveVehicle()
		{
			Vehicle v = GetVehicleValues();
			if (v != null)
				using (Connection sqlConn = new Connection(SQLiteBoostDB.GetDBPath()))
					new VehicleDB(sqlConn).SaveVehicle(v);
		}

		private Vehicle GetVehicleValues()
		{
			Vehicle v = LoadVehicleFromDatabase();

			//store values from form into vehicle object
			if (v != null)
			{
				string txtYear = GetEditItemValue ("YEAR");
				string txtMake = GetEditItemValue ("MAKE");
				string txtModel = GetEditItemValue ("MODEL");
				string txtBody = GetEditItemValue ("BODY");
				string txtMileage = GetEditItemValue ("MILEAGE");
				string txtPrice = GetEditItemValue ("PRICE");
				string txtStock = GetEditItemValue ("STOCKNUMBER");
				string txtVIN = GetEditItemValue ("VIN");
				string txtExterior = GetEditItemValue ("EXTERIOR COLOUR");
				string txtInterior = GetEditItemValue ("INTERIOR COLOUR");
				string txtSubModel = GetEditItemValue ("SUB-MODEL");
				string txtTransmission = GetEditItemValue ("TRANSMISSION");
				string txtEngine = GetEditItemValue ("ENGINE");
				string txtSeats = GetEditItemValue ("SEATS");
				string txtDoors = GetEditItemValue ("DOORS");
				string txtDriveTrain = GetEditItemValue ("DRIVETRAIN");

				v.Year = null;
				if (!string.IsNullOrWhiteSpace(txtYear) && txtYear.ToLower() != "required" && txtYear.ToShort() > 0)
					v.Year = txtYear.ToShort();

				v.Make = "";
				v.MakeID = 0;
				if (!string.IsNullOrWhiteSpace(txtMake) && txtMake.ToLower() != "required") {
					v.Make = txtMake;
					Make make = listMakes.FirstOrDefault(m => m.MakeName == txtMake);
					v.MakeID = (make != null && make.ID > 0) ? make.ID : 0;
				}

				v.Model = "";
				v.ModelID = 0;
				if (!string.IsNullOrWhiteSpace(txtModel) && txtModel.ToLower() != "required") {
					v.Model = txtModel;                    
					Model model = listModels.FirstOrDefault(m => m.MakeID == v.MakeID && m.ModelName == txtModel);
					v.ModelID = (model != null && model.ID > 0) ? model.ID : 0;                    
				}

				v.Body = "";
				v.BodyID = 0;
				if (!string.IsNullOrWhiteSpace(txtBody) && txtBody.ToLower() != "required") {
					v.Body = txtBody;
					Body body = listBodies.FirstOrDefault(b => b.BodyStyle == txtBody);
					v.BodyID = (body != null && body.ID > 0) ? body.ID : 0;
				}

				v.DriveTrainID = 0;
				if (!string.IsNullOrWhiteSpace(txtDriveTrain) && txtDriveTrain.ToLower() != "required") {
					Drivetrain driveTrain = listDrivetrains.FirstOrDefault(d => d.DrivetrainName == txtDriveTrain);
					v.DriveTrainID = (driveTrain != null && driveTrain.DrivetrainID > 0) ? driveTrain.DrivetrainID : 0;
				}

				v.Mileage = null;
				if (!string.IsNullOrWhiteSpace(txtMileage) && txtMileage.ToInt() >= 0)
					v.Mileage = txtMileage.ToInt();

				v.Price = null;
				if (!string.IsNullOrWhiteSpace(txtPrice) && txtPrice.ToInt() >= 0) 
					v.Price = txtPrice.ToInt();

				v.StockNumber = "";
				if (!string.IsNullOrWhiteSpace(txtStock))
					v.StockNumber = txtStock;

				v.VIN = "";
				if (!string.IsNullOrWhiteSpace(txtVIN))
					v.VIN = txtVIN.ToUpper();

				v.Colour = "";
				if (!string.IsNullOrWhiteSpace(txtExterior) && txtExterior != "optional")
					v.Colour = txtExterior;

				v.SubModel = "";
				if (!string.IsNullOrWhiteSpace(txtSubModel))
					v.SubModel = txtSubModel;

				v.Engine = "";
				if (!string.IsNullOrWhiteSpace(txtEngine) && txtEngine.ToLower() != "optional")
					v.Engine = txtEngine;

				v.Transmission = "";
				if (!string.IsNullOrWhiteSpace(txtTransmission) && txtTransmission != "optional")
					v.Transmission = txtTransmission;

				v.Interior = "";
				if (!string.IsNullOrWhiteSpace(txtInterior) && txtInterior != "optional")
					v.Interior = txtInterior;

				v.Doors = "";
				if (!string.IsNullOrWhiteSpace(txtDoors) && txtDoors != "optional")
					v.Doors = txtDoors;

				v.Seats = "";
				if (!string.IsNullOrWhiteSpace(txtSeats) && txtSeats != "optional")
					v.Seats = txtSeats;
			}

			return v;
		}   

		private void bw_GetDealers(object sender, System.ComponentModel.DoWorkEventArgs e)
		{
			int UserID = (int)e.Argument;
			if (UserID <= 0)
			{
				e.Result = listOfDealers;
				return;
			}

			listOfDealers = new GetDealers().GetDealersFromService(UserID);

			e.Result = listOfDealers;
		}

		private void bw_GetDealersCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
		{
			loadingOverlay.Hide();

			if (e.Error != null)
			{
				Controls.OkDialog("Error", "There was an error getting the dealers. Error was: " + e.Error.Message);
				return;
			}

			if (listOfDealers == null || listOfDealers.Count <= 0)
			{
				Controls.OkDialog("No Dealers Found", "We were unable to find any dealers that you have access to, please login again.");
				return;
			}

			if (listOfDealers.Count == 1)
			{
				dealershipSelected("Dealership", listOfDealers[0].DealershipName);
				return;
			}

			Controls.OkDialog("No Dealer Selected", "This vehicle is not assigned to a dealership.  You must now select the dealership that this vehicle belongs to before continuing.", delegate { PresentModalViewController(new ListScreenSearch(dealershipSelected, listOfDealers.Select(d => d.DealershipName).ToArray(), "Dealership"), true); });
		}


		public void dealershipSelected(string name, string value)
		{
			Console.WriteLine("selected dealer create vehicle=" + value);
			Dealership dealer = listOfDealers.FirstOrDefault(d => d.DealershipName == value);
			if (dealer == null) {
				Controls.OkDialog("Invalid Dealer", "Selected dealer does not exist, please select a different dealer.");
				return;
			}

			Vehicle v = LoadVehicleFromDatabase();
			v.DealershipID = dealer.DealershipID;
			using (Connection sqlConn = new Connection(SQLiteBoostDB.GetDBPath()))
				new VehicleDB(sqlConn).SaveVehicle(v);
		}

		private void Cancel()
		{
			Controls.YesNoDialog("Confirm Cancel", "Are you sure you want to cancel?",
				delegate { 
					if (!originChangeVehicle) {
						using (Connection sqlConn = new Connection(SQLiteBoostDB.GetDBPath()))
							new VehicleDB(sqlConn).DeleteVehicle(vehicleid);  
					}
					NavigationController.PopViewController(true);
				},
				delegate { return; });

		}

		private async void scanClicked()
		{
			Controls.RestrictRotation (false);
			//new UIAlertView("scan Clicked","You clicked on 'SCAN'",null,"Ok", null).Show();
			var options = new ZXing.Mobile.MobileBarcodeScanningOptions();
			options.PossibleFormats = new List<ZXing.BarcodeFormat>() { 
				ZXing.BarcodeFormat.CODE_128, ZXing.BarcodeFormat.CODE_39, ZXing.BarcodeFormat.CODE_93 
			};

			var scanner = new ZXing.Mobile.MobileBarcodeScanner();
			var result = await scanner.Scan(options,true);
			if (result != null) {
				string vin = result.Text;
				if (result.Text.Length == 18) 
					vin = vin.ToUpper ().StartsWith ("I") ? vin.Remove (0, 1) : vin;
				SetEditItemValue ("VIN", vin);
			}
		}
			

		private void explodeClicked()
		{
			SaveVehicle(); //save the vehicle before exploding so that we get the latest VIN entered.
			Controls.WifiDialog("You are not connected to WiFi.  Are you sure you want to explode the VIN?", delegate {
				loadingOverlay = Controls.ProgressDialog(this, "Loading Vehicle Data");
				Async.BackgroundProcess(bw_ExplodeVIN, bw_ExplodeVINComplete);
			});
		}

        private void bw_ExplodeVIN(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
			int selectedDealershipID = (int)NSUserDefaults.StandardUserDefaults.IntForKey("SelectedDealershipID");
			int userID = (int)NSUserDefaults.StandardUserDefaults.IntForKey("UserID");

			if (userID <= 0)
				throw new Exception("Invalid User ID. User ID=" + userID + ".");
			if (selectedDealershipID <= 0)
				throw new Exception("Invalid Selected Dealership ID. SelectedDealershipID=" + selectedDealershipID + ".");
			
			//explode the vehicle using PCL
			using (Connection sqlConn = new Connection(SQLiteBoostDB.GetDBPath()))
                new ExplodeVIN(sqlConn).DoExplosion(vehicleid,userID, selectedDealershipID);
        }

        private void bw_ExplodeVINComplete(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
			loadingOverlay.Hide ();
            if (e.Error != null) {
                Controls.OkDialog("Explode Error", "There was an error exploding the VIN.  The Error was: " + e.Error.Message);
                return;
            }

            //success
            setElementValues();
			Controls.ShowToast (this, null, "VIN Exploded Successfully");
            //Toast.MakeText(this, "VIN Exploded Successfully", ToastLength.Short).Show();
        }
			
		internal void OpenListScreen(string ItemName, string[] data, bool isSearchable)
		{
			if (ItemName == "MODEL") {
				string selectedMake = GetEditItemValue("MAKE");
				Make make = listMakes.FirstOrDefault(mk => mk.MakeName == selectedMake);
				if (string.IsNullOrWhiteSpace(selectedMake) || make == null) {
					Controls.OkDialog("Make is Required", "You must select a make first");
					return;
				}

				data = listModels.Where (m => m.MakeID == make.ID).Select (m => m.ModelName).ToArray ();
			}

			if (isSearchable)
				PresentModalViewController (new ListScreenSearch (SetEditItemValue, data, ItemName), true);
			else
				PresentModalViewController (new ListScreenController (SetEditItemValue, data, ItemName), true);
		}

		private List<UIView> GetVINViews()
		{
			List<UIView> listVINViews = new List<UIView> ();
			listVINViews.Add (new EditButton("SCAN A VIN", scanClicked).View);
			listVINViews.Add (new EditButton("EXPLODE A VIN", explodeClicked).View);
			listVINViews.Add (GetEditText ("VIN", 1, UIKeyboardType.Default, UIReturnKeyType.Next));
			listVINViews.Add (GetEditText ("STOCKNUMBER", 2, UIKeyboardType.Default, UIReturnKeyType.Next));

			return listVINViews;
		}

		private List<UIView> GetReqViews()
		{
			List<UIView> listReqViews = new List<UIView> ();
			listReqViews.Add (GetEditList("YEAR", listYears));
			listReqViews.Add (GetEditList("MAKE", listMakes.Select(m => m.MakeName).ToArray()));
			listReqViews.Add (GetEditList("MODEL",null));
			listReqViews.Add (GetEditText("SUB-MODEL", 4, UIKeyboardType.Default, UIReturnKeyType.Next));
			listReqViews.Add (GetEditList("BODY",listBodies.Select(b => b.BodyStyle).ToArray()));
			listReqViews.Add (GetEditText ("MILEAGE", 6, UIKeyboardType.NumbersAndPunctuation, UIReturnKeyType.Next, "required"));
			listReqViews.Add (GetEditText ("PRICE", 7, UIKeyboardType.NumbersAndPunctuation, UIReturnKeyType.Next, "required"));

			return listReqViews;
		}

		private List<UIView> GetOptViews()
		{
			List<UIView> listViews = new List<UIView> ();
			listViews.Add (GetEditList("ENGINE", listEngine, true, "optional"));
			listViews.Add (GetEditList("TRANSMISSION", listTransmissions, true, "optional"));
			listViews.Add (GetEditList("DRIVETRAIN", listDrivetrains.Select(d => d.DrivetrainName).ToArray(), false, "optional"));
			listViews.Add (GetEditList("EXTERIOR COLOUR", listExteriorColours, true, "optional"));
			listViews.Add (GetEditList("INTERIOR COLOUR", listInteriorColours, true, "optional"));
			listViews.Add (GetEditList("DOORS", listDoors, true, "optional"));
			listViews.Add (GetEditList("SEATS", listSeats, true, "optional"));

			return listViews;
		}

		private List<UIView> GetFeatureViews()
		{
			List<UIView> listViews = new List<UIView> ();
			listViews.Add (new EditButton("INTERIOR FEATURES", delegate { PresentModalViewController (new ListScreenFeatures (listInteriorFeatures, vehicleid, 2, listFeatures), true); }).View);
			listViews.Add (new EditButton("EXTERIOR FEATURES", delegate { PresentModalViewController (new ListScreenFeatures (listExteriorFeatures, vehicleid, 3, listFeatures), true); }).View);
			listViews.Add (new EditButton("SAFETY FEATURES", delegate { PresentModalViewController (new ListScreenFeatures (listSafetyFeatures, vehicleid, 4, listFeatures), true); }).View);
			listViews.Add (new EditButton("MECHANICAL FEATURES", delegate { PresentModalViewController (new ListScreenFeatures (listMechanicalFeatures, vehicleid, 5, listFeatures), true); }).View);

			return listViews;
		}

		private List<UIView> GetPhotosViews()
		{
			List<UIView> listViews = new List<UIView> ();
			listViews.Add (new EditButton("PHOTOS", delegate { PresentModalViewController (new Photos (vehicleid), true); }).View);

			return listViews;
		}

		private List<UIView> GetWholesaleViews()
		{
			List<UIView> listViews = new List<UIView> ();
			listViews.Add (new EditButton("INTERIOR DAMAGES", delegate { NavigationController.PushViewController(new DamageList (vehicleid, 1), true); }).View);
			listViews.Add (new EditButton("EXTERIOR DAMAGES", delegate { NavigationController.PushViewController (new DamageList (vehicleid, 2), true); }).View);
			listViews.Add (new EditButton("MECHANICAL DAMAGES", delegate { NavigationController.PushViewController (new DamageList (vehicleid, 3), true); }).View);
			listViews.Add (new EditButton("PAINT", delegate { PresentModalViewController (new Paint (vehicleid), true); }).View);
			listViews.Add (new EditButton("TIRES", delegate { PresentModalViewController (new Tires (vehicleid), true); }).View);

			return listViews;
		}

		private UILabel GetLabel(string labelText)
		{
			UILabel label = new UILabel (new CoreGraphics.CGRect(0f,0f, this.View.Bounds.Width, 10f));
			label.Font = UIFont.FromName ("Good Times", 14f);
			label.TextColor = UIColor.White;
			label.Text = labelText;

			return label;
		}

		private UIView GetEditText(string name, int textTag, UIKeyboardType keyboardType, UIReturnKeyType returnKeyType, string defaultValue="optional")
		{
			EditText et = new EditText (this, textTag, name, keyboardType, returnKeyType, defaultValue);
			dicEditItems.Add (name, et as EditItem);

			return et.View;
		}

		private UIView GetEditList(string name, string[] listData, bool isSearchable = false, string defaultValue = "required")
		{
			EditList el = new EditList (this.OpenListScreen,name, name, listData, isSearchable, defaultValue);
			dicEditItems.Add (name, el as EditItem);

			return el.View;
		}

		private string GetEditItemValue(string name)
		{
			EditItem item;
			if (dicEditItems.TryGetValue (name, out item) && item != null)
				return item.GetValue ().Replace("required","").Replace("optional", "");

			return string.Empty;
		}

		public void SetEditItemValue(string name, string value)
		{
			EditItem item;
			if (dicEditItems.TryGetValue (name, out item) && item != null)
				item.SetValue (value);
		}

		private void LoadDataFromDB()
		{
			//******************************LOAD DATA FROM DB*********************************************
			using (Connection sqlConn = new Connection(SQLiteBoostDB.GetDBPath())) {
				VehicleAssetsDB vadb = new VehicleAssetsDB(sqlConn);

				listMakes = vadb.GetMakes();
				listModels = vadb.GetModels();
				listBodies = vadb.GetBodies();
				listDrivetrains = vadb.GetDrivetrains();
				listTransmissions = vadb.GetTransmissions().Select(t => t.TransmissionName).ToArray();
				listExteriorColours = vadb.GetExteriorColours().Select(e => e.Colour).ToArray();
				listInteriorColours = vadb.GetInteriorColours().Select(i => i.Colour).ToArray();
				listDoors = vadb.GetDoors().Select(d => d.NumberOfDoors).ToArray();
				listSeats = vadb.GetSeats().Select(s => s.NumberOfSeats).ToArray();
				listFeatures = vadb.GetAvailableFeatures();

				listInteriorFeatures = listFeatures.Where(af => af.FeatureCategoryID == 2).Select(af => af.Feature).ToArray();
				listExteriorFeatures = listFeatures.Where(af => af.FeatureCategoryID == 3).Select(af => af.Feature).ToArray();
				listSafetyFeatures = listFeatures.Where(af => af.FeatureCategoryID == 4).Select(af => af.Feature).ToArray();
				listMechanicalFeatures = listFeatures.Where(af => af.FeatureCategoryID == 5).Select(af => af.Feature).ToArray();
			}

			//years
			int nextYear = DateTime.Now.Year + 1;
			List<string> listColYears = new List<string>();
			for (int i = nextYear; i >= nextYear - 45; i--)
				listColYears.Add(i.ToString());
			listYears = listColYears.ToArray ();

			//engine
			List<string> listColEngine = new List<string>();
			for (double i = 0.1; i <= 13.0; i += 0.1)
				listColEngine.Add(Math.Round(i, 1).ToString() + "L"); 
			listEngine = listColEngine.ToArray ();
		}

		private void GenerateLayout()
		{
			this.View.BackgroundColor = UIColor.FromPatternImage (UIImage.FromBundle("bg.jpg"));

			dicEditItems = new Dictionary<string, EditItem> ();
			scrollView = new UIScrollView (new CoreGraphics.CGRect(this.View.Bounds.X, 60f, this.View.Bounds.Width, this.View.Bounds.Height));

			//generate groups and labels
			scrollView.AddSubview (new EditGroup (GetVINViews()).View);
			scrollView.AddSubview (new EditGroup(GetReqViews()).View);
			scrollView.AddSubview (new EditGroup (GetOptViews ()).View);
			scrollView.AddSubview (GetLabel("FEATURES"));
			scrollView.AddSubview (new EditGroup (GetFeatureViews()).View);
			scrollView.AddSubview (GetLabel("PHOTOS"));
			scrollView.AddSubview (new EditGroup(GetPhotosViews()).View);
			scrollView.AddSubview (GetLabel("WHOLESALE"));
			scrollView.AddSubview (new EditGroup (GetWholesaleViews ()).View);

			//below spaces out the controls by 10 pixels (or whatever iOS unit uses)
			nfloat ypos = 0f;
			foreach (UIView view in scrollView.Subviews) {
				view.Frame = new CoreGraphics.CGRect (view.Bounds.X + 5f, ypos + 10f, this.View.Bounds.Width - 10f, view.Bounds.Height);
				ypos += view.Bounds.Height + 10f;
			}

			//makes height of scroller the total ypos of groups + 60 for the top bar + 10 for margin at bottom
			scrollView.ContentSize = new CoreGraphics.CGSize (scrollView.Bounds.Width, ypos + 60f + 10f);

			this.View.AddSubview (scrollView);

			btnSave.Clicked += (object sender, EventArgs e) => {
				validateVehicle();
			};

			btnCancel.Clicked += (object sender, EventArgs e) => {
				Cancel();
			};
		}

		private void selectDealer()
		{
			
		}

		private void CheckDealershipID()
		{
			Vehicle v = LoadVehicleFromDatabase();
			if (v.DealershipID == null || v.DealershipID == 0) {
				int UserID = (int)NSUserDefaults.StandardUserDefaults.IntForKey("UserID");
				loadingOverlay = Controls.ProgressDialog(this, "Loading Dealers...");
				Async.BackgroundProcess(bw_GetDealers, bw_GetDealersCompleted, UserID);
			}
		}

		private void setElementValues()
		{
			Vehicle v = LoadVehicleFromDatabase();

			//getting and checking data from vehicle
			if (v != null) {
				if (v.Year != null)
					SetEditItemValue ("YEAR", v.Year.ToString ());

				if (v.Make != null && !string.IsNullOrWhiteSpace (v.Make))
					SetEditItemValue ("MAKE", v.Make.ToString ());

				if (v.Model != null && !string.IsNullOrWhiteSpace(v.Model))
					SetEditItemValue ("MODEL", v.Model.ToString());

				if (v.Body != null && !string.IsNullOrWhiteSpace(v.Body))
					SetEditItemValue ("BODY", v.Body.ToString());

				if (v.Mileage != null && v.Mileage >= 0) 
					SetEditItemValue ("MILEAGE", v.Mileage.Value.ToString());

				if (v.Price != null && v.Price >= 0)
					SetEditItemValue ("PRICE", v.Price.Value.ToString());

				if (v.StockNumber != null && !string.IsNullOrWhiteSpace(v.StockNumber))
					SetEditItemValue ("STOCKNUMBER", v.StockNumber.ToString());

				if (v.VIN != null && !string.IsNullOrWhiteSpace(v.VIN))
					SetEditItemValue ("VIN", v.VIN.ToString());

				if (v.Colour != null && !string.IsNullOrWhiteSpace(v.Colour))
					SetEditItemValue ("EXTERIOR COLOUR", v.Colour.ToString());

				if (v.SubModel != null && !string.IsNullOrWhiteSpace(v.SubModel))
					SetEditItemValue ("SUB-MODEL", v.SubModel.ToString());

				if (v.Transmission != null && !string.IsNullOrWhiteSpace(v.Transmission))
					SetEditItemValue ("TRANSMISSION", v.Transmission.ToString());

				if (v.Engine != null && !string.IsNullOrWhiteSpace(v.Engine))
					SetEditItemValue ("ENGINE", v.Engine.ToString());

				if (v.Interior != null && !string.IsNullOrWhiteSpace(v.Interior))
					SetEditItemValue ("INTERIOR COLOUR", v.Interior.ToString());

				if (v.Seats != null && !string.IsNullOrWhiteSpace(v.Seats))
					SetEditItemValue ("SEATS", v.Seats.ToString());

				if (v.Doors != null && !string.IsNullOrWhiteSpace(v.Doors))
					SetEditItemValue ("DOORS", v.Doors.ToString());

				SetEditItemValue ("DRIVETRAIN", "");
				if (v.DriveTrainID != null && v.DriveTrainID > 0) {
					Drivetrain dt = listDrivetrains.FirstOrDefault(d => d.DrivetrainID == v.DriveTrainID.Value);
					if (dt != null)
						SetEditItemValue ("DRIVETRAIN", dt.DrivetrainName);
				}
			}
		}

		private Vehicle LoadVehicleFromDatabase()
		{
			Vehicle v = new Vehicle();
			using (Connection sqlConn = new Connection(SQLiteBoostDB.GetDBPath()))
				v = new VehicleDB(sqlConn).GetVehicle(vehicleid);

			return v;
		}

		private void KeyBoardUpNotification(NSNotification notification)
		{
			keyboardHeight = ((NSValue)(notification.UserInfo.ObjectForKey (UIKeyboard.BoundsUserInfoKey))).RectangleFValue.Height;

			//check to see if the keyboard pop up hides the textfield
			checkScrollRequired ();

			keyboardShowing = true;
		}

		private void KeyBoardHideNotification(NSNotification notification)
		{
			keyboardShowing = false;
		}

		private void TextFieldOnFocus(NSNotification notification)
		{
			//if the keyboard is already showing and the focus changes, check to see if the keyboard is hiding the textfield
			if (keyboardShowing)
				checkScrollRequired ();
		}

		private void checkScrollRequired()
		{
			//add Y value of Edit Text, Edit Group and Scroll View together to get the Y position on the screen of text box 
			//add 30 more pixels for the height of the Edit Text element
			//then get the Y value of the keyboard by getting the screen size and subtracting the keyboard height
			//subtract the keyboard Y value from the txt Y position (+ Edit List Height) to get the amount to scroll

			UIView firstResponder = GetFirstResponder (this.View);

			if (firstResponder != null) {
				UIView super1 = firstResponder.Superview;
				UIView super2 = firstResponder.Superview.Superview;
				UIView super3 = firstResponder.Superview.Superview.Superview;

				nfloat txtYPositionOnScreen = (firstResponder.Frame.Y + super1.Frame.Y + super2.Frame.Y + super3.Frame.Y + 30f) - scrollView.ContentOffset.Y;
				nfloat keyboardYPositionOnScreen = UIScreen.MainScreen.Bounds.Height - (keyboardHeight);
				nfloat scrollAmount = (txtYPositionOnScreen - keyboardYPositionOnScreen);

				if (scrollAmount > 0f)
					scrollView.SetContentOffset (new CoreGraphics.CGPoint (0f, scrollView.ContentOffset.Y + scrollAmount), true);
			}
		}

		private UIView GetFirstResponder(UIView parentView)
		{
			//recursively go through all views to find the first responder
			foreach (UIView view in parentView.Subviews) {
				if (view.IsFirstResponder)
					return view;
				if (view.Subviews.Count() > 0) {
					UIView recView = GetFirstResponder (view);
					if (recView != null)
						return recView;
				}
			}

			return null;
		}
	}
}

