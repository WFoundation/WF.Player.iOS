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
using WF.Player.Core.Engines;

namespace WF.Player.iPhone
{

	#region MainScreen
	
	public partial class ScreenMain : UIViewController
	{
		public UITableView Table;

		#region Constructor

		public ScreenMain (ScreenController ctrl) : base()
		{
			this.ctrl = ctrl;
			this.engine = ctrl.Engine;
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
			iconLocation = UIImage.FromFile ("Images/IconLocation.png");
			iconYouSee = UIImage.FromFile ("Images/IconYouSee.png");
			iconInventory = UIImage.FromFile ("Images/IconInventory.png");
			iconTask = UIImage.FromFile ("Images/IconTask.png");

			// Create source for table view
			MainScreenSource mainListSource = new MainScreenSource(this, ctrl);

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

			NavigationController.SetNavigationBarHidden(false,false);
			NavigationItem.Title = engine.Cartridge.Name;

			StartEvents ();

			Refresh();
		}
		
		public override void ViewDidDisappear (bool animated)
		{
			base.ViewDidDisappear(animated);

			StopEvents();
		}

		public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation) 
		{
			base.DidRotate(fromInterfaceOrientation);
			Refresh();
		}
		
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			// Return true for supported orientations
			return true;
		}
		
		#endregion

		#region Private Functions

		void Refresh()
		{
			Table.ReloadData ();
		}

		#endregion
	}
	
	#endregion

	#region MainScreenSource

	public class MainScreenSource : UITableViewSource 
	{ 
		ScreenController ctrl;
		ScreenMain owner;

		#region Constructor

		public MainScreenSource(ScreenMain owner, ScreenController ctrl) 
		{  
			this.owner = owner;
			this.ctrl = ctrl;
		}  

		#endregion

		#region MonoTouch Functions
		
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
			if (ctrl.Engine.GameState == EngineGameState.Playing) 
			{
				cell.RefreshCell (owner, ctrl.Engine, indexPath.Row);
			}
			
			return cell; 
		}  
		
		public override int RowsInSection (UITableView tableview, int section) 
		{ 
			return 4; 
		} 

		public override float GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
		{
			MainScreenCell cell = new MainScreenCell();

			// Set initial data 
			if (ctrl.Engine.GameState == EngineGameState.Playing) 
			{
				cell.RefreshCell (owner, ctrl.Engine, indexPath.Row);
			}

			float height = 0;

			foreach (UIView v in cell.Subviews)
				if (v.Frame.Bottom > height)
					height = v.Frame.Bottom;

			return height + 10;
		}
		
		public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
		{
			owner.EntrySelected(indexPath.Row);
		}

		#endregion

	} 

	#endregion

	#region MainScreenCell

	public partial class MainScreenCell : UITableViewCell
	{
		UILabel textTitle;
		UILabel textItems;
		UIImageView imageIcon;

		#region Constructor

		public MainScreenCell () : base ()
		{
			this.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
			CreateCell ();
		}
		
		public MainScreenCell (IntPtr handle) : base (handle)
		{
			this.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
			CreateCell ();
		}

		#endregion

		#region Helper Functions
		
		void CreateCell ()
		{
			float maxWidth = this.Bounds.Width - 20;

			imageIcon = new UIImageView()
			{
				Frame = new Rectangle(10,10,64,64)
			};
			
			textTitle = new UILabel()
			{
				Frame = new RectangleF(88,10,maxWidth - 0,26),
				Font = UIFont.SystemFontOfSize (24)
			};
			
			textItems = new UILabel()
			{
				Frame = new RectangleF(88,44,maxWidth - 0,54),
				Font = UIFont.SystemFontOfSize(12),
				TextAlignment = UITextAlignment.Left,
				Lines = 0,
				LineBreakMode = UILineBreakMode.TailTruncation | UILineBreakMode.WordWrap
			};
			
			this.AddSubview(imageIcon);
			this.AddSubview(textTitle);
			this.AddSubview(textItems);
		}

		#endregion

		#region Common Functions
		
		public void RefreshCell (ScreenMain owner, Engine engine, int row)
		{
			string header;
			string items;
			object image;

			// Are we in the start up
			if(engine.GameState != EngineGameState.Playing)
				return;

			owner.GetContentEntry (row, out header, out items, out image);

			textTitle.Text = header;
			textItems.Text = items;
			textItems.SizeToFit ();

			imageIcon.Image = (UIImage)image;
		}

		#endregion

	}

	#endregion

}
