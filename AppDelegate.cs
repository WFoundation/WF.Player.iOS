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
	public partial class AppDelegate : UIApplicationDelegate
	{
		// class-level declarations
		UIWindow window;
		UINavigationController navCartSelect;
		Engine engine;
		CartridgeList viewCartSelect;

		//
		// This method is invoked when the application has loaded and is ready to run. In this 
		// method you should instantiate the window, load the UI into it and then make the window
		// visible.
		//
		// You have 17 seconds to return from this method, or iOS will terminate your application.
		//
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			// create a new window instance based on the screen size
			window = new UIWindow (UIScreen.MainScreen.Bounds);

			// Create NavigationControlls
			navCartSelect = new UINavigationController();

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

//		public override bool ApplicationDidBecomeActive()
//		{
//			return true;
//		}

		public void CartStart(Cartridge cart)
		{
			// Create engine
			engine = new Engine ();
			
			// Load cartridge into engine
			engine.Init(new FileStream(cart.Filename,FileMode.Open),cart);

			// Create main screen handler
			ScreenController screen = new ScreenController(this);

			// Set engine belonging to this screen
			screen.Engine = engine;
			
			// Set as new navigation controll
			window.RootViewController = screen;
			
			engine.Start ();

			// Refresh screen
			screen.Refresh();
		}

		public void CartRestore(Cartridge cart)
		{
			// Create engine
			engine = new Engine ();
			
			// Load cartridge into engine
			engine.Init(new FileStream(cart.Filename,FileMode.Open),cart);

			// Create main screen handler
			ScreenController screen = new ScreenController(this);

			// Set engine belonging to this screen
			screen.Engine = engine;

			// Set as new navigation controll
			window.RootViewController = screen;
			
			engine.Restore (new FileStream(cart.SaveFilename,FileMode.Open));

			// Refresh screen
			screen.Refresh();
		}

		public void CartStop()
		{
			engine.Stop();

			var screen = window.RootViewController;
			window.RootViewController = navCartSelect;
			screen.Dispose ();
		}
	}
}