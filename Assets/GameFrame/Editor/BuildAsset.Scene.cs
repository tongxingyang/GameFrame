using UnityEditor;

namespace GameFrame.Editor
{
    public partial class BuildAsset
    {
        [MenuItem("Assets/Build Selected for Windows/Scene")]
        static void ExportSceneForWindows()
        {
        }
        [MenuItem("Assets/Build Selected for iOS/Scene")]
        static void ExportSceneForiOS()
        {
        }
        [MenuItem("Assets/Build Selected for Android/Scene")]
        static void ExportSceneForAndroid()
        {
        }

        [MenuItem("Build Windows/Build Scenes for Windows")]
        static void ExportAllScenesForWindows()
        {
        }
        [MenuItem("Build Android/Build Scenes for Android")]
        static void ExportAllScenesForAndroid()
        {
        }
        [MenuItem("Build iOS/Build Scenes for iOS")]
        static void ExportAllScenesForiOS()
        {
        }
    }
}