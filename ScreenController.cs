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
using System.Drawing;
using System.IO;
using System.Text;
using System.Timers;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using MonoTouch.CoreLocation;
using MonoTouch.AVFoundation;
using WF.Player.Core;
using WF.Player.Core.Engines;

namespace WF.Player.iPhone
{
	[CLSCompliantAttribute(false)]
	public class ScreenController : UINavigationController, IController
	{
		private AppDelegate appDelegate;
		private AVAudioPlayer soundPlayer; 
		private Cartridge cart;
		private Engine engine;
		private ScreenMain screenMain;
		private ScreenList screenList;
		private ScreenDetail screenDetail;
		private CLLocationManager locationManager;
		private bool animation = false;
		private TextWriter logFile;
		private LogLevel logLevel = LogLevel.Error;
		private int zoomLevel = 16;

		public ScreenType activeScreen;
		public UIObject activeObject;
		public bool Transitioning;

		public ScreenController (AppDelegate appDelegate, Cartridge cart)
		{
			// Save for later use
			this.appDelegate = appDelegate;
			this.cart = cart;

			// Set color of NavigationBar and NavigationButtons (TintColor)
			NavigationBar.SetBackgroundImage (new UIImage(), UIBarMetrics.Default);
			NavigationBar.BackgroundColor = UIColor.FromRGB(0.1992f,0.7070f,0.8945f);
			NavigationBar.TintColor = UIColor.FromRGB(1f,0f,0f);

						// Create Location Manager
			locationManager = new CLLocationManager();
			locationManager.Delegate = new LocationManagerDelegate(this);
			//locationManager.DistanceFilter = 5;
			locationManager.DistanceFilter = 2.0;
			locationManager.HeadingFilter = 5.0;
			locationManager.DesiredAccuracy = CLLocation.AccurracyBestForNavigation;
			if (CLLocationManager.LocationServicesEnabled)
			{
				locationManager.StartUpdatingLocation ();
				locationManager.StartUpdatingHeading ();
			}

				//locationManager.StartMonitoringSignificantLocationChanges();

			// Create Engine
			CreateEngine (cart);

			// Create screenMain
			screenMain = new ScreenMain(this);

			screenMain.NavigationItem.SetLeftBarButtonItem(new UIBarButtonItem(@"Quit",UIBarButtonItemStyle.Plain, (sender,args) => { quit (); }), true);
			screenMain.NavigationItem.SetRightBarButtonItem(new UIBarButtonItem(@"Menu",UIBarButtonItemStyle.Plain, (sender,args) => { menu (); }), true);

			Delegate = new ScreenControllerDelegate();

			// ... and push it to the UINavigationController 
			PushViewController (screenMain, animation);

			Title = cart.Name; 
		}

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

		public int ZoomLevel { get { return zoomLevel; } set { if (zoomLevel != value) zoomLevel = value; } }
		
		public void Refresh ()
		{
			if (VisibleViewController == screenMain) {
				if (screenMain.Table != null) {
					screenMain.Table.ReloadData();
				}
			}
			if (VisibleViewController == screenList) {
				if (screenList.Table != null) {
//					screenList.UpdateData(activeScreen);
					screenList.Table.ReloadData();
				}
			}
			if (VisibleViewController == screenDetail)
				ShowScreen (ScreenType.Details,screenDetail.Item);
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
			engine.NotifyOS += OnNotifyOS;
			engine.PlayMediaRequested += OnPlayMedia;
			engine.SaveRequested += OnSaveCartridge;
			engine.ShowMessageBoxRequested += OnShowMessageBox;
			engine.ShowScreenRequested += OnShowScreen;
			engine.ShowStatusTextRequested += OnShowStatusText;

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
			StopSound ();

			if (engine != null) {
				engine.Stop();

				engine.AttributeChanged -= OnAttributeChanged;
				engine.InventoryChanged -= OnInventoryChanged;
				engine.ZoneStateChanged -= OnZoneStateChanged;
				engine.CartridgeCompleted -= OnCartridgeComplete;
				engine.InputRequested -= OnGetInput;
				engine.LogMessageRequested -= OnLogMessage;
				engine.NotifyOS -= OnNotifyOS;
				engine.PlayMediaRequested -= OnPlayMedia;
				engine.SaveRequested -= OnSaveCartridge;
				engine.ShowMessageBoxRequested -= OnShowMessageBox;
				engine.ShowScreenRequested -= OnShowScreen;
				engine.ShowStatusTextRequested -= OnShowStatusText;

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
		public void OnCartridgeComplete (object sender, CartridgeEventArgs args)
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
			ScreenDialog dialogScreen = new ScreenDialog(input.Object);
			this.NavigationItem.SetHidesBackButton(true, animation);
			PushViewController (dialogScreen,animation);
			// Ensure, that screen is updated
//			NSRunLoop.Current.RunUntil(DateTime.Now);
		}

		[CLSCompliantAttribute(false)]
		public void OnLogMessage ( Object sender, LogMessageEventArgs args )
		{
			logMessage (args.Level, args.Message);
		}

		[CLSCompliantAttribute(false)]
		public void OnNotifyOS(Object sender, NotifyOSEventArgs args)
		{
			// TODO
			switch (args.Command) {
				case "StopSound":
					StopSound();
					break;
				case "SaveClose":
					engine.Save (new FileStream (cart.SaveFilename, FileMode.Create));
					DestroyEngine ();
					// Close log file
					locationManager.StopUpdatingLocation();
					appDelegate.CartStop();
					break;
				case "DriveTo":
					// TODO: Implement
					break;
				case "Alert":
					// TODO: Implement
					break;
			}
		}

		[CLSCompliantAttribute(false)]
		public void OnPlayMedia(Object sender, ObjectEventArgs<Media> mediaObj)
		{
			StartSound (mediaObj.Object);
		}

		[CLSCompliantAttribute(false)]
		public void OnSaveCartridge (object sender, CartridgeEventArgs args)
		{
			engine.Save (new FileStream (args.Cartridge.SaveFilename, FileMode.Create));
		}

		[CLSCompliantAttribute(false)]
		public void OnShowMessageBox(Object sender, MessageBoxEventArgs args)
		{
			ScreenDialog dialogScreen = new ScreenDialog(args.Descriptor);
			this.NavigationItem.SetHidesBackButton(true, animation);
			PushViewController (dialogScreen,animation);
			// Ensure, that screen is updated
//			NSRunLoop.Current.RunUntil(DateTime.Now);
		}

		[CLSCompliantAttribute(false)]
		public void OnShowScreen (Object sender, ScreenEventArgs args)
		{
			ShowScreen(args.Screen, args.Object);
		}

		[CLSCompliantAttribute(false)]
		public void OnShowStatusText(Object sender, StatusTextEventArgs args)
		{
		}

		#endregion

		#region Helper Functions

		[CLSCompliantAttribute(false)]
		public void RemoveScreen(ScreenType last)
		{
			PopViewControllerAnimated(animation);

			switch (last) {
				//				case ScreenType.Main:
				//					// ToDo: Main screen is the last screen to show, so stop the cartridge
				//					ShowScreen (ScreenType.Main, null);
				//					break;
				//				case ScreenType.Locations:
				//				case ScreenType.Items:
				//				case ScreenType.Inventory:
				//				case ScreenType.Tasks:
				//					ShowScreen (ScreenType.Main, null);
				//					break;
				case ScreenType.Details:
				// Show correct list for this zone/item/character/task
				if (activeObject != null) {
					// Remove active list from screen
					PopViewControllerAnimated(animation);
					// Select the correct list to show
					UIObject obj = activeObject;
					activeObject = null;
					if (obj is Zone)
						ShowScreen (ScreenType.Locations, null);
					if (obj is Task)
						ShowScreen (ScreenType.Tasks, null);
					if (obj is Item || obj is Character) {
						if (engine.IsInInventory ((Thing)obj))
							ShowScreen (ScreenType.Inventory, null);
						else
							ShowScreen (ScreenType.Items, null);
					}
				} else
					ShowScreen (ScreenType.Main, null);
				break;
				case ScreenType.Dialog:
				// Which screen to show
				if (activeScreen == ScreenType.Details && activeObject != null && !activeObject.Visible)
					RemoveScreen (ScreenType.Details);
				break;
			}
		}

		[CLSCompliantAttribute(false)]
		public void ShowScreen (ScreenType screenId, object param = null)
		{
			// If there is a old DialogScreen active, remove it
			if (VisibleViewController is ScreenDialog) {
				PopViewControllerAnimated (animation);
				if (VisibleViewController is ScreenMain)
					screenMain.Table.ReloadData ();
				if (VisibleViewController is ScreenList)
					screenList.Table.ReloadData ();
				// Ensure, that screen is updated
				NSRunLoop.Current.RunUntil(DateTime.Now);
			}

			if (screenId == ScreenType.Main)
			{
				this.NavigationItem.SetHidesBackButton(false, animation);
				PopToRootViewController(animation);
				screenMain.Table.ReloadData();
				// Ensure, that screen is updated
				NSRunLoop.Current.RunUntil(DateTime.Now);
			}
			if (screenId == ScreenType.Locations || screenId == ScreenType.Items || screenId == ScreenType.Inventory || screenId == ScreenType.Tasks)
			{
				if (VisibleViewController != screenMain)
					PopToRootViewController(animation);
				screenList = new ScreenList (this, screenId);
//				if (screenList.UpdateData(screenId)) {
					this.NavigationItem.SetHidesBackButton(false, animation);
					PushViewController (screenList,animation);
//					screenList.Table.ReloadData();
					// Ensure, that screen is updated
//					NSRunLoop.Current.RunUntil(DateTime.Now);
//				}
			}
			if (screenId == ScreenType.Details)
			{
				if (activeScreen != ScreenType.Details || activeObject != (UIObject)param) {
					activeObject = (UIObject)param;
					if (VisibleViewController is ScreenDetail)
						PopViewControllerAnimated (animation);
					// Create new ViewController
					screenDetail = new ScreenDetail (this, activeObject);
					screenDetail.NavigationItem.SetLeftBarButtonItem (new UIBarButtonItem (@"Back", UIBarButtonItemStyle.Plain, (sender, args) => {
						back ();
					}), true);
					if (activeObject is Zone || (activeObject is Thing && engine.VisibleObjects.Contains ((Thing)activeObject)))
						screenDetail.NavigationItem.SetRightBarButtonItem (new UIBarButtonItem (@"Map", UIBarButtonItemStyle.Plain, (sender, args) => {
							map ((Thing)activeObject);
						}), true);
					PushViewController (screenDetail, animation);
					// Ensure, that screen is updated
					NSRunLoop.Current.RunUntil (DateTime.Now);
				}
			}

			activeScreen = screenId;
		}

		[CLSCompliantAttribute(false)]
		public void StartSound(Media media)
		{
			NSError error;
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
			alert.Title = "Quit"; 
			alert.Message = "Would you save before quit?"; 
			alert.AddButton("Yes"); 
			alert.AddButton("No"); 
			alert.Clicked += (sender, e) => { 
				if (e.ButtonIndex == 0) 
					engine.Save(new FileStream(cart.SaveFilename,FileMode.Create)); 
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
			actionSheet.AddButton("Save");
			actionSheet.AddButton("About");
			actionSheet.AddButton ("Cancel");
			actionSheet.DestructiveButtonIndex = 0;  // Red button
			actionSheet.CancelButtonIndex = 2;       // Black button
			actionSheet.Clicked += delegate(object a, UIButtonEventArgs b) {
				if (b.ButtonIndex == 0)
					Save ();
				if (b.ButtonIndex == 1)
				{
					var alert = new UIAlertView(); 
					alert.Title = "WF.Player.iPhone"; 
					alert.Message = String.Format ("Copyright 2012-2013 by Wherigo Foundation, Dirk Weltz\n\nVersion\niPhone {0}\nCore {1}\n\nUsed parts of following products (copyrights see at product):\nGroundspeak, NLua, KeraLua, KopiLua, Lua ",0,Engine.CoreVersion); 
					alert.AddButton("Ok"); 
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
			mapScreen.NavigationItem.SetLeftBarButtonItem(new UIBarButtonItem(@"Back",UIBarButtonItemStyle.Plain, (sender,args) => { back (); }), true);
			mapScreen.Title = thing.Name;
			PushViewController (mapScreen,animation);
		}

		#endregion

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
				ctrl.Engine.RefreshLocation(newLocation.Coordinate.Latitude, newLocation.Coordinate.Longitude, newLocation.Altitude, newLocation.HorizontalAccuracy);
			}

			public override void UpdatedHeading(CLLocationManager manager, CLHeading newHeading)
			{
				if (ctrl.Engine.Heading != newHeading.TrueHeading)
				{
					ctrl.Engine.RefreshHeading(newHeading.TrueHeading);
					ctrl.Refresh();
				}
			}

			public override void Failed (CLLocationManager manager, NSError error)
			{
				Console.WriteLine("Failed to find location");
				base.Failed (manager, error);
			}
		}

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

