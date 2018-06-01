using UnityEditor;

namespace GameFrame.Editor
{
    public partial class BuildAsset
    {
        [MenuItem("Assets/Build Selected for Windows/Player")]
        static void ExportSelectedPlayersForWindows()
        {
        }
        [MenuItem("Assets/Build Selected for Android/Player")]
        static void ExportSelectedPlayersForAndroid()
        {
        }
        [MenuItem("Assets/Build Selected for iOS/Player")]
        static void ExportSelectedPlayersForiOS()
        {
        }
    
        [MenuItem("Build Windows/Build Players for Windows")]
        static void ExportAllPlayersForWindows()
        {
        }


        [MenuItem("Build Android/Build Players for Android")]
        static void ExportAllPlayersForAndroid()
        {
        }


        [MenuItem("Build iOS/Build Players for iOS")]
        static void ExportAllPlayersForiOS()
        {
        }
    }
}