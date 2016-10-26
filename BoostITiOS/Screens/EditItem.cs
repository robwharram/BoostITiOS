using System;
using UIKit;

namespace BoostITiOS
{
	public abstract class EditItem : UIViewController
	{
		public EditItem (string className) : base (className, null) { }

		public abstract string GetValue();
		public abstract void SetValue(string value);
	}
}

