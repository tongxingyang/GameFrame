using UnityEditor;

namespace GameFrame.Editor
{
    public partial class BuildAsset
    {
        
        [MenuItem("Assets/Build Selected for Windows/Texture")]
        static void ExportSelectedTexturesForWindows()
        {
        }
        [MenuItem("Assets/Build Selected for Android/Texture")]
        static void ExportSelectedTexturesForAndroid()
        {
        }
        [MenuItem("Assets/Build Selected for iOS/Texture")]
        static void ExportSelectedTexturesForiOS()
        {
        }

        [MenuItem("Build Windows/Build Textures for Windows")]
        static void ExportAllTexturesForWindows()
        {
        }


        [MenuItem("Build Android/Build Textures for Android")]
        static void ExportAllTexturesForAndroid()
        {
        }


        [MenuItem("Build iOS/Build Textures for iOS")]
        static void ExportAllTexturesForiOS()
        {
        }
    }
}