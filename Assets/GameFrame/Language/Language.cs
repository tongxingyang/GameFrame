using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GameFrame.Language
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
        Resource_CheckSDK = 2,
        Resource_Unzip = 3,
        Resource_CheckVersion = 4,
        Resource_DownloadNewApp = 5,
        Resource_CheckResource = 6,
        Resource_UpdateResource = 7,
        Resource_StartGame = 8,
        Resource_OffLine = 9,
        DownloadSizeAlert = 10,
        Resource_Notice = 11,
        Resource_ServerError = 12,
        Resource_UpdateDone = 13,
        Resource_DownLoadError = 14,
    }
}
