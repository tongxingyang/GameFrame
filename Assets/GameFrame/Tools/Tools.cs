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
		public static Type GetUIScripeByType(WindowType type)
		{
			Type ret = null;
			switch (type)
			{
				case WindowType.LoginAndRegister:
				    ret = typeof(LoginAndRegister);
					break;
                case WindowType.GameInfo:
                    ret = typeof(GameInfo);
                    break;
			    case WindowType.MainUI:
			        ret = typeof(MainUI);
			        break;
                case WindowType.FriendUI:
                    ret = typeof(FriendUI);
                    break;
        }
			return ret;
		}

		public static string GetPrefabPathByType(WindowType type)
		{
			string _path = String.Empty;
			switch (type)
			{
				case WindowType.LoginAndRegister:
					//_path = UI_PREFABPATH + "/Login";
					_path = "LoginAndRegister";
					break;
			    case WindowType.GameInfo:
			        //_path = UI_PREFABPATH + "/Login";
			        _path = "GameInfo";
			        break;
			    case WindowType.MainUI:
			        //_path = UI_PREFABPATH + "/Login";
			        _path = "MainUI";
			        break;
			    case WindowType.FriendUI:
			        //_path = UI_PREFABPATH + "/Login";
			        _path = "FriendUI";
			        break;
        }
			return _path;
		}
}

