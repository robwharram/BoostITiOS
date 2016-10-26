
using System;

using Foundation;
using UIKit;
using System.Collections.Generic;
using BoostIT.DataAccess;
using SQLite;
using BoostIT.Models;
using System.Linq;
using BoostIT;
using System.IO;

namespace BoostITiOS
{
	public partial class DamageAdd : UIViewController
	{
		private Dictionary<string, EditItem> dicEditItems;
		private int vehicleid, categoryid, damageid;
		private List<DamageArea> listArea;
		private List<DamageType> listType;
		private List<DamageAreaType> listAreaType;
		private string imagePath="";
		private Damage damage;
		private bool initialLoad = true;

		public DamageAdd (int VehicleID, int CategoryID, int DamageID) : base ("DamageAdd", null)
		{
			vehicleid = VehicleID;
			categoryid = CategoryID;
			damageid = DamageID;
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

			damage = new Damage ();

			//load data from database
			loadData();

			imagePath = Graphics.GetImagePath (vehicleid);

			this.View.BackgroundColor = UIColor.FromPatternImage (UIImage.FromBundle("bg.jpg"));

			dicEditItems = new Dictionary<string, EditItem> ();
			UIView eg = new EditGroup (GetEditViews ()).View;
			//eg.Frame = new CoreGraphics.CGRect (eg.Bounds.X, eg.Bounds.Y + 65f, eg.Bounds.Width, eg.Bounds.Height);

			UIScrollView scrollView = new UIScrollView (new CoreGraphics.CGRect(this.View.Bounds.X, 65f, this.View.Bounds.Width, eg.Bounds.Height));
			scrollView.AddSubview (eg);
			scrollView.ContentSize = new CoreGraphics.CGSize (scrollView.Bounds.Width, scrollView.Bounds.Height);
			this.View.AddSubview (scrollView);

			btnCancel.Clicked += (object sender, EventArgs e) =>  {
				DismissModalViewController(true);
			};

			btnSaVE.Clicked += BtnSaVE_Clicked;

			btnImage.TouchUpInside += btnImage_TouchUpInside;

			if (damageid > 0)
				setElementValues ();

			var g = new UITapGestureRecognizer (() => View.EndEditing (true));
			g.CancelsTouchesInView = false;
			View.AddGestureRecognizer (g);
		}

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);

			if (initialLoad) {
				if (!string.IsNullOrWhiteSpace(damage.FileName))
					DisplayThumbnail();    
				initialLoad = false;
			}
		}

		public override void TouchesBegan (NSSet touches, UIEvent evt)
		{
			base.TouchesBegan (touches, evt);

			if (tvComments.IsFirstResponder)
				tvComments.ResignFirstResponder ();
		}

		private void setElementValues()
		{			
			SetEditItemValue ("Area", damage.Area);
			SetEditItemValue ("Type", damage.TypeName);
			SetEditItemValue ("Length-Inches", damage.Length.ToString());
			tvComments.Text = damage.Comments;			        
		}

		void BtnSaVE_Clicked (object sender, EventArgs e)
		{
			DamageArea retArea = listArea.FirstOrDefault(a => a.Area == GetEditItemValue("Area"));
			if (retArea != null) {
				damage.AreaID = retArea.ID;
				damage.AreaSideID = retArea.AreaSideID;
				damage.Area = retArea.Area;
			}

			DamageType retType = listType.FirstOrDefault(t => t.TypeName == GetEditItemValue("Type"));
			if (retType != null) {
				damage.TypeID = retType.ID;
				damage.TypeName = retType.TypeName;
			}

			damage.Length = 0;
			string len = GetEditItemValue ("Length-Inches");
			if (!string.IsNullOrWhiteSpace (len) && len.ToInt () > 0)
				damage.Length = len.ToInt ();
			
			damage.Comments = tvComments.Text;
			damage.CategoryID = categoryid;
			damage.VehicleID = vehicleid;

			Validate ();
		}

		private void DisplayThumbnail()
		{	
			if (string.IsNullOrWhiteSpace (damage.FileName)) {
				btnImage.SetBackgroundImage (UIImage.FromBundle ("nophoto.png"), UIControlState.Normal);
				return;
			}
			
			UIView loadingView = showLoadingOverlay ();

			Async.BackgroundProcess(bw_getThumb, bw_getThumbCompleted, new ThumbHolder() { LoadingView = loadingView });
		}

		private void bw_getThumb(object sender, System.ComponentModel.DoWorkEventArgs e)
		{
			if (e.Argument == null)
				return;

			ThumbHolder holder = (ThumbHolder)e.Argument;
			if (holder == null)
				return;

			string thumbPath = Path.Combine (imagePath, Path.GetFileNameWithoutExtension(damage.FileName) + "_thumb.jpg");
			if (File.Exists (thumbPath))
				holder.thumbNail = UIImage.FromFile (thumbPath);

			e.Result = holder;
		}

		private void bw_getThumbCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
		{
			if (e.Error != null) 
				return;

			ThumbHolder holder = (ThumbHolder)e.Result;

			Graphics.HideActivitySpinner (holder.LoadingView);

			if (holder.thumbNail == null) {
				btnImage.SetBackgroundImage (UIImage.FromBundle ("nophoto.png"), UIControlState.Normal);
				return;
			}

			btnImage.SetBackgroundImage (holder.thumbNail, UIControlState.Normal);

			holder = null;
		}

		void btnImage_TouchUpInside (object sender, EventArgs e)
		{
			if (!string.IsNullOrWhiteSpace (damage.FileName))
				SelectPhotoAction ();
			else
				TakeOrSelect ();
		}

		private void TakeOrSelect()
		{
			var actionSheet = new UIActionSheet("Select a source", null, "Cancel", "Take a Picture", "Open Camera Roll"){
				Style = UIActionSheetStyle.Default
			};
			actionSheet.Clicked += delegate (object sender, UIButtonEventArgs args) {
				if (args.ButtonIndex == 0) 
					TakePicture();
				else if (args.ButtonIndex == 1)
					SelectPicture();				
			};
			actionSheet.ShowInView (View);
		}

		private void SelectPhotoAction()
		{
			var actionSheet = new UIActionSheet("Select an Action", null, "Cancel", "Take a Picture", "Open Camera Roll", "Delete Image", "Enlarge Image") {
				Style = UIActionSheetStyle.Default
			};
			actionSheet.Clicked += delegate (object sender, UIButtonEventArgs args) {
				if (args.ButtonIndex == 0) 
					TakePicture();
				else if (args.ButtonIndex == 1)
					SelectPicture();				
				else if (args.ButtonIndex == 2)
					deleteImage();
				else if (args.ButtonIndex == 3)
					enlargeImage();					
			};
			actionSheet.ShowInView (View);
		}

		private UIView showLoadingOverlay()
		{
			return Graphics.GetActivitySpinner (this.View, btnImage.Frame);
		}

		public void CameraCallBack (NSDictionary dic)
		{
			Async.BackgroundProcess (bw_CameraSaveImage, bw_CameraSaveImageCompleted, new CameraHolder() { CameraImage = (UIImage)dic.Values[0], LoadingView = showLoadingOverlay () });
		}

		private void bw_CameraSaveImage(object sender, System.ComponentModel.DoWorkEventArgs e)
		{
			CameraHolder holder = (CameraHolder)e.Argument;

			string fileName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName ());
			string filePath = Path.Combine(imagePath, fileName + ".jpg");
			string thumbPath = Path.Combine(imagePath, fileName + "_thumb.jpg");

			NSError fileError, thumbError;

			try
			{
				if (!Directory.Exists (imagePath))
					Directory.CreateDirectory (imagePath);
				if (File.Exists (filePath))
					File.Delete (filePath);
				if (File.Exists (thumbPath))
					File.Delete (thumbPath);

				using (NSData jpegData = holder.CameraImage.AsJPEG ()) {
					jpegData.Save (filePath, NSDataWritingOptions.Atomic, out fileError);
					UIImage thumb = Graphics.ResizeImage (UIImage.LoadFromData (jpegData), 100f, 75f);
					thumb.AsJPEG ().Save (thumbPath, NSDataWritingOptions.Atomic, out thumbError);

					if (fileError!=null)
						throw new Exception("Error saving image. " + fileError.ToString());
					if (thumbError!=null)
						throw new Exception("Error saving thumbnail. " + thumbError.ToString());

					holder.FilePath = filePath;
				}
			}
			catch (Exception ex) {
				holder.ErrorMsg = ex.Message;
			}

			e.Result = holder;
		}

		private void bw_CameraSaveImageCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
		{
			CameraHolder holder = (CameraHolder)e.Result;

			Graphics.HideActivitySpinner (holder.LoadingView);

			if (!string.IsNullOrWhiteSpace (holder.ErrorMsg))
				Controls.OkDialog ("Error Saving Image", "There was an error saving the image from the camera. The error was: " + holder.ErrorMsg);

			damage.FileName = holder.FilePath;

			DisplayThumbnail();
		}

		private void TakePicture()
		{            
			Camera.TakePicture (this, CameraCallBack);
		}

		private void SelectPicture()
		{
			Camera.SelectPicture (this, CameraCallBack);
		}

		private void deleteImage()
		{
			damage.FileName = null;
			btnImage.SetBackgroundImage (UIImage.FromBundle ("nophoto.png"), UIControlState.Normal);
		}

		private void enlargeImage()
		{
			string filePath = damage.FileName;
			if (!File.Exists (filePath))
				return;

			UIImageViewClickable iv = new UIImageViewClickable ();
			iv.Frame = this.View.Frame;
			iv.OnClick += () => { 
				iv.RemoveFromSuperview(); 
				if (UIDevice.CurrentDevice.Orientation != UIDeviceOrientation.Portrait)
					UIDevice.CurrentDevice.SetValueForKey (NSObject.FromObject(UIInterfaceOrientation.Portrait), new NSString("orientation"));

				Controls.RestrictRotation(true);
			};
			iv.ContentMode = UIViewContentMode.ScaleAspectFit;
			iv.AutosizesSubviews = true;
			iv.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
			iv.Image = UIImage.FromFile (filePath);

			Controls.RestrictRotation (false);

			this.View.AddSubview (iv);
		}

		private void Validate()
		{
			if (damage.AreaID <= 0) {
				Controls.OkDialog("Required Field", "Area is a required field.");
				return;
			}
			if (damage.TypeID <= 0) {
				Controls.OkDialog("Required Field", "Type is a required field.");
				return;
			}		
			if (damage.Length > 60) {
				Controls.OkDialog("Invalid length", "Length cannot be greater than 60 inches");
				return;
			}

			SaveDamage();
		}

		private void SaveDamage()
		{			
			using (Connection sqlConn = new Connection(SQLiteBoostDB.GetDBPath())) {
				DamageDB ddb = new DamageDB(sqlConn);
				if (damageid > 0)
					ddb.UpdateDamage(damage);
				else
					ddb.AddDamage(damage);
			}

			Controls.OkDialog("Success", "Damage Saved Successfully", Done);
		}

		private void Done()
		{
			DismissModalViewController(true);
		}

		private void loadData()
		{
			using (Connection sqlConn = new Connection(SQLiteBoostDB.GetDBPath())) {
				VehicleAssetsDB vadb = new VehicleAssetsDB(sqlConn);
				listArea = vadb.GetDamageAreas(categoryid);
				listType = vadb.GetDamageTypes(categoryid);
				listAreaType = vadb.GetDamageAreaTypes();
			}

			if (damageid > 0) {
				using (Connection sqlConn = new Connection(SQLiteBoostDB.GetDBPath()))
					damage = new DamageDB(sqlConn).GetDamage(damageid);
			}
		}

		private List<UIView> GetEditViews()
		{
			List<UIView> listViews = new List<UIView> ();
			listViews.Add (GetEditList("Area", listArea.Select(a => a.Area).ToArray(), false));
			listViews.Add (GetEditList("Type", null, false));
			listViews.Add (GetEditTextLength());

			return listViews;
		}

		private UIView GetEditList(string name, string[] listData, bool isSearchable = false, string defaultValue = "required")
		{
			EditList el = new EditList (this.OpenListScreen, name, name, listData, isSearchable, defaultValue);
			dicEditItems.Add (name, el as EditItem);

			return el.View;
		}

		private UIView GetEditTextLength()
		{
			EditText et = new EditText (this, 0, "Length-Inches", UIKeyboardType.NumbersAndPunctuation, UIReturnKeyType.Next);
			dicEditItems.Add ("Length-Inches", et as EditText);

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

		private void OpenListScreen(string ItemName, string[] data, bool isSearchable)
		{
			if (ItemName == "Type") {
				string selectedArea = GetEditItemValue("Area");
				DamageArea area = listArea.FirstOrDefault(a => a.Area == selectedArea);
				if (string.IsNullOrWhiteSpace(selectedArea) || area == null) {
					Controls.OkDialog("Area is Required", "You must select a damage area first");
					return;
				}

				List<int> listTypeIDs = listAreaType.Where (t => t.AreaID == area.ID).Select (t => t.TypeID).ToList();
				data = listType.Where (t => listTypeIDs.Contains (t.ID)).Select (t => t.TypeName).ToArray ();
			}

			PresentModalViewController (new ListScreenController (SetEditItemValue, data, ItemName), true);
		}
	}
}