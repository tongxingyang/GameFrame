using System.IO;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using GameFrameDebuger;
using Object = UnityEngine.Object;

namespace GameFrame
{
    public class ResourceManager : Singleton<ResourceManager>
    {
        private static Dictionary<string, List<string>> m_Dependencies = new Dictionary<string, List<string>>();
        private List<string> m_preLoadList = new List<string>();
        private readonly Dictionary<string, ResourceLoadTask> _loadingFiles = new Dictionary<string, ResourceLoadTask>();
        private readonly Dictionary<uint, ResourceLoadTask> _loadingTasks = new Dictionary<uint, ResourceLoadTask>();
        private readonly ObjectPool<ResourceLoadTask> _resourceLoadTaskPool = new ObjectPool<ResourceLoadTask>(50);
        private readonly Queue<ResourceLoadTask> _delayLoadTasks = new Queue<ResourceLoadTask>();
        private readonly ObjectPool<AssetLoadTask> _assetLoadTaskPool = new ObjectPool<AssetLoadTask>(m_DefaultMaxTaskCount);
        private readonly Queue<AssetLoadTask> _delayAssetLoadTasks = new Queue<AssetLoadTask>();
        /// <summary>
        /// 清理内存变量
        /// </summary>
        private bool _canStartCleanupMemory = true;
        private float _cleanupMemoryLastTime;
        private const float CleanUpMemoryDelayTime = 120;
        /// <summary>
        /// 清理AssetBundle变量
        /// </summary>
        private float _cleanupDependenciesLastTime;
        private const float CleanupDependenciesDelayTime = 120;
        /// <summary>
        /// 清理Asset变量
        /// </summary>
        private float _cleanupAssetCachedLastTime;
        private const float CleanupAssetCachedDelatTime = 120;
        //资源依赖加载的文件 预加载文件
        private  string m_dependencyPath = Platform.DepFileName;
        private  string m_preLoadListPath = Platform.PreloadList;
	     //当前加载任务数量
        private int m_currentTaskCount = 0;
        //默认最大的加载任务数量
        private const int m_DefaultMaxTaskCount = 20;
        //当前预加载的文件个数
        private int m_currentPreLoadCount = 0;
        
        public int MaxTaskCount { get; set; }
        public bool IsPreLoadDone { get { return m_currentPreLoadCount >= m_preLoadList.Count; } }
        private static uint _nextTaskId;

        public override void Init()
        {
            m_currentTaskCount = 0;
            MaxTaskCount = m_DefaultMaxTaskCount;
        	LoadDependencyConfig(m_dependencyPath);
            LoadPreLoadList(m_preLoadListPath);
            PreLoadResource();
        }
        /// <summary>
        /// 以常驻内存方式预加载
        /// </summary>
        private void PreLoadResource()
        {
            foreach (string path in m_preLoadList)
            {
                 AddTask(path, PreLoadEventHandler, (int)(AssetBundleLoadType.LoadBundleFromFile),(int)(CachePriority.Persistent));
            }
        }
        /// <summary>
        /// 预加载资源的回调处理函数
        /// </summary>
        /// <param name="obj"></param>
        private void PreLoadEventHandler(UnityEngine.Object obj)
        {
            m_currentPreLoadCount++;
        }
        /// <summary>
        /// 加载依赖关系 保存到字典數組中
        /// </summary>
        /// <param name="path"></param>
        private void LoadDependencyConfig(string path)
        {
            m_Dependencies.Clear();
            string filepath = Platform.Path + path;
            if (!FileManager.IsFileExist(filepath))
            {
                Debuger.LogError("AssetBundle依赖Depinfo资源不存在  " + filepath);
                return;
            }
            using (FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    int size = br.ReadInt32();
                    string rename;
                    string depname;
                    for (int i = 0; i < size; i++)
                    {
                        rename = br.ReadString();
                        int count = br.ReadInt32();
                        if (count !=0 && !m_Dependencies.ContainsKey(rename))
                        {
                            m_Dependencies[rename] = new List<string>();
                            for (int j = 0; j < count; j++)
                            {
                                depname = br.ReadString();
                                m_Dependencies[rename].Add(depname);
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 加载预加载文件列表
        /// </summary>
        /// <param name="path"></param>
        private void LoadPreLoadList(string path)
        {
            m_preLoadList.Clear();
            string dataPath = Platform.Path + path;
            if (!FileManager.IsFileExist(dataPath))
            {
//                Debuger.LogError("预加载的资源不存在  " + dataPath);
                return;
            }
            foreach (string readAllLine in File.ReadAllLines(dataPath))
            {
                m_preLoadList.Add(readAllLine);
            }
        }
      
        public void Update()
        {
            CleanupCacheBundle();
            CleanupMemoryInterval();
            CleanupDependenciesInterval();
            DoDelayTasks();
        }
        /// <summary>
        /// 清理AssetCache
        /// </summary>
        private void CleanupCacheBundle()
        {
            if (!(Time.realtimeSinceStartup > _cleanupAssetCachedLastTime + CleanupAssetCachedDelatTime)) return;
            Singleton<AssetCacheManager>.GetInstance().CleanUpAssetCache();
        }
        /// <summary>
        /// 清理AssetBundleCache
        /// </summary>
        public void CleanupDependenciesInterval()
        {
            if (!(Time.realtimeSinceStartup > _cleanupDependenciesLastTime + CleanupDependenciesDelayTime)) return;
            Singleton<AssetBundleCacheManager>.GetInstance().CleanUpAssetBundleCache();
        }
        /// <summary>
        /// 清理内存
        /// </summary>
        public void CleanupMemoryInterval()
        {
            if(!(Time.realtimeSinceStartup > _cleanupMemoryLastTime + CleanUpMemoryDelayTime) || !_canStartCleanupMemory)return;
            _canStartCleanupMemory = false;
            SingletonMono<GameFrameWork>.GetInstance().StartCoroutine(CleanupMemoryAsync());
        }

        private IEnumerator CleanupMemoryAsync()
        {
            //清理内存
            yield return Resources.UnloadUnusedAssets();
            //清理mono
            GC.Collect();
            //清理lua
            SingletonMono<LuaManager>.GetInstance().LuaGC();
            _canStartCleanupMemory = true;
            _cleanupMemoryLastTime = Time.realtimeSinceStartup;
        }
        /// <summary>
        /// 执行延迟加载任务
        /// </summary>
        private void DoDelayTasks()
        {
            // 1. 加载AssetBundle
            if (_delayLoadTasks.Count > 0)
            {
                while (_delayLoadTasks.Count > 0 && m_currentTaskCount < MaxTaskCount)
                {
                    ResourceLoadTask task = _delayLoadTasks.Dequeue();
                    DoTask(task);
                }
            }
            // 2. 加载Asset
            if (_delayAssetLoadTasks.Count > 0)
            {
                var maxloadtime = 0.02f;
                var starttime = Time.realtimeSinceStartup;
                while (_delayAssetLoadTasks.Count > 0 && Time.realtimeSinceStartup - starttime < maxloadtime)
                {
                    var assetLoadTask = _delayAssetLoadTasks.Dequeue();
                    LoadAllAssets(assetLoadTask);
                    _assetLoadTaskPool.PutObject(assetLoadTask);
                }
            }
        }
        /// <summary>
        /// 添加任务
        /// </summary>
        /// <param name="file"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public uint AddTask(string file, Action<UnityEngine.Object> action)
        {
            return AddTask(file, action, (int)AssetBundleLoadType.LoadBundleFromFile,(int)CachePriority.NoCache);
        }
        /// <summary>
        /// 添加任务
        /// </summary>
        /// <param name="file"></param>
        /// <param name="action"></param>
        /// <param name="loadType"></param>
        /// <param name="cachepriority"></param>
        /// <returns></returns>
        public uint AddTask(string file, Action<Object> action, int loadType,int cachepriority)
        {
            return AddTask(file, action, loadType,cachepriority, 0);
        }
        /// <summary>
        /// 添加任务
        /// </summary>
        /// <param name="file">文件名</param>
        /// <param name="action">回调函数</param>
        /// <param name="loadType">加载类型</param>
        /// <param name="cachepriority">缓存类型</param>
        /// <param name="parentTaskId">父任务的ID</param>
        /// <returns></returns>
        private uint AddTask(string file, Action<Object> action, int loadType, int cachepriority, uint parentTaskId)
        {
            if (String.IsNullOrEmpty(file))
            {
                return 0;
            }
            file = Platform.Path + file;
            string fileReplace = file.Replace(@"\", @"/");
            string lowerFile = fileReplace.ToLower();
            //从AssetBundleCache中查找文件
            if (parentTaskId == 0)
            {
                AssetCache assetBundleCache = Singleton<AssetCacheManager>.GetInstance().GetAssetCache(lowerFile);
                if (assetBundleCache != null)
                {
                    action(assetBundleCache.GetObject());
                    return 0;
                }
            }
            //从当前正在加载的字典中查找
            ResourceLoadTask oldTask;
            if (_loadingFiles.TryGetValue(lowerFile, out oldTask))
            {
                if (action != null)
                {
                    oldTask.Actions += action;
                }
                //修改缓存策略
                if (oldTask.CacheType != cachepriority)
                {
                    if (oldTask.CacheType < cachepriority)
                    {
                        oldTask.CacheType = cachepriority;
                    }
                }
                //将当前的父任务id加载到父任务列表的节点
                if (parentTaskId != 0)
                {
                    oldTask.ParentTaskIds.Add(parentTaskId);
                }
                return 0;
            }
            //新建任务
            uint id = ++_nextTaskId;
            List<uint> ptList = null;
            if (parentTaskId != 0)
            {
                ptList = new List<uint>();
                ptList.Add(parentTaskId);
            }
            ResourceLoadTask task = _resourceLoadTaskPool.GetObject();
            task.Reset();
            task.Id = id;
            task.ParentTaskIds = ptList;
            task.Path = lowerFile;
            task.LoadType = loadType;
            task.CacheType = cachepriority;
            task.Actions = action;
            task.Dependencies = (m_Dependencies.ContainsKey(lowerFile) ? m_Dependencies[lowerFile] : null);
            task.LoadedDependenciesCount = 0;

            _loadingFiles[lowerFile] = task;
            _loadingTasks[id] = task;

            if (Singleton<AssetBundleCacheManager>.GetInstance().GetAssetBundleCache(task.Path) != null)
            {
                //添加引用
                AddRefCount(task.Path);
            }
            
            if (m_currentTaskCount < MaxTaskCount)
            {
                DoTask(task);
            }
            else
            {
                _delayLoadTasks.Enqueue(task);
            }
            return id;
        }
        /// <summary>
        /// DoTask函数
        /// </summary>
        /// <param name="task"></param>
        private void DoTask(ResourceLoadTask task)
        {
            if (task.Dependencies == null)
            {
                DoImmediateTask(task);
            }
            else
            {
                if (task.LoadedDependenciesCount >= task.Dependencies.Count)
                {
                    DoImmediateTask(task);
                }
                else
                {
                    int i = task.LoadedDependenciesCount;
                    for (; i < task.Dependencies.Count; ++i)
                    {
                        if (Singleton<AssetBundleCacheManager>.GetInstance().GetAssetBundleCache(task.Dependencies[i]).GetAssetBundle()!=null)
                        {
                            task.LoadedDependenciesCount += 1;
                            if (task.LoadedDependenciesCount >= task.Dependencies.Count)
                            {
                                DoImmediateTask(task);
                                return;
                            }
                        }
                        else
                        {
                            AddTask(task.Dependencies[i], null, task.LoadType, task.CacheType,task.Id);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 实际加载文件的函数
        /// </summary>
        /// <param name="task"></param>
        private void DoImmediateTask(ResourceLoadTask task)
        {
            m_currentTaskCount += 1;
            if (task.LoadType == (int)AssetBundleLoadType.LoadBundleFromFile)
            {
                LoadBundleFromFile(task);
            }
            else if (task.LoadType == (int)AssetBundleLoadType.LoadBundleFromFileAsync)
            {
                SingletonMono<GameFrameWork>.GetInstance().StartCoroutine(LoadBundleFromFileAsync(task));
            }
            else if (task.LoadType == (int)AssetBundleLoadType.LoadFromMemory)
            {
                LoadBundleFromMemory(task);
            }
            else if (task.LoadType == (int)AssetBundleLoadType.LoadFromMemoryAsync)
            {
                SingletonMono<GameFrameWork>.GetInstance().StartCoroutine(LoadBundleFromMemoryAsync(task));
            }
            else if (task.LoadType == (int)AssetBundleLoadType.LoadFromStream)
            {
                LoadBundleFromStream(task);
            }
            else if (task.LoadType == (int)AssetBundleLoadType.LoadFromStreamAsync)
            {
                SingletonMono<GameFrameWork>.GetInstance().StartCoroutine(LoadBundleFromStreamAsync(task));
            }
            else
            {
                m_currentTaskCount -= 1;
                Debuger.LogError("loadtype 出错");
            }
        }
        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <param name="task"></param>
        private void LoadBundleFromFile(ResourceLoadTask task)
        {
            AssetBundle ab = AssetBundle.LoadFromFile(task.Path);
            OnBundleLoaded(task, ab);
        }
        private void LoadBundleFromMemory(ResourceLoadTask task)
        {
            AssetBundle ab = AssetBundle.LoadFromMemory(File.ReadAllBytes(task.Path));
            OnBundleLoaded(task, ab);
        }
        private void LoadBundleFromStream(ResourceLoadTask task)
        {
            AssetBundle ab = AssetBundle.LoadFromStream(File.Open(task.Path,FileMode.Open));
            OnBundleLoaded(task, ab);
        }
        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        private IEnumerator LoadBundleFromFileAsync(ResourceLoadTask task)
        {
            AssetBundleCreateRequest ab = AssetBundle.LoadFromFileAsync(task.Path);
            yield return ab;
            OnBundleLoaded(task, ab.assetBundle);
        }
        private IEnumerator LoadBundleFromMemoryAsync(ResourceLoadTask task)
        {
            AssetBundleCreateRequest ab = AssetBundle.LoadFromMemoryAsync(File.ReadAllBytes(task.Path));
            yield return ab;
            OnBundleLoaded(task, ab.assetBundle);
        }
        private IEnumerator LoadBundleFromStreamAsync(ResourceLoadTask task)
        {
            AssetBundleCreateRequest ab = AssetBundle.LoadFromStreamAsync(File.Open(task.Path,FileMode.Open));
            yield return ab;
            OnBundleLoaded(task, ab.assetBundle);
        }
        private void OnBundleLoaded(ResourceLoadTask task, AssetBundle ab)
        {
            m_currentTaskCount -= 1;
            Object obj = null;
            if (ab == null)
            {
                Debuger.LogError(string.Format("LoadBundle: {0} failed! assetBundle == NULL!", task.Path));
            }
            else
            {
                var assetLoadTask = _assetLoadTaskPool.GetObject();
                assetLoadTask.task = task;
                assetLoadTask.ab = ab;
                _delayAssetLoadTasks.Enqueue(assetLoadTask);
            }
        }
        /// <summary>
        /// 加载Asset资源
        /// </summary>
        /// <param name="_task"></param>
        private void LoadAllAssets(AssetLoadTask _task)
        {
            var task = _task.task;
            var ab = _task.ab;

            Object obj = null;
            if (ab != null)
            {
                var objs = ab.LoadAllAssets();
                if (objs.Length > 0)
                    obj = objs[0];
                if (obj == null)
                {
                    Debuger.LogError(string.Format("LoadBundle: {0} ! No Assets in Bundle!", task.Path));
                }
            }
            OnAseetsLoaded(task, ab, obj);
        }
    
        private void OnAseetsLoaded(ResourceLoadTask task, AssetBundle ab, Object obj)
        {
            if (Singleton<AssetBundleCacheManager>.GetInstance().GetAssetBundleCache(task.Path)!=null)
            {
                DelRefCount(task.Path);
            }

            _loadingFiles.Remove(task.Path);
            _loadingTasks.Remove(task.Id);

            if (task.Actions != null && task.ParentTaskIds == null)
            {
                Delegate[] delegates = task.Actions.GetInvocationList();
                foreach (var dele in delegates)
                {
                    var action = (Action<Object>)dele;
                    try
                    {
                        action(obj);
                    }
                    catch (Exception e)
                    {
                        string error = string.Format("Load Bundle {0} DoAction Exception: {1}", task.Path, e);
                        Debuger.LogError(error);
                    }
                }
            }
            
            if (ab != null && task.ParentTaskIds == null)
            {
                Singleton<AssetCacheManager>.GetInstance().CacheAsset(task.Path,new AssetCache(Time.realtimeSinceStartup,obj,(CachePriority)task.CacheType));
            }

            if (task.ParentTaskIds != null)
            {
                Singleton<AssetBundleCacheManager>.GetInstance().CacheAssetBundle(task.Path,new AssetBundleCache(ab,Time.realtimeSinceStartup,task.CacheType == (int)CachePriority.Persistent?true:false));
                for (int i = 0; i < task.ParentTaskIds.Count; ++i)
                {
                    uint taskid = task.ParentTaskIds[i];
                    ResourceLoadTask pt = null;
                    if (_loadingTasks.TryGetValue(taskid, out pt))
                    {
                        pt.LoadedDependenciesCount += 1;
                        if (pt.LoadedDependenciesCount >= pt.Dependencies.Count)
                        {
                            DoTask(pt);
                        }
                    }
                }
            }

            task.Reset();
            _resourceLoadTaskPool.PutObject(task);
        }

           
        public bool IsLoading(uint taskId)
        {
            return _loadingTasks.ContainsKey(taskId);
        }

        public void RemoveTask(uint taskId, Action<UnityEngine.Object> action)
        {
            if (IsLoading(taskId))
            {
                ResourceLoadTask oldTask = null;
                if (_loadingTasks.TryGetValue(taskId, out oldTask))
                {
                    if (null != action)
                    {
                        oldTask.Actions -= action;
                    }
                }
            }
        }

        public void Release()
        {
            Singleton<AssetCacheManager>.GetInstance().Clear();
            Singleton<AssetBundleCacheManager>.GetInstance().Clear();
        }
        /// <summary>
        /// 添加引用计数
        /// </summary>
        /// <param name="bundlename"></param>
        public void AddRefCount(string bundlename)
        {
            if (m_Dependencies.ContainsKey(bundlename))
            {
                foreach (var depname in m_Dependencies[bundlename])
                {
                    Singleton<AssetBundleCacheManager>.GetInstance().GetAssetBundleCache(depname).AddCount();
                }
            }
        }
        /// <summary>
        /// 删除引用计数
        /// </summary>
        /// <param name="bundlename"></param>
        public void DelRefCount(string bundlename)
        {
            if (m_Dependencies.ContainsKey(bundlename))
            {
                foreach (var depname in m_Dependencies[bundlename])
                {
                    Singleton<AssetBundleCacheManager>.GetInstance().GetAssetBundleCache(depname).SubCount();
                }
            }
        }
   
            
        #region Resources加载接口
        
        public IEnumerator IELoadResourceAsync<T>(string name,Action<Object> callback) where T:Object
        {
            ResourceRequest request = Resources.LoadAsync<T>(name);
            yield return request;
            if (callback != null)
            {
                callback(request.asset);
            }
        }
        public void LoadResourceAsync<T>(string name, Action<Object> callback) where T : Object
        {
            SingletonMono<GameFrameWork>.GetInstance().StartCoroutine(IELoadResourceAsync<T>(name, callback));
        }
        public void LoadResource<T>(string name, Action<Object> callback) where T : Object
        {
            T t =Resources.Load<T>(name);
            if (callback != null)
            {
                callback(t);
            }
        }
        public T LoadResource<T>(string name) where T : Object
        {
            T t = Resources.Load<T>(name);
            return t;
        }
        
        
        #endregion
    }
}
