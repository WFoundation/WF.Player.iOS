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
	public class ScreenController : UINavigationController, IUserInterface
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
		private int zoomLevel = 16;

		public int ScreenType;
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
				if (engine != value) 
					engine = value; 
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
					itemScreen.UpdateData();
					itemScreen.Table.ReloadData();
				}
			}
			if (VisibleViewController == detailScreen)
				ShowScreen (engine.DETAILSCREEN,detailScreen.Item.GetInt ("ObjIndex"));
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

		public void NotifyOS(string command)
		{
			this.InvokeOnMainThread( () => { notifyOS(command); } );
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

		public void MessageBox(string text, Media mediaObj, string btn1Label, string btn2Label, CallbackFunction wrapper)
		{
			this.InvokeOnMainThread( () => { messageBox(text, mediaObj, btn1Label, btn2Label, wrapper); } );
		}

		private void messageBox(string text, Media mediaObj, string btn1Label, string btn2Label, CallbackFunction wrapper)
		{
			MessageEntry entry;

			entry = new MessageEntry ();
			entry.Text = text;
			if (mediaObj != null) 
				entry.Image = UIImage.LoadFromData (NSData.FromArray (mediaObj.Data));
			entry.Buttons.Add (btn1Label);
			if (!btn2Label.Equals(""))
				entry.Buttons.Add (btn2Label);
			entry.Type = MessageEntry.sqeMessage;
			entry.Callback = wrapper;

			Screen (engine.DIALOGSCREEN, entry);
		}

		public void GetInput (Input input)
		{
			this.InvokeOnMainThread( () => { getInput(input); } );
		}

		private void getInput (Input input)
		{
			MessageEntry entry = new MessageEntry ();
			entry.Input = input;
			entry.Title = input.Name;
			entry.Text = input.Text;
			if (input.Image != null)
				entry.Image = UIImage.LoadFromData (NSData.FromArray (input.Image.Data));
			if (input.InputType.ToLower ().Equals ("text")) {
				entry.Type = MessageEntry.sqeInput;
				entry.Edit = "";
				entry.Buttons.Add ("OK");
			} else {
				entry.Type = MessageEntry.sqeChoice;
				foreach(string c in input.Choices)
					entry.Buttons.Add (c);
			}
			entry.Callback = input.Callback;
			Screen (engine.DIALOGSCREEN, entry);
		}

		public void ShowScreen (int screenId, int idxObject = -1)
		{
			this.InvokeOnMainThread( () => { Screen(screenId,idxObject); } );
		}

		public void Screen (int screenId, object param = null)
		{
			ScreenType = screenId;

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

			if (screenId == engine.MAINSCREEN)
			{
				this.NavigationItem.SetHidesBackButton(false, animation);
				PopToRootViewController(animation);
				mainScreen.Table.ReloadData();
				// Ensure, that screen is updated
				NSRunLoop.Current.RunUntil(DateTime.Now);
			}
			if (screenId == engine.LOCATIONSCREEN || screenId == engine.ITEMSCREEN || screenId == engine.INVENTORYSCREEN || screenId == engine.TASKSCREEN)
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
			if (screenId == engine.DETAILSCREEN)
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
				if (engine.IsZone(detail) || (engine.IsThing(detail) && engine.VisibleObjects.Contains((Thing)detail)))
					detailScreen.NavigationItem.SetRightBarButtonItem(new UIBarButtonItem(@"Map",UIBarButtonItemStyle.Plain, (sender,args) => { map ((Thing)detail); }), true);
				PushViewController (detailScreen,animation);
				// Ensure, that screen is updated
				NSRunLoop.Current.RunUntil(DateTime.Now);
			}
			if (screenId == engine.DIALOGSCREEN)
			{
				if (param == null)
					return;
				MessageEntry entry = (MessageEntry) param;
				DialogScreen dialogScreen = new DialogScreen(this, entry);
				this.NavigationItem.SetHidesBackButton(true, animation);
				PushViewController (dialogScreen,animation);
				// Ensure, that screen is updated
				NSRunLoop.Current.RunUntil(DateTime.Now);
			}
		}

		public void DialogCallback (MessageEntry entry, int button)
		{
			// TODO
			// Remove basic lua access
			Screen (engine.DIALOGSCREEN, null);
			if (entry.Type == MessageEntry.sqeInput)
				entry.Callback(entry.Edit);
			else if (entry.Type == MessageEntry.sqeChoice)
				entry.Callback(entry.Buttons[button]);
			else if (entry.Type == MessageEntry.sqeMessage && entry.Callback != null)
				entry.Callback(String.Format ("Button{0}",button+1));
		}

		public void MediaEvent(int type, Media mediaObj)
		{
			this.InvokeOnMainThread( () => { mediaEvent(type, mediaObj); } );
		}

		private void mediaEvent(int type, Media mediaObj)
		{
			// TODO
			// Remove basic lua access
			NSError error;
			soundPlayer = AVAudioPlayer.FromData(NSData.FromArray (mediaObj.Data), out error);
			if (soundPlayer != null)
				soundPlayer.Play ();
			else
				LogMessage (4,String.Format ("Audio file format of media {0} is not valid",mediaObj.Name));
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

		public void LogMessage ( int level, string text )
		{
			this.InvokeOnMainThread( () => { logMessage (level, text); } );
		}

		private void logMessage ( int level, string text )
		{
			if (logFile == null)
				logFile = new StreamWriter(cartridge.LogFilename,true,Encoding.UTF8);
      
      		StringBuilder logText = new StringBuilder();

      		// Add date, position and other things to log line
      		logText.AppendFormat("{0:yyy-mm-dd hh:mm:ss}|", DateTime.Now);
      		logText.AppendFormat("{0:0.000000}|{1:0.000000}|{2:0.0}|{3:0.0}|", engine.Latitude, engine.Longitude, engine.Altitude, engine.Accuracy);
      		logText.Append(text);
      
      		// Write to log file
      		logFile.WriteLine(logText);

			// TODO: wieder rausnehmen
			#if DEBUG
				Console.WriteLine (text);
			#endif
		}

		public void ShowStatusText(string text)
		{
			this.InvokeOnMainThread( () => { showStatusText (text); } );
		}

		private void showStatusText(string text)
		{
		}
		
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

		public void Syncronize ( SyncronizeTick Tick, object source )
		{
			this.InvokeOnMainThread( () => { Tick(source); } );
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
					alert.Message = String.Format ("Copyright 2012-2013 by Wherigo Foundation, Dirk Weltz\n\nVersion\niPhone {0}\nCore {1}\n\nUsed parts of following products (copyrights see at product):\nGroundspeak, NLua, KeraLua, KopiLua, SharpLua, Lua ",GetVersion(),Engine.CoreVersion); 
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
			PopViewControllerAnimated(animation);
			Refresh();
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

