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
using System.IO;
using System.Text;
using System.Timers;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using MonoTouch.CoreLocation;
using MonoTouch.AVFoundation;
using WF.Player.Core;

namespace WF.Player.iPhone
{
	public class ScreenController : UINavigationController
	{
		private AppDelegate appDelegate;
		private AVAudioPlayer soundPlayer; 
		private Cartridge cartridge;
		private Engine engine;
		private MainScreen mainScreen;
		private ItemScreen itemScreen;
		private DetailScreen detailScreen;
		private CLLocationManager locationManager;
		private bool animation = false;
		private TextWriter logFile;
		private LogLevel logLevel = LogLevel.Error;
		private int zoomLevel = 16;

		public ScreenType activeScreen;
		public bool Transitioning;

		public ScreenController (AppDelegate appDelegate)
		{
			// Save for later use
			this.appDelegate = appDelegate;

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

			// Create mainScreen
			mainScreen = new MainScreen(this);

			mainScreen.NavigationItem.SetLeftBarButtonItem(new UIBarButtonItem(@"Quit",UIBarButtonItemStyle.Plain, (sender,args) => { quit (); }), true);
			mainScreen.NavigationItem.SetRightBarButtonItem(new UIBarButtonItem(@"Menu",UIBarButtonItemStyle.Plain, (sender,args) => { menu (); }), true);

			// Creat itemScreen
			itemScreen = new ItemScreen(this);

			itemScreen.NavigationItem.SetLeftBarButtonItem(new UIBarButtonItem(@"Back",UIBarButtonItemStyle.Plain, (sender,args) => { back (); }), true);

			Delegate = new ScreenControllerDelegate();

			// ... and push it to the UINavigationController 
			PushViewController (mainScreen,animation);
			// Now start the gps
		}

		public Cartridge Cartridge { get { return cartridge; } }

		public Engine Engine { 
			get { 
				return engine; 
			} 
			set { 
				if (engine != value) {
					// Remove old engine
					if (engine != null) {
						engine.InputRequested -= OnGetInput;
						engine.LogMessageRequested -= OnLogMessage;
						engine.NotifyOS -= OnNotifyOS;
						engine.PlayMediaRequested -= OnPlayMedia;
						engine.ShowMessageBoxRequested -= OnShowMessageBox;
						engine.ShowScreenRequested -= OnShowScreen;
						engine.ShowStatusTextRequested -= OnShowStatusText;
						engine.SynchronizeRequested -= OnSynchronize;
					}
					// Set new engine
					engine = value;
					// Add all events
					engine.InputRequested += OnGetInput;
					engine.LogMessageRequested += OnLogMessage;
					engine.NotifyOS += OnNotifyOS;
					engine.PlayMediaRequested += OnPlayMedia;
					engine.ShowMessageBoxRequested += OnShowMessageBox;
					engine.ShowScreenRequested += OnShowScreen;
					engine.ShowStatusTextRequested += OnShowStatusText;
					engine.SynchronizeRequested += OnSynchronize;
				}
				if (engine != null) {
					cartridge = engine.Cartridge; 
					Title = cartridge.Name; 
				}
			} 
		}

		public int ZoomLevel { get { return zoomLevel; } set { if (zoomLevel != value) zoomLevel = value; } }
		
		public void Refresh ()
		{
			this.InvokeOnMainThread (refresh);
		}

		private void refresh ()
		{
			if (VisibleViewController == mainScreen) {
				if (mainScreen.Table != null) {
					mainScreen.Table.ReloadData();
				}
			}
			if (VisibleViewController == itemScreen) {
				if (itemScreen.Table != null) {
					itemScreen.UpdateData(activeScreen);
					itemScreen.Table.ReloadData();
				}
			}
			if (VisibleViewController == detailScreen)
				ShowScreen (ScreenType.Details,detailScreen.Item);
		}
		
		public void Start()
		{
			
		}

		public void End()
		{
			
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
			engine.Save(new FileStream(cartridge.SaveFilename,FileMode.Create));
			indicatorView.StopAnimating();
		}

		public void ShowError (string msg)
		{
			this.InvokeOnMainThread( () => { showError(msg); } );
		}

		private void showError (string msg)
		{
		}
		
		public void DebugMsg (string msg)
		{
			this.InvokeOnMainThread( () => { debugMsg(msg); } );
		}

		private void debugMsg (string msg)
		{
		}

		#region Events

		public void OnGetInput (Object sender, ObjectEventArgs<Input> input)
		{
			this.InvokeOnMainThread( () => { getInput(input.Object); } );
		}

		private void getInput (Input input)
		{
			ShowScreen (ScreenType.Dialog, input);
		}

		public void OnLogMessage ( Object sender, LogMessageEventArgs args )
		{
			this.InvokeOnMainThread( () => { logMessage (args.Level, args.Message); } );
		}

		private void logMessage ( LogLevel level, string text )
		{
			if (logFile == null)
				logFile = new StreamWriter(cartridge.LogFilename,true,Encoding.UTF8);

			if (level <= logLevel)
				logFile.WriteLine(engine.CreateLogMessage(text));

			// TODO: wieder rausnehmen
			#if DEBUG
				Console.WriteLine (text);
			#endif
		}

		public void OnNotifyOS(Object sender, NotifyOSEventArgs args)
		{
			this.InvokeOnMainThread( () => { notifyOS(args.Command); } );
		}

		private void notifyOS(string command)
		{
			// TODO
			switch (command.ToLower()) {
				case "stopsound":
					StopSound();
					break;
			}
		}

		public void OnPlayMedia(Object sender, ObjectEventArgs<Media> mediaObj)
		{
			this.InvokeOnMainThread( () => { playMedia(mediaObj.Object); } );
		}

		private void playMedia(Media mediaObj)
		{
			NSError error;
			soundPlayer = AVAudioPlayer.FromData(NSData.FromArray (mediaObj.Data), out error);
			if (soundPlayer != null)
				soundPlayer.Play ();
			else
				logMessage (LogLevel.Error,String.Format ("Audio file format of media {0} is not valid",mediaObj.Name));
		}

		public void OnShowMessageBox(Object sender, MessageBoxEventArgs args)
		{
			this.InvokeOnMainThread( () => { showMessageBox(args); } );
		}

		private void showMessageBox(MessageBoxEventArgs args)
		{
			ShowScreen (ScreenType.Dialog, args.Descriptor);
		}

		public void OnShowScreen (Object sender, ScreenEventArgs args)
		{
			this.InvokeOnMainThread( () => { ShowScreen(args.Screen, args.Object); } );
		}

		public void ShowScreen (ScreenType screenId, object param = null)
		{
			activeScreen = screenId;

			// If there is a old DialogScreen active, remove it
			if (VisibleViewController is DialogScreen) {
				PopViewControllerAnimated (animation);
				if (VisibleViewController is MainScreen)
					mainScreen.Table.ReloadData ();
				if (VisibleViewController is ItemScreen)
					itemScreen.Table.ReloadData ();
				// Ensure, that screen is updated
				NSRunLoop.Current.RunUntil(DateTime.Now);
			}

			if (screenId == ScreenType.Main)
			{
				this.NavigationItem.SetHidesBackButton(false, animation);
				PopToRootViewController(animation);
				mainScreen.Table.ReloadData();
				// Ensure, that screen is updated
				NSRunLoop.Current.RunUntil(DateTime.Now);
			}
			if (screenId == ScreenType.Locations || screenId == ScreenType.Items || screenId == ScreenType.Inventory || screenId == ScreenType.Tasks)
			{
				if (VisibleViewController != mainScreen)
					PopToRootViewController(animation);
				if (itemScreen.UpdateData(screenId)) {
					this.NavigationItem.SetHidesBackButton(false, animation);
					PushViewController (itemScreen,animation);
					itemScreen.Table.ReloadData();
					// Ensure, that screen is updated
					NSRunLoop.Current.RunUntil(DateTime.Now);
				}
			}
			if (screenId == ScreenType.Details)
			{
				int idxDetail = (int) param;
				if (idxDetail == -1)
					return;
				UIObject detail = (UIObject)engine.GetObject(idxDetail);
				if (VisibleViewController is DetailScreen)
					PopViewControllerAnimated(animation);
				// Create new ViewController
				detailScreen = new DetailScreen(this,detail);
				detailScreen.NavigationItem.SetLeftBarButtonItem(new UIBarButtonItem(@"Back",UIBarButtonItemStyle.Plain, (sender,args) => { back (); }), true);
				if (detail is Zone || (detail is Thing && engine.VisibleObjects.Contains((Thing)detail)))
					detailScreen.NavigationItem.SetRightBarButtonItem(new UIBarButtonItem(@"Map",UIBarButtonItemStyle.Plain, (sender,args) => { map ((Thing)detail); }), true);
				PushViewController (detailScreen,animation);
				// Ensure, that screen is updated
				NSRunLoop.Current.RunUntil(DateTime.Now);
			}
			if (screenId == ScreenType.Dialog)
			{
				if (param == null)
					return;
				DialogScreen dialogScreen;
				if (param is MessageBox)
					dialogScreen = new DialogScreen(this, (MessageBox)param);
				else
					dialogScreen = new DialogScreen(this, (Input)param);
				this.NavigationItem.SetHidesBackButton(true, animation);
				PushViewController (dialogScreen,animation);
				// Ensure, that screen is updated
				NSRunLoop.Current.RunUntil(DateTime.Now);
			}
		}

		public void RemoveScreen(ScreenType last)
		{
			PopViewControllerAnimated(animation);
			Refresh();

			// TODO: Set right new screen
			ShowScreen (ScreenType.Dialog, null);
		}

		public void StopSound ()
		{
			this.InvokeOnMainThread( () => { stopSound (); } );
		}

		private void stopSound ()
		{
			if (soundPlayer != null && soundPlayer.Playing) {
				soundPlayer.Stop ();
			}
		}

		public void OnShowStatusText(Object sender, StatusTextEventArgs args)
		{
			this.InvokeOnMainThread( () => { showStatusText (args.Text); } );
		}

		private void showStatusText(string text)
		{
		}

		public void OnSynchronize (Object sender, SynchronizeEventArgs args )
		{
			this.InvokeOnMainThread( () => { args.Tick(); } );
		}

		#endregion
		
		public void ZoneStateChanged(List<Zone> zones)
		{
			this.InvokeOnMainThread( () => { refresh (); } );;
		}

		public void InventoryChanged(Thing obj,Thing fromContainer,Thing toContainer)
		{
			this.InvokeOnMainThread( () => { refresh (); } );;
		}

		public void CartridgeChanged(string type)
		{
			if (type.ToLower ().Equals("sync"))
			{
				Engine.Sync (new FileStream(Engine.Cartridge.SaveFilename, FileMode.Create));
			}
			else
				this.InvokeOnMainThread( () => { refresh (); } );;
		}

		public void CommandChanged(Command obj)
		{
			this.InvokeOnMainThread( () => { refresh (); } );;
		}

		public void AttributeChanged(Table obj, string type)
		{
			this.InvokeOnMainThread( () => { refresh (); } );;
		}

		public string GetDevice()
		{
			return "iPhone";	
		}

		public string GetDeviceId()
		{
			// Use MAC Adress of en0 as DeviceId
			foreach (var i in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces ())
				if (i.Id.Equals ("en0")) 
					return i.GetPhysicalAddress ().ToString ();
			return "No Id";	
		}

		public string GetVersion()
		{
			return "0.1.0";	
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
					engine.Save(new FileStream(cartridge.SaveFilename,FileMode.Create)); 
				// Close log file
				locationManager.StopUpdatingLocation();
				appDelegate.CartStop();
				if (logFile != null)
					logFile.Close();
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
					alert.Message = String.Format ("Copyright 2012-2013 by Wherigo Foundation, Dirk Weltz\n\nVersion\niPhone {0}\nCore {1}\n\nUsed parts of following products (copyrights see at product):\nGroundspeak, NLua, KeraLua, KopiLua, Lua ",GetVersion(),Engine.CoreVersion); 
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
			MapScreen mapScreen = new MapScreen(this,thing);
			mapScreen.NavigationItem.SetLeftBarButtonItem(new UIBarButtonItem(@"Back",UIBarButtonItemStyle.Plain, (sender,args) => { back (); }), true);
			mapScreen.Title = thing.GetString ("Name");
			PushViewController (mapScreen,animation);
		}

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

