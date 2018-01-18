using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading;
using GameFrame;
using Junfine.Debuger;
using LuaInterface;
using UIFrameWork;
using UnityEngine;
using UnityEngine.UI;
namespace GameFrame
{
    //定义委托
    public delegate void UpdateAction();
    
    public enum enClientState
    {
        State_Init,
        State_InitSDK,
        State_UnZipData,
        State_UpdateApp,
        State_UpdateResource,
        State_GetServerList,
        State_Start,
    }

    public enum CompareResult
    {
        Greater, Less, Equal, Error
    }
    public class FileInfo
    {
        public string fullname;
        public string md5;
        public int size;
    }

    public class UpdateManager : Singleton<UpdateManager>
    {
        private static int DOWNLOAD_COUNT = 50;
        /// <summary>
        /// 获取组件
        /// </summary>
        private Text AlertContextText;
        private Text StatusText;
        private Text ProgressText;
        private Text LocalResVersionText;
        private Text OnlineResVersionText;
        private Text AppVersion;

        private Button SureButton;
        private Button CancelButton;

        private Slider ProgressSliber;

        private GameObject UpdateGameObject;
        private GameObject AlertObject;

        private GameObject CanvasObj;


        private Action<bool> UpdateCallback;
        private bool m_isBeginUpdate = false;
        private bool m_isCheckSDK = false;
        private float m_UpdateLastTime;
        
        private enClientState State = enClientState.State_Init;
        public  Dictionary<string,FileInfo> oldmd5Table = new Dictionary<string, FileInfo>();
        public  Dictionary<string,FileInfo> newmd5Table = new Dictionary<string, FileInfo>();
        
        public readonly HashSet<string> ResourcesHasUpdate = new HashSet<string>();
        public List<string> m_redownloadList = new List<string>();
        public List<string> m_downloadList = new List<string>();
        public  readonly ConcurrentQueue<UpdateAction> m_actions = new ConcurrentQueue<UpdateAction>();
        public  ConcurrentQueue<DownloadTask> m_taskQueue = new ConcurrentQueue<DownloadTask>();
        
        public  readonly object m_obj = new object();//线程安全锁 对象

        public float DownLoadProgress
        {
            get
            {
                if (TotalDownloadSize == 0)
                {
                    return 1f;
                }
                else
                {
                    return (float) DownloadSize / TotalDownloadSize;
                }
            }
        }
        private string initalVersion;
        private string currentVersion;
        private string onlineVersion;
        private int initalResVersion;
        private int currentResVersion;
        private int onlineResVersion;
        private bool isLoadOldTableAndVersion = false;
        private bool isCopyDone = false;
        private bool isUpdateAppDone = false;
        private int filecount = 0;
        private int currentcopycount = 0;

        public  int TotalDownloadSize = 0;
        public int DownloadSize = 0;
        private bool isDoneloadDone = false;

        public bool IsDoneloadDone
        {
            get { return isDoneloadDone; }
        }
        public int UpdateState
        {
            get { return m_updateState; }
        }

        private int m_updateState = 0;
        private bool m_loadOver = true;
        private int m_urlIndex = 2;
        private int m_ovverThreadNum = 0;
        private string SrcUrl = string.Empty;
        public override void Init()
        {
            base.Init();
            CanvasObj = GameObject.Find("Canvas");
            //加载本地文件列表
            SingletonMono<GameFrameWork>.GetInstance().StartCoroutine(LoadOldTableAndVersion());
        }

        private IEnumerator LoadOldTableAndVersion()
        {
            isLoadOldTableAndVersion = false;
            string filepath = Platform.InitalPath + Platform.AppVerFileName;
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IPHONE
            filepath = "file:///" + filepath;
#endif
            using (WWW w = new WWW(filepath))
            {
                yield return w;
                if (w.error != null)
                {
                    yield break;
                }
                initalVersion = w.text;
            }
             filepath = Platform.InitalPath + Platform.ResVersionFileName;
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IPHONE
            filepath = "file:///" + filepath;
#endif
            using (WWW w = new WWW(filepath))
            {
                yield return w;
                if (w.error != null)
                {
                    Debug.LogError(w.error);
                    yield break;
                }
                initalResVersion = int.Parse(w.text);
            }
            filepath = Platform.InitalPath + Platform.Md5FileName;
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IPHONE
            filepath = "file:///" + filepath;
#endif
            
            using (WWW w = new WWW(filepath))
            {
                yield return w;
                if (w.error != null)
                {
                    yield break;
                }
                string str = w.text;
                string[] strarr = str.Split('\n');
                for (int i = 0; i < strarr.Length; i++)
                {
                    var pair = strarr[i].Split(',');
                    if (pair.Length == 3)
                    {
                        FileInfo fileInfo = new FileInfo();
                        fileInfo.fullname = pair[0];
                        fileInfo.md5 = pair[1];
                        fileInfo.size = int.Parse(pair[2]);
                        oldmd5Table[pair[0]] = fileInfo;
                    }else if (pair.Length == 2)
                    {
                        FileInfo fileInfo = new FileInfo();
                        fileInfo.fullname = pair[0];
                        fileInfo.md5 = pair[1];
                        fileInfo.size = 0;
                        oldmd5Table[pair[0]] = fileInfo;
                    }  else if (pair.Length == 1)
                    {
                        FileInfo fileInfo = new FileInfo();        
                        fileInfo.fullname = pair[0];               
                        fileInfo.md5 = string.Empty;               
                        fileInfo.size = 0;                         
                        oldmd5Table[fileInfo.fullname] = fileInfo; 
                    }
                }
            }
            filecount = oldmd5Table.Count;
            isLoadOldTableAndVersion = true;
        }
        public void SetUpdateCallback(Action<bool> action)
        {
            UpdateCallback = action;
        }

        public void StartCheckUpdate(Action<bool> action)
        {
            UpdateCallback = action;
            //加载update prefab
            GameObject obj = Resources.Load<GameObject>("UpdatePrefab");
            UpdateGameObject = GameObject.Instantiate(obj);
            if (UpdateGameObject != null)
            {
                UpdateGameObject.transform.SetParent(CanvasObj.transform);
                UpdateGameObject.transform.localPosition = Vector3.zero;
                UpdateGameObject.transform.localScale = Vector3.one;
                UpdateGameObject.GetComponent<RectTransform>().offsetMax = Vector2.zero;
                UpdateGameObject.GetComponent<RectTransform>().offsetMin = Vector2.zero;
            }
            //给组件赋值
            AlertContextText = UpdateGameObject.transform.Find("Alert/Content/Image/Text").GetComponent<Text>();
            StatusText = UpdateGameObject.transform.Find("Text/Status").GetComponent<Text>();
            ProgressText = UpdateGameObject.transform.Find("Text/Progress").GetComponent<Text>();
            LocalResVersionText = UpdateGameObject.transform.Find("TopText/LocalVersion").GetComponent<Text>();
            OnlineResVersionText = UpdateGameObject.transform.Find("TopText/OnlineVersion").GetComponent<Text>();
            AppVersion = UpdateGameObject.transform.Find("TopText/APPVersion").GetComponent<Text>();

            AlertObject = UpdateGameObject.transform.Find("Alert").gameObject;

            SureButton = UpdateGameObject.transform.Find("Alert/Sure").GetComponent<Button>();
            CancelButton = UpdateGameObject.transform.Find("Alert/Cancel").GetComponent<Button>();

            ProgressSliber = UpdateGameObject.transform.Find("Slider").GetComponent<Slider>();
            m_UpdateLastTime = Time.time;
            m_isBeginUpdate = true;
            
            //测试模式
            ClearDataOath();
        }

        public void Update()
        {
            if (m_isBeginUpdate && isLoadOldTableAndVersion)
            {
                switch (State)
                { 
                    case  enClientState.State_Init:
                        if (!m_isCheckSDK)
                        {
                            m_isCheckSDK = true;
                            
                            SingletonMono<GameFrameWork>.GetInstance().StartCoroutine(CheckSDK());
                        }
                        break;
                    case enClientState.State_InitSDK:
                        StatusText.text = Singleton<LauncherString>.GetInstance().GetString("Resource_InitSDK");
                        ProgressSliber.value = 0.0f;
                        ProgressText.text = string.Format("{0}%", 0);
                        if (Singleton<Interface>.GetInstance().IsCaheckSDKFinish())
                        {
                            this.State = enClientState.State_UnZipData;
                            CheckVersion();
                        }
                        break;
                    case enClientState.State_UnZipData:
                        if (isCopyDone)
                        {
                             //获取网络信息
                            if (Application.internetReachability == NetworkReachability.NotReachable)
                            {
                                //没有网络
                                StatusText.text = Singleton<LauncherString>.GetInstance().GetString("Resource_OffLine");
                            }
                            else
                            {
                                State = enClientState.State_UpdateApp;
                                Singleton<ServerConfig>.GetInstance().Load();
                                SingletonMono<GameFrameWork>.GetInstance().StartCoroutine(UpdateApp());
                            }
                            ProgressSliber.value = 1;
                            ProgressText.text = string.Format("{0}%",100);
                            
                        }
                        else
                        {
                           RefLauncherInfo();
                        }
                        break;
                    case  enClientState.State_UpdateApp:
                        if (isUpdateAppDone)
                        {
                            State = enClientState.State_UpdateResource;
                            //开始检查本地与服务器资源资源
                            SingletonMono<GameFrameWork>.GetInstance().StartCoroutine(UpdateResource());
                        }
                        RefLauncherInfo();
                        break;
                    case  enClientState.State_UpdateResource:
                        if (isDoneloadDone)
                        {
                            State = enClientState.State_GetServerList;
                            SingletonMono<GameFrameWork>.GetInstance()
                                .StartCoroutine(Singleton<ServerListManager>.GetInstance().GetServerList());
                            //开始获取可用的服务器列表
                        }
                        else
                        {
                            update();//调用文件下载成功回调
                            RefLauncherInfo();
                        }
                        break;
                    case  enClientState.State_GetServerList:
                        if (Singleton<ServerListManager>.GetInstance().DownloadDone)
                        {
                            this.State = enClientState.State_Start;
                            RefLauncherInfo();
                            
                        }
                        else
                        {
                            RefLauncherInfo();
                        }
                        
                        break;
                    case  enClientState.State_Start:
                        //开始游戏
                        UpdateCallback(true);
                        break;
                }
            }
        }

        public string GetCurrentVersion()
        {
            if (!string.IsNullOrEmpty(currentVersion))
            {
                return currentVersion;
            }
            string filename = Platform.Path + Platform.AppVerFileName;
            if(File.Exists(filename))
            {
                try
                {
                    string version = File.ReadAllText(filename);
                    if (!string.IsNullOrEmpty(version))
                    {
                        currentVersion = version;
                        return currentVersion;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                    return null;
                }
            }
            return null;
        }

        public string GetOnlineVersion()
        {
            return String.Empty;
        }
        private void CheckVersion()
        {
            string filepath = Platform.Path + "hasupdate.txt";//沙河目录
            bool needUnzip = false;
            if (FileManager.IsFileExist(filepath))
            {
                GetCurrentVersion();
                if (currentVersion != null && initalVersion != null)
                {
                    var result = CompareVersion(initalVersion, currentVersion);
                    if (result == CompareResult.Greater)
                    {
                        ClearDataOath();
                        needUnzip = true;
                        
                    }
                    
                }else if (currentVersion == null && initalVersion != null)
                {
                    needUnzip = true;
                    ClearDataOath();
                }
            }
            else
            {
                needUnzip = true;
            }
            if (needUnzip)
            {
                SingletonMono<GameFrameWork>.GetInstance().StartCoroutine(Preprocess());
            }
            else
            {
                isCopyDone = true;
            }
        }

        public void ClearDataOath()
        {
            foreach (var directory in Directory.GetDirectories(Platform.Path))
            {
                Directory.Delete(directory,true);
            }
            foreach (var file in Directory.GetFiles(Platform.Path))
            {
                FileManager.DeleteFile(file);
            }
        }
        public CompareResult CompareVersion(string ver1, string ver2)
        {
            
            int[] vervalue1 = new int[3], vervalue2 = new int[3];
            string[] strver1, strver2;
            strver1 = ver1.Split('.');
            strver2 = ver2.Split('.');
            try
            {
                for (int i = 0; i < 3; i++)
                {
                    
                    vervalue1[i] = int.Parse(strver1[i]);
                    vervalue2[i] = int.Parse(strver2[i]);
                }
            }
            catch
            {
                return CompareResult.Error;
            }
            for (int i = 0; i < Math.Min(vervalue1.Length, vervalue2.Length); i++)
            {
                if (vervalue1[i] > vervalue2[i])
                    return CompareResult.Greater;
                if (vervalue1[i] < vervalue2[i])
                    return CompareResult.Less;
            }
            if (vervalue1.Length > vervalue2.Length)
                return CompareResult.Greater;
            if (vervalue1.Length < vervalue2.Length)
                return CompareResult.Less;
            return CompareResult.Equal;
        }

        private void RefProgress(float value)
        {
            if (UpdateGameObject)
            {
                ProgressText.text = string.Format("{0}%", Math.Round(value,2)*100);
                ProgressSliber.value = value;
            }
        }

        private void RefVersion(string oldv,string newv)
        {
            if (UpdateGameObject)
            {
                LocalResVersionText.text = oldv.ToString();
                OnlineResVersionText.text = newv.ToString();
            }
        }

        private void RefAppVersion(string newappversion)
        {
            AppVersion.text = newappversion;
        }

        private void RefLauncherInfo()
        {
            if (State == enClientState.State_UnZipData)
            {
                StatusText.text = Singleton<LauncherString>.GetInstance().GetString("Resource_Unzip");
                RefProgress((float)currentcopycount/filecount);

            }else if (State == enClientState.State_GetServerList)
            {
                
                StatusText.text = Singleton<LauncherString>.GetInstance().GetString("Resource_GetServerList");
                ProgressSliber.value = 0;
                ProgressText.text = string.Format("{0}%", 0);

            }else if (State == enClientState.State_UpdateApp)
            {
                if (UpdateState == 1)
                {
                    StatusText.text = Singleton<LauncherString>.GetInstance().GetString("Resource_CheckVersion");
                    ProgressSliber.value = 0;
                    ProgressText.text = string.Format("{0}%", 0);
                }
                else if (UpdateState == 2)
                {
                    StatusText.text = Singleton<LauncherString>.GetInstance().GetString("Resource_DownloadNewApp");
                    ProgressSliber.value = 0;
                    ProgressText.text = string.Format("{0}%", 0);
                }
                
            }else if (State == enClientState.State_UpdateResource)
            {
                if (UpdateState == 0)
                {
                    StatusText.text = "";
                    ProgressSliber.value = 0;
                    ProgressText.text = string.Format("{0}%", 0);
                }else if (UpdateState == 1)
                {
                    StatusText.text = Singleton<LauncherString>.GetInstance().GetString("Resource_CheckResource");
                    ProgressSliber.value = 0;
                    ProgressText.text = string.Format("{0}%", 0);
                }else if (UpdateState == 2)
                {
                    StatusText.text = Singleton<LauncherString>.GetInstance().GetString("Resource_UpdateResource");
                    ProgressSliber.value = DownLoadProgress;
                    ProgressText.text = string.Format("{0}%",Math.Round(DownLoadProgress,2)*100);

                }else if (UpdateState == 3)
                {
                    StatusText.text = Singleton<LauncherString>.GetInstance().GetString("Resource_StartGame");
                    ProgressSliber.value = 1;
                    ProgressText.text = string.Format("{0}%", 100);
                    
                }else if (UpdateState == 4)
                {
                    StatusText.text = Singleton<LauncherString>.GetInstance().GetString("Resource_OffLine");
                    ProgressSliber.value = 1;
                    ProgressText.text = string.Format("{0}%", 0);
                }
                
            }else if (State == enClientState.State_Start)
            {
                StatusText.text = Singleton<LauncherString>.GetInstance().GetString("Resource_StartGame");
                ProgressSliber.value = 1;
                ProgressText.text = string.Format("{0}%", 100);
            }
        }
        /// <summary>
        /// 读取沙河目录的md5文件
        /// </summary>
        private void LoadCurrentMd5Table()
        {
            string filename = Platform.Path + Platform.Md5FileName;
            oldmd5Table.Clear();
            using (StreamReader sr = File.OpenText(filename))
            {
                string line;
                while ((line = sr.ReadLine())!=null)
                {
                    var pair = line.Split(',');
                    if (pair.Length == 3)
                    {
                        FileInfo fileInfo = new FileInfo();
                        fileInfo.fullname = pair[0];
                        fileInfo.md5 = pair[1];
                        fileInfo.size = int.Parse(pair[2]);
                        oldmd5Table[fileInfo.fullname] = fileInfo;
                    }else if (pair.Length == 2)
                    {
                        FileInfo fileInfo = new FileInfo();
                        fileInfo.fullname = pair[0];
                        fileInfo.md5 = pair[1];
                        fileInfo.size = 0;
                        oldmd5Table[fileInfo.fullname] = fileInfo;
                    }else if (pair.Length == 1)
                    {
                        continue;//长度等于1 直接继续  
                    }
                    
                }
            }
        }

        private void LoadHasUpdateSet()
        {
            string filename = Platform.Path + Platform.HasUpdateFileName;
            using (StreamReader sr = File.OpenText(filename))
            {
                string line;
                while ((line=sr.ReadLine())!=null)
                {
                    ResourcesHasUpdate.Add(line);
                }
            }
        }

        private void LoadCurrentResVersion()
        {
            string filename = Platform.Path + Platform.ResVersionFileName;
            if (FileManager.IsFileExist(filename))
            {
                using (StreamReader sr = File.OpenText(filename))
                {
                    currentResVersion = int.Parse(sr.ReadLine());
                }
            }
            else
            {
                Debug.LogError("读取沙河目录的resVersion出错");
            }
        }
        /// <summary>
        /// 验证本地资源是否已经丢失了
        /// </summary>
        /// <returns></returns>
        private bool CheckOldMd5File()
        {
            List<string> lostFiles = new List<string>();
            foreach (KeyValuePair<string,FileInfo> keyValuePair in oldmd5Table)
            {
                FileInfo fileInfo = keyValuePair.Value;
                string filename = Platform.Path + fileInfo.fullname;
                //从沙河目录读取的md5 文件 遍历文件夹 寻找丢失的文件 加入到lostList中
                //或者resourcehasupdate 里面包含 并且在沙河目录中存在这个文件跳过 否则加入lostList里
                if (fileInfo.fullname.StartsWith("config") || fileInfo.fullname.StartsWith("scripts"))
                {
                    if (FileManager.IsFileExist(filename))
                    {
                        continue;
                    }
                    lostFiles.Add(fileInfo.fullname);
                }
                else
                {
                    if (ResourcesHasUpdate.Contains(fileInfo.fullname))
                    {
                        if (FileManager.IsFileExist(fileInfo.fullname))
                        {
                            continue;
                        }
                        lostFiles.Add(fileInfo.fullname);
                    }
                }
            }
            foreach (var file in lostFiles)
            {
                oldmd5Table.Remove(file);
            }
            if (lostFiles.Count>0)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 删除没用的文件
        /// </summary>
        private void DeleteUselessFiles()
        {
            if (newmd5Table.Count == 0)
            {
                return;
            }
            List<string> deleteFiles = new List<string>();
            foreach (KeyValuePair<string,FileInfo> keyValuePair in oldmd5Table)
            {
                if (newmd5Table.ContainsKey(keyValuePair.Key))
                {
                    continue;
                }
                if (ResourcesHasUpdate.Contains(keyValuePair.Key))
                {
                    ResourcesHasUpdate.Remove(keyValuePair.Key);
                    deleteFiles.Add(keyValuePair.Key);
                }
            }
            foreach (string deleteFile in deleteFiles)
            {
                string filename = Platform.Path + deleteFile;
                if (FileManager.IsFileExist(filename))
                {
                    FileManager.DeleteFile(filename);
                }
            }
        }
        /// <summary>
        /// 获得下载的文件列表
        /// </summary>
        private void GetDownloadFileList()
        {
            m_downloadList.Clear();
            TotalDownloadSize = 0;
            foreach (var keyValuePair in newmd5Table)
            {
                FileInfo fileInfo = null;
                oldmd5Table.TryGetValue(keyValuePair.Key, out fileInfo);
                if (fileInfo != null)
                {
                    if (fileInfo.md5.Equals(newmd5Table[keyValuePair.Key].md5))
                    {
                        continue;
                    }
                }
                m_downloadList.Add(keyValuePair.Key);
                TotalDownloadSize += newmd5Table[keyValuePair.Key].size;
            }
            if (m_downloadList.Count == 0)
            {
                isDoneloadDone = true;
            }
            Debug.LogError("需要下载的数量 "+m_downloadList.Count +",    大小  "+TotalDownloadSize);
        }

        private void ThreadProc()
        {
            while (true)
            {
                DownloadTask task = null;
                if (m_taskQueue.TryDequeue(out task))
                {
                    task.BeginDownload();
                }
                else
                {
                    break;
                }
            }
            lock (m_obj)
            {
                m_ovverThreadNum++;
            }
        }
        /// <summary>
        /// 保存md5文件
        /// </summary>
        /// <param name="tabDictionary"></param>
        private void SaveMD5Table(Dictionary<string,FileInfo> tabDictionary)
        {
            string filename = Platform.Path + Platform.Md5FileName;
            if (FileManager.IsFileExist(filename))
            {
                FileManager.DeleteFile(filename);
            }
            string directory = Path.GetDirectoryName(filename);
            if (!string.IsNullOrEmpty(directory) && !FileManager.IsDirectoryExist(directory))
            {
                FileManager.CreateDirectory(directory);
            }
            try
            {
                using (var write = new StreamWriter(new FileStream(filename,FileMode.Create)))
                {
                    foreach (KeyValuePair<string,FileInfo> keyValuePair in tabDictionary)
                    {
                        write.WriteLine(keyValuePair.Key+","+keyValuePair.Value.md5+","+keyValuePair.Value.size);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
        /// <summary>
        /// 保存hasupdate文件
        /// </summary>
        /// <param name="tab"></param>
        private void SaveResourceHasUpdateSet(HashSet<string> tab)
        {
            string filename = Platform.Path + Platform.HasUpdateFileName;
            if (FileManager.IsFileExist(filename))
            {
                FileManager.DeleteFile(filename);
            }
            string directory = Path.GetDirectoryName(filename);
            if (!string.IsNullOrEmpty(directory) && !FileManager.IsDirectoryExist(directory))
            {
                FileManager.DeleteDirectory(directory);
            }
            try
            {
                using (var write = new StreamWriter(new FileStream(filename,FileMode.Create)))
                {
                    foreach (string s in tab)
                    {
                        write.WriteLine(s);
                    }
                }
            }
            catch (Exception e)
            {
               Debug.LogError(e.Message);
            }
        }
        /// <summary>
        /// 保存resversion文件
        /// </summary>
        private void SaveResourceVersion()
        {
            var filename = Platform.Path + Platform.ResVersionFileName;
            if (FileManager.IsFileExist(filename))
            {
                FileManager.DeleteFile(filename);
            }
            try
            {
                using (var write = new StreamWriter(new FileStream(filename,FileMode.Create)))
                {
                    write.Write(onlineResVersion);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
        /// <summary>
        /// 向本地md5文件追加信息
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
        /// <param name="size"></param>
        public void AppendMD5File(string key,string val,int size)
        {
            string filename = Platform.Path + Platform.Md5FileName;
            try
            {
                using (var write = new StreamWriter(new FileStream(filename,FileMode.Append)))
                {
                    write.WriteLine(key+","+val+","+size);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
        /// <summary>
        /// 向本地hasupdate文件追加信息
        /// </summary>
        /// <param name="key"></param>
        public void AppendHasUpdateFile(string key)
        {
            string filename = Platform.Path + Platform.HasUpdateFileName;
            try
            {
                using (var write = new StreamWriter(new FileStream(filename,FileMode.Append)))
                {
                    write.WriteLine(key);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        public void update()
        {
            while (true)
            {
                UpdateAction action;
                if (m_actions.TryDequeue(out action))
                {
                    action();
                }
                else
                {
                    break;
                }
            }
        }
        
        #region 协成
        /// <summary>
        /// 更新资源
        /// </summary>
        /// <returns></returns>
        IEnumerator UpdateResource()
        {
            m_updateState = 1;
            isDoneloadDone = false; 
            //读取沙河md5
            LoadCurrentMd5Table();
            //读取hasUpdate文本
            LoadHasUpdateSet();
            //从沙河目录读取的md5 文件 遍历文件夹 寻找丢失的文件 加入到lostList中
            //或者resourcehasupdate 里面包含 并且在沙河目录中存在这个文件跳过 否则加入lostList里
            bool bCheck = CheckOldMd5File();
            m_urlIndex = Singleton<ServerConfig>.GetInstance().UpdateServer.Length - 1;
            m_loadOver = false;
            while (!m_loadOver && m_urlIndex>0)
            {
                yield return SingletonMono<GameFrameWork>.GetInstance().StartCoroutine(DownloadResourceFile());
                m_urlIndex--;
            }
            if (!m_loadOver)
            {
                m_updateState = 4;
                yield break;
            }
            bool bVersion = true;
            //加载沙河目录的resVersion
            LoadCurrentResVersion();
            
            if (currentResVersion < onlineResVersion)
            {
                bVersion = false;
            }
            Debug.LogError("是否需要更新资源= "+bVersion+", 沙河目录的res版本 = "+currentResVersion+", 服务器上的res版本 = "+onlineResVersion);
            RefVersion(currentResVersion.ToString(),onlineResVersion.ToString());
            if (!bCheck && bVersion)//沙河目录文件没有问题 并且 版本相同
            {
                m_updateState = 3;
                isDoneloadDone = true;
                yield break;
            }
            m_urlIndex = Singleton<ServerConfig>.GetInstance().UpdateServer.Length-1;
            m_loadOver = false;//重置 为了下次下载做准备
            while (!m_loadOver && m_urlIndex>0)
            {
                yield return SingletonMono<GameFrameWork>.GetInstance().StartCoroutine(DownloadMD5File());
                m_urlIndex++;
            }
            if (!m_loadOver)
            {
                m_updateState = 4;
                yield break;
            }
            DeleteUselessFiles();
            GetDownloadFileList(); //如果要下载的列表为0 isdownloaddone = true
            if (!isDoneloadDone)
            {
                NetworkReachability networkState = Application.internetReachability;
                if (networkState == NetworkReachability.ReachableViaCarrierDataNetwork)
                {
                    if (TotalDownloadSize > 1048576) //2m
                    {
                        AlertContextText.text =
                            string.Format(Singleton<LauncherString>.GetInstance().GetString("DownloadSizeAlert"),
                                TotalDownloadSize / 1048576f);
                        AlertObject.SetActive(true);
                        EventTriggerListener.Get(CancelButton.gameObject).SetEventHandle(EnumTouchEventType.OnClick,
                            (a, b, c) =>
                            {
                                Debug.LogError("cancel");
                                Application.Quit();
                            });
                        EventTriggerListener.Get(SureButton.gameObject).SetEventHandle(EnumTouchEventType.OnClick,
                            (a, b, c) =>
                            {
                                Debug.LogError("sure");
                                AlertObject.gameObject.SetActive(false);
                                Debug.LogError("开始下载文件.......");
                                SingletonMono<GameFrameWork>.GetInstance().StartCoroutine(BeginDownloadResource());
                            });
                    }
                    else
                    {
                          Debug.LogError("开始下载文件.......");
                          SingletonMono<GameFrameWork>.GetInstance().StartCoroutine(BeginDownloadResource());
                    }
                }
                else
                {
                      Debug.LogError("开始下载文件.......");
                      SingletonMono<GameFrameWork>.GetInstance().StartCoroutine(BeginDownloadResource());
                }
            }
            else
            {
                SaveResourceVersion();
                m_updateState = 3;
                currentResVersion = onlineResVersion;
                RefVersion(currentResVersion.ToString(),onlineResVersion.ToString());
            }
            
        }

        IEnumerator BeginDownloadResource()
        {
            //设置最大下载数量
            System.Net.ServicePointManager.DefaultConnectionLimit = DOWNLOAD_COUNT;
            m_updateState = 2;
            m_urlIndex = 0;
            Debug.LogError("开始下载时间 "  +Time.realtimeSinceStartup);
            while (m_urlIndex < Singleton<ServerConfig>.GetInstance().UpdateServer.Length)
            {
                //正式版本
                //SrcUrl = Singleton<ServerConfig>.GetInstance().UpdateServer[m_urlIndex];
                //本地测试版本
                SrcUrl = "http://192.168.6.24:8000/Documents/qyz/trunk/dist/Data/";
                //
                m_taskQueue.Clear();
                m_redownloadList.Clear();
                foreach (string s in m_downloadList)
                {
                    //正式版本
                    
//                    string url = SrcUrl + s;
//                    string suffix = "?version=" + newmd5Table[s].md5;
//                    url += suffix;
//                    string filepath = Platform.Path + s;
                    //本地测试版本
                    string url = SrcUrl + s;
                    string filepath = Platform.Path + s;
//                    Debug.LogError("下载文件路径 ： "+s);
//                    Debug.LogError("下载文件保存路径 ： "+filepath);
//                    Debug.LogError("下载文件路径URL ： "+url);
                    
                    DownloadTask task = new DownloadTask(url,s,filepath);
                    m_taskQueue.Enqueue(task);
                }
//                yield break;//  todo 测试
                if (m_taskQueue.Count > 0)
                {
                    m_ovverThreadNum = 0;
                    for (int i = 0; i < 5; i++)
                    {
                        Thread thread = new Thread(ThreadProc);
                        thread.Name = "download thread: " + i;
                        thread.Start();
                    }
                    while (m_ovverThreadNum < 5 || m_actions.Count > 0)
                    {
                        yield return null; //还没下载完成
                    }
                }
                else
                {
                    if (newmd5Table.Count > 0)
                    {
                        SaveMD5Table(newmd5Table);
                    }
                    SaveResourceHasUpdateSet(ResourcesHasUpdate);
                    isDoneloadDone = true;
                    Debug.LogError("下载完成........");
                    SaveResourceVersion();
                    currentResVersion = onlineResVersion;
                    RefVersion(currentVersion.ToString(),onlineResVersion.ToString());
                    yield break;
                }
                if (m_redownloadList.Count > 0)
                {
                    m_downloadList.Clear();
                    foreach (string s in m_redownloadList)
                    {
                        m_downloadList.Add(s);
                    }
                }
                else
                {
                    Debug.LogError("下载结束时间  "+Time.realtimeSinceStartup);
                    if (newmd5Table.Count > 0)
                    {
                        SaveMD5Table(newmd5Table);
                    }
                    SaveResourceHasUpdateSet(ResourcesHasUpdate);
                    isDoneloadDone = true;
                    Debug.LogError("下载完成..........");
                    SaveResourceVersion();
                    currentResVersion = onlineResVersion;
                    RefVersion(currentResVersion.ToString(),onlineResVersion.ToString());
                    yield break;
                }
                m_urlIndex++;
            }
            m_updateState = 4;
            foreach (string file in m_redownloadList)
            {
                Debug.LogError("下载文件出错 "+file);
            }
        }
        /// <summary>
        /// 下载服务器md5
        /// </summary>
        /// <returns></returns>
        private IEnumerator DownloadMD5File()
        {
            newmd5Table.Clear();
//            SrcUrl = Singleton<ServerConfig>.GetInstance().UpdateServer[m_urlIndex];
//            string url = SrcUrl + Platform.Md5FileName;
//            string suffix = "?version=" + onlineResVersion;
//            url += suffix;
            //测试版本
            SrcUrl = "http://192.168.6.24:8000/Documents/qyz/trunk/dist/" + Platform.Md5FileName;
            using (var www = new WWW(SrcUrl))
            {
                yield return www;
                if (www.error != null)
                {
                    yield break;
                }
                string m_line = www.text.Trim();
                using (StringReader br = new StringReader(m_line))
                {
                    string line;
                    while ((line = br.ReadLine())!=null)
                    {
                        var pair = line.Split(',');
                        if (pair.Length == 3)
                        {
                            FileInfo fileInfo = new FileInfo();
                            fileInfo.fullname = pair[0];
                            fileInfo.md5 = pair[1];
                            fileInfo.size = int.Parse(pair[2]);
                            newmd5Table[fileInfo.fullname] = fileInfo;
                        }else if (pair.Length == 2)
                        {
                            FileInfo fileInfo = new FileInfo();
                            fileInfo.fullname = pair[0];
                            fileInfo.md5 = pair[1];
                            fileInfo.size = 0;
                            newmd5Table[fileInfo.fullname] = fileInfo;
                        }else if (pair.Length == 1)
                        {
                            continue;//长度等于1 直接继续  
                        }
                    }
                    m_loadOver = true;
                    Debug.LogError("下载md5 文件成功  数量为  "+newmd5Table.Count);
                }
            }
        }
        /// <summary>
        /// 加载服务器的resVersion
        /// </summary>
        /// <returns></returns>
        private IEnumerator DownloadResourceFile()
        {
            //正常版本
//            SrcUrl = Singleton<ServerConfig>.GetInstance().UpdateServer[m_urlIndex];
//            string url = SrcUrl + Platform.ResVersionFileName;
//            string suffix = "?version=" + DateTime.Now.Ticks.ToString();
//            url += suffix;
            // 本地测试版本
            SrcUrl = "http://192.168.6.24:8000/Documents/qyz/trunk/dist/" + Platform.ResVersionFileName;
            using (var www = new WWW(SrcUrl))
            {
                yield return www;
                if (www.error != null)
                {
                    yield break;
                }
                string line = www.text.Trim();
                int.TryParse(line, out onlineResVersion);
                m_loadOver = true;
            }
        }
        IEnumerator Preprocess()
        {
            //创建目录
            if (!FileManager.IsDirectoryExist(Platform.Path))
            {
                FileManager.ClearDirectory(Platform.Path);
            }
            //拷贝文件 从streaming 目录拷贝到 沙河目录
            if (oldmd5Table == null || oldmd5Table.Count == 0) yield break;
            var itr = oldmd5Table.GetEnumerator();
            while (itr.MoveNext())
            {
                FileInfo fileInfo = itr.Current.Value;
                string fullname = FileManager.GetFileFullName(fileInfo.fullname);
                string directory = Path.GetDirectoryName(fullname);
                if (!FileManager.IsDirectoryExist(directory))
                {
                    FileManager.CreateDirectory(directory);
                }
                //拷贝文件
                yield return FileManager.StartCopyInitialFile(fileInfo.fullname);
                currentcopycount++;
            }
            itr.Dispose();
            isCopyDone = true;
            Debuger.LogError("拷贝成功");
        }
        
        IEnumerator CheckSDK()
        {
            StatusText.text = LauncherString.GetInstance().GetString("Resource_CheckSDK");
            ProgressText.text = string.Format("{0}%", 0);
            Singleton<Interface>.GetInstance().Init();
            yield return new WaitForSeconds(15);
            State = enClientState.State_InitSDK;
        }

        IEnumerator UpdateApp()
        {
            m_updateState = 1;
            isUpdateAppDone = false;
            m_urlIndex = Singleton<ServerConfig>.GetInstance().UpdateServer.Length - 1;
            m_loadOver = false;
            while (!m_loadOver && m_urlIndex>=0)
            {
                yield return SingletonMono<GameFrameWork>.GetInstance().StartCoroutine(DownloadAppFile());
                m_urlIndex--;
            }
            //获取沙河目录版本
            GetCurrentVersion();
            Debug.LogError("沙河目录版本 "+currentVersion +" 服务器上版本 "+onlineVersion);
            //下载服务器版本结束
            //获取沙河目录app版本  比较本地与服务器的高低
            var result = CompareVersion(onlineVersion, currentVersion);
            Debug.LogError("比较结果   "+result.ToString());
            switch (result)
            {
                  case CompareResult.Equal:
                      isUpdateAppDone = true;
                      yield break;
                  case CompareResult.Error:
                      isUpdateAppDone = true;
                      yield break;
                  case CompareResult.Greater:
                      m_updateState = 2;
#if UNITY_ANDROID
                      int channelId = 9;
#elif UNITY_IPHONE
                      int channelId = 9;    
#endif
                      using (var www = new WWW(Singleton<ServerConfig>.GetInstance().UpdateAppUrl))
                      {
                          yield return www;
                          if (www.error != null)
                          {
                              yield break;
                          }
                          string line = www.text.Trim();
                          string appurl = null;
                          using (StringReader br = new StringReader(line))
                          {
                              string str;
                              while ((str = br.ReadLine()) != null)
                              {
                                  var pair = str.Split(',');
                                  if (pair.Length == 2)
                                  {
                                      int id = int.Parse(pair[0]);
                                      if (id == channelId)
                                      {
                                          appurl = pair[1];
                                          break;
                                      }
                                  }
                              }
                          }
                          Debug.LogError("下载安装包地址: "+appurl);
                          //下载安装包
                          AlertContextText.text = Singleton<LauncherString>.GetInstance()
                              .GetString("Resource_DownloadNewApp");
                          AlertObject.SetActive(true);
                          EventTriggerListener.Get(CancelButton.gameObject).SetEventHandle(EnumTouchEventType.OnClick,
                              (a, b, c) =>
                              {
                                  Debug.LogError("cancel"); 
                                  Application.Quit();
                              });
                          EventTriggerListener.Get(SureButton.gameObject).SetEventHandle(EnumTouchEventType.OnClick,
                              (a, b, c) =>
                              {
                                  Debug.LogError("sure"); 
                                  Application.OpenURL(appurl);
                                  Application.Quit();
                              });
                      }
                      yield break;
                  case CompareResult.Less:
                      isUpdateAppDone = true;
                      yield break;
            }

        }

        public IEnumerator DownloadAppFile()
        {
            //正式版本
//            SrcUrl = Singleton<ServerConfig>.GetInstance().UpdateServer[m_urlIndex];
//            string url = SrcUrl + Platform.AppVerFileName;
//            string suffix = "?version=" + DateTime.Now.Ticks.ToString();
//            url += suffix;
            //本地测试版本
            SrcUrl = "http://192.168.6.24:8000/Documents/qyz/trunk/dist/apk_version.txt";
            using (WWW w = new WWW(SrcUrl))
            {
                yield return w;
                if (w.error != null)
                {
                    m_loadOver = true;
                    yield return null;
                }
                onlineVersion = w.text.Trim();//服务器上的app版本
                m_loadOver = true;
            }
            Debug.LogError("服务器上的APP 版本"+onlineVersion);
        }
        #endregion
    }

}
