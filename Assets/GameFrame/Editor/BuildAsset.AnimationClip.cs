using UnityEditor;

namespace GameFrame.Editor
{
    public partial class BuildAsset
    {
        
        [MenuItem("Assets/Build Selected for Windows/AnimationClip")]
        static void ExportSelectAnimationClipForWindows()
        {
        }
        [MenuItem("Assets/Build Selected for Android/AnimationClip")]
        static void ExportSelectAnimationClipForAndroid()
        {
        }
        [MenuItem("Assets/Build Selected for iOS/AnimationClip")]
        static void ExportSelectAnimationClipForiOS()
        {
        }

        [MenuItem("Build Windows/Build AnimationClips for Windows")]
        static void ExportAllAnimationClipsForWindows()
        {
        }

        [MenuItem("Build Android/Build AnimationClips for Android")]
        static void ExportAllAnimationClipsForAndroid()
        {
        }


        [MenuItem("Build iOS/Build AnimationClips for iOS")]
        static void ExportAllAnimationClipsForiOS()
        {
        }
    }
}