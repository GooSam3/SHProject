using UnityEditor;
using UnityEngine;

/// <summary>
/// 로그 표시 제어용 에디터 - 주석
/// </summary>
public class ZLogEditor : EditorWindow
{
	[MenuItem("ZGame/ZLogger Settings")]
	public static void ShowWindow()
	{
		GetWindow(typeof(ZLogEditor));
	}

	[SerializeField]
	private ZLogChannel loggerChannels = ZLog.Channels;

	private void OnGUI()
	{
		EditorGUI.BeginChangeCheck();

		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button("Clear all"))
		{
			loggerChannels = 0;
		}
		if (GUILayout.Button("Select all"))
		{
			loggerChannels = (ZLogChannel)~0u;
		}

		EditorGUILayout.EndHorizontal();

		GUILayout.Label("Click to toggle logging channels", EditorStyles.boldLabel);

		foreach (ZLogChannel channel in System.Enum.GetValues(typeof(ZLogChannel)))
		{
			EditorGUILayout.BeginHorizontal();
			GUILayout.Toggle((loggerChannels & channel) == channel, "", GUILayout.ExpandWidth(false));
			if (GUILayout.Button(channel.ToString()))
			{
				loggerChannels ^= channel;
			}
			EditorGUILayout.EndHorizontal();
		}

		// If the game is playing then update it live when changes are made
		if (EditorApplication.isPlaying && EditorGUI.EndChangeCheck())
		{
			ZLog.SetChannels(loggerChannels);
		}
	}

	// When the game starts update the logger instance with the users selections
	private void OnEnable()
	{
		ZLog.SetChannels(loggerChannels);
	}
}