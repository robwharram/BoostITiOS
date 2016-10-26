using System;
using UIKit;

namespace BoostITiOS
{
	public class CameraHolder
	{
		public UIImage CameraImage { get; set; }
		public UIView LoadingView { get; set; }
		public string ErrorMsg { get; set; }
		public int FileNumber { get; set; }
		public string FilePath { get; set; }
	}
}

