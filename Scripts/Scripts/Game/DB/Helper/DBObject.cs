using GameDB;
using System.Collections.Generic;

[UnityEngine.Scripting.Preserve]
public class DBObject : IGameDBHelper
{
	public void OnReadyData()
	{
	}

	public static Dictionary<uint, Object_Table> DicObject
    {
		get { return GameDBManager.Container.Object_Table_data; }
	}

	public static bool TryGet(uint _tid, out Object_Table outTable)
	{
		return GameDBManager.Container.Object_Table_data.TryGetValue(_tid, out outTable);
	}

	public static Object_Table Get(uint _tid)
	{
		if (GameDBManager.Container.Object_Table_data.TryGetValue(_tid, out var foundTable))
		{
			return foundTable;
		}
		return null;
	}
}
