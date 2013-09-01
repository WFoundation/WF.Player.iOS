// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;

namespace WF.Player.iPhone
{
	[Register ("MainScreenController")]
	partial class MainScreenController
	{
		[Outlet]
		MonoTouch.UIKit.UITableView MainTable { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (MainTable != null) {
				MainTable.Dispose ();
				MainTable = null;
			}
		}
	}
}
