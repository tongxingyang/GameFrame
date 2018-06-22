using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame
{
	public class AssetBundleCache : IAssetCache
	{
		public string Name { get; private set; }
		public Object Asset { get; private set; }
		public bool Permanent { get; set; }
		public CachePriority Priority { get; private set; }
		public float LastUseTime { get; private set; }

		public bool IsDisposeable
		{
			get
			{
				if (refCount > 0)
				{
					return false;
				}
				else
				{
					if (Permanent) return false;
					return Asset == null || (Time.time - LastUseTime) > CacheDisposeTime;
				}
			}
		}
		public float CacheDisposeTime { get; set; }
		
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
		public void SetName(string name)
		{
			Name = name;
		}

		public void SetAsset(Object obj)
		{
			Asset = obj;
		}

		public void SetLastUseTime(float time)
		{
			LastUseTime = time;
		}

		public void SetCachePriority(CachePriority cachePriority)
		{
			Priority = cachePriority;
			switch (Priority)
			{
				case CachePriority.MiddleTime:
					CacheDisposeTime = 150f;
					Permanent = false;
					break;
				case CachePriority.ShortTime:
					CacheDisposeTime = 60f;
					Permanent = false;
					break;
				case CachePriority.LongTime:
					CacheDisposeTime = 300f;
					Permanent = false;
					break;
				case CachePriority.Persistent:
					CacheDisposeTime = 0;
					Permanent = true;
					break;
				default:
					CacheDisposeTime = 0;
					Permanent = false;
					break;
			}
		}

		public bool Dispose()
		{
			if (Asset && !Permanent && IsDisposeable)
			{
				if (Asset is AssetBundle)
				{
					(Asset as AssetBundle).Unload(false);
				}
				Asset = null;
				return true;
			}
			return false;
		}

		public void ForceDispose()
		{
			if (Asset)
			{
				if (Asset is AssetBundle)
				{
					(Asset as AssetBundle).Unload(false);
				}
				Asset = null;
			}
		}
	}
}
