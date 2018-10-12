using UnityEngine;

namespace GameFrame
{
    public class GameConfig:MonoBehaviour
    {
        public bool CheckUpdate = false;
        public int FrameRate = 30;
        public bool EnableLog = true;
        public bool EnableSave = true;
#if UNITY_IPHONE
        public  Vector2 Resolution = new Vector2(1280,720);
#else
        public  Vector2 Resolution = new Vector2(1280,720);
#endif
    }
    public static class LuaConst
    {
        public static bool LuaBundleMode = false;                    //True:从bundle中加载lua, false:直接读lua文件
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