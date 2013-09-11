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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using MonoTouch.UIKit;
using WF.Player.Core;

namespace WF.Player.iPhone
{

	#region ItemScreen
	
	public class ItemScreen : UIViewController
	{
		private ScreenController ctrl;
		ItemScreenSource itemScreenSource;
		
		public UITableView Table;
		
		public ItemScreen (ScreenController ctrl) : base()
		{
			this.ctrl = ctrl;

			// Create source for table view
			itemScreenSource = new ItemScreenSource(ctrl);

			// Create table view
			Table = new UITableView()
			{
				Source = itemScreenSource,
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

		public bool UpdateData (ScreenType screenId)
		{
			if (itemScreenSource != null)
				return itemScreenSource.UpdateData (screenId);
			else
				return false;
		}

		#endregion
	}
	
	#endregion
	
	#region ItemScreenSource
	
	public class ItemScreenSource : UITableViewSource 
	{ 
		private ScreenController ctrl;
		private ScreenType screenType;
		private List<UIObject> list;

		public ItemScreenSource(ScreenController ctrl) 
		{  
			this.ctrl = ctrl;
		}  
		
		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{ 
			// Reuse a cell if one exists 
			ItemScreenCell cell = tableView.DequeueReusableCell ("ItemScreenCell") as ItemScreenCell;
			
			if (cell == null) 
			{  
				// We have to allocate a cell 
				cell = new ItemScreenCell();
			}
			
			// Set initial data 
			if (ctrl != null && ctrl.Cartridge != null) 
			{
				cell.UpdateData (screenType, ctrl.Engine, list[indexPath.Row]);
			}
			
			return cell; 
		}  
		
		public override int RowsInSection (UITableView tableview, int section) 
		{ 
			if (list == null)
				return 0;
			else
				return list.Count; 
		} 
		
		public override float GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
		{
			// Insert needed height value
			return 72.0f;
		}
		
		public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
		{
			// TODO
			// Remove convert
			Table obj = list[indexPath.Row];
			int idx = obj.GetInt ("ObjIndex");
			ctrl.ShowScreen (ScreenType.Details, idx);
		}

		public bool UpdateData(ScreenType type)
		{
			screenType = type;
			
			if (list != null)
				list.Clear();
			else
				list = new List<UIObject>();

			if (screenType == ScreenType.Locations)
			{
				List<Zone> zones = ctrl.Engine.ActiveVisibleZones;
				foreach(Zone z in zones)
					list.Add (z);
			}
			if (screenType == ScreenType.Items)
			{
				List<Thing> items = ctrl.Engine.VisibleObjects;
				foreach(Thing i in items)
					list.Add (i);
			}
			if (screenType == ScreenType.Inventory)
			{
				List<Thing> items = ctrl.Engine.VisibleInventory;
				foreach(Thing i in items)
					list.Add (i);
			}
			if (screenType == ScreenType.Tasks)
			{
				List<Task> tasks = ctrl.Engine.ActiveVisibleTasks;
				foreach(Task t in tasks)
					list.Add (t);
			}
			
			return list.Count != 0;
		}

	} 
	
	#endregion
	
	#region ItemScreenCell
	
	public partial class ItemScreenCell : UITableViewCell
	{
		private UILabel textTitle;
		private UILabel textDistance;
		private UIImageView imageIcon;
		private UIImageView imageDirection;

		public ItemScreenCell () : base ()
		{
			this.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
			createCell ();
		}
		
		public ItemScreenCell (IntPtr handle) : base (handle)
		{
			this.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
			createCell ();
		}
		
		private void createCell ()
		{
			float maxWidth = this.Bounds.Width - 20;

			imageIcon = new UIImageView()
			{
				Frame = new Rectangle(10,10,48,48)
			};
			
			textTitle = new UILabel()
			{
				Frame = new RectangleF(72,10,maxWidth - 140,48),
				Font = UIFont.SystemFontOfSize (20),
				Lines = 2
			};
			
			imageDirection = new UIImageView()
			{
				Frame = new RectangleF(maxWidth - 32,10,32,32),
				ContentMode = UIViewContentMode.ScaleAspectFit
			};
			
			textDistance = new UILabel()
			{
				Frame = new RectangleF(maxWidth - 40,46,46,21),
				Font = UIFont.SystemFontOfSize(10),
				TextColor = UIColor.Red,
				TextAlignment = UITextAlignment.Center,
				Lines = 1,
				LineBreakMode = UILineBreakMode.TailTruncation | UILineBreakMode.WordWrap
			};
			
			this.AddSubview (imageIcon);
			this.AddSubview (textTitle);
			this.AddSubview (imageDirection);
			this.AddSubview (textDistance);
		}

		public void UpdateData (ScreenType screenType, Engine engine, UIObject t)
		{
			if (t.Icon == null)
				this.imageIcon.Image = null;
			else
				this.imageIcon.Image = UIImage.LoadFromData (NSData.FromArray (t.Icon.Data));

			this.textTitle.Text = t.Name;

			if (screenType != ScreenType.Tasks && screenType != ScreenType.Inventory) {
				if(t is Zone && t.GetString ("State").ToLower ().Equals ("inside")) {
					this.imageDirection.Hidden = false;
					this.imageDirection.Image = drawCenter ();
					this.textDistance.Hidden = false;
					this.textDistance.Text = "Inside";
				} else {
					this.imageDirection.Hidden = false;
					this.imageDirection.Image = drawArrow ((((Thing)t).VectorFromPlayer.Bearing.GetValueOrDefault()+engine.Heading)%360); // * 180.0 / Math.PI);
					this.textDistance.Hidden = false;
					this.textDistance.Text = ((Thing)t).VectorFromPlayer.Distance.BestMeasureAs(DistanceUnit.Meters);
				}
			} else {
				this.imageDirection.Hidden = true;
				this.textDistance.Hidden = true;
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

