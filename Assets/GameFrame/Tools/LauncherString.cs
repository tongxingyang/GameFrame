using System.Collections;
using System.Collections.Generic;
using GameFrame;
using UnityEngine;

public class LauncherString :Singleton<LauncherString> {
	private  StringFile stringFile = null;
	public override void Init()
	{
		base.Init();
		stringFile = new StringFile();
		stringFile.ReadResources("launcher");
	}
	public string GetString(string key)
	{
		return stringFile.GetString(key);
	}
}
