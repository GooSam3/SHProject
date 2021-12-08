using UnityEngine;
using UnityEditor;
using System;
using System.Diagnostics;
using MS.Shell.Editor;
using System.Text;
using System.Globalization;

public class ProtocolFetcher
{
	/// <summary>
	/// WEB서버용 프로토콜 가져오기
	/// </summary>
	[MenuItem("ZGame/Protocols/[WEB] Sync ProtocolFiles")]
	public static void Web_SyncProtocolFile()
	{
		string workingDirectory = Application.dataPath + "/../Misc";
		string BatFilePath = workingDirectory + "/Sync_WEB_ProtocolFiles.bat";

		SyncProtocolFile(workingDirectory, BatFilePath);
	}

	/// <summary>
	/// MMO용 프로토콜 가져오기
	/// </summary>
	[MenuItem("ZGame/Protocols/[MMO] Sync ProtocolFiles")]
	public static void Mmo_SyncProtocolFile()
	{
		string workingDirectory = Application.dataPath + "/../Misc";
		string BatFilePath = workingDirectory + "/Sync_MMO_ProtocolFiles.bat";

		SyncProtocolFile(workingDirectory, BatFilePath);
	}

	/// <summary>
	/// WEB, MMO 프로토콜 순차적으로 가져오기
	/// </summary>
	[MenuItem( "ZGame/Protocols/[WEB]+[MMO] Sync ProtocolFiles" )]
	public static void WebAndMmo_SyncProtocolFile()
	{
		Web_SyncProtocolFile();
		Mmo_SyncProtocolFile();
	}

	public static void SyncProtocolFile(string workingDir, string batchFilePath)
	{
		Process process = new Process();

		string workingDirectory = workingDir;
		string BatFilePath = batchFilePath;

		process.StartInfo.UseShellExecute = false;
		process.StartInfo.RedirectStandardOutput = true;
		process.StartInfo.CreateNoWindow = false;
		process.StartInfo.FileName = batchFilePath;
		process.StartInfo.StandardOutputEncoding = Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage);
		//process.StartInfo.Arguments = arguments;
		process.StartInfo.WorkingDirectory = workingDir;


		int exitCode = -1;
		string output = null;

		try
		{
			process.Start();
			output = process.StandardOutput.ReadToEnd();
			process.WaitForExit();
		}
		catch (Exception e)
		{
			EditorUtility.DisplayDialog("[에러] Run Exception", e.ToString(), "OK");
		}
		finally
		{
			exitCode = process.ExitCode;

			process.Dispose();
			process = null;
		}

		EditorUtility.DisplayDialog("Success", output.ToString(), "OK");

		AssetDatabase.Refresh();
	}
}