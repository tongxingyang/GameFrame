using GameFrame.Language;
using UnityEngine;
using UnityEngine.UI;

namespace GameFrame.UGUI
{
    public class LocalizationText:Text
    {
        private LanguageId languageId = LanguageId.None;

        [HideInInspector]
        public LanguageId LanguageId
        {
            get { return languageId; }
            set {
                if (value != languageId)
                {
                    languageId = value;
                    ChangeText();
                }}
        }
        
        protected override void OnEnable()
        {
            base.OnEnable();
            ChangeText();
            LanguageManager.Instance.LanguageEvent += ChangeText;
        }
        
        private void ChangeText()
        {
            text = LanguageManager.Instance.GetLanguage(LanguageId);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (LanguageManager.Instance.LanguageEvent != null) LanguageManager.Instance.LanguageEvent -= ChangeText;
        }
    }
}