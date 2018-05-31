using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame
{
	public class ManagedResource : MonoBehaviour
	{
		public string bundleName;

		public void OnDestory()
		{
//			Debug.Log("destory :  "+bundleName);
//			Singleton<ResourceManager>.GetInstance().DelRefCount(bundleName);
		}
	}
}
