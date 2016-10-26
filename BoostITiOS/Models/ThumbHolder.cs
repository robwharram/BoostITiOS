using System;
using BoostIT.Models;
using UIKit;

namespace BoostITiOS
{
	public class ThumbHolder
	{        
		public UIButton btn { get; set; }
		public Image image { get; set; }
		public string filePath { get; set; }
		public int orientation { get; set; }
		public UIView LoadingView { get; set; }
		public UIImage thumbNail { get; set; }
		public UIImageView imageView { get; set; }
		//public Bitmap bm { get; set; }        
	}
}

