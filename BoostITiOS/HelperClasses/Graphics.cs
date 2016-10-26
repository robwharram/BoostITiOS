using System;
using System.IO;
using UIKit;
using CoreGraphics;
using System.Threading.Tasks;
using System.Net.Http;
using Foundation;
using System.Drawing;
using BoostIT.Models;
using SQLite;
using BoostIT.DataAccess;

namespace BoostITiOS
{
	public static class Graphics
	{
		public static string GetBaseImagesPath()
		{
			string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			string libraryPath = Path.Combine (documentsPath, "..", "Library");
			return Path.Combine(libraryPath, "Images");
		}

		public static string GetImagePath(int vehicleid)
		{
			string imagePath = Path.Combine(GetBaseImagesPath(), vehicleid.ToString());
			if (!Directory.Exists(imagePath))
				Directory.CreateDirectory(imagePath);

			return imagePath;
		}

		public static void SaveImage(int fileNumber, string filePath, int vehicleid)
		{
			Image image = new Image();
			image.VehicleID = vehicleid;
			image.FileNumber = fileNumber;
			image.FileName = filePath;

			using (Connection sqlConn = new Connection(SQLiteBoostDB.GetDBPath())) {
				VehicleImagesDB vidb = new VehicleImagesDB(sqlConn);
				vidb.RemoveImage(vehicleid, fileNumber);
				vidb.AddImage(image);
			}
		}

		public static UIView GetActivitySpinner(UIView viewToAddTo, CGRect frame)
		{
			UIView view = new UIView (frame);

			view.BackgroundColor = UIColor.Black;
			view.Alpha = 0.75f;
			view.AutoresizingMask = UIViewAutoresizing.FlexibleDimensions;

			// derive the center x and y
			nfloat centerX = frame.Width / 2;
			nfloat centerY = frame.Height / 2;

			// create the activity spinner, center it horizontall and put it 5 points above center x
			UIActivityIndicatorView activitySpinner = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.WhiteLarge);
			activitySpinner.Frame = new CGRect (
				centerX - (activitySpinner.Frame.Width / 2) ,
				centerY - (activitySpinner.Frame.Height /2) ,
				activitySpinner.Frame.Width ,
				activitySpinner.Frame.Height);

			activitySpinner.AutoresizingMask = UIViewAutoresizing.FlexibleMargins;
			view.AddSubview (activitySpinner);

			activitySpinner.StartAnimating ();

			viewToAddTo.AddSubview (view);

			return view;
		}

		public static void HideActivitySpinner(UIView view)
		{			
			UIView.Animate (
				0.5, // duration
				() => { view.Alpha = 0; },
				() => { view.RemoveFromSuperview(); }
			);

		}

		public static UIImage ResizeImage(UIImage source, float width, float height)
		{
			if (source == null)
			{
				return source;
			}

			nfloat imageWidth = source.Size.Width;
			nfloat imageHeight = source.Size.Height;

			nfloat newImageHeight = height;
			nfloat newImageWidth = width;

			if (width == -1 && height != -1)
			{
				newImageWidth = imageWidth / (imageHeight / newImageHeight);
			} else if (width != -1 && height == -1)
			{
				newImageHeight = imageHeight / (imageWidth / newImageWidth);
			}

			try {
				UIGraphics.BeginImageContext(new CGSize(newImageWidth, newImageHeight));
				source.Draw(new CGRect(0,0,newImageWidth, newImageHeight));
				return UIGraphics.GetImageFromCurrentImageContext();
			} finally 
			{
				UIGraphics.EndImageContext();
			}
		}

		public static void CleanUpTempImages()
		{
			string basePath = Graphics.GetBaseImagesPath();
			foreach (string dir in System.IO.Directory.GetDirectories(basePath)) {
				foreach (string dirInVehicleID in System.IO.Directory.GetDirectories(dir)) {
					if (dirInVehicleID.ToUpper().EndsWith("UPLOADS")) {
						foreach (string fileInUploads in System.IO.Directory.GetFiles(dirInVehicleID, "*.jpg")) {
							System.Console.WriteLine("deleting file: " + fileInUploads);
							File.Delete (fileInUploads);
						}
					}
				}
			}
		}

		public static ImageForUpload CopyAndResizeFile(ImageForUpload ifu, int dealershipID)
		{
			string uploadFileDir = System.IO.Path.Combine(GetImagePath(ifu.vehicleId), @"Uploads");
			string uploadFileName = Guid.NewGuid() + ".jpg";
			string uploadFilePath = Path.Combine (uploadFileDir, uploadFileName);
			if (!Directory.Exists (uploadFileDir))
				Directory.CreateDirectory (uploadFileDir);

			File.Copy (ifu.filePath, uploadFilePath, true);
			using (UIImage img = ResizeImage (UIImage.FromFile (uploadFilePath), 1600, 1200))
				img.AsJPEG ().Save (uploadFilePath, true);	

			GC.Collect ();

			if (File.Exists (uploadFilePath)) {
				if (ifu.damageId > 0)
					return new ImageForUpload() { vehicleId = ifu.vehicleId, dealershipId = dealershipID, filePath = uploadFilePath, fileSize = new FileInfo(uploadFilePath).Length, damageId = ifu.damageId };

				return new ImageForUpload() { vehicleId = ifu.vehicleId, dealershipId = dealershipID, fileNumber = ifu.fileNumber, filePath = uploadFilePath, fileSize = new FileInfo(uploadFilePath).Length, damageId = 0 };
			}

			return null;
		}

		public static UIImage FromUrl (string uri)
		{
			try {
				using (var url = new NSUrl(uri.Replace(" ", "%20")))
				using (var data = NSData.FromUrl (url))
					return UIImage.LoadFromData (data);
			}
			catch{
				return UIImage.FromBundle ("nophoto.png");
			}


		}

		public static async Task LoadUrl(this UIImageView imageView, string url)
		{	
			if (string.IsNullOrEmpty (url))
				return;
			var progress = new UIActivityIndicatorView (UIActivityIndicatorViewStyle.WhiteLarge)
			{
				Center = new CGPoint(imageView.Bounds.GetMidX(), imageView.Bounds.GetMidY()),
			};
			imageView.AddSubview (progress);


			var t = FileCache.Download (url);
			if (t.IsCompleted) {
				imageView.Image = UIImage.FromFile(t.Result);
				progress.RemoveFromSuperview ();
				return;
			}
			progress.StartAnimating ();
			var image = UIImage.FromFile(await t);

			UIView.Animate (.3, 
				() => imageView.Image = image,
				() => {
					progress.StopAnimating ();
					progress.RemoveFromSuperview ();
				});
		}
	}
}

