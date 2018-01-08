using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class LogWriter
{

    private string logpath = Application.persistentDataPath + "/Log/";
    private string logfilename = "log{0}.txt";
    private string logfilepath = string.Empty;

    public LogWriter()
    {
        if (!Directory.Exists(logpath))
        {
            Directory.CreateDirectory(logpath);
        }
        this.logfilepath = logpath + string.Format(this.logfilename,DateTime.Today.ToString("yyyy MMMM dd"));
    }

    public void ExcuteWrite(string content)
    {
        using (StreamWriter writer = new StreamWriter(logfilepath,true,Encoding.UTF8))
        {
            writer.WriteLine(content);
        }
    }
}
