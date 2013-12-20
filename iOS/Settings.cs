using System;
using System.Text;
using MonoTouch.UIKit;

namespace WF.Player.iPhone
{
	public class Colors
	{
		static public UIColor NavBar = UIColor.FromRGB(0.2118f,0.4588f,0.7333f);
		static public UIColor NavBarButton = UIColor.FromRGB(0.2000f,0.2824f,0.3725f);
		static public UIColor ButtonText = UIColor.FromRGB(1.0f,1.0f,1.0f);
	}

	public class Images
	{
		static public UIImage IconLocation = UIImage.FromFile ("Images/IconLocation.png");
		static public UIImage IconYouSee = UIImage.FromFile ("Images/IconYouSee.png");
		static public UIImage IconInventory = UIImage.FromFile ("Images/IconInventory.png");
		static public UIImage IconTask = UIImage.FromFile ("Images/IconTask.png");

		static public UIImage Button = UIImage.FromFile ("Images/blueButton.png").CreateResizableImage(new UIEdgeInsets (18f, 18f, 18f, 18f));
		static public UIImage ButtonHighlight = UIImage.FromFile ("Images/blueButtonHighlight.png").CreateResizableImage(new UIEdgeInsets (18f, 18f, 18f, 18f));
	}

	public class Strings
	{
		static public string TaskCorrect = Encoding.UTF8.GetString(new byte[] { 0xE2, 0x9C, 0x93 } );    // UTF-8 2713
		static public string TaskNotCorrect = Encoding.UTF8.GetString(new byte[] { 0xE2, 0x9C, 0x97 } );  // UTF-8 2717
	}
}

