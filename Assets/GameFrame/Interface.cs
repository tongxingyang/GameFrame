using System.Collections;
using System.Collections.Generic;
using GameFrame;
using UnityEngine;

public class Interface :Singleton<Interface>
{
	private bool isFinish = false;
	public override void Init()
	{
		base.Init();
		InitPlugin();
	}

	public void InitPlugin()
	{
		
	}

	IEnumerator ChangeFinish()
	{
		yield return new WaitForSeconds(20);
		isFinish = true;
	}
	public bool IsCaheckSDKFinish()
	{
		return isFinish;
	}
}
