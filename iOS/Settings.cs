﻿﻿using System;
using System.Text;
using MonoTouch.UIKit;

namespace WF.Player.iPhone
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

	public class Strings
	{
		static public string TaskCorrect = Encoding.UTF8.GetString(new byte[] { 0xE2, 0x9C, 0x93 } );    // UTF-8 2713
		static public string TaskNotCorrect = Encoding.UTF8.GetString(new byte[] { 0xE2, 0x9C, 0x97 } );  // UTF-8 2717
		static public string Infinite = Encoding.UTF8.GetString(new byte[] { 0xE2, 0x88, 0x9E } );  // UTF-8 221E
	}
}

