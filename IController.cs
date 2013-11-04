using System;
using System.Collections.Generic;
using System.IO;
using WF.Player.Core;
using WF.Player.Core.Engines;

namespace WF.Player.iPhone
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
		void OnCartridgeComplete (object sender, CartridgeEventArgs args);

		[CLSCompliantAttribute(false)]
		void OnGetInput (Object sender, ObjectEventArgs<Input> input);

		[CLSCompliantAttribute(false)]
		void OnInventoryChanged (Object sender, InventoryChangedEventArgs e);

		[CLSCompliantAttribute(false)]
		void OnLogMessage (Object sender, LogMessageEventArgs args);

		[CLSCompliantAttribute(false)]
		void OnNotifyOS (Object sender, NotifyOSEventArgs args);

		[CLSCompliantAttribute(false)]
		void OnPlayMedia (Object sender, ObjectEventArgs<Media> media);

		[CLSCompliantAttribute(false)]
		void OnSaveCartridge (object sender, CartridgeEventArgs args);

		[CLSCompliantAttribute(false)]
		void OnShowMessageBox (Object sender, MessageBoxEventArgs args);

		[CLSCompliantAttribute(false)]
		void OnShowScreen (Object sender, ScreenEventArgs args);

		[CLSCompliantAttribute(false)]
		void OnShowStatusText (Object sender, StatusTextEventArgs args);

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

