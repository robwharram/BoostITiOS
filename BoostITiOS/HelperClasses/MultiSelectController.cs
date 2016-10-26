using System;
using System.Collections.Generic;
using System.Drawing;
using Foundation;
using System.Threading.Tasks;
using UIKit;
using AssetsLibrary;
using SQLite;
using BoostIT.DataAccess;
using System.IO;
using BoostIT.Models;
using System.Linq;

namespace BoostITiOS
{
	public class AssetResult
	{
		public UIImage Image { get; set; }
	}


	/** 
     * Presents a photo picker dialog capable of selecting multiple images at once.
     * Usage:
     * 
     * var picker = MultiSelectController.Instance;
     * picker.MaximumImagesCount = 15;
     * picker.Completion.ContinueWith (t => {
     *   if (t.IsCancelled || t.Exception != null) {
     *     // no pictures for you!
	*   } else {
	*      // t.Result is a List<AssetResult>
	*    }
* });
* 
* PresentViewController (picker, true, null);
*/
public class MultiSelectController : UINavigationController
{
	public int MaximumImagesCount { get; set; }
	public int vehicleId { get; set; }
	private LoadingOverlay loadingOverlay;

	readonly TaskCompletionSource<List<AssetResult>> _TaskCompletionSource = new TaskCompletionSource<List<AssetResult>> ();
	public Task<List<AssetResult>> Completion {
		get {
			return _TaskCompletionSource.Task;
		}
	}

	public static MultiSelectController Instance {
		get {
			var albumPicker = new ELCAlbumPickerController ();
			var picker = new MultiSelectController (albumPicker);
			albumPicker.Parent = picker;
			albumPicker.vehicleId = picker.vehicleId;
			picker.MaximumImagesCount = 4;
			picker.NavigationBar.BarStyle = UIBarStyle.Black;
			return picker;
		}
	}

	MultiSelectController (UIViewController rootController) : base (rootController)
	{

	}

	public override void DidReceiveMemoryWarning ()
	{
		// Releases the view if it doesn't have a superview.
		base.DidReceiveMemoryWarning ();

		// Release any cached data, images, etc that aren't in use.
	}

	void SelectedAssets (List<ALAsset> assets)
	{
		//var results = new List<AssetResult> (assets.Count);

		loadingOverlay = Controls.ProgressDialog(this,"Loading Images, Please Wait");
		Async.BackgroundProcess(bw_SaveMulti,bw_SaveMultiCompleted, assets);
	}

	private void bw_SaveMulti(object sender, System.ComponentModel.DoWorkEventArgs e)
	{
		List<ALAsset> results = (List<ALAsset>)e.Argument;
		List<string> errors = new List<string> ();

		using (Connection sqlConn = new Connection(SQLiteBoostDB.GetDBPath())) {
			VehicleImagesDB vidb = new VehicleImagesDB(sqlConn);
			vidb.RemoveAllImages (this.vehicleId);
		}

		var imagePath = Graphics.GetImagePath (vehicleId);

		int counter = 0;

		foreach (var asset in results) {
			var obj = asset.AssetType;
			if (obj == default (ALAssetType))
				continue;

			using (var rep = asset.DefaultRepresentation) {
				if (rep != null) {
					//var result = new AssetResult ();
					using (CoreGraphics.CGImage cgImage = rep.GetFullScreenImage ()) {
						UIImageOrientation orientation = UIImageOrientation.Up;
						using (UIImage uiImage = new UIImage (cgImage, 1.0f, orientation)) {
							counter ++;
							string filePath = Path.Combine(imagePath,"image_" + counter + ".jpg");
							string thumbPath = Path.Combine(imagePath,"thumb_" + counter + ".jpg");
							NSError fileError, thumbError;

							try {
								if (!Directory.Exists (imagePath))
									Directory.CreateDirectory (imagePath);
								if (File.Exists (filePath))
									File.Delete (filePath);
								if (File.Exists (thumbPath))
									File.Delete (thumbPath);

								using (NSData jpegData = uiImage.AsJPEG ()) {
									jpegData.Save (filePath, NSDataWritingOptions.Atomic, out fileError);
									using (UIImage thumb = Graphics.ResizeImage (UIImage.LoadFromData (jpegData), 100f, 75f))
										thumb.AsJPEG ().Save (thumbPath, NSDataWritingOptions.Atomic, out thumbError);

									if (fileError!=null)
										throw new Exception("Error saving image. " + fileError.ToString());
									if (thumbError!=null)
										throw new Exception("Error saving thumbnail. " + thumbError.ToString());

									Graphics.SaveImage(counter, filePath, vehicleId);
								}
							}
							catch (Exception ex) {
								errors.Add (ex.ToString ());
							}
						}
					}
				}
			}
		}

		results = null;

		e.Result = errors;
	}

	private void bw_SaveMultiCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
	{
		GC.Collect ();

		List<string> errors = (List<string>)e.Result;

		loadingOverlay.Hide();

		if (errors != null && errors.Count >0) {
			string message = "";
			if (errors.Count == 1)
				message = "There was 1 error while loading images.  The error was: " + errors [0];
			else
				message = "There were " + errors.Count + " errors while loading images.  The first error was: " + errors [0];

			Controls.OkDialog ("Error", message, null);
		}

		_TaskCompletionSource.TrySetResult (null);
		DismissModalViewController(true);
	}

	void CancelledPicker ()
	{
		_TaskCompletionSource.TrySetCanceled ();
		DismissModalViewController(true);
	}

	bool ShouldSelectAsset (ALAsset asset, int previousCount)
	{
		var shouldSelect = MaximumImagesCount <= 0 || previousCount < MaximumImagesCount;
		if (!shouldSelect) {
			string title = String.Format ("Only {0} photos please!", MaximumImagesCount);
			string message = String.Format ("You can only send {0} photos at a time.", MaximumImagesCount);
			var alert = new UIAlertView (title, message, null, null, "Ok");
			alert.Show ();
		}
		return shouldSelect;
	}

	public class ELCAlbumPickerController : UITableViewController
	{
		static readonly NSObject _Dispatcher = new NSObject();
		readonly List<ALAssetsGroup> AssetGroups = new List<ALAssetsGroup> ();

		ALAssetsLibrary Library;

		WeakReference _Parent;
		public MultiSelectController Parent {
			get {
				return _Parent == null ? null : _Parent.Target as MultiSelectController;
			}
			set {
				_Parent = new WeakReference (value);
			}
		}

		public int vehicleId { get; set; }

		public ELCAlbumPickerController ()
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			NavigationItem.Title = "Loading...";
			var cancelButton = new UIBarButtonItem (UIBarButtonSystemItem.Cancel/*, cancelImagePicker*/);
			cancelButton.Clicked += CancelClicked;
			NavigationItem.RightBarButtonItem = cancelButton;

			AssetGroups.Clear ();

			Library = new ALAssetsLibrary ();
			Library.Enumerate (ALAssetsGroupType.All, GroupsEnumerator, GroupsEnumeratorFailed);
		}

		public override void ViewDidDisappear (bool animated)
		{
			base.ViewDidDisappear (animated);
			if (IsMovingFromParentViewController || IsBeingDismissed) {
				NavigationItem.RightBarButtonItem.Clicked -= CancelClicked;
			}
		}

		void CancelClicked (object sender = null, EventArgs e = null)
		{
			var parent = Parent;
			if (parent != null) {
				parent.CancelledPicker ();
			}
		}

		void GroupsEnumeratorFailed (Foundation.NSError error)
		{
			Console.WriteLine ("Enumerator failed!");
		}

		void GroupsEnumerator (ALAssetsGroup agroup, ref bool stop)
		{
			if (agroup == null) {
				return;
			}

			// added fix for camera albums order
			if (agroup.Name.ToString ().ToLower () == "camera roll" && agroup.Type == ALAssetsGroupType.SavedPhotos) {
				AssetGroups.Insert (0, agroup);
			} else {
				AssetGroups.Add (agroup);
			}

			_Dispatcher.BeginInvokeOnMainThread (ReloadTableView);
		}

		void ReloadTableView ()
		{
			TableView.ReloadData ();
			NavigationItem.Title = "Select an Album";
		}

		public override nint NumberOfSections (UITableView tableView)
		{
			return 1;
		}

		public override nint RowsInSection (UITableView tableview, nint section)
		{
			return AssetGroups.Count;
		}

		public override UITableViewCell GetCell (UITableView tableView, Foundation.NSIndexPath indexPath)
		{
			const string cellIdentifier = "Cell";

			var cell = tableView.DequeueReusableCell (cellIdentifier);
			if (cell == null) {
				cell = new UITableViewCell (UITableViewCellStyle.Default, cellIdentifier);
			}

			// Get count
			var g = AssetGroups [indexPath.Row];
			g.SetAssetsFilter (ALAssetsFilter.AllPhotos);
			var gCount = g.Count;
			cell.TextLabel.Text = string.Format ("{0} ({1})", g.Name, gCount);
			try {
				cell.ImageView.Image = new UIImage (g.PosterImage);
			} catch (Exception e) {
				Console.WriteLine ("Failed to set thumbnail {0}", e);
			}
			cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;

			return cell;
		}

		public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
		{
			var assetGroup = AssetGroups [indexPath.Row];
			assetGroup.SetAssetsFilter (ALAssetsFilter.AllPhotos);
			var picker = new ELCAssetTablePicker (assetGroup);
			picker.Parent = Parent;
			NavigationController.PushViewController (picker, true);
		}

		public override nfloat GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
		{
			return 57;
		}
	}


	class ELCAssetTablePicker : UITableViewController
	{
		static readonly NSObject _Dispatcher = new NSObject();

		int Columns = 4;
		public bool SingleSelection { get; set; }
		public bool ImmediateReturn { get; set; }
		readonly ALAssetsGroup AssetGroup;

		readonly List<ELCAsset> ElcAssets = new List<ELCAsset> ();

		WeakReference _Parent;
		public MultiSelectController Parent {
			get {
				return _Parent == null ? null : _Parent.Target as MultiSelectController;
			}
			set {
				_Parent = new WeakReference (value);
			}
		}

		public ELCAssetTablePicker (ALAssetsGroup assetGroup)
		{
			AssetGroup = assetGroup;
		}

		public override void ViewDidLoad ()
		{
			TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
			TableView.AllowsSelection = false;

			if (ImmediateReturn) {

			} else {
				var doneButtonItem = new UIBarButtonItem (UIBarButtonSystemItem.Done);
				doneButtonItem.Clicked += DoneClicked;
				NavigationItem.RightBarButtonItem = doneButtonItem;
				NavigationItem.Title = "Loading...";
			}

			Task.Run ((Action)PreparePhotos);
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			Columns = (int)(View.Bounds.Size.Width / 80f);
		}

		public override void ViewDidDisappear (bool animated)
		{
			base.ViewDidDisappear (animated);
			if (IsMovingFromParentViewController || IsBeingDismissed) {
				NavigationItem.RightBarButtonItem.Clicked -= DoneClicked;
			}
		}

		public override void DidRotate (UIInterfaceOrientation fromInterfaceOrientation)
		{
			base.DidRotate (fromInterfaceOrientation);
			Columns = (int)(View.Bounds.Size.Width / 80f);
			TableView.ReloadData ();
		}

		void PreparePhotos ()
		{
			AssetGroup.Enumerate (PhotoEnumerator);

			_Dispatcher.BeginInvokeOnMainThread (() => {
				TableView.ReloadData ();
				// scroll to bottom
				nint section = NumberOfSections (TableView) - 1;
				nint row = TableView.NumberOfRowsInSection (section) - 1;
				if (section >= 0 && row >= 0) {
					var ip = NSIndexPath.FromRowSection (row, section);
					TableView.ScrollToRow (ip, UITableViewScrollPosition.Bottom, false);
				}
				NavigationItem.Title = SingleSelection ? "Pick Photo" : "Pick Photos";
			});
		}

		void PhotoEnumerator (ALAsset result, nint index, ref bool stop)
		{
			if (result == null) {
				return;
			}

			ELCAsset elcAsset = new ELCAsset (this, result);

			bool isAssetFiltered = false;
			/*if (self.assetPickerFilterDelegate &&
                    [self.assetPickerFilterDelegate respondsToSelector:@selector(assetTablePicker:isAssetFilteredOut:)])
                {
                    isAssetFiltered = [self.assetPickerFilterDelegate assetTablePicker:self isAssetFilteredOut:(ELCAsset*)elcAsset];
                }*/

			if (result.DefaultRepresentation == null)
				isAssetFiltered = true;

			if (!isAssetFiltered) {
				ElcAssets.Add (elcAsset);
			}
		}

		void DoneClicked (object sender = null, EventArgs e = null)
		{
			var selected = new List<ALAsset> ();

			foreach (var asset in ElcAssets.Where(a => a.Selected).OrderBy(a => a.Order)) {
				selected.Add (asset.Asset);
			}

			var parent = Parent;
			if (parent != null) {
				parent.SelectedAssets (selected);
			}
		}

		bool ShouldSelectAsset (ELCAsset asset)
		{
			int selectionCount = TotalSelectedAssets;
			bool shouldSelect = true;

			var parent = Parent;
			if (parent != null) {
				shouldSelect = parent.ShouldSelectAsset (asset.Asset, selectionCount);
			}

			return shouldSelect;
		}

		void AssetSelected (ELCAsset asset, bool selected)
		{
			TotalSelectedAssets += (selected) ? 1 : -1;

			if (SingleSelection) {
				foreach (var elcAsset in ElcAssets) {
					if (asset != elcAsset) {
						elcAsset.Selected = false;
					}
				}
			}
			if (ImmediateReturn) {
				var parent = Parent;
				var obj = new List<ALAsset> (1);
				obj.Add (asset.Asset);
				parent.SelectedAssets (obj);
			}
		}

		public override nint NumberOfSections (UITableView tableView)
		{
			return 1;
		}

		public override nint RowsInSection (UITableView tableview, nint section)
		{
			if (Columns <= 0)
				return 4;
			int numRows = (int)Math.Ceiling ((float)ElcAssets.Count / Columns);
			return numRows;
		}

		List<ELCAsset> AssetsForIndexPath (NSIndexPath path)
		{
			int index = path.Row * Columns;
			int length = Math.Min (Columns, ElcAssets.Count - index);
			return ElcAssets.GetRange (index, length);
		}

		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			const string cellIdentifier = "Cell";

			var cell = TableView.DequeueReusableCell (cellIdentifier) as ELCAssetCell;
			if (cell == null) {
				cell = new ELCAssetCell (UITableViewCellStyle.Default, cellIdentifier);
			}
			cell.SetAssets (AssetsForIndexPath (indexPath), Columns);
			return cell;
		}

		public override nfloat GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
		{
			return 79;
		}

		public int TotalSelectedAssets;

		public class ELCAsset
		{
			public readonly ALAsset Asset;
			readonly WeakReference _Parent;

			bool _Selected;
			long _order;

			public ELCAsset (ELCAssetTablePicker parent, ALAsset asset)
			{
				_Parent = new WeakReference (parent);
				Asset = asset;
			}

			public void ToggleSelected ()
			{
				Selected = !Selected;
			}

			public bool Selected { 
				get { 
					return _Selected;
				}

				set {
					var parent = _Parent.Target as ELCAssetTablePicker;
					if (value && parent != null && !parent.ShouldSelectAsset (this)) {
						return;
					}

					if (value)
						_order = DateTime.Now.Ticks;

					_Selected = value;

					if (parent != null) {
						parent.AssetSelected (this, value);
					}
				}
			}

			public long Order {
				get { 
					return _order;
				}
			}
		}

		class ELCAssetCell : UITableViewCell
		{
			List<ELCAsset> RowAssets;
			int Columns;
			readonly List<UIImageView> ImageViewArray = new List<UIImageView> ();
			readonly List<UIImageView> OverlayViewArray = new List<UIImageView> ();

			public ELCAssetCell (UITableViewCellStyle style, string reuseIdentifier) : base (style, reuseIdentifier)
			{
				UITapGestureRecognizer tapRecognizer = new UITapGestureRecognizer (CellTapped);
				AddGestureRecognizer (tapRecognizer);

			}

			public void SetAssets (List<ELCAsset> assets, int columns)
			{
				RowAssets = assets;
				Columns = columns;

				foreach (var view in ImageViewArray) {
					view.RemoveFromSuperview ();
				}
				foreach (var view in OverlayViewArray) {
					view.RemoveFromSuperview ();
				}

				UIImage overlayImage = null;
				for (int i = 0; i < RowAssets.Count; i++) {
					var asset = RowAssets [i];

					if (i < ImageViewArray.Count) {
						var imageView = ImageViewArray [i];
						imageView.Image = new UIImage (asset.Asset.Thumbnail);
					} else {
						var imageView = new UIImageView (new UIImage (asset.Asset.Thumbnail));
						ImageViewArray.Add (imageView);
					}

					if (i < OverlayViewArray.Count) {
						var overlayView = OverlayViewArray [i];
						overlayView.Hidden = !asset.Selected;
					} else {
						if (overlayImage == null) {
							overlayImage = new UIImage ("Overlay.png");
						}
						var overlayView = new UIImageView (overlayImage);
						OverlayViewArray.Add (overlayView);
						overlayView.Hidden = !asset.Selected;
					}
				}
			}

			void CellTapped (UITapGestureRecognizer tapRecognizer)
			{
				CoreGraphics.CGPoint point = tapRecognizer.LocationInView (this);
				var totalWidth = Columns * 75 + (Columns - 1) * 4;
				var startX = (Bounds.Size.Width - totalWidth) / 2;

				var frame = new CoreGraphics.CGRect (startX, 2, 75, 75);
				for (int i = 0; i < RowAssets.Count; ++i) {
					if (frame.Contains (point)) {
						ELCAsset asset = RowAssets [i];
						asset.Selected = !asset.Selected;
						var overlayView = OverlayViewArray [i];
						overlayView.Hidden = !asset.Selected;
						break;
					}
					var x = frame.X + frame.Width + 4;
					frame = new CoreGraphics.CGRect (x, frame.Y, frame.Width, frame.Height);
				}
			}

			public override void LayoutSubviews ()
			{
				var totalWidth = Columns * 75 + (Columns - 1) * 4;
				var startX = (Bounds.Size.Width - totalWidth) / 2;

				var frame = new CoreGraphics.CGRect (startX, 2, 75, 75);

				int i = 0;
				foreach (var imageView in ImageViewArray) {
					imageView.Frame = frame;
					AddSubview (imageView);

					var overlayView = OverlayViewArray [i++];
					overlayView.Frame = frame;
					AddSubview (overlayView);

					var x = frame.X + frame.Width + 4;
					frame = new CoreGraphics.CGRect (x, frame.Y, frame.Width, frame.Height);
				}
			}
		}
	}
}
}