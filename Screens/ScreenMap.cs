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
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.CoreLocation;
using MonoTouch.CoreGraphics;
using Google.Maps;
using WF.Player.Core;
using WF.Player.Core.Engines;

namespace WF.Player.iOS
{
	public class ScreenMap : UIViewController
	{
		float zoom = 16f;
		Engine engine;
		ScreenController ctrl;
		Thing thing;
		MapView mapView;
		UIButton btnCenter;
		UIButton btnOrientation;
		UIButton btnMapType;
		Dictionary<int,Overlay> overlays = new Dictionary<int, Overlay> ();
		Dictionary<int,Marker> markers = new Dictionary<int, Marker> ();
		string[] properties = {"Name", "Icon", "Active", "Visible", "ObjectLocation"};

		
		public ScreenMap (ScreenController ctrl, Thing t)
		{
			this.ctrl = ctrl;
			this.engine = ctrl.Engine;
			this.thing = t;

			// OS specific details
			if (new Version (UIDevice.CurrentDevice.SystemVersion) >= new Version(7,0)) 
			{
				// Code that uses features from Xamarin.iOS 7.0
				this.EdgesForExtendedLayout = UIRectEdge.None;
			}
		}
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			// Get zoom factor
			zoom = NSUserDefaults.StandardUserDefaults.FloatForKey("MapZoom");

			if (zoom == 0f)
				zoom = 16f;

			// Create camera position
			CameraPosition camera;

			if (thing != null && thing.ObjectLocation != null)
				camera = CameraPosition.FromCamera (thing.ObjectLocation.Latitude, thing.ObjectLocation.Longitude, zoom);
			else
				camera = CameraPosition.FromCamera (engine.Latitude, engine.Longitude, zoom);

			// Init MapView
			mapView = MapView.FromCamera (RectangleF.Empty, camera);
			mapView.MyLocationEnabled = true;
			mapView.SizeToFit ();
			mapView.AutoresizingMask = UIViewAutoresizing.All;
			mapView.Frame = new RectangleF (0, 0, View.Frame.Width, View.Frame.Height);
			mapView.MyLocationEnabled = true;
			mapView.Settings.CompassButton = false;
			mapView.Settings.MyLocationButton = false;
			mapView.Settings.RotateGestures = false;
			mapView.Settings.TiltGestures = false;

			mapView.TappedOverlay += OnTappedOverlay;
			mapView.TappedInfo += OnTappedInfo;

			View.AddSubview(mapView);

			btnCenter = UIButton.FromType (UIButtonType.RoundedRect);
			btnCenter.Tag = 1;
			btnCenter.Frame = new RectangleF (12f, 12f, 36f, 36f);
			btnCenter.TintColor = UIColor.White;
			btnCenter.SetBackgroundImage(Images.BlueButton, UIControlState.Normal);
			btnCenter.SetBackgroundImage(Images.BlueButtonHighlight, UIControlState.Highlighted);
			btnCenter.SetImage (Images.ButtonCenter, UIControlState.Normal);
			btnCenter.ContentMode = UIViewContentMode.Center;
			btnCenter.TouchUpInside += OnTouchUpInside;

			View.AddSubview (btnCenter);

			btnOrientation = UIButton.FromType (UIButtonType.RoundedRect);
			btnOrientation.Tag = 2;
			btnOrientation.Frame = new RectangleF (12f, 61f, 36f, 36f);
			btnOrientation.TintColor = UIColor.White;
			btnOrientation.SetBackgroundImage(Images.BlueButton, UIControlState.Normal);
			btnOrientation.SetBackgroundImage(Images.BlueButtonHighlight, UIControlState.Highlighted);
			btnOrientation.SetImage (Images.ButtonOrientation, UIControlState.Normal);
			btnOrientation.ContentMode = UIViewContentMode.Center;
			btnOrientation.TouchUpInside += OnTouchUpInside;

			View.AddSubview (btnOrientation);

			btnMapType = UIButton.FromType (UIButtonType.RoundedRect);
			btnMapType.Tag = 3;
			btnMapType.Frame = new RectangleF (mapView.Frame.Width - 12f - 36f, 12f, 36f, 36f);
			btnMapType.AutoresizingMask = UIViewAutoresizing.FlexibleLeftMargin | UIViewAutoresizing.FlexibleBottomMargin;
			btnMapType.TintColor = UIColor.White;
			btnMapType.SetBackgroundImage(Images.BlueButton, UIControlState.Normal);
			btnMapType.SetBackgroundImage(Images.BlueButtonHighlight, UIControlState.Highlighted);
			btnMapType.SetImage (Images.ButtonMapType, UIControlState.Normal);
			btnMapType.ContentMode = UIViewContentMode.Center;
			btnMapType.TouchUpInside += OnTouchUpInside;

			View.AddSubview (btnMapType);

			Refresh ();
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			mapView.StartRendering ();
			StartEvents ();
		}

		public override void ViewWillDisappear (bool animated)
		{	
			StopEvents ();
			mapView.StopRendering ();

			// Save zoom factor
			NSUserDefaults.StandardUserDefaults.SetFloat(mapView.Camera.Zoom, "MapZoom");

			base.ViewWillDisappear (animated);
		}

		public void Refresh(Thing thing = null)
		{
			if (thing != null) {
				// Only one thing needs an update

			} else {
				// All things must be updated

				// All zones
				WherigoCollection<Zone> zones = ctrl.Engine.ActiveVisibleZones;

				foreach (Zone z in zones)
					CreateZone (z);

				// All items
				WherigoCollection<Thing> things = ctrl.Engine.VisibleObjects;

				foreach (Thing t in things)
					CreateThing(t);//					createThing (t);
			}
		}

		#region Private Functions

		void OnTappedOverlay (object sender, GMSOverlayEventEventArgs e)
		{
			var objIndex = overlays.FirstOrDefault(x => x.Value == e.Overlay).Key;
		}

		void OnTappedInfo (object sender, GMSMarkerEventEventArgs e)
		{
			var obj = engine.GetWherigoObject<Thing> (markers.FirstOrDefault (x => x.Value == e.Marker).Key);

			if (ctrl.activeScreen == ScreenType.Details && ctrl.activeObject == obj) {
				ctrl.RemoveScreen (ScreenType.Details);
				ctrl.ShowScreen(ScreenType.Details, obj); 
			} else
				ctrl.ShowScreen(ScreenType.Details, obj); 
		}

		void OnTouchUpInside (object sender, EventArgs e)
		{
			if (sender is UIButton && ((UIButton)sender).Tag == 1) {
				if (thing == null) {
					// No thing, so show location immediatly
				} else {
					// Ask, which to show
					UIActionSheet actionSheet = new UIActionSheet (Strings.GetString ("Focus on"));
					actionSheet.AddButton (thing.Name);
					actionSheet.AddButton (Strings.GetString ("Playing area"));
					actionSheet.AddButton (Strings.GetString ("Location"));
					actionSheet.AddButton (Strings.GetString ("Cancel"));
					actionSheet.CancelButtonIndex = 3;       // Black button
					actionSheet.Clicked += delegate(object a, UIButtonEventArgs b) {
						CameraUpdate cu = null;
						if (b.ButtonIndex == 0) {
							// Location of thing is selected and thing is a zone
							if (thing is Zone) {
								WherigoCollection<ZonePoint> points = ((Zone)thing).Points;
								double lat1 = points[0].Latitude;
								double lon1 = points[0].Longitude;
								double lat2 = points[0].Latitude;
								double lon2 = points[0].Longitude;
								foreach(ZonePoint zp in points) {
									lat1 = zp.Latitude < lat1 ? zp.Latitude : lat1;
									lon1 = zp.Longitude < lon1 ? zp.Longitude : lon1;
									lat2 = zp.Latitude > lat2 ? zp.Latitude : lat2;
									lon2 = zp.Longitude > lon2 ? zp.Longitude : lon2;
								}
//								cu = CameraUpdate.FitBounds(new CoordinateBounds(new CLLocationCoordinate2D(lat1, lon1),new CLLocationCoordinate2D(lat2, lon2)),30f);
								cu = CameraUpdate.SetTarget(new CLLocationCoordinate2D(lat1+(lat2-lat1)/2.0, lon1+(lon2-lon1)/2.0));
							} else {
								// Location of thing is selected and thing is no zone
								if (thing.ObjectLocation != null) {
									cu = CameraUpdate.SetTarget(new CLLocationCoordinate2D(thing.ObjectLocation.Latitude,thing.ObjectLocation.Longitude));
								}
							}
						}
						if (b.ButtonIndex == 2) {
							// Location of player is selected
							cu = CameraUpdate.SetTarget(new CLLocationCoordinate2D(engine.Latitude,engine.Longitude));
						}
						if (cu != null)
							mapView.MoveCamera(cu);
					};
					actionSheet.ShowInView (View);
				}
			}
			if  (sender is UIButton && ((UIButton)sender).Tag == 2) {
				// Check, if north should be on top
				if (((UIButton)sender).CurrentImage == Images.ButtonOrientation) {
					((UIButton)sender).SetImage (Images.ButtonOrientationNorth, UIControlState.Normal);
				} else {
					((UIButton)sender).SetImage (Images.ButtonOrientation, UIControlState.Normal);
				}
				Console.WriteLine ("BtnOrientation touched");
			}
			if  (sender is UIButton && ((UIButton)sender).Tag == 3) {
				// Change map type
				// Ask, which to show
				UIActionSheet actionSheet = new UIActionSheet (Strings.GetString ("Type of map"));
				actionSheet.AddButton ("Google Maps");
				actionSheet.AddButton (Strings.GetString ("Google Satellite"));
				actionSheet.AddButton (Strings.GetString ("Google Terrain"));
				actionSheet.AddButton (Strings.GetString ("Google Hybrid"));
				actionSheet.AddButton (Strings.GetString ("None"));
				actionSheet.AddButton (Strings.GetString ("Cancel"));
				actionSheet.CancelButtonIndex = 5;       // Black button
				actionSheet.Clicked += delegate(object a, UIButtonEventArgs b) {
					switch (b.ButtonIndex) {
					case 0:
						mapView.MapType = MapViewType.Normal;
						break;
					case 1:
						mapView.MapType = MapViewType.Satellite;
						break;
					case 2:
						mapView.MapType = MapViewType.Terrain;
						break;
					case 3:
						mapView.MapType = MapViewType.Hybrid;
						break;
					case 4:
						mapView.MapType = MapViewType.None;
						break;
					}
				};
				actionSheet.ShowInView (View);
			}
		}

		void StartEvents()
		{
			engine.AttributeChanged += OnAttributeChanged;
			engine.InventoryChanged += OnPropertyChanged;
			engine.ZoneStateChanged += OnZoneStateChanged;

			engine.PropertyChanged += OnPropertyChanged;
		}

		void StopEvents()
		{
			engine.AttributeChanged -= OnAttributeChanged;
			engine.InventoryChanged -= OnPropertyChanged;
			engine.ZoneStateChanged -= OnZoneStateChanged;

			engine.PropertyChanged -= OnPropertyChanged;
		}

		public void OnPropertyChanged(object sender,  EventArgs e)
		{
			bool newItems = false;

			newItems |= e is InventoryChangedEventArgs;
			newItems |= e is AttributeChangedEventArgs && ((AttributeChangedEventArgs)e).PropertyName.Equals("Active");
			newItems |= e is AttributeChangedEventArgs && ((AttributeChangedEventArgs)e).PropertyName.Equals("Visible");
			newItems |= e is PropertyChangedEventArgs && ((PropertyChangedEventArgs)e).PropertyName.Equals("Active");
			newItems |= e is PropertyChangedEventArgs && ((PropertyChangedEventArgs)e).PropertyName.Equals("Visible");

			// Check, if one of the visible entries changed
//			if (!(e is PropertyChangedEventArgs) || (e is PropertyChangedEventArgs && properties.Contains(((PropertyChangedEventArgs)e).PropertyName)))
//				Refresh(((PropertyChangedEventArgs)e).e.newItems);
		}

		public void OnAttributeChanged(object sender,  AttributeChangedEventArgs e)
		{
			if (properties.Contains(e.PropertyName)) {
				if (e.Object is Zone)
					CreateZone(e.Object as Zone);
			}
		}

		public void OnZoneStateChanged(object sender,  ZoneStateChangedEventArgs e)
		{
			foreach (Zone z in e.Zones) {
				if (z.Active && z.Visible) {
					CreateZone (z);
				} else {
					if (overlays.ContainsKey (z.ObjIndex)) {
						Overlay polygon;
						overlays.TryGetValue (z.ObjIndex, out polygon);
						if (polygon != null) 
							polygon.Map = null;
						overlays.Remove (z.ObjIndex);
						Marker marker;
						markers.TryGetValue (z.ObjIndex, out marker);
						if (marker != null)
							marker.Map = null;
						markers.Remove (z.ObjIndex);
					}
				}
			}
		}

		void CreateThing (Thing t)
		{
			Marker marker;

			// If the thing don't have a ObjectLocation, than don't draw it
			if (t.ObjectLocation == null)
				return;

			if (!markers.TryGetValue (t.ObjIndex, out marker)) {
				marker = new Marker () {
					Tappable = true,
					Icon = (t.Icon != null ? UIImage.LoadFromData (NSData.FromArray (t.Icon.Data)) : Images.IconMapZone),
					GroundAnchor = t.Icon != null ? new PointF(0.5f, 0.5f) : new PointF(0.5f, 1.0f),
					Map = mapView
				};
				markers.Add(t.ObjIndex, marker);
			}

			marker.Title = t.Name;
			((Marker)marker).Position = new CLLocationCoordinate2D(t.ObjectLocation.Latitude, t.ObjectLocation.Longitude);
		}

		void CreateZone(Zone z)
		{
			Overlay polygon;
			Marker marker;

			if (!overlays.TryGetValue(z.ObjIndex, out polygon)) {
				polygon = new Polygon () {
					FillColor = Colors.ZoneFill,
					StrokeColor = Colors.ZoneStroke,
					StrokeWidth = 2,
					Tappable = true,
					Map = mapView
				};
				overlays.Add (z.ObjIndex, polygon);
			}

			if (!markers.TryGetValue(z.ObjIndex, out marker)) {
				marker = new Marker () {
					Tappable = true,
					Icon = (z.Icon != null ? UIImage.LoadFromData (NSData.FromArray (z.Icon.Data)) : Images.IconMapZone),
					GroundAnchor = z.Icon != null ? new PointF(0.5f, 0.5f) : new PointF(0.5f, 1.0f),
					Map = mapView
				};
				markers.Add (z.ObjIndex, marker);
			}

			polygon.Title = z.Name;
			marker.Title = z.Name;

			MutablePath path = new MutablePath ();;
			WherigoCollection<ZonePoint> points = z.Points;

			double lat = 0;
			double lon = 0;

			foreach (ZonePoint zp in points) {
				lat += zp.Latitude;
				lon += zp.Longitude;
				path.AddLatLon (zp.Latitude, zp.Longitude);
			}

			((Polygon)polygon).Path = path;

			marker.Position = new CLLocationCoordinate2D ((float)lat / (float)points.Count, (float)lon / (float)points.Count);
		}

		#endregion

//			mapView = BuildMapView (true);
//
////			var center = new ThingAnnotation (thing.ObjectLocation.Latitude, thing.ObjectLocation.Longitude, thing.Name);
////			mapView.AddAnnotation(center);
//
//			//TODO: ISZone
//			var zones = ctrl.Engine.ActiveVisibleZones;
//			foreach(Zone z in zones)
//			{
//				var points = z.Points;
//				CLLocationCoordinate2D[] coords = new CLLocationCoordinate2D [points.Count];
//				int i = 0;
//				foreach(ZonePoint zp in points) {
////				for (int i = 0; i < points.Count; i++) {
////					ZonePoint zp = points[i+1];
//					coords[i++] = new CLLocationCoordinate2D(zp.Latitude, zp.Longitude);
//				}
//				var mkp = MKPolygon.FromCoordinates(coords);
			//				mapView.AddOverlay(mkp);
			//			}
//
//			UITapGestureRecognizer tgr = new UITapGestureRecognizer ();
//			tgr.AddTarget (this, new MonoTouch.ObjCRuntime.Selector ("TapGesture"));
//			tgr.Delegate = new TapRecognizerDelegate ();
//			this.View.AddGestureRecognizer (tgr);
//
//			mapView.Delegate = new MapDelegate();
//			
//			this.View.AddSubview(mapView);
//
//			// Set center to object
//			ZonePoint loc;
//			
//			if (thing is Zone)
//				loc = ((Zone)thing).OriginalPoint;
//			else if (thing is Item)
//				loc = ((Item)thing).ObjectLocation;
//			else
//				loc = ((Character)thing).ObjectLocation;
//
//			if (loc != null)
//				SetCenterCoordinate(new CLLocationCoordinate2D(loc.Latitude,loc.Longitude),ctrl.ZoomLevel,false);
		}
//		
//		private MKMapView BuildMapView(bool showUserLocation)
//		{
//			var view = new MKMapView()
//			{
//				ShowsUserLocation = showUserLocation
//			};
//
//			view.SizeToFit();
//			view.Frame = new RectangleF(0, 0, this.View.Frame.Width, this.View.Frame.Height);
//			return view;
//		}
//		
//		private MKCoordinateRegion BuildVisibleRegion(CLLocationCoordinate2D currentLocation)
//		{
//			var span = new MKCoordinateSpan(0.2,0.2);
//			var region = new MKCoordinateRegion(currentLocation,span);
//			
//			return region;
//		}
//
//		[Export("TapGesture")]
//		public void TapGesture (UIGestureRecognizer recognizer)
//		{
//			//http://freshmob.com.au/mapkit/mapkit-tap-and-hold-to-drop-a-pin-on-the-map/
//			//and
//			//http://inxunxa.wordpress.com/2011/03/10/monotouch-longpress/
//			
//			
//			if (recognizer.State != UIGestureRecognizerState.Ended)
//				return;
//			
//			// Get the point of the action ...
//			PointF point = recognizer.LocationInView (this.View);
//
//			// ... and convert it to lat/lon
//			CLLocationCoordinate2D coord = this.mapView.ConvertPoint (point, this.mapView);
//			MKMapPoint mapPoint = new MKMapPoint(coord.Latitude,coord.Longitude);
//
//			// Did we have any overlays?
//			if (mapView.Overlays == null)
//				return;
//
//			// Check all active objects, if they are inside
//			var zones = ctrl.Engine.ActiveVisibleZones;
//			foreach(Zone z in zones)
//			{
//				//Add pin annoation here
//				ZonePoint loc = z.OriginalPoint;
//				double lat = loc.Latitude;
//				double lon = loc.Longitude;
//				string name = z.Name;
//				ThingAnnotation ann = new ThingAnnotation (lat, lon, name);
//				this.mapView.AddAnnotation (ann);
//			}
//
//			foreach (NSObject overlay in mapView.Overlays) {
//				if (overlay is MKPolygon) {
//					MKPolygon poly = (MKPolygon)overlay;
//					MKOverlayView view = this.mapView.ViewForOverlay (poly);
//					if (view is MKPolygonView) {
//						MKPolygonView polyView = (MKPolygonView)view;
//						PointF polygonViewPoint = polyView.PointForMapPoint (mapPoint);
//						CGPath path = polyView.Path;
//						bool mapCoordinateIsInPolygon = path.ContainsPoint( polygonViewPoint,true);
//						if (mapCoordinateIsInPolygon) {
//							//Add pin annoation here
//							ThingAnnotation ann = new ThingAnnotation (coord.Latitude, coord.Longitude, thing.Name);
//							this.mapView.AddAnnotation (ann);
//						}
//					}
//				}
//			}
//		}
//
//		// Code in Objectiv-C from http://troybrant.net/blog/2010/01/set-the-zoom-level-of-an-mkmapview/
//
//		private const double MERCATOR_OFFSET = 268435456;
//		private const double MERCATOR_RADIUS = 85445659.44705395;
//		
//		private double longitudeToPixelSpaceX (double longitude)
//		{
//			return Math.Round (MERCATOR_OFFSET + MERCATOR_RADIUS * longitude * Math.PI / 180.0);
//		}
//		
//		private double latitudeToPixelSpaceY (double latitude)
//		{
//			return Math.Round (MERCATOR_OFFSET - MERCATOR_RADIUS * Math.Log((1 + Math.Sin(latitude * Math.PI / 180.0)) / (1 - Math.Sin(latitude * Math.PI / 180.0))) / 2.0);
//		}
//		
//		private double pixelSpaceXToLongitude (double pixelX)
//		{
//			return ((Math.Round(pixelX) - MERCATOR_OFFSET) / MERCATOR_RADIUS) * 180.0 / Math.PI;
//		}
//
//		private double pixelSpaceYToLatitude (double pixelY)
//		{
//			return (Math.PI / 2.0 - 2.0 * Math.Atan(Math.Exp((Math.Round(pixelY) - MERCATOR_OFFSET) / MERCATOR_RADIUS))) * 180.0 / Math.PI;
//		}
//		
//		private MKCoordinateSpan coordinateSpanWithMapView (MKMapView mapView, CLLocationCoordinate2D centerCoordinate, int zoomLevel)
//		{
//			// convert center coordiate to pixel space
//			double centerPixelX = longitudeToPixelSpaceX (centerCoordinate.Longitude);
//			double centerPixelY = latitudeToPixelSpaceY (centerCoordinate.Latitude);
//			
//			// determine the scale value from the zoom level
//			int zoomExponent = 20 - zoomLevel;
//			double zoomScale = Math.Pow(2, zoomExponent);
//			
//			// scale the map’s size in pixel space
//			SizeF mapSizeInPixels = mapView.Bounds.Size;
//			double scaledMapWidth = mapSizeInPixels.Width * zoomScale;
//			double scaledMapHeight = mapSizeInPixels.Height * zoomScale;
//			
//			// figure out the position of the top-left pixel
//			double topLeftPixelX = centerPixelX - (scaledMapWidth / 2);
//			double topLeftPixelY = centerPixelY - (scaledMapHeight / 2);
//			
//			// find delta between left and right longitudes
//			double minLng = pixelSpaceXToLongitude (topLeftPixelX);
//			double maxLng = pixelSpaceXToLongitude (topLeftPixelX + scaledMapWidth);
//			double longitudeDelta = maxLng - minLng;
//			
//			// find delta between top and bottom latitudes
//			double minLat = pixelSpaceYToLatitude (topLeftPixelY);
//			double maxLat = pixelSpaceYToLatitude (topLeftPixelY + scaledMapHeight);
//			double latitudeDelta = -1 * (maxLat - minLat);
//			
//			// create and return the lat/lng span
//			MKCoordinateSpan span = new MKCoordinateSpan(latitudeDelta, longitudeDelta);
//
//			return span;
//		}
//
//		public void SetCenterCoordinate (CLLocationCoordinate2D centerCoordinate, int zoomLevel, bool animated)
//		{
//			// clamp large numbers to 28
//			zoomLevel = Math.Min(zoomLevel, 28);
//			
//			// use the zoom level to compute the region
//			MKCoordinateSpan span = coordinateSpanWithMapView (mapView, centerCoordinate, zoomLevel);
//			MKCoordinateRegion region = new MKCoordinateRegion (centerCoordinate, span);
//			
//			// set the region like normal
//			mapView.SetRegion (region, animated);
//		}
//
//	}
//
//	public class ThingAnnotation : MKAnnotation
//	{
//		private string title;
//
//		public override CLLocationCoordinate2D Coordinate { get; set; }
//
//		public ThingAnnotation (double latitude, double longitude, string title) : base()
//		{
//			this.Coordinate = new CLLocationCoordinate2D(latitude,longitude);
//			this.title = title;
//		}
//
//		public override string Title { get { return title; } }
//	}
//
//	public class MapDelegate : MKMapViewDelegate
//	{
//		
//		public override MKAnnotationView GetViewForAnnotation (MKMapView Map, NSObject annotation)
//		{
//			if (annotation is MKAnnotation) {
//				MKAnnotation a = annotation as MKAnnotation;
//				if (a != null) { 
//					MKAnnotationView aView = new MKAnnotationView (a,"Anna");
//					// customize code for the MKPolygonView
//					aView.Image = new UIImage("IconLocation.png");
//					return aView;
//				} 
//			}
//			
//			return null;
//		}
//
//		public override MKOverlayView GetViewForOverlay (MKMapView mapView, NSObject overlay)
//		{
//			if (overlay is MKPolygon) {
//				MKPolygon polygon = overlay as MKPolygon;
//				if (polygon != null) { // "overlay" is the overlay object you added
//					MKPolygonView polyView = new MKPolygonView (polygon);
//					// customize code for the MKPolygonView
//					polyView.FillColor = UIColor.Red.ColorWithAlpha(0.3f);
//					polyView.StrokeColor = UIColor.Red.ColorWithAlpha(0.5f);
//					polyView.LineWidth = 0.5f;
//					return polyView;
//				} 
//			}
//			
//			return null;
//		}
//	}
//
//	public class TapRecognizerDelegate : MonoTouch.UIKit.UIGestureRecognizerDelegate
//	{
//		public override bool ShouldReceiveTouch (UIGestureRecognizer recognizer, UITouch touch)
//		{
//			return true;
//		}

}