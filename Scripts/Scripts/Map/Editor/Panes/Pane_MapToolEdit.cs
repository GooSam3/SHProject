using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.AI;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using Zero;

public class Pane_MapToolEdit : EditorPaneBase<MapToolEditor>
{
	/// <summary> 에디터용 타입 정보 표시용 </summary>
	private class MapObjectTypeInfo
	{
		public string Name;
		public string Desc;
		public System.Type ThisType;
	}

	/// <summary> MapObject Type 모음 </summary>
	private Dictionary<string, MapObjectTypeInfo> mDicObjectTypes = new Dictionary<string, MapObjectTypeInfo>();

	List<IMapObjectBase> mAllMapObjects = new List<IMapObjectBase>();
	Dictionary<uint, List<MapObject_Monster>> mMonsterGroupDic = new Dictionary<uint, List<MapObject_Monster>>();
	Dictionary<uint, List<MapObject_Npc>> mNpcGroupDic = new Dictionary<uint, List<MapObject_Npc>>();
	Dictionary<uint, List<MapObject_Portal>> mPortalGroupDic = new Dictionary<uint, List<MapObject_Portal>>();

	private uint mMonsterCount = 0;
	private uint mNpcCount = 0;
	private uint mPortalCount = 0;

	private int mSelectedObjectTypeIdx = -1;
	private MapObjectTypeInfo mSelectedTypeInfo = null;
	private bool isShowNavigation = false;

	private StringBuilder mEditLog = new StringBuilder();
	private Vector2 mLogScrollPos;
	private Vector2 mPresetScrPos;
	private Vector2 mObjectInfoScrPos;

	class Styles
	{
		public static GUIContent navExportMenu = EditorGUIUtility.TrTextContent("Export MMONav", "MMO를 위한 NavMesh데이터 추출");
		public static GUIContent navShowMenu = EditorGUIUtility.TrTextContent("ShowNavigation", "Navigation 보여주는 토글기능");

		public static GUIContent[] mainMenus = new GUIContent[]
		{
			EditorGUIUtility.TrTextContent("저장(필수)", "Scene 그리고 MapData 저장"),
			EditorGUIUtility.TrTextContent("닫기", "에디터 끄면, 처음부터 다시 작업하셔야합니다. 신중하게 눌러주세요."),
			EditorGUIUtility.TrTextContent("처음으로", "에디터 가장 처음 단계로 돌아가도록한다."),
		};
	}

	public Pane_MapToolEdit(MapToolEditor _owner) : base(_owner)
	{
	}

	public override void OnEnable()
	{
		CollectMapObjectTypes();
	}

	public override void OnDisable()
	{
		ChangeNavMeshVisualize(false);
	}

	/// <summary> <see cref="IMapObjectBase"/> 타입을 모두 수집 </summary>
	private void CollectMapObjectTypes()
	{
		mDicObjectTypes.Clear();

		List<Type> derivedObjectTypes = ReflectionHelper.FindAllDerivedTypes<IMapObjectBase>(); //System.AppDomain.CurrentDomain.GetAllDerivedTypes<IMapObjectBase>();
		foreach (Type type in derivedObjectTypes)
		{
			if (!type.IsClass || type.IsInterface || type.IsAbstract)
				continue;

			// 에디터용 정보가 존재하면 가져오기
			string newName = type.Name;
			string newDesc = type.FullName;
			var summaryType = type.GetNestedType("EditorSummary");
			if (null != summaryType)
			{
				newName = (string)summaryType.GetField("Name").GetValue(null);
				newDesc = (string)summaryType.GetField("Desc").GetValue(null);
			}
			else
			{
				mEditLog.AppendLine($"<color=red>{type.Name} 클래스에 EditorSummay 클래스를 선언하기를 권장합니다.</color>");
			}

			if (this.mDicObjectTypes.ContainsKey(type.Name))
			{
				Debug.LogError($"{type}의 {nameof(IGameDBHelper)} Class는 이미 존재합니다.");
				continue;
			}

			mDicObjectTypes.Add(type.Name, new MapObjectTypeInfo()
			{
				Name = newName,
				Desc = newDesc,
				ThisType = type
			});
		}
	}

	public override void DrawGUI()
	{
		DrawEditToolbar();
		DrawSceneInfo();

		ZGUIStyles.Separator();

		DrawMapObjectPresets();
		DrawEditMap();
	}

	void DrawEditToolbar()
	{
		EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);// EditorStyles.toolbarTextField);
		{
			if (EditorGUILayout.DropdownButton(Styles.navExportMenu, FocusType.Passive, EditorStyles.toolbarButton))
			{
				GenericMenu menu = new GenericMenu();
				menu.AddItem(new GUIContent("Nav -->> OBJ"), false, NavMeshExporter.ExportNavMesh_WithoutCommit);
				menu.AddItem(new GUIContent("Nav -->> OBJ (with SvnCommit)"), false, NavMeshExporter.ExportNavMesh_WithCommit);
				menu.ShowAsContext();
			}

			GUILayout.FlexibleSpace();

			bool showNav = GUILayout.Toggle(isShowNavigation, Styles.navShowMenu, EditorStyles.toolbarButton, GUILayout.Width(100), GUILayout.Height(20f));
			ChangeNavMeshVisualize(showNav);
		}
		EditorGUILayout.EndHorizontal();
	}

	void DrawSceneInfo()
	{
		// 맵 정보 그려주기.
		EditorGUILayout.BeginVertical("box");
		EditorGUILayout.LabelField("ScenePath :", $"{Owner.CurSceneInfo.Path}", ZGUIStyles.TitleLabel);
		EditorGUILayout.LabelField("Stage Info :", $"{Owner.CurSceneInfo.TableData.StageID}, {Owner.CurSceneInfo.TableData.StageTextID}", ZGUIStyles.TitleLabel);
		EditorGUILayout.EndVertical();
	}

	/// <summary>생성 가능한 MapObject 리스트 나열해서 보여준다.</summary>
	void DrawMapObjectPresets()
	{
		using (var v1 = new EditorGUILayout.VerticalScope(ZGUIStyles.BoxWhite))
		{
			using (var h1 = new EditorGUILayout.HorizontalScope())
			{
				GUILayout.FlexibleSpace();
				EditorGUILayout.LabelField("MapObject Presets", ZGUIStyles.BlueBoldLabel);
				GUILayout.FlexibleSpace();
			}

			/*
			 * 생성가능한 MapObject들을 Grid형태로 보이도록 한다.
			 */
			using (var scrView = new EditorGUILayout.ScrollViewScope(mPresetScrPos, GUILayout.MinHeight(50)))
			{
				mPresetScrPos = scrView.scrollPosition;

				List<GUIContent> paletteIcons = new List<GUIContent>();
				foreach (var pair in mDicObjectTypes)
				{
					paletteIcons.Add(new GUIContent(pair.Value.Name, pair.Value.Desc));
				}

				mSelectedObjectTypeIdx = GUILayout.SelectionGrid(mSelectedObjectTypeIdx, paletteIcons.ToArray(), 4, ZGUIStyles.MapButton, GUILayout.ExpandWidth(true), GUILayout.MaxHeight(34f));
				if (mSelectedObjectTypeIdx > -1)
				{
					mSelectedTypeInfo = mDicObjectTypes.ElementAt(mSelectedObjectTypeIdx).Value;
				}
			}

			GUI.enabled = mSelectedObjectTypeIdx > -1;
			string buttonName = GUI.enabled ? mSelectedTypeInfo.Name : "미선택";
			if (GUILayout.Button($"[{buttonName}] 생성", ZGUIStyles.BlueLabelButton))
			{
				//
				// 객체 생성!!
				//
				Owner.CreateObjectOnMap(mSelectedTypeInfo.ThisType);
			}
			GUI.enabled = true;
		}
	}

	/// <summary>
	/// 편집중 씬에 맞게 동작
	/// </summary>
	void DrawEditMap()
	{
		using (var scrView = new EditorGUILayout.ScrollViewScope(mObjectInfoScrPos))
		{
			mObjectInfoScrPos = scrView.scrollPosition;

			EditorGUILayout.BeginVertical("box");
			{
				if (Event.current.type == EventType.MouseDown || Event.current.type == EventType.Layout)
				{
					mAllMapObjects = MapToolUtil.GetMapObjectsInScene();
				}

				ClearTempCollections();
				foreach (var mapObj in mAllMapObjects)
				{
					switch (mapObj)
					{
						case MapObject_Monster monsterObj:
							{
								uint tableTid = monsterObj.TableTID;
								if (!mMonsterGroupDic.ContainsKey(tableTid))
									mMonsterGroupDic.Add(tableTid, new List<MapObject_Monster>());
								mMonsterGroupDic[tableTid].Add(monsterObj);

								++mMonsterCount;
							}
							break;

						case MapObject_Npc npcObj:
							{
								uint tableTid = npcObj.TableTID;
								if (!mNpcGroupDic.ContainsKey(tableTid))
									mNpcGroupDic.Add(tableTid, new List<MapObject_Npc>());
								mNpcGroupDic[tableTid].Add(npcObj);

								++mNpcCount;
							}
							break;

						case MapObject_Portal portalObj:
							{
								uint tableTid = portalObj.TableTID;
								if (!mPortalGroupDic.ContainsKey(tableTid))
									mPortalGroupDic.Add(tableTid, new List<MapObject_Portal>());
								mPortalGroupDic[tableTid].Add(portalObj);

								++mPortalCount;
							}
							break;
					}
				}

				using (var h1 = new EditorGUILayout.HorizontalScope())
				{
					EditorGUILayout.LabelField($"Monster개수 : {mMonsterCount}", EditorStyles.whiteLabel);
					EditorGUILayout.LabelField($"Npc개수 : {mNpcCount}", EditorStyles.whiteLabel);
					EditorGUILayout.LabelField($"Portal개수 : {mPortalCount}", EditorStyles.whiteLabel);
				}
				EditorGUILayout.Space();

				using (var h2 = new EditorGUILayout.HorizontalScope())
				{
					EditorGUILayout.BeginVertical();
					foreach (var pair in mMonsterGroupDic)
					{
						if (GUILayout.Button($"Monster[{pair.Key}] 합계 : {pair.Value.Count}", EditorStyles.toolbarButton))
						{
							SelectionTargets(pair.Value.ConvertAll<GameObject>((d) => d.gameObject));							
						}
					}
					EditorGUILayout.EndVertical();

					EditorGUILayout.Space();

					EditorGUILayout.BeginVertical();
					foreach (var pair in mNpcGroupDic)
					{
						if (GUILayout.Button($"Npc[{pair.Key}] 합계 : {pair.Value.Count}", EditorStyles.toolbarButton))
						{
							SelectionTargets(pair.Value.ConvertAll<GameObject>((d) => d.gameObject));
						}
					}
					EditorGUILayout.EndVertical();

					EditorGUILayout.BeginVertical();
					foreach (var pair in mPortalGroupDic)
					{
						if (GUILayout.Button($"Portal[{pair.Key}] 합계 : {pair.Value.Count}", EditorStyles.toolbarButton))
						{
							SelectionTargets(pair.Value.ConvertAll<GameObject>((d) => d.gameObject));
						}
					}
					EditorGUILayout.EndVertical();
				}
			}
			EditorGUILayout.EndVertical();
		}

		GUILayout.FlexibleSpace();

		mLogScrollPos = EditorGUILayout.BeginScrollView(mLogScrollPos);
		EditorGUILayout.TextArea(mEditLog.ToString(), ZGUIStyles.MiniLabel);
		EditorGUILayout.EndScrollView();

		// 배치된 맵오브젝트들 유효성 체크
		if (GUILayout.Button("맵 오브젝트들 위치 유효성 체크 & 보정", GUILayout.Height(25)))
		{
			CorrectPositionMapObjects();
		}

		if (GUILayout.Button("맵 오브젝트들 데이터 유효성 체크", GUILayout.Height(25)))
		{
			ValidateDataMapObjects();
		}

		ZGUIStyles.Separator();

		GUILayout.BeginHorizontal();
		{
			int toolbarindex = GUILayout.Toolbar(-1, Styles.mainMenus, ZGUIStyles.MapButton, GUILayout.ExpandWidth(true), GUILayout.Height(30f));

			if (toolbarindex == 0)
			{
				EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
				Owner.SaveToMapData();
				mEditLog.Clear();
			}
			else if (toolbarindex == 1)
			{
				EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
				Owner.Close();
			}
			else if (toolbarindex == 2)
			{
				Owner.ResetEditor();
			}
		}
		GUILayout.EndHorizontal();
	}

	private void ClearTempCollections()
	{
		mMonsterCount = 0;
		mNpcCount = 0;
		mPortalCount = 0;
		mMonsterGroupDic.Clear();
		mNpcGroupDic.Clear();
		mPortalGroupDic.Clear();
	}

	/// <summary>
	/// MapObject 위치 및 유효성 체크
	/// </summary>
	private void CorrectPositionMapObjects()
	{
		bool isSuccess = true;
		mAllMapObjects = MapToolUtil.GetMapObjectsInScene();
		foreach (var mapObject in mAllMapObjects)
		{
			if (!(mapObject is MonoBehaviour mapMono))
				continue;

			Transform trans = mapMono.transform;
			Vector3 originalPos = trans.position;
			Vector3 newPos = originalPos;

			if (NavMesh.SamplePosition(originalPos, out var sampleHit, 1000, -1))
			{
				newPos = sampleHit.position;
			}
			else
			{
				if (NavMesh.FindClosestEdge(originalPos, out var closeHit, -1))
				{
					newPos = sampleHit.position;
				}
			}

			if (originalPos != newPos)
			{
				trans.position = newPos;

				mEditLog.AppendLine($"<color=red>이동됨 : {trans.name}{originalPos} ==> {trans.position}</color>");
				isSuccess = false;
			}
		}

		if (isSuccess)
		{
			mEditLog.AppendLine("<color=green>축하드립니다. 모두 정상 위치에 있습니다!</color>");
		}
	}

	/// <summary>
	/// MapObject에 존재하는 데이터의 유효성 체크
	/// </summary>
	private void ValidateDataMapObjects()
	{
		//mEditLog.AppendLine("<color=red>미구현 상태</color>");

		mAllMapObjects = MapToolUtil.GetMapObjectsInScene();
		foreach (var mapObj in mAllMapObjects)
		{
			switch (mapObj)
			{
				case MapObject_Monster monsterObj:
					{
						if (!Owner.TableDB.MonsterTable.DicTable.ContainsKey(monsterObj.TableTID))
						{
							mEditLog.AppendLine($"<color=red>MapObject_Monster객체중에 테이블에 존재하지 않는 TID[{monsterObj.TableTID}]가 할당된 객체가 있습니다.</color>");
						}
					}
					break;

				case MapObject_Npc npcObj:
					{
						if (!Owner.TableDB.NPCTable.DicTable.ContainsKey(npcObj.TableTID))
						{
							mEditLog.AppendLine($"<color=red>MapObject_Npc객체중에 테이블에 존재하지 않는 TID[{npcObj.TableTID}]가 할당된 객체가 있습니다.</color>");
						}
					}
					break;

				case MapObject_Portal portalObj:
					{
						if (!Owner.TableDB.PortalTable.DicTable.ContainsKey(portalObj.TableTID))
						{
							mEditLog.AppendLine($"<color=red>MapObject_Portal객체중에 테이블에 존재하지 않는 TID[{portalObj.TableTID}]가 할당된 객체가 있습니다.</color>");
						}
					}
					break;
			}
		}

		/* ===== 사용도 떨어지면 삭제하는게 나을듯.
		if (GUILayout.Button("SpawnPoint 유효성 체크", GUILayout.Height(25)))
		{
			var spawnObjList = MapToolEditor.GetMapObjectsInScene<MapObject_SpawnPoint>();
			foreach (var mapObj in spawnObjList)
			{
				if (mapObj.UnitType == E_UnitType.Monster)
				{
					if (!mMonsterDBDic.ContainsKey(mapObj.TableTID))
					{
						validCheckSB.AppendLine($"<color=red>SpawnPoint({mapObj.UnitType})_테이블에 존재하지 않는 TID[{mapObj.TableTID}]가 할당된 객체가 있습니다.</color>");
					}
				}
				else if (mapObj.UnitType == E_UnitType.NPC)
				{
					if (!mNpcDBDic.ContainsKey(mapObj.TableTID))
					{
						validCheckSB.AppendLine($"<color=red>SpawnPoint({mapObj.UnitType})_테이블에 존재하지 않는 TID[{mapObj.TableTID}]가 할당된 객체가 있습니다.</color>");
					}
				}
			}
		}
		if (GUILayout.Button("PortalPoint 유효성 체크", GUILayout.Height(25)))
		{
			var portalObjList = MapToolEditor.GetMapObjectsInScene<MapObject_PortalPoint>();
			foreach (var mapObj in portalObjList)
			{
				if (!mPortalDBDic.ContainsKey(mapObj.TableTID))
				{
					validCheckSB.AppendLine($"<color=red>PortalPoint_테이블에 존재하지 않는 TID[{mapObj.TableTID}]가 할당된 객체가 있습니다.</color>");
				}
			}
		}
		*/
	}

	void SelectionTargets(List<GameObject> targets, bool bSelectInSceneView = true)
	{
		IEnumerable<GameObject> activeGameObjects = targets.Where(j => j.activeInHierarchy);
		Selection.objects = activeGameObjects.ToArray();

		if (bSelectInSceneView)
			SceneView.lastActiveSceneView.FrameSelected();
	}

	/// <summary> NavMesh Scene view 그리는거 상태 전환시키기</summary>
	void ChangeNavMeshVisualize(bool newValue)
	{
		if (isShowNavigation == newValue)
			return;

		if (newValue)
		{
			NavMeshVisualizationSettings.showNavigation++;
		}
		else
		{
			NavMeshVisualizationSettings.showNavigation--;
		}

		isShowNavigation = newValue;

		MapToolUtil.RepaintSceneAndGameViews();
	}
}
