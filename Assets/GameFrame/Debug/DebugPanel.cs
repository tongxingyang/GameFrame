using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace GameFrame.Debug
{
    public class DebugPanel:MonoBehaviour
    {
        public Queue AllQueue;
        public Queue LogQueue;
        public Queue WarningQueue;
        public Queue ErrorQueue;
        public GameObject GMPanel;
        public InputField input;
        public DebugLogType LogType;
        private DebugLogVO vo;
        public Text Text;
        public void Awake()
        {
//            gameObject.SetActive(false);
//            if (GMPanel != null)
//            {
//                GMPanel.SetActive(!GMPanel.activeSelf);
//            }
        }

        void Update()
        {
            if(LogType == DebugLogType.None) return;
            if (AllQueue == null)
            {
                AllQueue = DebugLogManager.Instance.allQueue;
            }
            if (LogQueue == null)
            {
                AllQueue = DebugLogManager.Instance.logQueue;
            }
            if (WarningQueue == null)
            {
                AllQueue = DebugLogManager.Instance.warningQueue;
            }
            if (ErrorQueue == null)
            {
                AllQueue = DebugLogManager.Instance.errorQueue;
            }
            if (LogType == DebugLogType.All && AllQueue.Count > 0)
            {
                vo = AllQueue.Dequeue() as DebugLogVO;
            }
            else if (LogType == DebugLogType.Log && LogQueue.Count > 0)
            {
                vo = LogQueue.Dequeue() as DebugLogVO;
            }
            else if (LogType == DebugLogType.Warning && WarningQueue.Count > 0)
            {
                vo = WarningQueue.Dequeue() as DebugLogVO;
            }
            else if (LogType == DebugLogType.Error && ErrorQueue.Count > 0)
            {
                vo = ErrorQueue.Dequeue() as DebugLogVO;
            }
            else
            {
                vo = null;
            }
    		
            if (vo == null)
            {
                return;
            }
    		
            string stackTrace = vo.stackTrack;
            Text.text += "\n";
            switch (vo.LogType)
            {
                case UnityEngine.LogType.Log:
                    Text.text += string.Format("<color='{1}'>{0}</color>\n{2}", vo.logString , "#008844", stackTrace);
                    break;
                case UnityEngine.LogType.Warning:
                    Text.text += string.Format("<color='{1}'>{0}</color>\n{2}", vo.logString, "#ffa500", stackTrace);
                    break;
                case UnityEngine.LogType.Assert:
                case UnityEngine.LogType.Error:
                case UnityEngine.LogType.Exception:
                    Text.text += string.Format("<color='{1}'>{0}</color>\n{2}", vo.logString, "#ff0000", stackTrace);
                    break;
                default:
                    break;
            }
            Text.rectTransform.sizeDelta = new Vector2(Text.rectTransform.sizeDelta.x, Text.preferredHeight > (Text.transform.parent as RectTransform).sizeDelta.y ? Text.preferredHeight :  (Text.transform.parent as RectTransform).sizeDelta.y);
    
        }
        public void ShowAll()
        {
            LogType = DebugLogType.All;
        }
    	
        public void ShowLog()
        {
            LogType = DebugLogType.Log;
        }
    	
        public void ShowWarning()
        {
            LogType = DebugLogType.Warning;
        }
    	
        public void ShowError()
        {
            LogType = DebugLogType.Error;
        }
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void OnGMSendButtonClick()
        {
            string str = input.text;
            UnityEngine.Debug.Log(str);
        }
        public void Clear()
        {
            Text.text = "";
        }

        public void ClearAll()
        {
            AllQueue.Clear();
            LogQueue.Clear();
            WarningQueue.Clear();
            ErrorQueue.Clear();
            Clear();
        }
        public void ShowInfo()
        {
            LogType = DebugLogType.None;
            Text.text += "\n";
            Text.text += ApplicationInfo.GetAppInfo();
            Text.rectTransform.sizeDelta = new Vector2(Text.rectTransform.sizeDelta.x, Text.preferredHeight > (Text.transform.parent as RectTransform).sizeDelta.y ? Text.preferredHeight :  (Text.transform.parent as RectTransform).sizeDelta.y);

        }
    	
    }
}