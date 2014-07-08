///
/// WF.Player.iPhone - A Wherigo Player for iPhone which use the Wherigo Foundation Core.
/// Copyright (C) 2012-2014  Dirk Weltz <web@weltz-online.de>
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
using System.Drawing;
using System.IO;
using System.Text;
using System.Timers;
using MonoTouch.AudioToolbox;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using MonoTouch.CoreLocation;
using MonoTouch.AVFoundation;
using Vernacular;
using WF.Player.Core;
using WF.Player.Core.Engines;

namespace WF.Player.iOS
{
	[CLSCompliantAttribute(false)]
	public class ScreenController : UINavigationController, IController
	{
		AppDelegate appDelegate;
		AVAudioPlayer soundPlayer; 
		Cartridge cart;
		Engine engine;
		CheckLocation checkLocation;
		ScreenMain screenMain;
		ScreenList screenListLocations;
		ScreenList screenListItems;
		ScreenList screenListInventory;
		ScreenList screenListTasks;
		ScreenDetail screenDetail;
		CLLocationManager locationManager;
		bool animation = false;
		bool restore = false;
		TextWriter logFile;
		LogLevel logLevel = LogLevel.Error;
		int zoomLevel = 16;

		public ScreenType activeScreen;
		public UIObject activeObject;
		public bool Transitioning;

		public ScreenController (AppDelegate appDelegate, Cartridge cart, Boolean restore)
		{
			// Save for later use
			this.appDelegate = appDelegate;
			this.cart = cart;
			this.restore = restore;

			// Set color of NavigationBar and NavigationButtons (TintColor)
			if (new Version (UIDevice.CurrentDevice.SystemVersion) >= new Version(7,0)) 
				NavigationBar.SetBackgroundImage (Images.BlueTop, UIBarMetrics.Default);
			else
				NavigationBar.SetBackgroundImage (Images.Blue, UIBarMetrics.Default);

			// Create Location Manager
			locationManager = new CLLocationManager ();
			locationManager.DesiredAccuracy = CLLocation.AccurracyBestForNavigation;
			if (CLLocationManager.LocationServicesEnabled) {
				locationManager.StartUpdatingLocation ();
			}

			// Now check, if location is accurate enough
			checkLocation = new CheckLocation(this, locationManager);
			PushViewController (checkLocation, true);
		}

		public void InitController(Boolean stop)
		{
			if (stop) {
				// Check location was aborted with quit
				locationManager.StopUpdatingLocation ();
				DestroyEngine ();
				appDelegate.CartStop ();

				return;
			}

			locationManager.StopUpdatingLocation ();
			locationManager.DistanceFilter = 2.0;
			locationManager.HeadingFilter = 5.0;
			locationManager.Delegate = new LocationManagerDelegate (this);
			locationManager.StartUpdatingLocation ();
			locationManager.StartUpdatingHeading ();

			// Create Engine
			CreateEngine (cart);

			// Create screens
			screenMain = new ScreenMain(this);

			// Set left button
			var leftBarButton = new UIBarButtonItem (Catalog.GetString("Quit"), UIBarButtonItemStyle.Plain, (sender, args) => {
				ButtonPressed(null);
				quit();
			});
			leftBarButton.TintColor = Colors.NavBarButton;
			screenMain.NavigationItem.SetLeftBarButtonItem(leftBarButton, true);

			// Set right button
			var rightBarButton = new UIBarButtonItem (Catalog.GetString("Save"), UIBarButtonItemStyle.Plain, (sender, args) => {
				ButtonPressed(null);
				Save();
			});
			rightBarButton.TintColor = Colors.NavBarButton;
			screenMain.NavigationItem.SetRightBarButtonItem(rightBarButton, true);

			screenListLocations = new ScreenList (this, ScreenType.Locations); 
			screenListItems = new ScreenList (this, ScreenType.Items); 
			screenListInventory = new ScreenList (this, ScreenType.Inventory); 
			screenListTasks = new ScreenList (this, ScreenType.Tasks); 

			Delegate = new ScreenControllerDelegate();

			// ... and push it to the UINavigationController while replacing the CheckLocation
			SetViewControllers (new UIViewController[] { screenMain }, animation);

			Title = cart.Name; 

			if (restore)
				Restore();
			else
				Start ();
		}

		#region Properties

		public Cartridge Cartridge 
		{ 
			get 
			{ 
				return cart; 
			} 
		}

		public Engine Engine 
		{ 
			get 
			{ 
				return engine; 
			} 
			internal set
			{
				if (engine != value)
					engine = value;
			} 
		}

		public CLLocationManager LocatitionManager {
			get { return locationManager; }
		}

		#endregion

		public void Refresh ()
		{
			if (VisibleViewController == screenMain) {
				if (screenMain.Table != null) {
					screenMain.Table.ReloadData();
				}
			}

			if (VisibleViewController == screenListLocations)
				screenListLocations.Refresh (true);

			if (VisibleViewController == screenListItems)
				screenListItems.Refresh (true);

			if (VisibleViewController == screenListInventory)
				screenListInventory.Refresh (true);

			if (VisibleViewController == screenListTasks)
				screenListTasks.Refresh (true);

			if (VisibleViewController is ScreenDetail)
				((ScreenDetail)VisibleViewController).Refresh();
		}

		#region Engine Handling

		public void CreateEngine (Cartridge cart)
		{
			if (engine != null)
				DestroyEngine ();

			var helper = new iOSPlatformHelper ();
			helper.Ctrl = this;

			engine = new Engine (helper);

			// Set all events for engine
			engine.AttributeChanged += OnAttributeChanged;
			engine.InventoryChanged += OnInventoryChanged;
			engine.ZoneStateChanged += OnZoneStateChanged;
			engine.CartridgeCompleted += OnCartridgeComplete;
			engine.InputRequested += OnGetInput;
			engine.LogMessageRequested += OnLogMessage;
			engine.PlayAlertRequested += OnPlayAlert;
			engine.PlayMediaRequested += OnPlayMedia;
			engine.SaveRequested += OnSaveCartridge;
			engine.ShowMessageBoxRequested += OnShowMessageBox;
			engine.ShowScreenRequested += OnShowScreen;
			engine.ShowStatusTextRequested += OnShowStatusText;
			engine.StopSoundsRequested += OnStopSound;

			// If there is a old logFile, close it
			if (logFile != null) {
				logFile.Flush ();
				logFile.Close ();
			}

			// Open logFile first time
			logFile = new StreamWriter(cart.LogFilename, true, System.Text.Encoding.UTF8);

			engine.Init (new FileStream (cart.Filename,FileMode.Open), cart);
		}

		public void DestroyEngine()
		{
			if (engine != null) {
				engine.Stop();
				engine.Reset ();

				engine.AttributeChanged -= OnAttributeChanged;
				engine.InventoryChanged -= OnInventoryChanged;
				engine.ZoneStateChanged -= OnZoneStateChanged;
				engine.CartridgeCompleted -= OnCartridgeComplete;
				engine.InputRequested -= OnGetInput;
				engine.LogMessageRequested -= OnLogMessage;
				engine.PlayAlertRequested -= OnPlayAlert;
				engine.PlayMediaRequested -= OnPlayMedia;
				engine.SaveRequested -= OnSaveCartridge;
				engine.ShowMessageBoxRequested -= OnShowMessageBox;
				engine.ShowScreenRequested -= OnShowScreen;
				engine.ShowStatusTextRequested -= OnShowStatusText;
				engine.StopSoundsRequested -= OnStopSound;

				engine.Dispose ();

				engine = null;
			}

			// If there is a old logFile, close it
			if (logFile != null) {
				logFile.Flush ();
				logFile.Close ();
			}
		}

		public void Pause()
		{
			if (engine != null)
				engine.Pause ();
		}

		public void Restore ()
		{
			if (engine != null) 
			{
				// TODO: Insert locationManager values
				engine.RefreshLocation (0,0,0,0);
				engine.Restore (new FileStream(cart.SaveFilename,FileMode.Open));
				Refresh ();
			}
		}

		public void Resume()
		{
			if (engine != null)
			{
				engine.RefreshLocation (0,0,0,0);
				engine.Resume ();
//				Refresh ();
			}
		}

		public void Start()
		{
			if (engine != null) 
			{
				// TODO: Insert locationManager values
				engine.RefreshLocation (0,0,0,0);
				engine.Start ();
				Refresh ();
			}
		}

		public void Save()
		{
			var indicatorView = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.Gray);
			indicatorView.HidesWhenStopped = true;
			var width  = (this.VisibleViewController.View.Frame.Width-20)/2;
			var height = (this.VisibleViewController.View.Frame.Height-20)/2;
			indicatorView.Frame = new RectangleF(width, height,20,20);
			this.VisibleViewController.View.AddSubview(indicatorView);
			indicatorView.StartAnimating();

			engine.Save(new FileStream(cart.SaveFilename,FileMode.Create));

			indicatorView.StopAnimating();
		}

		#endregion

		#region Events

		[CLSCompliantAttribute(false)]
		public void OnCartridgeComplete (object sender, WherigoEventArgs args)
		{
			// TODO: Implement
			// throw new NotImplementedException ();
		}

		[CLSCompliantAttribute(false)]
		public void OnAttributeChanged(Object sender, AttributeChangedEventArgs e)
		{
			// The easiest way is to redraw all screens
			if (engine != null)
				Refresh ();
		}

		[CLSCompliantAttribute(false)]
		public void OnInventoryChanged(Object sender, InventoryChangedEventArgs e)
		{
			// The easiest way is to redraw all screens
			if (engine != null)
				Refresh ();
		}

		[CLSCompliantAttribute(false)]
		public void OnZoneStateChanged(Object sender, ZoneStateChangedEventArgs e)
		{
			// The easiest way is to redraw all screens
			if (engine != null)
				Refresh ();
		}

		[CLSCompliantAttribute(false)]
		public void OnGetInput (Object sender, ObjectEventArgs<Input> input)
		{
			ShowScreen (ScreenType.Dialog, input.Object);
		}

		[CLSCompliantAttribute(false)]
		public void OnLogMessage ( Object sender, LogMessageEventArgs args )
		{
			logMessage (args.Level, args.Message);
		}

		void OnPlayAlert (object sender, WherigoEventArgs e)
		{
			Sounds.Alarm.PlayAlertSound ();
			SystemSound.Vibrate.PlaySystemSound ();
		}

		[CLSCompliantAttribute(false)]
		public void OnPlayMedia(Object sender, ObjectEventArgs<Media> mediaObj)
		{
			StartSound (mediaObj.Object);
		}

		[CLSCompliantAttribute(false)]
		public void OnSaveCartridge (object sender, SavingEventArgs args)
		{
			Save ();

			if (args.CloseAfterSave) {
				// Close log file
				locationManager.StopUpdatingLocation ();
				DestroyEngine ();
				appDelegate.CartStop ();
			}
		}

		[CLSCompliantAttribute(false)]
		public void OnShowMessageBox(Object sender, MessageBoxEventArgs args)
		{
			ShowScreen (ScreenType.Dialog, args);
		}

		[CLSCompliantAttribute(false)]
		public void OnShowScreen (Object sender, ScreenEventArgs args)
		{
			ShowScreen((ScreenType)args.Screen, args.Object);
		}

		[CLSCompliantAttribute(false)]
		public void OnShowStatusText(Object sender, StatusTextEventArgs args)
		{
		}

		[CLSCompliantAttribute(false)]
		public void OnStopSound(Object sender, WherigoEventArgs args)
		{
			StopSound ();
		}

		#endregion

		#region Helper Functions

		/// <summary>
		/// Buttons the pressed, so give the user the right feedback.
		/// </summary>
		/// <param name="button">Button, which is pressed.</param>
		public void ButtonPressed(UIButton button)
		{
			if (NSUserDefaults.StandardUserDefaults.BoolForKey("ButtonClick"))
				Sounds.KeyboardClick.PlaySystemSound ();
			if (NSUserDefaults.StandardUserDefaults.BoolForKey("ButtonVibrate"))
				SystemSound.Vibrate.PlaySystemSound ();
		}

		[CLSCompliantAttribute(false)]
		public void RemoveScreen(ScreenType type)
		{
			bool remove = true;
			ScreenType activeType = ScreenType.Main;

			// Get active screen type 
			if (ViewControllers [0] is ScreenMain)
				activeType = ScreenType.Main;
			if (ViewControllers [0] is ScreenList)
				activeType = ((ScreenList)ViewControllers [0]).Type;
			if (ViewControllers [0] is ScreenDetail)
				activeType = ScreenType.Details;
			if (ViewControllers [0] is ScreenDialog)
				activeType = ScreenType.Dialog;
			if (ViewControllers [0] is ScreenMap)
				activeType = ScreenType.Map;

			// Check if screen to remove is active screen, instead leave
			if (type != null) {
				if (ViewControllers [0] is ScreenList)
					remove &= ((ScreenList)ViewControllers [0]).Type == type;
				if (ViewControllers [0] is ScreenDetail)
					remove &= type == ScreenType.Details;
				if (ViewControllers [0] is ScreenDialog)
					remove &= type == ScreenType.Dialog;
				if (ViewControllers [0] is ScreenMap)
					remove &= type == ScreenType.Map;
			}

			if (!remove)
				return;

			switch (activeType) {
			case ScreenType.Main:
					// Don't remove the main screen
				break;
			case ScreenType.Locations:
				ShowScreen (ScreenType.Main, null);
				break;
			case ScreenType.Items:
				ShowScreen (ScreenType.Main, null);
				break;
			case ScreenType.Inventory:
				ShowScreen (ScreenType.Main, null);
				break;
			case ScreenType.Tasks:
				ShowScreen (ScreenType.Main, null);
				break;
			case ScreenType.Details:
					// Show correct list for this zone/item/character/task
				if (((ScreenDetail)ViewControllers [0]).ActiveObject != null) {
					// Select the correct list to show
					UIObject obj = ((ScreenDetail)ViewControllers [0]).ActiveObject;
					activeObject = null;
					if (obj is Zone)
						ShowScreen (ScreenType.Locations, null);
					if (obj is Task)
						ShowScreen (ScreenType.Tasks, null);
					if (obj is Item || obj is Character) {
						if (engine.VisibleInventory.Contains ((Thing)obj))
							ShowScreen (ScreenType.Inventory, null);
						else
							ShowScreen (ScreenType.Items, null);
					}
				} else
					ShowScreen (ScreenType.Main, null);
				break;
			case ScreenType.Dialog:
			case ScreenType.Map:
				if (activeScreen == ScreenType.Details && activeObject != null && !activeObject.Visible) {
					// Object for detail screen is no longer visible, so show correct list
					// Select the correct list to show
					UIObject obj = activeObject;
					activeObject = null;
					if (obj is Zone)
						ShowScreen (ScreenType.Locations, null);
					if (obj is Task)
						ShowScreen (ScreenType.Tasks, null);
					if (obj is Item || obj is Character) {
						if (engine.VisibleInventory.Contains ((Thing)obj))
							ShowScreen (ScreenType.Inventory, null);
						else
							ShowScreen (ScreenType.Items, null);
					}
				} else {
					ShowScreen (activeScreen, activeObject);
				}
				break;
			}
		}

		[CLSCompliantAttribute(false)]
		public void ShowScreen (ScreenType screenId, object param = null)
		{
			switch (screenId) {
			case ScreenType.Main:
				ViewControllers = new UIViewController[] { screenMain };
				break;
			case ScreenType.Locations:
				ViewControllers = new UIViewController[] { screenListLocations };
				break;
			case ScreenType.Items:
				ViewControllers = new UIViewController[] { screenListItems };
				break;
			case ScreenType.Inventory:
				ViewControllers = new UIViewController[] { screenListInventory };
				break;
			case ScreenType.Tasks:
				ViewControllers = new UIViewController[] { screenListTasks };
				break;
			case ScreenType.Details:
					// Is active ViewController is ScreenDetail
				if (!(VisibleViewController is ScreenDetail))
						// Active ViewController isn't ScreenDetail, so create a new one
						ViewControllers = new UIViewController[] { new ScreenDetail (this, (UIObject)param) };
				else
					((ScreenDetail)ViewControllers [0]).ActiveObject = (UIObject)param;
				break;
			case ScreenType.Dialog:
				if (param is MessageBoxEventArgs) {
					ViewControllers = new UIViewController[] { new ScreenDialog (((MessageBoxEventArgs)param).Descriptor) };
				}
				if (param is Input) {
					ViewControllers = new UIViewController[] { new ScreenDialog ((Input)param) };
				}
				break;
			case ScreenType.Map:
				ViewControllers = new UIViewController[] { new ScreenMap(this, (Thing)param) };
				break;
			}

			if (screenId != ScreenType.Dialog && screenId != ScreenType.Map) {
				activeScreen = screenId;
				activeObject = (UIObject)param;
			}
		}

		[CLSCompliantAttribute(false)]
		public void StartSound(Media media)
		{
			NSError error;
			if (soundPlayer != null) {
				soundPlayer.Stop();
				soundPlayer = null;
			}
			soundPlayer = AVAudioPlayer.FromData(NSData.FromArray (media.Data), out error);
			if (soundPlayer != null)
				soundPlayer.Play ();
			else
				logMessage (LogLevel.Error,String.Format ("Audio file format of media {0} is not valid",media.Name));
		}

		public void StopSound ()
		{
			if (soundPlayer != null && soundPlayer.Playing) {
				soundPlayer.Stop ();
				soundPlayer = null;
			}
		}

		#endregion

		#region Private Functions

		private void logMessage(LogLevel level, string message)
		{
			if (logFile == null)
				logFile = new StreamWriter(cart.LogFilename,true,Encoding.UTF8);

			if (level <= logLevel)
			{
				logFile.WriteLine(engine.CreateLogMessage(message));
				logFile.Flush ();
			}

			// TODO: wieder rausnehmen
			#if DEBUG
			Console.WriteLine (message);
			#endif
		}
		
		private void quit ()
		{
			// Ask, if user wants to save game
			var alert = new UIAlertView(); 
			alert.Title = Catalog.GetString("Quit"); 
			alert.Message = Catalog.GetString("Would you save before quit?"); 
			alert.AddButton(Catalog.GetString("Yes")); 
			alert.AddButton(Catalog.GetString("No")); 
			alert.AddButton(Catalog.GetString("Cancel")); 
			alert.Clicked += (sender, e) => { 
				if (e.ButtonIndex == 2)
					return;
				if (e.ButtonIndex == 0) 
					Save();
				// Close log file
				locationManager.StopUpdatingLocation();
				DestroyEngine();
				appDelegate.CartStop();
			};
			alert.Show();
		}

		private void menu ()
		{
			UIActionSheet actionSheet = new UIActionSheet ();
			actionSheet.AddButton(Catalog.GetString("Save"));
			actionSheet.AddButton(Catalog.GetString("About"));
			actionSheet.AddButton (Catalog.GetString("Cancel"));
			actionSheet.DestructiveButtonIndex = 0;  // Red button
			actionSheet.CancelButtonIndex = 2;       // Black button
			actionSheet.Clicked += delegate(object a, UIButtonEventArgs b) {
				if (b.ButtonIndex == 0)
					Save ();
				if (b.ButtonIndex == 1)
				{
					var alert = new UIAlertView(); 
					alert.Title = Catalog.GetString("WF.Player.iOS"); 
					alert.Message = Catalog.Format(Catalog.GetString("Copyright 2012-2013 by Wherigo Foundation, Dirk Weltz, Brice Clocher\n\nVersion\niPhone {0}\nCore {1}\n\nUsed parts of following products (copyrights see at product):\nGroundspeak, NLua, KeraLua, KopiLua, Lua "),0,Engine.CORE_VERSION); 
					alert.AddButton(Catalog.GetString("Ok")); 
//					alert.Clicked += (sender, e) => {
//					};
					alert.Show();
				}
			};
			actionSheet.ShowInView (View);
		}

		private void back ()
		{
			RemoveScreen (activeScreen);
		}

		private void map (Thing thing)
		{
			ScreenMap mapScreen = new ScreenMap(this,thing);
			mapScreen.NavigationItem.SetLeftBarButtonItem(new UIBarButtonItem(Catalog.GetString("Back"),UIBarButtonItemStyle.Plain, (sender,args) => { back (); }), true);
			mapScreen.Title = thing.Name;
			PushViewController (mapScreen,animation);
		}

		#endregion

		#region Location Manager Delegate

		/// <summary>
		/// MonoTouch definition seemed to work without too much trouble
		/// </summary>
		private class LocationManagerDelegate: CLLocationManagerDelegate
		{

			private ScreenController ctrl;

			public LocationManagerDelegate(ScreenController ctrl)
			{
				this.ctrl = ctrl;
			}

			/// <summary>
			/// Whenever the GPS sends a new location. 
			/// </summary>
			public override void UpdatedLocation (CLLocationManager manager, CLLocation newLocation, CLLocation oldLocation)
			{
				if (ctrl != null && ctrl.Engine != null && ctrl.Engine.GameState == EngineGameState.Playing)
					ctrl.Engine.RefreshLocation(newLocation.Coordinate.Latitude, newLocation.Coordinate.Longitude, newLocation.Altitude, newLocation.HorizontalAccuracy);
			}

			public override void UpdatedHeading(CLLocationManager manager, CLHeading newHeading)
			{
//				if (ctrl != null && ctrl.Engine != null && ctrl.Engine.GameState == EngineGameState.Playing)
//					if (ctrl.Engine.Heading != newHeading.TrueHeading)
//					{
//						ctrl.Engine.RefreshHeading(newHeading.TrueHeading);
//						ctrl.Refresh();
//					}
			}

			public override void Failed (CLLocationManager manager, NSError error)
			{
				Console.WriteLine("Failed to find location");
				// TODO: Do nothing, if there is no signal or start a timer, which says after a short time, that the signal is lost.
				// base.Failed (manager, error);
			}
		}

		#endregion

		private class ScreenControllerDelegate : UINavigationControllerDelegate
		{
			public override void WillShowViewController (UINavigationController navigationController, UIViewController viewController, bool animated)
			{
				((ScreenController)navigationController).Transitioning = true;
			}

			public override void DidShowViewController (UINavigationController navigationController, UIViewController viewController, bool animated)
			{
				((ScreenController)navigationController).Transitioning = false;
			}
		}

	}

}

