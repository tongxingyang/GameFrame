using UnityEditor;

namespace GameFrame.Editor
{
    public partial class BuildAsset
    {
        [MenuItem("Assets/Build Selected for Windows/Character")]
        static void ExportSelectCharacterForWindows()
        {
        }
        [MenuItem("Assets/Build Selected for Android/Character")]
        static void ExportSelectCharacterForAndroid()
        {
        }
        [MenuItem("Assets/Build Selected for iOS/Character")]
        static void ExportSelectCharacterForiOS()
        {
        }


        [MenuItem("Build Windows/Build Characters for Windows")]
        static void ExportAllCharactersForWindows()
        {
        }


        [MenuItem("Build Android/Build Characters for Android")]
        static void ExportAllCharactersForAndroid()
        {
        }

        [MenuItem("Build iOS/Build Characters for iOS")]
        static void ExportAllCharactersForiOS()
        {
        }

    }
}