using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Assets.GameFrame.Language;
using GameFrame;
using UnityEditor;
using UnityEngine;

namespace Assets.GameFrame.Editor
{
    class LanguageEditor
    {
        static private string saveDir = "Assets/Resources/";
        static private string exp = ".asset";
        [MenuItem("Language/CreateLanguageConfig")]
        public static void CreateLanguageCongif()
        {
            UnityEngine.Object asset = CreateAsset<LanguageConfigs>("LanguageConfig");
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }
        public static T CreateAsset<T>(string name) where T : ScriptableObject
        {
            T asset = ScriptableObject.CreateInstance<T>();
            string actives = AssetDatabase.GetAssetPath(Selection.activeObject);
            string path = string.IsNullOrEmpty(actives) ? saveDir : actives;
            if (Path.GetExtension(path) != "") path = path.Replace(Path.GetFileName(path), "");
            path = path + name + exp;
            if (FileManager.IsFileExist(path))
            {
                FileManager.DeleteFile(path);
            }
            SaveAssetTo(asset, path);
            return asset;
        }
        public static void SaveAssetTo(UnityEngine.Object asset, string path)
        {
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
