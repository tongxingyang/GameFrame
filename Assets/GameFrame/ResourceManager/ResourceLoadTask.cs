using System;
using System.Collections.Generic;

namespace GameFrame
{
    public class ResourceLoadTask
    {
        public uint Id;
        public List<uint> ParentTaskIds;
        public int LoadType;//加载方式
        public int CacheType;//缓存方式
        public string Path;
        public Action<UnityEngine.Object> Actions;
        public List<string> Dependencies;
        public int LoadedDependenciesCount = 0;
        public void Reset()
        {
            Id = 0;
            LoadType = 0;
            CacheType = 0;
            ParentTaskIds = null;
            Path = string.Empty;
            Actions = null;
            Dependencies = null;
            LoadedDependenciesCount = 0;
        }
    }
}