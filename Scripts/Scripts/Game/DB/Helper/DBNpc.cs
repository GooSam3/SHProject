using GameDB;
using System.Collections.Generic;

[UnityEngine.Scripting.Preserve]
public class DBNpc : IGameDBHelper
{
	public void OnReadyData()
	{
	}

	public static Dictionary<uint, NPC_Table> DicNpc
	{
		get { return GameDBManager.Container.NPC_Table_data; }
	}

	public static bool TryGet(uint _tid, out NPC_Table outTable)
	{
		return GameDBManager.Container.NPC_Table_data.TryGetValue(_tid, out outTable);
	}

	public static NPC_Table Get(uint _tid)
	{
		if (GameDBManager.Container.NPC_Table_data.TryGetValue(_tid, out var foundTable))
		{
			return foundTable;
		}
		return null;
	}
}
