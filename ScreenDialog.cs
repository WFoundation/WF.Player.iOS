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
using System.Drawing;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using MonoTouch.UIKit;
using WF.Player.Core;

namespace WF.Player.iPhone
{
	
	public class ScreenDialog : UIViewController
	{

		private float scrollamount = 0.0f;
		private float bottomPoint = 0.0f;
		private bool moveViewUp = false;

		private List<UIButton> buttonViews = new List<UIButton> ();
		private List<string> buttons = new List<string>();
		private UIView buttonView;
		private MessageBox msgBox;
		private Input input;
		private Media image;
		private string text;
		private UITextField inputView;
		private UIImageView imageView;
		private UIScrollView scrollView;
		private UILabel textView;
		
		public ScreenDialog (MessageBox msgBox) : base ()
		{
			this.msgBox = msgBox;
			this.text = msgBox.Text;
			this.image = msgBox.Image;
			if (!String.IsNullOrWhiteSpace (msgBox.FirstButtonLabel)) {
				this.buttons.Add (msgBox.FirstButtonLabel);
				if (!String.IsNullOrWhiteSpace (msgBox.SecondButtonLabel))
					this.buttons.Add (msgBox.SecondButtonLabel);
			}

			// OS specific details
			if (new Version (UIDevice.CurrentDevice.SystemVersion) >= new Version(7,0)) 
			{
				// Code that uses features from Xamarin.iOS 7.0
				this.EdgesForExtendedLayout = UIRectEdge.None;
			}
		}
		
		public ScreenDialog (Input input) : base ()
		{
			this.input = input;
			this.text = input.Text;
			this.image = input.Image;
			if (input.InputType == InputType.Text)
				buttons.Add ("Ok");
			if (input.InputType == InputType.MultipleChoice)
				foreach (string s in input.Choices)
					buttons.Add (s);
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

			this.NavigationController.SetNavigationBarHidden(false,false);
			CreateView ();
		}

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear(animated);
			this.NavigationItem.SetHidesBackButton(true,true);
			Refresh ();
		}
		
		public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation) 
		{
			base.DidRotate(fromInterfaceOrientation);
			Refresh();
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
			
			if(inputView != null && inputView.IsEditing)
			{
				//Calculate the bottom of the Texbox
				//plus a small margin...
				bottomPoint = (this.inputView.Frame.Y + this.inputView.Frame.Height + 10);
				
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
				scroll(moveViewUp);
			}
			else moveViewUp = false;
		}

		private bool inputShouldReturn (UITextField tf)
		{
			tf.ResignFirstResponder ();
			if (moveViewUp) { scroll(false); }
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

		private void scroll(bool movedUp)
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
			string result = "";
			if (inputView != null && inputView.Text != null)
				result = inputView.Text;
			// TODO: Remove old screen
			((ScreenController)ParentViewController).RemoveScreen (ScreenType.Dialog);
			// Show right screen
			if (input != null) {
				if (input.InputType == InputType.Text)
					input.GiveResult (result);
				else
					input.GiveResult (buttons[((UIButton)sender).Tag]);
			} else {
				var btn = ((UIButton)sender).Tag == 0 ? MessageBoxResult.FirstButton : MessageBoxResult.SecondButton;
				msgBox.GiveResult (btn);
			}
		}
		
		#endregion

		void CreateView ()
		{
			string saveInput = null;

			// If input, than safe text for later use
			if (input != null && inputView != null)
				saveInput = inputView.Text;

			// Remove all existing subviews
			foreach (UIView view in this.View.Subviews) {
				view.RemoveFromSuperview();
			}
			
			// Now create and add all other views
			float frame = 10;
			float maxWidth = this.View.Bounds.Width - 2 * frame;
			float maxHeight = this.View.Bounds.Height;

			if (image != null) {
				imageView = new UIImageView (UIImage.LoadFromData (NSData.FromArray (image.Data))) {
					ContentMode = UIViewContentMode.Center | UIViewContentMode.ScaleAspectFit
				};
				imageView.Bounds = new RectangleF (0, 0, maxWidth, imageView.Bounds.Height);
			} else {
				imageView = null;
			}
			if (!String.IsNullOrEmpty (text)) {
				textView = new UILabel () {
					Text = text,
					BackgroundColor = UIColor.Clear,
					Lines = 0,
					LineBreakMode = UILineBreakMode.WordWrap,
					TextAlignment = UITextAlignment.Center,
					AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight,
					ContentMode = UIViewContentMode.Center
				};
				SizeF size = textView.SizeThatFits (new SizeF (maxWidth, 999999));
				textView.Bounds = new RectangleF (0, 0, maxWidth, size.Height);
			} else {
				textView = null;
			}
			if (input != null && input.InputType == InputType.Text) {
				inputView = new UITextField (new RectangleF (frame, maxHeight - (buttons.Count + 1) * 45, maxWidth, 35)) {
					BorderStyle = UITextBorderStyle.RoundedRect,
					ReturnKeyType = UIReturnKeyType.Done
				};
				inputView.ShouldReturn += inputShouldReturn;
				if (!String.IsNullOrEmpty(saveInput))
				    inputView.Text = saveInput;
			} else {
				inputView = null;
			}

			buttonView = new UIView (){
				ContentMode = UIViewContentMode.Center
			};
			if (buttons != null && buttons.Count > 0) {
				buttonView.Bounds = new RectangleF (0, 0, maxWidth, buttons.Count * 45);
				buttonView.BackgroundColor = UIColor.Clear;
				int pos = 0;
				foreach (string s in buttons) {
					UIButton button = UIButton.FromType (UIButtonType.RoundedRect);
					button.Tag = pos;
					button.Bounds = new RectangleF (0, 0, maxWidth, 35);
					button.Frame = new RectangleF (0, pos * 45, maxWidth, 35);
					button.HorizontalAlignment = UIControlContentHorizontalAlignment.Center;
					button.AutoresizingMask = UIViewAutoresizing.FlexibleTopMargin;
					button.SetTitleColor(Colors.ButtonText,UIControlState.Normal);
					button.SetBackgroundImage(Images.BlueButton, UIControlState.Normal);
					button.SetBackgroundImage(Images.BlueButtonHighlight, UIControlState.Highlighted);
					button.SetTitle (s, UIControlState.Normal);
					button.TouchUpInside += OnTouchUpInside;
					buttonView.AddSubview (button);
					buttonViews.Add (button);
					pos++;
				}
			} else {
				buttonView = null;
			}
			
			// Create scroll view, which holds all other views
			scrollView = new UIScrollView (new RectangleF (0, 0, this.View.Bounds.Width, maxHeight)) {
				BackgroundColor = UIColor.White,
				ScrollEnabled = true
			};

			float height = 10;
			if (imageView != null) {
				imageView.Frame = new RectangleF (frame, height, imageView.Bounds.Width, imageView.Bounds.Height);
				scrollView.AddSubview (imageView);
				height += imageView.Bounds.Height + frame;
			}
			if (textView != null) {
				textView.Frame = new RectangleF (frame, height, textView.Bounds.Width, textView.Bounds.Height);
				scrollView.AddSubview (textView);
				height += textView.Bounds.Height + frame;
			}
			if (inputView != null) {
				textView.Frame = new RectangleF (frame, height, inputView.Bounds.Width, inputView.Bounds.Height);
				scrollView.AddSubview (inputView);
				height += inputView.Bounds.Height + frame;
			}
			if (buttonView != null) { 
				if (height + buttonView.Bounds.Height + this.NavigationController.NavigationBar.Bounds.Height < maxHeight)
					height = maxHeight - buttonView.Bounds.Height - this.NavigationController.NavigationBar.Bounds.Height;
				buttonView.Frame = new RectangleF ( frame, height, buttonView.Bounds.Width, buttonView.Bounds.Height);
				scrollView.AddSubview (buttonView);
				height += buttonView.Bounds.Height;
			}
			
			scrollView.ContentSize = new SizeF(scrollView.Frame.Width,height);
		
			this.View.AddSubview (scrollView);
			this.View.BackgroundColor = UIColor.Clear;
		}

		void Refresh()
		{
			float height = 10;
			float frame = 10;
			float maxWidth = this.View.Bounds.Width - 2 * frame;
			float maxHeight = this.View.Bounds.Height;
			
			scrollView.Frame = new RectangleF(0,0,this.View.Bounds.Width,maxHeight);
			
			if (imageView != null) {
				imageView.Frame = new RectangleF (frame, height, maxWidth, imageView.Bounds.Height);
				height += imageView.Bounds.Height + frame;
			}
			if (textView != null) {
				SizeF size = textView.SizeThatFits (new SizeF (maxWidth, 999999));
				textView.Bounds = new RectangleF (0, 0, maxWidth, size.Height);
				textView.Frame = new RectangleF (frame, height, textView.Bounds.Width, textView.Bounds.Height);
				height += textView.Bounds.Height + frame;
			}
			if (inputView != null) {
				inputView.Frame = new RectangleF (frame, height, maxWidth, inputView.Bounds.Height);
				height += inputView.Bounds.Height + frame;
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

			scrollView.ContentSize = new SizeF(scrollView.Frame.Width,height);
		}

	}
	
}

