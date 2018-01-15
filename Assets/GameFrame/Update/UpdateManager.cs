using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
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
        private bool isLoadOldTableAndVersion = false;
        public override void Init()
        {
            base.Init();
            CanvasObj = GameObject.Find("Canvas");
            //加载本地文件列表
//            string oldmd5file = Platform.InitalPath + Platform.Md5FileName;
//            oldmd5Table.Clear();
//            using (StreamReader sr = File.OpenText(oldmd5file))
//            {
//                string line;
//                while ((line = sr.ReadLine())!=null)
//                {
//                    var pair = line.Split(',');
//                    if (pair.Length == 3)
//                    {
//                        FileInfo fileInfo = new FileInfo();
//                        fileInfo.fullname = pair[0];
//                        fileInfo.md5 = pair[1];
//                        fileInfo.size = int.Parse(pair[2]);
//                        oldmd5Table[pair[0]] = fileInfo;
//                    }else if (pair.Length == 2)
//                    {
//                        FileInfo fileInfo = new FileInfo();
//                        fileInfo.fullname = pair[0];
//                        fileInfo.md5 = pair[1];
//                        fileInfo.size = 0;
//                        oldmd5Table[pair[0]] = fileInfo;
//                    }
//                }
//            }
//            //
//            string version = Platform.InitalPath + Platform.AppVerFileName;
//            using (StreamReader sr = File.OpenText(version))
//            {
//                initalVersion = sr.ReadToEnd();
//            }
            SingletonMono<GameFrameWork>.GetInstance().StartCoroutine(LoadOldTableAndVersion());
        }

        private IEnumerator LoadOldTableAndVersion()
        {
            isLoadOldTableAndVersion = false;
            string filepath = Platform.Path + Platform.AppVerFileName;
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IPHONE
            filepath = "file:///" + filepath;
#endif
            using (WWW w = new WWW(filepath))
            {
                yield return w;
                if (w.error != null)
                {
                    Debuger.LogError(w.error);
                    yield break;
                }
                initalVersion = w.text;
            }
            filepath = Platform.Path + Platform.Md5FileName;
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IPHONE
            filepath = "file:///" + filepath;
#endif
            using (WWW w = new WWW(filepath))
            {
                yield return w;
                if (w.error != null)
                {
                    Debuger.LogError(w.error);
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
        }

        public void Update()
        {
            if (m_isBeginUpdate)
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
                            
                        }
                        break;
                    case enClientState.State_UnZipData:
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
//                    Debuger.LogError(w.error);
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
                    Debuger.LogError(e.Message);
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
            string filepath = Platform.Path + "hasupdate.txtx";
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
                SingletonMono<GameFrameWork>.GetInstance().StartCoroutine(Preprocess());
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
            }
            itr.Dispose();
            Debuger.LogError("拷贝成功");
        }

        #region 协成

        IEnumerator CheckSDK()
        {
            StatusText.text = LauncherString.GetInstance().GetString("Resource_CheckSDK");
            Singleton<Interface>.GetInstance().Init();
            yield return new WaitForSeconds(3);
            State = enClientState.State_InitSDK;
        }

        #endregion
    }

}
