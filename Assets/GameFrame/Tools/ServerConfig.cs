using System;
using System.IO;
using System.Security;
using GameFrame;
using Mono.Xml;
using UnityEngine;

public class ServerConfig : Singleton<ServerConfig>
{
	private const string m_filePath = "config/urlconfig.xml";
	public string[] UpdateServer { get; private set; }
	public string AndroidServerlistUrl { get; private set; }
	public string IosServerlistUrl { get; private set; }
	public string UpdateAppUrl { get; private set; }
	public override void Init()
	{
		base.Init();
		
	}

	public void Read()
	{
		try
		{
			string filepath = Platform.Path + m_filePath;
			if (!FileManager.IsFileExist(filepath))
			{
				return;
			}
			FileStream fileStream = new FileStream(filepath, FileMode.Open, FileAccess.Read);
			string xmlStr = fileStream.ToString();
		    SecurityParser parse = new SecurityParser();
			parse.LoadXml(xmlStr);
			SecurityElement se = parse.ToXml();
			foreach (SecurityElement element in se.Children)
			{
				if (element.Tag.Equals("root"))
				{
					UpdateServer = element.Attribute("UpdateServer").ToString().Split('|');
					AndroidServerlistUrl = element.Attribute("AndroidServerListUrl").ToString();
					IosServerlistUrl = element.Attribute("IosServerListUrl").ToString();
					UpdateAppUrl = element.Attribute("UpdateAppUrl").ToString();
				}
			}
		}
		catch (Exception e)	
		{
			Debug.LogError(e.Message);
		}
	}
	
	
}
