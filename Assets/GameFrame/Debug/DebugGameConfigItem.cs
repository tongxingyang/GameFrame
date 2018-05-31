using UnityEngine;
using UnityEngine.UI;

namespace GameFrame.AssetManager
{
    public class DebugGameConfigItem:MonoBehaviour
    {
        public string key;
        public object value;
        public InputField input;
        public Toggle toggle;

        public void SetString(string value)
        {
            input.text = value;
            input.gameObject.SetActive(true);
            toggle.gameObject.SetActive(false);
        }

        public void SetBool( bool vallue)
        {
            toggle.isOn = vallue;
            input.gameObject.SetActive(false);
            toggle.gameObject.SetActive(true);
        }

        public string GetString()
        {
            return input.text;
        }

        public bool GetBool()
        {
            return toggle.isOn;
        }

        public object GetObject()
        {
            if (value is bool)
            {
                return GetBool();
            }
            else
            {
                return GetString();
            }
        }
    }
}