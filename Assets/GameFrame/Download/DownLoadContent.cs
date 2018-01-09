using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Junfine.Debuger;
using UnityEngine;

namespace GameFrame
{
    public class DownLoadContent
    {
        public const int FILE_WRITE_SIZE = 32;//写入未下载的时间位数
        public const int BUFFER_SIZE = 1024;//缓冲区大小
        public const string TEMP_EXTENSION = ".download";//下载的文件扩展名
        public enDownloadState State;// 当前状态
        public string FullName;//文件名称
        public long LastTimeCompleteLength;// 上一次下载大小
        public byte[] Buffer; // 缓冲数组
        public DateTime LastModifTime;//上一次修改的时间
        public FileStream ContentFileStream;// 下载文件数据流
        public Stream ResponseStream { get; private set; }//响应文件流
        //临时文件下载名
        public string TempFullName
        {
            get { return FullName + TEMP_EXTENSION; }
        }
        private HttpWebResponse m_httpwebresponse;
        public HttpWebResponse WebResponse
        {
            get { return m_httpwebresponse; }
            set
            {
                if (value != null)
                {
                    m_httpwebresponse = value;
                    ResponseStream = m_httpwebresponse.GetResponseStream();
                }
            }
        }

        public DownLoadContent(string fillname)
        {
            FullName = fillname;
            State = enDownloadState.Downloading;
            Buffer = new byte[BUFFER_SIZE];
            OpenFile();
        }

        private void OpenFile()
        {
            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(FullName)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(FullName));
                }
                if (FileManager.IsFileExist(TempFullName))
                {
                    ContentFileStream = new FileStream(TempFullName,FileMode.Create,FileAccess.ReadWrite);
                    LastModifTime = DateTime.MinValue;
                    LastTimeCompleteLength = 0;
                }
                else
                {
                    ContentFileStream = new FileStream(TempFullName,FileMode.OpenOrCreate,FileAccess.ReadWrite);
                    LastTimeCompleteLength = ContentFileStream.Length;
                    if (ContentFileStream.Length > FILE_WRITE_SIZE && ReadLastModified(ref LastModifTime))
                    {
                        LastTimeCompleteLength = LastTimeCompleteLength - FILE_WRITE_SIZE;
                        ContentFileStream.Seek(LastTimeCompleteLength, SeekOrigin.Begin);
                    }
                    else
                    {
                        LastModifTime = DateTime.MinValue;
                        ContentFileStream.Seek(0, SeekOrigin.Begin);
                        LastTimeCompleteLength = 0;
                    }
                }
                //return;
            }
            catch (Exception e)
            {
                Debuger.LogError(e.Message);
                if (ContentFileStream != null)
                {
                    ContentFileStream.Close();
                    ContentFileStream = null;
                }
                throw;
            } 
        }
        public void Close()
        {
            if (WebResponse != null)
            {
                CloseFile(WebResponse.LastModified);
            }
            else
            {
                CloseFile();
            }
            if (ResponseStream != null)
            {
                ResponseStream.Close();
                ResponseStream = null;
            }
            if (WebResponse != null)
            {
                WebResponse.Close();
                WebResponse = null;
            }
        }

        public void CloseFile()
        {
            if (ContentFileStream != null)
            {
                ContentFileStream.Close();
                ContentFileStream = null;
            }
            if (FileManager.IsFileExist(TempFullName))
            {
                if (State == enDownloadState.Completed)
                {
                    if (FileManager.IsFileExist(FullName))
                    {
                        FileManager.DeleteFile(FullName);
                    }
                    File.Move(TempFullName,FullName);
                }
                else
                {
                    FileManager.DeleteFile(TempFullName);
                }
            }
        }

        public void CloseFile(DateTime dt)
        {
            if (State == enDownloadState.Failed)
            {
                WriteLastModified(dt);
            }
            if (ContentFileStream != null)
            {
                ContentFileStream.Close();
                ContentFileStream = null;
            }
            if (State == enDownloadState.Completed)
            {
                if (FileManager.IsFileExist(TempFullName))
                {
                    if (FileManager.IsFileExist(FullName))
                    {
                        FileManager.DeleteFile(FullName);
                    }
                    File.Move(TempFullName,FullName);
                }
            }
            
        }

        private void WriteLastModified(DateTime lasTime)
        {
            if (ContentFileStream != null)
            {
                string str = lasTime.Ticks.ToString("d" + FILE_WRITE_SIZE);
                byte[] bytes = Encoding.UTF8.GetBytes(str);
                ContentFileStream.Write(bytes,0,bytes.Length);
            }
        }

        private bool ReadLastModified(ref DateTime time)
        {
            if (ContentFileStream != null)
            {
                byte[] bytes = new byte[FILE_WRITE_SIZE];
                ContentFileStream.Seek(LastTimeCompleteLength - FILE_WRITE_SIZE, SeekOrigin.Begin);
                ContentFileStream.Read(bytes, 0, FILE_WRITE_SIZE);
                string str = Encoding.UTF8.GetString(bytes);
                time = new DateTime(long.Parse(str));
                return true;
            }
            return false;
        }
    }
}
