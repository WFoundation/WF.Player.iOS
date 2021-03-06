///
/// WF.Player.iPhone - A Wherigo Player for iPhone which use the Wherigo Foundation Core.
/// Copyright (C) 2012-2014  Dirk Weltz <mail@wfplayer.com>
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
using Vernacular;
using WF.Player.Core;

namespace WF.Player.iOS
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
			this.activeObject = obj;

			// OS specific details
			if (new Version (UIDevice.CurrentDevice.SystemVersion) >= new Version(7,0)) 
			{
				// Code that uses features from Xamarin.iOS 7.0
				this.EdgesForExtendedLayout = UIRectEdge.None;
			}
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
			// Get all commands for this object

			// Show back button
			NavigationItem.SetLeftBarButtonItem(new UIBarButtonItem(Catalog.GetString("Back"),UIBarButtonItemStyle.Plain, (sender,args) => { 
				ctrl.ButtonPressed(null);
				ctrl.RemoveScreen(ScreenType.Details); 
			}), false);
			NavigationItem.LeftBarButtonItem.TintColor = Colors.NavBarButton;
			NavigationItem.SetHidesBackButton (false, false);

			commands = null;
			targets = null;
			if (!(activeObject is Task)) {
				commands = ((Thing)activeObject).ActiveCommands;
//				foreach(Command c in ((Thing)obj).ActiveCommands)
//					commands.Add (c);
			}
			// Create view
			CreateViews ();
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
			// Play sound and vibrate, if set in the settings
			ctrl.ButtonPressed ((UIButton)sender);

			if (actionCommand == null) {
				Command command = commands[((UIButton)sender).Tag];
				if (command.CmdWith) {
					// This command did have a list of things, with which it works, so show this list
					targets = command.TargetObjects;
					// If things has no entry, than there are no targets for this command
					if (targets.Count == 0) {
						actionCommandEmpty = Catalog.GetString(command.EmptyTargetListText);
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
				if (!(activeObject is Task)) {
					commands = ((Thing)activeObject).ActiveCommands;
				}
				Refresh ();
			}
		}

		#endregion

		#region Private Functions

		void CreateViews ()
		{
			// Remove all existing subviews
			foreach (UIView view in this.View.Subviews) {
				view.RemoveFromSuperview ();
			}

			float frame = Values.Frame;
			float maxWidth = this.View.Bounds.Width - 2 * frame;
			float maxHeight = this.View.Bounds.Height;

			UITextAlignment textAlign = UITextAlignment.Center;
			int format = NSUserDefaults.StandardUserDefaults.IntForKey("TextAlignment");

			switch (format) {
				case 0:
					textAlign = UITextAlignment.Center;
					break;
				case 1:
					textAlign = UITextAlignment.Left;
					break;
				case 2:
					textAlign = UITextAlignment.Right;
					break;
			}

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
					TextAlignment = textAlign,
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

		public void Refresh(string what = "")
		{
			float maxWidth = this.View.Bounds.Width - 2 * frame;
			float maxHeight = this.View.Bounds.Height;

			if ((activeObject is Zone || (activeObject is Thing && ctrl.Engine.VisibleObjects.Contains ((Thing)activeObject))) && activeObject.ObjectLocation != null)
				NavigationItem.SetRightBarButtonItem (new UIBarButtonItem (Catalog.GetString("Map"), UIBarButtonItemStyle.Plain, (sender, args) => {
					ctrl.ShowScreen(ScreenType.Map, activeObject);
				}), false);

			scrollView.Frame = new RectangleF (0, 0, this.View.Bounds.Width, maxHeight);

			if (what.Equals ("") || what.Equals ("Name")) 
			{
				string name = activeObject.Name == null ? "" : activeObject.Name;
				if (activeObject is Task)
					this.NavigationItem.Title = (((Task)activeObject).Complete ? (((Task)activeObject).CorrectState == TaskCorrectness.NotCorrect ? Strings.TaskNotCorrect : Strings.TaskCorrect) + " " : "") + name;
				else
					this.NavigationItem.Title = name;
			}

			if (what.Equals ("") || what.Equals ("Media")) {
				if (activeObject.Image != null) {
					image.Image = UIImage.LoadFromData (NSData.FromArray (activeObject.Image.Data));
					image.Hidden = false;
					if (image.Image.Size.Width > maxWidth)
						image.Bounds = new RectangleF (0, 0, maxWidth, image.Image.Size.Height * maxWidth / image.Image.Size.Width);
					else
						image.Bounds = new RectangleF (0, 0, maxWidth, image.Image.Size.Height);
				} else {
					image.Image = null;
					image.Hidden = true;
				}
			}

			if (what.Equals ("") || what.Equals ("Description")) {
				if (!String.IsNullOrWhiteSpace (activeObject.Description)) {
					text.Text = activeObject.Description;
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
					buttons [i].SetTitle ((commands [i].Text == null ? "" : commands[i].Text), UIControlState.Normal);

				buttonView.Hidden = commands.Count == 0;
			}

			if (actionCommand != null) {
				actionText.Text = String.IsNullOrEmpty (actionCommandEmpty) ? (actionCommand.Text == null ? "" : actionCommand.Text) : actionCommandEmpty;
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
						buttons [i].SetTitle ((targets [i].Name == null ? "" : targets[i].Name), UIControlState.Normal);
						buttons [i].Tag = i;
					}
				}
				else
					// Show empty text
					buttons[0].SetTitle (Catalog.GetString("Ok"), UIControlState.Normal);

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
				button.SetBackgroundImage(Images.BlueButton, UIControlState.Normal);
				button.SetBackgroundImage(Images.BlueButtonHighlight, UIControlState.Highlighted);
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

