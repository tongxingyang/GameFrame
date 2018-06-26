using System;
using System.Collections;
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
        private Dictionary<string,Action<string>> CallBacks = new Dictionary<string, Action<string>>();
        public void AsynLoadConfig(string fileName, bool isCacheBundle, Action<string> callBack)
        {
            if (ConfigConst.ConfigBundleMode)
            {
                if (CallBacks.ContainsKey(fileName))
                {
                    Action<string> action = CallBacks[fileName];
                    action += callBack;
                    return;
                }
                else
                {
                    CallBacks[fileName] = callBack;
                    SingletonMono<GameFrameWork>.GetInstance().StartCoroutine(AsynReadFile(fileName, isCacheBundle));
                }
            }
            else
            {
                UnityEngine.Debug.LogError("编辑器模式不支持异步夹杂config文件  ConfigBundleMode==false");
            }
        }

        private IEnumerator AsynReadFile(string fileName,bool isCacheBundle)
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
                AssetBundleCreateRequest assetBundleRequest;
                assetBundleRequest = AssetBundle.LoadFromFileAsync(ConfigConst.configResDir + "/" + zipName+Platform.AssetBundleExt);
                yield return assetBundleRequest;
                if (assetBundleRequest.isDone)
                {
                    zipFile = assetBundleRequest.assetBundle;
                    if (isCacheBundle)
                    {
                        configMap[zipName] = zipFile;
                    }
                }
            }
            else
            {
                isHaveCache = true;
            }
            
            if (zipFile != null)
            {
                TextAsset configCode = null;
#if UNITY_5 || UNITY_2017 
                AssetBundleRequest assetBundleRequest;
                assetBundleRequest = zipFile.LoadAssetAsync<TextAsset>(fileName);
                yield return assetBundleRequest;
                if (assetBundleRequest.isDone)
                {
                    configCode = (TextAsset) assetBundleRequest.asset;
                }
#else
                 configCode = zipFile.Load(fileName, typeof(TextAsset)) as TextAsset;
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
                //回调
                if (CallBacks.ContainsKey(fileName))
                {
                    Action<string> action = CallBacks[fileName];
                    if (action != null)
                    {
                       Delegate[] actions = action.GetInvocationList();
                        foreach (var dele in actions)
                        {
                            var act = (Action<string>)dele;
                            try
                            {
                                act(content);
                            }
                            catch (Exception e)
                            {
                                UnityEngine.Debug.LogError("异步加载config文件出错 "+fileName);
                            }
                        }
                    }
                    CallBacks.Remove(fileName);
                }
                yield return null;
            }
        }
    }
}