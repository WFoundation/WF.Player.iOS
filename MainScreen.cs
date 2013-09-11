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
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.ObjCRuntime;
using WF.Player.Core;

namespace WF.Player.iPhone
{

	#region MainScreen
	
	public class MainScreen : UIViewController
	{
		private ScreenController ctrl;

		public UITableView Table;

		public MainScreen (ScreenController ctrl) : base()
		{
			this.ctrl = ctrl;
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

			// Create source for table view
			MainScreenSource mainListSource = new MainScreenSource(ctrl);

			// Create table view
			Table = new UITableView()
			{
				Source = mainListSource,
				AutoresizingMask = UIViewAutoresizing.All,
				AutosizesSubviews = true
			};

			// Set the table view to fit the width of the app.
			Table.SizeToFit();
			// Reposition and resize the receiver
			Table.Frame = new RectangleF (0, 0, this.View.Frame.Width,this.View.Frame.Height);
			// Add the table view as a subview
			this.View.AddSubviews(this.Table);
		}
		
		public override void ViewDidUnload ()
		{
			base.ViewDidUnload ();
			
			// Clear any references to subviews of the main view in order to
			// allow the Garbage Collector to collect them sooner.
			//
			// e.g. myOutlet.Dispose (); myOutlet = null;

			if (Table != null) {
				Table.Dispose ();
				Table = null;
			}
		}

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear(animated);
			this.NavigationController.SetNavigationBarHidden(false,false);
			Table.ReloadData ();
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
		
		#endregion
	}
	
	#endregion

	#region MainScreenSource

	public class MainScreenSource : UITableViewSource 
	{ 
		private ScreenController ctrl;
		
		public MainScreenSource(ScreenController ctrl) 
		{  
			this.ctrl = ctrl;
		}  
		
		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{ 
			// Reuse a cell if one exists 
			MainScreenCell cell = tableView.DequeueReusableCell ("MainScreenCell") as MainScreenCell;
			
			if (cell == null) 
			{  
				// We have to allocate a cell 
				cell = new MainScreenCell();
			}
			
			// Set initial data 
			if (ctrl != null && ctrl.Cartridge != null) 
			{
				cell.UpdateData (ctrl.Engine, indexPath.Row);
			}
			
			return cell; 
		}  
		
		public override int RowsInSection (UITableView tableview, int section) 
		{ 
			return 4; 
		} 
		
		public override float GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
		{
			// Insert needed height value
			return 104.0f;
		}
		
		public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
		{
			switch (indexPath.Row) {
			case 0:
				ctrl.ShowScreen (ScreenType.Locations, 0);
				break;
			case 1:
				ctrl.ShowScreen (ScreenType.Items, 0);
				break;
			case 2:
				ctrl.ShowScreen (ScreenType.Inventory, 0);
				break;
			case 3:
				ctrl.ShowScreen (ScreenType.Tasks, 0);
				break;
			}
		}
	} 

	#endregion

	#region MainScreenCell

	public partial class MainScreenCell : UITableViewCell
	{
		private UILabel textTitle;
		private UILabel textItems;
		private UIImageView imageIcon;

		public MainScreenCell () : base ()
		{
			this.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
			createCell ();
		}
		
		public MainScreenCell (IntPtr handle) : base (handle)
		{
			this.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
			createCell ();
		}
		
		private void createCell ()
		{
			float maxWidth = this.Bounds.Width - 20;

			imageIcon = new UIImageView()
			{
				Frame = new Rectangle(10,10,64,64)
			};
			
			textTitle = new UILabel()
			{
				Frame = new RectangleF(88,10,maxWidth - 88,26),
				Font = UIFont.SystemFontOfSize (24)
			};
			
			textItems = new UILabel()
			{
				Frame = new RectangleF(88,44,maxWidth - 88,54),
				Font = UIFont.SystemFontOfSize(12),
				TextAlignment = UITextAlignment.Left,
				Lines = 3,
				LineBreakMode = UILineBreakMode.TailTruncation | UILineBreakMode.WordWrap
			};
			
			this.AddSubview(imageIcon);
			this.AddSubview(textTitle);
			this.AddSubview(textItems);
		}
		
		public void UpdateData (Engine engine, int row)
		{
			string title = "";
			string icon = "";
			string empty = "";
			StringBuilder itemsText = new StringBuilder ();
			List<string> itemList = new List<string> ();

			// Are we in the start up
			if(engine.Cartridge == null)
				return;

			switch (row) {
			case 0:
				title = "Locations";
				icon = "Images/IconLocation.png";
				empty = engine.Cartridge.EmptyZonesListText;
				List<Zone> zones = engine.ActiveVisibleZones;
				foreach(Zone z in zones)
					itemList.Add (z.Name);
				break;
			case 1:
				title = "You see";
				icon = "Images/IconYouSee.png";
				empty = engine.Cartridge.EmptyYouSeeListText;
				List<Thing> items = engine.VisibleObjects;
				foreach (Thing i in items)
					itemList.Add (i.GetString ("Name"));
				break;
			case 2:
				title = "Inventory";
				icon = "Images/IconInventory.png";
				empty = engine.Cartridge.EmptyInventoryListText;
				items = engine.VisibleInventory;
				foreach (Thing i in items)
					itemList.Add (i.GetString ("Name"));
				break;
			case 3:
				title = "Tasks";
				icon = "Images/IconTask.png";
				empty = engine.Cartridge.EmptyTasksListText;
				List<Task> tasks = engine.ActiveVisibleTasks;
				foreach(Task t in tasks)
					itemList.Add (t.Name);
				break;
			}
			
			this.textTitle.Text = string.Format ("{0} [{1}]", title, itemList.Count);

			if (itemList.Count == 0)
				textItems.Text = empty;
			else {
				foreach (string s in itemList) {
					if (itemsText.Length != 0)
						itemsText.Append (", ");
					itemsText.Append (s);
				}
				this.textItems.Text = itemsText.ToString ();
			}

			textItems.SizeToFit ();

			this.imageIcon.Image = UIImage.FromFile (icon);
		}
	}

	#endregion

}
