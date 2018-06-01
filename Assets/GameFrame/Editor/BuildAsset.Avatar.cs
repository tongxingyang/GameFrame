using UnityEditor;

namespace GameFrame.Editor
{
    public partial class BuildAsset
    {
        [MenuItem("Assets/Build Selected for Windows/Avatar")]
        static void ExportSelectedAvatarsForWindows()
        {
        }
        [MenuItem("Assets/Build Selected for Android/Avatar")]
        static void ExportSelectedAvatarsForAndroid()
        {
        }
        [MenuItem("Assets/Build Selected for iOS/Avatar")]
        static void ExportSelectedAvatarsForiOS()
        {
        }

        [MenuItem("Build Windows/Build Avatars for Windows")]
        static void ExportAllAvatarsForWindows()
        {
        }


        [MenuItem("Build Android/Build Avatars for Android")]
        static void ExportAllAvatarsForAndroid()
        {
        }


        [MenuItem("Build iOS/Build Avatars for iOS")]
        static void ExportAllAvatarsForiOS()
        {
        }
    }
}