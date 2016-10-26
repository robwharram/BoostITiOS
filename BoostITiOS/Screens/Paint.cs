
using System;

using Foundation;
using UIKit;
using System.Collections.Generic;
using System.Linq;
using SQLite;
using BoostIT.DataAccess;
using BoostIT;

namespace BoostITiOS
{
	public partial class Paint : UIViewController
	{
		private Dictionary<string, EditItem> dicEditItems;
		private bool keyboardShowing = false;
		private float keyboardHeight = 0f;
		private UIScrollView scrollView;
		private int vehicleid;

		public Paint (int VehicleID) : base ("Paint", null)
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

			scrollView = new UIScrollView (new CoreGraphics.CGRect(this.View.Bounds.X, 65f, this.View.Bounds.Width, this.View.Bounds.Height));
			scrollView.AddSubview (new EditGroup (GetEditViews ()).View);
			scrollView.ContentSize = new CoreGraphics.CGSize (scrollView.Bounds.Width, 420 + 60f + 10f);
			this.View.AddSubview (scrollView);

			btnSave.Clicked += (object sender, EventArgs e) => { SavePaint(); };
			btnCancel.Clicked += (object sender, EventArgs e) =>  {	Done();	};

			NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.DidShowNotification,KeyBoardUpNotification);
			NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.DidHideNotification,KeyBoardHideNotification);
			NSNotificationCenter.DefaultCenter.AddObserver (UITextField.TextDidBeginEditingNotification, TextFieldOnFocus);

			setElementValues ();

			var g = new UITapGestureRecognizer (() => View.EndEditing (true));
			g.CancelsTouchesInView = false;
			View.AddGestureRecognizer (g);
		}

		private void SavePaint()
		{
			using (Connection sqlConn = new Connection(SQLiteBoostDB.GetDBPath())) {
				PaintDB pdb = new PaintDB(sqlConn);

				string leftHood = GetEditItemValue ("Left Hood");
				if (!string.IsNullOrWhiteSpace(leftHood) && leftHood.ToDecimal() != 0)
					pdb.AddPaint(new BoostIT.Models.Paint() { VehicleID = vehicleid, PaintAreaID = 1, Depth = leftHood.ToDecimal() });
				string leftFender = GetEditItemValue ("Left Fender");
				if (!string.IsNullOrWhiteSpace(leftFender) && leftFender.ToDecimal() != 0)
					pdb.AddPaint(new BoostIT.Models.Paint() { VehicleID = vehicleid, PaintAreaID = 3, Depth = leftFender.ToDecimal() });
				string leftFrontDoor = GetEditItemValue ("Left Front Door");
				if (!string.IsNullOrWhiteSpace(leftFrontDoor) && leftFrontDoor.ToDecimal() != 0)
					pdb.AddPaint(new BoostIT.Models.Paint() { VehicleID = vehicleid, PaintAreaID = 5, Depth = leftFrontDoor.ToDecimal() });
				string leftQuarterPanel = GetEditItemValue ("Left 1/4 Panel");
				if (!string.IsNullOrWhiteSpace(leftQuarterPanel) && leftQuarterPanel.ToDecimal() != 0)
					pdb.AddPaint(new BoostIT.Models.Paint() { VehicleID = vehicleid, PaintAreaID = 11, Depth = leftQuarterPanel.ToDecimal() });
				string leftRearDoor = GetEditItemValue ("Left Rear Door");
				if (!string.IsNullOrWhiteSpace(leftRearDoor) && leftRearDoor.ToDecimal() != 0)
					pdb.AddPaint(new BoostIT.Models.Paint() { VehicleID = vehicleid, PaintAreaID = 7, Depth = leftRearDoor.ToDecimal() });
				string leftRoof = GetEditItemValue ("Left Roof");
				if (!string.IsNullOrWhiteSpace(leftRoof) && leftRoof.ToDecimal() != 0)
					pdb.AddPaint(new BoostIT.Models.Paint() { VehicleID = vehicleid, PaintAreaID = 9, Depth = leftRoof.ToDecimal() });
				string leftTrunk = GetEditItemValue ("Left Trunk");
				if (!string.IsNullOrWhiteSpace(leftTrunk) && leftTrunk.ToDecimal() != 0)
					pdb.AddPaint(new BoostIT.Models.Paint() { VehicleID = vehicleid, PaintAreaID = 13, Depth = leftTrunk.ToDecimal() });
				string rightFender = GetEditItemValue ("Right Fender");
				if (!string.IsNullOrWhiteSpace(rightFender) && rightFender.ToDecimal() != 0)
					pdb.AddPaint(new BoostIT.Models.Paint() { VehicleID = vehicleid, PaintAreaID = 4, Depth = rightFender.ToDecimal() });
				string rightFrontDoor = GetEditItemValue ("Right Front Door");
				if (!string.IsNullOrWhiteSpace(rightFrontDoor) && rightFrontDoor.ToDecimal() != 0)
					pdb.AddPaint(new BoostIT.Models.Paint() { VehicleID = vehicleid, PaintAreaID = 6, Depth = rightFrontDoor.ToDecimal() });
				string rightHood = GetEditItemValue ("Right Hood");
				if (!string.IsNullOrWhiteSpace(rightHood) && rightHood.ToDecimal() != 0)
					pdb.AddPaint(new BoostIT.Models.Paint() { VehicleID = vehicleid, PaintAreaID = 2, Depth = rightHood.ToDecimal() });
				string rightQuarterPanel = GetEditItemValue ("Right 1/4 Panel");
				if (!string.IsNullOrWhiteSpace(rightQuarterPanel) && rightQuarterPanel.ToDecimal() != 0)
					pdb.AddPaint(new BoostIT.Models.Paint() { VehicleID = vehicleid, PaintAreaID = 12, Depth = rightQuarterPanel.ToDecimal() });
				string rightRearDoor = GetEditItemValue ("Right Rear Door");
				if (!string.IsNullOrWhiteSpace(rightRearDoor) && rightRearDoor.ToDecimal() != 0)
					pdb.AddPaint(new BoostIT.Models.Paint() { VehicleID = vehicleid, PaintAreaID = 8, Depth = rightRearDoor.ToDecimal() });
				string rightRoof = GetEditItemValue ("Right Roof");
				if (!string.IsNullOrWhiteSpace(rightRoof) && rightRoof.ToDecimal() != 0)
					pdb.AddPaint(new BoostIT.Models.Paint() { VehicleID = vehicleid, PaintAreaID = 10, Depth = rightRoof.ToDecimal() });
				string rightTrunk = GetEditItemValue ("Right Trunk");
				if (!string.IsNullOrWhiteSpace(rightTrunk) && rightTrunk.ToDecimal() != 0)
					pdb.AddPaint(new BoostIT.Models.Paint() { VehicleID = vehicleid, PaintAreaID = 14, Depth = rightTrunk.ToDecimal() });
			}

			Controls.OkDialog("Success", "Paint Saved Successfully", Done);
		}

		private void Done()
		{
			DismissModalViewController (true);
		}

		private void setElementValues()
		{
			//load paint values
			using (Connection sqlConn = new Connection(SQLiteBoostDB.GetDBPath())) {
				PaintDB pdb = new PaintDB(sqlConn);
				List<BoostIT.Models.Paint> listPaint = pdb.GetPaintList(vehicleid);

				foreach (BoostIT.Models.Paint paint in listPaint)
				{
					if (paint.PaintAreaID == 3)
						SetEditItemValue ("Left Fender", paint.Depth.ToString ());
					if (paint.PaintAreaID == 5)
						SetEditItemValue("Left Front Door", paint.Depth.ToString());
					if (paint.PaintAreaID == 1)
						SetEditItemValue ("Left Hood", paint.Depth.ToString ());
					if (paint.PaintAreaID == 11)
						SetEditItemValue ("Left 1/4 Panel", paint.Depth.ToString ());
					if (paint.PaintAreaID == 7)
						SetEditItemValue ("Left Rear Door", paint.Depth.ToString ());
					if (paint.PaintAreaID == 9)
						SetEditItemValue ("Left Roof", paint.Depth.ToString ());
					if (paint.PaintAreaID == 13)
						SetEditItemValue ("Left Trunk", paint.Depth.ToString ());
					if (paint.PaintAreaID == 4)
						SetEditItemValue ("Right Fender", paint.Depth.ToString ());
					if (paint.PaintAreaID == 6)
						SetEditItemValue ("Right Front Door", paint.Depth.ToString ());
					if (paint.PaintAreaID == 2)
						SetEditItemValue ("Right Hood", paint.Depth.ToString ());
					if (paint.PaintAreaID == 12)
						SetEditItemValue ("Right 1/4 Panel", paint.Depth.ToString ());
					if (paint.PaintAreaID == 8)
						SetEditItemValue ("Right Rear Door", paint.Depth.ToString ());
					if (paint.PaintAreaID == 10)
						SetEditItemValue ("Right Roof", paint.Depth.ToString ());
					if (paint.PaintAreaID == 14)
						SetEditItemValue ("Right Trunk", paint.Depth.ToString ());
				}
			}

		}

		private List<UIView> GetEditViews()
		{
			List<UIView> listViews = new List<UIView> ();
			listViews.Add (GetEditText("Left Hood", 1, UIKeyboardType.NumbersAndPunctuation, UIReturnKeyType.Next));
			listViews.Add (GetEditText("Left Fender",2, UIKeyboardType.NumbersAndPunctuation, UIReturnKeyType.Next));
			listViews.Add (GetEditText("Left Front Door",3, UIKeyboardType.NumbersAndPunctuation, UIReturnKeyType.Next));
			listViews.Add (GetEditText("Left Rear Door",4, UIKeyboardType.NumbersAndPunctuation, UIReturnKeyType.Next));
			listViews.Add (GetEditText("Left Roof",5, UIKeyboardType.NumbersAndPunctuation, UIReturnKeyType.Next));
			listViews.Add (GetEditText("Left 1/4 Panel",6, UIKeyboardType.NumbersAndPunctuation, UIReturnKeyType.Next));
			listViews.Add (GetEditText("Left Trunk",7, UIKeyboardType.NumbersAndPunctuation, UIReturnKeyType.Next));
			listViews.Add (GetEditText("Right Trunk",8, UIKeyboardType.NumbersAndPunctuation, UIReturnKeyType.Next));
			listViews.Add (GetEditText("Right 1/4 Panel",9, UIKeyboardType.NumbersAndPunctuation, UIReturnKeyType.Next));
			listViews.Add (GetEditText("Right Roof",10, UIKeyboardType.NumbersAndPunctuation, UIReturnKeyType.Next));
			listViews.Add (GetEditText("Right Rear Door",11, UIKeyboardType.NumbersAndPunctuation, UIReturnKeyType.Next));
			listViews.Add (GetEditText("Right Front Door",12, UIKeyboardType.NumbersAndPunctuation, UIReturnKeyType.Next));
			listViews.Add (GetEditText("Right Fender",13, UIKeyboardType.NumbersAndPunctuation, UIReturnKeyType.Next));
			listViews.Add (GetEditText("Right Hood",14, UIKeyboardType.NumbersAndPunctuation, UIReturnKeyType.Done));

			return listViews;
		}

		private UIView GetEditText(string label, int textTag, UIKeyboardType keyboardType, UIReturnKeyType returnKeyType, string defaultValue="optional")
		{
			EditText et = new EditText(this, textTag, label, keyboardType, returnKeyType, defaultValue);
			dicEditItems.Add (label, et as EditItem);

			return et.View;
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

			scrollView.SetContentOffset (new CoreGraphics.CGPoint (0f, 0f), true);
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

