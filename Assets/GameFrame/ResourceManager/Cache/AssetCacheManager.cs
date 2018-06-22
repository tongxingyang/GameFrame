using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameFrame
{
	/// <summary>
	/// assetbundle缓存
	/// </summary>
	public class AssetCacheManager :Singleton<AssetCacheManager>
	{
		/// <summary>
		/// bundle缓存字典
		/// </summary>
		private Dictionary<string, AssetBundleCache> mAssetBundleCaches = null;
		/// <summary>
		/// asset缓存字典
		/// </summary>
		private Dictionary<string, AssetCache> mAssetCaches = null;
		
		public Dictionary<string,AssetBundleCache> AssetBundleCaches {get { return mAssetBundleCaches; }}
		public Dictionary<string,AssetCache> AssetCaches {get { return mAssetCaches; }}
		public override void Init()
		{
			base.Init();
			mAssetCaches = new Dictionary<string, AssetCache>();
			mAssetBundleCaches = new Dictionary<string, AssetBundleCache>();
		}
		public void CacheAsset(string path, Object asset,CachePriority priority)
		{
			if(!asset) return;
			AssetCache cache = GetAssetCache(path);
			if (cache == null)
			{
				cache = new AssetCache();
				CacheAsset(path,cache,asset,priority);
			}
			cache.SetLastUseTime(Time.time);
		}
		public void CacheAssetBundle(string path,AssetBundle asset,CachePriority priority)
		{
			if(!asset) return;
			AssetBundleCache cache = GetAssetBundleCache(path) as AssetBundleCache;
			if (cache == null)
			{
				cache = new AssetBundleCache();
				CacheAssetBundle(path,cache,asset,priority);
			}
			cache.SetLastUseTime(Time.time);
		}
		
		public AssetCache GetAssetCache(string name)
		{
			AssetCache cache;
			mAssetCaches.TryGetValue(name, out cache);
			return cache;
		}

		public AssetBundleCache GetAssetBundleCache(string name)
		{
			AssetBundleCache cache;
			mAssetBundleCaches.TryGetValue(name, out cache);
			return cache;
		}
		
		private void CacheAsset(string name,AssetCache cache, Object asset,CachePriority priority)
		{
			cache.SetName(name);
			cache.SetCachePriority(priority);
			cache.SetAsset(asset);
			
			if (!mAssetCaches.ContainsKey(name))
			{
				{
					mAssetCaches.Add(name , cache);
				}
			}
			else
			{
				cache.Dispose();
			}
		}
		private void CacheAssetBundle(string name, AssetBundleCache cache, Object asset,CachePriority priority)
		{
			cache.SetName(name);
			cache.SetCachePriority(priority);
			cache.SetAsset(asset);
			
			if (!mAssetBundleCaches.ContainsKey(name))
			{
				{
					mAssetBundleCaches.Add(name , cache);
				}
			}
			else
			{
				cache.Dispose();
			}
		}
		
		/// <summary>
		/// 获取Asset资源缓存
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public Object GetCacheAsset(string name)
		{
			AssetCache cache = GetAssetCache(name);
			if (cache != null)
			{
				cache.SetLastUseTime(Time.time);
				return cache.Asset;
			}
			return null;
		}

		public AssetBundle GetCacheAssetBundle(string name)
		{
			AssetBundleCache cache = GetAssetBundleCache(name);
			if (cache != null)
			{
				cache.SetLastUseTime(Time.time);
				return cache.Asset as AssetBundle;
			}
			return null;
		}
		
			
		/// <summary>
		/// 增加缓存的AssetBundle的引用计数
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
		/// 减少缓存的AssetBundle的引用计数
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
		/// <summary>
		/// 清空AssetCache数据
		/// </summary>
		public void ClearAssetCache()
		{
			List<string> caches = new List<string>();
			using (var enumer = mAssetCaches.GetEnumerator())
			{
				while (enumer.MoveNext())
				{
					if (enumer.Current.Value.Dispose())
					{
						caches.Add(enumer.Current.Key);
					}
				}
				foreach (string assetName in caches)
				{
					mAssetCaches.Remove(assetName);
				}
				if (mAssetCaches.Count == 0)
				{
					mAssetCaches.Clear();
				}
				Resources.UnloadUnusedAssets();
				GC.Collect();
			}
		}
		/// <summary>
		/// 清空AssetBundle缓存数据
		/// </summary>
		public void ClearAssetBundleCache()
		{
			List<string> caches = new List<string>();
			using (var enumer = mAssetBundleCaches.GetEnumerator())
			{
				while (enumer.MoveNext())
				{
					if (enumer.Current.Value.Dispose())
					{
						caches.Add(enumer.Current.Key);
					}
				}
				foreach (string assetName in caches)
				{
					mAssetBundleCaches.Remove(assetName);
				}
				if (mAssetBundleCaches.Count == 0)
				{
					mAssetBundleCaches.Clear();
				}
				Resources.UnloadUnusedAssets();
				GC.Collect();
			}
		}
		/// <summary>
		/// 强制清空缓存的数据
		/// </summary>
		public void ForceClear()
		{
			using (var enumer1 = mAssetCaches.GetEnumerator() )
			{
				using (var enumer2 = mAssetBundleCaches.GetEnumerator() )
				{
					while (enumer1.MoveNext())
					{
						enumer1.Current.Value.ForceDispose();
					}
					mAssetCaches.Clear();
					while (enumer2.MoveNext())
					{
						enumer2.Current.Value.ForceDispose();
					}
					mAssetBundleCaches.Clear();
					Resources.UnloadUnusedAssets();
					GC.Collect();
				}
			}
		}

		public void ForceClearAsset(string name)
		{
			using (var enumer1 = mAssetCaches.GetEnumerator() )
			{
				while (enumer1.MoveNext())
				{
					if (enumer1.Current.Key == name)
					{
						enumer1.Current.Value.ForceDispose();
						mAssetCaches.Remove(name);
						break;
					}
				}
			}
		}
		public void ForceClearAssetBundle(string name)
		{
			using (var enumer1 = mAssetBundleCaches.GetEnumerator() )
			{
				while (enumer1.MoveNext())
				{
					if (enumer1.Current.Key == name)
					{
						enumer1.Current.Value.ForceDispose();
						mAssetBundleCaches.Remove(name);
						break;
					}
				}
			}
		}
	}
}
