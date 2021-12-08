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
    
	/// <summary>���� �������� �� ����</summary>
	public EditorSceneInfo CurSceneInfo { get; private set; }

	/// <summary> ��庰 UI ��ü </summary>
	public Dictionary<string, EditorPaneBase<MapToolEditor>> ChildPanes	{ get; } = new Dictionary<string, EditorPaneBase<MapToolEditor>>();

	/// <summary> ���� �׷����� �ִ� UI </summary>
	public EditorPaneBase<MapToolEditor> CurPane { get; private set; }

	static public string ProgressTitle = "�о������..";

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

		// �ʱ� ���û��·� ������.
		ResetEditor();
	}

    void OnDisable()
    {
		CurPane?.OnDisable();
	}

	/// <summary> �����͸� �ʱ� ���·� �ǵ�����. ��� �ʱ�ȭ </summary>
    public void ResetEditor()
    {
		ChildPanes.Clear();
		ChildPanes.Add(nameof(Pane_MapToolStart), new Pane_MapToolStart(this));
		ChildPanes.Add(nameof(Pane_MapToolStageSelect), new Pane_MapToolStageSelect(this));
		ChildPanes.Add(nameof(Pane_MapToolEdit), new Pane_MapToolEdit(this));
		ChangePane(nameof(Pane_MapToolStart));

		// ������ ���� �ʱ�ȭ
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
			MapToolUtil.CenterLabel($"[�ɰ��� ���� �߻�] �����͸� �ٽ� �������ּ���.",
				ZGUIStyles.WarningLabel, GUILayout.Height(65f));
			return;
		}

		CurPane?.DrawGUI();
	}

    /// <summary>
    /// �ش� ���� �ε��ϰ�, �� ������ �ʿ��� �⺻ ������ �����Ѵ�.
    /// </summary>
    /// <param name="scenePath"></param>
    public EditorSceneInfo LoadAndSetupScene(string scenePath, Stage_Table stageTable, List<string> subSceneList = null)
    {

		//====  ���ξ� �ε�
		EditorUtility.DisplayProgressBar(ProgressTitle, "MainScene", 0.0f);
		var loadedScene = EditorSceneManager.OpenScene(scenePath);

		CurSceneInfo.Renew(
			scenePath, 
			stageTable,
			loadedScene);

		//==== ����� �ε�
		{
			EditorUtility.DisplayProgressBar(ProgressTitle, "SubScenes", 0.1f);
			foreach (string subScene in subSceneList)
			{
				EditorSceneManager.OpenScene(subScene, OpenSceneMode.Additive);
			}
		}

		//
		// ���� ���� �ִ� ��ü���� ���� �� ���� ����� ������ �Ѵ�!
		//
		SceneVisibilityManager.instance.DisableAllPicking();
		//foreach (var go in this.CurSceneInfo.Scene.GetRootGameObjects())
		//{
		//	SceneVisibilityManager.instance.DisablePicking(go, true);
		//}

		// �� ������ ���� ------------------------------------------
		EditorUtility.DisplayProgressBar(ProgressTitle, "MapObjects", 0.3f);
		var management = LoadMapRootObject(stageTable.StageID);
		EditorUtility.DisplayProgressBar(ProgressTitle, "Finish!", 0.9f);

		// �� �����ڴ� ���������ϵ��� ���� --------------------------
		SceneVisibilityManager.instance.EnablePicking(management.gameObject, true);

		EditorSceneManager.SetActiveScene(CurSceneInfo.Scene);

		// SceneView �ٷ� ���̵��� �ϱ�.
		if (null != SceneView.lastActiveSceneView)
			SceneView.lastActiveSceneView.Focus();

		EditorUtility.ClearProgressBar();

		return CurSceneInfo;
	}

	/// <summary> �� �����ڸ� �����´�. ���ٸ� ���� </summary>
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

        // �� ������ ������Ʈ�� ���� ���� Ȯ�� �� ����
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
                LogWarning($"���� ���� ������ MapData�� Stage[{mapManagement.MainData.StageTID}]�� �����Ǿ��ֽ��ϴ�. �����Ϸ��� �������� ������ �ٸ��ϴ�. {mapManagement.MainData.StageTID} != {_stageTid}");
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

        /// <see cref="MapData"/> ��� �����Ϳ� ���� ��ü �����ϵ��� �Ѵ�.
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
		
        //============ Map Bounds �����ؼ� �����ϱ� ===============
        Bounds mapBounds = MapToolUtil.GetCurrectMapBounds();

        ZLog.Log(ZLogChannel.Map, ZLogLevel.Warning, $"mapBounds : {mapBounds}");
        mapManagement.MapBounds = mapBounds;
        mapManagement.MapRadius = Mathf.Max(mapBounds.extents.x, mapBounds.extents.y, mapBounds.extents.z);

        mapData.MapBounds = mapBounds;

		return mapManagement;
    }


    /// <summary>
    /// GameObject�� �����ϴ� �� ��ü�� ScriptableObject�� �����ϱ�.
    /// </summary>
    public void SaveToMapData()
    {
        var mapData = this.CurSceneInfo.MapManagement.MainData;
        if (null == mapData)
        {
            LogWarning("�� �޽����� �ҷȴٸ�, �ʵ����� ����� ����ȵȰ���!");
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

		// ���̺�� ������ �ʿ��� ó�� �۵�.
		SaveMapDataToTable(mapData);

        mapDataSO.ApplyModifiedProperties();
        EditorUtility.SetDirty(mapData);
        AssetDatabase.SaveAssets();

        ExportMapDataToJson(mapData);
		// �� �� NavMesh �����͵� ����ǵ��� �Ѵ�.
		NavMeshExporter.ExportNavMesh_WithoutCommit();
	}

    /// <summary>Scene�� <see cref="MapObjectBase"/>��� ��ü ����</summary>
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

		// ī�װ� ����
		GameObject categoryGo = MapToolUtil.CreateGameObjectOfType(objectType);
		categoryGo.hideFlags |= HideFlags.DontSave;

		mapObject.transform.parent = categoryGo.transform;

		// ī�װ� Root�� ����
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

	/// <summary>�� ������Ʈ�� ���ξ��� ���</summary>
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

    /// <summary> ������ json ������ ����� </summary>
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
