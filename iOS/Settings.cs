using System;
using System.Text;
using MonoTouch.AudioToolbox;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Vernacular;

namespace WF.Player.iOS
{
	public class Values
	{
		static public float Frame = 10.0f;
		static public float ButtonHeight = 35.0f;
	}

	public class Colors
	{
		public static UIColor NavBar = UIColor.FromRGB(0.2118f,0.4588f,0.7333f);
		public static UIColor NavBarButton = UIColor.FromRGB(0.1569f,0.3608f,0.5961f);
		public static UIColor ButtonText = UIColor.FromRGB(1.0f,1.0f,1.0f);
		public static UIColor ZoneFill = UIColor.FromRGBA (1.0f, 0, 0, 0.1f);
		public static UIColor ZoneStroke = UIColor.FromRGBA (1.0f, 0, 0, 0.3f);
	}

	public class Sounds
	{
		public static SystemSound KeyboardClick = new SystemSound(NSUrl.FromFilename (NSBundle.FromIdentifier ("com.apple.UIKit").PathForResource ("Tock", "aiff")));
	}

	public class Images
	{
		public static UIImage IconLocation = UIImage.FromFile ("Images/IconLocation.png");
		public static UIImage IconYouSee = UIImage.FromFile ("Images/IconYouSee.png");
		public static UIImage IconInventory = UIImage.FromFile ("Images/IconInventory.png");
		public static UIImage IconTask = UIImage.FromFile ("Images/IconTask.png");

		public static UIImage IconMapZone = UIImage.FromFile ("Images/IconMapZone.png");
		public static UIImage IconMapCharacter = UIImage.FromFile ("Images/IconMapCharacter.png");
		public static UIImage IconMapItem = UIImage.FromFile ("Images/IconMapItem.png");

		public static UIImage ButtonCenter = UIImage.FromFile ("Images/ButtonCenter.png");
		public static UIImage ButtonOrientation = UIImage.FromFile ("Images/ButtonOrientation.png");
		public static UIImage ButtonOrientationNorth = UIImage.FromFile ("Images/ButtonOrientationNorth.png");
		public static UIImage ButtonMapType = UIImage.FromFile ("Images/ButtonLayer.png");

		public static UIImage Blue = UIImage.FromFile ("Images/blue.png").StretchableImage(0, 0);
		public static UIImage BlueTop = UIImage.FromFile ("Images/blueTop.png").StretchableImage(0, 0);
		public static UIImage BlueButton = UIImage.FromFile ("Images/blueButton.png").CreateResizableImage(new UIEdgeInsets (18f, 18f, 18f, 18f));
		public static UIImage BlueButtonHighlight = UIImage.FromFile ("Images/blueButtonHighlight.png").CreateResizableImage(new UIEdgeInsets (18f, 18f, 18f, 18f));
		public static UIImage OrangeButton = UIImage.FromFile ("Images/orangeButton.png").CreateResizableImage(new UIEdgeInsets (18f, 18f, 18f, 18f));
		public static UIImage OrangeButtonHighlight = UIImage.FromFile ("Images/orangeButtonHighlight.png").CreateResizableImage(new UIEdgeInsets (18f, 18f, 18f, 18f));
		public static UIImage GreenButton = UIImage.FromFile ("Images/greenButton.png").CreateResizableImage(new UIEdgeInsets (18f, 18f, 18f, 18f));
		public static UIImage GreenButtonHighlight = UIImage.FromFile ("Images/greenButtonHighlight.png").CreateResizableImage(new UIEdgeInsets (18f, 18f, 18f, 18f));
	}

	public sealed class Strings
	{
		public static string TaskCorrect = Encoding.UTF8.GetString(new byte[] { 0xE2, 0x9C, 0x93 } );    	// UTF-8 2713
		public static string TaskNotCorrect = Encoding.UTF8.GetString(new byte[] { 0xE2, 0x9C, 0x97 } );  	// UTF-8 2717
		public static string Infinite = Encoding.UTF8.GetString(new byte[] { 0xE2, 0x88, 0x9E } );  		// UTF-8 221E
		public static string Checked = Encoding.UTF8.GetString(new byte[] { 0xE2, 0x9C, 0x93 } );    		// UTF-8 2713

		public static string GetString(string text)
		{
			return Catalog.GetString (text);
		}

		public static string GetStringFmt(string text, params object[] args)
		{
			return Catalog.Format(GetString(text), args);
		}

		public static string GetPluralString(string singular, string plural, int n)
		{
			return Catalog.GetPluralString(singular, plural, n);
		}

		public static string GetPluralStringFmt(string singular, string plural, int n, params object[] args)
		{
			return Catalog.Format(Catalog.GetPluralString(singular, plural, n), args);
		}
	}
}

