using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using GameFrameDebuger;
using UnityEngine;
using Path = System.IO.Path;

namespace GameFrame
{
    public class FileManager
    {
        public delegate void DelegateOnFileOperateFileFail(string fullpath, enFileOperation enFileOperation,Exception exception);
        
        public static DelegateOnFileOperateFileFail m_delefateOnFileOperateFileFail = delegate { };

        public static bool IsFileExist(string filePath)
        {
            return File.Exists(filePath);
        }

        public static bool IsDirectoryExist(string directory)
        {
            return Directory.Exists(directory);
        }

        public static string ReadAllText(string path)
        {
            return File.ReadAllText(path);
        }
        
        public static bool CreateDirectory(string directory)
        {
            bool result = false;
            if (IsDirectoryExist(directory))
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

        public static bool DeleteDirectory(string directory, bool recursive = true)
        {
            bool result = false;
            if (IsDirectoryExist(directory))
            {
                try
                {
                    Directory.Delete(directory, recursive);
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
                    Debuger.LogError("GetFileLength出错 filePath:  "+filePath+" "+e.Message);
                }
            }
            else
            {
                result = 0;
                Debuger.LogError("GetFileLength出错 文件不存在 filePath:  "+filePath);
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
                    Debuger.LogError("Error ReadFile "+e.Message);
                }
            }
            else
            {
                Debuger.LogError("读取文件出错 文件不存在 filePath :"+filePath);
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
            else
            {
                Debuger.LogError("删除文件出错 文件不存在 filePath :"+filePath);
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
                fileStream = null;
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

        public static string GetFileMD5(string filePath)
        {
            if (IsFileExist(filePath))
            {
                return MD5Util.ComputeHash(ReadFile(filePath));
            }
            return string.Empty;
        }

        public static string GetMD5(byte[] data)
        {
            return MD5Util.ComputeHash(data);
        }

        public static string GetMd5UTF8(string data)
        {
            return MD5Util.ComputeHashUTF8(data);
        }

        public static string GetMd5Unicode(string data)
        {
            return MD5Util.ComputeHashUnicode(data);
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
            int length = fullPath.LastIndexOf("/", StringComparison.Ordinal);
            if (length > 0)
            {
                result = fullPath.Substring(length + 1, fullPath.Length - length - 1);
            }
            return result;
        }

        public static string ChangeExtension(string path, string ext)
        {
            string e = Path.GetExtension(path);
            if (string.IsNullOrEmpty(e))
            {
                return path + ext;
            }

            bool backDSC = path.IndexOf('\\') != -1;
            path = path.Replace('\\', '/');
            if (path.IndexOf('/') == -1)
            {
                return path.Substring(0, path.LastIndexOf('.')) + ext;
            }

            string dir = path.Substring(0, path.LastIndexOf('/'));
            string name = path.Substring(path.LastIndexOf('/'), path.Length - path.LastIndexOf('/'));
            name = name.Substring(0, name.LastIndexOf('.')) + ext;
            path = dir + name;

            if (backDSC)
            {
                path = path.Replace('/', '\\');
            }
            return path;
        }

        public static string EraseExtension(string fullname)
        {
            string result = string.Empty;
            int length = fullname.LastIndexOf(".", StringComparison.Ordinal);
            if (length > 0)
            {
                result = fullname.Substring(0,length);
            }
            return result;
        }

        public static string GetExtension(string fullname)
        {
            string result = string.Empty;
            int length = fullname.LastIndexOf(".", StringComparison.Ordinal);
            if (length > 0 && length+1>fullname.Length)
            {
                result = fullname.Substring(length+1);
            }
            return result;
        }

        public static void RecursiveFile(List<string> paths, List<string> fileList, List<string> exts = null)
        {
            RecursiveFile(paths.ToArray(), fileList, exts);
        }

        public static void RecursiveFile(string[] paths, List<string> fileList, List<string> exts = null)
        {
            for (int i = 0; i < paths.Length; i++)
            {
                RecursiveFile(paths[i], fileList, exts);
            }
        }
        public static void RecursiveFile(string path, List<string> fileList, List<string> exts = null)
        {


            string[] names = Directory.GetFiles(path);
            string[] dirs = Directory.GetDirectories(path);
            bool isCheckExt = exts != null && exts.Count > 0;
            foreach (string filename in names)
            {
                if (isCheckExt)
                {
                    string ext = Path.GetExtension(filename).ToLower();
                    if (!exts.Contains(ext))
                        continue;
                }


                string fn = Path.GetFileName(filename);
                if (fn.Equals(".DS_Store")) continue;

                string file = filename.Replace('\\', '/');
                fileList.Add(file);
            }

#if UNITY_EDITOR
            int count = dirs.Length;
            int index = 0;
#endif


            foreach (string dir in dirs)
            {

#if UNITY_EDITOR
                UnityEditor.EditorUtility.DisplayProgressBar("遍历目录", path, 1f * (index++) / count);
#endif
                RecursiveFile(dir, fileList, exts);
            }

#if UNITY_EDITOR
            UnityEditor.EditorUtility.ClearProgressBar();
#endif
        }


        public static string GetDirectoryName(string fullName)
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
                    CopyDirectory(dir, name);
                }
                result = true;
            }
            return result;
        }

        public static void CopyFile(string srcFile, string desFile)
        {
            File.Copy(srcFile, desFile);
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
                    Debuger.LogError("文件拷贝出错 源文件 :"+src+"  目标文件:   "+des+"  "+w.error);
                }
            }
        }

        public static void WriteBytesToFile(string path, byte[] bytes, int length)
        {
            string directory = GetDirectoryName(path);
            if (!IsDirectoryExist(directory))
            {
                CreateDirectory(directory);
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