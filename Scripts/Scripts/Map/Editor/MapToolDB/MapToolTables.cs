using GameDB;
using OfficeOpenXml;
using System.Collections.Generic;
using UnityEngine;

public interface IToolTableBase
{
	void Load();
	void Save();
	void Clear();
}

/// <summary>
/// 맵 에디터용 테이블 처리용 클래스
/// </summary>
/// <typeparam name="KEY_TYPE"></typeparam>
/// <typeparam name="TABLE_TYPE"></typeparam>
public abstract class ToolTableBase<KEY_TYPE, TABLE_TYPE> : IToolTableBase
	where TABLE_TYPE : class
{
	public Dictionary<KEY_TYPE, TABLE_TYPE> DicTable { get; protected set; } = new Dictionary<KEY_TYPE, TABLE_TYPE>();

	/// <summary>테이블 저장/로드시 사용될 시트명</summary>
	protected abstract string SheetName { get; }

	private string mTableFilePath;

	public ToolTableBase(string _xlsxPath)
	{
		mTableFilePath = _xlsxPath;
	}

	public void Load()
	{
		var sheet = MapToolUtil.LoadExcelSheet(
			mTableFilePath,
			SheetName,
			out var excelPackage);

		Clear();

		OnLoad(sheet, excelPackage);
	}

	public void Save()
	{
		var sheet = MapToolUtil.LoadExcelSheet(
			mTableFilePath,
			SheetName,
			out var excelPackage);

		OnSave(sheet, excelPackage);

		// 저장
		excelPackage.Save();
	}

	/// <summary></summary>
	protected abstract void OnLoad(ExcelWorksheet _sheet, ExcelPackage _excelPackage);
	/// <summary></summary>
	protected abstract void OnSave(ExcelWorksheet _sheet, ExcelPackage _excelPackage);

	protected Dictionary<string, int> GetColumnIndexes(ExcelWorksheet _sheet)
	{
		Dictionary<string, int> indexes = new Dictionary<string, int>();

		var fields = typeof(TABLE_TYPE).GetFields();
		foreach (var f in fields)
		{
			indexes.Add(f.Name, 0);
		}

		for (int i = 1; i <= _sheet.Dimension.Columns; i++)
		{
			if (_sheet.GetValue(1, i) == null)
				continue;

			// 컬럼명 구하기
			string str = _sheet.GetValue<string>(1, i);

			if (!indexes.ContainsKey(str))
				continue;

			indexes[str] = i;
		}

		return indexes;
	}

	public virtual void Clear()
	{
		DicTable.Clear();
	}
}


/// <summary> 
/// <see cref="GameDB.Stage_Table"/> 
/// </summary>
public class Tool_StageTable : ToolTableBase<uint, Stage_Table>
{
	protected override string SheetName => nameof(Stage_Table);

	public MapData MapData { get; private set; }

	public Tool_StageTable(string _xlsxPath) : base(_xlsxPath) { }

	protected override void OnLoad(ExcelWorksheet _sheet, ExcelPackage _excelPackage)
	{
		Dictionary<string, int> indexes = GetColumnIndexes(_sheet);

		for (int rowIdx = 2; rowIdx <= _sheet.Dimension.Rows; ++rowIdx)
		{
			//UDebug.Log(_sheet.GetValue(rowIdx, 1 /*주석*/) + ", " + _sheet.GetValue(rowIdx, 2 /*StageID*/) + ", " + _sheet.GetValue(rowIdx, 5 /*ResourceFileName*/));
			string stageComment = _sheet.GetValue<string>(rowIdx, indexes[nameof(GameDB.Stage_Table.StageTextID)] /*주석*/);
			string stageSceneName = _sheet.GetValue<string>(rowIdx, indexes[nameof(GameDB.Stage_Table.ResourceFileName)] /*ResourceFileName*/);
			uint stageTid = _sheet.GetValue<uint>(rowIdx, indexes[nameof(GameDB.Stage_Table.StageID)] /*StageID*/);

			GameDB.Stage_Table tableData = new GameDB.Stage_Table()
			{
				StageID = stageTid,
				StageTextID = stageComment,
				ResourceFileName = stageSceneName,
			};

			this.DicTable.Add(tableData.StageID, tableData);
		}
	}

	protected override void OnSave(ExcelWorksheet _sheet, ExcelPackage _excelPackage)
	{
		Dictionary<string, int> indexes = GetColumnIndexes(_sheet);

		for (int rowIdx = 2; rowIdx <= _sheet.Dimension.Rows; ++rowIdx)
		{
			var stageId = _sheet.GetValue<uint>(rowIdx, indexes[nameof(GameDB.Stage_Table.StageID)] /*StageId*/);

			if (this.MapData.StageTID != stageId)
				continue;

			Vector3 boundsMin = this.MapData.MapBounds.min;
			Vector3 boundsMax = this.MapData.MapBounds.max;

			string strBoundsMin = $"{boundsMin.x},{boundsMin.z}";
			string strBoundsMax = $"{boundsMax.x},{boundsMax.z}";

			_sheet.SetValue(rowIdx, indexes[nameof(GameDB.Stage_Table.BoundsMin)] /* BoundsMin */, strBoundsMin);
			_sheet.SetValue(rowIdx, indexes[nameof(GameDB.Stage_Table.BoundsMax)] /* BoundsMax */, strBoundsMax);
		}
	}

	/// <summary> 확장함수 </summary>
	public void Save(MapData _mapData)
	{
		this.MapData = _mapData;
		this.Save();
	}
}


/// <summary> 
/// <see cref="GameDB.Portal_Table"/> 
/// </summary>
public class Tool_PortalTable : ToolTableBase<uint, Portal_Table>
{
	protected override string SheetName => nameof(Portal_Table);

	public MapData MapData { get; private set; }

	public Tool_PortalTable(string _xlsxPath) : base(_xlsxPath) { }

	protected override void OnLoad(ExcelWorksheet _sheet, ExcelPackage _excelPackage)
	{
		Dictionary<string, int> indexes = GetColumnIndexes(_sheet);

		for (int rowIdx = 2; rowIdx <= _sheet.Dimension.Rows; ++rowIdx)
		{
			//Debug.Log(_sheet.GetValue<uint>(rowIdx, indexes[nameof(GameDB.Portal_Table.PortalID)] /*PortalID*/));

			GameDB.Portal_Table tableData = new GameDB.Portal_Table()
			{
				PortalID = _sheet.GetValue<uint>(rowIdx, indexes[nameof(GameDB.Portal_Table.PortalID)] /*PortalID*/),
				PortalType = (E_PortalType)System.Enum.Parse(typeof(E_PortalType), _sheet.GetValue<string>(rowIdx, indexes[nameof(GameDB.Portal_Table.PortalType)])),
				StageID = _sheet.GetValue<uint>(rowIdx, indexes[nameof(GameDB.Portal_Table.StageID)] /*StageID*/),
				Position = _sheet.GetValue<string>(rowIdx, indexes[nameof(GameDB.Portal_Table.Position)] /* Position */),
				Radius = _sheet.GetValue<float>(rowIdx, indexes[nameof(GameDB.Portal_Table.Radius)] /* Radius */),				
			};

			this.DicTable.Add(tableData.PortalID, tableData);
		}
	}

	protected override void OnSave(ExcelWorksheet _sheet, ExcelPackage _excelPackage)
	{
		Dictionary<string, int> indexes = GetColumnIndexes(_sheet);

		for (int rowIdx = 2; rowIdx <= _sheet.Dimension.Rows; ++rowIdx)
		{
			var portalTid = _sheet.GetValue<uint>(rowIdx, indexes[nameof(Portal_Table.PortalID)] /*PortalID*/);

			// 외부데이터는 외부에서 Table에 설정한 상태로 오도록 해야한다.
			// 여기서는 Tool_Table로 가지고 있던 값을 [실 테이블] 파일로 저장하는 작업만 해야함.

			MapData.PortalInfo portalInfo = this.MapData.PortalInfos.Find(info => info.TableTID == portalTid && info.Purpose != MapData.PortalInfo.PurposeType.AttackBoss);
			if (null == portalInfo)
				continue;

			_sheet.SetValue(rowIdx, indexes[nameof(Portal_Table.Position)] /* Position */, VectorHelper.ToString(portalInfo.Position));
			_sheet.SetValue(rowIdx, indexes[nameof(Portal_Table.Radius)] /* Radius */, portalInfo.StartInRadius.ToString("F3"));
		}

		_excelPackage.Save();
	}

	/// <summary> 확장함수 </summary>
	public void Save(MapData _mapData)
	{
		this.MapData = _mapData;
		this.Save();
	}
}


/// <summary> 
/// <see cref="GameDB.Monster_Table"/> 
/// </summary>
public class Tool_MonsterTable : ToolTableBase<uint, Monster_Table>
{
	protected override string SheetName => nameof(Monster_Table);

	public MapData MapData { get; private set; }

	public Tool_MonsterTable(string _xlsxPath) : base(_xlsxPath) { }

	protected override void OnLoad(ExcelWorksheet _sheet, ExcelPackage _excelPackage)
	{
		Dictionary<string, int> indexes = GetColumnIndexes(_sheet);

		for (int rowIdx = 2; rowIdx <= _sheet.Dimension.Rows; ++rowIdx)
		{
			GameDB.Monster_Table tableData = new GameDB.Monster_Table()
			{
				MonsterID = _sheet.GetValue<uint>(rowIdx, indexes[nameof(Monster_Table.MonsterID)] /*MosnterID*/),
				PlaceStageID = _sheet.GetValue<uint>(rowIdx, indexes[nameof(Monster_Table.PlaceStageID)] /*PlaceStageID*/),
				MonsterType = (E_MonsterType)System.Enum.Parse(typeof(E_MonsterType), _sheet.GetValue<string>(rowIdx, indexes[nameof(Monster_Table.MonsterType)] /*MonsterType*/)),
				SpawnType = (E_SpawnType)System.Enum.Parse(typeof(E_SpawnType), _sheet.GetValue<string>(rowIdx, indexes[nameof(Monster_Table.SpawnType)] /* SpawnType */)),
				SpawnTime = _sheet.GetValue<float>(rowIdx, indexes[nameof(Monster_Table.SpawnTime)] /* SpawnTime */),
				ResourceID = _sheet.GetValue<uint>(rowIdx, indexes[nameof(Monster_Table.ResourceID)]),
				SearchRange = _sheet.GetValue<uint>(rowIdx, indexes[nameof(Monster_Table.SearchRange)]),
				Scale = _sheet.GetValue<uint>(rowIdx, indexes[nameof(Monster_Table.Scale)]),
			};
						
			this.DicTable.Add(tableData.MonsterID, tableData);
		}
	}

	protected override void OnSave(ExcelWorksheet _sheet, ExcelPackage _excelPackage)
	{
	}

	/// <summary> 확장함수 </summary>
	public void Save(MapData _mapData)
	{
		this.MapData = _mapData;
		this.Save();
	}
}


/// <summary> 
/// <see cref="GameDB.NPC_Table"/> 
/// </summary>
public class Tool_NPCTable : ToolTableBase<uint, NPC_Table>
{
	protected override string SheetName => nameof(NPC_Table);

	public Tool_NPCTable(string _xlsxPath) : base(_xlsxPath) { }

	protected override void OnLoad(ExcelWorksheet _sheet, ExcelPackage _excelPackage)
	{
		Dictionary<string, int> indexes = GetColumnIndexes(_sheet);

		for (int rowIdx = 2; rowIdx <= _sheet.Dimension.Rows; ++rowIdx)
		{
			GameDB.NPC_Table tableData = new GameDB.NPC_Table()
			{
				NPCID = _sheet.GetValue<uint>(rowIdx, indexes[nameof(NPC_Table.NPCID)] /*NPCID*/),
				NPCTextID = _sheet.GetValue<string>(rowIdx, indexes[nameof(NPC_Table.NPCTextID)] /*_주석*/),
				NPCType = (E_NPCType)System.Enum.Parse(typeof(E_NPCType), _sheet.GetValue<string>(rowIdx, indexes[nameof(NPC_Table.NPCType)])),
				JobType = (E_JobType)System.Enum.Parse(typeof(E_JobType), _sheet.GetValue<string>(rowIdx, indexes[nameof(NPC_Table.JobType)])),
				ResourceID = _sheet.GetValue<uint>(rowIdx, indexes[nameof(NPC_Table.ResourceID)]),
				Scale = _sheet.GetValue<uint>(rowIdx, indexes[nameof(NPC_Table.Scale)]),
			};

			this.DicTable.Add(tableData.NPCID, tableData);
		}
	}

	protected override void OnSave(ExcelWorksheet _sheet, ExcelPackage _excelPackage)
	{
	}
}

/// <summary> 
/// <see cref="GameDB.Object_Table"/> 
/// </summary>
public class Tool_ObjectTable : ToolTableBase<uint, Object_Table>
{
    protected override string SheetName => nameof(Object_Table);

    public Tool_ObjectTable(string _xlsxPath) : base(_xlsxPath) { }

    protected override void OnLoad(ExcelWorksheet _sheet, ExcelPackage _excelPackage)
    {
        Dictionary<string, int> indexes = GetColumnIndexes(_sheet);

        for (int rowIdx = 2; rowIdx <= _sheet.Dimension.Rows; ++rowIdx)
        {
            GameDB.Object_Table tableData = new GameDB.Object_Table()
            {
                ObjectID = _sheet.GetValue<uint>(rowIdx, indexes[nameof(Object_Table.ObjectID)] /*ObjectID*/),
                ObjectTextID = _sheet.GetValue<string>(rowIdx, indexes[nameof(Object_Table.ObjectTextID)] /*_주석*/),
                ObjectType = (E_ObjectType)System.Enum.Parse(typeof(E_ObjectType), _sheet.GetValue<string>(rowIdx, indexes[nameof(Object_Table.ObjectType)])),
                //NeedQuestID = _sheet.GetValue<uint>(rowIdx, indexes[nameof(Object_Table.NeedQuestID)] /*_주석*/),
                ResourceName = _sheet.GetValue<string>(rowIdx, indexes[nameof(Object_Table.ResourceName)]),
				Scale = _sheet.GetValue<uint>(rowIdx, indexes[nameof(Object_Table.Scale)]),
			};

            this.DicTable.Add(tableData.ObjectID, tableData);
        }
    }

    protected override void OnSave(ExcelWorksheet _sheet, ExcelPackage _excelPackage)
    {
    }
}

/// <summary>
/// <see cref="GameDB.Resource_Table"/>
/// </summary>
public class Tool_ResourceTable : ToolTableBase<uint, Resource_Table>
{
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
			};

			// 빈 값이면 skip
			if (0 == tableData.ResourceID)
				continue;

			this.DicTable.Add(tableData.ResourceID, tableData);
		}
	}

	protected override void OnSave(ExcelWorksheet _sheet, ExcelPackage _excelPackage)
	{
	}
}