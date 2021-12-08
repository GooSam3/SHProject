using GameDB;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UDebug = UnityEngine.Debug;

/// <summary>
/// 객체 충돌체 설정 도구 (ResourceTable 저장기능 포함)
/// </summary>
public class ColliderMeasureEditor : EditorWindow
{
	/// <summary>
	/// <see cref="GameDB.Resource_Table"/>
	/// </summary>
	public class Tool_ResourceTable : ToolTableBase<string, Resource_Table>
	{
		public Dictionary<string, List<Resource_Table>> DicTableEx { get; protected set; } = new Dictionary<string, List<Resource_Table>>();

		protected override string SheetName => nameof(Resource_Table);

		public Tool_ResourceTable(string _xlsxPath) : base(_xlsxPath) { }

		protected override void OnLoad(ExcelWorksheet _sheet, ExcelPackage _excelPackage)
		{
			Dictionary<string, int> indexes = GetColumnIndexes(_sheet);

			for (int rowIdx = 2; rowIdx <= _sheet.Dimension.Rows; ++rowIdx)
			{
				GameDB.Resource_Table tableData = new GameDB.Resource_Table()
				{
					ResourceID = _sheet.GetValue<uint>(rowIdx, indexes[nameof(Resource_Table.ResourceID)]),
					ResourceFile = _sheet.GetValue<string>(rowIdx, indexes[nameof(Resource_Table.ResourceFile)]),
					SizeX = _sheet.GetValue<float>(rowIdx, indexes[nameof(Resource_Table.SizeX)]),
					SizeY = _sheet.GetValue<float>(rowIdx, indexes[nameof(Resource_Table.SizeY)]),
					SizeZ = _sheet.GetValue<float>(rowIdx, indexes[nameof(Resource_Table.SizeZ)]),
				};

				if (!this.DicTableEx.TryGetValue(tableData.ResourceFile, out var list))
				{
					this.DicTableEx.Add(tableData.ResourceFile, new List<Resource_Table>());
				}

				this.DicTableEx[tableData.ResourceFile].Add(tableData);
			}
		}

		protected override void OnSave(ExcelWorksheet _sheet, ExcelPackage _excelPackage)
		{
			Dictionary<string, int> indexes = GetColumnIndexes(_sheet);

			for (int rowIdx = 2; rowIdx <= _sheet.Dimension.Rows; ++rowIdx)
			{
				var resId = _sheet.GetValue<uint>(rowIdx, indexes[nameof(GameDB.Resource_Table.ResourceID)]);
				var resName = _sheet.GetValue<string>(rowIdx, indexes[nameof(GameDB.Resource_Table.ResourceFile)]);
				if (!DicTableEx.TryGetValue(resName, out var table))
					continue;

				if (this.DicTableEx.TryGetValue(resName, out var list))
				{
					Resource_Table editorTable = list.Find((t) => t.ResourceID == resId);
					if (null != editorTable)
					{
						_sheet.SetValue(rowIdx, indexes[nameof(GameDB.Resource_Table.SizeX)], editorTable.SizeX.ToString("F2"));
						_sheet.SetValue(rowIdx, indexes[nameof(GameDB.Resource_Table.SizeY)], editorTable.SizeY.ToString("F2"));
						_sheet.SetValue(rowIdx, indexes[nameof(GameDB.Resource_Table.SizeZ)], editorTable.SizeZ.ToString("F2"));
					}
				}
			}
		}
	}

	static public ColliderMeasureEditor instance { get; private set; }

	private Tool_ResourceTable ResourceDB { get; set; } = new Tool_ResourceTable("");
	private bool mLoadedGameDB = false;

	public string SelectedPrefabName { get; private set; }
	private bool availableSave = false;

	[MenuItem("ZGame/Tools/ColliderMeasureEditor")]
	static void Initialize()
	{
		instance = (ColliderMeasureEditor)GetWindow(typeof(ColliderMeasureEditor));
		instance.Show();
	}

	private void OnEnable()
	{
		this.SelectedPrefabName = null;
		this.mLoadedGameDB = false;
		Selection.selectionChanged += OnSelectionChanged;
	}

	private void OnDisable()
	{
		Selection.selectionChanged -= OnSelectionChanged;
	}

	private void OnDestroy()
	{
		Selection.selectionChanged -= OnSelectionChanged;
	}

	private void OnSelectionChanged()
	{
		Repaint();
	}

	void OnGUI()
	{
		EditorGUILayout.HelpBox(@"선택한 프리팹이 ResourceTable에 존재한다면, 충돌체 사이즈 설정 저장 가능.", MessageType.Info);
		EditorGUILayout.Space(10);

		if (!mLoadedGameDB)
		{
			DrawExcelPath();

			GUILayout.Space(20f);
			GUI_CenterLabel($"테이블을 읽어오세요!", EditorStyles.whiteLargeLabel, GUILayout.Height(65f));
		}
		else
		{
			if (null != Selection.activeObject && Selection.activeObject is GameObject prefabGO && prefabGO.activeInHierarchy)
			{
				GUI_CenterLabel($"Selected GameObject : {prefabGO.name}", EditorStyles.whiteLargeLabel, GUILayout.Height(65f));

				if (ResourceDB.DicTableEx.TryGetValue(prefabGO.name, out var tableList))
				{
					GUILayout.Label($"[테이블에 존재하는 캐릭터]");

					if (SelectedPrefabName != prefabGO.name)
					{
						SelectedPrefabName = prefabGO.name;
						availableSave = false;
					}

					var boxCollider = prefabGO.GetComponent<BoxCollider>();
					if (null == boxCollider)
					{
						// 현재 충돌체가 없으면, 테이블 기반 데이터로 생성해준다.
						var ren = prefabGO.GetComponentInChildren<SkinnedMeshRenderer>();
						if (null != ren)
						{
							Resource_Table resTable = tableList[0];

							boxCollider = prefabGO.GetOrAddComponent<BoxCollider>();
							boxCollider.size = new Vector3(resTable.SizeX, resTable.SizeY, resTable.SizeZ);
							boxCollider.center = new Vector3(0, boxCollider.size.y * 0.5f, 0);
						}
					}

					if (GUILayout.Button($"메시 기반 충돌체로 설정", GUILayout.Height(40f)))
					{
						Renderer[] renderers = null;

						var lodGroup = prefabGO.GetComponent<LODGroup>();
						if (null != lodGroup)
						{
							var lods = lodGroup.GetLODs();
							renderers = lods[0].renderers;
						}
						else
						{
							renderers = prefabGO.GetComponentsInChildren<SkinnedMeshRenderer>();
							if (null == renderers)
							{
								renderers = prefabGO.GetComponentsInChildren<MeshRenderer>();
							}
						}

						if (null != renderers)
						{
							Bounds newBounds = new Bounds();
							foreach (var ren in renderers)
							{
								// 무기는 영역에서 제외
								if (ren.name.Contains("_W_"))
									continue;

								newBounds.Encapsulate(ren.bounds);
							}

							if (renderers.Length > 0)
							{
								boxCollider = prefabGO.GetOrAddComponent<BoxCollider>();
								boxCollider.size = newBounds.size;
								boxCollider.center = new Vector3(0, boxCollider.size.y * 0.5f, 0);
							}
						}
					}
					GUI.enabled = null != boxCollider;
					if (GUILayout.Button($"최종 점검", GUILayout.Height(40f)))
					{
						boxCollider.center = new Vector3(0, boxCollider.size.y * 0.5f, 0);
						availableSave = true;

						foreach (var table in tableList)
						{
							table.SizeX = boxCollider.size.x;
							table.SizeY = boxCollider.size.y;
							table.SizeZ = boxCollider.size.z;
						}
					}
					GUI.enabled = availableSave;
					if (GUILayout.Button(availableSave ? $"테이블에 저장하기" : "[최종 점검] 한번 눌러줘야 저장가능", GUILayout.Height(40f)))
					{
						if (SaveToExcel(EditorPrefs.GetString(Prefs_GameDBPath, string.Empty)))
						{
							availableSave = false;
							Debug.Log("저장 성공!");
						}
					}
					GUI.enabled = true;
				}
				else
				{
					GUILayout.Label($"선택된 객체({prefabGO.name})는 Resource_Table에 데이터가 존재하지 않는 파일명입니다.", ZGUIStyles.WarningLabel);
				}
			}
			else
			{
				GUILayout.Label($"유닛을 Scene에 꺼내놓고, 선택해주세요.", ZGUIStyles.WarningLabel);
			}
		}
	}

	private void ClearDB()
	{
		ResourceDB.Clear();
		mLoadedGameDB = false;
	}

	private void LoadDBForTool(string _dbTableFolder)
	{
		ClearDB();

		try
		{
			ResourceDB = new Tool_ResourceTable(Path.Combine(_dbTableFolder, "Resource_table.xlsx"));
			ResourceDB.Load();

			mLoadedGameDB = true;
		}
		catch (Exception e)
		{
			LogError(e.Message);

			Debug.LogError(e);

			mLoadedGameDB = false;
		}

		UDebug.Log($"로드 성공 여부: {mLoadedGameDB}");
	}

	public bool SaveToExcel(string _dbTableFolder)
	{
		try
		{
			ResourceDB?.Save();
		}
		catch (System.Exception)
		{
			return false;
		}

		return true;
	}
	
	public const string Prefs_GameDBPath = "ZERO_GameDB_ResourceTablePath";

	public void DrawExcelPath()
	{
		EditorGUILayout.BeginVertical("window", GUILayout.Height(100f));
		{
			string gameTablesPath = EditorPrefs.GetString(Prefs_GameDBPath, string.Empty);
			if (string.IsNullOrEmpty(gameTablesPath))
			{
				EditorGUILayout.LabelField("ResourceTable Excel Path : Not Selected!!");
			}
			else
			{
				EditorGUILayout.LabelField($"ResourceTable Excel Path : {gameTablesPath}");
			}

			EditorGUILayout.Space();
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			{
				if (GUILayout.Button("ResourceTable 폴더 선택", GUILayout.Height(40f)))
				{
					string path = EditorUtility.OpenFolderPanel("Select Excel Path", gameTablesPath, "");

					if (string.IsNullOrEmpty(path) == false)
					{
						gameTablesPath = path;
						EditorPrefs.SetString(Prefs_GameDBPath, path);
					}
				}

				if (GUILayout.Button("테이블 읽어오기", GUILayout.Height(40f)))
				{
					LoadDBForTool(gameTablesPath);
				}
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}
		EditorGUILayout.EndVertical();
	}

	static void GUI_CenterLabel(string label, GUIStyle style, params GUILayoutOption[] options)
	{
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Label(label, style, options);
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
	}

	private void LogError(string message)
	{
		UDebug.LogError(message);

		ShowNotification(new GUIContent(message));
	}
}