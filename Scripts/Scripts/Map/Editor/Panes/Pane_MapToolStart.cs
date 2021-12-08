using UnityEditor;
using UnityEngine;

public class Pane_MapToolStart : EditorPaneBase<MapToolEditor>
{
	public Pane_MapToolStart(MapToolEditor _owner) : base(_owner)
	{
	}

	public override void OnEnable()
	{
	}

	public override void OnDisable()
	{
	}

	public override void DrawGUI()
	{
		if (!Owner.CurSceneInfo.IsValidTable)
		{
			DrawExcelPath();

			if (!MapToolEditor.IsLoadedGameDB)
			{
				GUILayout.Space(20f);
				MapToolUtil.CenterLabel($"맵툴 편집이 불가능합니다. 아래 상황을 확인해주세요." +
					$"\n - [GameDB폴더 선택]버튼으로 경로 설정했는지 or 제대로 설정했는지 확인." +
					$"\n - [Excep Path] 올바른 GameDB Table경로 설정했다면, [테이블 읽어오기] 버튼 수행.", ZGUIStyles.WarningLabel, GUILayout.Height(65f));

				return;
			}
		}
	}

	public void DrawExcelPath()
	{
		EditorGUILayout.BeginVertical("window", GUILayout.Height(100f));
		{
			string gameTablesPath = EditorPrefs.GetString(MapToolDefines.Prefs_GameDBPath, string.Empty);
			if (string.IsNullOrEmpty(gameTablesPath))
			{
				EditorGUILayout.LabelField("Excel Path : Not Selected!!");
			}
			else
			{
				EditorGUILayout.LabelField($"Excel Path : {gameTablesPath}");
			}

			EditorGUILayout.Space();
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			{
				if (GUILayout.Button("GameDB폴더 선택", GUILayout.Height(40f)))
				{
					string path = EditorUtility.OpenFolderPanel("Select GameTable Folder", gameTablesPath, "");

					if (string.IsNullOrEmpty(path) == false)
					{
						gameTablesPath = path;
						EditorPrefs.SetString(MapToolDefines.Prefs_GameDBPath, path);
					}
				}

				using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(gameTablesPath)))
				{
					if (GUILayout.Button("테이블 읽어오기", GUILayout.Height(40f)))
					{
						if (this.Owner.LoadDBForMapTool(gameTablesPath))
						{
							this.Owner.ChangePane(nameof(Pane_MapToolStageSelect));
						}
					}
				}
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}
		EditorGUILayout.EndVertical();
	}
}
