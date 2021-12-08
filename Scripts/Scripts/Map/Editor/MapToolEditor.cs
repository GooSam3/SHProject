using GameDB;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UDebug = UnityEngine.Debug;


public partial class MapToolEditor : EditorWindow
{
    static public MapToolEditor instance { get; private set; }
    
	/// <summary>현재 편집중인 씬 정보</summary>
	public EditorSceneInfo CurSceneInfo { get; private set; }

	/// <summary> 모드별 UI 객체 </summary>
	public Dictionary<string, EditorPaneBase<MapToolEditor>> ChildPanes	{ get; } = new Dictionary<string, EditorPaneBase<MapToolEditor>>();

	/// <summary> 현재 그려지고 있는 UI </summary>
	public EditorPaneBase<MapToolEditor> CurPane { get; private set; }

	static public string ProgressTitle = "읽어오는중..";

	[MenuItem("ZGame/Map Editor %#m", priority = 0)]
    static void Initialize()
    {
        instance = (MapToolEditor)GetWindow(typeof(MapToolEditor));
        instance.Show();
    }

    void OnDestroy()
    {
	}

    void OnEnable()
    {
		instance = this;

		// 초기 셋팅상태로 돌리기.
		ResetEditor();
	}

    void OnDisable()
    {
		CurPane?.OnDisable();
	}

	/// <summary> 에디터를 초기 상태로 되돌린다. 모든 초기화 </summary>
    public void ResetEditor()
    {
		ChildPanes.Clear();
		ChildPanes.Add(nameof(Pane_MapToolStart), new Pane_MapToolStart(this));
		ChildPanes.Add(nameof(Pane_MapToolStageSelect), new Pane_MapToolStageSelect(this));
		ChildPanes.Add(nameof(Pane_MapToolEdit), new Pane_MapToolEdit(this));
		ChangePane(nameof(Pane_MapToolStart));

		// 데이터 관련 초기화
        this.ClearDatas();
		this.CurSceneInfo = new EditorSceneInfo();
    }

	public bool ChangePane(string paneName)
	{
		if (!ChildPanes.TryGetValue(paneName, out var targetPane))
			return false;

		CurPane?.OnDisable();
		CurPane = targetPane;
		CurPane?.OnEnable();

		return true;
	}

    void OnGUI()
    {
		if (null == CurPane)
		{
			MapToolUtil.CenterLabel($"[심각한 문제 발생] 에디터를 다시 시작해주세요.",
				ZGUIStyles.WarningLabel, GUILayout.Height(65f));
			return;
		}

		CurPane?.DrawGUI();
	}

    /// <summary>
    /// 해당 맵을 로드하고, 맵 편집에 필요한 기본 설정을 진행한다.
    /// </summary>
    /// <param name="scenePath"></param>
    public EditorSceneInfo LoadAndSetupScene(string scenePath, Stage_Table stageTable, List<string> subSceneList = null)
    {

		//====  메인씬 로드
		EditorUtility.DisplayProgressBar(ProgressTitle, "MainScene", 0.0f);
		var loadedScene = EditorSceneManager.OpenScene(scenePath);

		CurSceneInfo.Renew(
			scenePath, 
			stageTable,
			loadedScene);

		//==== 서브씬 로드
		{
			EditorUtility.DisplayProgressBar(ProgressTitle, "SubScenes", 0.1f);
			foreach (string subScene in subSceneList)
			{
				EditorSceneManager.OpenScene(subScene, OpenSceneMode.Additive);
			}
		}

		//
		// 원래 씬에 있는 객체들은 수정 및 선택 기능을 막도록 한다!
		//
		SceneVisibilityManager.instance.DisableAllPicking();
		//foreach (var go in this.CurSceneInfo.Scene.GetRootGameObjects())
		//{
		//	SceneVisibilityManager.instance.DisablePicking(go, true);
		//}

		// 맵 관리자 설정 ------------------------------------------
		EditorUtility.DisplayProgressBar(ProgressTitle, "MapObjects", 0.3f);
		var management = LoadMapRootObject(stageTable.StageID);
		EditorUtility.DisplayProgressBar(ProgressTitle, "Finish!", 0.9f);

		// 맵 관리자는 편집가능하도록 셋팅 --------------------------
		SceneVisibilityManager.instance.EnablePicking(management.gameObject, true);

		EditorSceneManager.SetActiveScene(CurSceneInfo.Scene);

		// SceneView 바로 보이도록 하기.
		if (null != SceneView.lastActiveSceneView)
			SceneView.lastActiveSceneView.Focus();

		EditorUtility.ClearProgressBar();

		return CurSceneInfo;
	}

	/// <summary> 맵 관리자를 가져온다. 없다면 생성 </summary>
	MapDataManagement LoadMapRootObject(uint _stageTid)
    {
        bool isChanged = false;

        var managementGO = GameObject.Find(MapToolDefines.MapManagementObjectName);
        if (null == managementGO)
        {
            managementGO = new GameObject(MapToolDefines.MapManagementObjectName);
            managementGO.transform.position = Vector3.zero;

            isChanged = true;
        }

        // 맵 관리용 컴포넌트도 존재 여부 확인 및 생성
        var mapManagement = managementGO.GetOrAddComponent<MapDataManagement>();
        if (null == mapManagement.MainData || mapManagement.MainData.StageTID != _stageTid)
        {
            string saveAssetPath = Path.Combine(MapToolDefines.Data_SaveFolder, $"Map_{_stageTid.ToString()}.asset");

            var savedMapdata = AssetDatabase.LoadAssetAtPath<MapData>(saveAssetPath);
            if (savedMapdata)
            {
                mapManagement.MainData = savedMapdata;
            }
            else
            {
                mapManagement.MainData = ScriptableObjectFactory.CreateAt<MapData>(saveAssetPath);
                mapManagement.MainData.ClearAll();
            }
            mapManagement.MainData.StageTID = _stageTid;
            EditorUtility.SetDirty(mapManagement);
        }
        else
        {
            if (_stageTid != mapManagement.MainData.StageTID)
            {
                LogWarning($"현재 씬에 설정된 MapData가 Stage[{mapManagement.MainData.StageTID}]로 설정되어있습니다. 편집하려는 스테이지 정보가 다릅니다. {mapManagement.MainData.StageTID} != {_stageTid}");
            }
        }

		CurSceneInfo.SetManagement(mapManagement);

		if (isChanged)
        {
            if (null != this.CurSceneInfo.Scene)
            {
                EditorSceneManager.MarkSceneDirty(this.CurSceneInfo.Scene);
            }
        }

        if (null != this.CurSceneInfo.Scene)
        {
            EditorSceneManager.MarkSceneDirty(this.CurSceneInfo.Scene);
        }

        /// <see cref="MapData"/> 기반 에디터용 게임 객체 생성하도록 한다.
        var mapData = mapManagement.MainData;
		foreach (var monInfo in mapData.MonsterInfos)
		{
			MapObject_Monster loadedObj = CreateObjectOnMap<MapObject_Monster>();
			loadedObj.Import(monInfo);
			loadedObj.SetModel(CreateMonsterModel(monInfo.TableTID));
		}

		foreach (var npcInfo in mapData.NpcInfos)
		{
			MapObject_Npc loadedObj = CreateObjectOnMap<MapObject_Npc>();
			loadedObj.Import(npcInfo);
			loadedObj.SetModel(CreateNPCModel(npcInfo.TableTID));
		}

        foreach (var objectInfo in mapData.ObjectInfos)
        {
            MapObject_Object loadedObj = CreateObjectOnMap<MapObject_Object>();
            loadedObj.Import(objectInfo);
            loadedObj.SetModel(CreateObjectModel(objectInfo.TableTID));
        }

        foreach (var portalInfo in mapData.PortalInfos)
        {
            MapObject_Portal loadedObj = CreateObjectOnMap<MapObject_Portal>();
            loadedObj.Import(portalInfo);
        }
		
        //============ Map Bounds 측정해서 저장하기 ===============
        Bounds mapBounds = MapToolUtil.GetCurrectMapBounds();

        ZLog.Log(ZLogChannel.Map, ZLogLevel.Warning, $"mapBounds : {mapBounds}");
        mapManagement.MapBounds = mapBounds;
        mapManagement.MapRadius = Mathf.Max(mapBounds.extents.x, mapBounds.extents.y, mapBounds.extents.z);

        mapData.MapBounds = mapBounds;

		return mapManagement;
    }


    /// <summary>
    /// GameObject로 존재하던 맵 객체들 ScriptableObject로 저장하기.
    /// </summary>
    public void SaveToMapData()
    {
        var mapData = this.CurSceneInfo.MapManagement.MainData;
        if (null == mapData)
        {
            LogWarning("이 메시지가 불렸다면, 맵데이터 제대로 저장안된거임!");
            return;
        }

        EditorUtility.SetDirty(this.CurSceneInfo.MapManagement);

        var mapDataSO = new SerializedObject(mapData);

		mapData.ClearAll();
		mapData.StageTID = this.CurSceneInfo.TableData.StageID;

		var monsterObjList = MapToolUtil.GetMapObjectsInScene<MapObject_Monster>();
		foreach (var mapObj in monsterObjList)
		{
			mapData.MonsterInfos.Add((MapData.MonsterInfo)mapObj.Export());
		}

		var npcObjList = MapToolUtil.GetMapObjectsInScene<MapObject_Npc>();
		foreach (var mapObj in npcObjList)
		{
			mapData.NpcInfos.Add((MapData.NpcInfo)mapObj.Export());
		}

        var objectObjList = MapToolUtil.GetMapObjectsInScene<MapObject_Object>();
        foreach (var mapObj in objectObjList)
        {
            mapData.ObjectInfos.Add((MapData.ObjectInfo)mapObj.Export());
        }

        var portalObjList = MapToolUtil.GetMapObjectsInScene<MapObject_Portal>();
        foreach (var mapObj in portalObjList)
        {
            mapData.PortalInfos.Add((MapData.PortalInfo)mapObj.Export());
        }

		// 테이블로 저장이 필요한 처리 작동.
		SaveMapDataToTable(mapData);

        mapDataSO.ApplyModifiedProperties();
        EditorUtility.SetDirty(mapData);
        AssetDatabase.SaveAssets();

        ExportMapDataToJson(mapData);
		// 씬 상에 NavMesh 데이터도 저장되도록 한다.
		NavMeshExporter.ExportNavMesh_WithoutCommit();
	}

    /// <summary>Scene상에 <see cref="MapObjectBase"/>기반 객체 생성</summary>
    public OBJECT_TYPE CreateObjectOnMap<OBJECT_TYPE>() where OBJECT_TYPE : IMapObjectBase
	{
        System.Type objectType = typeof(OBJECT_TYPE);
        
        return (OBJECT_TYPE)CreateObjectOnMap(objectType);
    }

	public object CreateObjectOnMap(System.Type mapObjectType)
	{
		System.Type objectType = mapObjectType;

		string newName = objectType.Name.Replace("MapObject_", "");

		GameObject mapObject = new GameObject(newName, objectType);
		mapObject.transform.position = MapToolUtil.GetSceneviewCenterPosition();
		mapObject.hideFlags |= HideFlags.DontSave;

		// 카테고리 연결
		GameObject categoryGo = MapToolUtil.CreateGameObjectOfType(objectType);
		categoryGo.hideFlags |= HideFlags.DontSave;

		mapObject.transform.parent = categoryGo.transform;

		// 카테고리 Root로 연결
		if (categoryGo.transform.parent != this.CurSceneInfo.MapManagement.transform)
		{
			categoryGo.transform.SetParent(this.CurSceneInfo.MapManagement.transform, false);
		}

		Selection.SetActiveObjectWithContext(mapObject, mapObject);

		if (null != this.CurSceneInfo.Scene)
		{
			EditorSceneManager.MarkSceneDirty(this.CurSceneInfo.Scene);
		}

		return mapObject.GetComponent(objectType);
	}

	/// <summary>현 프로젝트의 메인씬들 경로</summary>
	public List<string> GetAllMainScenePaths()
    {
        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", MapToolDefines.SceneStorageFolders); 

        List<string> scenePaths = new List<string>();

        foreach (var guid in sceneGuids)
        {
            var scenePath = AssetDatabase.GUIDToAssetPath(guid);

            scenePaths.Add(scenePath);
        }

        return scenePaths;
    }

    #region ========:: Excel Management ::========
    

    private void LogError(object message)
    {
        UDebug.LogError(message);

        ShowNotification(new GUIContent(message.ToString()));
    }

    private void LogWarning(object message)
    {
        UDebug.LogWarning(message);

        ShowNotification(new GUIContent(message.ToString()));
    }

    #endregion

    /// <summary> 서버용 json 데이터 저장용 </summary>
    private void ExportMapDataToJson(MapData mapData)
    {
        var jsonTextData = JsonUtility.ToJson(mapData, true);
        
        string savePath = Path.Combine(MapToolDefines.DataJson_SaveFolder, $"Map_{mapData.StageTID}.json");

		using (FileStream fs = new FileStream(savePath, FileMode.Create))
		{
			byte[] writeBytes = Encoding.UTF8.GetBytes(jsonTextData);

			fs.Write(writeBytes, 0, writeBytes.Length);
		}
    }
}
