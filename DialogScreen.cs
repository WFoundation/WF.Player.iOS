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
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using MonoTouch.UIKit;
using WF.Player.Core;

namespace WF.Player.iPhone
{
	
	public class DialogScreen : UIViewController
	{

		private float scrollamount = 0.0f;
		private float bottomPoint = 0.0f;
		private bool moveViewUp = false;

		private ScreenController ctrl;
		private List<UIButton> buttons = new List<UIButton> ();
		private UIView buttonView;
		private MessageEntry content;
		private UITextField input;
		private UIImageView image;
		private UIScrollView scroll;
		private UILabel text;
		
		public DialogScreen (ScreenController ctrl, MessageEntry content) : base ()
		{
			this.ctrl = ctrl;
			this.content = content;
		}
		
		#region MonoTouch Functions
		
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
//			createView ();
			NSNotificationCenter.DefaultCenter.AddObserver ("UIKeyboardDidShowNotification", keyboardUpNotification);

			this.NavigationController.SetNavigationBarHidden(true,false);
			createView ();
		}

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear(animated);
			this.NavigationItem.SetHidesBackButton(true,true);
			resizeView ();
		}
		
		public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation) 
		{
			base.DidRotate(fromInterfaceOrientation);
			resizeView();
		}

		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			// Return true for supported orientations
			return true;
		}

		private void keyboardUpNotification (NSNotification notification)
		{
			//Get the bounds of the keyboard...
			resetView();
			
			RectangleF r = UIKeyboard.BoundsFromNotification(notification);
			
			if(input != null && input.IsEditing)
			{
				//Calculate the bottom of the Texbox
				//plus a small margin...
				bottomPoint = (this.input.Frame.Y + this.input.Frame.Height + 10);
				
				//Calculate the amount to scroll the view
				//upwards so the Textbox becomes visible...
				//This is the height of the Keyboard -
				//(the height of the display - the bottom
				//of the Texbox)...	
				scrollamount = (r.Height - (View.Frame.Size.Height - bottomPoint));
			}
			
			//Check to see whether the view
			//should be moved up...
			if (scrollamount > 0)
			{
				moveViewUp = true;
				scrollView(moveViewUp);
			}
			else moveViewUp = false;
		}

		private bool inputShouldReturn (UITextField tf)
		{
			tf.ResignFirstResponder ();
			if (moveViewUp) { scrollView(false); }
			return true;
		}

		private void resetView()
		{
			UIView.BeginAnimations(string.Empty, System.IntPtr.Zero);
			UIView.SetAnimationDuration(0.3);
			
			RectangleF frame = View.Frame;
			frame.Y = 0;
			View.Frame = frame;
			UIView.CommitAnimations();
		}

		private void scrollView(bool movedUp)
		{
			//To invoke a views built-in animation behaviour,
			//you create an animation block and
			//set the duration of the move...
			//Set the display scroll animation and duration...
			UIView.BeginAnimations(string.Empty, System.IntPtr.Zero);
			UIView.SetAnimationDuration(0.3);
			
			//Get Display size...
			RectangleF frame = View.Frame;
			
			if (movedUp) {
				//If the view should be moved up,
				//subtract the keyboard height from the display...
				frame.Y -= scrollamount;
			}
			else {
				//If the view shouldn't be moved up, restore it
				//by adding the keyboard height back to the original...
				frame.Y += scrollamount;
			}
			
			//Assign the new frame to the view...
			View.Frame = frame;
			
			//Tell the view that your all done with setting
			//the animation parameters, and it should
			//start the animation...
			UIView.CommitAnimations();
		}
		
		#endregion
		
		#region C# Events

		private void OnTouchUpInside(object sender, EventArgs e) 
		{
			if (input != null)
				if (input.Text == null)
					content.Edit = "";
				else
					content.Edit = input.Text;
			// Show right screen
			ctrl.DialogCallback (content,((UIButton)sender).Tag);
		}
		
		#endregion

		private void createView ()
		{
			// If input, than safe text for later use
			if (input != null)
				content.Edit = input.Text;

			// Remove all existing subviews
			foreach (UIView view in this.View.Subviews) {
				view.RemoveFromSuperview();
			}
			
			// Now create and add all other views
			float frame = 10;
			float maxWidth = this.View.Bounds.Width - 2 * frame;
			float maxHeight = this.View.Bounds.Height;

			if (content.Image != null) {
				image = new UIImageView (content.Image) {
					ContentMode = UIViewContentMode.Center | UIViewContentMode.ScaleAspectFit
				};
				image.Bounds = new RectangleF (0, 0, maxWidth, image.Bounds.Height);
			} else {
				image = null;
			}
			if (!String.IsNullOrEmpty (content.Text)) {
				text = new UILabel () {
					Text = content.Text,
					BackgroundColor = UIColor.Clear,
					Lines = 0,
					LineBreakMode = UILineBreakMode.WordWrap,
					TextAlignment = UITextAlignment.Center,
					AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight,
					ContentMode = UIViewContentMode.Center
				};
				SizeF size = text.SizeThatFits (new SizeF (maxWidth, 999999));
				text.Bounds = new RectangleF (0, 0, maxWidth, size.Height);
			} else {
				text = null;
			}
			if (content.Type == MessageEntry.sqeInput) {
				input = new UITextField (new RectangleF (frame, maxHeight - (content.Buttons.Count + 1) * 45, maxWidth, 35)) {
					BorderStyle = UITextBorderStyle.RoundedRect,
					ReturnKeyType = UIReturnKeyType.Done
				};
				input.ShouldReturn += inputShouldReturn;
				if (!String.IsNullOrEmpty(content.Edit))
				    input.Text = content.Edit;
			} else {
				input = null;
			}
			buttonView = new UIView (){
				ContentMode = UIViewContentMode.Center
			};
			if (content.Buttons != null && content.Buttons.Count > 0) {
				buttonView.Bounds = new RectangleF (0, 0, maxWidth, content.Buttons.Count * 45);
				buttonView.BackgroundColor = UIColor.Clear;
				int pos = 0;
				foreach (string s in content.Buttons) {
					UIButton button = UIButton.FromType (UIButtonType.RoundedRect);
					button.Tag = pos;
					button.Bounds = new RectangleF (0, 0, maxWidth, 35);
					button.Frame = new RectangleF (0, pos * 45, maxWidth, 35);
					button.HorizontalAlignment = UIControlContentHorizontalAlignment.Center;
					button.AutoresizingMask = UIViewAutoresizing.FlexibleTopMargin;
					button.SetTitle (s, UIControlState.Normal);
					button.TouchUpInside += OnTouchUpInside;
					buttonView.AddSubview (button);
					buttons.Add (button);
					pos++;
				}
			} else {
				buttonView = null;
			}
			
			// Create scroll view, which holds all other views
			scroll = new UIScrollView (new RectangleF (0, 0, this.View.Bounds.Width, maxHeight)) {
				BackgroundColor = UIColor.White,
				ScrollEnabled = true
			};

			float height = 10;
			if (image != null) {
				image.Frame = new RectangleF (frame, height, image.Bounds.Width, image.Bounds.Height);
				scroll.AddSubview (image);
				height += image.Bounds.Height + frame;
			}
			if (text != null) {
				text.Frame = new RectangleF (frame, height, text.Bounds.Width, text.Bounds.Height);
				scroll.AddSubview (text);
				height += text.Bounds.Height + frame;
			}
			if (input != null) {
				text.Frame = new RectangleF (frame, height, input.Bounds.Width, input.Bounds.Height);
				scroll.AddSubview (input);
				height += input.Bounds.Height + frame;
			}
			if (buttonView != null) { 
				if (height + buttonView.Bounds.Height + this.NavigationController.NavigationBar.Bounds.Height < maxHeight)
					height = maxHeight - buttonView.Bounds.Height - this.NavigationController.NavigationBar.Bounds.Height;
				buttonView.Frame = new RectangleF ( frame, height, buttonView.Bounds.Width, buttonView.Bounds.Height);
				scroll.AddSubview (buttonView);
				height += buttonView.Bounds.Height;
			}
			
			scroll.ContentSize = new SizeF(scroll.Frame.Width,height);
		
			this.View.AddSubview (scroll);
			this.View.BackgroundColor = UIColor.Clear;
		}

		private void resizeView()
		{
			float height = 10;
			float frame = 10;
			float maxWidth = this.View.Bounds.Width - 2 * frame;
			float maxHeight = this.View.Bounds.Height;
			
			scroll.Frame = new RectangleF(0,0,this.View.Bounds.Width,maxHeight);
			
			if (image != null) {
				image.Frame = new RectangleF (frame, height, maxWidth, image.Bounds.Height);
				height += image.Bounds.Height + frame;
			}
			if (text != null) {
				SizeF size = text.SizeThatFits (new SizeF (maxWidth, 999999));
				text.Bounds = new RectangleF (0, 0, maxWidth, size.Height);
				text.Frame = new RectangleF (frame, height, text.Bounds.Width, text.Bounds.Height);
				height += text.Bounds.Height + frame;
			}
			if (input != null) {
				input.Frame = new RectangleF (frame, height, maxWidth, input.Bounds.Height);
				height += input.Bounds.Height + frame;
			}
			if (buttonView != null) { 
				if (height + buttonView.Bounds.Height < maxHeight)
					height = maxHeight - buttonView.Bounds.Height;
				buttonView.Frame = new RectangleF ( frame, height, maxWidth, buttonView.Bounds.Height);
				int pos = 0;
				foreach(UIView v in buttonView.Subviews) {
					v.Bounds = new RectangleF ( 0, 0, maxWidth, v.Bounds.Height ); 
					v.Frame = new RectangleF ( 0, pos * 45, maxWidth, v.Bounds.Height ); 
					pos++;
				}
				height += buttonView.Bounds.Height;
			}
			
			scroll.ContentSize = new SizeF(scroll.Frame.Width,height);
		}

	}
	
}

