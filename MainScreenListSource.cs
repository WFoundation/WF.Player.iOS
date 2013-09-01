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
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using WF.Player.Core;

namespace WF.Player.iPhone
{
	public class MainScreenListSource : UITableViewSource 
	{ 
		private ScreenController ctrl;

		public MainScreenListSource(ScreenController ctrl) 
		{  
			this.ctrl = ctrl;
		}  

		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{ 
			// Reuse a cell if one exists 
			MainScreenListCell cell = tableView.DequeueReusableCell ("MainScreenListCell") as MainScreenListCell;

			if (cell == null) 
			{  
				// We have to allocate a cell 
				var views = NSBundle.MainBundle.LoadNib ("MainScreenListCell", tableView, null); 
				cell = Runtime.GetNSObject (views.ValueAt (0)) as MainScreenListCell; 
			}
			
			// Set initial data 
			if (ctrl != null && ctrl.Cartridge != null) 
			{
				cell.UpdateData (ctrl.Cartridge, indexPath.Row);
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
				ctrl.ShowScreen (Const.LOCATIONSCREEN, null);
				break;
			case 1:
				ctrl.ShowScreen (Const.ITEMSCREEN, null);
				break;
			case 2:
				ctrl.ShowScreen (Const.INVENTORYSCREEN, null);
				break;
			case 3:
				ctrl.ShowScreen (Const.TASKSCREEN, null);
				break;
			}
		}
	} 
}

