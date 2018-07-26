using System.Collections.Generic;
using UnityEngine;

namespace GameFrame
{
    /// <summary>
    /// AssetCache
    /// </summary>
    public class AssetCache
    {
        private float lastUseTime = 0;
        private float cacheDelayTime = 0;
        private CachePriority cachePriority = CachePriority.NoCache;
        private Object obj;

        public AssetCache(float lasttime,Object ob,CachePriority priority)
        {
            lastUseTime = lasttime;
            obj = ob;
            cachePriority = priority;
            switch (priority)
            {
                case CachePriority.NoCache:
                    cacheDelayTime = 0;
                     break;
                case CachePriority.ShortTime:
                    cacheDelayTime = 30;
                    break;
                case CachePriority.MiddleTime:
                    cacheDelayTime = 100;
                    break;
                case CachePriority.LongTime:
                    cacheDelayTime = 200;
                    break;
                case CachePriority.Persistent:
                    cacheDelayTime = 0;
                    break;
            }
        }

        public Object GetObject()
        {
            return obj;
        }
        public CachePriority GetCachePriority()
        {
            return cachePriority;
        }

        public float GetLastUseTime()
        {
            return lastUseTime;
        }

        public float GetCacheDelayTime()
        {
            return cacheDelayTime;
        }
        public void SetLastUseTime(float time)
        {
            lastUseTime = time;
        }
        public void SetCachePriority(CachePriority priority)
        {
            cachePriority = priority;
            switch (priority)
            {
                case CachePriority.NoCache:
                    cacheDelayTime = 0;
                    break;
                case CachePriority.ShortTime:
                    cacheDelayTime = 30;
                    break;
                case CachePriority.MiddleTime:
                    cacheDelayTime = 100;
                    break;
                case CachePriority.LongTime:
                    cacheDelayTime = 200;
                    break;
                case CachePriority.Persistent:
                    cacheDelayTime = 0;
                    break;
            }
        }
        
    }
    /// <summary>
    /// AssetCacheManager
    /// </summary>
    public class AssetCacheManager : Singleton<AssetCacheManager>
    {
        /// <summary>
        /// 缓存字典
        /// </summary>
        private Dictionary<string,AssetCache> m_assetCacheDic = new Dictionary<string, AssetCache>();
        private List<string> rmList = new List<string>();
        /// <summary>
        /// 获取AssetCache资源
        /// </summary>
        /// <returns></returns>
        public AssetCache GetAssetCache(string path)
        {
            if (m_assetCacheDic.ContainsKey(path))
            {
                return m_assetCacheDic[path];
            }
            return null;
        }

        public void CacheAsset(string path,AssetCache cache)
        {
            m_assetCacheDic[path] = cache;
        }
        public void CleanUpAssetCache()
        {
            if(m_assetCacheDic.Count<=0) return;
            rmList.Clear();
            var now = Time.realtimeSinceStartup;
            foreach (KeyValuePair<string,AssetCache> keyValuePair in m_assetCacheDic)
            {
                //跳过常驻内存的Asset资源
                AssetCache temp = keyValuePair.Value;
                if(temp.GetCachePriority() == CachePriority.Persistent) continue;
                if (now > (temp.GetLastUseTime() + temp.GetCacheDelayTime()))
                {
                    rmList.Add(keyValuePair.Key);
                    Object obj = temp.GetObject();
                    if (null != obj)
                    {
                        if (obj is GameObject)
                        {
                            Object.DestroyImmediate(obj, true);
                        }
                        else
                        {
                            Resources.UnloadAsset(obj);
                        }
                    }
                }
            }
            foreach (string s in rmList)
            {
                m_assetCacheDic.Remove(s);
            }
        }
        /// <summary>
        /// 强制清除
        /// </summary>
        public void Clear()
        {
            foreach (KeyValuePair<string,AssetCache> keyValuePair in m_assetCacheDic)
            {
                AssetCache temp = keyValuePair.Value;
                Object obj = temp.GetObject();
                if (null != obj)
                {
                    if (obj is GameObject)
                    {
                        Object.DestroyImmediate(obj, true);
                    }
                    else
                    {
                        Resources.UnloadAsset(obj);
                    }
                }
            }
            m_assetCacheDic.Clear();
        }
    }
}