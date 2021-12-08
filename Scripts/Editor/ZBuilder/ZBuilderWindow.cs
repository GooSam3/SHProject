using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using System.Reflection;

/// <summary>
/// Builder TEST
/// </summary>
public class ZBuilderWindow : EditorWindow
{
	//[MenuItem("ZGame/Build Window %&z", false, priority: 11)]
	public static void OnOpenFromMenu()
	{
		EditorWindow.GetWindow<ZBuilderWindow>("ZBuilderWindow");
	}

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
	}

	Vector2 sceneScrollPos;
	private void OnGUI()
	{
		GUILayout.Label($"EditorUserBuildSettings.selectedBuildTargetGroup : {EditorUserBuildSettings.selectedBuildTargetGroup}");
		GUILayout.Label($"EditorUserBuildSettings.activeBuildTarget : {EditorUserBuildSettings.activeBuildTarget}");
				
		if (GUILayout.Button("Player Settings..."))
		{
			ShowPlayerSettings();
		}


		GUI.enabled = false;
		EditorGUILayout.EnumFlagsField(EditorUserBuildSettings.selectedBuildTargetGroup);
		EditorGUILayout.EnumFlagsField(EditorUserBuildSettings.activeBuildTarget);
		GUI.enabled = true;

		UnityEditor.PlayerSettings.bundleVersion = EditorGUILayout.TextField("Game Version", UnityEditor.PlayerSettings.bundleVersion, EditorStyles.textField).Trim();

		ZGUIStyles.Separator();
		
		EditorGUILayout.BeginHorizontal();
		{
			//GUILayout.Label($"Revision. {CurBuildInfo.Revision}", EditorStyles.boldLabel);

			//if (GUILayout.Button("Get Revision"))
			//{
			//	EditorUtil.GetSVNRevision(Application.dataPath + "/../", EditorUtil.eSVNRevsionType.HEAD, ref HeadRevision);
			//	EditorUtil.GetSVNRevision(Application.dataPath + "/../", EditorUtil.eSVNRevsionType.BASE, ref CurBuildInfo.Revision);
			//}
			//if (CurBuildInfo.Revision != HeadRevision)
			//{
			//	GUIStyle style = new GUIStyle();
			//	style.normal.textColor = Color.red;
			//	style.fontStyle = FontStyle.BoldAndItalic;
			//	EditorGUILayout.LabelField($"SVN Head Revision", HeadRevision.ToString(), style);
			//	if (GUILayout.Button("Update Svn"))
			//	{
			//		EditorUtil.UpdateSVNInfo(Application.dataPath + "/../", () => {
			//			bUpdateRevisionInfo = true;
			//		});

			//	}
			//}
			//if (bUpdateRevisionInfo)
			//	UpdateRevisionInfo();
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.LabelField($"BuildOptions: {GetBuildPlayerOptions().options}");

		GUILayout.Space(10f);
		GUILayout.FlexibleSpace();

		Draw_NTSDKConfig();
		

		if (GUILayout.Button("Build", GUILayout.ExpandWidth(true), GUILayout.Height(50f)))
		{
		}
	}
	
	static BuildPlayerOptions GetBuildPlayerOptions(bool askForLocation = false, BuildPlayerOptions defaultOptions = new BuildPlayerOptions())
	{
		// Get static internal "GetBuildPlayerOptionsInternal" method
		MethodInfo method = typeof(BuildPlayerWindow).GetMethod(
			"GetBuildPlayerOptionsInternal",
			BindingFlags.NonPublic | BindingFlags.Static);

		// invoke internal method
		return (BuildPlayerOptions)method.Invoke(
			null,
			new object[] { askForLocation, defaultOptions });
	}

	/// <summary> NTSDK에 만들어둔 ConfigList 보기 </summary>
	private void Draw_NTSDKConfig()
	{
		if (null == NTCore.ConfigListData.Instance)
			return;

		var configListData = NTCore.ConfigListData.Instance;

		using (new EditorGUILayout.VerticalScope("NTSDK Config"))
		{
			GUILayout.Label($"SelectedIdx: {configListData.SelectedIdx}");

			foreach (var pair in configListData.SavedConfigPreset)
			{
				if (GUILayout.Button(pair.Key))
				{
					//강제로 NTSDK 설정 변경
					configListData.ChangeConfig(pair.Key);
				}

				for (int i = 0; i < pair.Value.Length; i++)
				{
					GUILayout.Label($"Key: {pair.Value[i].Key}, Value: {pair.Value[i].Value}");
				}				
			}
		}
	}

	public static void ShowPlayerSettings()
	{
		SettingsService.OpenProjectSettings("Project/Player");
	}
}