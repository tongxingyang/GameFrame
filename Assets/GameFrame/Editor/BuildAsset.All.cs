using UnityEditor;

namespace GameFrame.Editor
{
    public partial class BuildAsset
    {
       
        [MenuItem("Build Windows/Build All AssetBundles for Windows")]
        static void ExportAllResourcesForWindows()
        {
        }

        [MenuItem("Build Android/Build All AssetBundles for Android")]
        static void ExportAllResourcesForAndroid()
        {
        }


        [MenuItem("Build iOS/Build All AssetBundles for iOS")]
        static void ExportAllResourcesForiOS()
        {
        }

        [MenuItem("Build Windows/ReBuild All AssetBundles for Windows")]
        static void ReExportAllResourcesForWindows()
        {
        }

        [MenuItem("Build Android/ReBuild All AssetBundles for Android")]
        static void ReExportAllResourcesForAndroid()
        {
        }


        [MenuItem("Build iOS/ReBuild All AssetBundles for iOS")]
        static void ReExportAllResourcesForiOS()
        {
        }

        [MenuItem("Tools/Clean AssetBundle Names")]
        static void CleanAllAssetBundleNames()
        {
        }

        [MenuItem("Tools/Clear ProgressBar")]
        static void ClearProgressBar()
        {
            EditorUtility.ClearProgressBar();
        }
    }
}