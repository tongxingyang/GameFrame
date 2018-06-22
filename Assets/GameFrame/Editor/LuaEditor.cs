using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
namespace GameFrame.Editor
{
//    [InitializeOnLoad]
    public static  class LuaEditor
    {
        [MenuItem("LuaTools/Encode LuaFile with UTF-8",false)]
        public static void EncodeLuaFile()
        {
            return;
            string path = LuaConst.luaDir;//lua文件夹
            string[] files = Directory.GetFiles(path, "*.lua", SearchOption.AllDirectories);
            foreach (string f in files)
            {
                string file = f.Replace('\\', '/');
                string content = File.ReadAllText(file);
                using (var sw = new StreamWriter(file,false,Encoding.UTF8))
                {
                    sw.Write(content);
                }
            }
        }

        public static bool IsEncryptBatch = true;
        public static string SrcLuaPath = LuaConst.luaDir;
        public static  string DesLuaPath = LuaConst.luaTempDir;

        

        [MenuItem("LuaTools/Encrypt(RC4) LuaFile",false)]
        public static void EncryptLuaFile()
        {
            EncryptOperate(SrcLuaPath,DesLuaPath);
            AssetDatabase.Refresh();
        }

        [MenuItem("LuaTools/Decrypt(RC4) LuaFile",false)]
        public static void DecryptLuaFile()
        {
            DecryptOperate(SrcLuaPath,DesLuaPath);
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
                UnityEngine.Debug.LogError("加密文件出错");
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
                UnityEngine.Debug.LogError("加密文件夹出错");
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
                        retbyte = LuaEncrype.Encrypt(srcbyte);
                    }
                    else
                    {
                        retbyte = LuaEncrype.Decrypt(srcbyte);
                    }
                    string despath = des.FullName + "//" + src.Name;
                    File.WriteAllBytes(despath,retbyte);
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("加密文件出错");
            }
        }
        

        
        [MenuItem("LuaTools/Encode and Encrypt Lua(.bytes)",false)]
        public static void HandleLuaBundle()
        {
            EncodeLuaFile();
            string tempLuaDir = LuaConst.luaTempDir;
            if (Directory.Exists(tempLuaDir))
            {
                Directory.Delete(LuaConst.luaTempDir);
            }
            Directory.CreateDirectory(tempLuaDir);
            string sourceDir = LuaConst.luaDir;
            string[] files = Directory.GetFiles(sourceDir, "*.lua", SearchOption.AllDirectories);
            int len = sourceDir.Length;

            if (sourceDir[len - 1] == '/' || sourceDir[len - 1] == '\\')
            {
                --len;
            }
            for (int j = 0; j < files.Length; j++)
            {
                string str = files[j].Remove(0, len);
                string dest = tempLuaDir + str + ".bytes";
                string dir = Path.GetDirectoryName(dest);
                Directory.CreateDirectory(dir);
                byte[] srcbyte = File.ReadAllBytes(files[j]);
                byte[] enbyte = LuaEncrype.Encrypt(srcbyte);
                File.WriteAllBytes(dest,enbyte);
            }
            tempLuaDir = LuaConst.toluaTempDir;
            if (!Directory.Exists(tempLuaDir))
                Directory.CreateDirectory(tempLuaDir);
            sourceDir = LuaConst.toluaDir;
            files = Directory.GetFiles(sourceDir, "*.lua", SearchOption.AllDirectories);
            len = sourceDir.Length;

            if (sourceDir[len - 1] == '/' || sourceDir[len - 1] == '\\')
            {
                --len;
            }
            for (int j = 0; j < files.Length; j++)
            {
                string str = files[j].Remove(0, len);
                string dest = tempLuaDir + str + ".bytes";
                string dir = Path.GetDirectoryName(dest);
                Directory.CreateDirectory(dir);
                byte[] srcbyte = File.ReadAllBytes(files[j]);
                File.WriteAllBytes(dest,srcbyte);
            }
            AssetDatabase.Refresh();
        }
    }
}