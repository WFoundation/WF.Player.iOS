///
/// WF.Player.iPhone - A Wherigo Player for iPhone which use the Wherigo Foundation Core.
/// Copyright (C) 2012-2014  Dirk Weltz <mail@wfplayer.com>
///
/// This program is free software: you can redistribute it and/or modify
/// it under the terms of the GNU Lesser General Public License as
/// published by the Free Software Foundation, either version 3 of the
/// License, or (at your option) any later version.
/// 
/// This program is distributed in the hope that it will be useful,
/// but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
/// GNU Lesser General Public License for more details.
/// 
/// You should have received a copy of the GNU Lesser General Public License
/// along with this program.  If not, see <http://www.gnu.org/licenses/>.
/// 

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using MonoTouch.UIKit;
using Vernacular;
using WF.Player.Core;
using WF.Player.Core.Live;

namespace WF.Player.iOS
{

	#region ItemScreen
	
	[CLSCompliantAttribute(false)]
	public class CartridgeList : UIViewController
	{
		CartridgeListSource cartListSource;
		AppDelegate appDelegate;

		public UITableView Table;
		public CartridgeDetail cartDetail;

		
		public CartridgeList (AppDelegate app) : base()
		{
			this.appDelegate = app;

			// OS specific details
			if (new Version (UIDevice.CurrentDevice.SystemVersion) >= new Version(7,0)) 
			{
				// Code that uses features from Xamarin.iOS 7.0
				this.EdgesForExtendedLayout = UIRectEdge.None;
			}

			// Create source for table view
			cartListSource = new CartridgeListSource();
			cartListSource.OnSelect = OnSelect;

			// Create table view
			Table = new UITableView()
			{
				Source = cartListSource,
				AutoresizingMask = UIViewAutoresizing.All,
				AutosizesSubviews = true
			};
		}
		
		#region MonoTouch Functions
		
		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			// Perform any additional setup after loading the view, typically from a nib.
			this.Title = Catalog.GetString("Cartridges");

			// Set the table view to fit the width of the app.
			Table.SizeToFit();
			// Reposition and resize the receiver
			Table.Frame = new RectangleF (0, 0, this.View.Frame.Width,this.View.Frame.Height);
			// Add the table view as a subview
			this.View.AddSubviews(this.Table);
			this.View.AutoresizingMask = UIViewAutoresizing.All;
		}
		
		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear(animated);
			this.NavigationController.SetNavigationBarHidden(false,false);
			((CartridgeListSource)Table.Source).GetCartridgeList ();
			Table.ReloadData();
		}
		
		public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation) 
		{
			base.DidRotate(fromInterfaceOrientation);
			Table.ReloadData();
		}
		
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			// Return true for supported orientations
			return true;
		}

		public void OnSelect(Cartridge cart)
		{
			if (cartDetail == null)
				cartDetail = new CartridgeDetail(appDelegate);
			NavigationController.PushViewController (cartDetail,true);
			cartDetail.Cartridge = cart;
		}

		#endregion
	}
	
	#endregion
	
	#region CartridgeListSource
	
	public class CartridgeListSource : UITableViewSource 
	{ 
		private Cartridges cartList;
		
		public delegate void OnSelectEvent(Cartridge cartList);
		
		public OnSelectEvent OnSelect;
		
		public CartridgeListSource() 
		{  
			cartList = new Cartridges();
			GetCartridgeList();
		}  
		
		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{ 
			// Reuse a cell if one exists 
			CartridgeListCell cell = tableView.DequeueReusableCell ("CartridgeListCell") as CartridgeListCell;
			
			if (cell == null) 
			{  
				// We have to allocate a cell 
				cell = new CartridgeListCell(); 
			}
			
			// Set initial data 
			if (cartList != null && cartList.Count > indexPath.Row) 
			{
				cell.UpdateData (cartList [indexPath.Row]);
			}
			
			return cell; 
		}  
		
		public override int RowsInSection (UITableView tableview, int section) 
		{ 
			return cartList.Count; 
		} 
		
		public override float GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
		{
			// Insert needed height value
			return 104.0f;
		}
		
		public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
		{
			Cartridge activeCart = cartList [indexPath.Row];
			
			if (OnSelect != null)
				OnSelect(activeCart);
		}
		
		public void GetCartridgeList ()
		{
			string documentsPath = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
			FileInfo[] cartridgeFiles = new DirectoryInfo (documentsPath).GetFiles ("*.gwc");

			List<string> fileList = new List<string> (); 

			foreach (FileInfo fileInfo in cartridgeFiles) 
			{
				fileList.Add (fileInfo.FullName);
			}

			cartList.GetByFileList(fileList);
			
//			foreach (FileInfo fileInfo in cartridgeFiles) 
//			{
//				cartList.Add (new Cartridge(fileInfo.FullName));
//				cartList[cartList.Count-1]..PreLoadGWC(new FileStream(fileInfo.FullName,FileMode.Open));
//			}
		}
	} 

	#endregion
	
	#region CartridgeListCell
	
	public partial class CartridgeListCell : UITableViewCell
	{
		float directWidth = 0;
		UILabel textTitle;
		UILabel textDetail;
		UILabel textVersion;
		UILabel textAuthor;
		UIImageView imagePoster;

		public CartridgeListCell () : base ()
		{
			this.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
			CreateCell ();
		}
		
		public CartridgeListCell (IntPtr handle) : base (handle)
		{
			this.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
			CreateCell ();
		}
		
		void CreateCell ()
		{
			float maxWidth = this.Bounds.Width - 2 * Values.Frame;

			imagePoster = new UIImageView()
			{
				Frame = new Rectangle(10,10,48,64),
				ContentMode = UIViewContentMode.ScaleAspectFit
			};
			
			textTitle = new UILabel()
			{
				Frame = new RectangleF(72,10,maxWidth - directWidth - 72,120),
				Font = UIFont.BoldSystemFontOfSize(1.3f * UIFont.SystemFontSize),
				Lines = 2
			};
			
			textVersion = new UILabel()
			{
				Frame = new RectangleF(72,55,maxWidth - directWidth - 72,40),
				Font = UIFont.SystemFontOfSize(UIFont.SystemFontSize),
				TextAlignment = UITextAlignment.Left,
				Lines = 1,
				LineBreakMode = UILineBreakMode.TailTruncation | UILineBreakMode.WordWrap
			};

			textAuthor = new UILabel()
			{
				Frame = new RectangleF(72,67,maxWidth - directWidth - 72,40),
				Font = UIFont.SystemFontOfSize(UIFont.SystemFontSize),
				TextAlignment = UITextAlignment.Left,
				Lines = 1,
				LineBreakMode = UILineBreakMode.TailTruncation | UILineBreakMode.WordWrap
			};

			this.AddSubview (imagePoster);
			this.AddSubview (textTitle);
			this.AddSubview (textVersion);
			this.AddSubview (textAuthor);
		}

		public void UpdateData (Cartridge cart)
		{
			float maxWidth = this.Bounds.Width - 2 * Values.Frame;
			float maxHeight = 104.0f;
			float height = Values.Frame;

			textTitle.Text = cart.Name;
			if (!String.IsNullOrEmpty (cart.Version))
				// GETTEXT: Version for cartridge list in one line
				textVersion.Text += Catalog.Format(Catalog.GetString("Version {0}"), cart.Version);
			else
				textVersion.Text = "";
			if (!String.IsNullOrEmpty(cart.AuthorName) || !String.IsNullOrEmpty(cart.AuthorCompany))
				textAuthor.Text = Catalog.Format(Catalog.GetString("By {0}"), (String.IsNullOrEmpty(cart.AuthorName) ? "" : cart.AuthorName) + (!String.IsNullOrEmpty(cart.AuthorName) && !String.IsNullOrEmpty(cart.AuthorCompany) ? " / " : "") + (String.IsNullOrEmpty(cart.AuthorCompany) ? "" : cart.AuthorCompany));
			else
				textAuthor.Text = "";
			// TODO: Load default image for cart.ActivityType
			if (cart.Icon != null)
				this.imagePoster.Image = UIImage.LoadFromData (NSData.FromArray (cart.Icon.Data));
			else if (cart.Poster != null)
				this.imagePoster.Image = UIImage.LoadFromData (NSData.FromArray (cart.Poster.Data));
			else
				this.imagePoster.Image = null;

			// Do this, because of wrong values after rotating
			textTitle.Frame = new RectangleF (72, Values.Frame, maxWidth - directWidth - 72, 999999);
			textTitle.SizeToFit ();
			textTitle.Frame = new RectangleF (72, height, maxWidth - directWidth - 72, textTitle.Bounds.Height);

			textAuthor.Frame = new RectangleF (72, Values.Frame, maxWidth - directWidth - 72, 999999);
			textAuthor.SizeToFit ();
			textAuthor.Frame = new RectangleF (72, maxHeight - textAuthor.Bounds.Height - Values.Frame, maxWidth - directWidth - 72, textAuthor.Bounds.Height);

			height += textAuthor.Frame.Location.Y - Values.Frame;

			textVersion.Frame = new RectangleF (72, Values.Frame, maxWidth - directWidth - 72, 999999);
			textVersion.SizeToFit ();
			textVersion.Frame = new RectangleF (72, height - textVersion.Bounds.Height, maxWidth - directWidth - 72, textVersion.Bounds.Height);
		}

	}
	
	#endregion
	
}

