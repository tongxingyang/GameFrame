using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace GameFrame.ConfigManager
{
    public class ConfigManager:Singleton<ConfigManager>
    {
        public static bool IsOpenStartLoadConfigBundle = false;
        protected Dictionary<string, AssetBundle> configMap = new Dictionary<string, AssetBundle>();

        public override void Init()
        {
            base.Init();
        }

        public void InitConfig()
        {
            InitConfigBundle();
        }
        private void InitConfigBundle()
        {
            if (IsOpenStartLoadConfigBundle)
            {
                string[] files = Directory.GetFiles(ConfigConst.configResDir, "*" + Platform.AssetBundleExt);
                foreach (string file in files)
                {
                    var filename = file.Replace(ConfigConst.configResDir+"/", "");
                    string url = ConfigConst.configResDir+"/"+ filename.ToLower();
                    if (File.Exists(url))
                    {
                        AssetBundle bundle = AssetBundle.LoadFromFile(url);
                        if (bundle != null)
                        {
                            filename = filename.Replace(Platform.AssetBundleExt, "");
                            configMap[filename.ToLower()] = bundle;
                        }
                    }
                }
            }
        }
        public virtual string ReadFile(string fileName,bool isCacheBundle)
        {
            if (!ConfigConst.ConfigBundleMode)
            {
                string path = FindFile(fileName);
                string str = null;

                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
#if !UNITY_WEBPLAYER
                    str = File.ReadAllText(path);
#else
                    throw new Exception("can't run in web platform, please switch to other platform");
#endif
                }

                return str;
            }
            else
            {
                return ReadZipFile(fileName,isCacheBundle);
            }
        }
        public string FindFile(string fileName)
        {
            if (fileName == string.Empty)
            {
                return string.Empty;
            }
            string fullPath = null;
            fullPath = ConfigConst.configDir + "/" + fileName;
            if (File.Exists(fullPath))
            {
                return fullPath;
            }
            

            return null;
        }
        string ReadZipFile(string fileName,bool isCacheBundle)
        {
            AssetBundle zipFile = null;
            string content = string.Empty;
            byte[] buffer = null;
            string zipName = null;

            using (CString.Block())
            {
                
                CString sb = CString.Alloc(256);
                sb.Append("config");
                int pos = fileName.LastIndexOf('/');

                if (pos > 0)
                {
                    sb.Append("_");                    
                    sb.Append(fileName, 0, pos).ToLower().Replace('/', '_');                                        
                    fileName = fileName.Substring(pos + 1);
                }
                zipName = sb.ToString();
                
                configMap.TryGetValue(zipName, out zipFile);
            }
            var isHaveCache = false;
            if (zipFile == null)
            {
                isHaveCache = false;
                zipFile = AssetBundle.LoadFromFile(ConfigConst.configResDir + "/" + zipName+Platform.AssetBundleExt);
                if (isCacheBundle)
                {
                    configMap[zipName] = zipFile;
                }
            }
            else
            {
                isHaveCache = true;
            }
            
            if (zipFile != null)
            {
#if UNITY_5 || UNITY_2017
                TextAsset configCode = zipFile.LoadAsset<TextAsset>(fileName);
#else
                TextAsset configCode = zipFile.Load(fileName, typeof(TextAsset)) as TextAsset;
#endif

                if (configCode != null)
                {
                    buffer = configCode.bytes;
                    Resources.UnloadAsset(configCode);
                    byte[] decrypt = ConfigEncrypt.Decrypt(buffer);
                    content = System.Text.Encoding.UTF8.GetString(decrypt);
                }
                if (!isCacheBundle && !isHaveCache)
                {
                    zipFile.Unload(true);
                }
            }

            return content;
        }

        public string LoadConfig(string fileName,bool isCacheBundle = false)
        {
            string buffer = ReadFile(fileName,isCacheBundle);
            UnityEngine.Debug.Log(buffer);
            return buffer;
        }

    }
}