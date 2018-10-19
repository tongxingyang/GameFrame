using GameFrame.Language;
using GameFrame.UGUI;
using UnityEditor;

namespace GameFrame.Editor
{
    [CustomEditor(typeof(LocalizationText))]
    public class LocalizationTextEditor:UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            LocalizationText localizationText = target as LocalizationText;
            localizationText.LanguageId =(LanguageId) EditorGUILayout.EnumPopup("语言id", localizationText.LanguageId);
            base.OnInspectorGUI();
        }
    }
}