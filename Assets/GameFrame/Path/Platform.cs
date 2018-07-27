using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using UnityEngine;
namespace GameFrame
{
        public class Platform
        {
                public static string HasUpdateFileName = "hasupdate.txt";
                
#if UNITY_EDITOR|| UNITY_EDITOR_OSX||UNITY_STANDALONE_OSX
                public static string STREAMING_ASSETS_PATH = Application.streamingAssetsPath;
                public static string PERSISTENT_DATA_PATH = Application.streamingAssetsPath;

#elif UNITY_IPHONE
                public static string STREAMING_ASSETS_PATH = Application.streamingAssetsPath;
                public static string PERSISTENT_DATA_PATH = Application.persistentDataPath;

#elif UNITY_ANDROID
                public static string STREAMING_ASSETS_PATH = Application.streamingAssetsPath;
                public static string PERSISTENT_DATA_PATH = Application.persistentDataPath;
#endif
                
#if UNITY_STANDALONE_WIN
                public static string osDir = "windows";
#elif UNITY_STANDALONE_OSX
                public static string osDir = "mac";    
#elif UNITY_ANDROID
                public static string osDir = "android";  
#elif UNITY_IPHONE
                public static string osDir = "ios";        
#else
                public static string osDir = "";        
                #endif
                public static readonly string Path = Platform.PERSISTENT_DATA_PATH + "/" + osDir+"/";
                public static readonly string InitalPath = Platform.STREAMING_ASSETS_PATH + "/" + osDir+"/";

                public static string AssetBundle = "assetbundles";
                public static string LuaBundleKey = "LuaKey";
                public static string ConfigBundleKey = "ConfigKey";
                public static string AssetBundleExt = ".assetbundle";
                public static string AssetBundleExportPath = "Assets/StreamingAssets/";
                public static string DepFileName = "depinfo.all";
                public static string ResVersionFileName = "resversion.txt";
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
        public static class LuaConst
        {
                public static bool LuaBundleMode = true;                    //True:从bundle中加载lua, false:直接读lua文件
                public static bool LuaByteMode = false;                       //Lua字节码模式-默认关闭 
                public static string luaDir = Application.dataPath + "/Lua";                //lua逻辑代码目录
                public static string toluaDir = Application.dataPath + "/ToLua/Lua";        //tolua lua文件目录
                public static string luaTempDir = Application.dataPath + "/Temp/Lua";
                public static string toluaTempDir = Application.dataPath + "/Temp/ToLua";
                public static string TempDir = Application.dataPath + "/Temp";
                #if UNITY_STANDALONE_WIN
                public static string osDir = "windows";
                #elif UNITY_STANDALONE_OSX
                public static string osDir = "mac";    
                #elif UNITY_ANDROID
                public static string osDir = "android";  
                #elif UNITY_IPHONE
                public static string osDir = "ios";        
                #else
                public static string osDir = "";        
                #endif
                
                public static string luaResDir = string.Format("{0}/{1}/lua", Platform.PERSISTENT_DATA_PATH, osDir);      //手机运行时lua文件下载目录    
                
                #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN    
                public static string zbsDir = "D:/ZeroBraneStudio/lualibs/mobdebug";        //ZeroBraneStudio目录       
                #elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
                public static string zbsDir = "/Applications/ZeroBraneStudio.app/Contents/ZeroBraneStudio/lualibs/mobdebug";
                #else
                public static string zbsDir = luaResDir + "/mobdebug/";
                #endif    
                
                public static bool openLuaSocket = true;            //是否打开Lua Socket库
                public static bool openLuaDebugger = false;         //是否连接lua调试器
                
        }
        public static class ConfigConst
        {
                public static bool ConfigBundleMode = true;                  
                public static string configDir = Application.dataPath + "/Config";                
                public static string tempconfigDir = Application.dataPath + "/TempConfig";        
                #if UNITY_STANDALONE_WIN
                public static string osDir = "windows";
                #elif UNITY_STANDALONE_OSX
                public static string osDir = "mac";    
                #elif UNITY_ANDROID
                public static string osDir = "android";  
                #elif UNITY_IPHONE
                public static string osDir = "ios";        
                #else
                public static string osDir = "";        
                #endif
                
                public static string configResDir = string.Format("{0}/{1}/config", Platform.PERSISTENT_DATA_PATH, osDir);     
                
             
                
        }
        public static class SoundConst
        {
                public static bool SoundBundleMode = true;      
                public static string soundDir = Application.dataPath + "/Sound";                
#if UNITY_STANDALONE_WIN
                public static string osDir = "windows";
                #elif UNITY_STANDALONE_OSX
                public static string osDir = "mac";    
                #elif UNITY_ANDROID
                public static string osDir = "android";  
                #elif UNITY_IPHONE
                public static string osDir = "ios";        
#else
                public static string osDir = "";        
                #endif
                
                public static string soundResDir = string.Format("{0}/{1}/sound", Platform.PERSISTENT_DATA_PATH, osDir);     
                
             
                
        }
        
}
