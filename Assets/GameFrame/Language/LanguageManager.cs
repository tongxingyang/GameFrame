using System;
using System.Collections.Generic;
using GameFrame;
using GameFrameDebuger;
using UnityEngine;

namespace GameFrame.Language
{
    class LanguageManager:Singleton<LanguageManager>
    {
        public delegate void RegisterLanguageEvent();

        public RegisterLanguageEvent LanguageEvent;
        
        private Language language;
        private LanguageConfigs curConfigs;
        private LanguageConfig curConfig;
        private Dictionary<LanguageId, string> languageDic = new Dictionary<LanguageId, string>();

        
        // 1 step
        public void LoadLanguageConfig()
        {
            curConfigs = Resources.Load<LanguageConfigs>("LanguageConfig");
           
        }
        // 2 step
        public void SetCurrentLanguage(Language l)
        {
            language = l;
            LoadLanguage();
            if (LanguageEvent != null)
            {
                LanguageEvent();
            }
        }
        
        private void LoadLanguage()
        {
            curConfig = Array.Find(curConfigs.tpls, it => it.language == language);
            if (curConfig == null)
            {
                Debuger.Log("找不到配置文件" + language);
            }
            languageDic = new Dictionary<LanguageId, string>();
            foreach (var item in curConfig.datas)
            {
                languageDic.Add(item.id, item.value);
            }
        }
        

        public string GetLanguage(LanguageId key)
        {
            if (languageDic.Count == 0) return String.Empty;
            if (!string.IsNullOrEmpty(languageDic[key]))
            {
                return languageDic[key];
            }
            Debuger.LogError("language配置中未发现数据 "+key);
            return "";
        }

    }
}
