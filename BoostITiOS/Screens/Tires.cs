
using System;

using Foundation;
using UIKit;
using System.Collections.Generic;
using BoostIT.Models;
using SQLite;
using BoostIT.DataAccess;
using BoostIT;
using System.Linq;

namespace BoostITiOS
{
	public partial class Tires : UIViewController
	{
		private string[] depths;
		private Dictionary<string, EditItem> dicEditItems;
		private int tag = 0;
		private UIScrollView scrollView;
		private nfloat keyboardHeight = 0f;
		private bool keyboardShowing = false;
		private int vehicleid;
		private string currentTextLabel = "";

		public Tires (int VehicleID) : base ("Tires", null)
		{
			vehicleid = VehicleID;
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

			Controls.RestrictRotation (true);

			dicEditItems = new Dictionary<string, EditItem> ();
			
			this.View.BackgroundColor = UIColor.FromPatternImage (UIImage.FromBundle("bg.jpg"));

			List<string> listDepths = new List<string> ();
			for (int i = 1; i <= 32; i++)
				listDepths.Add (i + "/32");
			depths = listDepths.ToArray ();

			scrollView = new UIScrollView (new CoreGraphics.CGRect(this.View.Bounds.X, 65f, this.View.Bounds.Width, this.View.Bounds.Height));
			scrollView.AddSubview (GetLabel("Left Front"));
			scrollView.AddSubview (new EditGroup (GetTireGroup("LF")).View);
			scrollView.AddSubview (GetLabel("Left Rear"));
			scrollView.AddSubview (new EditGroup (GetTireGroup("LR")).View);
			scrollView.AddSubview (GetLabel("Right Front"));
			scrollView.AddSubview (new EditGroup (GetTireGroup("RF")).View);
			scrollView.AddSubview (GetLabel("Right Rear"));
			scrollView.AddSubview (new EditGroup (GetTireGroup("RR")).View);
			scrollView.AddSubview (GetLabel("Spare"));
			scrollView.AddSubview (new EditGroup (GetTireGroup("SP")).View);

			nfloat ypos = 0f;
			foreach (UIView view in scrollView.Subviews) {
				view.Frame = new CoreGraphics.CGRect (view.Bounds.X + 5f, ypos + 10f, this.View.Bounds.Width - 10f, view.Bounds.Height);
				ypos += view.Bounds.Height + 10f;
			}

			//makes height of scroller the total ypos of groups + 60 for the top bar + 10 for margin at bottom
			scrollView.ContentSize = new CoreGraphics.CGSize (scrollView.Bounds.Width, ypos + 60f + 10f);

			this.View.AddSubview (scrollView);

			btnSave.Clicked += (object sender, EventArgs e) => { Validate(); };
			btnCancel.Clicked += (object sender, EventArgs e) => { Done(); };

			NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.DidShowNotification,KeyBoardUpNotification);
			NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.DidHideNotification,KeyBoardHideNotification);
			NSNotificationCenter.DefaultCenter.AddObserver (UITextField.TextDidBeginEditingNotification, TextFieldOnFocus);
			NSNotificationCenter.DefaultCenter.AddObserver (UITextField.TextDidEndEditingNotification, TextFieldEndEditing);

			setElementValues();

			var g = new UITapGestureRecognizer (() => View.EndEditing (true));
			g.CancelsTouchesInView = false;
			View.AddGestureRecognizer (g);
		}

		private void Done()
		{
			DismissModalViewController (true);
		}

		private List<UIView> GetTireGroup(string appendLabel)
		{
			tag += 1;
			List<UIView> listViews = new List<UIView> ();
			listViews.Add (GetEditText(appendLabel + " Brand", tag, "Brand", UIKeyboardType.Default, UIReturnKeyType.Next, "required"));
			tag += 1;
			listViews.Add (GetEditText(appendLabel + " Size", tag, "Size", UIKeyboardType.NumbersAndPunctuation, UIReturnKeyType.Next, "required"));
			listViews.Add (GetEditList(appendLabel + " Depth", "Depth", depths, false));

			tag += 1;
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

		private UIView GetEditText(string name, int textTag, string label, UIKeyboardType keyboardType, UIReturnKeyType returnKeyType, string defaultValue="optional")
		{
			EditText et = new EditText (this, textTag, label, keyboardType, returnKeyType, defaultValue);
			dicEditItems.Add (name, et as EditItem);

			return et.View;
		}

		private UIView GetEditList(string name, string label, string[] listData, bool isSearchable = false, string defaultValue = "required")
		{
			EditList el = new EditList (this.OpenListScreen, name, label, listData, isSearchable, defaultValue);
			dicEditItems.Add (name, el as EditItem);

			return el.View;
		}

		private void OpenListScreen(string ItemName, string[] data, bool isSearchable)
		{
			//if (isSearchable)
			//	PresentModalViewController (new ListScreenSearch (this, data, ItemName), true);
			//else
			PresentModalViewController (new ListScreenController (SetEditItemValue, data, ItemName), true);
		}

		private Tire GetTire(int positionId)
		{
			string appendLabel = GetPositionName (positionId);
			string brand = GetEditItemValue (appendLabel + " Brand");
			string size = GetEditItemValue (appendLabel + " Size");
			string tread = GetEditItemValue (appendLabel + " Depth");

			Tire tire = new Tire();
			tire.VehicleID = vehicleid;
			tire.PositionID = positionId;
			if (!string.IsNullOrWhiteSpace(brand))
				tire.Brand = brand;
			if (!string.IsNullOrWhiteSpace(size))
				tire.Size = size;
			if (!string.IsNullOrWhiteSpace(tread) && tread != "required" && tread.Replace("/32", "").ToInt() > 0)
				tire.Tread = tread.Replace("/32", "").ToInt();

			return tire;
		}

		private string GetPositionName(int positionId)
		{
			if (positionId == 1)
				return "LF";
			if (positionId == 2)
				return "LR";
			if (positionId == 3)
				return "RF";
			if (positionId == 4)
				return "RR";
			if (positionId == 5)
				return "SP";

			return "";
		}

		private void Validate()
		{
			List<Tire> listOfTires = new List<Tire>();
			for (int i = 1; i <= 5; i++) {
				Tire tire = GetTire(i);
				if (tire == null)
					continue;

				if (!string.IsNullOrWhiteSpace(tire.Brand) || !string.IsNullOrWhiteSpace(tire.Size) || tire.Tread > 0) {
					//something is entered - validate
					if (string.IsNullOrWhiteSpace(tire.Brand) || string.IsNullOrWhiteSpace(tire.Size) || tire.Tread <= 0) {
						Controls.OkDialog("Validation Error", "One of the required fields is not entered for the " + GetPositionName(i) + " tire.", delegate { return; });
						return;
					}
					//tire is good, add to list
					listOfTires.Add(tire);
				}
			}

			if (listOfTires.Count > 0)
				SaveTires(listOfTires);            
		}

		private void SaveTires(List<Tire> listTires)
		{
			using (Connection sqlConn = new Connection(SQLiteBoostDB.GetDBPath())) {
				TireDB tdb = new TireDB(sqlConn);

				tdb.RemoveAllTires(vehicleid);
				foreach (Tire tire in listTires)
					tdb.AddTire(tire);
			}

			Controls.OkDialog("Success", "Tires Saved Successfully", Done);
		}

		private void setTire(Tire tire)
		{
			string appendLabel = GetPositionName (tire.PositionID);

			SetEditItemValue (appendLabel + " Brand", tire.Brand);
			SetEditItemValue (appendLabel + " Size", tire.Size);
			SetEditItemValue (appendLabel + " Depth", tire.Tread.ToString() + "/32");
		}

		private void setElementValues()
		{
			//load tires values
			using (Connection sqlConn = new Connection(SQLiteBoostDB.GetDBPath())) {
				TireDB tdb = new TireDB(sqlConn);

				List<Tire> list = tdb.GetTireList(vehicleid);
				if (list.Count <= 0)
					return;

				foreach (Tire tire in list) 
					setTire(tire);
			}
		}

		private string GetEditItemValue(string name)
		{
			EditItem item;
			if (dicEditItems.TryGetValue (name, out item) && item != null)
				return item.GetValue ().Replace("required","").Replace("optional", "");

			return string.Empty;
		}

		private void SetEditItemValue(string name, string value)
		{
			EditItem item;
			if (dicEditItems.TryGetValue (name, out item) && item != null)
				item.SetValue (value);

			if (name == "LF Depth") {
				SetEditItemValue (name.Replace ("LF", "LR"), value);
				SetEditItemValue (name.Replace ("LF", "RF"), value);
				SetEditItemValue (name.Replace ("LF", "RR"), value);
			}
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
			currentTextLabel = "";
			UIView firstResponder = GetFirstResponder (this.View);
			if (firstResponder != null) {
				if (firstResponder.Tag == 1) 
					currentTextLabel = "LF Brand";
				else if (firstResponder.Tag == 2)
					currentTextLabel = "LF Size";
			}
		}

		private void TextFieldEndEditing(NSNotification notification)
		{
			if (!string.IsNullOrWhiteSpace(currentTextLabel)) {
				string val = GetEditItemValue (currentTextLabel);
				SetEditItemValue (currentTextLabel.Replace ("LF", "LR"), val);
				SetEditItemValue (currentTextLabel.Replace ("LF", "RF"), val);
				SetEditItemValue (currentTextLabel.Replace ("LF", "RR"), val);
			}
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

