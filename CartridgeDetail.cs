///
/// WF.Player.iPhone - A Wherigo Player for iPhone which use the Wherigo Foundation Core.
/// Copyright (C) 2012-2013  Dirk Weltz <web@weltz-online.de>
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
using System.Globalization;
using System.IO;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.MapKit;
using MonoTouch.CoreLocation;
using MonoTouch.CoreGraphics;
using WF.Player.Core;

namespace WF.Player.iPhone
{
	[CLSCompliantAttribute(false)]
	public partial class CartridgeDetail : UIViewController
	{
		UILabel textHeader;
		UILabel textVersion;
		UILabel textAuthor;
		UIImageView imagePoster;
		UILabel textDescription;
		AppDelegate appDelegate;
		Cartridge cart;
		UIScrollView page1;
		UIScrollView page2;
		UITableView page3;

		public CartridgeDetail (AppDelegate appDelegate) : base ("CartridgeDetail", null)
		{
			this.appDelegate = appDelegate;

			// OS specific details
			if (new Version (UIDevice.CurrentDevice.SystemVersion) >= new Version(7,0)) 
			{
				// Code that uses features from Xamarin.iOS 7.0
				this.EdgesForExtendedLayout = UIRectEdge.None;
			}
		}

		[CLSCompliantAttribute(false)]
		public Cartridge Cartridge { 
			get { 
				return cart; 
			} 
			set { 
				if (cart != value) {
					cart = value;
					if (PagesView != null)
						Refresh();
				}
			}
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
			
			// Perform any additional setup after loading the view, typically from a nib.
			PagesController.CurrentPage = 0;
			PagesController.Pages = 3;
			PagesController.ValueChanged += delegate(object sender, EventArgs e) {
				var pageWidth = PagesView.Bounds.Width;
				PagesView.ContentOffset = new PointF(pageWidth * PagesController.CurrentPage, 0.0f);
			};

			// Resize ScrollView
			PagesView.ContentSize = new SizeF(PagesView.Bounds.Width * PagesController.Pages, PagesView.Bounds.Height - 6f);

			// Create views
			CreateViews();

			if (cart != null)
				Refresh();
		}

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear(animated);

			if (cart != null)
				Refresh();
		}

		void HandleScrolled (object sender, EventArgs e)
		{
			var pageWidth = PagesView.Bounds.Width;
			int page = Convert.ToInt16(Math.Floor((PagesView.ContentOffset.X - pageWidth / 2) / pageWidth) + 1);
			PagesController.CurrentPage = page;
		}
		
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			// Return true for supported orientations
			return (toInterfaceOrientation != UIInterfaceOrientation.PortraitUpsideDown);
		}

		public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation) 
		{
			base.DidRotate(fromInterfaceOrientation);

			Refresh ();
		}

		partial void BarItemStartPressed (MonoTouch.Foundation.NSObject sender)
		{
			appDelegate.CartStart(cart);
		}

		partial void BarItemRestorePressed (MonoTouch.Foundation.NSObject sender)
		{
			appDelegate.CartRestore(cart);
		}
		
		partial void BarItemStorePressed (MonoTouch.Foundation.NSObject sender)
		{
			//			appDelegate.CartStart(cart);
		}

		#region Private Functions

		void CreateViews()
		{
			PagesView.Scrolled += HandleScrolled;
			// PagesView.Frame = new RectangleF(0.0f, 64f, PagesView.Bounds.Width, PagesController.Frame.Top - this.NavigationController.NavigationBar.Frame.Height - 22);
			// PagesView.ContentSize = new SizeF(PagesView.Frame.Width, PagesView.Frame.Height);
			// PagesView.BackgroundColor = UIColor.Green;

			var width = PagesView.Bounds.Width;
			var height = PagesView.Bounds.Height;

			// Create pages

			// Poster page
			page1 = new UIScrollView (PagesView.Frame) {
				Frame = new RectangleF (0.0f, 0.0f, width, height),
				BackgroundColor = UIColor.Clear
			};
			textHeader = new UILabel() {
				Text = "Header",
				Font = UIFont.BoldSystemFontOfSize(2.0f * UIFont.SystemFontSize),
				Lines = 0,
				LineBreakMode = UILineBreakMode.WordWrap,
				TextAlignment = UITextAlignment.Center,
				AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight,
				ContentMode = UIViewContentMode.Top,
				BackgroundColor = UIColor.Clear,
				Frame = new RectangleF (Values.Frame, Values.Frame, width - 2 * Values.Frame, height)
			};
			imagePoster = new UIImageView() {
				ContentMode = UIViewContentMode.Center | UIViewContentMode.ScaleAspectFit,
				BackgroundColor = UIColor.Clear
			};
			textVersion = new UILabel() {
				Text = "Version",
				Lines = 0,
				LineBreakMode = UILineBreakMode.WordWrap,
				TextAlignment = UITextAlignment.Center,
				AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight,
				ContentMode = UIViewContentMode.Top,
				BackgroundColor = UIColor.Clear,
				Frame = new RectangleF (Values.Frame, Values.Frame, width - 2 * Values.Frame, height)
			};
			textAuthor = new UILabel() {
				Text = "Author",
				Lines = 0,
				LineBreakMode = UILineBreakMode.WordWrap,
				TextAlignment = UITextAlignment.Center,
				AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight,
				ContentMode = UIViewContentMode.Top,
				BackgroundColor = UIColor.Clear,
				Frame = new RectangleF (Values.Frame, Values.Frame, width - 2 * Values.Frame, height)
			};
			page1.AddSubview(textHeader);
			page1.AddSubview(imagePoster);
			page1.AddSubview(textVersion);
			page1.AddSubview(textAuthor);

			PagesView.AddSubview (page1);

			// Description
			page2 = new UIScrollView (PagesView.Frame) {
				Frame = new RectangleF (1.0f * width, 0.0f, width, PagesView.Frame.Height),
				ContentSize = new SizeF(width, height),
				BackgroundColor = UIColor.Clear
			};
			textDescription = new UILabel () {
				Text = "Description", 
				Lines = 0,
				LineBreakMode = UILineBreakMode.WordWrap,
				TextAlignment = UITextAlignment.Center,
				AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight,
				ContentMode = UIViewContentMode.Top,
				BackgroundColor = UIColor.Clear,
				Frame = new RectangleF (Values.Frame, Values.Frame, width - 2 * Values.Frame, height)
			};
			page2.AddSubview (textDescription);

			PagesView.AddSubview (page2);

			page3 = new UITableView (PagesView.Frame) {
				Frame = new RectangleF (2 * width, 0.0f, width, height),
				SeparatorStyle = UITableViewCellSeparatorStyle.None
			};

			PagesView.AddSubview (page3);
		}

		void Refresh()
		{
			SizeF size;
			float height = Values.Frame;
			float maxWidth = PagesView.Bounds.Width - 2 * Values.Frame;
			float maxHeight = PagesView.Bounds.Height;

			// We start with the default information page
			PagesView.ContentOffset = new PointF(0.0f, 0.0f);

			// Set size of parent scroll view
			PagesView.Frame = new RectangleF(0.0f, 0.0f, PagesController.Frame.Width, PagesController.Frame.Top);

			if (cart != null) {
				textHeader.Text = cart.Name; 
				size = textHeader.SizeThatFits (new SizeF (maxWidth, 999999));
				textHeader.Bounds = new RectangleF (0, 0, maxWidth, size.Height);
				textHeader.Frame = new RectangleF(Values.Frame, height, textHeader.Bounds.Width, textHeader.Bounds.Height);
				height += textHeader.Bounds.Height + Values.Frame;
				if (cart.Poster != null) {
					imagePoster.Image = UIImage.LoadFromData (NSData.FromArray (cart.Poster.Data));
					if (imagePoster.Image.Size.Width > maxWidth)
						imagePoster.Bounds = new RectangleF (0, 0, maxWidth, imagePoster.Image.Size.Height * maxWidth / imagePoster.Image.Size.Width);
					else
						imagePoster.Bounds = new RectangleF (0, 0, imagePoster.Image.Size.Width, imagePoster.Image.Size.Height);
					imagePoster.Hidden = false;
					imagePoster.Frame = new RectangleF(Values.Frame, height, maxWidth, imagePoster.Bounds.Height);
					height += imagePoster.Bounds.Height + Values.Frame;
				} else {
					imagePoster.Image = null;
					imagePoster.Hidden = true;
				}
				if (String.IsNullOrEmpty(cart.Version)) 
					textVersion.Hidden = true;
				else {
					textVersion.Hidden = false;
					textVersion.Text = "Version " + cart.Version;
					size = textVersion.SizeThatFits (new SizeF (maxWidth, 999999));
					textVersion.Bounds = new RectangleF (0, 0, maxWidth, size.Height);
					textVersion.Frame = new RectangleF(Values.Frame, height, textVersion.Bounds.Width, textVersion.Bounds.Height);
					height += textVersion.Bounds.Height + Values.Frame;
				}
				if (String.IsNullOrEmpty(cart.AuthorName) && String.IsNullOrEmpty(cart.AuthorCompany))
					textAuthor.Hidden = true;
				else {
					textAuthor.Hidden = false;
					textAuthor.Text = "By " + (String.IsNullOrEmpty(cart.AuthorName) ? "" : cart.AuthorName) + (!String.IsNullOrEmpty(cart.AuthorName) && !String.IsNullOrEmpty(cart.AuthorCompany) ? " / " : "") + (String.IsNullOrEmpty(cart.AuthorCompany) ? "" : cart.AuthorCompany);
					size = textAuthor.SizeThatFits (new SizeF (maxWidth, 999999));
					textAuthor.Bounds = new RectangleF (0, 0, maxWidth, size.Height);
					textAuthor.Frame = new RectangleF(Values.Frame, height, textAuthor.Bounds.Width, textAuthor.Bounds.Height);
					height += textAuthor.Bounds.Height + Values.Frame;
				}
				page1.ContentSize = new SizeF(maxWidth + 2 * Values.Frame, height);
				textDescription.Text = cart.LongDescription;
				size = textDescription.SizeThatFits (new SizeF (maxWidth, 999999));
				textDescription.Frame = new RectangleF (Values.Frame, Values.Frame, maxWidth, size.Height);
				page2.ContentSize = new SizeF(textDescription.Bounds.Width + 2 * Values.Frame, textDescription.Bounds.Height + 2 * Values.Frame);

				List<string> dataKey = new List<string> ();
				List<string> dataValue = new List<string> ();

				if (!String.IsNullOrEmpty (cart.AuthorName)) {
					dataKey.Add ("Author");
					dataValue.Add (cart.AuthorName);
				}

				if (!String.IsNullOrEmpty (cart.AuthorCompany)) {
					dataKey.Add ("Company");
					dataValue.Add (cart.AuthorCompany);
				}

				if (!String.IsNullOrEmpty (cart.Version)) {
					dataKey.Add ("Version");
					dataValue.Add (cart.Version);
				}

				if (!String.IsNullOrEmpty (cart.Device)) {
					dataKey.Add ("Designed for");
					dataValue.Add (cart.Device);
				}

				if (cart.CreateDate != null) {
					dataKey.Add ("Create Date");
					dataValue.Add (cart.CreateDate.ToString("d", CultureInfo.CurrentCulture));
				}

				if (cart.DateLastUpdated != null) {
					dataKey.Add ("Last Update");
					dataValue.Add (((DateTime)cart.DateLastUpdated).ToString("d", CultureInfo.CurrentCulture));
				}

				if (cart.DateAdded != null) {
					dataKey.Add ("Upload Date");
					dataValue.Add (((DateTime)cart.DateAdded).ToString("d", CultureInfo.CurrentCulture));
				}

				page3.Source = new CartridgeDetailSource (dataKey, dataValue);
			}

			BarItemStore.Title = File.Exists (cart.Filename) ? "Delete" : "Save";
			BarItemRestore.Enabled = File.Exists (cart.SaveFilename);
			BarItemStart.Enabled = File.Exists (cart.Filename);
		}

		#endregion

	}

	#region CartridgeDetailSource

	public class CartridgeDetailSource : UITableViewSource 
	{ 
		List<string> dataKey;
		List<string> dataValue;

		public CartridgeDetailSource(List<string> dataKey, List<string> dataValue) 
		{  
			this.dataKey = dataKey;
			this.dataValue = dataValue;
		}  

		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{ 
			// Reuse a cell if one exists 
			UITableViewCell cell = tableView.DequeueReusableCell ("CartridgeDetailCell") as ScreenListCell;

			if (cell == null) 
			{  
				// We have to allocate a cell 
				cell = new UITableViewCell(UITableViewCellStyle.Value2, "CartridgeDetailCell");
			}

			cell.DetailTextLabel.Text = dataValue[indexPath.Row];
			cell.TextLabel.Text = dataKey[indexPath.Row];

			return cell; 
		}  

		public override int RowsInSection (UITableView tableview, int section) 
		{ 
			return dataKey.Count; 
		} 
	} 

	#endregion

}

