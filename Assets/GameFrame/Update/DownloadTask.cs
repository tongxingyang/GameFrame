using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using GameFrameDebuger;
using UnityEngine;

namespace GameFrame
{
	public class DownloadTask
	{
		public static long ChunkSize = 4096L;
		public  static int TIMEOUT = 2 * 60 * 1000;

		private string m_url;
		private string m_filename;
		private string m_filepath;
		public Action callback;

		public DownloadTask(string url, string filename, string filepath)
		{
			m_url = url;
			m_filename = filename;
			m_filepath = filepath;
		}

		public void BeginDownload()
		{
			string directory = Path.GetDirectoryName(m_filepath);
			if (!string.IsNullOrEmpty(directory) && !FileManager.IsDirectoryExist(directory))
			{
				FileManager.CreateDirectory(directory);
			}
			try
			{
				if (FileManager.IsFileExist(m_filepath))
				{
					FileManager.DeleteFile(m_filepath);
				}
				HttpWebRequest httpWebRequest = (HttpWebRequest) HttpWebRequest.Create(new Uri(m_url));
				httpWebRequest.Timeout = TIMEOUT;
				httpWebRequest.KeepAlive = false;
				HttpWebResponse httpWebResponse = (HttpWebResponse) httpWebRequest.GetResponse();
				byte[] buffer = new byte[ChunkSize];
				using (FileStream fs = new FileStream(m_filepath,FileMode.OpenOrCreate,FileAccess.ReadWrite))
				{
					using (Stream stream = httpWebResponse.GetResponseStream())
					{
						int readTotalSize = 0;
						int size = stream.Read(buffer, 0, buffer.Length);
						while (size>0)
						{
							fs.Write(buffer,0,size);
							readTotalSize += size;
							size = stream.Read(buffer, 0, buffer.Length);
						}
					}
				}
				httpWebRequest.Abort();
				httpWebRequest = null;
				httpWebResponse.Close();
				httpWebResponse = null;
				string md5 = String.Empty;
				if (FileManager.IsFileExist(m_filepath))
				{
					md5 = MD5Util.ComputeFileHash(m_filepath);
				}
				string md5_ = Singleton<UpdateManager>.GetInstance().newmd5Table[m_filename].md5;

				if (md5.Equals(md5_))
				{
					Debuger.LogError("文件下载成功 文件名 "+m_filename);
					FileInfo fileInfo = Singleton<UpdateManager>.Instance.newmd5Table[m_filename];
					Singleton<UpdateManager>.Instance.DownloadSize +=
						Singleton<UpdateManager>.Instance.newmd5Table[m_filename].size;
					Singleton<UpdateManager>.Instance.AppendHasUpdateFile(m_filename,md5,Singleton<UpdateManager>.Instance.newmd5Table[m_filename].size);
				}
				else
				{
					Debuger.LogError("文件下载失败.......... 文件名 "+m_filename);
					Singleton<UpdateManager>.GetInstance().m_redownloadList.Add(m_filename);
				}
				callback();

			}
			catch (Exception e)
			{
				Debuger.Log("download file error = " + m_filename + ", ex = " + e.Message);
				Singleton<UpdateManager>.GetInstance().m_redownloadList.Add(m_filename);
				callback();
				Debuger.LogError(e.Message);
			}
		}
	}

}

