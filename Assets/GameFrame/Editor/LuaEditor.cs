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
            string path = Platform.Lua;
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
        public static string SrcLuaPath = Platform.Lua;
        public static  string DesLuaPath = Platform.LuaBytes;
        private const int KeyCount = 256;
        private static int[] sbox;
        private const string KeyString = "GameFrame";
        

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
                        retbyte = Encrypt(srcbyte);
                    }
                    else
                    {
                        retbyte = Decrypt(srcbyte);
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
        
        public static void  InitKey()
        {
            sbox = new int[KeyCount];
            int b = 0;
            int[] key = new int[KeyCount];
            int n = KeyString.Length; //7
            for (int a = 0; a < KeyCount; a++)
            {
                key[a] = (int)KeyString[a % n];
                sbox[a] = a;
            }

            for (int a = 0; a < KeyCount; a++)
            {
                b = (b + sbox[a] + key[a]) % KeyCount;
                int tempSwap = sbox[a];
                sbox[a] = sbox[b];
                sbox[b] = tempSwap;
            }
        }
        public static byte[] Encrypt(byte[] src)
        {
            InitKey();
            int i = 0, j = 0, k  = 0;
            for (int a = 0; a < src.Length; a++)
            {
                i = (i + 1) % KeyCount;
                j = (j + sbox[i]) % KeyCount;
                int tempSwap = sbox[i];
                sbox[i] = sbox[j];
                sbox[j] = tempSwap;

                k = sbox[(sbox[i] + sbox[j]) % KeyCount];
                int cipherBy = ((int)src[a]) ^ k;  
                src[a] = Convert.ToByte(cipherBy);
            }
            return src;
        }
        public static byte[] Decrypt(byte[] src)
        {
            return Encrypt(src);
        }
    }
}