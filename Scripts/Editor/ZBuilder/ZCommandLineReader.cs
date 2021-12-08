using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ZCommandLineReader
{
	//Config
	private const string CUSTOM_ARGS_PREFIX = "-RunArgs:";
	private const char CUSTOM_ARGS_SEPARATOR = ';';

	public static string[] GetCommandLineArgs()
	{
		return Environment.GetCommandLineArgs();
	}

	public static string GetCommandLine()
	{
		string[] args = GetCommandLineArgs();

		if (args.Length > 0)
		{
			return string.Join(" ", args);
		}
		else
		{
			Debug.LogError("CommandLineReader.cs - GetCommandLine() - Can't find any command line arguments!");
			return "";
		}
	}

	public static bool HasCustomArgs(string customArgsPrefix)
	{
		string[] commandLineArgs = GetCommandLineArgs();
		try
		{
			commandLineArgs.Where(row => row.Contains(customArgsPrefix)).Single();
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	/// <summary>커스텀 인자값에 해당하는 라인에서 데이터 수집</summary>
	/// <param name="customArgsPrefix">커스텀 인자 파싱 시작용 값</param>
	/// <returns>커스텀 인자 존재 여부</returns>
	public static bool TryGetCustomArguments(string customArgsPrefix, out Dictionary<string, string> customArgsDict)
	{
		customArgsDict = new Dictionary<string, string>();
		string[] commandLineArgs = GetCommandLineArgs();
		string[] customArgs;
		string[] customArgBuffer;
		string customArgsStr = "";

		try
		{
			customArgsStr = commandLineArgs.Where(row => row.Contains(customArgsPrefix)).Single();
		}
		catch (Exception)
		{
			return false;
		}

		customArgsStr = customArgsStr.Replace(customArgsPrefix, "");
		customArgs = customArgsStr.Split(CUSTOM_ARGS_SEPARATOR);

		foreach (string customArg in customArgs)
		{
			customArgBuffer = customArg.Split('=');
			if (customArgBuffer.Length == 2)
			{
				customArgsDict.Add(customArgBuffer[0], customArgBuffer[1]);
			}
			else
			{
				Debug.LogWarning($"GetCustomArguments() - 인자값이 정해진 포맷에 맞지 않습니다. arg[{customArg}].  ex) args=value");
			}
		}

		return true;
	}

	public static Dictionary<string, string> GetCustomArguments()
	{
		Dictionary<string, string> customArgsDict = new Dictionary<string, string>();
		string[] commandLineArgs = GetCommandLineArgs();
		string[] customArgs;
		string[] customArgBuffer;
		string customArgsStr = "";

		try
		{
			customArgsStr = commandLineArgs.Where(row => row.Contains(CUSTOM_ARGS_PREFIX)).Single();
		}
		catch (Exception e)
		{
			Debug.LogError("CommandLineReader.cs - GetCustomArguments() - Can't retrieve any custom arguments in the command line [" + commandLineArgs + "]. Exception: " + e);
			return customArgsDict;
		}

		customArgsStr = customArgsStr.Replace(CUSTOM_ARGS_PREFIX, "");
		customArgs = customArgsStr.Split(CUSTOM_ARGS_SEPARATOR);

		foreach (string customArg in customArgs)
		{
			customArgBuffer = customArg.Split('=');
			if (customArgBuffer.Length == 2)
			{
				customArgsDict.Add(customArgBuffer[0], customArgBuffer[1]);
			}
			else
			{
				Debug.LogWarning("CommandLineReader.cs - GetCustomArguments() - The custom argument [" + customArg + "] seem to be malformed.");
			}
		}

		return customArgsDict;
	}

	public static string GetCustomArgument(string argumentName)
	{
		Dictionary<string, string> customArgsDict = GetCustomArguments();

		if (customArgsDict.ContainsKey(argumentName))
		{
			return customArgsDict[argumentName];
		}
		else
		{
			Debug.LogWarning("CommandLineReader.cs - GetCustomArgument() - Can't retrieve any custom argument named [" + argumentName + "] in the command line [" + GetCommandLine() + "].");
			return "";
		}
	}
}