using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using GameFrame;
using UnityEngine;

public class HttpDownload
{
    public const int TIMEOUT_TIME = 20000;//超时时间
    public string URL { get; private set; }//下载地址
    public string Root { get; private set; }//根目录
    public string LocalName { get; private set; }//LocalName

    public string FullName
    {
        get { return string.IsNullOrEmpty(Root) || string.IsNullOrEmpty(LocalName) ? null : Root + "/" + LocalName; }
    }
    public bool IsDone { get; private set; }//是否完成
    public enDownloadErrorState ErrorCode;//错误代码
    public long Length { get; private set; }//总下载大小
    public long CompleteLength { get; private set; }//当前已下载大小
    private Action<HttpDownload, long> update_callback;
    private Action<HttpDownload, bool> finish_callback;
    private Action<HttpDownload, enDownloadErrorState> error_callback;
    private DownLoadContent m_content = null;//content对象
    private HttpWebRequest m_httpwebrequest = null;//httpwebrequest对象
    object lock_obj = new object();//锁对象

    public HttpDownload(string url)
    {
        URL = url;
    }
}
