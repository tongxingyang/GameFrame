using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using GameFrameDebuger;
using UnityEngine;
/// <summary>
/// 验证url是否可用 返回可以下载的http url
/// </summary>
namespace GameFrame
{
    public class URLVerify  {
    public bool IsDone { get; private set; }
    public string URL { get; private set; }
    public List<string> UrlList { get; private set; }
    private Thread m_thread;

    public URLVerify(List<string> list)
    {
        IsDone = false;
        URL = String.Empty;
        UrlList = list;
    }

    public void Start()
    {
        if (UrlList == null || UrlList.Count == 0)
        {
            IsDone = true;
            URL = String.Empty;
            return;
        }
        if (m_thread == null)
        {
            m_thread = new Thread(URLVerifyList);
            m_thread.Start();
        }
    }

    private void URLVerifyList()
    {
        IsDone = false;
        URL = string.Empty;
        for (int i = 0; i < UrlList.Count; i++)
        {
            if (Verify(UrlList[i]))
            {
                URL = UrlList[i];
                break;
            }
        }
        IsDone = true;
    }

    private bool Verify(string url)
    {
        bool result = false;
        HttpWebRequest httpWebRequest = null;
        HttpWebResponse httpWebResponse = null;
        try
        {
            httpWebRequest = WebRequest.Create(url) as HttpWebRequest;
            httpWebRequest.KeepAlive = false;
            httpWebRequest.Method = "HEAD";
            httpWebRequest.Timeout = 5000;
            httpWebRequest.AllowAutoRedirect = false;
            httpWebRequest.UseDefaultCredentials = true;
            httpWebResponse = httpWebRequest.GetResponse() as HttpWebResponse;
            if (httpWebResponse.StatusCode == HttpStatusCode.OK)
            {
                result = true;
            }
        }
        catch (Exception e)
        {
            result = false;
            Debuger.LogError(e.Message);
            throw;
        }
        finally
        {
            if (httpWebRequest != null)
            {
                httpWebRequest.Abort();
                httpWebRequest = null;
            }
            if (httpWebResponse != null)
            {
                httpWebResponse.Close();
                httpWebResponse = null;
            }
        }
        return result;
        
    }
    public void Abort()
    {
        if (m_thread != null)
        {
            m_thread.Abort();
            m_thread = null;
        }
        IsDone = true;
        URL = string.Empty;
    }
}

}
