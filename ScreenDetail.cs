using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using WF.Player.Core;
using WF.Player.Core.Engines;

namespace WF.Player.iPhone
{
	#region ScreenDetail

	public partial class ScreenDetail
	{
		ScreenController ctrl;
		Engine engine;
		UIObject obj;
		Command com;
		List<Command> commands;
		List<Thing> targets;
		string[] properties = {"Name", "Description", "Media", "Commands"};

		string taskCorrect = Encoding.UTF8.GetString(new byte[] { 0xE2, 0x9C, 0x93 } );    // UTF-8 2713
		string taskNotCorrect = Encoding.UTF8.GetString(new byte[] { 0xE2, 0x9C, 0x97 } );  // UTF-8 2717


		#region Common Functions

		void StartEvents()
		{
			obj.PropertyChanged += OnPropertyChanged;
		}

		void StopEvents()
		{
			obj.PropertyChanged -= OnPropertyChanged;
		}

		public void OnPropertyChanged(object sender,  PropertyChangedEventArgs e)
		{
			bool remove = false;

			if (e.PropertyName.Equals("Commands"))
			    commands = ((Thing)obj).ActiveCommands;

			// Check, if one of the visible entries changed
			if (!(e is PropertyChangedEventArgs) || (e is PropertyChangedEventArgs && properties.Contains(((PropertyChangedEventArgs)e).PropertyName)))
				Refresh(e.PropertyName);

			// The object is set to not visible or not active, so it should removed from screen
			if (e.PropertyName.Equals("Visible") || e.PropertyName.Equals("Active"))
				remove = !obj.Visible;
			// The object is moved to nil, so it should removed from screen
			if (e.PropertyName.Equals("Container") && !(obj is Task) && ((Thing)obj).Container == null)
				remove = true;

			if (remove)
				ctrl.RemoveScreen(ScreenType.Details);
		}

		#endregion

	}

	#endregion

}

