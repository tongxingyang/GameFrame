using System.Collections;
using System.Collections.Generic;
using System.IO;
using GameFrame;
using UnityEngine;

namespace GameFrame
{
    public class PathResolver {
        public static void ClearAllPlatformDirctory()
        {
            foreach(RuntimePlatform p in Platform.runtimePlatformEnums)
            {
                ClearPlatformDirctory(p);
            }
        }


        public static void ClearOtherPlatformDirctory(RuntimePlatform p)
        {
            foreach(RuntimePlatform o in Platform.runtimePlatformEnums)
            {
                if (o == p)
                    continue;
            
                ClearPlatformDirctory(o);
            }
        }
        public static void ClearPlatformDirctory(RuntimePlatform p)
        {
            ClearDirectory( Application.streamingAssetsPath + "/" + Platform.GetPlatformDirectory(p, true));
        }

        public static void ClearDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }
        public static void DeleteDirectory(string path)
        {
            if (!Directory.Exists (path))
            {
                return;
            }

            string[] names = Directory.GetFiles(path);
            string[] dirs = Directory.GetDirectories(path);

		
            foreach (string dir in dirs) {
                DeleteDirectory(dir);
            }


            foreach (string filename in names) {
                File.Delete(filename);
            }

		
            Directory.Delete(path);
        }
        public static void DeleteFile(string path)
        {
            if(File.Exists(path)) File.Delete(path);
        }
        public static string ChangeExtension(string path, string ext)
        {
            string e = Path.GetExtension(path);
            if(string.IsNullOrEmpty(e))
            {
                return path + ext;
            }

            bool backDSC = path.IndexOf('\\') != -1;
            path = path.Replace('\\', '/');
            if(path.IndexOf('/') == -1)
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
        public static void RecursiveFile(List<string> paths, List<string> fileList, List<string> exts = null)
        {
            RecursiveFile(paths.ToArray(), fileList, exts);
        }

        public static void RecursiveFile(string[] paths, List<string> fileList, List<string> exts = null)
        {
            for (int i = 0; i < paths.Length; i ++)
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
                if(fn.Equals(".DS_Store")) continue;

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
                UnityEditor.EditorUtility.DisplayProgressBar("遍历目录", path, 1f *(index ++) / count);
#endif

                RecursiveFile(dir, fileList, exts);
            }


#if UNITY_EDITOR
            UnityEditor.EditorUtility.ClearProgressBar();
#endif
        }
    }
}

