using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;

public class LogHeiper:MonoBehaviour
{
    private static string logPath = Application.persistentDataPath + "/log/";
    private class LogUnit
    {
        public LogLevel Level;
        public string msg = string.Empty;
    }

    public static bool isWriteLog = true;
    public static bool isReportLog = true;
    public static bool isSync = false;
    private static bool isDestroyed = false;
    private static Queue logMsgQueue = Queue.Synchronized(new Queue());//线程同步安全
    private static string curLogFileName = string.Empty;
    private static FileStream curFileStream = null;
    private static object lockobj = new object();

    private void Start()
    {
        
    }

    private void Update()
    {
        while (LogHeiper.logMsgQueue.Count>0)   
        {
            LogUnit logUnit = LogHeiper.logMsgQueue.Dequeue() as LogUnit;
            switch (logUnit.Level)
            {
                case LogLevel.kDEBUG:
                    if (LogHeiper.isWriteLog)
                    {
                        LogHeiper.Write(logUnit.msg);
                    }
                    break;
                case LogLevel.kERROR:
                    if (LogHeiper.isWriteLog)
                    {
                        LogHeiper.Write(logUnit.msg);
                    }
                    break;
                case LogLevel.kINFO:
                    if (LogHeiper.isWriteLog)
                    {
                        LogHeiper.Write(logUnit.msg);
                    }
                    break;
                case LogLevel.kWARN:
                    if (LogHeiper.isWriteLog)
                    {
                        LogHeiper.Write(logUnit.msg);
                    }
                    break;
                case LogLevel.kREPORT:
                    if (LogHeiper.isReportLog)
                    {
                        //同过网络发送log信息
                    }
                    break;
            }
        }
    }

    private static void Write(string msg)
    {
        if (LogHeiper.isSync)
        {
            LogHeiper.SyncWriteLog(msg);
        }
        else
        {
            LogHeiper.WriteLog(msg);
        }
    }
    private void OnDestroy()
    {
        LogHeiper.isDestroyed = true;
        if (LogHeiper.curFileStream != null)
        {
            LogHeiper.curFileStream.Close();
        }
    }

    private static void Enqueue(LogUnit unit)
    {
        LogHeiper.logMsgQueue.Enqueue(unit);
    }

    private  static string GetThreadID()
    {
        return string.Empty;
    }

    private static int GetLineNum()
    {
        int result;
        try
        {
            StackTrace stackTrace = new StackTrace(2,true);
            result = stackTrace.GetFrame(0).GetFileLineNumber();
        }
        catch (Exception e)
        {
            result = 0;
        }
        return result;
    }
    private static int GetColumnNum()
    {
        int result;
        try
        {
            StackTrace stackTrace = new StackTrace(2,true);
            result = stackTrace.GetFrame(0).GetFileColumnNumber();
        }
        catch (Exception e)
        {
            result = 0;
        }
        return result;
    }

    private static string GetFileName()
    {
        string  result;
        try
        {
            StackTrace stackTrace = new StackTrace(2,true);
            result = stackTrace.GetFrame(0).GetFileName();
        }
        catch (Exception e)
        {
            result = "#";
        }
        return result;
    }
    private static string GetFuncName()
    {
        string  result;
        try
        {
            StackTrace stackTrace = new StackTrace(2,true);
            result = stackTrace.GetFrame(0).GetMethod().Name;
        }
        catch (Exception e)
        {
            result = "#";
        }
        return result;
    }

    public static void DEBUG(string msg)
    {
        string text = string.Concat(new object[]
        {
           "Debug",
            " ",
            LogHeiper.GetFileName(),
            ":",
            LogHeiper.GetLineNum(),
            ":",
            LogHeiper.GetColumnNum(),
            ":",
            LogHeiper.GetFuncName(),
            " ",
            msg
        });
        string formatmsg = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + " " + text;
        LogHeiper.Enqueue(new LogUnit
        {
            Level = LogLevel.kDEBUG,
            msg = formatmsg
        });
    }
    public static void INFO(string msg)
    {
        string text = string.Concat(new object[]
        {
            "Info",
            " ",
            LogHeiper.GetFileName(),
            ":",
            LogHeiper.GetLineNum(),
            ":",
            LogHeiper.GetColumnNum(),
            ":",
            LogHeiper.GetFuncName(),
            " ",
            msg
        });
        string formatmsg = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + " " + text;
        LogHeiper.Enqueue(new LogUnit
        {
            Level = LogLevel.kINFO,
            msg = formatmsg
        });
    }
    public static void ERROR(string msg)
    {
        string text = string.Concat(new object[]
        {
            "Error",
            " ",
            LogHeiper.GetFileName(),
            ":",
            LogHeiper.GetLineNum(),
            ":",
            LogHeiper.GetColumnNum(),
            ":",
            LogHeiper.GetFuncName(),
            " ",
            msg
        });
        string formatmsg = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + " " + text;
        LogHeiper.Enqueue(new LogUnit
        {
            Level = LogLevel.kERROR,
            msg = formatmsg
        });
    }
    public static void WARN(string msg)
    {
        string text = string.Concat(new object[]
        {
            "Warn",
            " ",
            LogHeiper.GetFileName(),
            ":",
            LogHeiper.GetLineNum(),
            ":",
            LogHeiper.GetColumnNum(),
            ":",
            LogHeiper.GetFuncName(),
            " ",
            msg
        });
        string formatmsg = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + " " + text;
        LogHeiper.Enqueue(new LogUnit
        {
            Level = LogLevel.kWARN,
            msg = formatmsg
        });
    }
    public static void REPORT(string msg,int reportType,int returnCode)
    {
        Dictionary<string, object> dictionary = new Dictionary<string, object>();
        dictionary.Add("str_respara", msg);
        dictionary.Add("uint_report_type", reportType);
        dictionary.Add("uint_toreturncode", returnCode);
//        UserData userData = Pandora.Instance.GetUserData();
//        dictionary.set_Item("str_openid", userData.sOpenId);
//        string msg2 = Json.Serialize(dictionary);
        string msg2 = String.Empty;
        LogHeiper.Enqueue(new LogUnit
        {
            Level = LogLevel.kREPORT,
            msg = msg2
        });
    }

    private static void WriteLog(string msg)
    {
        try
        {
            string text = LogHeiper.logPath + "/log-" + DateTime.Now.ToString("yyyy-M-d dddd") + ".txt";
            if (text != LogHeiper.curLogFileName)
            {
                LogHeiper.curLogFileName = text;
                if (curFileStream != null)
                {
                    curFileStream.Close();
                    curFileStream = null;
                }
                LogHeiper.curFileStream = new FileStream(LogHeiper.curLogFileName,FileMode.Append);
            }
            if (LogHeiper.curFileStream == null)
            {
                LogHeiper.curFileStream = new FileStream(LogHeiper.curLogFileName,FileMode.Append);
            }
            byte[] bytes = Encoding.UTF8.GetBytes(msg + "\n");
            LogHeiper.curFileStream.Write(bytes,0,bytes.Length);
            LogHeiper.curFileStream.Flush();
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log("写日志出现异常" + e.Message);
        }
    }

    private static void SyncWriteLog(string msg)
    {
        if (isDestroyed == true)
        {
            return;
        }
        object obj = LogHeiper.lockobj;
        lock (obj)
        {
            try
            {
                string text = LogHeiper.logPath + "/log-" + DateTime.Now.ToString("yyyy-M-d dddd") + ".txt";
                if (text != LogHeiper.curLogFileName)
                {
                    LogHeiper.curLogFileName = text;
                    if (curFileStream != null)
                    {
                        curFileStream.Close();
                        curFileStream = null;
                    }
                    LogHeiper.curFileStream = new FileStream(LogHeiper.curLogFileName,FileMode.Append);
                }
                if (LogHeiper.curFileStream == null)
                {
                    LogHeiper.curFileStream = new FileStream(LogHeiper.curLogFileName,FileMode.Append);
                }
                byte[] bytes = Encoding.UTF8.GetBytes(msg + "\n");
                LogHeiper.curFileStream.Write(bytes,0,bytes.Length);
                LogHeiper.curFileStream.Flush();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log("写日志出现异常" + e.Message);
            }
        }
    }
}
