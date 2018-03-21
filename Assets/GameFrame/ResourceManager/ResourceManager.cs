using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;
/*
jar:file:///data/app/com.xxx.xxx-1.apk!/assets
           /data/app/com.xxx.xxx-1.apk


android  Application.dataPath + "!assets"     loadfromfile loadfromfileasyn 
ios streamingasset
 
android applicationstreamingasset               www
ios  file://application.streamingassetpaths
*/
namespace GameFrame
{
	class ResourceLoadTask
	{
		public uint ID;
		public List<uint> ParentTaskIDs;
		public int LoadType;
		public string Path;
		public Action<Object> Actions;
		public List<string> Dependencies;
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
		/// 缓存超过最大加载数量的队列
		/// </summary>
		private readonly Queue<ResourceLoadTask>m_delayLoadTasks = new Queue<ResourceLoadTask>();
		/// <summary>
		/// assetloadtask 对象池
		/// </summary>
		private readonly ObjectPool<AssetLoadTask>m_assetLoadTaskPool = new ObjectPool<AssetLoadTask>();
		/// <summary>
		/// 缓存超过最大加载数量的队列
		/// </summary>
		private readonly Queue<AssetLoadTask>m_delayAssetLoadTasks = new Queue<AssetLoadTask>();
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
			if (Interface.Instance.GetMemory() < 1024 * 1024)
			{
				MaxTaskCount = 5;
			}
			LoadDependencyConfig(m_dependencyPath);
			//LoadPreLoadList(m_preLoadListPath);
			
			//PreLoadResource();
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
			int count = 20;//最多加载数 20
			if (m_delayAssetLoadTasks.Count > 0)
			{
				var maxloadtime = 0.02f;
				var starttime = Time.realtimeSinceStartup;
				while (m_delayAssetLoadTasks.Count > 0)
				{
					var assetLoadTask = m_delayAssetLoadTasks.Dequeue();
					LoadAllAssets(assetLoadTask);
					m_assetLoadTaskPool.PutObject(assetLoadTask);
					count--;
					if (count == 0)
					{
						break;
					}
				}
			}
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
			if (Time.time > m_cleanupCacheLastTime + CleanCacheIntervalTime)
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
		
		private IEnumerator ClearAssetCacheAsyn()
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
			if (Time.time > m_cleanupMemoryLastTime + CleanMemoryIntervalTime)
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
			foreach (KeyValuePair<string,IAssetCache> keyValuePair in Singleton<AssetCacheManager>.GetInstance().Caches)
			{
				Debug.LogError("key "+keyValuePair.Key);
			}
			if (parentTaskId == 0) //不是加载依赖  看缓存object有没有 
			{
				IAssetCache o;
				if (Singleton<AssetCacheManager>.GetInstance().Caches.TryGetValue(file+"Object", out o))
				{
					action(o.Asset);
					return 0;
				}
			}
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
						if (Singleton<AssetCacheManager>.GetInstance().Caches.ContainsKey(task.Dependencies[i]+"AssetBundle") )
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
			}
			else
			{
				m_currentTaskCount -= 1;
				Debug.LogError("loadtype 出错");
			}
		}

		private void LoadBundleFromFile(ResourceLoadTask task)
		{
			string path = Application.streamingAssetsPath + "/"+task.Path;
			AssetBundle ab = AssetBundle.LoadFromFile(path);
			OnBundleLoaded(task,ab);
		}

		private IEnumerator LoadBundleFromWWW(ResourceLoadTask task)
		{
			string path = PathResolver.GetBundlePath(task.Path);
			var www = new WWW(path);
			yield return www;
			if (null != www.error)
			{
				Debug.LogError("LoadAssetbundle 失败");
			}
			OnBundleLoaded(task,www.assetBundle);
			www.Dispose();
		}

		private void OnBundleLoaded(ResourceLoadTask task, AssetBundle ab)
		{
			m_currentTaskCount -= 1;
			if (ab == null)
			{
				Debug.LogError("assetbundle 为空"+task.Path);
				OnAseetsLoaded(task,null,null);
			}
			else
			{
				var assetLoadTask = m_assetLoadTaskPool.GetObject();
				assetLoadTask.task = task;
				assetLoadTask.ab = ab;
				m_delayAssetLoadTasks.Enqueue(assetLoadTask);
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

		

		public bool IsLoading(uint taskId)
		{
			return m_loadingTasks.ContainsKey(taskId);
		}

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
