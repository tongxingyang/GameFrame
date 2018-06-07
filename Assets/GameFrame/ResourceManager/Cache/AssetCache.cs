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
		protected string name;
		public string Name { get { return name; } }
		protected Object asset;
		public Object Asset { get { return asset; } }
		protected bool permanentMemory;
		public bool PermanentMemory { get { return permanentMemory;} }
		
		protected float lastUseTime;
		public float LastUseTime { get { return lastUseTime; } }

		protected virtual float CacheDisposeTime
		{
			get { return 300f; }
		}
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
			this.name = name;
		}

		public void SetAsset(Object asset)
		{
			this.asset = asset;
		}
		
		public void SetPermanentMemory(bool parm)
		{
			permanentMemory = parm;
		}
		
		public void SetLastUseTime(float time)
		{
			lastUseTime = time;
		}
		public virtual bool IsDisposeable {
			get
			{
				if (permanentMemory) return false;
				if (refCount == 0)
				{
					return asset == null || Time.time - lastUseTime >= CacheDisposeTime;
				}
				return false;
			}
		}
		public virtual void Dispose()
		{
			if (asset)
			{
				if (!permanentMemory)//不是常驻内存 才会处理
				{
//					if (asset is GameObject)
//					{
//						Object.DestroyImmediate(asset, true);//gameobject 
//					}
//					else
					{
						Resources.UnloadAsset(asset); // 普通资源类型
					}
					asset = null;
				}
				
			}
		}

		public virtual void ForcedDispose()
		{
			if (asset)
			{
//				if (asset is GameObject)
//				{
//					Object.DestroyImmediate(asset, true);
//				}
//				else
				{
					Resources.UnloadAsset(asset);
				}
				asset = null;
			}
		}
	}
}
