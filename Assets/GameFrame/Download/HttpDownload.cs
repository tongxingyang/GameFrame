using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using GameFrame;
using GameFrameDebuger;
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
    public void Start(string root,string fillname,Action<HttpDownload,long> update = null,Action<HttpDownload,bool> finish = null,Action<HttpDownload,enDownloadErrorState> error = null){
        Abort();
        Root = root;
        LocalName = fillname;
        IsDone = false;
        Length = 0;
        CompleteLength = 0;
        update_callback = update;
        finish_callback = finish;
        error_callback = error;
        ErrorCode = enDownloadErrorState.None;
        m_content = new DownLoadContent(FullName);
        Download();
    }
    public void Abort(){
        lock(lock_obj)
        {
            if (m_content != null && m_content.State == enDownloadState.Downloading)
            {
                OnFailed(enDownloadErrorState.Abort);
            }
        }

    }
    public void Cancel(){
        lock(lock_obj){
            if(m_content!=null && m_content.State == enDownloadState.Downloading){
                m_content.State = enDownloadState.Canceling;
            }else
            {
                IsDone = true;
            }
        }
    }
    private void OnFinish(){
        lock(lock_obj){
            if(m_content!=null){
                m_content.State = enDownloadState.Completed;
                m_content.Close();
                m_content = null;
            }
            if(m_httpwebrequest!=null){
                m_httpwebrequest.Abort();
                m_httpwebrequest = null;
            }
            IsDone = true;
        }
    }
    private void OnFailed(enDownloadErrorState state){
        lock(lock_obj){
            if(m_content!=null)
            {
                m_content.State = enDownloadState.Failed;
                m_content.Close();
                m_content = null;
            }
            if(m_httpwebrequest!=null){
                m_httpwebrequest.Abort();
                m_httpwebrequest = null;
            }
            IsDone = true;
            ErrorCode = state;
            if(error_callback!=null){
                error_callback(this,ErrorCode);
            }
        }
    }
    private void Download(){
        try
        {
            lock(lock_obj){
                m_httpwebrequest = WebRequest.Create(URL+LocalName) as HttpWebRequest;
                m_httpwebrequest.Timeout = TIMEOUT_TIME;
                m_httpwebrequest.KeepAlive = false;
                m_httpwebrequest.IfModifiedSince = m_content.LastModifTime;
                IAsyncResult iasync = m_httpwebrequest.BeginGetResponse(OnResponseCallback,m_httpwebrequest);
                RegisterTimeOut(iasync.AsyncWaitHandle);
            }
        }catch (Exception e)
        {
            Debuger.LogWarning("HttpDownload - \"" + LocalName + "\" download failed!"
                                    + "\nMessage:" + e.Message);
                UnRegisterTimeOut();
                OnFailed(enDownloadErrorState.NoResponse);
        }
    }
    private void OnResponseCallback(IAsyncResult ar){
        try
        {
            UnRegisterTimeOut();
            lock(lock_obj){
                HttpWebRequest httpwebrequest = ar.AsyncState as HttpWebRequest;
                HttpWebResponse httpwebresponse = httpwebrequest.BetterEndGetResponse(ar) as HttpWebResponse;
                if(httpwebresponse.StatusCode == HttpStatusCode.OK){
                     m_content.WebResponse = httpwebresponse;
                     Length = httpwebresponse.ContentLength;
                     BeginRead(OnReadCallback);// public delegate void AsyncCallback(IAsyncResult ar);
                }else if(httpwebresponse.StatusCode == HttpStatusCode.NotModified){
                     
                }
            }
        }
        catch
        {
        }
    }
    private void PartialDownload(){

    }
    private void OnPartialResponseCallback(IAsyncResult ar){

    }
    private void BeginRead(AsyncCallback callback){
        if(m_content==null){
            OnFailed(enDownloadErrorState.Abort);
            return;
        }
        if(m_content.State == enDownloadState.Canceling){
            OnFailed(enDownloadErrorState.Cancel);
            return;
        }
        //duqu shiju
        m_content.ResponseStream.BeginRead(m_content.Buffer, 0, DownLoadContent.BUFFER_SIZE, callback, m_content);
    }
    private void OnReadCallback(IAsyncResult ar){
        try
        {
            lock(lock_obj){
                 DownLoadContent content = ar.AsyncState as DownLoadContent;
                 if(content.ResponseStream==null){
                    return;
                 }
                 int read = content.ResponseStream.EndRead(ar);
                 if(read>0){
                    content.ContentFileStream.Write(content.Buffer,0,read);
                    content.ContentFileStream.Flush();
                     CompleteLength += read;
                    if(update_callback!=null){
                        update_callback(this,(long)read);
                    }
                 }else{
                    OnFinish();
                    if(finish_callback!=null){
                        finish_callback(this,true);
                    }
                    return;
                 }
                 BeginRead(OnReadCallback);
            }

        }
        catch (Exception e)
        {
            Debuger.LogError(e.Message);
            OnFailed(enDownloadErrorState.DownloadError);
        }
    }

    #region TimeOut

    private RegisteredWaitHandle m_registerwaithandle;
    private WaitHandle m_waihandle;

    private void RegisterTimeOut(WaitHandle handle)
    {
        m_waihandle = handle;
        m_registerwaithandle =
            ThreadPool.RegisterWaitForSingleObject(handle, TimeOutCallback, m_httpwebrequest, TIMEOUT_TIME, true);
    }
    private void UnRegisterTimeOut()
    {
        if (m_registerwaithandle != null && m_waihandle != null)
        {
            m_registerwaithandle.Unregister(m_waihandle);
        }
    }
    private void TimeOutCallback(object state,bool timeout)
    {
        lock (lock_obj)
        {
            if (timeout)
            {
                OnFailed(enDownloadErrorState.TimeOut);
            }
            UnRegisterTimeOut();
        }
    }

    #endregion


}













































