// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;

namespace WF.Player.iPhone
{
	[Register ("CartridgeDetail")]
	partial class CartridgeDetail
	{
		[Outlet]
		MonoTouch.UIKit.UITextView TextTitle { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextView TextDescription { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIImageView ImagePoster { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton ButtonStart { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton ButtonResume { get; set; }

		[Action ("ButtonStartPressed:")]
		partial void ButtonStartPressed (MonoTouch.Foundation.NSObject sender);

		[Action ("ButtonResumePressed:")]
		partial void ButtonResumePressed (MonoTouch.Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (TextTitle != null) {
				TextTitle.Dispose ();
				TextTitle = null;
			}

			if (TextDescription != null) {
				TextDescription.Dispose ();
				TextDescription = null;
			}

			if (ImagePoster != null) {
				ImagePoster.Dispose ();
				ImagePoster = null;
			}

			if (ButtonStart != null) {
				ButtonStart.Dispose ();
				ButtonStart = null;
			}

			if (ButtonResume != null) {
				ButtonResume.Dispose ();
				ButtonResume = null;
			}
		}
	}
}
