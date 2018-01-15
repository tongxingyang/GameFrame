using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using UnityEngine;

namespace GameFrame
{
    public class Platform
    {
            public static string HasUpdateFileName = "hasupdate.txt";
#if UNITY_EDITOR
        public static string STREAMING_ASSETS_PATH = Application.streamingAssetsPath;
        public static string PERSISTENT_DATA_PATH = Application.dataPath + "/PersistentAssets";
        public static string SrcRoot = "/pc/";
        public static string ResVersionFileName = "pc_version.txt";
        public static string AppVerFileName = "exe_version.txt";
        public static string Md5FileName = "pc_resource_md5.txt";
        public static string ServerAppVersionFileName =SrcRoot  + "exe_version.txt";
#elif UNITY_IPHONE
        public static string STREAMING_ASSETS_PATH = Application.streamingAssetsPath;
        public static string PERSISTENT_DATA_PATH = Application.persistentDataPath;
        public static string SrcRoot = "/android/";
        public static string AppVerFileName = "app_version.txt";
        public static string Md5FileName = "ios_resource_md5.txt";
        public static string ResVersionFileName = "ios_version.txt";
        public static string ServerAppVersionFileName = SrcRoot + "app_version.txt";
#elif UNITY_ANDROID
        public static string STREAMING_ASSETS_PATH = Application.streamingAssetsPath;
        public static string PERSISTENT_DATA_PATH = Application.persistentDataPath;
        public static string SrcRoot = "/ios/";
        public static string AppVerFileName = "apk_version.txt";
        public static string Md5FileName = "android_resource_md5.txt";
        public static string ResVersionFileName = "android_version.txt";
        public static string ServerAppVersionFileName = SrcRoot + "apk_version.txt";
#endif
        public static readonly string Root = "Data/";
        public static readonly string Path = Platform.PERSISTENT_DATA_PATH + "/" + Root;
        public static readonly string InitalPath = Platform.STREAMING_ASSETS_PATH + "/" + Root;
        
        
    }
   
}
