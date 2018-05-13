using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ProtoBuf;
using UIFrameWork;
using UnityEngine;


	public class Tools
	{

		public const string UI_PREFABPATH = "UIPrefab/";
		

		public static string GetPrefabPathByType(enWindowType type)
		{
			string _path = String.Empty;
			switch (type)
			{
				case enWindowType.LoginAndRegister:
					//_path = UI_PREFABPATH + "/Login";
					_path = "LoginAndRegister";
					break;
			    case enWindowType.GameInfo:
			        //_path = UI_PREFABPATH + "/Login";
			        _path = "GameInfo";
			        break;
			    case enWindowType.MainUI:
			        //_path = UI_PREFABPATH + "/Login";
			        _path = "MainUI";
			        break;
			    case enWindowType.FriendUI:
			        //_path = UI_PREFABPATH + "/Login";
			        _path = "FriendUI";
			        break;
        }
			return _path;
		}
}

