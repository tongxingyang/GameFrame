using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using GameFrame;
using UnityEngine;

/// <summary>
/// 获取游戏服务器列表
/// </summary>
namespace GameFrame
{
    public class ServerLoad
    {
        private bool IsConnectionSuccessful = false;
        public string Host { get; private set; }
        public int Port { get; private set; }
        private Socket clent;
        private int timeoutMiliSecond;
        private sbyte load;
        private int serverid;
        
    }
    public class ServerListManager:Singleton<ServerListManager>
    {
        private static string androidContent = "";
        private static string iosContent = "";
        private static bool androidDone = false;
        private static bool iosDone = false;
        private static Dictionary<int,int> serversState = new Dictionary<int, int>();
        private static Dictionary<int, ServerLoad> serverLoads = new Dictionary<int, ServerLoad>();
        private bool stateAltered = false;

        public string this[RuntimePlatform platform]
        {
            get
            {
                if (platform == RuntimePlatform.Android)
                {
                    return androidContent;
                }
                else
                {
                    return iosContent;
                }
            }
            set
            {
                if (platform == RuntimePlatform.Android)
                {
                    androidContent = value;
                }
                else
                {
                    iosContent = value;
                }
            }
        }

        public bool DownloadDone
        {
            get { return androidDone && iosDone; }
        }
        

        public bool StateAltered
        {
            get
            {
                lock (this)
                {
                    return stateAltered;
                }
            }
        }

        public Dictionary<int, int> GetServerState()
        {
            lock (this)
            {
                stateAltered = false;
                return serversState;
            }
        }

        public IEnumerator GetServerList()
        {
            yield return DownloadServerList(3, RuntimePlatform.Android);
            yield return DownloadServerList(3, RuntimePlatform.IPhonePlayer);
        }

        private IEnumerator DownloadServerList(int times,RuntimePlatform platform)
        {
            string url = "";
            if (platform == RuntimePlatform.Android)
            {
                url = Singleton<ServerConfig>.GetInstance().AndroidServerlistUrl;
            }
            else
            {
                url = Singleton<ServerConfig>.GetInstance().IosServerlistUrl;
            }
            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(new Uri(url));
                httpWebRequest.KeepAlive = false;
                httpWebRequest.Timeout = 15 * 1000;
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (Stream stream = httpWebResponse.GetResponseStream())
                {
                    StreamReader streamReader = new StreamReader(stream);
                    this[platform] = streamReader.ReadToEnd();
                    streamReader.Close();
                    if (platform == RuntimePlatform.Android)
                    {
                        androidDone = true;
//                        Debug.LogError("安卓服务器 字段 "+ androidContent);
                    }
                    else
                    {
                        iosDone = true;
//                        Debug.LogError("IOS服务器 字段 "+ iosContent);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                if (times > 0)
                {
                    DownloadServerList(times - 1, platform);
                }
                //三次下载都失败了 todo
//                else
//                {
//                    Singleton<UpdateManager>.GetInstance().
//                }
            }
            yield break;
        }

        public void AddGroupServers(int[] serverids, string[] hosts, int[] ports)
        {
            for (int i = 0; i < serverids.Length; i++)
            {
                AddServer(serverids[i],hosts[i],ports[i]);
            }
        }

        private void AddServer(int serverid,string host,int port)
        {
            if (!serverLoads.ContainsKey(serverid))
            {
                serverLoads.Add(serverid,new ServerLoad());// todo
            }
            //缺少验证服务器是否可用链接
        }
        public void ClearServersState()
        {
            stateAltered = false;
            serversState.Clear();
            serverLoads.Clear();
        }
    }
}

