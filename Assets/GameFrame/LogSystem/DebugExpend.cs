//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.IO;
//using System.Linq;
//using System.Text;
//using UnityEngine;
//using UnityEngine.Internal;
//using Debug = UnityEngine.Debug;
//
//public sealed class MUDebug
//{
//  private static bool _isOpenBubug = true;
//  private static bool _isOpenWriteLogTofile = false;
//  private static bool _isOpenWriteStrToFile = false;
//  public static string LogFileName = "";
//  private static Application.LogCallback LogCallBack = (Application.LogCallback) null;
//  private static StreamWriter WriterForLog = (StreamWriter) null;
//  private static StreamWriter WriterForStr = (StreamWriter) null;
//  private static string LogOutPutPath = (string) null;
//  private static string WriteStrFilePath = (string) null;
//  private static string OriginalFileContent = (string) null;
//  private static Dictionary<string, int> FrameLastTicks = new Dictionary<string, int>();
//  private static int LastTicks = 0;
//  private static int LastTicks2 = 0;
//
//  public static bool IsOpenDebug
//  {
//    get
//    {
//      return MUDebug._isOpenBubug;
//    }
//    set
//    {
//      MUDebug._isOpenBubug = value;
//    }
//  }
//
//  public static bool IsOpenWriteLogToFile
//  {
//    get
//    {
//      return MUDebug._isOpenWriteLogTofile;
//    }
//    set
//    {
//      MUDebug._isOpenWriteLogTofile = value;
//      if (MUDebug._isOpenWriteLogTofile && !MUDebug.IsMethodInDelegateList(MUDebug.LogCallBack, new Application.LogCallback(MUDebug.LogMessageWriteToFile)))
//      {
//        MUDebug.LogCallBack += new Application.LogCallback(MUDebug.LogMessageWriteToFile);
//        if (MUDebug.WriterForLog == null)
//          MUDebug.WriterForLog = File.CreateText(MUDebug.SetLogOutPutPath());
//      }
//      else if (!MUDebug._isOpenWriteLogTofile && MUDebug.IsMethodInDelegateList(MUDebug.LogCallBack, new Application.LogCallback(MUDebug.LogMessageWriteToFile)))
//        MUDebug.LogCallBack -= new Application.LogCallback(MUDebug.LogMessageWriteToFile);
//      Application.RegisterLogCallback(MUDebug.LogCallBack);
//    }
//  }
//
//  public static bool IsOpenWriteStrToFile
//  {
//    get
//    {
//      return MUDebug._isOpenWriteStrToFile;
//    }
//    set
//    {
//      MUDebug._isOpenWriteStrToFile = value;
//      if (!MUDebug._isOpenWriteStrToFile || File.Exists(MUDebug.SetWriteStrFilePath()))
//        return;
//      File.Create(MUDebug.SetWriteStrFilePath()).Dispose();
//    }
//  }
//
//  public static void InitFlag()
//  {
//    MUDebug.IsOpenDebug = File.Exists(PathUtils.GetPersistentPath("debug.data"));
//    string persistentPath1 = PathUtils.GetPersistentPath("writeStr.data");
//    if (!MUDebug.IsOpenWriteStrToFile)
//      MUDebug.IsOpenWriteStrToFile = File.Exists(persistentPath1);
//    string persistentPath2 = PathUtils.GetPersistentPath("writeLog.data");
//    if (MUDebug.IsOpenWriteLogToFile || !File.Exists(persistentPath2))
//      return;
//    MUDebug.IsOpenWriteLogToFile = File.Exists(persistentPath2);
//  }
//
//  private static bool IsMethodInDelegateList(Application.LogCallback targetCallBack, Application.LogCallback method)
//  {
//    return targetCallBack != null && ((IEnumerable<Delegate>) targetCallBack.GetInvocationList()).Contains<Delegate>((Delegate) method);
//  }
//
//  public static void Break()
//  {
//    if (!MUDebug.IsOpenDebug)
//      return;
//    Debug.Break();
//  }
//
//  public static void ClearDeveloperConsole()
//  {
//    if (!MUDebug.IsOpenDebug)
//      return;
//    Debug.ClearDeveloperConsole();
//  }
//
//  public static void DebugBreak()
//  {
//    if (!MUDebug.IsOpenDebug)
//      return;
//    Debug.DebugBreak();
//  }
//
//  private static StringBuilder LogArray<T>(string separator, T[] messages)
//  {
//    StringBuilder stringBuilder = new StringBuilder();
//    stringBuilder.Append(DateTime.Now.ToString("G"));
//    stringBuilder.Append(" ：");
//    if (messages != null)
//    {
//      if (messages.Length > 0)
//      {
//        for (int index = 0; index < messages.Length; ++index)
//        {
//          T message = messages[index];
//          if (index < messages.Length - 1)
//          {
//            if ((object) message == null)
//              stringBuilder.AppendFormat("{0}{1}", (object) "null", (object) separator);
//            else
//              stringBuilder.AppendFormat("{0}{1}", (object) message, (object) separator);
//          }
//          else if ((object) message == null)
//            stringBuilder.AppendFormat("{0}", (object) "null");
//          else
//            stringBuilder.AppendFormat("{0}", (object) message);
//        }
//      }
//      else
//        stringBuilder.AppendFormat("messages.Length:{0}", (object) messages.Length);
//    }
//    else
//      stringBuilder.Append("messsages:null");
//    return stringBuilder;
//  }
//
//  public static void Log<T>(string separator, T[] messages)
//  {
//    if (!MUDebug.IsOpenDebug && !MUDebug.IsOpenWriteLogToFile)
//      return;
//    Debug.Log((object) MUDebug.LogArray<T>(separator, messages).ToString());
//  }
//
//  public static void Log(params object[] messages)
//  {
//    MUDebug.Log<object>("   ", messages);
//  }
//
//  public static void Log<T>(params T[] messages)
//  {
//    MUDebug.Log<T>("   ", messages);
//  }
//
//  public static void Log<T>(List<T> messages)
//  {
//    MUDebug.Log<T>("   ", messages.ToArray());
//  }
//
//  public static void Log(Vector3 vector)
//  {
//    if (!MUDebug.IsOpenDebug)
//      return;
//    StringBuilder stringBuilder = new StringBuilder();
//    stringBuilder.Append(DateTime.Now.ToString("G"));
//    stringBuilder.Append(" ：");
//    stringBuilder.Append("Vector3(");
//    stringBuilder.AppendFormat("{0},", (object) vector.x);
//    stringBuilder.AppendFormat("{0},", (object) vector.y);
//    stringBuilder.AppendFormat("{0}", (object) vector.z);
//    stringBuilder.Append(")");
//    Debug.Log((object) stringBuilder.ToString());
//  }
//
//  public static void Log<T, M>(Dictionary<T, M> dics)
//  {
//    if (!MUDebug.IsOpenDebug)
//      return;
//    StringBuilder stringBuilder = new StringBuilder();
//    stringBuilder.Append(DateTime.Now.ToString("G"));
//    stringBuilder.Append(" ：");
//    stringBuilder.Append("Dictionary ");
//    if (dics != null)
//    {
//      foreach (T key in dics.Keys)
//      {
//        M dic = dics[key];
//        if ((object) dic == null)
//          stringBuilder.AppendFormat(" {0}:{1} ,", (object) key, (object) "null");
//        else
//          stringBuilder.AppendFormat(" {0}:{1} ,", (object) key, (object) dic);
//      }
//      stringBuilder.AppendFormat("    keys Count:{0}", (object) dics.Keys.Count);
//    }
//    else
//      stringBuilder.Append("be null");
//    Debug.Log((object) stringBuilder.ToString());
//  }
//
//  public static void Log(object message, UnityEngine.Object context)
//  {
//    if (!MUDebug.IsOpenDebug)
//      return;
//    Debug.Log(message, context);
//  }
//
//  public static void LogWarning(params object[] messages)
//  {
//    MUDebug.LogWarning<object>("   ", messages);
//  }
//
//  public static void LogWarning<T>(string separator, T[] messages)
//  {
//    if (!MUDebug.IsOpenDebug)
//      return;
//    Debug.LogWarning((object) MUDebug.LogArray<T>(separator, messages).ToString());
//  }
//
//  public static void LogWarning<T>(params T[] messages)
//  {
//    MUDebug.LogWarning<T>("  ", messages);
//  }
//
//  public static void LogWarning<T>(List<T> messages)
//  {
//    MUDebug.LogWarning<T>("  ", messages.ToArray());
//  }
//
//  public static void LogWarning(object message, UnityEngine.Object context)
//  {
//    if (!MUDebug.IsOpenDebug)
//      return;
//    Debug.LogWarning(message, context);
//  }
//
//  public static void LogError(params object[] messages)
//  {
//    MUDebug.LogError<object>("   ", messages);
//  }
//
//  public static void LogError<T>(string separator, T[] messages)
//  {
//    if (!MUDebug.IsOpenDebug && !MUDebug.IsOpenWriteLogToFile)
//      return;
//    Debug.LogError((object) MUDebug.LogArray<T>(separator, messages).ToString());
//  }
//
//  public static void LogError<T>(params T[] messages)
//  {
//    MUDebug.LogError<T>("   ", messages);
//  }
//
//  public static void LogError(object message, UnityEngine.Object context)
//  {
//    if (!MUDebug.IsOpenDebug)
//      return;
//    Debug.LogError(message, context);
//  }
//
//  public static void LogException(Exception exception)
//  {
//    if (!MUDebug.IsOpenDebug)
//      return;
//    Debug.LogException(exception);
//  }
//
//  public static void LogException(Exception exception, UnityEngine.Object context)
//  {
//    if (!MUDebug.IsOpenDebug)
//      return;
//    Debug.LogException(exception, context);
//  }
//
//  public static void LogStackMsg(string message)
//  {
//    if (!MUDebug.IsOpenDebug)
//      return;
//    StackTrace stackTrace = new StackTrace();
//    Debug.LogError((object) (message + (object) stackTrace));
//  }
//
//  [ExcludeFromDocs]
//  public static void DrawLine(Vector3 start, Vector3 end)
//  {
//    if (!MUDebug.IsOpenDebug)
//      return;
//    Debug.DrawLine(start, end);
//  }
//
//  [ExcludeFromDocs]
//  public static void DrawLine(Vector3 start, Vector3 end, Color color)
//  {
//    if (!MUDebug.IsOpenDebug)
//      return;
//    Debug.DrawLine(start, end, color);
//  }
//
//  [ExcludeFromDocs]
//  public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration)
//  {
//    if (!MUDebug.IsOpenDebug)
//      return;
//    Debug.DrawLine(start, end, color, duration);
//  }
//
//  public static void DrawLine(Vector3 start, Vector3 end, [DefaultValue("Color.white")] Color color, [DefaultValue("0.0f")] float duration, [DefaultValue("true")] bool depthTest)
//  {
//    if (!MUDebug.IsOpenDebug)
//      return;
//    Debug.DrawLine(start, end, color, duration, depthTest);
//  }
//
//  [ExcludeFromDocs]
//  public static void DrawRay(Vector3 start, Vector3 dir)
//  {
//    if (!MUDebug.IsOpenDebug)
//      return;
//    Debug.DrawRay(start, dir);
//  }
//
//  [ExcludeFromDocs]
//  public static void DrawRay(Vector3 start, Vector3 dir, Color color)
//  {
//    if (!MUDebug.IsOpenDebug)
//      return;
//    Debug.DrawRay(start, dir, color);
//  }
//
//  [ExcludeFromDocs]
//  public static void DrawRay(Vector3 start, Vector3 dir, Color color, float duration)
//  {
//    if (!MUDebug.IsOpenDebug)
//      return;
//    Debug.DrawRay(start, dir, color, duration);
//  }
//
//  public static void DrawRay(Vector3 start, Vector3 dir, [DefaultValue("Color.white")] Color color, [DefaultValue("0.0f")] float duration, [DefaultValue("true")] bool depthTest)
//  {
//    if (!MUDebug.IsOpenDebug)
//      return;
//    Debug.DrawRay(start, dir, color, duration, depthTest);
//  }
//
//  public static string GetWriteLogFilePath()
//  {
//    return MUDebug.LogOutPutPath;
//  }
//
//  public static string GetWriteStrFilePath()
//  {
//    return MUDebug.WriteStrFilePath;
//  }
//
//  private static string SetLogOutPutPath()
//  {
//    FileInfo[] files = MUDebug.CheckDirectoryOrCreate(Application.persistentDataPath + "/DebugLog").GetFiles("*.txt", SearchOption.AllDirectories);
//    if (files.Length >= 5)
//    {
//      double totalMinutes = (files[0].CreationTime - DateTime.MinValue).TotalMinutes;
//      FileInfo fileInfo1 = files[0];
//      foreach (FileInfo fileInfo2 in files)
//      {
//        TimeSpan timeSpan = fileInfo2.CreationTime - DateTime.MinValue;
//        if (timeSpan.TotalMinutes < totalMinutes)
//        {
//          totalMinutes = timeSpan.TotalMinutes;
//          fileInfo1 = fileInfo2;
//        }
//      }
//      fileInfo1.Delete();
//    }
//    MUDebug.LogOutPutPath = Application.persistentDataPath + "/DebugLog/" + DateTime.Now.ToString("yyyy_MM_dd , hh-mm-ss") + ".txt";
//    return MUDebug.LogOutPutPath;
//  }
//
//  private static string SetWriteStrFilePath()
//  {
//    MUDebug.CheckDirectoryOrCreate(Application.persistentDataPath + "/DebugLog");
//    MUDebug.WriteStrFilePath = Application.persistentDataPath + "/DebugLog/" + SystemInfo.deviceModel+ "_" + DateTime.Now.ToString("yyyy_MM_dd") + ".log";
//    return MUDebug.WriteStrFilePath;
//  }
//
//  private static DirectoryInfo CheckDirectoryOrCreate(string path)
//  {
//    DirectoryInfo directoryInfo = new DirectoryInfo(path);
//    if (!directoryInfo.Exists)
//      directoryInfo.Create();
//    return directoryInfo;
//  }
//
//  private static void LogMessageWriteToFile(string message, string stackTrace, LogType type)
//  {
//    MUDebug.WriterForLog.WriteLine(type.ToString() + ":" + message + "  StackTrace:" + stackTrace);
//    MUDebug.WriterForLog.WriteLine("");
//    MUDebug.WriterForLog.Flush();
//  }
//
//  public static void WriteStrToFile(string strContent)
//  {
//    if (!MUDebug.IsOpenWriteStrToFile)
//      return;
//    if (MUDebug.OriginalFileContent == null)
//    {
//      try
//      {
//        MUDebug.OriginalFileContent = File.ReadAllText(MUDebug.WriteStrFilePath);
//      }
//      catch (Exception ex)
//      {
//        return;
//      }
//    }
//    if (MUDebug.WriterForStr == null)
//    {
//      MUDebug.WriterForStr = File.CreateText(MUDebug.WriteStrFilePath);
//      MUDebug.WriterForStr.Write(MUDebug.OriginalFileContent);
//    }
//    MUDebug.WriterForStr.WriteLine(strContent);
//    MUDebug.WriterForStr.WriteLine("");
//    MUDebug.WriterForStr.Flush();
//  }
//
//  public static void Dispose()
//  {
//    if (MUDebug.WriterForLog != null)
//      MUDebug.WriterForLog.Dispose();
//    if (MUDebug.WriterForStr == null)
//      return;
//    MUDebug.WriterForStr.Dispose();
//  }
//
////  public static void LogTime2(string routineName)
////  {
////    int myTimer = Global.GetMyTimer();
////    Debug.LogError((object) (routineName + Global.GetLang(" 时间间隔 ") + (object) (myTimer - MUDebug.LastTicks2)));
////    MUDebug.LastTicks2 = myTimer;
////  }
//
//}
//
