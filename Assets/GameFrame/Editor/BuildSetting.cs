using UnityEngine;

namespace GameFrame.Editor
{
    public class BuildSetting
    {
        public static string Lua = "Assets/GameFrame/Lua";
        public static string LuaBytes = "Assets/LuaBytes";//加密后的lua目录
        public static string ConfigCSV =  "Assets/GameFrame/Config/CSV";
        public static string ConfigXML =  "Assets/GameFrame/Config/XML";
        public static string ConfigCSVBytes =  "Assets/ConfigBytes/CSV";
        public static string ConfigXMLBytes =  "Assets/ConfigBytes/XML";
        public static string AssetBundle = "AssetBundles";
        public static string LuaBundleKey = "LuaKey";
        public static string ConfigBundleKey = "ConfigKey";
        public static string AssetBundleExt = ".assetbundle";
        public static string AssetBundleExportPath = "Assets/StreamingAssets/";
        public static string DepFileName = "DepInfo.all";
        public static string Resource_MD5 = "Resource_MD5.txt";
    }
}