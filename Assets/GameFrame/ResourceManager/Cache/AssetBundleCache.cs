using System.Collections.Generic;
using UnityEngine;

namespace GameFrame
{
    /// <summary>
    /// AssetBundleCache
    /// </summary>
    public class AssetBundleCache
    {
        private float lastUseTime = 0;
        private float cacheDelayTime = 60;
        private AssetBundle assetBundle;
        private bool Persistent = false;
        private int Count = 0;
        public AssetBundleCache(AssetBundle ab,float usetime,bool isper)
        {
            assetBundle = ab;
            lastUseTime = usetime;
            Persistent = isper;
        }

        public void AddCount()
        {
            Count++;
        }

        public void SubCount()
        {
            Count--;
            if (Count <= 0)
            {
                lastUseTime = Time.realtimeSinceStartup;
            }
        }

        public int GetCount()
        {
            return Count;
        }
        
        public bool GetPersistent()
        {
            return Persistent;
        }

        public void SetPersistent(bool isper)
        {
            Persistent = isper;
        }
        public AssetBundle GetAssetBundle()
        {
            return assetBundle;
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
    }
    /// <summary>
    /// AssetBundleCacehManager
    /// </summary>
    public class AssetBundleCacheManager : Singleton<AssetBundleCacheManager>
    {
        /// <summary>
        /// 缓存字典
        /// </summary>
        private Dictionary<string,AssetBundleCache> m_assetBundleCacheDic = new Dictionary<string, AssetBundleCache>();
        private List<string> rmList = new List<string>();

        public AssetBundleCache GetAssetBundleCache(string path)
        {
            if (m_assetBundleCacheDic.ContainsKey(path))
            {
                return m_assetBundleCacheDic[path];
            }
            return null;
        }

        public void CacheAssetBundle(string path,AssetBundleCache cache)
        {
            m_assetBundleCacheDic[path] = cache;
        }
        
        public void CleanUpAssetBundleCache()
        {
            if(m_assetBundleCacheDic.Count<=0) return;
            rmList.Clear();
            var now = Time.realtimeSinceStartup;
            foreach (KeyValuePair<string,AssetBundleCache> keyValuePair in m_assetBundleCacheDic)
            {
                //跳过常驻内存的Asset资源
                AssetBundleCache temp = keyValuePair.Value;
                if (temp.GetPersistent())
                {
                    continue;
                }
                else if(temp.GetCount()>0)
                {
                    continue;
                }
                if (now > (temp.GetLastUseTime() + temp.GetCacheDelayTime()))
                {
                    rmList.Add(keyValuePair.Key);
                    AssetBundle ab = temp.GetAssetBundle();
                    if (null != ab)
                    {
                        ab.Unload(false);
                    }
                }
            }
            foreach (string s in rmList)
            {
                m_assetBundleCacheDic.Remove(s);
            }
        }

        public void Clear()
        {
            foreach (KeyValuePair<string,AssetBundleCache> keyValuePair in m_assetBundleCacheDic)
            {
                AssetBundleCache temp = keyValuePair.Value;
                AssetBundle ab = temp.GetAssetBundle();
                if (null != ab)
                {
                    ab.Unload(false);
                }
            }
            m_assetBundleCacheDic.Clear();
        }
    }
}