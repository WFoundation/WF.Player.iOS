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
using MonoTouch.UIKit;
using WF.Player.Core;

namespace WF.Player.iPhone
{

	public class MessageEntry
	{
		public const int sqeInput = 0;
		public const int sqeChoice = 1;
		public const int sqeMessage = 2;

		public int Type;
		public UIImage Image;
		public string Text;
		public string Title;
		public string Edit;
		public List<string> Buttons = new List<string> ();
		public CallbackFunction Callback;
		public Input Input;
		public List<Command> Commands = new List<Command> ();

		public MessageEntry ()
		{
		}

		public MessageEntry (string text, UIImage image = null)
		{
			this.Type = sqeMessage;
			this.Text = text;
			this.Image = image;
			Buttons.Add ("Ok");
			Commands.Add (null);
		}

	}

}

