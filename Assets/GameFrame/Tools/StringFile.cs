using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GameFrame
{
	public class StringFile {
		public const string ErrorStr = "null = ";
		private Dictionary<string, string> strings;

		public string GetString(string key)
		{
			if(strings!=null && strings.ContainsKey(key))
			{
				return strings[key];
			}
			else
			{
				return ErrorStr + key;
			}
		}

		private void ParseString(StreamReader r)
		{
			if(r == null) return;
			strings = new Dictionary<string,string>();
			string line;
			while((line = r.ReadLine())!=null)
			{
				int pos = line.IndexOf(' ');
				string key = line.Substring(0,pos);
				string value = line.Substring(pos+1);
				if(value != null && value.Length>1 && value[0] == '"' && value[value.Length-1] == '"')
				{
					value = value.Substring(1,value.Length-2);
				}
				strings[key] = value;
			}
		}
		public bool ReadResources(string filename)
		{
			TextAsset text = Resources.Load<TextAsset>(filename);
			if(text!=null)
			{
				MemoryStream stream = new MemoryStream(text.bytes);
				StreamReader r = new StreamReader(stream);
				ParseString(r);

				return true;
			}
			return false;
		}
		public bool ReadFile(string filename)
		{
			if (!FileManager.IsFileExist(filename))
			{
				return false;
			}
			using (StreamReader r = File.OpenText(filename))
			{
				ParseString(r);
			}
			return true;
		}
	}

}

