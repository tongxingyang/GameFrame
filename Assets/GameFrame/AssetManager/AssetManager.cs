using System;
using System.Collections.Generic;

namespace GameFrame.AssetManager
{
    public class AssetManager:Singleton<AssetManager>
    {
        Dictionary<Type, Dictionary<string, ResLoaded>>       m_LoadedResources        = new Dictionary<Type, Dictionary<string, ResLoaded>>();
        public Dictionary<Type, Dictionary<string, ResLoaded>>   LoadedResources
        {
            get
            {
                return m_LoadedResources;
            }
        }
        public Dictionary<string, AssetLoaded>   LoadedAssetBundles = new Dictionary<string, AssetLoaded>();
    }
}