using System;
using System.Collections.Generic;
using GameFrame;
using GameFrameDebuger;
using UnityEngine;

namespace Assets.GameFrame.Language
{
    class LanguageManager:Singleton<LanguageManager>
    {
        private Language language;
        private LanguageConfig curConfig;
        private Dictionary<LanguageId, string> languageDic = new Dictionary<LanguageId, string>();

        public void SetCurrentLanguage(Language l)
        {
            language = l;
        }
        public void LoadLanguageConfig()
        {
            LanguageConfigs languageConfigs = Resources.Load<LanguageConfigs>("LanguageConfig");
            curConfig = Array.Find(languageConfigs.tpls, it => it.language == language);
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
            if (!string.IsNullOrEmpty(languageDic[key]))
            {
                return languageDic[key];
            }
            Debuger.LogError("language配置中未发现数据 "+key);
            return "";
        }

    }
}
