using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame
{
	public enum enResourceLoadType  {
		Default = 0,
		Persistent = 1 << 0,
		Cache = 1 << 1,
		NotCache = 1 << 2,
		// 加载方式
		LoadBundleFromFile= 1 << 3,  // 同步
		LoadBundleFromFileAsync= 1 << 4,  //异步
		LoadBundleFromWWW = 1 << 5, //异步
		ReturnAssetBundle = 1 << 6, //返回加载的assetbundle 没啥用 先注释

	}
}
