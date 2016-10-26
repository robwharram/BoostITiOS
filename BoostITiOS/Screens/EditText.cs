
using System;
using System.Drawing;

using Foundation;
using UIKit;

namespace BoostITiOS
{
	public partial class EditText : EditItem
	{
		private string label, defaultvalue;
		private UIKeyboardType keyboardtype;
		private UIReturnKeyType returnkeytype;
		private UIViewController controller;
		private int tag;

		public EditText (UIViewController viewController, int textTag, string labelText, UIKeyboardType keyboardType, UIReturnKeyType returnKeyType, string valueText = "optional") : base ("EditText")
		{
			label = labelText;
			defaultvalue = valueText;
			keyboardtype = keyboardType;
			returnkeytype = returnKeyType;
			controller = viewController;
			tag = textTag;
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
			
			lblTitle.Text = label;
			txtValue.Text = defaultvalue;
			txtValue.KeyboardType = keyboardtype;
			txtValue.ReturnKeyType = returnkeytype;
			if (tag > 0)
				txtValue.Tag = tag;
			txtValue.ShouldReturn += textFieldShouldReturn;
			this.View.Layer.CornerRadius = 7.5f;
			this.View.Layer.BorderColor = UIColor.White.CGColor;
			this.View.Layer.BorderWidth = 0.5f;

			txtValue.EditingDidBegin += HandleEditingDidBegin;
			txtValue.EditingDidEnd += HandleEditingDidEnd;
		}

		private bool textFieldShouldReturn (UITextField txt)
		{
			if (txt.Tag <= 0) {
				txt.ResignFirstResponder ();
				return false;
			}

			nint nextTag = txt.Tag + 1;
			UIResponder nextResponder = controller.View.ViewWithTag (nextTag);
			if (nextResponder != null)
				nextResponder.BecomeFirstResponder ();
			else
				txt.ResignFirstResponder ();

			return false;
		}

		void HandleEditingDidEnd (object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace (txtValue.Text))
				txtValue.Text = defaultvalue;
		}

		void HandleEditingDidBegin (object sender, EventArgs e)
		{
			if (txtValue.Text == defaultvalue)
				txtValue.Text = "";
		}

		public override string GetValue()
		{
			return txtValue.Text;
		}

		public override void SetValue (string value)
		{
			txtValue.Text = value;
		}
	}
}

