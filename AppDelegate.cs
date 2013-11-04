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
using System.IO;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using WF.Player.Core;

namespace WF.Player.iPhone
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the 
	// User Interface of the application, as well as listening (and optionally responding) to 
	// application events from iOS.
	[Register ("AppDelegate")]
	[CLSCompliantAttribute(false)]
	public partial class AppDelegate : UIApplicationDelegate
	{
		// class-level declarations
		UIWindow window;
		UINavigationController navCartSelect;
		CartridgeList viewCartSelect;
		ScreenController screenCtrl;

		//
		// This method is invoked when the application has loaded and is ready to run. In this 
		// method you should instantiate the window, load the UI into it and then make the window
		// visible.
		//
		// You have 17 seconds to return from this method, or iOS will terminate your application.
		//
		[CLSCompliantAttribute(false)]
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			// create a new window instance based on the screen size
			window = new UIWindow (UIScreen.MainScreen.Bounds);

			// Create NavigationControlls
			navCartSelect = new UINavigationController();

			// Set color of NavigationBar and NavigationButtons (TintColor)
			navCartSelect.NavigationBar.SetBackgroundImage (new UIImage(), UIBarMetrics.Default);
			navCartSelect.NavigationBar.BackgroundColor = UIColor.FromRGB(0.1992f,0.7070f,0.8945f);
			navCartSelect.NavigationBar.TintColor = UIColor.FromRGB(1f,0f,0f);

			// Now create list for cartridges
			viewCartSelect = new CartridgeList(this);

			// Add the cartridge view to the navigation controller
			// (it'll be the top most screen)
			navCartSelect.PushViewController((UIViewController)viewCartSelect, false);

			// Set the root view controller on the window. The nav
			// controller will handle the rest
			// If you have defined a view, add it here:
			// window.AddSubview (navigationController.View);
			this.window.RootViewController = navCartSelect;
			
			// make the window visible
			window.MakeKeyAndVisible ();
			
			return true;
		}

		public override void DidEnterBackground (UIApplication application)
		{
			// Save game before going into background
			if (screenCtrl != null && screenCtrl.Engine != null) {
				// Save game automatically
				screenCtrl.Engine.Save (new FileStream (screenCtrl.Engine.Cartridge.SaveFilename, FileMode.Create));
				// Pause engine until we have focus again
				screenCtrl.Engine.Pause ();
			}
		}

		public override void WillEnterForeground (UIApplication application)
		{
			if (screenCtrl != null && screenCtrl.Engine != null) {
				// Resume engine, so we continue
				screenCtrl.Engine.Resume ();
			}
		}

		public override void ReceiveMemoryWarning (UIApplication application)
		{
			// Save game before we could get killed
			if (screenCtrl != null && screenCtrl.Engine != null) {
				// Save game automatically
				screenCtrl.Engine.Save (new FileStream (screenCtrl.Engine.Cartridge.SaveFilename, FileMode.Create));
			}
		}

		[CLSCompliantAttribute(false)]
		public void CartStart(Cartridge cart)
		{
			// Create main screen handler
			screenCtrl = new ScreenController(this, cart);

			// Set as new navigation controll
			window.RootViewController = screenCtrl;

			screenCtrl.Start ();
		}

		[CLSCompliantAttribute(false)]
		public void CartRestore(Cartridge cart)
		{
			// Create main screen handler
			screenCtrl = new ScreenController(this, cart);

			// Set as new navigation controll
			window.RootViewController = screenCtrl;
			
			screenCtrl.Restore();
		}

		public void CartStop()
		{
			if (screenCtrl != null) {
				window.RootViewController = navCartSelect;

				screenCtrl.Dispose ();
				screenCtrl = null;
			}
		}
	}
}