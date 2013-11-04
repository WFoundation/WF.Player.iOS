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
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.MapKit;
using MonoTouch.CoreLocation;
using MonoTouch.CoreGraphics;
using WF.Player.Core;


namespace WF.Player.iPhone
{
	public class ScreenMap : UIViewController
	{
		private ScreenController ctrl;
		private Thing thing;
		private MKMapView mapView;
		
		public ScreenMap (ScreenController ctrl, Thing t)
		{
			this.ctrl = ctrl;
			this.thing = t;
		}
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			CLLocationCoordinate2D coord = new CLLocationCoordinate2D (ctrl.Engine.Latitude, ctrl.Engine.Longitude);
			MKCoordinateRegion visibleRegion = BuildVisibleRegion (coord);

			mapView = BuildMapView (true);

//			var center = new ThingAnnotation (thing.ObjectLocation.Latitude, thing.ObjectLocation.Longitude, thing.Name);
//			mapView.AddAnnotation(center);

			//TODO: ISZone
			var zones = ctrl.Engine.ActiveVisibleZones;
			foreach(Zone z in zones)
			{
				List<ZonePoint> points = z.Points;
				CLLocationCoordinate2D[] coords = new CLLocationCoordinate2D [points.Count];
				int i = 0;
				foreach(ZonePoint zp in points) {
//				for (int i = 0; i < points.Count; i++) {
//					ZonePoint zp = points[i+1];
					coords[i++] = new CLLocationCoordinate2D(zp.Latitude, zp.Longitude);
				}
				var mkp = MKPolygon.FromCoordinates(coords);
				mapView.AddOverlay(mkp);
			}

			UITapGestureRecognizer tgr = new UITapGestureRecognizer ();
			tgr.AddTarget (this, new MonoTouch.ObjCRuntime.Selector ("TapGesture"));
			tgr.Delegate = new TapRecognizerDelegate ();
			this.View.AddGestureRecognizer (tgr);

			mapView.Delegate = new MapDelegate();
			
			this.View.AddSubview(mapView);

			// Set center to object
			ZonePoint loc;
			
			if (thing is Zone)
				loc = ((Zone)thing).OriginalPoint;
			else if (thing is Item)
				loc = ((Item)thing).ObjectLocation;
			else
				loc = ((Character)thing).ObjectLocation;

			SetCenterCoordinate(new CLLocationCoordinate2D(loc.Latitude,loc.Longitude),ctrl.ZoomLevel,false);
		}
		
		private MKMapView BuildMapView(bool showUserLocation)
		{
			var view = new MKMapView()
			{
				ShowsUserLocation = showUserLocation
			};

			view.SizeToFit();
			view.Frame = new RectangleF(0, 0, this.View.Frame.Width, this.View.Frame.Height);
			return view;
		}
		
		private MKCoordinateRegion BuildVisibleRegion(CLLocationCoordinate2D currentLocation)
		{
			var span = new MKCoordinateSpan(0.2,0.2);
			var region = new MKCoordinateRegion(currentLocation,span);
			
			return region;
		}

		[Export("TapGesture")]
		public void TapGesture (UIGestureRecognizer recognizer)
		{
			//http://freshmob.com.au/mapkit/mapkit-tap-and-hold-to-drop-a-pin-on-the-map/
			//and
			//http://inxunxa.wordpress.com/2011/03/10/monotouch-longpress/
			
			
			if (recognizer.State != UIGestureRecognizerState.Ended)
				return;
			
			// Get the point of the action ...
			PointF point = recognizer.LocationInView (this.View);

			// ... and convert it to lat/lon
			CLLocationCoordinate2D coord = this.mapView.ConvertPoint (point, this.mapView);
			MKMapPoint mapPoint = new MKMapPoint(coord.Latitude,coord.Longitude);

			// Did we have any overlays?
			if (mapView.Overlays == null)
				return;

			// Check all active objects, if they are inside
			List<Zone> zones = ctrl.Engine.ActiveVisibleZones;
			foreach(Zone z in zones)
			{
				//Add pin annoation here
				ZonePoint loc = z.OriginalPoint;
				double lat = loc.Latitude;
				double lon = loc.Longitude;
				string name = z.Name;
				ThingAnnotation ann = new ThingAnnotation (lat, lon, name);
				this.mapView.AddAnnotation (ann);
			}

			foreach (NSObject overlay in mapView.Overlays) {
				if (overlay is MKPolygon) {
					MKPolygon poly = (MKPolygon)overlay;
					MKOverlayView view = this.mapView.ViewForOverlay (poly);
					if (view is MKPolygonView) {
						MKPolygonView polyView = (MKPolygonView)view;
						PointF polygonViewPoint = polyView.PointForMapPoint (mapPoint);
						CGPath path = polyView.Path;
						bool mapCoordinateIsInPolygon = path.ContainsPoint( polygonViewPoint,true);
						if (mapCoordinateIsInPolygon) {
							//Add pin annoation here
							ThingAnnotation ann = new ThingAnnotation (coord.Latitude, coord.Longitude, thing.Name);
							this.mapView.AddAnnotation (ann);
						}
					}
				}
			}
		}

		// Code in Objectiv-C from http://troybrant.net/blog/2010/01/set-the-zoom-level-of-an-mkmapview/

		private const double MERCATOR_OFFSET = 268435456;
		private const double MERCATOR_RADIUS = 85445659.44705395;
		
		private double longitudeToPixelSpaceX (double longitude)
		{
			return Math.Round (MERCATOR_OFFSET + MERCATOR_RADIUS * longitude * Math.PI / 180.0);
		}
		
		private double latitudeToPixelSpaceY (double latitude)
		{
			return Math.Round (MERCATOR_OFFSET - MERCATOR_RADIUS * Math.Log((1 + Math.Sin(latitude * Math.PI / 180.0)) / (1 - Math.Sin(latitude * Math.PI / 180.0))) / 2.0);
		}
		
		private double pixelSpaceXToLongitude (double pixelX)
		{
			return ((Math.Round(pixelX) - MERCATOR_OFFSET) / MERCATOR_RADIUS) * 180.0 / Math.PI;
		}

		private double pixelSpaceYToLatitude (double pixelY)
		{
			return (Math.PI / 2.0 - 2.0 * Math.Atan(Math.Exp((Math.Round(pixelY) - MERCATOR_OFFSET) / MERCATOR_RADIUS))) * 180.0 / Math.PI;
		}
		
		private MKCoordinateSpan coordinateSpanWithMapView (MKMapView mapView, CLLocationCoordinate2D centerCoordinate, int zoomLevel)
		{
			// convert center coordiate to pixel space
			double centerPixelX = longitudeToPixelSpaceX (centerCoordinate.Longitude);
			double centerPixelY = latitudeToPixelSpaceY (centerCoordinate.Latitude);
			
			// determine the scale value from the zoom level
			int zoomExponent = 20 - zoomLevel;
			double zoomScale = Math.Pow(2, zoomExponent);
			
			// scale the map’s size in pixel space
			SizeF mapSizeInPixels = mapView.Bounds.Size;
			double scaledMapWidth = mapSizeInPixels.Width * zoomScale;
			double scaledMapHeight = mapSizeInPixels.Height * zoomScale;
			
			// figure out the position of the top-left pixel
			double topLeftPixelX = centerPixelX - (scaledMapWidth / 2);
			double topLeftPixelY = centerPixelY - (scaledMapHeight / 2);
			
			// find delta between left and right longitudes
			double minLng = pixelSpaceXToLongitude (topLeftPixelX);
			double maxLng = pixelSpaceXToLongitude (topLeftPixelX + scaledMapWidth);
			double longitudeDelta = maxLng - minLng;
			
			// find delta between top and bottom latitudes
			double minLat = pixelSpaceYToLatitude (topLeftPixelY);
			double maxLat = pixelSpaceYToLatitude (topLeftPixelY + scaledMapHeight);
			double latitudeDelta = -1 * (maxLat - minLat);
			
			// create and return the lat/lng span
			MKCoordinateSpan span = new MKCoordinateSpan(latitudeDelta, longitudeDelta);

			return span;
		}

		public void SetCenterCoordinate (CLLocationCoordinate2D centerCoordinate, int zoomLevel, bool animated)
		{
			// clamp large numbers to 28
			zoomLevel = Math.Min(zoomLevel, 28);
			
			// use the zoom level to compute the region
			MKCoordinateSpan span = coordinateSpanWithMapView (mapView, centerCoordinate, zoomLevel);
			MKCoordinateRegion region = new MKCoordinateRegion (centerCoordinate, span);
			
			// set the region like normal
			mapView.SetRegion (region, animated);
		}

	}

	public class ThingAnnotation : MKAnnotation
	{
		private string title;

		public override CLLocationCoordinate2D Coordinate { get; set; }

		public ThingAnnotation (double latitude, double longitude, string title) : base()
		{
			this.Coordinate = new CLLocationCoordinate2D(latitude,longitude);
			this.title = title;
		}

		public override string Title { get { return title; } }
	}

	public class MapDelegate : MKMapViewDelegate
	{
		
		public override MKAnnotationView GetViewForAnnotation (MKMapView Map, NSObject annotation)
		{
			if (annotation is MKAnnotation) {
				MKAnnotation a = annotation as MKAnnotation;
				if (a != null) { 
					MKAnnotationView aView = new MKAnnotationView (a,"Anna");
					// customize code for the MKPolygonView
					aView.Image = new UIImage("IconLocation.png");
					return aView;
				} 
			}
			
			return null;
		}

		public override MKOverlayView GetViewForOverlay (MKMapView mapView, NSObject overlay)
		{
			if (overlay is MKPolygon) {
				MKPolygon polygon = overlay as MKPolygon;
				if (polygon != null) { // "overlay" is the overlay object you added
					MKPolygonView polyView = new MKPolygonView (polygon);
					// customize code for the MKPolygonView
					polyView.FillColor = UIColor.Red.ColorWithAlpha(0.3f);
					polyView.StrokeColor = UIColor.Red.ColorWithAlpha(0.5f);
					polyView.LineWidth = 0.5f;
					return polyView;
				} 
			}
			
			return null;
		}
	}

	public class TapRecognizerDelegate : MonoTouch.UIKit.UIGestureRecognizerDelegate
	{
		public override bool ShouldReceiveTouch (UIGestureRecognizer recognizer, UITouch touch)
		{
			return true;
		}
	}

}