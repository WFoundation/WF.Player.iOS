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
using System.Drawing;
using System.IO;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using WF.Player.Core;

namespace WF.Player.iPhone
{
	public partial class CartridgeDetail : UIViewController
	{
		private AppDelegate appDelegate;
		private Cartridge cart;

		public CartridgeDetail (AppDelegate appDelegate) : base ("CartridgeDetail", null)
		{
			this.appDelegate = appDelegate;
		}

		public Cartridge Cartridge { 
			get { 
				return cart; 
			} 
			set { 
				if (cart != value) {
					cart = value;
					updateData ();
				}
			}
		}
		
		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			// Perform any additional setup after loading the view, typically from a nib.
		}
		
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			// Return true for supported orientations
			return (toInterfaceOrientation != UIInterfaceOrientation.PortraitUpsideDown);
		}

		partial void ButtonStartPressed (MonoTouch.Foundation.NSObject sender)
		{
			appDelegate.CartStart(cart);
		}

		partial void ButtonResumePressed (MonoTouch.Foundation.NSObject sender)
		{
			appDelegate.CartRestore(cart);
		}
		
		private void updateData ()
		{
			this.TextTitle.Text = cart.Name;
			this.TextDescription.Text = cart.Description;
			this.TextDescription.SizeToFit();
			if (cart.Poster != null)
				this.ImagePoster.Image = UIImage.LoadFromData (NSData.FromArray (cart.Poster.Data));
			else
				this.ImagePoster.Image = null;
			ButtonResume.SetTitle (ButtonResume.Title(UIControlState.Normal),UIControlState.Disabled);
			ButtonResume.SetTitleColor(UIColor.Gray,UIControlState.Disabled);
			ButtonResume.Enabled = File.Exists (cart.SaveFilename);

		}
	}
}

