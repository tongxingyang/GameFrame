﻿using System;
using System.IO;
using System.Net;
using GameFrame.Update;

namespace GameFrame
{
	public class DownloadTask
	{
		public static long ChunkSize = 4096L;
		public static int TIMEOUT = 2 * 60 * 1000;

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
				HttpWebRequest httpWebRequest = (HttpWebRequest) WebRequest.Create(new Uri(m_url));
				httpWebRequest.Timeout = TIMEOUT;
				httpWebRequest.KeepAlive = false;
				HttpWebResponse httpWebResponse = (HttpWebResponse) httpWebRequest.GetResponse();
				byte[] buffer = new byte[ChunkSize];
				using (FileStream fs = new FileStream(m_filepath,FileMode.OpenOrCreate,FileAccess.ReadWrite))
				{
					using (Stream stream = httpWebResponse.GetResponseStream())
					{
						if (stream != null)
						{
							int size = stream.Read(buffer, 0, buffer.Length);
							while (size>0)
							{
								fs.Write(buffer,0,size);
								size = stream.Read(buffer, 0, buffer.Length);
							}
						}
					}
				}
				httpWebRequest.Abort();
				httpWebRequest = null;
				httpWebResponse.Close();
				httpWebResponse = null;
				string downloadmd5 = String.Empty;
				if (FileManager.IsFileExist(m_filepath))
				{
					downloadmd5 = MD5Util.ComputeFileHash(m_filepath);
				}
				string servermd5 = Singleton<UpdateManager>.GetInstance().newmd5Table[m_filename].md5;

				if (downloadmd5.Equals(servermd5))
				{
					GameFrame.Update.FileInfo fileInfo = Singleton<UpdateManager>.Instance.newmd5Table[m_filename];
					Singleton<UpdateManager>.Instance.DownloadSize +=fileInfo.size;
					Singleton<UpdateManager>.Instance.AppendHasUpdateFile(m_filename,downloadmd5,fileInfo.size);
				}
				else
				{
					Singleton<UpdateManager>.GetInstance().m_redownloadList.Add(m_filename);
				}
				callback();
			}
			catch (Exception e)
			{
				Singleton<UpdateManager>.GetInstance().m_redownloadList.Add(m_filename);
				callback();
			}
		}
	}

}

