//#define ZLOG_DIALOGS // 에디터에서 플레이도중 심각한 에러시, 다이얼로그창 표시 기능 사용 여부
//using FlatBuffers;
using System.Collections.Generic;
using UnityEngine;

[System.Flags]
public enum ZLogChannel : uint
{
	Default = 1 << 0,
	System = 1 << 1,
	WebSocket = 1 << 2,
	MMO = 1 << 3,
	UI = 1 << 4,
	Loading = 1 << 5,
	Sound = 1 << 6,
    Camera = 1 << 7,
    PostProcess = 1 << 8,
    Map = 1 << 9,
    Entity = 1 << 10,
    Temple = 1 << 11,
    Party = 1 << 12,

	Model = 1 << 15,
	IAP = 1 << 16,
	MWebSocket = 1 << 17,
	ChatServer = 1 << 18,

	Quest = 1 << 20,
	Event = 1 << 21,

	Raid = 1 << 27,
	Stat = 1 << 28,
	Skill = 1 << 29,
	Pet = 1 << 30,
}

public enum ZLogLevel
{
	Info,
	Warning,
	Error,
	Fatal,
}

public class ZLog
{
	/// <summary> 출력될 로그용 채널들 </summary>
	public static ZLogChannel Channels { get; private set; } = (ZLogChannel)~0u;

	/// <summary>
	/// 컬러표 참고 : https://docs.unity3d.com/2018.3/Documentation/Manual/StyledText.html
	/// </summary>
	private static Dictionary<ZLogChannel, string> ChannelColors = new Dictionary<ZLogChannel, string>
	{
		{ ZLogChannel.Default, "white" },
		{ ZLogChannel.System, "lightblue" },
		{ ZLogChannel.WebSocket, "orange" },
		{ ZLogChannel.MMO, "orange" },
		{ ZLogChannel.UI, "teal" },
		{ ZLogChannel.Loading, "lightblue" },
		{ ZLogChannel.Sound, "silver" },
        { ZLogChannel.Camera, "silver" },
        { ZLogChannel.PostProcess, "red"},
        { ZLogChannel.Map, "red"},

        { ZLogChannel.Entity, "yellow"},
        { ZLogChannel.Temple, "orange"},
        { ZLogChannel.Party, "blue"},

        { ZLogChannel.Model, "red" },
		{ ZLogChannel.IAP, "green" },
		{ ZLogChannel.MWebSocket, "orange" },

		{ ZLogChannel.Raid, "yellow" },
		{ ZLogChannel.Stat, "magenta" },
		{ ZLogChannel.Skill, "purple" },
		{ ZLogChannel.Pet, "purple" },

		{ ZLogChannel.Quest, "green" },		
	};

	private static string HR = "\n\n-------------------------------------------------------------------------";


	#region ========:: Management Channels ::========

	public static void ResetChannels()
	{
		Channels = (ZLogChannel)~0u; //전체
	}

	public static void SetChannels(ZLogChannel channels)
	{
		ZLog.Channels = channels;
	}

	public static void AddChannel(ZLogChannel channel)
	{
		Channels |= channel;
	}

	public static void RemoveChannel(ZLogChannel channel)
	{
		Channels &= ~channel;
	}

	public static void ToggleChannel(ZLogChannel channel)
	{
		Channels ^= channel;
	}

	public static bool IsChannelActive(ZLogChannel channel)
	{
		return (Channels & channel) == channel;
	}

	#endregion

	#region ========:: Channel Version Log Functions ::========

	/// <summary>
	/// 
	/// </summary>
	/// <param name="channel"></param>
	/// <param name="message"></param>
	[System.Diagnostics.Conditional("ZLOG")]
	public static void Log(ZLogChannel channel, string message)
	{
		FinalLog(channel, ZLogLevel.Info, message);
	}

	[System.Diagnostics.Conditional("ZLOG")]
	public static void Log(ZLogChannel channel, string message, Object context)
	{
		FinalLog(channel, ZLogLevel.Info, message, context);
	}

	[System.Diagnostics.Conditional("ZLOG")]
	public static void LogWarn(ZLogChannel channel, string message)
	{
		FinalLog(channel, ZLogLevel.Warning, message);
	}

	[System.Diagnostics.Conditional("ZLOG")]
	public static void LogWarn(ZLogChannel channel, string message, Object context)
	{
		FinalLog(channel, ZLogLevel.Warning, message, context);
	}

	[System.Diagnostics.Conditional("ZLOG")]
	public static void LogError(ZLogChannel channel, string message)
	{
		FinalLog(channel, ZLogLevel.Error, message);
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="channel"></param>
	/// <param name="logLevel"></param>
	/// <param name="message"></param>
	[System.Diagnostics.Conditional("ZLOG")]
	public static void Log(ZLogChannel channel, ZLogLevel logLevel, string message)
	{
		FinalLog(channel, logLevel, message);
	}

	[System.Diagnostics.Conditional("ZLOG")]
	public static void Log(ZLogChannel channel, ZLogLevel logLevel, string message, Object context)
	{
		FinalLog(channel, logLevel, message, context);
	}

	[System.Diagnostics.Conditional("ZLOG")]
	public static void Log(string message, UnityEngine.Object context)
	{
		FinalLog(0, ZLogLevel.Info, message, context);
	}

	/// <summary> 유니티 Debug를 사용한 최종 로그 출력 </summary>
	private static void FinalLog(ZLogChannel channel, ZLogLevel logLevel, string message, Object context = null)
	{
		if ((Channels & channel) != channel) //허용된 채널인지 검사
			return;

#if UNITY_EDITOR && ZLOG_DIALOGS
		if (logLevel == ZLogLevel.Fatal)
		{
			var ignore = UnityEditor.EditorUtility.DisplayDialog("체크해야할 에러", message, "Ignore", "Break");
			if (!ignore)
			{
				Debug.Break();
			}
		}
#endif

		// 최종 로그 조건에 맞게 생성
		if (channel > 0)
		{
			string finalMsg = ContructLogString(channel, logLevel, message) + HR;

			switch (logLevel)
			{
				case ZLogLevel.Info:
					Debug.Log(finalMsg, context);
					break;

				case ZLogLevel.Warning:
					Debug.LogWarning(finalMsg, context);
					break;

				case ZLogLevel.Error:
				case ZLogLevel.Fatal:
					Debug.LogError(finalMsg, context);
					break;
			}
		}
		else
		{
			Debug.Log(message + HR, context);
		}
	}

	private static string ContructLogString(ZLogChannel channel, ZLogLevel logLevel, string message, bool shouldColor = true)
	{
		if (!ChannelColors.TryGetValue(channel, out var channelColor))
		{
			channelColor = "grey";
		}

		if (shouldColor)
		{
			return $"<b><color={channelColor}>[{channel}] </color></b> {message}";
			//return $"<b><color={channelColor}>[{channel}] </color></b> <color={2}>{message}</color>";
		}

		return $"[{channel}] {message}";
	}

	public static string ReplaceColorLog(string context, Color? color = null)
	{
		if (color.HasValue)
		{
			context = $"<color=#{ColorUtility.ToHtmlStringRGBA(color.Value)}>{context}</color>";
		}

		return context;
	}

	#endregion

	#region :: Profiler ::

	static Dictionary<string, System.Diagnostics.Stopwatch> ChildSamples = new Dictionary<string, System.Diagnostics.Stopwatch>();

	[System.Diagnostics.Conditional("ZBENCH")]
	[System.Diagnostics.Conditional("ZLOG")]
	public static void BeginProfile(string keyName, int stackOffset = 0)
	{
		if (!ChildSamples.TryGetValue(keyName, out var watch))
		{
			watch = new System.Diagnostics.Stopwatch();
			ChildSamples.Add(keyName, watch);
		}

		watch.Stop();
		watch.Restart();
	}

	[System.Diagnostics.Conditional("ZBENCH")]
	[System.Diagnostics.Conditional("ZLOG")]
	public static void EndProfile(string key)
	{
		if (string.IsNullOrEmpty(key))
			return;

		if (ChildSamples.TryGetValue(key, out var watch))
		{
			watch.Stop();

			PrintWatch(key, watch);
		}
	}

	[System.Diagnostics.Conditional("ZBENCH")]
	[System.Diagnostics.Conditional("ZLOG")]
	public static void ResetProfile()
	{
		ChildSamples.Clear();
	}

	/// <summary> 현재 전체 정보 로그 뿌리기 </summary>
	[System.Diagnostics.Conditional("ZBENCH")]
	[System.Diagnostics.Conditional("ZLOG")]
	public static void PrintRecoredProfileData(bool printIsRunning = true)
	{
		string logStr = "=======[ CustomSampler Status ]=======\n";

		foreach (var watch in ChildSamples)
		{
			// 동작중인거 skip할지..
			if (watch.Value.IsRunning && !printIsRunning)
				continue;

			string statusStr = $"{watch.Key} : {watch.Value.ElapsedMilliseconds} ms, {watch.Value.ElapsedTicks} tick, Running: {watch.Value.IsRunning}\n";

			if (watch.Value.IsRunning)
			{
				logStr += ReplaceColorLog(statusStr, Color.green);
			}
			else
			{
				logStr += ReplaceColorLog(statusStr, Color.red);
			}
		}

		Debug.LogWarning(logStr);
	}

	[System.Diagnostics.Conditional("ZBENCH")]
	[System.Diagnostics.Conditional("ZLOG")]
	public static void PrintWatch(string key, System.Diagnostics.Stopwatch watch)
	{
		// 동작중인거 skip할지..

		string statusStr = $"[Bench] {key} : {watch.ElapsedMilliseconds} ms, {watch.ElapsedTicks} tick, Running: {watch.IsRunning}";

		if (watch.IsRunning)
		{
			statusStr = ReplaceColorLog(statusStr, Color.red);
		}
		else
		{
			statusStr = ReplaceColorLog(statusStr, Color.green);
		}

		Debug.LogWarning(statusStr);
	}

	#endregion
}