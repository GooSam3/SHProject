using System.Collections.Generic;
using System.IO;

/// <summary>
/// 맵 에디터에서 사용하는 테이블들
/// </summary>
public class MapToolDB
{
	public bool IsLoaded { get; private set; } = false;

	public Tool_StageTable StageTable { get; private set; } = new Tool_StageTable("");
	public Tool_PortalTable PortalTable { get; private set; } = new Tool_PortalTable("");
	public Tool_MonsterTable MonsterTable { get; private set; } = new Tool_MonsterTable("");
	public Tool_NPCTable NPCTable { get; private set; } = new Tool_NPCTable("");
    public Tool_ObjectTable ObjectTable { get; private set; } = new Tool_ObjectTable("");
    public Tool_ResourceTable ResourceTable { get; private set; } = new Tool_ResourceTable("");

	private List<IToolTableBase> mListTables = new List<IToolTableBase>();

	public void LoadTables(string _dbTableFolder)
	{
		StageTable = (Tool_StageTable)LoadTable(new Tool_StageTable(Path.Combine(_dbTableFolder, "Stage_table.xlsx")));
		PortalTable = (Tool_PortalTable)LoadTable(new Tool_PortalTable(Path.Combine(_dbTableFolder, "Portal_table.xlsx")));
		MonsterTable = (Tool_MonsterTable)LoadTable(new Tool_MonsterTable(Path.Combine(_dbTableFolder, "Monster_table.xlsx")));
		NPCTable = (Tool_NPCTable)LoadTable(new Tool_NPCTable(Path.Combine(_dbTableFolder, "NPC_table.xlsx")));
        ObjectTable = (Tool_ObjectTable)LoadTable(new Tool_ObjectTable(Path.Combine(_dbTableFolder, "Object_table.xlsx")));
        ResourceTable = (Tool_ResourceTable)LoadTable(new Tool_ResourceTable(Path.Combine(_dbTableFolder, "Resource_table.xlsx")));

		IsLoaded = true;
	}

	private object LoadTable<KEY, VALUE>(ToolTableBase<KEY, VALUE> _newTable) where VALUE : class
	{
		_newTable.Load();

		mListTables.Add(_newTable);

		return _newTable;
	}

	public void Clear()
	{
		foreach (var table in mListTables)
		{
			table?.Clear();
		}
	}
}
