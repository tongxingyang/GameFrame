using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame
{
	/// <summary>
	/// asset 资源缓存
	/// </summary>
	public class AssetCache : IAssetCache
	{
		public string Name { get; private set; }
		public Object Asset { get; private set; }
		public bool Permanent { get;  set; }
		public CachePriority Priority { get; private set; }
		public float LastUseTime { get; private set; }

		public bool IsDisposeable
		{
			get
			{
				if (Permanent) return false;
				return Asset == null || (Time.time - LastUseTime) > CacheDisposeTime;
			}
		}

		private float CacheDisposeTime { get; set; }

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
				if (Asset is GameObject)
				{
					Object.Destroy(Asset);
				}
				else
				{
					Resources.UnloadAsset(Asset);
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
				if (Asset is GameObject)
				{
					Object.Destroy(Asset);
				}
				else
				{
					Resources.UnloadAsset(Asset);
				}
				Asset = null;
			}
		}
	}
}
