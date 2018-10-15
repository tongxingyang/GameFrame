using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using GameFrameDebuger;
using UIFrameWork;
using UnityEngine;
using UnityEngine.UI;
namespace GameFrame
{
    public enum enClientState
    {
        State_Init,//初始化
        State_InitSDK,//初始化sdk
        State_UnZipData,//解压资源
        State_UpdateApp,//更新应用
        State_UpdateResource,//更新资源
        State_Start,//更新完成
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
        private Text AlertContextText;
        private Text StatusText;
        private Text ProgressText;
        private Text CurrentAppVersionText;
        private Text OnlineAppVersionText;
//        private Text AppVersion;
        private Button SureButton;
        private Button CancelButton;
        private Slider ProgressSliber;
        private GameObject UpdateGameObject;
        private GameObject AlertObject;
        private GameObject CanvasObj;

        //下载数量限制
        private static int DOWNLOAD_COUNT = 50;
        private int CurrentDownCount = 0;
        private Action<bool> UpdateCallback;
        private bool m_isBeginUpdate = false;
        private bool m_isCheckSDK = false;
        private bool m_isInitSDK = false;
        private bool m_isInitSDKDone = false;
        private enClientState State = enClientState.State_Init;
        public  Dictionary<string,FileInfo> oldmd5Table = new Dictionary<string, FileInfo>();
        public  Dictionary<string,FileInfo> newmd5Table = new Dictionary<string, FileInfo>();
        public Dictionary <string,FileInfo> ResourcesHasUpdate = new Dictionary<string, FileInfo>();
        public List<string> m_redownloadList = new List<string>();
        public List<string> m_downloadList = new List<string>();
        public  ConcurrentQueue<DownloadTask> m_taskQueue = new ConcurrentQueue<DownloadTask>();
        public  readonly object m_obj = new object();

        private string initalVersion; //安装包的版本
        private string currentVersion;//沙河目录的版本
        private string onlineVersion; //服务器上的版本

        private bool isLoadOldTableAndVersion = false;
        private bool isCopyDone = false;
        private bool isUpdateAppDone = false;
        private bool isDownloadDone = false;

        private int filecount = 0;
        private int currentcopycount = 0;
        public int TotalDownloadSize = 0;//总共需要下载的文件大小
        public int DownloadSize = 0;//已经下载的文件大小

        private int m_updateState = 0;
        private bool m_loadOver = true;
        private int m_urlIndex = 2;
        private int m_overThreadNum = 0;
        private string SrcUrl = string.Empty;

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


        public bool IsDoneloadDone
        {
            get { return isDownloadDone; }
        }
        public int UpdateState
        {
            get { return m_updateState; }
        }

        public override void Init()
        {
            base.Init();
            CanvasObj = GameObject.Find("Canvas");
            SingletonMono<GameFrameWork>.GetInstance().StartCoroutine(LoadOldTableAndVersion());
        }

        private IEnumerator LoadOldTableAndVersion()
        {
            isLoadOldTableAndVersion = false;
            string filepath = Platform.InitalPath + Platform.AppVerFileName;
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IPHONE
            filepath = "file:///" + filepath;//ios平台
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
            CurrentAppVersionText = UpdateGameObject.transform.Find("TopText/LocalVersion").GetComponent<Text>();
            OnlineAppVersionText = UpdateGameObject.transform.Find("TopText/OnlineVersion").GetComponent<Text>();

            AlertObject = UpdateGameObject.transform.Find("Alert").gameObject;

            SureButton = UpdateGameObject.transform.Find("Alert/Sure").GetComponent<Button>();
            CancelButton = UpdateGameObject.transform.Find("Alert/Cancel").GetComponent<Button>();

            ProgressSliber = UpdateGameObject.transform.Find("Slider").GetComponent<Slider>();
            m_isBeginUpdate = true;
            
            //测试模式
//            ClearData();
        }

        public void OnUpdate()
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
                        if (!m_isInitSDK)
                        {
                            m_isInitSDK = true;
                            m_isInitSDKDone = false;
                            SingletonMono<GameFrameWork>.GetInstance().StartCoroutine(InitSDK());
                        }
                       
                        if (m_isInitSDK && this.m_isInitSDKDone)
                        {
                            this.State = enClientState.State_UnZipData;
                            CheckVersion();
                        }
                        break;
                    case enClientState.State_UnZipData:
                        if (isCopyDone)
                        {
                             //获取网络信息
                            if (Util.NetAvailable)
                            {
                                //没有网络
                                StatusText.text = Singleton<LauncherString>.GetInstance().GetString("Resource_OffLine");
                            }
                            else
                            {
                                State = enClientState.State_UpdateApp;
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
                            SingletonMono<GameFrameWork>.GetInstance().StartCoroutine(UpdateResource());
                        }
                        RefLauncherInfo();
                        break;
                    case  enClientState.State_UpdateResource:
                        if (isDownloadDone)
                        {
                            State = enClientState.State_Start;
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
                        m_isBeginUpdate = false;
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
            if(FileManager.IsFileExist(filename))
            {
                try
                {
                    string version = FileManager.ReadAllText(filename);
                    if (!string.IsNullOrEmpty(version))
                    {
                        currentVersion = version;
                        return currentVersion;
                    }
                }
                catch (Exception e)
                {
                    Debuger.LogError(e.Message);
                    return null;
                }
            }
            return null;
        }

        private void CheckVersion()
        {

            int isCopy = PlayerPrefsUtil.GetIntSimple(PlayerPrefsKey.IsCopyAssets);
            
            bool needUnzip = false;
            
            //TODO 没考虑用户手动删除的问题
            
            if (isCopy==1)
            {
                GetCurrentVersion();
                if (currentVersion != null && initalVersion != null)
                {
                    var result = CompareVersion(initalVersion, currentVersion);
                    if (result == CompareResult.Greater)
                    {
                        //安装包的版本大于沙河目录的版本
                        needUnzip = true;
                    }
                    
                }else if (currentVersion == null && initalVersion != null)
                {
                    needUnzip = true;
                }
            }
            else
            {
                needUnzip = true;
            }
            if (needUnzip)
            {
                ClearData();
                SingletonMono<GameFrameWork>.GetInstance().StartCoroutine(Preprocess());
            }
            else
            {
                isCopyDone = true;
                PlayerPrefsUtil.SetIntSimple(PlayerPrefsKey.IsCopyAssets,1);
            }
        }

        /// <summary>
        /// 刪除沙河目录的数据
        /// </summary>
        public void ClearData()
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
        //1.1.1 1.1.0
        public CompareResult CompareVersion(string ver1, string ver2)
        {
            
            int[] vervalue1 = new int[3], vervalue2 = new int[3];
            string[]  strver1 = ver1.Split('.');
            string[]  strver2 = ver2.Split('.');
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
           
            return CompareResult.Equal;
        }
        public CompareResult CompareVersion(string ver1, string ver2,int count)
        {
            
            int[] vervalue1 = new int[count], vervalue2 = new int[count];
            string[]  strver1 = ver1.Split('.');
            string[]  strver2 = ver2.Split('.');
            try
            {
                for (int i = 0; i < count; i++)
                {
                    
                    vervalue1[i] = int.Parse(strver1[i]);
                    vervalue2[i] = int.Parse(strver2[i]);
                }
            }
            catch
            {
                return CompareResult.Error;
            }
            for (int i = 0; i < count; i++)
            {
                if (vervalue1[i] > vervalue2[i])
                    return CompareResult.Greater;
                if (vervalue1[i] < vervalue2[i])
                    return CompareResult.Less;
            }
           
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

        private void RefAppVersion(string cur,string onl)
        {
            if (UpdateGameObject)
            {
                CurrentAppVersionText.text = cur;
                OnlineAppVersionText.text = onl;
            }
        }  
       

        private void RefLauncherInfo()
        {
            if (State == enClientState.State_UnZipData)
            {
                StatusText.text = Singleton<LauncherString>.GetInstance().GetString("Resource_Unzip");
                RefProgress((float)currentcopycount/filecount);

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
                    StatusText.text = Singleton<LauncherString>.GetInstance().GetString("Resource_GetServerList");
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
                        continue;
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
                    string[] pair = line.Split(',');
                    FileInfo fileInfo = new FileInfo();
                    if (pair.Length == 3)
                    {
                        fileInfo.fullname = pair[0];
                        fileInfo.md5 = pair[1];
                        fileInfo.size = int.Parse(pair[2]);
                    }else if (pair.Length == 2)
                    {
                        fileInfo.fullname = pair[0];
                        fileInfo.md5 = pair[1];
                        fileInfo.size = 0;
                    }
                    else
                    {
                        fileInfo.fullname = pair[0];
                        fileInfo.md5 = " ";
                        fileInfo.size = 0;
                    }
                    if (oldmd5Table.ContainsKey(fileInfo.fullname))
                    {
                        oldmd5Table[fileInfo.fullname] = fileInfo;
                    }
                    else
                    {
                        oldmd5Table.Add(fileInfo.fullname,fileInfo);
                    }
                    ResourcesHasUpdate.Add(fileInfo.fullname,fileInfo);
                }
            }
            Debuger.LogError("加载本地hasupdate文件成功");
        }

        /// <summary>
        /// 验证本地资源是否已经丢失了 并从oldmd5删除对应文件
        /// </summary>
        /// <returns></returns>
        private bool CheckOldMd5File()
        {
            List<string> lostFiles = new List<string>();
            foreach (KeyValuePair<string,FileInfo> keyValuePair in oldmd5Table)
            {
                FileInfo fileInfo = keyValuePair.Value;
                string filename = Platform.Path + fileInfo.fullname;
                if (FileManager.IsFileExist(filename))
                {
                    continue;
                }
                lostFiles.Add(fileInfo.fullname);
                
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
                deleteFiles.Add(keyValuePair.Key);
                //删除沙河目录下的没用文件 
                oldmd5Table.Remove(keyValuePair.Key);
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
                }// 新添加的文件直接加入下载队列中
                m_downloadList.Add(keyValuePair.Key);
                TotalDownloadSize += newmd5Table[keyValuePair.Key].size;
            }
            if (m_downloadList.Count == 0)
            {
                isDownloadDone = true;
            }
            Debuger.LogError("需要下载的数量 "+m_downloadList.Count +",    大小  "+TotalDownloadSize);
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
                m_overThreadNum++;
            }
        }
        /// <summary>
        /// 保存md5文件 都更新成功后保存
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
                Debuger.LogError(e.Message);
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
            if (!string.IsNullOrEmpty(directory) && FileManager.IsDirectoryExist(directory))
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
                Debuger.LogError(e.Message);
            }
        }

        private void ClearResourceHasUpdate()
        {
            string filename = Platform.Path + Platform.HasUpdateFileName;
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
                File.Create(filename);
            }
            catch (Exception e)
            {
                Debuger.LogError(e.Message);
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
                Debuger.LogError(e.Message);
            }
        }
        /// <summary>
        /// 向本地hasupdate文件追加信息
        /// </summary>
        /// <param name="key"></param>
        public void AppendHasUpdateFile(string key,string md5,int size)
        {
            string filename = Platform.Path + Platform.HasUpdateFileName;
            try
            {
                using (var write = new StreamWriter(new FileStream(filename,FileMode.Append)))
                {
                    write.WriteLine(key+","+md5+","+size);
                }
            }
            catch (Exception e)
            {
                Debuger.LogError(e.Message);
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
            isDownloadDone = false; 
            LoadCurrentMd5Table();
            LoadHasUpdateSet();
            bool bCheck = CheckOldMd5File();
            bool bVersion = true;
            int curSmallVer = int.Parse(currentVersion.Split('.')[2]);
            int onlineSmallVer = int.Parse(onlineVersion.Split('.')[2]);
            //当前的更新小版本比较
            if (curSmallVer < onlineSmallVer)
            {
                bVersion = false;
            }
            Debuger.LogError("是否需要更新资源= "+!bVersion+", 沙河目录的app版本 = "+currentVersion+", 服务器上的app版本 = "+onlineVersion);
            if (!bCheck && bVersion)
            {
                m_updateState = 3;
                isDownloadDone = true;
                yield break;
            }
            m_urlIndex = GameConfig.UpdateServer.Length-1;
            m_loadOver = false;
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
            GetDownloadFileList();
            if (!isDownloadDone)
            {
                if (Util.IsCarrier)
                {
                    if (TotalDownloadSize > 1048576) 
                    {
                        AlertContextText.text =
                            string.Format(Singleton<LauncherString>.GetInstance().GetString("DownloadSizeAlert"),
                                TotalDownloadSize / 1048576f);
                        AlertObject.SetActive(true);
                        EventTriggerListener.Get(CancelButton.gameObject).SetEventHandle(EnumTouchEventType.OnClick,
                            (a, b, c) =>
                            {
                                Application.Quit();
                            });
                        EventTriggerListener.Get(SureButton.gameObject).SetEventHandle(EnumTouchEventType.OnClick,
                            (a, b, c) =>
                            {
                                AlertObject.gameObject.SetActive(false);
                                SingletonMono<GameFrameWork>.GetInstance().StartCoroutine(BeginDownloadResource());
                            });
                    }
                    else
                    {
                          SingletonMono<GameFrameWork>.GetInstance().StartCoroutine(BeginDownloadResource());
                    }
                }
                else
                {
                      SingletonMono<GameFrameWork>.GetInstance().StartCoroutine(BeginDownloadResource());
                }
            }
            else
            {
                m_updateState = 3;
                RefAppVersion(currentVersion,onlineVersion);
            }
        }

        IEnumerator BeginDownloadResource()
        {
            //设置最大下载数量
            System.Net.ServicePointManager.DefaultConnectionLimit = DOWNLOAD_COUNT;
            m_updateState = 2;
            m_urlIndex = 0;
            Debuger.LogError("开始下载时间 "  +Time.realtimeSinceStartup);
            while (m_urlIndex < GameConfig.UpdateServer.Length)
            {
                SrcUrl = GameConfig.UpdateServer[m_urlIndex]+"Documents/qyz/trunk/dist/Data/";
                CurrentDownCount = 0;
                m_taskQueue.Clear();
                m_redownloadList.Clear();
                
                foreach (string s in m_downloadList)
                {
                    string url = SrcUrl + s;
                    string filepath = Platform.Path + s;
                    DownloadTask task = new DownloadTask(url,s,filepath);
                    task.callback = DownCallBack;
                    m_taskQueue.Enqueue(task);
                }
                if (m_taskQueue.Count > 0)
                {
                    m_overThreadNum = 0;
                    for (int i = 0; i < 5; i++)
                    {
                        Thread thread = new Thread(ThreadProc);
                        thread.Name = "download thread: " + i;
                        thread.Start();
                    }
                    while (CurrentDownCount!=m_downloadList.Count)
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
                    //清空hasupdate的内容
                    ClearResourceHasUpdate();
                    isDownloadDone = true;
                    Debuger.LogError("下载完成........");
                    RefAppVersion(currentVersion,onlineVersion);
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
                    Debuger.LogError("下载结束时间  "+Time.realtimeSinceStartup);
                    if (newmd5Table.Count > 0)
                    {
                        SaveMD5Table(newmd5Table);
                    }
                    //清空hasupdate的内容
                    ClearResourceHasUpdate();
                    isDownloadDone = true;
                    Debuger.LogError("下载完成..........");
                    RefAppVersion(currentVersion,onlineVersion);
                    break;
                }
                m_urlIndex++;
            }
            
            m_updateState = 4;
            foreach (string file in m_redownloadList)
            {
                Debuger.LogError("下载文件出错 "+file);
            }
        }

        public void DownCallBack()
        {
            CurrentDownCount++;
        }
        /// <summary>
        /// 下载服务器md5
        /// </summary>
        /// <returns></returns>
        private IEnumerator DownloadMD5File()
        {
            newmd5Table.Clear();
            SrcUrl = GameConfig.UpdateServer[m_urlIndex]+"/Documents/qyz/trunk/dist/" + Platform.Md5FileName;
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
                }
            }
        }

        IEnumerator Preprocess()
        {
            if (!FileManager.IsDirectoryExist(Platform.Path))
            {
                FileManager.ClearDirectory(Platform.Path);
            }
            //拷贝文件 从streaming 目录拷贝到 沙河目录
            if (oldmd5Table == null || oldmd5Table.Count == 0) yield break;
            foreach (KeyValuePair<string,FileInfo> keyValuePair in oldmd5Table)
            {
                FileInfo fileInfo = keyValuePair.Value;
                yield return FileManager.StartCopyInitialFile(fileInfo.fullname);
                currentcopycount++;
            }
            isCopyDone = true;
            PlayerPrefsUtil.SetIntSimple(PlayerPrefsKey.IsCopyAssets,1);
            Debuger.Log("拷贝成功");
        }
        
        IEnumerator CheckSDK()
        {
            StatusText.text = LauncherString.GetInstance().GetString("Resource_CheckSDK");
            ProgressText.text = string.Format("{0}%", 0);
            Singleton<Interface>.GetInstance().Init();
            yield return SingletonMono<GameFrameWork>.GetInstance().StartCoroutine(Singleton<Interface>.GetInstance().CheckSDK());
            State = enClientState.State_InitSDK;
        }

        IEnumerator InitSDK()
        {
            StatusText.text = LauncherString.GetInstance().GetString("Resource_InitSDK");
            ProgressText.text = string.Format("{0}%", 0);
            yield return SingletonMono<GameFrameWork>.GetInstance().StartCoroutine(Singleton<Interface>.GetInstance().InitSDK());
            this.m_isInitSDKDone = true;
        }
        
        IEnumerator UpdateApp()
        {
            m_updateState = 1;
            isUpdateAppDone = false;
            m_urlIndex = GameConfig.UpdateServer.Length - 1;
            m_loadOver = false;
            while (!m_loadOver && m_urlIndex>=0)
            {
                yield return SingletonMono<GameFrameWork>.GetInstance().StartCoroutine(DownloadAppFile());
                m_urlIndex--;
            }
            if (m_loadOver == false)//当前服务器列表不可用
            {
                StatusText.text = Singleton<LauncherString>.GetInstance().GetString("Resource_ServerError");
                yield break;
            }
            //获取沙河目录版本
            GetCurrentVersion();
            Debuger.LogError("沙河目录版本 "+currentVersion +" 服务器上版本 "+onlineVersion);
            //获取沙河目录app版本  比较本地与服务器的高低 1.1.1 1.1.0
            var result = CompareVersion(onlineVersion, currentVersion,2);
            Debuger.LogError("比较结果   "+result.ToString());
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
                    using (var www = new WWW(GameConfig.UpdateAppUrl))
                      {
                          yield return www;
                          if (www.error != null)
                          {
                              yield break;
                          }
                          string appurl = www.text.Trim();
                         
                          //下载安装包
                          AlertContextText.text = Singleton<LauncherString>.GetInstance()
                              .GetString("Resource_DownloadNewApp");
                          AlertObject.SetActive(true);
                          EventTriggerListener.Get(CancelButton.gameObject).SetEventHandle(EnumTouchEventType.OnClick,
                              (a, b, c) =>
                              {
                                  Application.Quit();
                              });
                          EventTriggerListener.Get(SureButton.gameObject).SetEventHandle(EnumTouchEventType.OnClick,
                              (a, b, c) =>
                              {
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
            SrcUrl = GameConfig.UpdateServer[m_urlIndex]+"/Documents/qyz/trunk/dist/apk_version.txt";
            using (WWW w = new WWW(SrcUrl))
            {
                yield return w;
                if (w.error != null)
                {
                    m_loadOver = true;
                    onlineVersion = w.text.Trim();//服务器上的app版本
                    Debuger.Log("服务器上的APP 版本"+onlineVersion);
                    yield return null;
                }
            }
        }
        #endregion
    }

}
