using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using UnityEditor;
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
                public static string ServerAppVersionFileName = SrcRoot + "exe_version.txt";
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

                public static RuntimePlatform[] runtimePlatformEnums = new RuntimePlatform[]
                {
                        RuntimePlatform.Android, RuntimePlatform.IPhonePlayer, RuntimePlatform.OSXPlayer,
                        RuntimePlatform.WindowsPlayer
                };

                public static string[] runtimePlatformNames = new string[] {"Android", "IOS", "OSX", "Windows"};

                public static RuntimePlatform CurrentPlatform
                {
                        get
                        {
#if UNITY_STANDALONE_OSX
                                return RuntimePlatform.OSXPlayer;
#elif UNITY_STANDALONE_WIN
                            return RuntimePlatform.WindowsPlayer;
                        #elif UNITY_ANDROID
                            return RuntimePlatform.Android;
                        #elif UNITY_IOS || UNITY_IPHONE
                            return RuntimePlatform.IPhonePlayer;
                        #else
                            return RuntimePlatform.OSXPlayer;
                        #endif
                        }
                }

                public static RuntimePlatform GetRuntimePlatform(int index)
                {
                        if (index >= 0 && index < runtimePlatformEnums.Length)
                        {
                                return runtimePlatformEnums[index];
                        }
                        return CurrentPlatform;
                }

                public static int GetRuntimePlatform(RuntimePlatform platform)
                {
                        switch (platform)
                        {
                                case RuntimePlatform.Android:
                                        return 0;
                                case RuntimePlatform.IPhonePlayer:
                                        return 1;
                                case RuntimePlatform.OSXPlayer:
                                        return 2;
                                case RuntimePlatform.WindowsPlayer:
                                        return 3;
                        }
                        return -1;
                }
#if UNITY_EDITOR
                public static BuildTarget GetBuildTarget(RuntimePlatform platform)
                {
                        switch (platform)
                        {
                                case RuntimePlatform.Android:
                                        return UnityEditor.BuildTarget.Android;
                                case RuntimePlatform.IPhonePlayer:
                                        return UnityEditor.BuildTarget.iOS;
                                case RuntimePlatform.OSXPlayer:
                                        return UnityEditor.BuildTarget.StandaloneOSXIntel;
                                case RuntimePlatform.WindowsPlayer:
                                        return UnityEditor.BuildTarget.StandaloneWindows;
                        }
                        return UnityEditor.BuildTarget.StandaloneWindows;
                }
                public static RuntimePlatform GetRuntimePlatform(UnityEditor.BuildTarget p)
                {
                        switch(p)
                        {
                                case UnityEditor.BuildTarget.Android:
                                        return RuntimePlatform.Android;
                                case UnityEditor.BuildTarget.iOS:
                                        return RuntimePlatform.IPhonePlayer;
                                case UnityEditor.BuildTarget.StandaloneOSXIntel:
                                        return RuntimePlatform.OSXPlayer;
                                case UnityEditor.BuildTarget.StandaloneWindows:
                                        return RuntimePlatform.WindowsPlayer;
                        }
                        return RuntimePlatform.WindowsPlayer;
                }
              

            #endif
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
                _PlatformDirectorDict.Add(RuntimePlatform.PS3, "PS3");
                _PlatformDirectorDict.Add(RuntimePlatform.PS4, "PS4");
                _PlatformDirectorDict.Add(RuntimePlatform.OSXPlayer, "OSX");
                _PlatformDirectorDict.Add(RuntimePlatform.OSXEditor, "OSX");
                _PlatformDirectorDict.Add(RuntimePlatform.WindowsPlayer, "Windows");
                _PlatformDirectorDict.Add(RuntimePlatform.WSAPlayerX86, "Windows");
                _PlatformDirectorDict.Add(RuntimePlatform.WSAPlayerX64, "Windows");
                _PlatformDirectorDict.Add(RuntimePlatform.WSAPlayerARM, "Windows");
                _PlatformDirectorDict.Add(RuntimePlatform.WindowsEditor, "Windows");
                _PlatformDirectorDict.Add(RuntimePlatform.WP8Player, "WP8");
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
        return "Platform/" + GetPlatformDirectoryName(platform, editor);
    }



    public static string PlatformDirectoryName
    {
        get
        {
            return PlatformDirectorDict[Application.platform];
        }
    }

    public static string PlatformDirectory
    {
        get
        {
            return "Platform/" + PlatformDirectorDict[Application.platform];
        }
    }
    }
   
}
