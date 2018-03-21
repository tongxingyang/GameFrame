using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame
{
	public class AssetCacheManager :Singleton<AssetCacheManager>
	{
		private Dictionary<string, IAssetCache> mCaches = null;
		public Dictionary<string,IAssetCache> Caches {get { return mCaches; }}
		public override void Init()
		{
			base.Init();
			mCaches = new Dictionary<string, IAssetCache>();
		}

		public void CacheSourceAsset(string path, bool permanent, Object asset)
		{
			if(!asset) return;
			AssetCache cache = GetCache(path) as AssetCache;
			if (cache == null)
			{
				cache = new AssetCache();
				CacheAsset(path,permanent,cache,asset);
			}
			cache.SetLastUseTime(Time.time);
		}
		public Object GetCacheSourceAsset(string name)
		{
			IAssetCache cache = GetCache(name);
			if (cache != null)
			{
				cache.SetLastUseTime(Time.time);
				return cache.Asset;
			}
			return null;
		}
		public void CacheAssetBundle(string path,bool permanent,AssetBundle asset)
		{
			if(!asset) return;
			AssetBundleCache cache = GetCache(path) as AssetBundleCache;
			if (cache == null)
			{
				cache = new AssetBundleCache();
				CacheAsset(path,permanent,cache,asset);
			}
			cache.SetLastUseTime(Time.time);
		}
		public AssetBundle GetCacheAssetBundle(string name)
		{
			IAssetCache cache = GetCache(name);
			if (cache != null)
			{
				cache.SetLastUseTime(Time.time);
				return cache.Asset as AssetBundle;
			}
			return null;
		}
		/// <summary>
		/// 获取assetbundlecache接口
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public AssetBundleCache GetAssetBundleCache(string name)
		{
			IAssetCache assetBundleCache;
			mCaches.TryGetValue(name, out assetBundleCache);
			return assetBundleCache as AssetBundleCache;
		}
		/// <summary>
		/// 增加加载依赖 的引用计数
		/// </summary>
		/// <param name="name"></param>
		public void AddAssetBundleCacheRefCount(string name)
		{
			AssetBundleCache assetBundleCache = GetAssetBundleCache(name);
			if (assetBundleCache != null)
			{
				assetBundleCache.AddRefCount();
			}
		}
		/// <summary>
		/// 减少加载依赖 的引用计数
		/// </summary>
		/// <param name="name"></param>
		public void SubAssetBundleCacheRefCount(string name)
		{
			AssetBundleCache assetBundleCache = GetAssetBundleCache(name);
			if (assetBundleCache != null)
			{
				assetBundleCache.SubRefCount();
			}
		}
		private void CacheAsset(string name, bool permanent, IAssetCache cache, Object asset)
		{
			cache.SetName(name);
			cache.SetPermanentMemory(permanent);
			cache.SetAsset(asset);
			
			if (!mCaches.ContainsKey(name))
			{
				
				{
					mCaches.Add(name , cache);
				}
			}
			else
			{
				cache.Dispose();
			}
		}
		public IAssetCache GetCache(string name)
		{
			IAssetCache cache;
			mCaches.TryGetValue(name, out cache);
			return cache;
		}
		public void Clear()
		{
			using (var enumer = mCaches.GetEnumerator())
			{
				while (enumer.MoveNext())
				{
					enumer.Current.Value.Dispose();
				}
				mCaches.Clear();
				Resources.UnloadUnusedAssets();
			}
		}
	}
}
