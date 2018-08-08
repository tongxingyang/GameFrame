using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using GameFrame;
using GameFrameDebuger;
using UnityEngine;
using Path = System.IO.Path;

namespace GameFrame
{
    public class FileManager
    {
        public delegate void DelefateOnFileOperateFileFail(string fullpath, enFileOperation enFileOperation,
            Exception exception);
        /// <summary>
        /// 缓存路径
        /// </summary>
        private static string m_cachePath = null;

        public static string m_extractFolder = "Resources";
        private static string m_extractPath = null;
        private static MD5CryptoServiceProvider m_md5Provider = new MD5CryptoServiceProvider();
        public static DelefateOnFileOperateFileFail m_delefateOnFileOperateFileFail = delegate { };

        public static bool IsFileExist(string filePath)
        {
            return File.Exists(filePath);
        }

        public static bool IsDirectoryExist(string directory)
        {
            return Directory.Exists(directory);
        }

        public static bool CreateDirectory(string directory)
        {
            bool result = false;
            if (Directory.Exists(directory))
            {
                result = true;
            }
            else
            {
                try
                {
                    Directory.CreateDirectory(directory);
                    result = true;
                }
                catch (Exception e)
                {
                    result = false;
                    m_delefateOnFileOperateFileFail(directory, enFileOperation.CreateDirectory, e);
                }
            }
            return result;
        }

        public static bool DeleteDirectory(string directory)
        {
            bool result = false;
            if (IsDirectoryExist(directory))
            {
                try
                {
                    Directory.Delete(directory);
                    result = true;
                }
                catch (Exception e)
                {
                    m_delefateOnFileOperateFileFail(directory, enFileOperation.DeleteDirectory, e);
                    result = false;
                }
            }
            else
            {
                result = true;
            }
            return result;
        }

        public static int GetFileLength(string filePath)
        {
            int result = 0;
            if (IsFileExist(filePath))
            {
                try
                {
                    System.IO.FileInfo fileInfo = new  System.IO.FileInfo(filePath);
                    result = (int)fileInfo.Length;
                }
                catch (Exception e)
                {
                    Debuger.LogError("Error GetFileLength");
                }
            }
            else
            {
                result = 0;
            }
            return result;
        }

        public static byte[] ReadFile(string filePath)
        {
            byte[] result = null;
            if (IsFileExist(filePath))
            {
                try
                {
                    result = File.ReadAllBytes(filePath);
                }
                catch (Exception e)
                {
                    Debuger.LogError("Error ReadFile");
                }
            }
            return result;
        }

        public static void DeleteFile(string filePath)
        {
            if (IsFileExist(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (Exception e)
                {
                    m_delefateOnFileOperateFileFail(filePath, enFileOperation.DeleteFile, e);
                }
            }
        }
        public static bool WriteFile(string filePath, byte[] data)
        {
            bool result = false;
            try
            {
                File.WriteAllBytes(filePath, data);
                result = true;
            }
            catch (Exception e)
            {
                DeleteFile(filePath);
                m_delefateOnFileOperateFileFail(filePath, enFileOperation.WriteFile, e);
            }
            return result;
        }

        public static bool WriteFile(string filePath, byte[] data, int offect, int length)
        {
            bool result = false;
            FileStream fileStream = null;
            try
            {
                fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
                fileStream.Write(data, offect, length);
                fileStream.Close();
                result = true;
            }
            catch (Exception e)
            {
                if (fileStream != null)
                {
                    fileStream.Close();
                }
                DeleteFile(filePath);
                m_delefateOnFileOperateFileFail(filePath, enFileOperation.WriteFile, e);
            }
            return result;
        }

        public static void CopyFile(string srcFile, string desFile)
        {
            File.Copy(srcFile, desFile);
        }

        public static string GetFileMD5(string filePath)
        {
            if (IsFileExist(filePath))
            {
                return BitConverter.ToString(m_md5Provider.ComputeHash(ReadFile(filePath))).Replace("-", string.Empty);
            }
            return string.Empty;
        }

        public static string GetMD5(byte[] data)
        {
            return BitConverter.ToString(m_md5Provider.ComputeHash(data)).Replace("-", string.Empty);
        }

        public static string GetMd5(string data)
        {
            return BitConverter.ToString(m_md5Provider.ComputeHash(Encoding.UTF8.GetBytes(data))).Replace("-", string.Empty);
        }

        public static string CombinePath(string path1, string path2)
        {
            string result = string.Empty;
            if (!path1.EndsWith("/"))
            {
                path1 = path1 + "/";
            }
            if (path2.StartsWith("/"))
            {
                path2 = path2.Substring(1);
            }
            result = path1 + path2;
            return result;
        }

        public static string GetFullName(string fullPath)
        {
            string result = string.Empty;
            int length = fullPath.LastIndexOf("/");
            if (length > 0)
            {
                result = fullPath.Substring(length + 1, fullPath.Length - length - 1);
            }
            return result;
        }

        public static string EraseExtension(string fullname)
        {
            string result = string.Empty;
            int length = fullname.LastIndexOf(".");
            if (length > 0)
            {
                result = fullname.Substring(0,length);
            }
            return result;
        }

        public static string GetExtension(string fullname)
        {
            string result = string.Empty;
            int length = fullname.LastIndexOf(".");
            if (length > 0 && length+1>fullname.Length)
            {
                result = fullname.Substring(length+1);
            }
            return result;
        }

        public static string GetFullDirectory(string fullName)
        {
            return Path.GetDirectoryName(fullName);
        }

        public static bool ClearDirectory(string directoryPath)
        {
            bool result = false;
            try
            {
                string[] files = Directory.GetFiles(directoryPath);
                foreach (var file in files)
                {
                    DeleteFile(file);
                }
                string[] directorys = Directory.GetDirectories(directoryPath);
                foreach (var directory in directorys)
                {
                    DeleteDirectory(directory);
                }
                result = true;
            }
            catch (Exception e)
            {
                result = false;
                m_delefateOnFileOperateFileFail(directoryPath, enFileOperation.DeleteDirectory, e);
            }
            return result;
        }

        public static string GetFileFullName(string filename)
        {
            return Platform.Path +  filename;
        }
        public static bool CopyDirectory(string srcDir,string desDir)
        {
            bool result = false;
            if (!Directory.Exists(srcDir))
            {
                result = false;
            }
            else
            {
                if (!Directory.Exists(desDir))
                {
                    Directory.CreateDirectory(desDir);
                }
                string[] files = Directory.GetFiles(srcDir);
                foreach (var file in files)
                {
                    string filename = Path.GetFileName(file);
                    string desDirfilename = Path.Combine(desDir, filename);
                    File.Copy(file,desDirfilename);
                }
                string[] dirs = Directory.GetDirectories(srcDir);
                foreach (var dir in dirs)
                {
                    string dirname = Path.GetDirectoryName(dir);
                    string name = Path.Combine(desDir, dirname);
                    CopyDirectory(dir, dirname);
                }
                result = true;
            }
            return result;
        }

        public static IEnumerator StartCopyInitialFile(string localname)
        {
            yield return CopyStreamingAssetsToFile(GetInitialFileName(localname), GetFileFullName(localname));
        }

        public static IEnumerator CopyStreamingAssetsToFile(string src,string des)
        {
            
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IPHONE
            src = "file:///" + src;
#endif
//            Debug.LogError("拷贝原路径 安装目录    "+src);
//            Debug.LogError("沙河目录   "+des);
            using (WWW w = new WWW(src))
            {
                yield return w;
                if (string.IsNullOrEmpty(w.error))
                {
                    while (w.isDone == false)
                    {
                        yield return null;
                    }
                    WriteBytesToFile(des,w.bytes,w.bytes.Length);
                }
                else
                {
                    Debuger.LogError(w.error);
                }
            }
        }

        public static void WriteBytesToFile(string path, byte[] bytes, int length)
        {
            string directory = Path.GetDirectoryName(path);
            if (!FileManager.IsDirectoryExist(directory))
            {
                FileManager.CreateDirectory(directory);
            }
            System.IO.FileInfo fileInfo = new System.IO.FileInfo(path);
            using (Stream sw = fileInfo.Open(FileMode.Create,FileAccess.ReadWrite))
            {
                if (bytes != null && length > 0)
                {
                    sw.Write(bytes,0,length);
                }
            }
        }

        public static string GetInitialFileName(string flie)
        {
            return Platform.InitalPath + flie;
        }
    }
}