using GameDB;
using System.Collections.Generic;

[UnityEngine.Scripting.Preserve]
public class DBMonster : IGameDBHelper
{
	private static Dictionary<uint, List<MonsterDrop_Table>> dicDropGroup = new Dictionary<uint, List<MonsterDrop_Table>>();

	public void OnReadyData()
	{
		dicDropGroup.Clear();

		foreach(var iter in GameDBManager.Container.MonsterDrop_Table_data.Values)
        {
			if (dicDropGroup.ContainsKey(iter.DropGroupID) == false)
				dicDropGroup.Add(iter.DropGroupID, new List<MonsterDrop_Table>());

			dicDropGroup[iter.DropGroupID].Add(iter);
        }
	}

	public static Dictionary<uint, Monster_Table> DicMonster
	{
		get { return GameDBManager.Container.Monster_Table_data; }
	}

	public static bool TryGet(uint _tid, out Monster_Table outTable)
	{
		return GameDBManager.Container.Monster_Table_data.TryGetValue(_tid, out outTable);
	}

	public static Monster_Table Get(uint _tid)
	{
		if (GameDBManager.Container.Monster_Table_data.TryGetValue(_tid, out var foundTable))
		{
			return foundTable;
		}
		return null;
	}

	public static bool GetDropTableList(uint dropGrouptId, out List<MonsterDrop_Table> list)
    {
		return dicDropGroup.TryGetValue(dropGrouptId, out list);
    }
}
