
using GameDB;
using System.Collections.Generic;

[UnityEngine.Scripting.Preserve]
public class DBPortal : IGameDBHelper
{
	private static Dictionary<uint, List<Portal_Table>> dicStagePortal = new Dictionary<uint, List<Portal_Table>>();

	public void OnReadyData()
	{
		foreach(var iter in GameDBManager.Container.Portal_Table_data)
        {
			var portal = iter.Value;

			if (dicStagePortal.ContainsKey(portal.StageID) == false)
				dicStagePortal.Add(portal.StageID, new List<Portal_Table>());
			
			dicStagePortal[portal.StageID].Add(portal);
        }
	}

	public static bool TryGet(uint _tid, out GameDB.Portal_Table outTable)
	{
		return GameDBManager.Container.Portal_Table_data.TryGetValue(_tid, out outTable);
	}

	public static GameDB.Portal_Table Get(uint _tid)
	{
		if (GameDBManager.Container.Portal_Table_data.TryGetValue(_tid, out var foundTable))
			return foundTable;

		ZLog.LogError(ZLogChannel.System, $"해당 [{_tid}]가 {nameof(GameDB.Portal_Table)}에 존재하지 않습니다.");

		return null;
	}

	public static bool GetFromStageTid(uint _tid, out List<Portal_Table> list)
    {
		return dicStagePortal.TryGetValue(_tid, out list);
    }
}
