

using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using GameFrame;
using UnityEngine;
using UnityEditor;

public class LuaBundle
{
	const string tempLua = "/templua/";

//	[MenuItem("Locke/Lua/Build for Windows")]
	public static void Build_for_Windows()
	{
		BuildLuaBundles(BuildTarget.StandaloneWindows);
	}

//	[MenuItem("Locke/Lua/Build for IOS")]
	public static void Build_for_IOS()
	{
		BuildLuaBundles(BuildTarget.iOS);
	}

//	[MenuItem("Locke/Lua/Build for Android")]
	public static void Build_for_Android()
	{
		BuildLuaBundles(BuildTarget.Android);
	}

	/// <summary>
	/// ��ƽ̨build bundle
	/// </summary>
	/// <param name="target"></param>
	private static void BuildLuaBundles(BuildTarget target)
	{
		System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
		watch.Start();

		// delete streamingasset and recreate it.
		string streamPath = Application.streamingAssetsPath;
		if (Directory.Exists(streamPath))
		{
			Directory.Delete(streamPath, true);
		}
		Directory.CreateDirectory(streamPath);
		AssetDatabase.Refresh();

		List<AssetBundleBuild> buildList = new List<AssetBundleBuild>();

		HandleLuaBundle(ref buildList);

		string outputPath = Platform.AssetBundleExportPath + LuaConst.osDir + "/lua/";
		BuildAssetBundleOptions opt = BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.UncompressedAssetBundle;
		BuildPipeline.BuildAssetBundles(outputPath, buildList.ToArray(), opt, target);
		BuildFileIndex();

		string streamDir = Application.dataPath + tempLua;
		if (Directory.Exists(streamDir))
			Directory.Delete(streamDir, true);
		AssetDatabase.Refresh();

		watch.Stop();
		UnityEngine.Debug.Log(string.Format("build done ! take {0} ms.", watch.ElapsedMilliseconds));
	}

	static void AddBuildMap(ref List<AssetBundleBuild> buildList, string bundleName, string pattern, string path)
	{
		string[] files = Directory.GetFiles(path, pattern);
		if (files.Length == 0) return;

		for (int i = 0; i < files.Length; i++)
		{
			files[i] = files[i].Replace('\\', '/');
		}
		AssetBundleBuild build = new AssetBundleBuild();
		build.assetBundleName = bundleName;
		build.assetNames = files;
		buildList.Add(build);
	}


	static void HandleLuaBundle(ref List<AssetBundleBuild> buildList)
	{
		// �Ȱ�lua�ļ���������ʱĿ¼
		string tempLuaDir = Application.dataPath + tempLua;
		if (!Directory.Exists(tempLuaDir))
			Directory.CreateDirectory(tempLuaDir);

		string[] srcDirs = { Application.dataPath + "/Lua", Application.dataPath + "/ToLua/Lua" };
		for (int i = 0; i < srcDirs.Length; i++)
		{
			if (LuaConst.LuaByteMode)
			{
				string sourceDir = srcDirs[i];
				string[] files = Directory.GetFiles(sourceDir, "*.lua", SearchOption.AllDirectories);
				int len = sourceDir.Length;

				if (sourceDir[len - 1] == '/' || sourceDir[len - 1] == '\\')
				{
					--len;
				}
				for (int j = 0; j < files.Length; j++)
				{
					string str = files[j].Remove(0, len);
					string dest = tempLuaDir + str + ".bytes";
					string dir = Path.GetDirectoryName(dest);
					Directory.CreateDirectory(dir);
					EncodeLuaFile(files[j], dest);
				}
			}
			else
			{
				ToLuaMenu.CopyLuaBytesFiles(srcDirs[i], tempLuaDir);
			}
		}

		// ����ʱĿ¼��ÿ�����ļ��е�lua���ļ��д��
		string[] dirs = Directory.GetDirectories(tempLuaDir, "*", SearchOption.AllDirectories);
		for (int i = 0; i < dirs.Length; i++)
		{
			string name = dirs[i].Replace(tempLuaDir, string.Empty);
			name = name.Replace('\\', '_').Replace('/', '_');
			name = "lua/lua_" + name.ToLower() + ".unity3d";

			string path = "Assets" + dirs[i].Replace(Application.dataPath, "");
			AddBuildMap(ref buildList, name, "*.bytes", path);
		}
		// ����ʱĿ¼������lua��һ����
		AddBuildMap(ref buildList, "lua/lua" + ".unity3d", "*.bytes", "Assets" + tempLua);

		//-------------------------------�����Lua�ļ�----------------------------------
		string luaTargetPath = AppDataPath + "/StreamingAssets/lua/";
		for (int i = 0; i < srcDirs.Length; i++)
		{
			string luaDataPath = srcDirs[i].ToLower();
			List<string> paths = new List<string>();
			List<string> files = new List<string>();
			Recursive(luaDataPath, ref paths, ref files);
			foreach (string f in files)
			{
				if (f.EndsWith(".meta") || f.EndsWith(".lua"))
					continue;
				string newfile = f.Replace(luaDataPath, "");
				string destPath = Path.GetDirectoryName(luaTargetPath + newfile);
				if (!Directory.Exists(destPath))
					Directory.CreateDirectory(destPath);

				string destfile = destPath + "/" + Path.GetFileName(f);
				File.Copy(f, destfile, true);
			}
		}
		AssetDatabase.Refresh();
	}

	static void HandleLuaFile()
	{
		string resPath = AppDataPath + "/StreamingAssets/";
		string luaPath = resPath + "/lua/";

		//----------����Lua�ļ�----------------
		if (!Directory.Exists(luaPath))
		{
			Directory.CreateDirectory(luaPath);
		}
		string[] luaPaths = { AppDataPath + "/lua/",
                              AppDataPath + "/Tolua/Lua/" };

		for (int i = 0; i < luaPaths.Length; i++)
		{
			string luaDataPath = luaPaths[i].ToLower();
			List<string> paths = new List<string>();
			List<string> files = new List<string>();
			Recursive(luaDataPath, ref paths, ref files);
			int n = 0;
			foreach (string f in files)
			{
				if (f.EndsWith(".meta")) continue;
				string newfile = f.Replace(luaDataPath, "");
				string newpath = luaPath + newfile;
				string path = Path.GetDirectoryName(newpath);
				if (!Directory.Exists(path))
					Directory.CreateDirectory(path);

				if (File.Exists(newpath))
				{
					File.Delete(newpath);
				}
				if (LuaConst.LuaByteMode)
				{
					EncodeLuaFile(f, newpath);
				}
				else
				{
					File.Copy(f, newpath, true);
				}
				UpdateProgress(n++, files.Count, newpath);
			}
		}
		EditorUtility.ClearProgressBar();
		AssetDatabase.Refresh();
	}

	static string AppDataPath
	{
		get { return Application.dataPath.ToLower(); }
	}

	static void BuildFileIndex()
	{
		string resPath = AppDataPath + "/StreamingAssets/";
		///----------------------�����ļ��б�-----------------------
		string newFilePath = resPath + "/files.txt";
		if (File.Exists(newFilePath))
			File.Delete(newFilePath);

		List<string> pathList = new List<string>();
		List<string> fileList = new List<string>();
		Recursive(resPath, ref pathList, ref fileList);

		FileStream fs = new FileStream(newFilePath, FileMode.CreateNew);
		StreamWriter sw = new StreamWriter(fs);
		for (int i = 0; i < fileList.Count; i++)
		{
			string filePath = fileList[i];
			//string ext = Path.GetExtension(filePath);
			if (filePath.EndsWith(".meta") || filePath.Contains(".DS_Store"))
				continue;
// todo 
//			string md5 = Util.md5file(filePath);
//			string value = filePath.Replace(resPath, string.Empty);
//			sw.WriteLine(value + "|" + md5);
		}
		sw.Close();
		fs.Close();
	}

	static void Recursive(string path, ref List<string> pathList, ref List<string> fileList)
	{
		string[] names = Directory.GetFiles(path);
		string[] dirs = Directory.GetDirectories(path);
		foreach (string filename in names)
		{
			string ext = Path.GetExtension(filename);
			if (ext.Equals(".meta")) continue;
			fileList.Add(filename.Replace('\\', '/'));
		}
		foreach (string dir in dirs)
		{
			pathList.Add(dir.Replace('\\', '/'));
			Recursive(dir, ref pathList, ref fileList);
		}
	}

	static void UpdateProgress(int progress, int progressMax, string desc)
	{
		string title = "Processing...[" + progress + " - " + progressMax + "]";
		float value = (float)progress / (float)progressMax;
		EditorUtility.DisplayProgressBar(title, desc, value);
	}

	public static void EncodeLuaFile(string srcFile, string outFile)
	{
		if (!srcFile.ToLower().EndsWith(".lua"))
		{
			File.Copy(srcFile, outFile, true);
			return;
		}
		bool isWin = true;
		string luaexe = string.Empty;
		string args = string.Empty;
		string exedir = string.Empty;
		string currDir = Directory.GetCurrentDirectory();
		if (Application.platform == RuntimePlatform.WindowsEditor)
		{
			isWin = true;
			luaexe = "luajit.exe";
			args = "-b " + srcFile + " " + outFile;
			exedir = AppDataPath.Replace("assets", "") + "LuaEncoder/luajit/";
		}
		else if (Application.platform == RuntimePlatform.OSXEditor)
		{
			isWin = false;
			luaexe = "./luac";
			args = "-o " + outFile + " " + srcFile;
			exedir = AppDataPath.Replace("assets", "") + "LuaEncoder/luavm/";
		}
		Directory.SetCurrentDirectory(exedir);
		ProcessStartInfo info = new ProcessStartInfo();
		info.FileName = luaexe;
		info.Arguments = args;
		info.WindowStyle = ProcessWindowStyle.Hidden;
		info.ErrorDialog = true;
		info.UseShellExecute = isWin;
		UnityEngine.Debug.Log(info.FileName + " " + info.Arguments);

		Process pro = Process.Start(info);
		pro.WaitForExit();
		Directory.SetCurrentDirectory(currDir);
	}

}
