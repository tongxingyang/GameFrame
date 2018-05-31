using System.Collections;
using UnityEngine;

namespace GameFrame.Debug
{
    public class DebugLogVO
    {
        public string logString;
        public string stackTrack;
        public LogType LogType;
    }

    public enum DebugLogType
    {
        None,
        Log,
        Warning,
        Error,
        All
    }
    public class DebugLogManager:MonoBehaviour
    {

        public static DebugLogManager Instance;
        public Queue allQueue;
        public Queue logQueue;
        public Queue warningQueue;
        public Queue errorQueue;
        public bool isShowLog = true;
        public bool isShowWarning = true;
        public bool isShowError = true;
        public bool isShowException = true;

        private DebugLogVO vo;
        private DebugLogVO preVO;
        
        public bool IsShowLog
        {
            get
            {
                return isShowLog;
            }
            set
            {
                isShowLog = value;
            }
        }

        public bool IsShowWarning
        {
            get
            {
                return isShowWarning;
            }
            set
            {
                isShowWarning = value;
            }
        }

        public bool IsShowError
        {
            get
            {
                return isShowError;
            }
            set
            {
                isShowError = value;
            }
        }

        public bool IsShowException
        {
            get
            {
                return isShowException;
            }
            set
            {
                isShowException = value;
            }
        }

        void Awake()
        {
            Instance = this;
            allQueue = Queue.Synchronized(new Queue());
            logQueue = Queue.Synchronized(new Queue());
            warningQueue = Queue.Synchronized(new Queue());
            errorQueue = Queue.Synchronized(new Queue());

            vo = new DebugLogVO();
            vo.logString = "Register Log Callback !";
            vo.stackTrack = "";
            vo.LogType = LogType.Log;
            logQueue.Enqueue(vo);
            Application.logMessageReceived          += CatchLogInfo;
            Application.logMessageReceivedThreaded  += CatchLogInfo;
        }
        private void CatchLogInfo(string logString, string stackTrace, LogType type)
        {
            if(preVO != null && preVO.logString == logString && preVO.stackTrack == stackTrace && preVO.LogType == type)
            {
                return;
            }

            vo = new DebugLogVO();
            vo.logString = logString;
            vo.stackTrack = stackTrace;
            vo.LogType = type;

            preVO = vo;

            switch (type)
            {
                case LogType.Log:
                    logQueue.Enqueue(vo);
                    break;
                case LogType.Warning:
                    warningQueue.Enqueue(vo);
                    break;
                case LogType.Assert:
                case LogType.Error:
                case LogType.Exception:
                    errorQueue.Enqueue(vo);
                    break;
                default:
                    break;
            }

            allQueue.Enqueue(vo);
        }
    }
}