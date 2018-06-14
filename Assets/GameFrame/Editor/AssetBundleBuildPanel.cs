using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace GameFrame.Editor
{
    public class AssetBundlePanelConfig:ScriptableObject
    {
        public enum EncryptConfigBundle
        {
            Encrypt,
            NoEncrypt,
        }
        public enum EncryptLuaScriptBundle
        {
            Encrypt,
            NoEncrypt,
        }
        public EncryptConfigBundle EncryptConfig;
        public EncryptLuaScriptBundle EncryptLuaScript;
        public List<AssetBundleFilter> Filters = new List<AssetBundleFilter>();
        public string ConfigBundleName = "Config.assetbundle";
        public string LuaScriptBundleName = "LuaScript.assetbundle";
        public int IsSpriteTag = 1;
        public int Windows = 1;
        public int Mac = 1;
        public int Android = 1;
        public int IOS = 1;
        public Version Version;
    }
    [Serializable]
    public class Version
    {
        /// <summary>
        /// 主版本号
        /// </summary>
        public int master = 1;
        /// <summary>
        /// 次版本号
        /// </summary>
        public int minor = 0;
        /// <summary>
        /// 修订版本号
        /// </summary>
        public int revised = 0;
        /// <summary>
        /// 发布时间
        /// </summary>
        public Int64 dateTime = 0;
        /// <summary>
        /// 资源版本表示号
        /// </summary>
        public int resversion = 1;
        
        public void SetNowDatetime()
        {
            dateTime = Convert.ToInt64(DateTime.Now.ToString("yyyyMMddHHmmss"));
        }
        public string ToDateString()
        {
            string str = dateTime.ToString();
            string dstr = string.Format("{0}年{1}月{2}日{3}:{4}", 
                str.Substring(0, 4), 
                str.Substring(4, 2), 
                str.Substring(6, 2), 

                str.Substring(8, 2),
                str.Substring(10, 2)
            );

            return string.Format("{4}-ver{0:D2}.{1:D2}.{2:D2}-resversion{3}", master, minor, revised, resversion,dstr);
        }

        
    }
    [Serializable]
    public class AssetBundleFilter
    {
        [SerializeField]
        public bool vaild = true;
        [SerializeField]
        public string path = String.Empty;
        [SerializeField]
        public string filter = String.Empty;
    }
    
    public class AssetBundleBuildPanel:EditorWindow
    {
        private const string savePath = "Assets/GameFrame/Editor/configasset.asset";
        private AssetBundlePanelConfig _config;
        private ReorderableList _list;
        private Vector2 scrollPos = Vector2.zero;
        private static AssetBundleBuildPanel panel;
        
        static string[] filterDirList = new string[]{};//过滤的文件夹
        static List<string> filterExts = new List<string>{".cs", ".js"};//过滤的后缀
        static List<string> imageExts = new List<string>{".png", ".jpg", ".jpeg", ".bmp", "gif", ".tga", ".tiff", ".psd"};
        public static List<string> exts = new List<string>(new string[]{ ".prefab", ".png", ".jpg", ".jpeg", ".bmp", ".gif", ".tga", ".tiff", ".psd", ".mat", ".mp3", ".ogg",".wav" , ".shader", ".ttf"});
        
        [MenuItem("AssetBundle/AssetBundlePanel")]
        static void Open()
        {
            panel = GetWindow<AssetBundleBuildPanel>(true,"AssetBundlePanel");
            panel.minSize = new Vector2(800,600);
            panel.Show();
        } 
        [MenuItem("AssetBundle/SetAssetBundleName")]
        public static void SetAssetBundleName()
        {
            AssetBundlePanelConfig config =  LoadAssetAtPath<AssetBundlePanelConfig>(savePath);
            if (config == null)
            {
                return;
            }
            List<string> list = new List<string>();
            for (int i = 0; i < config.Filters.Count ; i++)
            {
                if (config.Filters[i].vaild)
                {
                    List<string> exts = new List<string>();
                    string[] strs = config.Filters[i].filter.Split('|');
                    string resourcesPath = config.Filters[i].path;
                    for (int j = 0; j < strs.Length; j++)
                    {
                        exts.Add(strs[j]);
                    }
                    PathResolver.RecursiveFile(resourcesPath,list,exts);
                }
             
            }
            
            // 分析资源的依赖关系
            if (list.Count==0)
            {
                return;
            }
            Dictionary<string, AssetNode> nodeDict = AssetNodeUtil.GenerateAllNode(list, filterDirList, filterExts,
                imageExts, config.IsSpriteTag == 1, Platform.AssetBundleExt);
            AssetNodeUtil.GenerateNodeDependencies(nodeDict);
            List<AssetNode> roots = AssetNodeUtil.FindRoots(nodeDict);
            
            AssetNodeUtil.RemoveParentShare(roots);
            
            AssetNodeUtil.MergeParentCountOnce(roots);
            Dictionary<string, AssetNode> assetDict = AssetNodeUtil.GenerateAssetBundleNodes(roots);
            AssetNodeUtil.SetAssetBundleNames(assetDict,Platform.AssetBundleExt);
            AssetDatabase.RemoveUnusedAssetBundleNames();
        }
        
        [MenuItem("AssetBundle/Clear AssetBundle Name")]
        public static void ClearAssetBundleNames()
        {
            string[] names = AssetDatabase.GetAllAssetBundleNames();

            int count = names.Length;
            for(int i = 0; i < count; i ++)
            {
                if (names[i].IndexOf(Platform.AssetBundleExt) != -1)
                {
                    string[] assets = AssetDatabase.GetAssetPathsFromAssetBundle(names[i]);
                    for (int j = 0; j < assets.Length; j++)
                    {
                        AssetImporter importer = AssetImporter.GetAtPath(assets[j]);
                        importer.assetBundleName = null;
                    }
                }
            }
        }

        [MenuItem("AssetBundle/BuildAssetBundles")]
        static void BuildAssetBundles()
        {
            AssetBundlePanelConfig config =  LoadAssetAtPath<AssetBundlePanelConfig>(savePath);
            if (config == null)
            {
                return;
            }
            SetAssetBundleName();
            var outPath = string.Empty;
            BuildTarget target = BuildTarget.Android;
            if (config.Windows==1)
            {
                outPath = Platform.AssetBundleExportPath+"windows/";
                target = BuildTarget.StandaloneWindows64;
                BuildAssets(config,outPath,target);
                SaveAllDepInfo(outPath);
                MakeMD5(outPath);
                AssetDatabase.Refresh();
            }
            if (config.Mac == 1)
            {
                outPath = Platform.AssetBundleExportPath+"mac/";
                target = BuildTarget.StandaloneOSXIntel64;
                BuildAssets(config,outPath,target);
                SaveAllDepInfo(outPath);
                MakeMD5(outPath);
                AssetDatabase.Refresh();
            }
            if (config.Android == 1)
            {
                outPath = Platform.AssetBundleExportPath+"android/";
                target = BuildTarget.Android;
                BuildAssets(config,outPath,target);
                SaveAllDepInfo(outPath);
                MakeMD5(outPath);
                AssetDatabase.Refresh();
            }
            if (config.IOS == 1)
            {
                outPath = Platform.AssetBundleExportPath+"ios/";
                target = BuildTarget.iOS;
                BuildAssets(config,outPath,target);
                SaveAllDepInfo(outPath);
                MakeMD5(outPath);
                AssetDatabase.Refresh();
            }
            
        }

        public static void BuildAssets(AssetBundlePanelConfig config,string Path, BuildTarget target)
        {
            string outPath = Path+Platform.AssetBundle;
            if (!Directory.Exists(outPath))
            {
                Directory.CreateDirectory(outPath);
            }
            BuildPipeline.BuildAssetBundles(outPath, BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.ChunkBasedCompression, target);
            //打包资源
            if (config.EncryptConfig == AssetBundlePanelConfig.EncryptConfigBundle.Encrypt)
            {
                //加密luabundle包
                string configfilename = outPath + "/" + config.ConfigBundleName;
                if(!File.Exists(configfilename)) return;
                byte[] bytes = File.ReadAllBytes(configfilename);
                bytes = EncryptBytes(bytes, Platform.ConfigBundleKey);
                File.Delete(configfilename);
                File.WriteAllBytes(configfilename,bytes);
            }
            if (config.EncryptLuaScript == AssetBundlePanelConfig.EncryptLuaScriptBundle.Encrypt)
            {
                //加密configbundle包
                string configfilename = outPath + "/" + config.LuaScriptBundleName;
                if(!File.Exists(configfilename)) return;
                byte[] bytes = File.ReadAllBytes(configfilename);
                bytes = EncryptBytes(bytes, Platform.LuaBundleKey);
                File.Delete(configfilename);
                File.WriteAllBytes(configfilename,bytes);
            }
            //生成依赖关系配置文件
            AssetDatabase.Refresh();
        }
        //保存資源之間的依賴配置文件
        private static void SaveAllDepInfo(string Path)
        {
            string outPath = Path + Platform.AssetBundle;
            AssetBundle ab = AssetBundle.LoadFromFile(outPath + "/" + Platform.AssetBundle);
            AssetBundleManifest manifest = ab.LoadAsset("AssetBundleManifest")as AssetBundleManifest;
            string[] assetbundles = manifest.GetAllAssetBundles();
            Dictionary<string,List<string>> depinfos = new Dictionary<string, List<string>>();
            foreach (string assetbundle in assetbundles)
            {
                string[] deps = manifest.GetDirectDependencies(assetbundle);
                List<string> lists = new List<string>();
                foreach (var dep in deps)
                {
                    lists.Add(dep);
                }
                depinfos[assetbundle] = lists;
            }
            //將信息保存在字典中 將字典中的信息保存在本地文本中
            if (File.Exists(Path + "/" + Platform.DepFileName))
            {
                File.Delete(Path+"/"+Platform.DepFileName);
            }
            using (FileStream fs = new FileStream(Path+"/"+Platform.DepFileName,FileMode.CreateNew))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    foreach (KeyValuePair<string,List<string>> keyValuePair in depinfos)
                    {
                        sw.WriteLine(keyValuePair.Key);
                        sw.WriteLine(keyValuePair.Value.Count);
                        foreach (string s in keyValuePair.Value)
                        {
                            sw.WriteLine(s);
                        }
                        sw.WriteLine("<---------------------------->");
                    }
                }
            }
            
        }
        
        [MenuItem("AssetBundle/Build Config Bundle")]
        public static void BuildConfig()
        {
            ConfigEditor.EncryptConfigFile();//加密
            AssetBundlePanelConfig config =  LoadAssetAtPath<AssetBundlePanelConfig>(savePath);
            if (config == null)
            {
                return;
            }
            if (!Directory.Exists(Platform.ConfigCSVBytes))
            {
                UnityEngine.Debug.Log("目录不存在"+Platform.ConfigCSVBytes);
                return;
            }
            List<string> m_assetList = new List<string>();
            GetAssetsRecursively(Platform.ConfigCSVBytes,"*.csv",ref m_assetList);
            AssetImporter importer = null;
            foreach (string s in m_assetList)
            {
                importer = AssetImporter.GetAtPath(s);
                if (importer.assetBundleName == null || importer.assetBundleName != config.ConfigBundleName)
                {
                    importer.assetBundleName = config.ConfigBundleName;
                } 
            }
            
            if (!Directory.Exists(Platform.ConfigXMLBytes))
            {
                UnityEngine.Debug.Log("目录不存在"+Platform.ConfigXMLBytes);
                return;
            }
            m_assetList.Clear();
            GetAssetsRecursively(Platform.ConfigXMLBytes,"*.csv",ref m_assetList);
            importer = null;
            foreach (string s in m_assetList)
            {
                importer = AssetImporter.GetAtPath(s);
                if (importer.assetBundleName == null || importer.assetBundleName != config.ConfigBundleName)
                {
                    importer.assetBundleName = config.ConfigBundleName;
                } 
            }
            
        }
        public static byte[] EncryptBytes(byte[] data, string Skey)  
        {  
            DESCryptoServiceProvider DES = new DESCryptoServiceProvider();  
            DES.Key = ASCIIEncoding.ASCII.GetBytes(Skey);  
            DES.IV = ASCIIEncoding.ASCII.GetBytes(Skey);  
            ICryptoTransform desEncrypt = DES.CreateEncryptor();  
            byte[] result = desEncrypt.TransformFinalBlock(data, 0, data.Length);  
            return result;
        }
        public static string StandardlizePath(string path)
        {
            string pathReplace = path.Replace(@"\", @"/");
            return pathReplace;
        }
        
        [MenuItem("AssetBundle/Build LuaScript Bundle")]
        public static void BuildLuaScript()
        { 
            LuaEditor.EncodeLuaFile();//先加密lua脚本
            LuaEditor.EncryptLuaFile();
            AssetBundlePanelConfig config =  LoadAssetAtPath<AssetBundlePanelConfig>(savePath);
            if (config == null)
            {
                return;
            }
            if (!Directory.Exists(Platform.LuaBytes))
            {
                UnityEngine.Debug.Log("目录不存在"+Platform.LuaBytes);
                return;
            }
            List<string> m_assetList = new List<string>();
            GetAssetsRecursively(Platform.LuaBytes,"*.lua",ref m_assetList);
            AssetImporter importer = null;//import
            foreach (string s in m_assetList)//获取所有的加密后的脚本
            {
                importer = AssetImporter.GetAtPath(s);
                if (importer.assetBundleName == null || importer.assetBundleName != config.LuaScriptBundleName)
                {
                    importer.assetBundleName = config.LuaScriptBundleName;
                } 
            }
        }
        [MenuItem("AssetBundle/Clear Mainifest Files")]
        public static void ClearMainifestHelpFile()
        {
            AssetBundlePanelConfig config =  LoadAssetAtPath<AssetBundlePanelConfig>(savePath);
            if (config == null)
            {
                return;
            }
            var outPath = string.Empty;
            if (config.Windows==1)
            {
                outPath = Platform.AssetBundleExportPath+"windows/";
                ClearMainifest(outPath);
            }
            if (config.Mac == 1)
            {
                outPath = Platform.AssetBundleExportPath+"mac/";
                ClearMainifest(outPath);
            }
            if (config.Android == 1)
            {
                outPath = Platform.AssetBundleExportPath+"android/";
                ClearMainifest(outPath);
            }
            if (config.IOS == 1)
            {
                outPath = Platform.AssetBundleExportPath+"ios/";
                ClearMainifest(outPath);
            }
            AssetDatabase.Refresh();
        }

        public static void ClearMainifest(string outpath)
        {
            if(!Directory.Exists(outpath)) return;
            string[] files = Directory.GetFiles(outpath+"/"+Platform.AssetBundle, "*.mainfest");
            foreach (string file in files)
            {
               File.Delete(file);
            }
        }
        
        [MenuItem("AssetBundle/生成本地md5文件")]
        public static void MakeMD5File()
        {
            AssetBundlePanelConfig config =  LoadAssetAtPath<AssetBundlePanelConfig>(savePath);
            if (config == null)
            {
                return;
            }
            var outPath = string.Empty;
            if (config.Windows==1)
            {
                outPath = Platform.AssetBundleExportPath+"windows/";
                MakeMD5(outPath);
            }
            if (config.Mac == 1)
            {
                outPath = Platform.AssetBundleExportPath+"mac/";
                MakeMD5(outPath);
            }
            if (config.Android == 1)
            {
                outPath = Platform.AssetBundleExportPath+"android/";
                MakeMD5(outPath);
            }
            if (config.IOS == 1)
            {
                outPath = Platform.AssetBundleExportPath+"ios/";
                MakeMD5(outPath);
            }
            AssetDatabase.Refresh();
        }

        public static void MakeMD5(string outpath)
        {
            if(!Directory.Exists(outpath)) return;
            if (File.Exists(outpath + Platform.Md5FileName))
            {
                File.Delete(outpath+Platform.Md5FileName);
            }
            using (FileStream fs = new FileStream(outpath+"/"+Platform.Md5FileName,FileMode.CreateNew))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    string[] files = Directory.GetFiles(outpath+"/"+Platform.AssetBundle, "*" + Platform.AssetBundleExt);
                    foreach (string file in files)
                    {
                        System.IO.FileInfo fileInfo = new System.IO.FileInfo(file);
                        string md5 = MD5Util.ComputeFileHash(file);
                        sw.WriteLine(fileInfo.Name+","+md5+","+fileInfo.Length);
                    }
                }
            }
            
        }
        static void GetAssetsRecursively(string srcFolder, string searchPattern,  ref List<string> assets)
        {
            string searchFolder = StandardlizePath(srcFolder);
            if (!Directory.Exists(searchFolder))
                return;

            string srcDir = searchFolder;
            string[] files = Directory.GetFiles(srcDir, searchPattern);
            foreach (string oneFile in files)
            {
                string srcFile = StandardlizePath(oneFile);
                if (!File.Exists(srcFile))
                    continue;
                assets.Add(srcFile);
            }

            string[] dirs = Directory.GetDirectories(searchFolder);
            foreach (string oneDir in dirs)
            {
                GetAssetsRecursively(oneDir, searchPattern,ref assets);
            }
        }
        
        AssetBundleBuildPanel()
        {
           
        }
        string SelectFolder()
        {
            string datapath = Application.dataPath;
            string selectedpath = EditorUtility.OpenFolderPanel("路径", datapath, "");
            if (!string.IsNullOrEmpty(selectedpath))
            {
                if (selectedpath.StartsWith(datapath))
                {
                    return "Assets/" + selectedpath.Substring(datapath.Length + 1);
                }
                else
                {
                    ShowNotification(new GUIContent("目录要在Assets目录之内"));
                }
            }
            return null;
        }
        static T LoadAssetAtPath<T>(string path) where T : UnityEngine.Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }

        void OnListHeaderGUI(Rect rect)
        {
            EditorGUI.LabelField(rect,"打包搜索目录列表");
        }

        void OnListElementGUI(Rect rect,int index,bool isactive,bool isfocused)
        {
            const float GAP = 5;

            AssetBundleFilter filter = _config.Filters[index];
            rect.y++;

            Rect r = rect;
            r.width = 16;
            r.height = 18;
            filter.vaild = GUI.Toggle(r, filter.vaild, GUIContent.none);

            r.xMin = r.xMax + GAP;
            r.xMax = rect.xMax - 300;
            GUI.enabled = false;
            filter.path = GUI.TextField(r, filter.path);
            GUI.enabled = true;

            r.xMin = r.xMax + GAP;
            r.width = 50;
            if (GUI.Button(r, "选择"))
            {
                var path = SelectFolder();
                if (path != null)
                    filter.path = path;
            }

            r.xMin = r.xMax + GAP;
            r.xMax = rect.xMax;
            filter.filter = GUI.TextField(r, filter.filter);
        }
        void InitConfig()
        {
            _config = AssetDatabase.LoadAssetAtPath<AssetBundlePanelConfig>(savePath);
            if (_config == null)
            {
                _config = CreateInstance<AssetBundlePanelConfig>();
            }
        }


        
        void Add()
        {
            string path = SelectFolder();
            if (!string.IsNullOrEmpty(path))
            {
                var filter = new AssetBundleFilter();
                filter.path = path;
                _config.Filters.Add(filter);
            }
        }
        void InitFilterList()
        {
            _list = new ReorderableList(_config.Filters,typeof(AssetBundleFilter));
            _list.drawElementCallback = OnListElementGUI;
            _list.drawHeaderCallback = OnListHeaderGUI;
            _list.draggable = true;
            _list.elementHeight = 30;
            _list.onAddCallback = (list) => Add();
        }

        void Build()
        {
            Save();
            BuildAssetBundles();
        }

        void Save()
        {
            if (LoadAssetAtPath<AssetBundlePanelConfig>(savePath) == null)
            {
                AssetDatabase.CreateAsset(_config,savePath);
            }
            else
            {
                EditorUtility.SetDirty(_config);
                AssetDatabase.SaveAssets();
            }
        }
        [MenuItem("AssetBundle/生成版本信息")]
        public static void CreateVersionInfo()
        {
            AssetBundlePanelConfig config =  LoadAssetAtPath<AssetBundlePanelConfig>(savePath);
            if (config == null)
            {
                return;
            }
            string outPath = string.Empty;
            if (config.Windows==1)
            {
                outPath = Platform.AssetBundleExportPath+"windows/";
                CreateVersion(outPath, config);
            }
            if (config.Mac == 1)
            {
                outPath = Platform.AssetBundleExportPath+"mac/";
                CreateVersion(outPath, config);
            }
            if (config.Android == 1)
            {
                outPath = Platform.AssetBundleExportPath+"android/";
                CreateVersion(outPath, config);
            }
            if (config.IOS == 1)
            {
                outPath = Platform.AssetBundleExportPath+"ios/";
                CreateVersion(outPath, config);
            }
            AssetDatabase.Refresh();
        }

        public static void CreateVersion(string outpath, AssetBundlePanelConfig config)
        {
            if(!Directory.Exists(outpath)) return;
            if (File.Exists(outpath + Platform.AppVerFileName))
            {
                File.Delete(outpath+Platform.AppVerFileName);
            }
            using (FileStream fs = new FileStream(outpath+"/"+Platform.AppVerFileName,FileMode.CreateNew))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                   sw.Write(config.Version.master+"."+config.Version.minor+"."+config.Version.revised);
                }
            }
            if (File.Exists(outpath + Platform.ResVersionFileName))
            {
                File.Delete(outpath+Platform.ResVersionFileName);
            }
            using (FileStream fs = new FileStream(outpath+"/"+Platform.ResVersionFileName,FileMode.CreateNew))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(config.Version.resversion);
                }
            }
        }
        void OnGUI()
        {
            if (_config == null)
            {
                InitConfig();
            }
            if (_list == null)
            {
                InitFilterList();
            }
            GUILayout.BeginVertical();
            {
                
         
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("添加", GUILayout.MinHeight(30), GUILayout.MaxWidth(200)))
                {
                    Add();
                }
                GUILayout.Space(100);
                if (GUILayout.Button("保存", GUILayout.MinHeight(30), GUILayout.MaxWidth(200)))
                {
                    Save();
                }
                GUILayout.Space(100);
                if (GUILayout.Button("构建", GUILayout.MinHeight(30), GUILayout.MaxWidth(200)))
                {
                    Build();
                }
                GUILayout.Space(100);
                if (GUILayout.Button("生成md5文件", GUILayout.MinHeight(30), GUILayout.MaxWidth(200)))
                {
                    MakeMD5File();
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(20);
            GUILayout.BeginVertical();
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(50);
                    EditorGUILayout.LabelField("Congif打包是否加密");
                    GUILayout.Space(100);
                    _config.EncryptConfig =
                        (AssetBundlePanelConfig.EncryptConfigBundle) EditorGUILayout.EnumPopup(_config.EncryptConfig);
                }
                GUILayout.EndHorizontal();
                
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(50);
                    EditorGUILayout.LabelField("Config BundleName");
                    _config.ConfigBundleName = GUILayout.TextArea(_config.ConfigBundleName);
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(20);
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(50);
                    EditorGUILayout.LabelField("LuaScript打包是否加密");
                    GUILayout.Space(100);
                    _config.EncryptLuaScript =
                        (AssetBundlePanelConfig.EncryptLuaScriptBundle) EditorGUILayout.EnumPopup(_config.EncryptLuaScript);
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(50);
                    EditorGUILayout.LabelField("LuaScript BundleName");
                    _config.LuaScriptBundleName = GUILayout.TextArea(_config.LuaScriptBundleName);
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(20);
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(50);
                    _config.IsSpriteTag = EditorGUILayout.Toggle("是否启用SpriteTag", _config.IsSpriteTag == 1) ? 1 : 0;
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(20);
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(20);
                    _config.Windows = EditorGUILayout.Toggle("打包Window平台", _config.Windows == 1) ? 1 : 0;
                    GUILayout.Space(20);
                    _config.Mac = EditorGUILayout.Toggle("打包Mac平台", _config.Mac == 1) ? 1 : 0;
                    GUILayout.Space(20);
                    _config.Android = EditorGUILayout.Toggle("打包Android平台", _config.Android == 1) ? 1 : 0;
                    GUILayout.Space(20);
                    _config.IOS = EditorGUILayout.Toggle("打包IOS平台", _config.IOS == 1) ? 1 : 0;
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(20);
                GUILayout.BeginVertical();
                {
                    //version
                    GUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("主版本号");
                        _config.Version.master = Convert.ToInt32(GUILayout.TextArea(_config.Version.master.ToString(),20));
 
                    }GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("次版本号");
                        _config.Version.minor = Convert.ToInt32(GUILayout.TextArea(_config.Version.minor.ToString()));
                   
                    }GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("修订版本号");
                        _config.Version.revised = Convert.ToInt32(GUILayout.TextArea(_config.Version.revised.ToString()));
                   
                    }GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("资源版本号");
                        _config.Version.resversion = Convert.ToInt32(GUILayout.TextArea(_config.Version.resversion.ToString()));
           
                    }GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("生成版本信息");
                        if (GUILayout.Button("生成", GUILayout.MinHeight(30), GUILayout.MaxWidth(200)))
                        {
                            CreateVersionInfo();
                        }
                    }GUILayout.EndHorizontal();
                }
                //版本信息
                GUILayout.EndVertical();
                scrollPos = GUILayout.BeginScrollView(scrollPos);
                {
                    _list.DoLayoutList();
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndVertical();
            if (GUI.changed)
            {
                EditorUtility.SetDirty(_config);
                AssetDatabase.SaveAssets();
            }
            }
            GUILayout.EndHorizontal();
        }
    }
}