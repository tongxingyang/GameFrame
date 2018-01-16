using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Web;
using GameFrame;
using Junfine.Debuger;
using UnityEngine;
using UnityEngine.Experimental.UIElements.StyleEnums;
using UnityEngine.UI;
namespace GameFrame
{
    public enum enClientState
    {
        State_Init,
        State_InitSDK,
        State_UnZipData,
        State_UpdateApp,
        State_UpdateResource,
        State_GetServerList,
        State_Start,
        State_Game,
        State_LoadScene,
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
        private Dictionary<string,FileInfo> oldmd5Table = new Dictionary<string, FileInfo>();
        private Dictionary<string,FileInfo> newmd5Table = new Dictionary<string, FileInfo>();
        private string initalVersion;
        private string currentVersion;
        private string onlineVersion;
        private string initalResVersion;
        private string currentResVersion;
        private string onlineResVersion;
        private bool isLoadOldTableAndVersion = false;
        private bool isCopyDone = false;
        private bool isUpdateAppDone = false;
        private int filecount = 0;
        private int currentcopycount = 0;

        public int UpdateState
        {
            get { return m_updateState; }
        }

        private int m_updateState = 0;
        private bool m_loadOver = true;
        private int m_urlIndex = 2;
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
                initalResVersion = w.text;
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
                                Singleton<ServerConfig>.GetInstance().Read();
                                SingletonMono<GameFrameWork>.GetInstance().StartCoroutine(UpdateApp());
                            }
                            ProgressSliber.value = 1;
                            ProgressText.text = string.Format("{0}%,100");
                            
                        }
                        else
                        {
                           RefLauncherInfo();
                        }
                        break;
                    case  enClientState.State_UpdateApp:
                        
                        break;
                    case  enClientState.State_UpdateResource:
                        break;
                    case  enClientState.State_GetServerList:
                        break;
                    case  enClientState.State_Start:
                        break;
                    case  enClientState.State_LoadScene:
                        break;
                    case  enClientState.State_Game:
                        break;
                }
            }
           
        }

//        private IEnumerator GetInitialVersion()
//        {
//            string filepath = Platform.Path + Platform.AppVerFileName;
//#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IPHONE
//            filepath = "file:///" + filepath;
//#endif
//            using (WWW w = new WWW(filepath))
//            {
//                yield return w;
//                if (w.error != null)
//                {
//                    //Debuger.LogError(w.error);
//                    yield break;
//                }
//                initalVersion = w.text;
//            }
//        }

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
                    string version = File.ReadAllText(filename);
                    if (!string.IsNullOrEmpty(version))
                    {
                        currentVersion = version;
                        return currentVersion;
                    }
                }
                catch (Exception e)
                {
                    //Debuger.LogError(e.Message);
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
                
               // yield return GetInitialVersion();//安装包的app版本
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
//                Debug.LogError("需要解压拷贝文件 oldtable legth"+oldmd5Table.Count);
//                Debug.LogError(" md5 文件路径 " + Platform.InitalPath+Platform.Md5FileName);
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
            int[] vervalue1 = null, vervalue2 = null;
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
                
            }
        }

        private void RefLauncherInfo()
        {
            if (State == enClientState.State_UnZipData)
            {
                StatusText.text = Singleton<LauncherString>.GetInstance().GetString("Resource_Unzip");
                RefProgress((float)currentcopycount/filecount);

            }else if (State == enClientState.State_GetServerList)
            {
                
            }else if (State == enClientState.State_UpdateApp)
            {
                
            }else if (State == enClientState.State_UpdateResource)
            {
                
            }else if (State == enClientState.State_Game)
            {
                
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

        #region 协成

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
            //下载服务器版本结束
            //获取沙河目录app版本  比较本地与服务器的高低
            var result = CompareVersion(onlineVersion, currentVersion);
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
                          //下载安装包// todo 
                          
                      }
                      yield break;
                  case CompareResult.Less:
                      isUpdateAppDone = true;
                      yield break;
            }

        }

        public IEnumerator DownloadAppFile()
        {
            SrcUrl = Singleton<ServerConfig>.GetInstance().UpdateServer[m_urlIndex];
            string url = SrcUrl + Platform.AppVerFileName;
            string suffix = "?version=" + DateTime.Now.Ticks.ToString();
            url += suffix;
            using (WWW w = new WWW(url))
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
        }
        #endregion
    }

}
