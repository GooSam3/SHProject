using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class Pane_MapToolStageSelect : EditorPaneBase<MapToolEditor>
{
	/// <summary> 프로젝트 전체 씬 경로들 </summary>
	private List<string> mAllScenePaths = new List<string>();
	/// <summary> Key : SceneName, Value : ScenePath</summary>
	private Dictionary<string, string> mAllScenePathDic = new Dictionary<string, string>();

	public Pane_MapToolStageSelect(MapToolEditor _owner) : base(_owner)
	{
		// 프로젝트내에 존재하는 모든 Scene을 caching해둔다.
		this.mAllScenePaths = this.Owner.GetAllMainScenePaths();
		this.mAllScenePathDic.Clear();
		this.mAllScenePaths.ForEach(scenePath =>
		{
			this.mAllScenePathDic.Add(Path.GetFileNameWithoutExtension(scenePath), scenePath);
		});
	}

	public override void OnEnable()
	{
	}

	public override void OnDisable()
	{
	}

	public override void DrawGUI()
	{
		DrawStageSelect();
	}

	Vector2 scrollMapSelPos = Vector2.zero;
	void DrawStageSelect()
	{
		MapToolUtil.HelpBox("편집하고자 하는 스테이지를 선택해주세요.", MessageType.Info, true);

		scrollMapSelPos = EditorGUILayout.BeginScrollView(scrollMapSelPos);

		GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
		buttonStyle.alignment = TextAnchor.MiddleLeft;
		buttonStyle.richText = true;

		foreach (var stageTable in this.Owner.TableDB.StageTable.DicTable.Values)
		{
			string resourceFileName = stageTable.ResourceFileName;

			//테스트
			//resourceFileName = "TownMap101_Sub1";

			if (string.IsNullOrEmpty(resourceFileName))
				continue;

			GUI.enabled = mAllScenePathDic.ContainsKey(resourceFileName);

			using (new GUILayout.HorizontalScope())
			{
				if (!GUI.enabled)
				{
					GUILayout.Label($"씬이 존재하지 않음!", ZGUIStyles.WarningLabel, GUILayout.Width(120));
				}

				if (GUILayout.Button($"[{stageTable.StageID}], <color=white>{resourceFileName}</color>, {stageTable.StageTextID}", buttonStyle, GUILayout.Height(24f)))
				{
					if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
					{
						string mainScenePath = mAllScenePathDic[resourceFileName];
						this.Owner.LoadAndSetupScene(mainScenePath, stageTable, FindSubScenes(mainScenePath));
						this.Owner.ChangePane(nameof(Pane_MapToolEdit));
					}
				}
			}
		}

		EditorGUILayout.EndScrollView();

		GUILayout.Space(30f);
		GUI.enabled = true;
		if (GUILayout.Button("전체 로딩 후 저장 (Refresh)"))
		{

			int totalCnt = this.Owner.TableDB.StageTable.DicTable.Count;
			int saveCnt = 0;

			foreach (var stageTable in this.Owner.TableDB.StageTable.DicTable.Values)
			{
				if (string.IsNullOrEmpty(stageTable.ResourceFileName))
					continue;

				GUI.enabled = mAllScenePathDic.ContainsKey(stageTable.ResourceFileName);

				if (!GUI.enabled)
					continue;

				this.Owner.LoadAndSetupScene(mAllScenePathDic[stageTable.ResourceFileName], stageTable);
				this.Owner.SaveToMapData();

				EditorSceneManager.SaveScene(this.Owner.CurSceneInfo.Scene);
				EditorSceneManager.CloseScene(this.Owner.CurSceneInfo.Scene, true);

				saveCnt++;

				EditorUtility.DisplayProgressBar("맵 저장 중...", $"{string.Format("{0}%", ((float)saveCnt / (float)totalCnt) * 100f)}", (float)saveCnt / (float)totalCnt);
			}

			EditorUtility.ClearProgressBar();
		}
		GUI.enabled = true;
	}


	///// <summary>
	///// Scene리스트 보여주고, 로드절차까지 진행
	///// </summary>
	//void DrawMapSelect()
	//{
	//	MapToolUtil.HelpBox("편집하고자 하는 Scene을 선택해주세요.", MessageType.Info, true);

	//	scrollMapSelPos = EditorGUILayout.BeginScrollView(scrollMapSelPos);

	//	foreach (var scenePath in mAllScenePathDic.Values)
	//	{
	//		if (GUILayout.Button(scenePath))
	//		{
	//			if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
	//			{
	//				//Scene activeScene = EditorSceneManager.GetActiveScene();
	//				//EditorSceneManager.SaveScene(activeScene);
	//			}

	//			this.Owner.LoadAndSetupScene(scenePath, null);
	//		}
	//	}

	//	EditorGUILayout.EndScrollView();
	//}

	public List<string> FindSubScenes(string _mainScenePath)
	{
		//==== 서브씬 로드
		string mainSceneName = Path.GetFileNameWithoutExtension(_mainScenePath);
        string[] split = mainSceneName.Split('_');
        int index = split.Length - 1;
        string findNameRule = mainSceneName.Replace("_" + mainSceneName.Split('_')[index], "") + "_Sub";        
		List<string> subScenePaths = new List<string>();

		// 찾기 규칙에 맞는 씬은 모두 수집한다.
		foreach (string existSceneName in mAllScenePaths)
		{
			string scName = Path.GetFileNameWithoutExtension(existSceneName);
			if (!string.IsNullOrEmpty(scName) && scName.StartsWith(findNameRule))
			{
				subScenePaths.Add(existSceneName);
			}
		}

		foreach (string subName in subScenePaths)
		{
			EditorSceneManager.OpenScene(subName, OpenSceneMode.Additive);
		}

		return subScenePaths;
	}
}
