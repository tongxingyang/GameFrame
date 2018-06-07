using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace GameFrame.Editor
{
    public class AssetBundlePanelConfig : ScriptableObject
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
    }

    [System.Serializable]
    public class AssetBundleFilter
    {
        public bool vaild = true;
        public string path = String.Empty;
        public string filter = String.Empty;
    }
    
    public class AssetBundleBuildPanel:EditorWindow
    {
        private const string savePath = "Assets/GameFrame/configasset.asset";
        private AssetBundlePanelConfig _config;
        private ReorderableList _list;
        private Vector2 scrollPos = Vector2.zero;
        private static AssetBundleBuildPanel panel;
        
        static string[] filterDirList = new string[]{};
        static List<string> filterExts = new List<string>{".cs", ".js"};
        static List<string> imageExts = new List<string>{".png", ".jpg", ".jpeg", ".bmp", "gif", ".tga", ".tiff", ".psd"};
        public static List<string> exts = new List<string>(new string[]{ ".prefab", ".png", ".jpg", ".jpeg", ".bmp", "gif", ".tga", ".tiff", ".psd", ".mat", ".mp3", ".ogg",".wav" , ".shader", ".ttf"});
        
        [MenuItem("AssetBundle/AssetBundlePanel")]
        static void Open()
        {
            panel = GetWindow<AssetBundleBuildPanel>(true,"AssetBundlePanel");
            panel.minSize = new Vector2(800,600);
            panel.Show();
        }
        [MenuItem("AssetBundle/Set AssetBundle Name")]
        public static void SetAssetBundleName(AssetBundlePanelConfig config)
        {
            List<string> list = new List<string>();
            for (int i = 0; i < config.Filters.Count ; i++)
            {
                list.Clear();
                PathResolver.RecursiveFile(config.Filters[i].path,list,exts);
                if (list.Count==0)
                {
                    continue;
                }
                Dictionary<string, AssetNode> nodeDict = AssetNodeUtil.GenerateAllNode(list, filterDirList, filterExts,
                    imageExts, config.IsSpriteTag == 1, BuildSetting.AssetBundleExt);
                AssetNodeUtil.GenerateNodeDependencies(nodeDict);
                List<AssetNode> roots = AssetNodeUtil.FindRoots(nodeDict);
                AssetNodeUtil.RemoveParentShare(roots);
                AssetNodeUtil.MergeParentCountOnce(roots);
                Dictionary<string, AssetNode> assetDict = AssetNodeUtil.GenerateAssetBundleNodes(roots);
                AssetNodeUtil.SetAssetBundleNames(assetDict,config.Filters[i].path,BuildSetting.AssetBundleExt);
            }
            AssetDatabase.RemoveUnusedAssetBundleNames();
        }
        [MenuItem("AssetBundle/Clear AssetBundle Name")]
        public static void ClearAssetBundleNames()
        {
            string[] names = AssetDatabase.GetAllAssetBundleNames();

            int count = names.Length;
            for(int i = 0; i < count; i ++)
            {
                if (names[i].IndexOf(BuildSetting.AssetBundleExt) != -1)
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
            SetAssetBundleName(config);
            var outPath = BuildSetting.AssetBundleExportPath+BuildSetting.AssetBundle;
            //打包资源
            if (config.EncryptConfig == AssetBundlePanelConfig.EncryptConfigBundle.Encrypt)
            {
                //加密luabundle包
                string configfilename = outPath + "/" + config.ConfigBundleName;
                if(!File.Exists(configfilename)) return;
                byte[] bytes = File.ReadAllBytes(configfilename);
                bytes = EncryptBytes(bytes, BuildSetting.ConfigBundleKey);
                if(!File.Exists(configfilename)) return;
                File.Delete(configfilename);
                File.WriteAllBytes(configfilename,bytes);
            }
            if (config.EncryptLuaScript == AssetBundlePanelConfig.EncryptLuaScriptBundle.Encrypt)
            {
                //加密configbundle包
                string configfilename = outPath + "/" + config.LuaScriptBundleName;
                if(!File.Exists(configfilename)) return;
                byte[] bytes = File.ReadAllBytes(configfilename);
                bytes = EncryptBytes(bytes, BuildSetting.LuaBundleKey);
                if(!File.Exists(configfilename)) return;
                File.Delete(configfilename);
                File.WriteAllBytes(configfilename,bytes);
            }
            AssetDatabase.Refresh();
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
            if (!Directory.Exists(BuildSetting.ConfigCSVBytes))
            {
                UnityEngine.Debug.Log("目录不存在"+BuildSetting.ConfigCSVBytes);
                return;
            }
            List<string> m_assetList = new List<string>();
            GetAssetsRecursively(BuildSetting.ConfigCSVBytes,"*.csv",ref m_assetList);
            AssetImporter importer = null;
            foreach (string s in m_assetList)
            {
                importer = AssetImporter.GetAtPath(s);
                if (importer.assetBundleName == null || importer.assetBundleName != config.ConfigBundleName)
                {
                    importer.assetBundleName = config.ConfigBundleName;
                } 
            }
            
            /////
            
            if (!Directory.Exists(BuildSetting.ConfigXMLBytes))
            {
                UnityEngine.Debug.Log("目录不存在"+BuildSetting.ConfigXMLBytes);
                return;
            }
            m_assetList.Clear();
            GetAssetsRecursively(BuildSetting.ConfigXMLBytes,"*.csv",ref m_assetList);
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
            if (!Directory.Exists(BuildSetting.LuaBytes))
            {
                UnityEngine.Debug.Log("目录不存在"+BuildSetting.LuaBytes);
                return;
            }
            List<string> m_assetList = new List<string>();
            GetAssetsRecursively(BuildSetting.LuaBytes,"*.lua",ref m_assetList);
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
            _config = LoadAssetAtPath<AssetBundlePanelConfig>(savePath);
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
            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(100);
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
            }
        }
    }
}