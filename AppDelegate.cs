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
using System.Globalization;
using System.IO;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using GNU.Gettext;
using WF.Player.Core;

namespace WF.Player.iOS
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
		NSObject observerSettings;

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
			// TODO: Delete
			Console.WriteLine ("FinishedLaunching");

			//			System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo("de-DE");
			observerSettings = NSNotificationCenter.DefaultCenter.AddObserver ((NSString)"NSUserDefaultsDidChangeNotification", DefaultsChanged);
			DefaultsChanged (null);

			// create a new window instance based on the screen size
			window = new UIWindow (UIScreen.MainScreen.Bounds);

			// Set default color for NavigationButtons
			UIBarButtonItem.Appearance.TintColor = Colors.NavBarButton;

			// Create NavigationControlls
			navCartSelect = new UINavigationController();

			// Set color of NavigationBar and NavigationButtons (TintColor)
			// OS specific details
			if (new Version (UIDevice.CurrentDevice.SystemVersion) >= new Version(7,0)) 
				navCartSelect.NavigationBar.SetBackgroundImage (Images.BlueTop, UIBarMetrics.Default);
			else
				navCartSelect.NavigationBar.SetBackgroundImage (Images.Blue, UIBarMetrics.Default);

			navCartSelect.NavigationBar.TintColor = Colors.NavBarButton;

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
			// TODO: Delete
			Console.WriteLine ("DidEnterBackground");

			// Save game before going into background
			if (screenCtrl != null && screenCtrl.Engine != null && screenCtrl.Engine.GameState == WF.Player.Core.Engines.EngineGameState.Playing) {
				// Save game automatically
				Console.WriteLine ("Start Save");
				screenCtrl.Engine.Save (new FileStream (screenCtrl.Engine.Cartridge.SaveFilename, FileMode.Create));
				Console.WriteLine ("Ende Save");
				// Pause engine until we have focus again
				screenCtrl.Engine.Pause ();
			}
		}

		public override void WillEnterForeground(UIApplication application)
		{
			// TODO: Delete
			Console.WriteLine ("WillEnterForeground");

			if (screenCtrl != null && screenCtrl.Engine != null && screenCtrl.Engine.GameState == WF.Player.Core.Engines.EngineGameState.Paused) {
				// Resume engine, so we continue
				screenCtrl.Engine.Resume ();
			}
		}

		public override void OnResignActivation(UIApplication application)
		{
			// TODO: Delete
			Console.WriteLine ("OnResignActivation");

			// Save game before going into background
			if (screenCtrl != null && screenCtrl.Engine != null && screenCtrl.Engine.GameState == WF.Player.Core.Engines.EngineGameState.Playing) {
				// Save game automatically
				screenCtrl.Engine.Save (new FileStream (screenCtrl.Engine.Cartridge.SaveFilename, FileMode.Create));
				// Pause engine until we have focus again
				screenCtrl.Engine.Pause ();
			}
		}

		public override void OnActivated(UIApplication application)
		{
			// TODO: Delete
			Console.WriteLine ("OnActivated");

			if (screenCtrl != null && screenCtrl.Engine != null && screenCtrl.Engine.GameState == WF.Player.Core.Engines.EngineGameState.Paused) {
				// Resume engine, so we continue
				screenCtrl.Engine.Resume ();
			}
		}

		public override void ReceiveMemoryWarning (UIApplication application)
		{
			// TODO: Delete
			Console.WriteLine ("ReceiveMemoryWarning");

			// Save game before we could get killed
			if (screenCtrl != null && screenCtrl.Engine != null) {
				// Free memory
				screenCtrl.Engine.FreeMemory ();
				// Save game automatically
				screenCtrl.Engine.Save (new FileStream (screenCtrl.Engine.Cartridge.SaveFilename, FileMode.Create));
			}
		}

		[CLSCompliantAttribute(false)]
		public void CartStart(Cartridge cart)
		{
			Start (cart, false);
		}

		[CLSCompliantAttribute(false)]
		public void CartRestore(Cartridge cart)
		{
			Start (cart, true);
		}

		public void CartStop()
		{
			if (screenCtrl != null) {
				window.RootViewController = navCartSelect;

				screenCtrl.Dispose ();
				screenCtrl = null;
			}

			UIApplication.SharedApplication.IdleTimerDisabled = false;
		}

		#region Private Functions

		void DefaultsChanged(NSNotification obj)
		{
			//			_label.Text = NSUserDefaults.StandardUserDefaults.StringForKey("Server_IP_Adress");
		}

		void Start(Cartridge cart, Boolean restore = false)
		{
			UIApplication.SharedApplication.IdleTimerDisabled = true;

			// Create main screen handler
			screenCtrl = new ScreenController(this, cart, restore);

			// Set as new navigation controll
			window.RootViewController = screenCtrl;
		}

		#endregion
	}
}