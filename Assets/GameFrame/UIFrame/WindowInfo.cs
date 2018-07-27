using System;
using UnityEngine;

namespace UIFrameWork
{	
	/// <summary>
	/// window窗口信息类
	/// </summary>
	[Serializable]
	public class WindowInfo
	{
		[SerializeField] public enWindowType Type;
		[SerializeField] public string Name;
		[SerializeField] public enWindowColliderMode ColliderMode;
		[SerializeField] public bool IsSinglen;
		[SerializeField] public enWindowPriority Priority;
		[SerializeField] public int Group;
		[SerializeField] public bool FullScreenBG;
		[SerializeField] public bool DisableInput;
		[SerializeField] public bool HideUnderForms;
		[SerializeField] public bool AlwaysKeepVisible;
		public WindowInfo( enWindowType windowType,enWindowColliderMode windowcolliderMode,bool issinglen,enWindowPriority priority,int group,bool isfullbg,bool isdisableinput
			,bool hideunderinput,bool alwayskeepxisible) 
		{
			Type = windowType;
			ColliderMode = windowcolliderMode;
			Name = windowType.ToString();
			IsSinglen = issinglen;
			Priority = priority;
			Group = group;
			FullScreenBG = isfullbg;
			DisableInput = isdisableinput;
			HideUnderForms = hideunderinput;
			AlwaysKeepVisible = alwayskeepxisible;
		}
	
	}
}


