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

	public partial class ScreenDetail : UIViewController
	{
		float frame = 10;
		List<UIButton> buttons = new List<UIButton> ();
		UIView buttonView;
		UIImageView image;
		UIScrollView scrollView;
		UILabel text;
		UILabel actionText;
		Command actionCommand;
		string actionCommandEmpty;

		public ScreenDetail (ScreenController ctrl, UIObject obj) : base ()
		{
			this.ctrl = ctrl;
			this.obj = obj;

			// OS specific details
			if (new Version (UIDevice.CurrentDevice.SystemVersion) >= new Version(7,0)) 
			{
				// Code that uses features from Xamarin.iOS 7.0
				this.EdgesForExtendedLayout = UIRectEdge.None;
			}
		}

		public UIObject Item { get { return obj; } }

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
			// Get all commands for this object
			commands = null;
			targets = null;
			if (!(obj is Task)) {
				commands = ((Thing)obj).ActiveCommands;
//				foreach(Command c in ((Thing)obj).ActiveCommands)
//					commands.Add (c);
			}
			// Create view
			CreateView ();
		}

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear(animated);
			this.NavigationController.SetNavigationBarHidden(false,false);

			StartEvents ();

			Refresh();
		}

		public override void ViewDidDisappear (bool animated)
		{
			base.ViewDidDisappear (animated);

			StopEvents ();
		}

		public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation) 
		{
			base.DidRotate(fromInterfaceOrientation);

			Refresh();
		}

		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			// We support all orientations
			return true;
		}

		private void OnTouchUpInside (object sender, EventArgs e)
		{
			if (actionCommand == null) {
				Command command = commands[((UIButton)sender).Tag];
				if (command.CmdWith) {
					// This command did have a list of things, with which it works, so show this list
					targets = command.TargetObjects;
					// If things has no entry, than there are no targets for this command
					if (targets.Count == 0) {
						actionCommandEmpty = command.EmptyTargetListText;
					}
					actionCommand = command;
					Refresh ();
				} else {
					// This command didn't have a list of things, with which it works, so call the method
					command.Execute ();
				}
			} else {
				// Player select 
				if (String.IsNullOrEmpty(actionCommandEmpty)) {
					actionCommand.Execute(targets[((UIButton)sender).Tag]);
				}
				actionCommand = null;
				actionCommandEmpty = null;
				commands = null;
				targets = null;
				if (!(obj is Task)) {
					commands = ((Thing)obj).ActiveCommands;
				}
				Refresh ();
			}
		}

		#endregion

		#region Private Functions

		void CreateView ()
		{
			// Remove all existing subviews
			foreach (UIView view in this.View.Subviews) {
				view.RemoveFromSuperview ();
			}

			float frame = 10;
			float maxWidth = this.View.Bounds.Width - 2 * frame;
			float maxHeight = this.View.Bounds.Height;

			if (image == null)
				image = new UIImageView () {
					BackgroundColor = UIColor.Clear, // UIColor.Yellow, 
					ContentMode = UIViewContentMode.Center | UIViewContentMode.ScaleAspectFit,
					Hidden = true
				};

			if (text == null)
				text = new UILabel () {
				BackgroundColor = UIColor.Clear, // UIColor.Red,
					Lines = 0,
					LineBreakMode = UILineBreakMode.WordWrap,
					TextAlignment = UITextAlignment.Center,
					AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight,
					ContentMode = UIViewContentMode.Center,
					Hidden = true
				};

			if (actionText == null)
				actionText = new UILabel(new RectangleF(0, 0, maxWidth, 35)) {
					BackgroundColor = UIColor.Clear, // UIColor.Green,
					Lines = 0,
					LineBreakMode = UILineBreakMode.WordWrap,
					TextAlignment = UITextAlignment.Center,
					AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight,
					ContentMode = UIViewContentMode.Center,
					Hidden = true
				};

			if (buttonView == null)
				buttonView = new UIView () {
					BackgroundColor = UIColor.Clear, // UIColor.Blue
					ContentMode = UIViewContentMode.Center,
					Hidden = true
				};

			// Create scroll view, which holds all other views
			if (scrollView == null) {
				scrollView = new UIScrollView (new RectangleF (0, 0, this.View.Bounds.Width, maxHeight)) {
					BackgroundColor = UIColor.White,
					ScrollEnabled = true
				};

				scrollView.AddSubview (image);
				scrollView.AddSubview (text);
				scrollView.AddSubview (actionText);
				scrollView.AddSubview (buttonView);
			}

			this.View.AddSubview (scrollView);
			this.View.BackgroundColor = UIColor.Clear;
		}

		void Refresh(string what = "")
		{
			float maxWidth = this.View.Bounds.Width - 2 * frame;
			float maxHeight = this.View.Bounds.Height;

			scrollView.Frame = new RectangleF (0, 0, this.View.Bounds.Width, maxHeight);

			if (what.Equals ("") || what.Equals ("Name")) 
			{
				if (obj is Task)
					this.NavigationItem.Title = (((Task)obj).Complete ? (((Task)obj).CorrectState == TaskCorrectness.NotCorrect ? Strings.TaskNotCorrect : Strings.TaskCorrect) + " " : "") + obj.Name;
				else
					this.NavigationItem.Title = obj.Name;
			}

			if (what.Equals ("") || what.Equals ("Media")) {
				if (obj.Image != null) {
					image.Image = UIImage.LoadFromData (NSData.FromArray (obj.Image.Data));
					image.Hidden = false;
					if (image.Image.Size.Width > image.Image.Size.Height)
						image.Bounds = new RectangleF (0, 0, maxWidth, image.Image.Size.Height * maxWidth / image.Image.Size.Width);
					else
						image.Bounds = new RectangleF (0, 0, maxWidth, image.Image.Size.Height);
				} else {
					image.Image = null;
					image.Hidden = true;
				}
			}

			if (what.Equals ("") || what.Equals ("Description")) {
				if (!String.IsNullOrWhiteSpace (obj.Description)) {
					text.Text = obj.Description;
					SizeF size = text.SizeThatFits (new SizeF (maxWidth, 999999));
					text.Bounds = new RectangleF (0, 0, maxWidth, size.Height);
					text.Hidden = false;
				} else {
					text.Hidden = true;
				}
			}

			// Check, if commands changed
			if (what.Equals ("Commands")) {
				// If commds have changed, than abort target selection
				actionCommand = null;
				actionCommandEmpty = null;
			}

			if (commands != null && ((what.Equals ("") && actionCommand == null) || what.Equals ("Commands"))) {
				// Set size
				buttonView.Bounds = new RectangleF (0, 0, maxWidth, 45 * commands.Count);
				// Make all buttons we need
				MakeButtons (commands.Count);
				// Set text for buttons
				for (int i = 0; i < commands.Count; i++)
					buttons [i].SetTitle (commands [i].Text, UIControlState.Normal);

				buttonView.Hidden = commands.Count == 0;
			}

			if (actionCommand != null) {
				actionText.Text = String.IsNullOrEmpty (actionCommandEmpty) ? actionCommand.Text : actionCommandEmpty;
				actionText.Hidden = false;
				SizeF size = actionText.SizeThatFits (new SizeF (maxWidth, 999999));
				actionText.Bounds = new RectangleF (0, 0, maxWidth, size.Height);
				// Now create buttons for targets
				int num = String.IsNullOrEmpty (actionCommandEmpty) ? targets.Count : 1;
				// Make all buttons we need
				MakeButtons (num);
				if (String.IsNullOrEmpty (actionCommandEmpty)) {
					// Show targets
					for (int i = 0; i < targets.Count; i++) {
						buttons [i].SetTitle (targets [i].Name, UIControlState.Normal);
						buttons [i].Tag = i;
					}
				}
				else
					// Show empty text
					buttons[0].SetTitle ("Ok", UIControlState.Normal);

				buttonView.Hidden = num == 0;
			}
			else 
			{
				actionText.Hidden = true;
			}

			// Calc new size
			float height = frame;

			if (!image.Hidden) {
				image.Frame = new RectangleF (frame, height, image.Bounds.Width, image.Bounds.Height);
				height += image.Bounds.Height + frame;
			}
			if (!text.Hidden) {
				text.Frame = new RectangleF (frame, height, text.Bounds.Width, text.Bounds.Height);
				height += text.Bounds.Height + frame;
			}
			if (!actionText.Hidden) {
				actionText.Frame = new RectangleF (frame, height, actionText.Bounds.Width, actionText.Bounds.Height);
				height += actionText.Bounds.Height + frame;
			}
			if (!buttonView.Hidden) { 
				if (height + buttonView.Bounds.Height < maxHeight)
					height = maxHeight - buttonView.Bounds.Height;
				buttonView.Frame = new RectangleF (frame, height, buttonView.Bounds.Width, buttonView.Bounds.Height);
				height += buttonView.Bounds.Height;
			}

			scrollView.ContentSize = new SizeF(scrollView.Frame.Width,height);
		}

		void MakeButtons(int num)
		{
			float maxWidth = this.View.Bounds.Width - 2 * frame;
			float maxHeight = this.View.Bounds.Height;

			// Set size
			buttonView.Bounds = new RectangleF (0, 0, maxWidth, 45 * num);

			// Delete buttons
			while (buttons.Count > num) {
				buttons[buttons.Count-1].RemoveFromSuperview ();
				buttons.RemoveAt(buttons.Count-1);
			}
			// Create new buttons
			while (buttons.Count < num) {
				int pos = buttons.Count;
				UIButton button = UIButton.FromType (UIButtonType.RoundedRect);
				button.Tag = pos;
				button.Bounds = new RectangleF (0, 0, maxWidth, 35);
				button.HorizontalAlignment = UIControlContentHorizontalAlignment.Center;
				button.AutoresizingMask = UIViewAutoresizing.FlexibleTopMargin;
				button.SetTitleColor(Colors.ButtonText,UIControlState.Normal);
				button.SetBackgroundImage(Images.Button, UIControlState.Normal);
				button.SetBackgroundImage(Images.ButtonHighlight, UIControlState.Highlighted);
				button.TouchUpInside += OnTouchUpInside;
				buttons.Add (button);
				buttonView.AddSubview (button);
			}

			for (int i = 0; i < buttons.Count; i++)
				buttons[i].Frame = new RectangleF (0, i * 45, maxWidth, 35);
		}

		#endregion

	}

}

