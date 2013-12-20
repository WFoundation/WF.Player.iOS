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
using System.Text;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using MonoTouch.UIKit;
using WF.Player.Core;
using WF.Player.Core.Engines;

namespace WF.Player.iPhone
{

	#region ScreenList
	
	public partial class ScreenList : UIViewController
	{
		ScreenListSource screenListSource;
		
		public UITableView Table;

		#region Constructor
		
		public ScreenList (ScreenController ctrl, ScreenType screen) : base()
		{
			this.ctrl = ctrl;
			this.engine = ctrl.Engine;
			this.screen = screen;

			// Create source for table view
			screenListSource = new ScreenListSource(this, ctrl, screen);

			// OS specific details
			if (new Version (UIDevice.CurrentDevice.SystemVersion) >= new Version(7,0)) 
			{
				// Code that uses features from Xamarin.iOS 7.0
				this.EdgesForExtendedLayout = UIRectEdge.None;
			}
		}

		#endregion
		
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
			NavigationItem.SetLeftBarButtonItem(new UIBarButtonItem(@"Back",UIBarButtonItemStyle.Plain, (sender,args) => { ctrl.RemoveScreen(screen); }), false);
			NavigationItem.LeftBarButtonItem.TintColor = Colors.NavBarButton;

			// Create table view
			Table = new UITableView()
			{
				Source = screenListSource,
				AutoresizingMask = UIViewAutoresizing.All,
				AutosizesSubviews = true
			};

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

			StartEvents ();

			Refresh(true);
		}
		
		public override void ViewDidDisappear (bool animated)
		{
			base.ViewDidDisappear(animated);

			StopEvents ();
		}

		public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation) 
		{
			base.DidRotate(fromInterfaceOrientation);

			Refresh (false);
		}
		
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			// Return true for supported orientations
			return true;
		}

		#endregion

		#region Private Functions

		void Refresh(bool itemsChanged)
		{
			if (itemsChanged)
				NavigationItem.Title = GetContent ();

			Table.ReloadData ();
		}

		#endregion
	}
	
	#endregion
	
	#region ScreenListSource
	
	public class ScreenListSource : UITableViewSource 
	{ 
		ScreenList owner;
		ScreenController ctrl;
		ScreenType screen;

		public ScreenListSource(ScreenList owner, ScreenController ctrl, ScreenType screen) 
		{  
			this.owner = owner;
			this.ctrl = ctrl;
			this.screen = screen;
		}  
		
		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{ 
			// Reuse a cell if one exists 
			ScreenListCell cell = tableView.DequeueReusableCell ("ScreenListCell") as ScreenListCell;
			
			if (cell == null || cell.HasIcon != owner.ShowIcons || cell.HasDirection != owner.ShowDirections) 
			{  
				// We have to allocate a cell 
				cell = new ScreenListCell(owner.ShowIcons, owner.ShowDirections);
			}
			
			cell.RefreshCell (owner, screen, ctrl.Engine, owner.Items[indexPath.Row]);

			return cell; 
		}  

		public override int RowsInSection (UITableView tableview, int section) 
		{ 
			return owner.Items.Count; 
		} 
		
		public override float GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
		{
			// Insert needed height value
			return 72.0f;
		}
		
		public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
		{
			owner.EntrySelected(indexPath.Row);
		}
	} 
	
	#endregion
	
	#region ScreenListCell
	
	public partial class ScreenListCell : UITableViewCell
	{
		private UILabel textTitle;
		private UILabel textDistance;
		private UIImageView imageIcon;
		private UIImageView imageDirection;

		public bool HasIcon;
		public bool HasDirection;

		public ScreenListCell (bool showIcons, bool showDirections) : base ()
		{
			this.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
			CreateCell (showIcons, showDirections);
		}
		
		public ScreenListCell (IntPtr handle, bool showIcons, bool showDirections) : base (handle)
		{
			this.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
			CreateCell (showIcons, showDirections);
		}
		
		void CreateCell (bool showIcons, bool showDirections)
		{
			float maxWidth = this.Bounds.Width - 20;

			if (showIcons) 
			{
				imageIcon = new UIImageView () {
					Bounds = new Rectangle (0, 0, 48, 48),
					Frame = new Rectangle (10, 10, 48, 48)
				};
				this.AddSubview (imageIcon);
			}

			HasIcon = showIcons;

			textTitle = new UILabel()
			{
				Frame = showIcons ? new RectangleF(72,10,maxWidth - (showDirections ? 140 : 0),48) : new RectangleF(10,10,maxWidth - (showDirections ? 140 : 0),48),
				Font = UIFont.SystemFontOfSize (20),
				Lines = 2
			};

			this.ContentView.AddSubview (textTitle);

			if (showDirections) 
			{
				imageDirection = new UIImageView () {
					Frame = new RectangleF (maxWidth - 32, 10, 32, 32),
					ContentMode = UIViewContentMode.ScaleAspectFit
				};
				
				textDistance = new UILabel () {
					Frame = new RectangleF (maxWidth - 40, 46, 46, 21),
					Font = UIFont.SystemFontOfSize (10),
					TextColor = UIColor.Red,
					TextAlignment = UITextAlignment.Center,
					Lines = 1,
					LineBreakMode = UILineBreakMode.TailTruncation | UILineBreakMode.WordWrap
				};
			
				this.ContentView.AddSubview (imageDirection);
				this.ContentView.AddSubview (textDistance);
			}

			HasDirection = showDirections;
		}

		public void RefreshCell (ScreenList owner, ScreenType screenType, Engine engine, UIObject obj)
		{
			if (imageIcon != null) 
			{
				if (obj.Icon == null)
					imageIcon.Image = null;
				else
				{
					imageIcon.Image = UIImage.LoadFromData (NSData.FromArray (obj.Icon.Data));
					imageIcon.ContentMode = UIViewContentMode.ScaleAspectFit;
				}
			}

			if (screenType == ScreenType.Tasks) 
			{
				// If a task, than show CorrectState by character in front of name
				textTitle.Text = (((Task)obj).Complete ? (((Task)obj).CorrectState == TaskCorrectness.NotCorrect ? Strings.TaskNotCorrect : Strings.TaskCorrect) + " " : "") + obj.Name;
			}
			else
				textTitle.Text = obj.Name;

			if (HasDirection)
			{
				if (screenType != ScreenType.Tasks && screenType != ScreenType.Inventory) {
					if (obj is Zone && ((Zone)obj).State == PlayerZoneState.Inside) {
						imageDirection.Hidden = false;
						imageDirection.Image = drawCenter ();
						textDistance.Hidden = false;
						textDistance.Text = "Inside";
					} else {
						if (((Thing)obj).VectorFromPlayer != null) {
							imageDirection.Hidden = false;
							imageDirection.Image = drawArrow ((((Thing)obj).VectorFromPlayer.Bearing.GetValueOrDefault () + engine.Heading) % 360); // * 180.0 / Math.PI);
							textDistance.Hidden = false;
							textDistance.Text = ((Thing)obj).VectorFromPlayer.Distance.BestMeasureAs (DistanceUnit.Meters);
						} else {
							imageDirection.Hidden = true;
							textDistance.Hidden = true;
						}
					}
				} else {
					imageDirection.Hidden = true;
					textDistance.Hidden = true;
				}
			}
		}

		private UIImage drawCenter ()
		{
			UIGraphics.BeginImageContext (new SizeF(64, 64));
			
			using (CGContext cont = UIGraphics.GetCurrentContext()) {
				
				using (CGPath path = new CGPath()) {
					
					cont.SetLineWidth (1f);
					cont.SetRGBStrokeColor (1f, 0, 0, 1);
					cont.SetRGBFillColor (0.5f, 0, 0, 1);
					path.AddElipseInRect(new RectangleF(24,24,16,16));
					path.CloseSubpath ();
					
					cont.AddPath (path);
					cont.DrawPath (CGPathDrawingMode.FillStroke);
					
				}
				
				using (CGPath path = new CGPath()) {
					
					cont.SetRGBStrokeColor (1f, 0, 0, 1);
					cont.SetLineWidth(3f);
//					cont.SetRGBFillColor (.5f, 0, 0, 1);
					path.AddElipseInRect(new RectangleF(16,16,32,32));
					path.CloseSubpath ();
					
					cont.AddPath (path);
					cont.DrawPath (CGPathDrawingMode.Stroke);
					
				}
				
				return UIGraphics.GetImageFromCurrentImageContext ();
				
			}
			
			UIGraphics.EndImageContext ();
		}

		private UIImage drawArrow (double direction)
		{
//			direction = 180.0 - direction;

			double rad1 = direction / 180.0 * Math.PI;
			double rad2 = (direction + 180.0 + 30.0) / 180.0 * Math.PI;
			double rad3 = (direction + 180.0 - 30.0) / 180.0 * Math.PI; 
			double rad4 = (direction + 180.0) / 180.0 * Math.PI; 

			PointF p1 = new PointF((float) (32 + 32 * Math.Sin (rad1)), (float) (32 + 32 * Math.Cos (rad1)));
			PointF p2 = new PointF((float) (32 + 32 * Math.Sin (rad2)), (float) (32 + 32 * Math.Cos (rad2)));
			PointF p3 = new PointF((float) (32 + 32 * Math.Sin (rad3)), (float) (32 + 32 * Math.Cos (rad3)));
			PointF p4 = new PointF((float) (32 + 20 * Math.Sin (rad4)), (float) (32 + 20 * Math.Cos (rad4)));

			UIGraphics.BeginImageContext (new SizeF(64, 64));
			
			using (CGContext cont = UIGraphics.GetCurrentContext()) {

				using (CGPath path = new CGPath()) {
					
					cont.SetLineWidth (1f);
					cont.SetRGBStrokeColor (0f, 0, 0, 1);
					cont.SetRGBFillColor (1f, 0, 0, 1);
					path.AddLines (new PointF[] { p1, p2, p4 });
					path.CloseSubpath ();
					
					cont.AddPath (path);
					cont.DrawPath (CGPathDrawingMode.FillStroke);

				}

				using (CGPath path = new CGPath()) {
					
					cont.SetRGBStrokeColor (0f, 0, 0, 1);
					cont.SetRGBFillColor (.5f, 0, 0, 1);
					path.AddLines (new PointF[] { p1, p3, p4 });
					path.CloseSubpath ();
					
					cont.AddPath (path);
					cont.DrawPath (CGPathDrawingMode.FillStroke);

				}

				return UIGraphics.GetImageFromCurrentImageContext ();
				
			}

			UIGraphics.EndImageContext ();
		}

	}
	
	#endregion
	
}

