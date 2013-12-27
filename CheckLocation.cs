using System;
using System.Drawing;
using MonoTouch.CoreGraphics;
using MonoTouch.CoreLocation;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace WF.Player.iOS
{
	[CLSCompliantAttribute(false)]
	public class CheckLocation : UIViewController
	{
		string formatAccuracy;
		UILabel textDescription;
		UILabel textCoordinates;
		UILabel textAccuracy;
		UIButton button;
		ScreenController ctrl;
		CLLocationManager locationManager;

		public CheckLocation (ScreenController sc, CLLocationManager lm) : base()
		{
			ctrl = sc;
			locationManager = lm;

			// OS specific details
			if (new Version (UIDevice.CurrentDevice.SystemVersion) >= new Version(7,0)) 
			{
				// Code that uses features from Xamarin.iOS 7.0
				this.EdgesForExtendedLayout = UIRectEdge.None;
			}

			lm.Delegate = new LocationManagerDelegate (this);
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			// Perform any additional setup after loading the view, typically from a nib.
			Title = Strings.GetString("GPS Check");

			var leftBarButton = new UIBarButtonItem (Strings.GetString("Quit"), UIBarButtonItemStyle.Plain, (sender, args) => {
				quit ();
			});
			leftBarButton.TintColor = Colors.NavBarButton;
			NavigationItem.SetLeftBarButtonItem(leftBarButton, true);

			CreateViews ();
		}

		public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation) 
		{
			base.DidRotate(fromInterfaceOrientation);

			Refresh(null);
		}

		public void CreateViews()
		{
			View.BackgroundColor = UIColor.White;

			textDescription = new UILabel () {
				Text = Strings.GetString("For much fun with the cartridge, you should wait for a good accuracy of your GPS signal."),
				Frame = new RectangleF (Values.Frame, Values.Frame, View.Bounds.Width - 2f * Values.Frame, 999999),
				BackgroundColor = UIColor.Clear,
				Lines = 0,
				LineBreakMode = UILineBreakMode.WordWrap,
				TextAlignment = UITextAlignment.Center
			};

			textCoordinates = new UILabel () {
				Text = Strings.GetStringFmt("Current Coordinates\n{0}", "N 00° 00.000   E 000° 00.000"),
				Frame = new RectangleF (Values.Frame, Values.Frame, View.Bounds.Width - 2f * Values.Frame, 999999),
				BackgroundColor = UIColor.Clear,
				Lines = 2,
				LineBreakMode = UILineBreakMode.WordWrap,
				TextAlignment = UITextAlignment.Center
			};

			textAccuracy = new UILabel () {
				Text = Strings.GetStringFmt("Current Accuracy\n{0} m", Strings.Infinite),
				Frame = new RectangleF (Values.Frame, Values.Frame, View.Bounds.Width - 2f * Values.Frame, 999999),
				BackgroundColor = UIColor.Clear,
				Lines = 2,
				LineBreakMode = UILineBreakMode.WordWrap,
				TextAlignment = UITextAlignment.Center
			};

			formatAccuracy = Strings.GetString("Current Accuracy\n{0} m");

			button = UIButton.FromType (UIButtonType.RoundedRect);
			button.Frame = new RectangleF (Values.Frame, View.Bounds.Height - Values.ButtonHeight - Values.Frame, View.Bounds.Width - 2f * Values.Frame, Values.ButtonHeight);
			button.SetTitle(Strings.GetString("Start anyway"),UIControlState.Normal);
			button.SetTitleColor(Colors.ButtonText,UIControlState.Normal);
			button.HorizontalAlignment = UIControlContentHorizontalAlignment.Center;
			button.AutoresizingMask = UIViewAutoresizing.FlexibleTopMargin;
			button.SetBackgroundImage(Images.OrangeButton, UIControlState.Normal);
			button.SetBackgroundImage(Images.OrangeButtonHighlight, UIControlState.Highlighted);
			button.TouchUpInside += delegate(object sender, EventArgs e) {
				start ();
			};

			View.AddSubview (textDescription);
			View.AddSubview (textCoordinates);
			View.AddSubview (textAccuracy);
			View.AddSubview (button);

			Refresh (null);
		}

		public void Refresh(CLLocation loc)
		{
			if (loc != null) {
				textCoordinates.Text = Strings.GetStringFmt("Current Coordinates\n{0}", coordinatesToString (loc.Coordinate.Latitude, loc.Coordinate.Longitude));
				textAccuracy.Text = String.Format(formatAccuracy,Convert.ToInt32(Math.Floor(loc.HorizontalAccuracy)));
				if (loc.HorizontalAccuracy > 20.0) {
					button.SetTitle (Strings.GetString("Start anyway"),UIControlState.Normal);
					button.SetBackgroundImage(Images.OrangeButton, UIControlState.Normal);
					button.SetBackgroundImage(Images.OrangeButtonHighlight, UIControlState.Highlighted);
				} else {
					button.SetTitle (Strings.GetString("Start"),UIControlState.Normal);
					button.SetBackgroundImage(Images.GreenButton, UIControlState.Normal);
					button.SetBackgroundImage(Images.GreenButtonHighlight, UIControlState.Highlighted);
				}
			}

			float height = Values.Frame;

			// Do this, because of wrong values after rotating
			textDescription.Frame = new RectangleF (Values.Frame, Values.Frame, View.Bounds.Width - 2f * Values.Frame, 999999);
			textDescription.SizeToFit ();
			textDescription.Frame = new RectangleF (Values.Frame, height, View.Bounds.Width - 2f * Values.Frame, textDescription.Bounds.Height);

			height += textDescription.Bounds.Height + Values.Frame;

			textCoordinates.Frame = new RectangleF (Values.Frame, Values.Frame, View.Bounds.Width - 2f * Values.Frame, 999999);
			textCoordinates.SizeToFit ();
			textCoordinates.Frame = new RectangleF (Values.Frame, height, View.Bounds.Width - 2f * Values.Frame, textCoordinates.Bounds.Height);

			height += textCoordinates.Bounds.Height + Values.Frame;

			textAccuracy.Frame = new RectangleF (Values.Frame, Values.Frame, View.Bounds.Width - 2f * Values.Frame, 999999);
			textAccuracy.SizeToFit ();
			textAccuracy.Frame = new RectangleF (Values.Frame, height, View.Bounds.Width - 2f * Values.Frame, textAccuracy.Bounds.Height);

			button.Frame = new RectangleF (Values.Frame, View.Bounds.Height - Values.Frame - Values.ButtonHeight, View.Bounds.Width - 2f * Values.Frame, Values.ButtonHeight);
		}

		#region Private Functions

		void start()
		{
			ctrl.InitController(false);
		}

		void quit()
		{
			ctrl.InitController(true);
		}

		string coordinatesToString(double lat, double lon)
		{
			string latDirect;
			string lonDirect;
			int latDegrees;
			int lonDegrees;
			int latMin;
			int lonMin;
			int latSec;
			int lonSec;
			double latDecimalMin;
			double lonDecimalMin;

			latDirect = lat > 0 ? Strings.GetString("N") : Strings.GetString("S");
			lonDirect = lon > 0 ? Strings.GetString("E") : Strings.GetString("W");

			latDegrees = Convert.ToInt32 (Math.Floor(lat));
			lonDegrees = Convert.ToInt32 (Math.Floor(lon));

			latMin = Convert.ToInt32 (Math.Floor((lat - latDegrees) * 60.0));
			lonMin = Convert.ToInt32 (Math.Floor((lon - lonDegrees) * 60.0));

			latSec = Convert.ToInt32 (Math.Floor((((lat - latDegrees) * 60.0) - latMin) * 60.0));
			lonSec = Convert.ToInt32 (Math.Floor((((lon - lonDegrees) * 60.0) - lonMin) * 60.0));

			latDecimalMin = Math.Round((lat - latDegrees) * 60.0, 3);
			lonDecimalMin = Math.Round((lon - lonDegrees) * 60.0, 3);

			return String.Format("{0} {1:00}° {2:00.000}'   {3} {4:000}° {5:00.000}'", new object[] {latDirect, latDegrees, latDecimalMin, lonDirect, lonDegrees, lonDecimalMin});
		}

		#endregion
	
		#region Location Manager Delegate

		/// <summary>
		/// MonoTouch definition seemed to work without too much trouble
		/// </summary>
		private class LocationManagerDelegate: CLLocationManagerDelegate
		{

			private CheckLocation ctrl;

			public LocationManagerDelegate(CheckLocation ctrl)
			{
				this.ctrl = ctrl;
			}

			/// <summary>
			/// Whenever the GPS sends a new location. 
			/// </summary>
			public override void LocationsUpdated (CLLocationManager manager, CLLocation[] locations)
			{	
				if (locations.Length > 0)
					ctrl.Refresh(locations[0]);
			}

			public override void UpdatedHeading(CLLocationManager manager, CLHeading newHeading)
			{
			}

			public override void Failed (CLLocationManager manager, NSError error)
			{
				Console.WriteLine("Failed to find location");
				// TODO: Do nothing, if there is no signal or start a timer, which says after a short time, that the signal is lost.
				// base.Failed (manager, error);
			}
		}

		#endregion

	}

}

