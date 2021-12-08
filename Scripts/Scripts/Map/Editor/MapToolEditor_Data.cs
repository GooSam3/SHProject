using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public partial class MapToolEditor
{
	/// <summary> xlsx에서 바로 읽어와서 가공된 정보 (일부만 들어있음)</summary>
	public MapToolDB TableDB { get; private set; } = new MapToolDB();

	public static bool IsLoadedGameDB
	{
		get
		{
			return null != instance && null != instance.TableDB && instance.TableDB.IsLoaded;
		}
	}

	/// <summary>
	/// 
	/// </summary>
    public bool LoadDBForMapTool(string _dbTableFolder)
    {
		TableDB.Clear();

		try
		{
			TableDB.LoadTables(_dbTableFolder);
		}
		catch (Exception e)
		{
			LogError(e);
			return false;
		}

		return true;
    }

	private void ClearDatas()
	{
		TableDB.Clear();
	}

	/// <summary>
	/// 테이블(xlsx)로 저장되어야할 데이터들 처리
	/// </summary>
	public void SaveMapDataToTable(MapData _mapData)
	{
		TableDB.PortalTable.Save(_mapData);
		TableDB.StageTable.Save(_mapData);
	}	

    /// <summary></summary>
    /// <param name="useCurrentStage">현재 편집중인 스테이지에 해당하는 정보만 뽑아내는지 여부</param>
    public List<string> GetPortalTIDStrs(bool useCurrentStage)
    {
        List<string> results = new List<string>();
		uint curStageTID = this.CurSceneInfo.TableData.StageID;

		foreach (var table in TableDB.PortalTable.DicTable.Values)
        {
            if (useCurrentStage && table.StageID != curStageTID)
                continue;

            results.Add(table.PortalID.ToString());
        }

        return results;
    }

    public List<string> GetMonsterTIDStrs(bool useCurrentStage)
    {
		List<string> results = new List<string>();
		uint curStageTID = this.CurSceneInfo.TableData.StageID;

        foreach (var table in TableDB.MonsterTable.DicTable.Values)
        {
            if (useCurrentStage && table.PlaceStageID != curStageTID)
                continue;

            results.Add($"{table.MonsterID.ToString()}({table.MonsterType})");
        }

        return results;
    }

	public List<GameDB.Monster_Table> GetMonsterTables(bool useCurrentStage)
	{
		List<GameDB.Monster_Table> results = new List<GameDB.Monster_Table>();
		uint curStageTID = this.CurSceneInfo.TableData.StageID;

		foreach (var table in TableDB.MonsterTable.DicTable.Values)
		{
			if (useCurrentStage && table.PlaceStageID != curStageTID)
				continue;

			results.Add(table);
		}

		return results;
	}

	public List<GameDB.Portal_Table> GetPortalTables(bool useCurrentStage)
	{
		List<GameDB.Portal_Table> results = new List<GameDB.Portal_Table>();
		uint curStageTID = this.CurSceneInfo.TableData.StageID;

		foreach (var table in TableDB.PortalTable.DicTable.Values)
		{
			if (useCurrentStage && table.StageID != curStageTID)
				continue;

			results.Add(table);
		}

		return results;
	}

	public List<string> GetNpcTIDStrs()
    {
        List<string> results = new List<string>();

        foreach (var table in TableDB.NPCTable.DicTable.Values)
        {
            results.Add(table.NPCID.ToString());
        }

        return results;
    }

	/// <summary> 에디터상에서 몬스터 모델 로드 </summary>
	public GameObject CreateMonsterModel(uint monsterTid)
	{
		if (!IsLoadedGameDB)
			return null;

		// 몬스터 로드해보자
		bool existTable = MapToolEditor.instance.TableDB.MonsterTable.DicTable.TryGetValue(monsterTid, out var monTable);
		if (!existTable)
		{
			LogError($"MonsterTID[{monsterTid}]는 존재하지 않습니다.");
			return null;
		}

		existTable &= MapToolEditor.instance.TableDB.ResourceTable.DicTable.TryGetValue(monTable.ResourceID, out var resTable);

		if (!existTable)
			return null;

		return CreateModel(resTable.ResourceFile, monTable.Scale);
	}

	/// <summary> 에디터상에서 NPC 모델 로드 </summary>
	public GameObject CreateNPCModel(uint npcTid)
	{
		if (!IsLoadedGameDB)
			return null;

		// 몬스터 로드해보자
		bool existTable = MapToolEditor.instance.TableDB.NPCTable.DicTable.TryGetValue(npcTid, out var npcTable);
		if (null == npcTable)
			return null;

		existTable &= MapToolEditor.instance.TableDB.ResourceTable.DicTable.TryGetValue(npcTable.ResourceID, out var resTable);

		if (!existTable)
			return null;

		return CreateModel(resTable.ResourceFile, npcTable.Scale);
	}

    /// <summary> 에디터상에서 NPC 모델 로드 </summary>
	public GameObject CreateObjectModel(uint objectTid)
    {
        if (!IsLoadedGameDB)
            return null;

        // 몬스터 로드해보자
        bool existTable = MapToolEditor.instance.TableDB.ObjectTable.DicTable.TryGetValue(objectTid, out var objectTable);
		if (existTable)
			existTable &= (false == string.IsNullOrEmpty(objectTable.ResourceName));
		else
			ZLog.LogError(ZLogChannel.Map, $"해당 Tid (objectTid) 데이터가 없습니다.");

        if (!existTable)
            return null;

        return CreateModel(objectTable.ResourceName, objectTable.Scale);
    }

    private GameObject CreateModel(string resourceName, float scale)
	{
		string[] guids = AssetDatabase.FindAssets($"{resourceName} t:GameObject", new[] { "Assets/_ZAssetBundle/Character" });

		GameObject createdModel = null;
		foreach (string guid in guids)
		{
			string myObjectPath = AssetDatabase.GUIDToAssetPath(guid);

			var originPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(myObjectPath);
			createdModel = Instantiate<GameObject>(originPrefab);

			createdModel.transform.localScale = Vector3.one * scale * 0.01f;
		}

		return createdModel;
	}
}