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

	public class DetailScreen : UIViewController
	{

		private ScreenController ctrl;
		private List<UIButton> buttons = new List<UIButton> ();
		private UIView buttonView;
		private UIObject obj;
		private UIImageView image;
		private UIScrollView scroll;
		private UILabel text;
		private Command actionCommand;
		private string actionCommandEmpty;
		private List<Command> commands;
		private List<Thing> things;

		public DetailScreen (ScreenController ctrl, UIObject obj) : base ()
		{
			this.ctrl = ctrl;
			this.obj = obj;
		}

		public Table Item { get { return obj; } }

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
			commands = new List<Command> ();
			things = null;
			if (!(obj is Task)) {
				foreach(Command c in ((Thing)obj).ActiveCommands)
					commands.Add (c);
			}
			// Create view
			createView ();
		}

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear(animated);
			this.NavigationController.SetNavigationBarHidden(false,false);
			resizeView();
		}

		public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation) 
		{
			base.DidRotate(fromInterfaceOrientation);
			resizeView();
		}

		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			// We support all orientations
			return true;
		}

		#endregion

		#region C# Events

		private void OnTouchUpInside (object sender, EventArgs e)
		{
			// TODO
			// Remove basic lua access
			if (actionCommand == null) {
				Command command = commands[((UIButton)sender).Tag];
				if (command.CmdWith) {
					// This command did have a list of things, with which it works, so show this list
					things = new List<Thing> ();
					foreach(Thing t in command.TargetObjects)
						things.Add (t);
					// If things has no entry, than there are no targets for this command
					if (things.Count == 0) {
						actionCommandEmpty = command.EmptyTargetListText;
					}
					actionCommand = command;
					createView ();
					resizeView ();
				} else {
					// This command didn't have a list of things, with which it works, so call the method
					command.Execute ();
				}
			} else {
				// Player select 
				ctrl.ShowScreen (ScreenType.Details,obj);
				if (String.IsNullOrEmpty(actionCommandEmpty))
				actionCommand.Execute(things[((UIButton)sender).Tag]);
			}

		}

		#endregion

		private void createView ()
		{
			// Remove all existing subviews
			foreach (UIView view in this.View.Subviews) {
				view.RemoveFromSuperview ();
			}

			float frame = 10;
			float maxWidth = this.View.Bounds.Width - 2 * frame;
			float maxHeight = this.View.Bounds.Height;

			// Create image
			if (obj.Image != null) {
				image = new UIImageView (UIImage.LoadFromData (NSData.FromArray (obj.Image.Data))) {
					ContentMode = UIViewContentMode.Center | UIViewContentMode.ScaleAspectFit
				};
				image.Bounds = new RectangleF (0, 0, maxWidth, image.Bounds.Height);
			} else {
				image = null;
			}

			// Create description
			if (!String.IsNullOrEmpty(obj.Description)) {
				text = new UILabel () {
				    Text = obj.Description,
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

			// Create buttons
			if (things != null || !String.IsNullOrEmpty(actionCommandEmpty)) {
				buttonView = new UIView () {
					ContentMode = UIViewContentMode.Center
				};
				// Calc size of buttonView
				buttonView.Bounds = new RectangleF (0, 0, maxWidth, (things.Count + 1) * 45);
				buttonView.BackgroundColor = UIColor.Clear;
				UILabel action = new UILabel(new RectangleF(0, 0, maxWidth, 35)) {
					Text = String.IsNullOrEmpty(actionCommandEmpty) ? actionCommand.Text : actionCommandEmpty,
					BackgroundColor = UIColor.Clear,
					Lines = 0,
					LineBreakMode = UILineBreakMode.WordWrap,
					TextAlignment = UITextAlignment.Center,
					AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight,
					ContentMode = UIViewContentMode.Center
				};
				buttonView.AddSubview(action);
				int pos = 1;
				if (String.IsNullOrEmpty(actionCommandEmpty)) {
					foreach (Thing t in things) {
						UIButton button = UIButton.FromType (UIButtonType.RoundedRect);
						button.Tag = pos-1;
						button.Bounds = new RectangleF (0, 0, maxWidth, 35);
						button.Frame = new RectangleF (0, pos * 45, maxWidth, 35);
						button.HorizontalAlignment = UIControlContentHorizontalAlignment.Center;
						button.AutoresizingMask = UIViewAutoresizing.FlexibleTopMargin;
						button.SetTitle (t.Name, UIControlState.Normal);
						button.TouchUpInside += OnTouchUpInside;
						buttonView.AddSubview (button);
						buttons.Add (button);
						pos++;
					}
				} else {
					UIButton button = UIButton.FromType (UIButtonType.RoundedRect);
					button.Tag = pos-1;
					button.Bounds = new RectangleF (0, 0, maxWidth, 35);
					button.Frame = new RectangleF (0, pos * 45, maxWidth, 35);
					button.HorizontalAlignment = UIControlContentHorizontalAlignment.Center;
					button.AutoresizingMask = UIViewAutoresizing.FlexibleTopMargin;
					button.SetTitle ("Ok", UIControlState.Normal);
					button.TouchUpInside += OnTouchUpInside;
					buttonView.AddSubview (button);
					buttons.Add (button);

				}
			} else if (!(obj is Task) && commands.Count > 0) {
				buttonView = new UIView () {
					ContentMode = UIViewContentMode.Center
				};
				// Calc size of buttonView
				buttonView.Bounds = new RectangleF (0, 0, maxWidth, commands.Count * 45);
				buttonView.BackgroundColor = UIColor.Clear;
				int pos = 0;
				foreach (Command c in commands) {
					UIButton button = UIButton.FromType (UIButtonType.RoundedRect);
					button.Tag = pos;
					button.Bounds = new RectangleF (0, 0, maxWidth, 35);
					button.Frame = new RectangleF (0, pos * 45, maxWidth, 35);
					button.HorizontalAlignment = UIControlContentHorizontalAlignment.Center;
					button.AutoresizingMask = UIViewAutoresizing.FlexibleTopMargin;
					button.SetTitle (c.Text, UIControlState.Normal);
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
			
			if (!String.IsNullOrEmpty(obj.Name))
				this.NavigationItem.Title = obj.Name;
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

