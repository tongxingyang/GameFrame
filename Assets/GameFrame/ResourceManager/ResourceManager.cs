using System.IO;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using GameFrame;
using Object = UnityEngine.Object;
using UnityEditor;

public class ManagedResource : MonoBehaviour
{
    public string bundlename;
    public void OnDestroy()
    {
        Singleton<ResourceManager>.GetInstance().DelRefCount(bundlename);
    }
}

    public delegate IEnumerator ResourceLoader();

    enum ResourceLoadType
    {
        Default = 0,
        Persistent = 1 << 0,        // 永驻内存的资源
        Cache = 1 << 1,             // Asset需要缓存

        UnLoad = 1 << 4,            // 利用www加载并且处理后是否立即unload

        Immediate = 1 << 5,         // 需要立即加载
        // 加载方式
        LoadBundleFromFile = 1 << 6, // 利用AssetBundle.LoadFromFile加载
        LoadBundleFromFileAsync = 1 << 7, // 利用AssetBundle.LoadFromFile异步加载
        LoadBundleFromWWW = 1 << 8, // 利用WWW 异步加载 AssetBundle
        ReturnAssetBundle = 1 << 9, // 返回scene AssetBundle
    }

    class ResourceLoadTask
    {
        public uint Id;
        public List<uint> ParentTaskIds;
        public int LoadType;
        public string Path;
        public Action<Object> Actions;
        public List<string> Dependencies;
        public int LoadedDependenciesCount = 0;
        public void Reset()
        {
            Id = 0;
            ParentTaskIds = null;
            Path = string.Empty;
            Actions = null;
            Dependencies = null;
            LoadedDependenciesCount = 0;
        }
    }

    class AssetLoadTask
    {
        public ResourceLoadTask task;
        public AssetBundle ab;
    }

    class CachedResource
    {
        public Object Obj;
        public float LastUseTime;
    }

    public class ResourceManager : Singleton<ResourceManager>
{
        private readonly Dictionary<string, Object> _persistantBundles = new Dictionary<string, Object>();
        private readonly Dictionary<string, CachedResource> _generalCachedBundles = new Dictionary<string, CachedResource>();  //一般Cache策略

        private readonly Dictionary<string, ResourceLoadTask> _loadingFiles = new Dictionary<string, ResourceLoadTask>();
        private readonly Dictionary<uint, ResourceLoadTask> _loadingTasks = new Dictionary<uint, ResourceLoadTask>();
        private readonly ObjectPool<ResourceLoadTask> _resourceLoadTaskPool = new ObjectPool<ResourceLoadTask>(50);
        private readonly Queue<ResourceLoadTask> _delayLoadTasks = new Queue<ResourceLoadTask>();
        private readonly ObjectPool<AssetLoadTask> _assetLoadTaskPool = new ObjectPool<AssetLoadTask>(m_DefaultMaxTaskCount);
        private readonly Queue<AssetLoadTask> _delayAssetLoadTasks = new Queue<AssetLoadTask>();
        private bool _canStartCleanupMemory = true;
        private float _cleanupMemoryLastTime;
        private float _cleanupCachedBundleLastTime;
        private float _cleanupDependenciesLastTime;


        private const string m_dependencyPath = "config/dependency";
        private const string m_preLoadListPath = "config/preloadlist.txt";
        private static Dictionary<string, List<string>> m_Dependencies = new Dictionary<string, List<string>>();
        private List<string> m_preLoadList = new List<string>();

        private Dictionary<string, AssetBundle> m_DependenciesObj = new Dictionary<string, AssetBundle>();
        Dictionary<string, int> RefCount = new Dictionary<string, int>();
        Dictionary<string, float> RefDelTime = new Dictionary<string, float>();

        private int m_currentTaskCount = 0;
        private const int m_DefaultMaxTaskCount = 10;
        private int m_currentPreLoadCount = 0;

        //private bool showLog = true;


        public static readonly ResourceManager Instance = new ResourceManager();

        public void Init()
        {
            // if (Interface.Instance.GetMemInfo() <= 1024*1024)
            //{
            //    MaxTaskCount = 5;
            //}
            m_currentTaskCount = 0;
            MaxTaskCount = m_DefaultMaxTaskCount;
        LoadDependencyConfig(m_dependencyPath);
            LoadPreLoadList(m_preLoadListPath);
            PreLoadResource();
        }

        private void LoadDependencyConfig(string path)
        {
#if UNITY_EDITOR_OSX
				string filepath = Application.streamingAssetsPath+"/Data/"+path;
#else
            string filepath = PathResolver.GetPath(path);
#endif
            if (!FileManager.IsFileExist(filepath))
            {
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
                        if (!m_Dependencies.ContainsKey(rename))
                        {
                            m_Dependencies[rename] = new List<string>();
                        }
                        for (int j = 0; j < count; j++)
                        {
                            depname = br.ReadString();
                            m_Dependencies[rename].Add(depname);
                        }
                    }
                }
            }
        }

        public void Update()
        {
            CleanupCacheBundle();
            CleanupMemoryInterval();
            CleanupDependenciesInterval();
            DoDelayTasks();
        }

        private void DoDelayTasks()
        {
            if (_delayLoadTasks.Count > 0)
            {
                while (_delayLoadTasks.Count > 0 && m_currentTaskCount < MaxTaskCount)
                {
                    ResourceLoadTask task = _delayLoadTasks.Dequeue();
                    DoTask(task);
                }
            }
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
        public int MaxTaskCount { get; set; }

        private void PreLoadResource()
        {
            foreach (string path in m_preLoadList)
            {
                if (path.Contains("_etx_alpha") && Application.platform != UnityEngine.RuntimePlatform.Android)
                {
                    m_currentPreLoadCount++;
                }
                else
                {
                    AddTask(path, PreLoadEventHandler, (int)(ResourceLoadType.LoadBundleFromFile | ResourceLoadType.Persistent));
                }

            }
        }
        private void PreLoadEventHandler(UnityEngine.Object obj)
        {
            m_currentPreLoadCount++;
        }

        public bool IsPreLoadDone { get { return m_currentPreLoadCount >= m_preLoadList.Count; } }

        private void CleanupCacheBundle()
        {
            if (_generalCachedBundles.Count <= 0) return;
            if (!(Time.realtimeSinceStartup > _cleanupCachedBundleLastTime + 10)) return;

            var now = _cleanupCachedBundleLastTime = Time.realtimeSinceStartup;

            const float cleanupTimeInterval = 180;
            var tempList = new List<string>();
            foreach (var pair in _generalCachedBundles)
            {
                if (now > pair.Value.LastUseTime + cleanupTimeInterval)
                {
                    tempList.Add(pair.Key);
                    if (null != pair.Value.Obj)
                    {
                        //Debug.LogError("try to destroy object : " + pair.Key);
                        if (pair.Value.Obj is GameObject)
                        {
                            UnityEngine.Object.DestroyImmediate(pair.Value.Obj, true);
                        }
                        else
                        {
                            Resources.UnloadAsset(pair.Value.Obj);
                        }
                    }
                }
            }
            foreach (var bundle in tempList)
            {
                _generalCachedBundles.Remove(bundle);
            }
        }

        private void LoadPreLoadList(string path)
        {
            m_preLoadList.Clear();
            string dataPath = PathResolver.GetPath(path);
            if (!FileManager.IsFileExist(dataPath))
            {
                Debug.LogError("预加载的资源不存在  " + dataPath);
                return;
            }
            foreach (string readAllLine in File.ReadAllLines(dataPath))
            {
                m_preLoadList.Add(readAllLine);
            }
        }
      
        public void CleanupMemoryInterval()
        {
            const float interval = 120;
            if (Time.realtimeSinceStartup > _cleanupMemoryLastTime + interval)
            {
                CleanupMemoryNow();
            }
        }

        public void CleanupMemoryNow()
        {
            if (_canStartCleanupMemory)
            {
                _canStartCleanupMemory = false;
                _cleanupMemoryLastTime = Time.realtimeSinceStartup;
                SingletonMono<GameFrameWork>.GetInstance().StartCoroutine(CleanupMemoryAsync());
            }
        }

        private static uint _nextTaskId;

        public uint AddTask(string file, Action<UnityEngine.Object> action)
        {
            return AddTask(file, action, (int)ResourceLoadType.LoadBundleFromFile);
        }

        public uint AddTask(string file, Action<Object> action, int loadType)
        {
            return AddTask(file, action, loadType, 0);
        }

        private uint AddTask(string file, Action<Object> action, int loadType, uint parentTaskId)
        {
            //Util.LogColor("red", "AddTask:" + file);
            if (String.IsNullOrEmpty(file))
            {
                return 0;
            }
            string fileReplace = file.Replace(@"\", @"/");
            string lowerFile = fileReplace.ToLower();
            Object o;
            if (_persistantBundles.TryGetValue(lowerFile, out o))
            {
                action(o);
                return 0;
            }

            CachedResource cachedTask;
            if (_generalCachedBundles.TryGetValue(lowerFile, out cachedTask))
            {
                cachedTask.LastUseTime = Time.realtimeSinceStartup;

                action(cachedTask.Obj);
                return 0;
            }

            ResourceLoadTask oldTask;
            if (_loadingFiles.TryGetValue(lowerFile, out oldTask))
            {
                if (action != null)
                {
                    oldTask.Actions += action;
                }

                if ((loadType & (int)ResourceLoadType.Persistent) > 0)
                {
                    oldTask.LoadType |= (int)ResourceLoadType.Persistent;
                }

                if (parentTaskId != 0)
                {
                    if (oldTask.ParentTaskIds == null)
                    {
                        oldTask.ParentTaskIds = new List<uint>();
                        LogError("resource path {0} type is , dependency resource or not", oldTask.Path);
                    }
                    oldTask.ParentTaskIds.Add(parentTaskId);
                }

                return 0;
            }
            uint id = ++_nextTaskId;
            List<uint> ptList = null;
            if (parentTaskId != 0)
            {
                ptList = new List<uint>();
                ptList.Add(parentTaskId);
            }
            var task = _resourceLoadTaskPool.GetObject();
            //var task = new ResourceLoadTask
            {
                task.Id = id;
                task.ParentTaskIds = ptList;
                task.Path = lowerFile;
                task.LoadType = loadType;
                task.Actions = action;
                task.Dependencies = (m_Dependencies.ContainsKey(lowerFile) ? m_Dependencies[lowerFile] : null);
                task.LoadedDependenciesCount = 0;

            };

            _loadingFiles[lowerFile] = task;
            _loadingTasks[id] = task;
            if (m_Dependencies.ContainsKey(task.Path))
            {
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

        private void LogError(string format, params object[] args)
        {
            Debug.LogError(string.Format(format, args));
        }

        private bool IsType(ResourceLoadTask task, ResourceLoadType loadType)
        {
            return (task.LoadType & (int)loadType) != 0;
        }

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
                        if (m_DependenciesObj.ContainsKey(task.Dependencies[i]) || _persistantBundles.ContainsKey(task.Dependencies[i]))
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
                            AddTask(task.Dependencies[i], null, task.LoadType, task.Id);
                        }
                    }
                }
            }
        }


        private void DoImmediateTask(ResourceLoadTask task)
        {
            m_currentTaskCount += 1;
            if (IsType(task, ResourceLoadType.LoadBundleFromWWW))
            {
                SingletonMono<GameFrameWork>.GetInstance().StartCoroutine(LoadBundleFromWWW(task));
            }
            else if (IsType(task, ResourceLoadType.LoadBundleFromFile))
            {
                LoadBundleFromFile(task);
            }
            else if (IsType(task, ResourceLoadType.LoadBundleFromFileAsync))
            {
                SingletonMono<GameFrameWork>.GetInstance().StartCoroutine(LoadBundleFromFileAsync(task));
            }
            else
            {
                m_currentTaskCount -= 1;
                Debug.LogError("loadtype 出错");
            }
        }
        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <param name="task"></param>
        private void LoadBundleFromFile(ResourceLoadTask task)
        {
            string path = Application.streamingAssetsPath + "/" + task.Path;
            AssetBundle ab = AssetBundle.LoadFromFile(path);
            OnBundleLoaded(task, ab);
        }
        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        private IEnumerator LoadBundleFromFileAsync(ResourceLoadTask task)
        {
            string path = Application.streamingAssetsPath + "/" + task.Path;
            AssetBundleCreateRequest ab = AssetBundle.LoadFromFileAsync(path);
            yield return ab;
            OnBundleLoaded(task, ab.assetBundle);
        }
        /// <summary>
        /// www加载资源
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        private IEnumerator LoadBundleFromWWW(ResourceLoadTask task)
        {
            string path = PathResolver.GetBundlePath(task.Path);
            using (WWW www = new WWW(path))
            {
                yield return www;
                if (null != www.error)
                {
                    Debug.LogError("LoadAssetbundle 失败");
                }
                OnBundleLoaded(task, www.assetBundle);
            }
        }
        private void OnBundleLoaded(ResourceLoadTask task, AssetBundle ab)
        {
            m_currentTaskCount -= 1;
            Object obj = null;
            if (ab == null)
            {
                LogError("LoadBundle: {0} failed! assetBundle == NULL!", task.Path);
                OnAseetsLoaded(task, ab, obj);
            }
            else
            {
                var assetLoadTask = _assetLoadTaskPool.GetObject();
                assetLoadTask.task = task;
                assetLoadTask.ab = ab;
                _delayAssetLoadTasks.Enqueue(assetLoadTask);
            }
        }


        private void LoadAllAssets(AssetLoadTask _task)
        {
            var task = _task.task;
            var ab = _task.ab;

            Object obj = null;
            if (ab != null)
            {
                if (!ab.isStreamedSceneAssetBundle)
                {
                    var objs = ab.LoadAllAssets();
                    if (objs.Length > 0)
                        obj = objs[0];
                    if (obj == null)
                    {
                        LogError("LoadBundle: {0} ! No Assets in Bundle!", task.Path);
                    }
                }
            }
            OnAseetsLoaded(task, ab, obj);
        }


        private void OnAseetsLoaded(ResourceLoadTask task, AssetBundle ab, Object obj)
        {
            if (m_Dependencies.ContainsKey(task.Path))
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
                        if ((task.LoadType & (int)ResourceLoadType.ReturnAssetBundle) > 0)
                        {
                            action(ab);
                        }
                        else
                        {
                            action(obj);
                        }
                    }
                    catch (Exception e)
                    {
                        string error = string.Format("Load Bundle {0} DoAction Exception: {1}", task.Path, e);
                        Debug.LogError(error);
                    }
                }
            }
            if (ab != null && task.ParentTaskIds == null)
            {
                if ((task.LoadType & (int)ResourceLoadType.Persistent) > 0)
                {
                    _persistantBundles[task.Path] = obj;
                    if ((task.LoadType & (int)ResourceLoadType.UnLoad) > 0)
                    {
                        ab.Unload(false);
                    }
                }
                else
                {
                    if ((task.LoadType & (int)ResourceLoadType.Cache) > 0)
                    {
                        var cachedTask = new CachedResource
                        {
                            LastUseTime = Time.realtimeSinceStartup,
                            Obj = obj
                        };
                        _generalCachedBundles[task.Path] = cachedTask;
                    }
                    if ((task.LoadType & (int)ResourceLoadType.ReturnAssetBundle) == 0)
                    {
                        //Util.LogColor("cyan", "~~~ab.Unload(false): " + task.Path);
                        ab.Unload(false);
                    }

                }
            }

            if (task.ParentTaskIds != null)
            {
                m_DependenciesObj[task.Path] = ab;
                //Util.LogColor("yellow", "~~~Loading Dependencies Asset: " + task.Path);
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

        private IEnumerator CleanupMemoryAsync()
        {
            yield return Resources.UnloadUnusedAssets();
            GC.Collect();
            _canStartCleanupMemory = true;
            _cleanupMemoryLastTime = Time.realtimeSinceStartup;
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

        public uint AddTaskAvatar(string file, Action<UnityEngine.Object> action)
        {
            return AddTask(file, action, (int)ResourceLoadType.Cache | (int)ResourceLoadType.LoadBundleFromFile);
        }

        public void Release()
        {
            foreach (KeyValuePair<string, UnityEngine.Object> pair in _persistantBundles)
            {
                if (pair.Value != null)
                {
                    if (pair.Value is GameObject)
                    {
                        UnityEngine.Object.DestroyImmediate(pair.Value, true);
                    }
                    else
                    {
                        Resources.UnloadAsset(pair.Value);
                    }

                }
            }

            _persistantBundles.Clear();

            foreach (var pair in m_DependenciesObj)
            {
                if (pair.Value != null)
                {
                    pair.Value.Unload(true);
                }
            }
            m_DependenciesObj.Clear();

            foreach (var pair in _generalCachedBundles)
            {
                if (pair.Value != null && pair.Value.Obj != null)
                {
                    if (pair.Value.Obj is GameObject)
                    {
                        UnityEngine.Object.DestroyImmediate(pair.Value.Obj, true);
                    }
                    else
                    {
                        Resources.UnloadAsset(pair.Value.Obj);
                    }

                }
            }
            _generalCachedBundles.Clear();
        }


        public void AddRefCount(string bundlename)
        {
            if (m_Dependencies.ContainsKey(bundlename))
            {
                //Util.LogColor("yellow", "AddRefCount:" + bundlename);
                foreach (var depname in m_Dependencies[bundlename])
                {
                    if (!_persistantBundles.ContainsKey(depname))
                    {
                        if (!RefCount.ContainsKey(depname))
                        {
                            RefCount[depname] = 0;
                        }
                        RefCount[depname]++;
                    }
                }
            }
        }

        public void DelRefCount(string bundlename)
        {
            if (m_Dependencies.ContainsKey(bundlename))
            {
                //Util.LogColor("red", "DelRefCount:" + bundlename);
                foreach (var depname in m_Dependencies[bundlename])
                {
                    if (RefCount.ContainsKey(depname))
                    {
                        RefCount[depname]--;
                        if (RefCount[depname] <= 0)
                        {
                            RefDelTime[depname] = Time.realtimeSinceStartup;
                        }
                    }
                }
            }
        }

        public GameObject NewGameObject(string path)
        {
            var gameObj = new GameObject();
            if (path != null)
            {
                var mr = gameObj.AddComponent<ManagedResource>();
                mr.bundlename = path.Replace(@"\", @"/").ToLower();
                AddRefCount(mr.bundlename);
            }
            return gameObj;
        }

        public GameObject Instantiate(Object obj, string path)
        {
            var gameObj = Object.Instantiate(obj) as GameObject;
            if (gameObj != null && path != null)
            {
                var mr = gameObj.AddComponent<ManagedResource>();
                mr.bundlename = path.Replace(@"\", @"/").ToLower();
                AddRefCount(mr.bundlename);
            }
            return gameObj;
        }

        public GameObject Copy(Object obj)
        {
            var gameObj = Object.Instantiate(obj) as GameObject;
            if (gameObj != null)
            {
                var mr = gameObj.GetComponent<ManagedResource>();
                if (mr != null)
                {
                    AddRefCount(mr.bundlename);
                }
            }
            return gameObj;
        }

        public void CleanupDependenciesInterval()
        {
            const float interval = 30;
            if (Time.realtimeSinceStartup > _cleanupDependenciesLastTime + interval)
            {
                CleanupDependenciesNow();
            }
        }

     
        public void CleanupDependenciesNow()
        {
            if (RefCount == null || RefDelTime == null || m_DependenciesObj == null)
            {
                return;
            }
            _cleanupDependenciesLastTime = Time.realtimeSinceStartup;
            List<string> RefCountToRemove = new List<string>();
            foreach (var pairs in RefCount)
            {
                if (pairs.Value <= 0)
                {
                    if (m_DependenciesObj.ContainsKey(pairs.Key) &&
                        RefDelTime.ContainsKey(pairs.Key) &&
                        Time.realtimeSinceStartup - RefDelTime[pairs.Key] > 60)
                    {
                        //Util.LogColor("red", "CleanupDependenciesNow:" + pairs.Key + pairs.Value);
                        if (m_DependenciesObj.ContainsKey(pairs.Key))
                        {
                            if (m_DependenciesObj[pairs.Key] != null)
                                m_DependenciesObj[pairs.Key].Unload(false);
                            m_DependenciesObj[pairs.Key] = null;
                            m_DependenciesObj.Remove(pairs.Key);
                        }
                        RefDelTime.Remove(pairs.Key);
                        RefCountToRemove.Add(pairs.Key);
                        //Util.LogColor("red", "~~~ Destory Dependencies Asset : " + pairs.Key);
                    }

                }
            }
            foreach (var remove in RefCountToRemove)
            {
                RefCount.Remove(remove);
            }
        }

    #region Resources加载

    public IEnumerator LoadResourceAsync<T>(string name,Action<Object> callback) where T:Object
    {
        ResourceRequest request = Resources.LoadAsync<T>(name);
        yield return request;
        if (callback != null)
        {
            callback(request.asset);
        }
    }

    public void LoadResource<T>(string name, Action<Object> callback) where T : Object
    {
        T t =Resources.Load<T>(name);
        if (callback != null)
        {
            callback(t);
        }
    }   
    #endregion
}


#region  Old
/*
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace GameFrame
{
	class ResourceLoadTask
	{
		/// <summary>
		/// ID
		/// </summary>
		public uint ID;
		/// <summary>
		/// 谁依赖我
		/// </summary>
		public List<uint> ParentTaskIDs;
		/// <summary>
		/// 加载类型
		/// </summary>
		public int LoadType;
		/// <summary>
		/// 路径
		/// </summary>
		public string Path;
		/// <summary>
		/// 加载完成的回调
		/// </summary>
		public Action<Object> Actions;
		/// <summary>
		/// 我依赖的资源
		/// </summary>
		public List<string> Dependencies;
		/// <summary>
		/// 加载的依赖文件个数
		/// </summary>
		public int LoadedDependenciesCount = 0;

		public void Reset()
		{
			ID = 0;
			ParentTaskIDs = null;
			Path = string.Empty;
			Actions = null;
			Dependencies = null;
			LoadedDependenciesCount = 0;
		}
	}

	//class AssetLoadTask
	//{
	//	public ResourceLoadTask task;
	//	public AssetBundle ab;
	//}
	
	public class ResourceManager : Singleton<ResourceManager> {
		
		private const string m_dependencyPath = "config/dependency";
		private const string m_preLoadListPath = "config/preloadlist.txt";
		/// <summary>
		/// 正在加载的任务字典  Key为string
		/// </summary>
		private readonly Dictionary<string,ResourceLoadTask> m_loadingFiles = new Dictionary<string, ResourceLoadTask>();
		/// <summary>
		/// 正在加载的任务字典 Key为uint
		/// </summary>
		private readonly Dictionary<uint,ResourceLoadTask>m_loadingTasks = new Dictionary<uint, ResourceLoadTask>();
		/// <summary>
		/// resourceloadtask 对象池
		/// </summary>
		private readonly ObjectPool<ResourceLoadTask> m_resourceLoadTaskPool = new ObjectPool<ResourceLoadTask>();
		/// <summary>
		/// 缓存加载队列
		/// </summary>
		private readonly Queue<ResourceLoadTask>m_delayLoadTasks = new Queue<ResourceLoadTask>();
		///// <summary>
		///// assetloadtask 对象池
		///// </summary>
		//private readonly ObjectPool<AssetLoadTask>m_assetLoadTaskPool = new ObjectPool<AssetLoadTask>();
		///// <summary>
		///// 缓存超过最大加载数量的队列
		///// </summary>
		//private readonly Queue<AssetLoadTask>m_delayAssetLoadTasks = new Queue<AssetLoadTask>();
		/// <summary>
		/// 是否可以清理内存
		/// </summary>
		private bool m_canStartCleanupMemory = true;
		/// <summary>
		/// 上一次清理内存的时间
		/// </summary>
		private float m_cleanupMemoryLastTime;
		/// <summary>
		/// 是否可以清理缓存
		/// </summary>
		private bool m_canStartCleanupCache = true;
		/// <summary>
		/// 上一次清理缓存的时间
		/// </summary>
		private float m_cleanupCacheLastTime;
		/// <summary>
		/// 文件依赖字典
		/// </summary>
		private static Dictionary<string,List<string>> m_Dependencies = new Dictionary<string, List<string>>();
		/// <summary>
		/// 预加载文件列表
		/// </summary>
		private List<string> m_preLoadList = new List<string>();
		public bool IsPreLoadDone { get { return m_currentPreLoadCount >= m_preLoadList.Count; } }
		
		private static uint m_nextTaskID;
		private int m_currentTaskCount = 0;
		public static  int MaxTaskCount = 10;
		private int m_currentPreLoadCount = 0;

		/////////////////////////////////
		public const float CleanCacheIntervalTime = 180;
		public const int CleanMemoryIntervalTime = 120;
		public override void Init()
		{
			//if (Interface.Instance.GetMemory() < 1024 * 1024)
			//{
			//	MaxTaskCount = 5;
			//}
			LoadDependencyConfig(m_dependencyPath);
			LoadPreLoadList(m_preLoadListPath);
			PreLoadResource();
		}

		private void LoadDependencyConfig(string path)
		{
			#if UNITY_EDITOR_OSX
				string filepath = Application.streamingAssetsPath+"/Data/"+path;
			#else
				string filepath = PathResolver.GetPath(path);
			#endif
			if (!FileManager.IsFileExist(filepath))
			{
				return;
			}
			using (FileStream fs = new FileStream(filepath,FileMode.Open,FileAccess.Read))
			{
				using (BinaryReader br  = new BinaryReader(fs))
				{
					int size = br.ReadInt32();
					string rename;
					string depname;
					for (int i = 0; i < size; i++)
					{
						rename = br.ReadString();
						int count = br.ReadInt32();
						if (!m_Dependencies.ContainsKey(rename))
						{
							m_Dependencies[rename] = new List<string>();
						}
						for (int j = 0; j < count; j++)
						{
							depname = br.ReadString();
							m_Dependencies[rename].Add(depname);
						}
					}
				}
			}
		}

		public void Update()
		{
			CleanCacheInterval();
			CleanMemoryInterval();
			DoDelayTasks();
		}

		private void DoDelayTasks()
		{
			if (m_delayLoadTasks.Count > 0)
			{
				while (m_delayLoadTasks.Count > 0 && m_currentTaskCount < MaxTaskCount)
				{
					ResourceLoadTask task = m_delayLoadTasks.Dequeue();
					DoTask(task);
				}
			}
			//int count = 20;//最多加载数 20
			//if (m_delayAssetLoadTasks.Count > 0)
			//{
			//	var maxloadtime = 0.02f;
			//	var starttime = Time.realtimeSinceStartup;
			//	while (m_delayAssetLoadTasks.Count > 0)
			//	{
			//		var assetLoadTask = m_delayAssetLoadTasks.Dequeue();
			//		LoadAllAssets(assetLoadTask);
			//		m_assetLoadTaskPool.PutObject(assetLoadTask);
			//		count--;
			//		if (count == 0)
			//		{
			//			break;
			//		}
			//	}
			//}
		}

		private void PreLoadResource()
		{
			foreach (string s in m_preLoadList)
			{
				AddTask(s, PreLoadEventHandler, (int)(enResourceLoadType.LoadBundleFromFile | enResourceLoadType.Persistent));
			}
		}

		private void PreLoadEventHandler(UnityEngine.Object obj)
		{
			m_currentPreLoadCount++;
		}
		/// <summary>
		/// 定时清理缓存
		/// </summary>
		public void CleanCacheInterval()
		{
			if (Time.time > (m_cleanupCacheLastTime + CleanCacheIntervalTime) && m_canStartCleanupCache)
			{
				CleanAssetCacheNow();
			}
		}
		public void CleanAssetCacheNow()
		{
			if (m_canStartCleanupCache)
			{
				m_canStartCleanupCache = false;
				SingletonMono<GameFrameWork>.GetInstance().StartCoroutine(ClearAssetCacheAsyn());
			}
		}
		
		private IEnumerator ClearAssetCacheAsyn()//没有引用过期的asset缓存和assetbundle缓存
		{
			List<IAssetCache> caches= new List<IAssetCache>(Singleton<AssetCacheManager>.GetInstance().Caches.Values);
			bool disposed = false;
			for (int i = 0; i < caches.Count; i++)
			{
				if (caches[i].IsDisposeable)
				{
					disposed = true;
					caches[i].Dispose();
					Singleton<AssetCacheManager>.GetInstance().Caches.Remove(caches[i].Name);
				}
			}
			if (disposed)
			{
				yield return Resources.UnloadUnusedAssets();
			}
			m_canStartCleanupCache = true;
			m_cleanupCacheLastTime = Time.time;
		}

		private void LoadPreLoadList(string path)
		{
			m_preLoadList.Clear();
			string dataPath = PathResolver.GetPath(path);
			if (!FileManager.IsFileExist(dataPath))
			{
				Debug.LogError("预加载的资源不存在  "+dataPath);
				return;
			}
			foreach (string readAllLine in File.ReadAllLines(dataPath))
			{
				m_preLoadList.Add(readAllLine);
			}
		}
		/// <summary>
		/// 定时清理内存
		/// </summary>
		public void CleanMemoryInterval()
		{
			if (Time.time > (m_cleanupMemoryLastTime + CleanMemoryIntervalTime) && m_canStartCleanupMemory)
			{
				CleanMemoryNow();
			}
		}

		public void CleanMemoryNow()
		{
			if (m_canStartCleanupMemory)
			{
				m_canStartCleanupMemory = false;
				SingletonMono<GameFrameWork>.GetInstance().StartCoroutine(CleanMemoryAsync());
			}
		}
		private IEnumerator CleanMemoryAsync()
		{
			yield return Resources.UnloadUnusedAssets();
			GC.Collect();
			m_canStartCleanupMemory = true;
			m_cleanupMemoryLastTime = Time.time;
		}
		
		
		public uint AddTask(string file, Action<UnityEngine.Object> action)
		{
			return AddTask(file, action, (int) enResourceLoadType.LoadBundleFromFile);
		}

		public uint AddTask(string file, Action<Object> action, int loadType)
		{
			return AddTask(file, action, loadType, 0);
		}

		private uint AddTask(string file, Action<Object> action, int loadType, uint parentTaskId)
		{
			if (string.IsNullOrEmpty(file))
			{
				return 0;
			}
            //foreach (KeyValuePair<string,IAssetCache> keyValuePair in Singleton<AssetCacheManager>.GetInstance().Caches)
            //{
            //	Debug.LogError("key "+keyValuePair.Key);
            //}
            //if (parentTaskId == 0) //不是加载依赖  看缓存object有没有 
            //{
            IAssetCache o;
            if (Singleton<AssetCacheManager>.GetInstance().Caches.TryGetValue(file, out o))
            {
                action(o.Asset);
                AssetCacheManager.Instance.AddAssetBundleCacheRefCount(file);
                return 0;
            }
            //}
            ResourceLoadTask task;
			if (m_loadingFiles.TryGetValue(file, out task))
			{
				if (action != null)
				{
					task.Actions += action;
				}
				if ((loadType & (int) enResourceLoadType.Persistent) > 0)
				{
					task.LoadType |= (int) enResourceLoadType.Persistent;
				}
				if (parentTaskId != 0)
				{
					if (task.ParentTaskIDs == null)
					{
						task.ParentTaskIDs = new List<uint>();
					}
					task.ParentTaskIDs.Add(parentTaskId);
				}
				return 0;
			}
			uint id = ++m_nextTaskID;
			List<uint> ptList = null;
			if (parentTaskId != 0)
			{
				ptList = new List<uint>();
				ptList.Add(parentTaskId);
			}
			ResourceLoadTask resourceLoadTask = m_resourceLoadTaskPool.GetObject();
			resourceLoadTask.ID = id;
			resourceLoadTask.ParentTaskIDs = ptList;
			resourceLoadTask.Path = file;
			resourceLoadTask.LoadType = loadType;
			resourceLoadTask.Actions = action;
			resourceLoadTask.Dependencies = (m_Dependencies.ContainsKey(file) ? m_Dependencies[file] : null);
			resourceLoadTask.LoadedDependenciesCount = 0;
			m_loadingFiles[file] = resourceLoadTask;
			m_loadingTasks[id] = resourceLoadTask;
			if (m_currentTaskCount < MaxTaskCount)
			{
				DoTask(resourceLoadTask);
			}
			else
			{
				m_delayLoadTasks.Enqueue(resourceLoadTask);
			}
			return id;
		}
		/// <summary>
		/// 判断加载类型是不是loadtype
		/// </summary>
		/// <param name="task"></param>
		/// <param name="loadType"></param>
		/// <returns></returns>
		private bool IsType(ResourceLoadTask task, enResourceLoadType loadType)
		{
			return (task.LoadType & (int)loadType) != 0;
		}

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
					for (; i < task.Dependencies.Count; i++)
					{
						if (Singleton<AssetCacheManager>.GetInstance().Caches.ContainsKey(task.Dependencies[i]) )
						{
							task.LoadedDependenciesCount++;
							if (task.LoadedDependenciesCount >= task.Dependencies.Count)
							{
								DoImmediateTask(task);
								return;
							}
						}
						else
						{
							AddTask(task.Dependencies[i], null, task.LoadType, task.ID);
						}
					}
				}
			}
		}

		private void DoImmediateTask(ResourceLoadTask task)
		{
			m_currentTaskCount += 1;
			if (IsType(task, enResourceLoadType.LoadBundleFromWWW))
			{
				SingletonMono<GameFrameWork>.GetInstance().StartCoroutine(LoadBundleFromWWW(task));
			}else if (IsType(task, enResourceLoadType.LoadBundleFromFile))
			{
				LoadBundleFromFile(task);
			}else if (IsType(task, enResourceLoadType.LoadBundleFromFileAsync))
			{
				SingletonMono<GameFrameWork>.GetInstance().StartCoroutine(LoadBundleFromFileAsync(task));
			}
			else
			{
				m_currentTaskCount -= 1;
				Debug.LogError("loadtype 出错");
			}
		}
		/// <summary>
		/// 同步加载资源
		/// </summary>
		/// <param name="task"></param>
		private void LoadBundleFromFile(ResourceLoadTask task)
		{
			string path = Application.streamingAssetsPath + "/"+task.Path;
			AssetBundle ab = AssetBundle.LoadFromFile(path);
			OnBundleLoaded(task,ab);
		}
		/// <summary>
		/// 异步加载资源
		/// </summary>
		/// <param name="task"></param>
		/// <returns></returns>
		private IEnumerator LoadBundleFromFileAsync(ResourceLoadTask task)
		{
			string path = Application.streamingAssetsPath + "/"+task.Path;
			AssetBundleCreateRequest ab = AssetBundle.LoadFromFileAsync(path);
			yield return ab;
			OnBundleLoaded(task,ab.assetBundle);
		}
		/// <summary>
		/// www加载资源
		/// </summary>
		/// <param name="task"></param>
		/// <returns></returns>
		private IEnumerator LoadBundleFromWWW(ResourceLoadTask task)
		{	
			string path = PathResolver.GetBundlePath(task.Path);
			using (WWW www = new WWW(path))
			{
				yield return www;
				if (null != www.error)
				{
					Debug.LogError("LoadAssetbundle 失败");
				}
				OnBundleLoaded(task,www.assetBundle);
			}
		}

		private void OnBundleLoaded(ResourceLoadTask task, AssetBundle ab)
		{
			//m_currentTaskCount -= 1;
			//if (ab == null)
			//{
			//	Debug.LogError("assetbundle 为空"+task.Path);
			//	OnAseetsLoaded(task,null,null);
			//}
			//else
			//{
			//	var assetLoadTask = m_assetLoadTaskPool.GetObject();
			//	assetLoadTask.task = task;
			//	assetLoadTask.ab = ab;
			//	m_delayAssetLoadTasks.Enqueue(assetLoadTask);
			//}
		}

		private void LoadAllAssets(AssetLoadTask _task)
		{
			
			var task = _task.task;
			var ab = _task.ab;
			Object obj = null;
			if (ab != null)
			{
				if (!ab.isStreamedSceneAssetBundle)
				{
					var objs = ab.LoadAllAssets();
					if (objs.Length > 0)
					{
						obj = objs[0];
					}
					if (obj == null)
					{
						Debug.LogError("no assets in bundle");
					}
				}
			}
			OnAseetsLoaded(task,ab,obj);
		}

		private void OnAseetsLoaded(ResourceLoadTask task, AssetBundle ab, Object obj)
		{
			Debug.LogError(task.Path +"   加载完成");
			
			if (m_Dependencies.ContainsKey(task.Path))
			{
				List<string> list = m_Dependencies[task.Path];
				
				for (int i = 0; i < list.Count; i++)
				{
					Singleton<AssetCacheManager>.GetInstance().SubAssetBundleCacheRefCount(list[i]+"AssetBundle");
				}
			}
			
			m_loadingFiles.Remove(task.Path);
			m_loadingTasks.Remove(task.ID);
			if (task.Actions != null && task.ParentTaskIDs == null)
			{
				if ((task.LoadType & (int) enResourceLoadType.Persistent) > 0)
				{
					Singleton<AssetCacheManager>.GetInstance().CacheSourceAsset(task.Path+"Object",true,obj);
				}
				if ((task.LoadType & (int) enResourceLoadType.Cache) > 0)
				{
					Singleton<AssetCacheManager>.GetInstance().CacheSourceAsset(task.Path+"Object",false,obj);
				}
				Delegate[] delegates = task.Actions.GetInvocationList();
				foreach (Delegate dele in delegates)
				{
					Action<Object> action = (Action<Object>) dele;
					try
					{
						if ((task.LoadType & (int) enResourceLoadType.ReturnAssetBundle) > 0)
						{
							action(ab);
						}
						else
						{
							action(obj);
						}
					}
					catch (Exception e)
					{
						Debug.LogError("Load Bundle Error");
					}
				}
			}
			if (ab != null && task.ParentTaskIDs == null)
			{
				if ((task.LoadType & (int) enResourceLoadType.Persistent) > 0)
				{
					Singleton<AssetCacheManager>.GetInstance().CacheAssetBundle(task.Path+"AssetBundle",true,ab);
				}
				else if((task.LoadType & (int) enResourceLoadType.Cache) > 0)
				{
					Singleton<AssetCacheManager>.GetInstance().CacheAssetBundle(task.Path+"AssetBundle",false,ab);
				}
				else
				{
					
					ab.Unload(false);
				}
			}
			foreach (KeyValuePair<string,IAssetCache> keyValuePair in Singleton<AssetCacheManager>.GetInstance().Caches)
			{
				//if (keyValuePair.Value is AssetBundleCache)
				{
					Debug.Log("  ----------- name "+keyValuePair.Key);
				}
			}
			if (task.ParentTaskIDs != null)
			{
				if ((task.LoadType & (int) enResourceLoadType.Persistent) > 0)
				{
					Singleton<AssetCacheManager>.GetInstance().CacheAssetBundle(task.Path + "AssetBundle", true, ab);
				}
				else
				{
					Singleton<AssetCacheManager>.GetInstance().CacheAssetBundle(task.Path + "AssetBundle", false, ab);
				}
				
				for (int i = 0; i < task.ParentTaskIDs.Count; i++)
				{
					uint taskid = task.ParentTaskIDs[i];
					Singleton<AssetCacheManager>.GetInstance().AddAssetBundleCacheRefCount(task.Path+"AssetBundle");
					ResourceLoadTask pt = null;
					if (m_loadingTasks.TryGetValue(taskid, out pt))
					{
						pt.LoadedDependenciesCount += 1;
						if (pt.LoadedDependenciesCount >= pt.Dependencies.Count)
						{
							DoTask(pt);
						}
					}
				}
				Debug.LogError("加载依赖---------------");
				foreach (KeyValuePair<string,IAssetCache> keyValuePair in Singleton<AssetCacheManager>.GetInstance().Caches)
				{
					if (keyValuePair.Value is AssetBundleCache)
					{
						AssetBundleCache cache = keyValuePair.Value as AssetBundleCache;
						Debug.Log(" name "+keyValuePair.Key +"     value   "+cache.RefCount);
					}
				}
			}
			
			task.Reset();
			m_resourceLoadTaskPool.PutObject(task);
		}
		/// <summary>
		/// taskid的任务是否正在加载
		/// </summary>
		/// <param name="taskId"></param>
		/// <returns></returns>
		public bool IsLoading(uint taskId)
		{
			return m_loadingTasks.ContainsKey(taskId);
		}
		/// <summary>
		/// 移除taskid的回调
		/// </summary>
		/// <param name="taskId"></param>
		/// <param name="action"></param>
		public void RemoveTask(uint taskId, Action<UnityEngine.Object> action)
		{
			if (IsLoading(taskId))
			{
				ResourceLoadTask oldTask = null;
				if (m_loadingTasks.TryGetValue(taskId, out oldTask))
				{
					if (null != action)
					{
						oldTask.Actions -= action;
					}
				}
			}
		}
		/// <summary>
		/// 释放AssetCacheManager缓存
		/// </summary>
		public void Release()
		{
			foreach (KeyValuePair<string,IAssetCache> keyValuePair in Singleton<AssetCacheManager>.GetInstance().Caches)
			{
				keyValuePair.Value.ForcedDispose();
			}
			Singleton<AssetCacheManager>.GetInstance().Caches.Clear();
		}
	}
}
*/
#endregion