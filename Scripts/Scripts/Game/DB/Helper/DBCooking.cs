using GameDB;
using System.Collections.Generic;
using System.Linq;

[UnityEngine.Scripting.Preserve]
public class DBCooking : IGameDBHelper
{
	public void OnReadyData()
	{
	}

	public static Dictionary<uint, Cooking_Table> DicCooking
	{
		get { return GameDBManager.Container.Cooking_Table_data; }
	}

	public static bool TryGet(uint _tid, out Cooking_Table outTable)
	{
		return GameDBManager.Container.Cooking_Table_data.TryGetValue((byte)_tid, out outTable);
	}

	public static Cooking_Table Get(uint _tid)
	{
		if (GameDBManager.Container.Cooking_Table_data.TryGetValue((byte)_tid, out var foundTable))
		{
			return foundTable;
		}
		return null;
	}

	public static bool GetAllTable(out List<Cooking_Table> table)
	{
		table = GameDBManager.Container.Cooking_Table_data.Values.ToList();
		return table != null;
	}
}