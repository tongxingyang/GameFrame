using System.IO;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using GameFrame;
using Object = UnityEngine.Object;

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
	    LoadBundleFromWWW = 1 << 1,     // 利用WWW 异步加载 AssetBundle
	    LoadBundleFromFile= 1 << 2,     // 利用AssetBundle.LoadFromFile加载
	    LoadBundleFromFileAsync = 1<< 3,// 利用AssetBundle.LoadFromFile异步加载
	    LoadFromMemory = 1<< 4,         // 利用LoadFromMemory加载
	    LoadFromMemoryAsync = 1<< 5,    // 利用LoadFromMemoryAsync异步加载
	    LoadFromStream = 1<< 6,         // 利用LoadFromStream加载
	    LoadFromStreamAsync = 1<<7 ,    // 利用LoadFromStreamAsync异步加载
	    ReturnAssetBundle = 1 << 8,    // 返回scene AssetBundle
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


        private  string m_dependencyPath = Platform.DepFileName;
        private  string m_preLoadListPath = Platform.PreloadList;
	
        private static Dictionary<string, List<string>> m_Dependencies = new Dictionary<string, List<string>>();
        private List<string> m_preLoadList = new List<string>();

        private Dictionary<string, AssetBundle> m_DependenciesObj = new Dictionary<string, AssetBundle>();
        Dictionary<string, int> RefCount = new Dictionary<string, int>();
        Dictionary<string, float> RefDelTime = new Dictionary<string, float>();

        private int m_currentTaskCount = 0;
        private const int m_DefaultMaxTaskCount = 10;
        private int m_currentPreLoadCount = 0;

        public static readonly ResourceManager Instance = new ResourceManager();

        public void Init()
        {
            m_currentTaskCount = 0;
            MaxTaskCount = m_DefaultMaxTaskCount;
        	LoadDependencyConfig(m_dependencyPath);
            LoadPreLoadList(m_preLoadListPath);
            PreLoadResource();
        }

        private void LoadDependencyConfig(string path)
        {
#if UNITY_STANDALONE_OSX
				string filepath = Application.streamingAssetsPath+"/Data/"+path;
#else
            string filepath = "";
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
                else// todo
                {
//                    AddTask(path, PreLoadEventHandler, (int)(ResourceLoadType.LoadBundleFromFile | ResourceLoadType.Persistent));
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
//            string dataPath = PathResolver.GetPath(path); // todo todo 
            string dataPath = "";
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
                }// todo
//
//                if ((loadType & (int)ResourceLoadType.Persistent) > 0)
//                {
//                    oldTask.LoadType |= (int)ResourceLoadType.Persistent;
//                }

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
            string path = ""; // todo todo
//            string path = PathResolver.GetBundlePath(task.Path);
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
            if (ab != null && task.ParentTaskIds == null)//todo
            {
//                if ((task.LoadType & (int)ResourceLoadType.Persistent) > 0)
//                {
//                    _persistantBundles[task.Path] = obj;
//                    if ((task.LoadType & (int)ResourceLoadType.NoCache) > 0)
//                    {
//                        ab.Unload(false);
//                    }
//                }
//                else
//                {
//                    if ((task.LoadType & (int)ResourceLoadType.Cache) > 0)
//                    {
//                        var cachedTask = new CachedResource
//                        {
//                            LastUseTime = Time.realtimeSinceStartup,
//                            Obj = obj
//                        };
//                        _generalCachedBundles[task.Path] = cachedTask;
//                    }
//                    if ((task.LoadType & (int)ResourceLoadType.ReturnAssetBundle) == 0)
//                    {
//                        //Util.LogColor("cyan", "~~~ab.Unload(false): " + task.Path);
//                        ab.Unload(false);
//                    }
//
//                }
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

//        public uint AddTaskAvatar(string file, Action<UnityEngine.Object> action)
//        {// todo
//            return AddTask(file, action, (int)ResourceLoadType.Cache | (int)ResourceLoadType.LoadBundleFromFile);
//        }

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
    public T LoadResource<T>(string name) where T : Object
    {
        T t = Resources.Load<T>(name);
        return t;
    }
    #endregion
}
