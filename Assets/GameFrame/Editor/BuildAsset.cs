using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GameFrame.Editor
{
    public partial class BuildAsset
    {
        public static string GetPlatformPath(BuildTarget buildTarget)
        {
            string ret = string.Empty;
            switch (buildTarget)
            {
                case BuildTarget.iOS:
                    ret = "Build/ios";
                    break;
                case BuildTarget.Android:
                    ret = "Build/android";
                    break;
                default:
                    ret = "Build/pc";
                    break;
            }
            return ret;
        }

        public static string GetBundleSavePath(BuildTarget target )
        {
            string path;
            switch (target)
            {
                case BuildTarget.Android:
                    path = string.Format("{0}/../../{1}", Application.dataPath, GetPlatformPath(target));
                    break;
                case BuildTarget.iOS:  
                    path = string.Format("{0}/../../{1}", Application.dataPath, GetPlatformPath(target));
                    break;
                default:
                    path = string.Format("{0}/../../{1}", Application.dataPath, GetPlatformPath(target));
                    break;
            }
            return path;
        }

        public static void ResetAssetBundleNames()
        {
            
        }
        public static void SetAssetBundleName(Dictionary<string ,string> assets,string [] depFormats,string depPath,bool preFix = false)
        {
            ResetAssetBundleNames();
            AssetImporter importer = null;
            foreach (KeyValuePair<string,string> keyValuePair in assets)
            {
                string[] depdens = AssetDatabase.GetDependencies(keyValuePair.Key);
                foreach (string depden in depdens)
                {
                    string dep = depden.ToLower();
                    foreach (string depFormat in depFormats)
                    {
                        if (dep.EndsWith(depFormat))
                        {
                            importer = AssetImporter.GetAtPath(dep);
                            string newname;
                            
                        }
                    }
                }
            }
        }
    }
}