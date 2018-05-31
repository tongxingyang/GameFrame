using GameFrame.GameConfig;
using UnityEngine;

namespace GameFrame
{
    /// <summary>
    /// 封装本地数据的接口函数  常量key定义在Key那个脚本中
    /// </summary>
    public class PlayerPrefsUtil
    {
        public static bool UseUserId = true;//是否启用用户id

        public static string GetKey(string key)
        {
            return GetKey(key, true);
        }

        public static string GetKey(string key, bool isbinduserid)
        {
            if (UseUserId && isbinduserid)
            {
                return GameConfig.GameConfig.Gamedata.AppPrefix + GameConfig.GameConfig.Gamedata.UserId + "_" + key;
            }
            else
            {
                return GameConfig.GameConfig.Gamedata.AppPrefix + "_" + key;
            }
        }

        public static bool HasKey(string key)
        {
            return HasKey(key, true);
        }

        public static bool HasKey(string key, bool isbinduseid)
        {
            string name = GetKey(key, isbinduseid);
            return PlayerPrefs.HasKey(key);
        }

        public static bool HasBool(string key)
        {
            return HasBool(key, true);
        }

        public static bool HasBool(string key, bool isbinduseid)
        {
            string name = GetKey(key, isbinduseid);
            return PlayerPrefs.GetInt(name) == 1;
        }

        public static void SetBool(string key, bool value, bool isbinduseid)
        {
            string name = GetKey(key, isbinduseid);
            PlayerPrefs.DeleteKey(name);
            PlayerPrefs.SetInt(name, value ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static int GetInt(string key,bool isbinduseid)
        {
            string name = GetKey(key, isbinduseid);
            return PlayerPrefs.GetInt(key);
        }

        public static void SetInt(string key,bool isbinduseid,int value)
        {
            string name = GetKey(key, isbinduseid);
            PlayerPrefs.DeleteKey(name);
            PlayerPrefs.SetInt(name,value);
            PlayerPrefs.Save();
        }

        public static int GetIntSimple(string key)
        {
            return PlayerPrefs.GetInt(key);
        }

        public static void SetIntSimple(string key, int value)
        {
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.SetInt(key,value);
            PlayerPrefs.Save();
        }
        public static float GetFloat(string key,bool isbinduseid)
        {
            string name = GetKey(key, isbinduseid);
            return PlayerPrefs.GetFloat(key);
        }

        public static void SetFloat(string key,bool isbinduseid,float value)
        {
            string name = GetKey(key, isbinduseid);
            PlayerPrefs.DeleteKey(name);
            PlayerPrefs.SetFloat(name,value);
            PlayerPrefs.Save();
        }

        public static float GetFloatSimple(string key)
        {
            return PlayerPrefs.GetFloat(key);
        }

        public static void SetFloatSimple(string key, float value)
        {
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.SetFloat(key,value);
            PlayerPrefs.Save();
        }
        public static string GetString(string key,bool isbinduseid)
        {
            string name = GetKey(key, isbinduseid);
            return PlayerPrefs.GetString(key);
        }

        public static void SetString(string key,bool isbinduseid,string value)
        {
            string name = GetKey(key, isbinduseid);
            PlayerPrefs.DeleteKey(name);
            PlayerPrefs.SetString(name,value);
            PlayerPrefs.Save();
        }

        public static string GetStringSimple(string key)
        {
            return PlayerPrefs.GetString(key);
        }

        public static void SetStringSimple(string key, string value)
        {
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.SetString(key,value);
            PlayerPrefs.Save();
        }

        public static void DeleteAll()
        {
            PlayerPrefs.DeleteAll();
        }

        public static void DeleteKey(string key ,bool isbinduseid)
        {
            string name = GetKey(key, isbinduseid);
            PlayerPrefs.DeleteKey(name);
        }
    }
}