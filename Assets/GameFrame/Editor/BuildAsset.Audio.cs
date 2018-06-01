using UnityEditor;

namespace GameFrame.Editor
{
    public partial class BuildAsset
    {
        [MenuItem("Assets/Build Selected for Windows/Audio")]
        static void ExportAudioForWindows()
        {
        }

        [MenuItem("Assets/Build Selected for iOS/Audio")]
        static void ExportAudioForiOS()
        {
        }
        [MenuItem("Assets/Build Selected for Android/Audio")]
        static void ExportAudioForAndroid()
        {
        }
        [MenuItem("Build Windows/Build Audios for Windows")]
        static void ExportAllAudiosForWindows()
        {
        }


        [MenuItem("Build Android/Build Audios for Android")]
        static void ExportAllAudiosForAndroid()
        {
        }


        [MenuItem("Build iOS/Build Audios for iOS")]
        static void ExportAllAudiosForiOS()
        {
        }
    }
}