using UnityEditor;

namespace GameFrame.Editor
{
    public partial class BuildAsset
    {
        [MenuItem("Assets/Build Selected for Windows/UI")]
        static void ExportSelectedUIsForWindows()
        {
        }
        [MenuItem("Assets/Build Selected for Android/UI")]
        static void ExportSelectedUIsForAndroid()
        {
        }
        [MenuItem("Assets/Build Selected for iOS/UI")]
        static void ExportSelectedUIsForiOS()
        {
        }

        [MenuItem("Build Windows/Build UIs for Windows")]
        static void ExportAllUIsForWindows()
        {
        }


        [MenuItem("Build Android/Build UIs for Android")]
        static void ExportAllUIsForAndroid()
        {
        }


        [MenuItem("Build iOS/Build UIs for iOS")]
        static void ExportAllUIsForiOS()
        {
        }
    }
}