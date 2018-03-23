using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame
{
	public class AssetBundleCache : AssetCache
	{
		private int refCount = 0;
		public int RefCount{get { return refCount; }}

		public void AddRefCount()
		{
			refCount++;
		}

		public int GetRefCount()
		{
			return refCount;
		}
		public void SubRefCount()
		{
			refCount--;
			if (refCount < 0)
			{
				refCount = 0;
			}
			if (refCount == 0)
			{
				SetLastUseTime(Time.time);
			}
		}
		public override bool IsDisposeable
		{
			get
			{
				if (permanentMemory)
				{
					return false;
				}
				if (refCount == 0)
				{
					return asset == null || Time.time - lastUseTime >= CacheDisposeTime;
				}
				return false;
			}
		}

		public override void Dispose()
		{
			if (asset)
			{
				if (!permanentMemory)
				{
					if (asset is AssetBundle)
					{
						AssetBundle ab = asset as AssetBundle;
						ab.Unload(false);
						asset = null;
					}
				}
			}
		}

		protected override float CacheDisposeTime
		{
			get { return 180; }
		}

		public override void ForcedDispose()
		{
			if (asset)
			{
				if (asset is AssetBundle)
				{
					AssetBundle ab = asset as AssetBundle;
					ab.Unload(false);
					asset = null;
				}
			}
		}
	}
}
