using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame
{
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
				return asset == null || Time.time - lastUseTime >= CacheDisposeTime;
			}
		}
		public virtual void Dispose()
		{
			if (asset)
			{
				if (!permanentMemory)//不是常驻内存 才会处理
				{
					if (asset is GameObject)
					{
						Object.DestroyImmediate(asset, true);
					}
					else
					{
						Resources.UnloadAsset(asset);
					}
					asset = null;
				}
				
			}
		}

		public virtual void ForcedDispose()
		{
			if (asset)
			{
				if (asset is GameObject)
				{
					Object.DestroyImmediate(asset, true);
				}
				else
				{
					Resources.UnloadAsset(asset);
				}
				asset = null;
			}
		}
	}
}
