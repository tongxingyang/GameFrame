using System.Collections;
using System.Collections.Generic;
using GameFrame;
using UnityEngine;

namespace GameFrame
{
    public class PathResolver :Singleton<PathResolver> {
        public static string GetPath(string relativePath)
        {
//            return Interface.Instance.GetPath(relativePath);
            return string.Empty;
        }
        // GetBundlePath相比GetPath包含了file:// 用于Asset Bundle
        public static string GetBundlePath(string relativePath)
        {
//            return Interface.Instance.GetBundlePath(relativePath);
            return string.Empty;
        }
        // for AssetBundle.LoadFromFile to get the bundle path
        public static string GetFilePath(string relativePath)
        {
//            return Interface.Instance.GetFilePath(relativePath);
            return string.Empty;
        }
    }
}

