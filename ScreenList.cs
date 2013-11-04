using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using WF.Player.Core;
using WF.Player.Core.Engines;

namespace WF.Player.iPhone
{
	public partial class ScreenList
	{
		ScreenController ctrl;
		Engine engine;
		ScreenType screen;
		string[] properties = {"Name", "Icon", "Active", "Visible", "ObjectLocation"};

		public string TaskCorrect = Encoding.UTF8.GetString(new byte[] { 0xE2, 0x9C, 0x93 } );    // UTF-8 2713
		public string TaskNotCorrect = Encoding.UTF8.GetString(new byte[] { 0xE2, 0x9C, 0x97 } );  // UTF-8 2717
				
		public List<UIObject> Items = new List<UIObject>();
		public bool ShowIcons;
		public bool ShowDirections;

		#region Common Functions

		public void EntrySelected(int position)
		{
			UIObject obj = Items[position];

			if (!(obj is Zone) && obj.HasOnClick)
			{
				obj.CallOnClick();
			}
			else
			{
				ctrl.ShowScreen (ScreenType.Details, obj);
			}
		}

		public string GetContent()
		{
			string header = "";

			StopEvents();

			ShowIcons = false;
			ShowDirections = false;

			Items = new List<UIObject> ();

			switch (screen) 
			{
				case ScreenType.Locations:
					header = "Locations";
					ShowDirections = true;
					foreach (UIObject item in engine.ActiveVisibleZones) 
					{
						ShowIcons |= item.Icon != null;
						Items.Add (item);
					}
					break;
				case ScreenType.Items:
					header = "You see";
					ShowDirections = true;
					foreach (UIObject item in engine.VisibleObjects)
					{
						ShowIcons |= item.Icon != null;
						Items.Add (item);
					}
					break;
				case ScreenType.Inventory:
					header = "Inventory";
					foreach (UIObject item in engine.VisibleInventory)
					{
						ShowIcons |= item.Icon != null;
						Items.Add (item);
					}
					break;
				case ScreenType.Tasks:
					header = "Task";
					foreach (UIObject item in engine.ActiveVisibleTasks)
					{
						ShowIcons |= item.Icon != null;
						Items.Add (item);
					}
					break;
			}

			StartEvents();

			Refresh(false);

			return header;
		}

		void StartEvents()
		{
			foreach(UIObject o in Items)
				o.PropertyChanged += OnPropertyChanged;

			engine.AttributeChanged += OnPropertyChanged;
			engine.InventoryChanged += OnPropertyChanged;
			engine.ZoneStateChanged += OnPropertyChanged;

			engine.PropertyChanged += OnPropertyChanged;
		}

		void StopEvents()
		{
			foreach(UIObject o in Items)
				o.PropertyChanged -= OnPropertyChanged;

			engine.AttributeChanged -= OnPropertyChanged;
			engine.InventoryChanged -= OnPropertyChanged;
			engine.ZoneStateChanged -= OnPropertyChanged;

			engine.PropertyChanged -= OnPropertyChanged;
		}

		public void OnPropertyChanged(object sender,  EventArgs e)
		{
			bool newItems = false;

			newItems |= (e is AttributeChangedEventArgs && ((AttributeChangedEventArgs)e).PropertyName.Equals("Active"));
			newItems |= e is AttributeChangedEventArgs && ((AttributeChangedEventArgs)e).PropertyName.Equals("Visible");
			newItems |= e is InventoryChangedEventArgs;
			newItems |= e is PropertyChangedEventArgs && ((PropertyChangedEventArgs)e).PropertyName.Equals("Active");
			newItems |= e is PropertyChangedEventArgs && ((PropertyChangedEventArgs)e).PropertyName.Equals("Visible");

			// Check, if one of the visible entries changed
			if (!(e is PropertyChangedEventArgs) || (e is PropertyChangedEventArgs && properties.Contains(((PropertyChangedEventArgs)e).PropertyName)))
				Refresh(newItems);
		}

		#endregion

	}
}
