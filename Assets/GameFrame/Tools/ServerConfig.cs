using System;
using System.IO;
using System.Security;
using GameFrame;
using Mono.Xml;
using UnityEngine;

public class ServerConfig : Singleton<ServerConfig>
{
	class LocalHandler : SmallXmlParser.IContentHandler
	{
		public void OnStartParsing(SmallXmlParser parser) { }
		public void OnEndParsing(SmallXmlParser parser) { }
		public void OnStartElement(string name, SmallXmlParser.IAttrList attrs)
		{
			ServerConfig local = Instance;

			var values = attrs.Values;
			switch (name)
			{
                    
				case "UpdateServer":
					local.UpdateServer = values[0].Split('|');
					break;
				case "AndroidServerListUrl":
					local.AndroidServerlistUrl = values[0];
					break;
				case "IosServerListUrl":
					local.IosServerlistUrl = values[0];
					break;
				case "UpdateAppUrl":
					local.UpdateAppUrl = values[0];
					break;
				default:
					break;
			}
		}
		public void OnEndElement(string name) { }
		public void OnChars(string s) { }
		public void OnIgnorableWhitespace(string s) { }
		public void OnProcessingInstruction(string name, string text) { }
	}
	private const string m_FilePath = "config/urlconfig.xml";
	public string[] UpdateServer { get; private set; }
	public string AndroidServerlistUrl { get; private set; }
	public string IosServerlistUrl { get; private set; }
	public string UpdateAppUrl { get; private set; }

	public void Load()
	{
		try
		{
			string path = Platform.Path+m_FilePath;
			if (!File.Exists(path))
			{
				return;
			}
			using (var stream = new FileStream(path, FileMode.Open))
			using (var reader = new System.IO.StreamReader(stream))
			{
				var parser = new SmallXmlParser();
				var handler = new LocalHandler();
				parser.Parse(reader, handler);
			}
		}
		catch (System.Exception ex)
		{
			UnityEngine.Debug.LogError(ex);
		}
	}
	
	
	
}
