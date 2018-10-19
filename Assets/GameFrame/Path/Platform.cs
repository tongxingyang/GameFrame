using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using UnityEngine;
namespace GameFrame
{
public class Platform
    {
    #if  UNITY_STANDALONE_WIN||UNITY_EDITOR_WIN
        public static string STREAMING_ASSETS_PATH = Application.dataPath + "/StreamingAssets";
        public static string PERSISTENT_DATA_PATH = Application.streamingAssetsPath;
        public static string DEBUG_LOG_PATH = Application.dataPath+"/DebugerLog/";
        public static string osDir = "windows";
    #elif UNITY_STANDALONE_OSX||UNITY_EDITOR_OSX
        public static string STREAMING_ASSETS_PATH = Application.dataPath + "/StreamingAssets";
        public static string PERSISTENT_DATA_PATH = Application.streamingAssetsPath;
        public static string DEBUG_LOG_PATH = Application.dataPath+"/DebugerLog/";
        public static string osDir = "mac";
    #elif UNITY_IPHONE
        public static string STREAMING_ASSETS_PATH = Application.dataPath + "/Raw";
        public static string PERSISTENT_DATA_PATH = Application.persistentDataPath;
        public static string DEBUG_LOG_PATH = Application.streamingAssetsPath+"/DebugerLog/";
        public static string osDir = "ios";
    #elif UNITY_ANDROID
        public static string STREAMING_ASSETS_PATH = "jar:file://" + Application.dataPath + "!/assets";
        public static string PERSISTENT_DATA_PATH = Application.persistentDataPath;
        public static string DEBUG_LOG_PATH = Application.streamingAssetsPath+"/DebugerLog/";
        public static string osDir = "android"; 
    #endif

        public static readonly string Path = Platform.PERSISTENT_DATA_PATH + "/" + osDir+"/";
        public static readonly string InitalPath = Platform.STREAMING_ASSETS_PATH + "/" + osDir+"/";
        
        public static string HasUpdateFileName = "hasupdate.txt";
        public static string AssetBundle = "assetbundles";
        public static string LuaBundleKey = "LuaKey";
        public static string ConfigBundleKey = "ConfigKey";
        public static string AssetBundleExt = ".assetbundle";
        public static string AssetBundleExportPath = "Assets/StreamingAssets/";
        public static string DepFileName = "depinfo.all";
        public static string AppVerFileName = "appversion.txt";
        public static string Md5FileName = "resource_md5.txt";
        public static string PreloadList = "preloadlist.txt";
                
        /// <summary>
        /// 是否使用bundle 加载UI false默认在resource文件夹中查找
        /// </summary>
        public static bool IsLoadFromBundle = false;
                
        public static bool IsMobile
        {
            get
            {
                if (Application.platform == RuntimePlatform.Android ||
                    Application.platform == RuntimePlatform.IPhonePlayer)
                {
                        return true;
                }
                else
                {
                        return false;
                }
            }
        }

        public static bool IsPC
        {
            get
            {
                if (Application.platform == RuntimePlatform.WindowsPlayer ||
                    Application.platform == RuntimePlatform.OSXPlayer ||
                    Application.platform == RuntimePlatform.LinuxPlayer)
                {
                        return true;
                }
                else
                {
                        return false;
                }
            }
        }

        public static bool IsEditor
        {
            get
            {
                if (Application.platform == RuntimePlatform.LinuxEditor ||
                    Application.platform == RuntimePlatform.WindowsEditor ||
                    Application.platform == RuntimePlatform.OSXEditor)
                {
                        return true;
                }
                else
                {
                        return false;
                }
            }
        }

        private static Dictionary<RuntimePlatform, string> _PlatformDirectorDict;
        
        public static Dictionary<RuntimePlatform, string> PlatformDirectorDict
        {
            get
            {
                if (_PlatformDirectorDict == null)
                {
                    _PlatformDirectorDict = new Dictionary<RuntimePlatform, string>();
                    _PlatformDirectorDict.Add(RuntimePlatform.Android, "Android");
                    _PlatformDirectorDict.Add(RuntimePlatform.IPhonePlayer, "IOS");
                    _PlatformDirectorDict.Add(RuntimePlatform.PS4, "PS4");
                    _PlatformDirectorDict.Add(RuntimePlatform.OSXPlayer, "OSX");
                    _PlatformDirectorDict.Add(RuntimePlatform.OSXEditor, "OSX");
                    _PlatformDirectorDict.Add(RuntimePlatform.WindowsPlayer, "Windows");
                    _PlatformDirectorDict.Add(RuntimePlatform.WSAPlayerX86, "Windows");
                    _PlatformDirectorDict.Add(RuntimePlatform.WSAPlayerX64, "Windows");
                    _PlatformDirectorDict.Add(RuntimePlatform.WSAPlayerARM, "Windows");
                    _PlatformDirectorDict.Add(RuntimePlatform.WindowsEditor, "Windows");
                }
                return _PlatformDirectorDict;
            }
        }


        
        public static string GetPlatformDirectoryName(RuntimePlatform platform, bool editor = false)
        {
            if (editor == false)
            {
                #if UNITY_STANDALONE_OSX
                platform = RuntimePlatform.OSXPlayer;
                #elif UNITY_STANDALONE_WIN
                platform = RuntimePlatform.WindowsPlayer;
                #elif UNITY_ANDROID
                platform = RuntimePlatform.Android;
                #elif UNITY_IOS || UNITY_IPHONE
                platform = RuntimePlatform.IPhonePlayer;
                #endif
            }
            return PlatformDirectorDict[platform];
        }

        public static string GetPlatformDirectory(RuntimePlatform platform, bool editor = false)
        {
            return GetPlatformDirectoryName(platform, editor);
        }
        
        public static string PlatformDirectoryName
        {
            get
            {
                return PlatformDirectorDict[Application.platform];
            }
        }
    }     
}
