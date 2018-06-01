using UnityEditor;

namespace GameFrame.Editor
{
    public partial class BuildAsset
    {
        
        [MenuItem("Assets/Build Selected for Windows/Effect")]
        static void ExportSfxForWindows()
        {
        }
        [MenuItem("Assets/Build Selected for iOS/Effect")]
        static void ExportSfxForiOS()
        {
        }
        [MenuItem("Assets/Build Selected for Android/Effect")]
        static void ExportSfxForAndroid()
        {
        }    
        [MenuItem("Build Windows/Build Effect for Windows")]
        static void ExportAllSfxsForWindows()
        {
        }


        [MenuItem("Build Android/Build Effect for Android")]
        static void ExportAllSfxsForAndroid()
        {
        }


        [MenuItem("Build iOS/Build Effect for iOS")]
        static void ExportAllSfxsForiOS()
        {
        }
    }
}