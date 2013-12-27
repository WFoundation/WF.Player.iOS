using System;
using System.Text;
using MonoTouch.UIKit;
using GNU.Gettext;

namespace WF.Player.iOS
{
	public class Values
	{
		static public float Frame = 10.0f;
		static public float ButtonHeight = 35.0f;
	}

	public class Colors
	{
		static public UIColor NavBar = UIColor.FromRGB(0.2118f,0.4588f,0.7333f);
		static public UIColor NavBarButton = UIColor.FromRGB(0.1569f,0.3608f,0.5961f);
		static public UIColor ButtonText = UIColor.FromRGB(1.0f,1.0f,1.0f);
	}

	public class Images
	{
		static public UIImage IconLocation = UIImage.FromFile ("Images/IconLocation.png");
		static public UIImage IconYouSee = UIImage.FromFile ("Images/IconYouSee.png");
		static public UIImage IconInventory = UIImage.FromFile ("Images/IconInventory.png");
		static public UIImage IconTask = UIImage.FromFile ("Images/IconTask.png");

		static public UIImage Blue = UIImage.FromFile ("Images/blue.png").StretchableImage(0, 0);
		static public UIImage BlueTop = UIImage.FromFile ("Images/blueTop.png").StretchableImage(0, 0);
		static public UIImage BlueButton = UIImage.FromFile ("Images/blueButton.png").CreateResizableImage(new UIEdgeInsets (18f, 18f, 18f, 18f));
		static public UIImage BlueButtonHighlight = UIImage.FromFile ("Images/blueButtonHighlight.png").CreateResizableImage(new UIEdgeInsets (18f, 18f, 18f, 18f));
		static public UIImage OrangeButton = UIImage.FromFile ("Images/orangeButton.png").CreateResizableImage(new UIEdgeInsets (18f, 18f, 18f, 18f));
		static public UIImage OrangeButtonHighlight = UIImage.FromFile ("Images/orangeButtonHighlight.png").CreateResizableImage(new UIEdgeInsets (18f, 18f, 18f, 18f));
		static public UIImage GreenButton = UIImage.FromFile ("Images/greenButton.png").CreateResizableImage(new UIEdgeInsets (18f, 18f, 18f, 18f));
		static public UIImage GreenButtonHighlight = UIImage.FromFile ("Images/greenButtonHighlight.png").CreateResizableImage(new UIEdgeInsets (18f, 18f, 18f, 18f));
	}

	public sealed class Strings
	{
		static GettextResourceManager catalog = new GettextResourceManager("WF.Player.iOS");

		public static string TaskCorrect = Encoding.UTF8.GetString(new byte[] { 0xE2, 0x9C, 0x93 } );    	// UTF-8 2713
		public static string TaskNotCorrect = Encoding.UTF8.GetString(new byte[] { 0xE2, 0x9C, 0x97 } );  	// UTF-8 2717
		public static string Infinite = Encoding.UTF8.GetString(new byte[] { 0xE2, 0x88, 0x9E } );  		// UTF-8 221E

		public static string GetString(string text)
		{
			return catalog.GetString (text);
		}

		public static string GetStringFmt(string text, params object[] args)
		{
			return String.Format(catalog.GetString(text), args);
		}

		public static string GetPluralString(string singular, string plural, long n)
		{
			return catalog.GetPluralString(singular, plural, n);
		}

		public static string GetPluralStringFmt(string singular, string plural, long n, params object[] args)
		{
			return String.Format(catalog.GetPluralString(singular, plural, n), args);
		}

		public static string GetParticularString(string context, string text)
		{
			return catalog.GetParticularString(context, text);
		}

		public static string GetParticularPluralString(string context, string singular, string plural, long n)
		{
			return catalog.GetParticularPluralString(context, singular, plural, n);
		}
	}
}

