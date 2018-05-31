using System.IO;
using UnityEditor;
using UnityEngine;

namespace GameFrame.GameConfig
{
    public class GameConfig
    {
        public  string UserId
        {
            get
            {
                return "00001";
            }
        }
        public  bool DevelopMode = true;
        public  bool UpdateMode = false;
        public  string AppName = "";
        public  string AppPrefix = "";
        public  string ReleaseUrl = "";
        public  string DevelopUrl = "";
        public  string Version = "0.0.0";
        public  bool IsCache = false;
        public  bool ForceAsyncLoad = false;
        private static GameConfig m_instance;

        public static GameConfig Gamedata
        {
            get
            {
                m_instance = Load();
                return m_instance;
            }
        }

        public static GameConfig Load()
        {
            string filepath = Application.streamingAssetsPath + "/" + AssetManager.FileNameConfig.GameConst;
            var fileinfo = new System.IO.FileInfo(filepath);
            if (fileinfo.Exists)
            {
                using (var sr = fileinfo.OpenText())
                {
                    var stringvalue = sr.ReadToEnd();
                    GameConfig gameConfig = JsonUtility.FromJson<GameConfig>(stringvalue);
                    return gameConfig;
                }
            }
            return new GameConfig();
        }

        public static void Save()
        {
            string filepath = Application.streamingAssetsPath + "/" + AssetManager.FileNameConfig.GameConst;
            string str = JsonUtility.ToJson(Gamedata, true);
            if (!Directory.Exists(filepath))
            {
                Directory.CreateDirectory(filepath);
            }
            if (File.Exists(filepath)) File.Delete(filepath);
            using (var fs = new FileStream(filepath, FileMode.CreateNew))
            {
                using (var sw = new StreamWriter(fs))
                {
                    sw.Write(str);
                }
            }
            AssetDatabase.Refresh();
            UnityEngine.Debug.Log("保存设置成功");
        }
        
    }
    
}