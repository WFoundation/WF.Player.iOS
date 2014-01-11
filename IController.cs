///
/// WF.Player.iOS/WF.Player.Android - A Wherigo Player for Android, iPhone which use the Wherigo Foundation Core.
/// Copyright (C) 2012-2014  Dirk Weltz <web@weltz-online.de>
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
using System.IO;
using WF.Player.Core;
using WF.Player.Core.Engines;

namespace WF.Player.iOS
{
	public interface IController
	{
//		private Cartridge cartridge;
//		private Engine engine;
//		private ScreenMain screenMain;
//		private ScreenList screenList;
//		private ScreenDetail screenDetail;
//		private object locationManager;
//		private TextWriter logFile;
//		private LogLevel logLevel = LogLevel.Error;
//		private ScreenType activeScreen;
//		private UIObject activeObject;

		#region Engine Handling

		[CLSCompliantAttribute(false)]
		void CreateEngine (Cartridge cart);

		void DestroyEngine();

		void Pause();

		void Restore();

		void Resume ();

		void Save();

		void Start();

		#endregion

		#region Events

		[CLSCompliantAttribute(false)]
		void OnAttributeChanged (Object sender, AttributeChangedEventArgs e);

		[CLSCompliantAttribute(false)]
		void OnCartridgeComplete (object sender, WherigoEventArgs args);

		[CLSCompliantAttribute(false)]
		void OnGetInput (Object sender, ObjectEventArgs<Input> input);

		[CLSCompliantAttribute(false)]
		void OnInventoryChanged (Object sender, InventoryChangedEventArgs e);

		[CLSCompliantAttribute(false)]
		void OnLogMessage (Object sender, LogMessageEventArgs args);

		[CLSCompliantAttribute(false)]
		void OnPlayMedia (Object sender, ObjectEventArgs<Media> media);

		[CLSCompliantAttribute(false)]
		void OnSaveCartridge (object sender, SavingEventArgs args);

		[CLSCompliantAttribute(false)]
		void OnShowMessageBox (Object sender, MessageBoxEventArgs args);

		[CLSCompliantAttribute(false)]
		void OnShowScreen (Object sender, ScreenEventArgs args);

		[CLSCompliantAttribute(false)]
		void OnShowStatusText (Object sender, StatusTextEventArgs args);

		[CLSCompliantAttribute(false)]
		void OnStopSound (object sender, WherigoEventArgs args);

		[CLSCompliantAttribute(false)]
		void OnZoneStateChanged (Object sender, ZoneStateChangedEventArgs e);

		#endregion

		#region Helper Functions

		[CLSCompliantAttribute(false)]
		void RemoveScreen (ScreenType last);

		[CLSCompliantAttribute(false)]
		void ShowScreen (ScreenType screenId, object param = null);

		[CLSCompliantAttribute(false)]
		void StartSound (Media mediaObj);

		void StopSound ();

		#endregion
	}
}

