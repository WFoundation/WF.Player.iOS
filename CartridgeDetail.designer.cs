// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;
using System.CodeDom.Compiler;

namespace WF.Player.iPhone
{
	[Register ("CartridgeDetail")]
	partial class CartridgeDetail
	{
		[Outlet]
		MonoTouch.UIKit.UIBarButtonItem BarItemRestore { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIBarButtonItem BarItemStart { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIBarButtonItem BarItemStore { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIPageControl PagesController { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIScrollView PagesView { get; set; }

		[Action ("BarItemRestorePressed:")]
		partial void BarItemRestorePressed (MonoTouch.Foundation.NSObject sender);

		[Action ("BarItemStartPressed:")]
		partial void BarItemStartPressed (MonoTouch.Foundation.NSObject sender);

		[Action ("BarItemStorePressed:")]
		partial void BarItemStorePressed (MonoTouch.Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (BarItemRestore != null) {
				BarItemRestore.Dispose ();
				BarItemRestore = null;
			}

			if (BarItemStart != null) {
				BarItemStart.Dispose ();
				BarItemStart = null;
			}

			if (BarItemStore != null) {
				BarItemStore.Dispose ();
				BarItemStore = null;
			}

			if (PagesController != null) {
				PagesController.Dispose ();
				PagesController = null;
			}

			if (PagesView != null) {
				PagesView.Dispose ();
				PagesView = null;
			}
		}
	}
}
