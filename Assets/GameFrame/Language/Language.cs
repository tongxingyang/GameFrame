using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.GameFrame.Language
{
    public class LanguageConfigs : ScriptableObject
    {
        public LanguageConfig[] tpls;
    }

    [System.Serializable]
    public class LanguageConfig
    {
        public Language language;
        public LanguageData[] datas;
        public string[] fonts;
    }

    [System.Serializable]
    public class LanguageData
    {
        public LanguageId id;
        public string value;
    }
    public enum Language
    {
        ZH_CN = 1,
        ENG = 2,
    }
    public enum LanguageId
    {
        Resource_InitSDK = 1,
        Resource_Unzip = 2,
        Resource_CheckVersion = 3,
        Resource_DownloadNewApp = 4,
        Resource_CheckResource = 5,
        Resource_UpdateResource = 6,
        Resource_StartGame = 7,
        Resource_OffLine = 8,
        DownloadSizeAlert = 9,
        Resource_Notice = 10,
        Resource_ServerError = 11,
    }
}
