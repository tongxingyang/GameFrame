using System;
using System.Collections.Generic;

namespace UIFrameWork
{

	public struct WindowInfo
	{
		public WindowType WindowType { get; private set; }
		public string PerfabPath { get; private set; }
		public string Name { get; private set; }
		public Type Script { get; private set; }
		public ShowMode ShowMode { get; private set; }
		public OpenAction OpenAction { get; private set; }
		public ColliderMode ColliderMode { get; private set; }

		public WindowInfo( WindowType windowType,ShowMode showMode, OpenAction openAction,
			ColliderMode colliderMode) : this()
		{
			PerfabPath = Tools.GetPrefabPathByType(windowType);
			Script = Tools.GetUIScripeByType(windowType);
			WindowType = windowType;
			ShowMode = showMode;
			OpenAction = openAction;
			ColliderMode = colliderMode;
		    Name = windowType.ToString();
		}
	
	}

	public class WindowStackData
	{
		public WindowInfo WindowInfo;
		public WindowBase WindowBase;
		public List<WindowBase> HistoryWindowBases = null;
		public WindowBase RecordeWindowBase = null;
	}

}


