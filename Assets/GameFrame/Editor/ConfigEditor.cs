using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace GameFrame.Editor
{
    public class ConfigEditor
    {
        public static bool IsEncryptBatch = true;
        public static string SrcConfigPath = ConfigConst.configDir;
        public static string DesConfigPath = ConfigConst.tempconfigDir;

        
//        [MenuItem("ConfigTools/Encrypt(RC4) ConfigFile",false)]
        public static void EncryptConfigFile()
        {
            EncryptOperate(SrcConfigPath,DesConfigPath);
            AssetDatabase.Refresh();
        }
        

//        [MenuItem("ConfigTools/Decrypt(RC4) ConfigFile",false)]
        public static void DecryptConfigFile(string src ,string dec)
        {
            DecryptOperate(SrcConfigPath,DesConfigPath);
            AssetDatabase.Refresh();
        }

        
        public static void EncryptOperate(string src,string des)
        {
            IsEncryptBatch = true;
            EnOrDeOperate(src,des);
        }

        public static void DecryptOperate(string src, string des)
        {
            IsEncryptBatch = false;      
            EnOrDeOperate(src,des);
        }

        public static void EnOrDeOperate(string src,string des)
        {
            try
            {
                if (!Directory.Exists(src))
                {
                    UnityEngine.Debug.LogError("加密原文件夹不存在");
                    return;
                }
                DirectoryInfo srcDirectoryInfo = new DirectoryInfo(src);
                if (Directory.Exists(des))
                {
                    Directory.Delete(des,true);
                }
                Directory.CreateDirectory(des);
                DirectoryInfo desDirectoryInfo = new DirectoryInfo(des);
                EncryptDir(srcDirectoryInfo,desDirectoryInfo);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("加密文件出错"+e.Message);
            }
        }

        public static void EncryptDir(DirectoryInfo src, DirectoryInfo des)
        {
            try
            {
                foreach (System.IO.FileInfo fileInfo in src.GetFiles())
                {
                    EncryptFile(fileInfo,des);
                }
                foreach (DirectoryInfo directoryInfo in src.GetDirectories())
                {
                    string subdespath = des.FullName + "//" + directoryInfo.Name;
                    if (!Directory.Exists(subdespath))
                    {
                        Directory.CreateDirectory(subdespath);
                    }
                    DirectoryInfo desDirectoryInfo = new DirectoryInfo(subdespath);
                    EncryptDir(directoryInfo,desDirectoryInfo);
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("加密文件夹出错"+e.Message);
            }
        }

        public static void EncryptFile(System.IO.FileInfo src,DirectoryInfo des)
        {
            try
            {
                if (File.Exists(src.FullName))
                {
                    byte[] srcbyte = File.ReadAllBytes(src.FullName);
                    byte[] retbyte;
                    if (IsEncryptBatch)
                    {
                        retbyte = ConfigEncrypt.Encrypt(srcbyte);
                    }
                    else
                    {
                        retbyte = ConfigEncrypt.Decrypt(srcbyte);
                    }
                    string despath = des.FullName + "//" + src.Name;
                    File.WriteAllBytes(despath,retbyte);
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("加密文件出错"+e.Message);
            }
        }
        
      

        public static void HandleConfigBundle()
        {
            string tempLuaDir = ConfigConst.tempconfigDir;
            if (Directory.Exists(tempLuaDir))
            {
                Directory.Delete(ConfigConst.tempconfigDir);
            }
            Directory.CreateDirectory(tempLuaDir);
            string sourceDir = ConfigConst.configDir;
            string[] files = Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories);
            int len = sourceDir.Length;

            if (sourceDir[len - 1] == '/' || sourceDir[len - 1] == '\\')
            {
                --len;
            }
            for (int j = 0; j < files.Length; j++)
            {
                if(files[j].Contains("DS_Store"))continue;
                if(files[j].Contains("meta"))continue;
                string str = files[j].Remove(0, len);
                string dest = tempLuaDir + str;
                string dir = Path.GetDirectoryName(dest);
                Directory.CreateDirectory(dir);
                byte[] srcbyte = System.Text.Encoding.UTF8.GetBytes(File.ReadAllText(files[j]));
                byte[] enbyte = ConfigEncrypt.Encrypt(srcbyte);
                File.WriteAllBytes(dest,enbyte);
            }
            AssetDatabase.Refresh();
        }
    }
}