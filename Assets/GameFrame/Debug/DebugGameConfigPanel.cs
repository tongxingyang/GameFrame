using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace GameFrame.AssetManager
{
    public class DebugGameConfigPanel:MonoBehaviour
    {
        public List<DebugGameConfigItem> items = new List<DebugGameConfigItem>();
        
        public void Close()
        {
            gameObject.SetActive(false);
        }

        public void Show()
        {
            gameObject.SetActive(true);
            Load();
        }
        void OnEnable()
        {
            Load();
        }

        public void Load()
        {
            for (int i = 0; i < items.Count; i++)
            {
                SetValue(items[i]);
            }
        }

        public void SetValue(DebugGameConfigItem item)
        {
            if (item.key == "DevelopMode")
            {
                item.SetBool(GameConfig.GameConfig.Gamedata.DevelopMode);
            }
            else if(item.key == "UpdateMode")
            {
                 item.SetBool(GameConfig.GameConfig.Gamedata.UpdateMode);   
            }else if(item.key == "AppName")
            {
                item.SetString(GameConfig.GameConfig.Gamedata.AppName);
            }else if(item.key == "AppPrefix")
            {
                item.SetString(GameConfig.GameConfig.Gamedata.AppPrefix);
            }else if(item.key == "ReleaseUrl")
            {
                item.SetString(GameConfig.GameConfig.Gamedata.ReleaseUrl);
                
            }else if(item.key == "DevelopUrl")
            {
                item.SetString(GameConfig.GameConfig.Gamedata.DevelopUrl);
            }else if(item.key == "Version")
            {
                item.SetString(GameConfig.GameConfig.Gamedata.Version);
            }else if(item.key == "IsCache")
            {
                item.SetBool(GameConfig.GameConfig.Gamedata.IsCache);
            }else if(item.key == "ForceAsyncLoad")
            {
                item.SetBool(GameConfig.GameConfig.Gamedata.ForceAsyncLoad);
            }
        }

        public void Save()
        {
            for (int i = 0; i < items.Count; i++)
            {
                SaveValue(items[i]);
            }
            GameConfig.GameConfig.Save();
        }
        
        public void SaveValue(DebugGameConfigItem item)
        {
            if (item.key == "DevelopMode")
            {
                GameConfig.GameConfig.Gamedata.DevelopMode = item.GetBool();
            }
            else if(item.key == "UpdateMode")
            {
                GameConfig.GameConfig.Gamedata.UpdateMode = item.GetBool();
            }else if(item.key == "AppName")
            {
                GameConfig.GameConfig.Gamedata.AppName = item.GetString();
            }else if(item.key == "AppPrefix")
            {
                GameConfig.GameConfig.Gamedata.AppPrefix = item.GetString();
            }else if(item.key == "ReleaseUrl")
            {
                GameConfig.GameConfig.Gamedata.ReleaseUrl = item.GetString();

            }else if(item.key == "DevelopUrl")
            {
                GameConfig.GameConfig.Gamedata.DevelopUrl = item.GetString();
            }else if(item.key == "Version")
            {
                GameConfig.GameConfig.Gamedata.Version = item.GetString();
            }else if(item.key == "IsCache")
            {
                GameConfig.GameConfig.Gamedata.IsCache = item.GetBool();
            }else if(item.key == "ForceAsyncLoad")
            {
                GameConfig.GameConfig.Gamedata.ForceAsyncLoad = item.GetBool();
            }
        }
    }
}