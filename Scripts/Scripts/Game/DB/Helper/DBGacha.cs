using System.Collections.Generic;
using GameDB;

[UnityEngine.Scripting.Preserve]
public class DBGacha : IGameDBHelper
{
    private static Dictionary<uint, List<GameDB.Gacha_Table>> dicGachaGroupTable = new Dictionary<uint, List<Gacha_Table>>();

    public void OnReadyData()
    {
        dicGachaGroupTable.Clear();
        Dictionary<uint, Gacha_Table>.Enumerator it = GameDBManager.Container.Gacha_Table_data.GetEnumerator();
        while(it.MoveNext())
		{
            if (dicGachaGroupTable.ContainsKey(it.Current.Value.GroupID) == false)
			{
                dicGachaGroupTable.Add(it.Current.Value.GroupID, new List<Gacha_Table>());
			}

            dicGachaGroupTable[it.Current.Value.GroupID].Add(it.Current.Value);
		}
    }

    public static bool TryGet(uint _tid, out GameDB.Gacha_Table outTable)
    {
        return GameDBManager.Container.Gacha_Table_data.TryGetValue(_tid, out outTable);
    }

    public static GameDB.Gacha_Table Get(uint _tid)
    {
        if (GameDBManager.Container.Gacha_Table_data.TryGetValue(_tid, out var foundTable))
            return foundTable;

        ZLog.LogError(ZLogChannel.System, $"해당 TempleID[{_tid}]가 테이블에 존재하지 않습니다.");

        return null;
    }

    public static List<ListGroup_Table> GetCachaListGroupID(uint _groupID)
	{
        List<ListGroup_Table>   gachaListGroupID = new List<ListGroup_Table>();
        List<Gacha_Table>      gachaGroup = null;

        if (dicGachaGroupTable.ContainsKey(_groupID))
		{
            gachaGroup = dicGachaGroupTable[_groupID];

            for (int i = 0; i < gachaGroup.Count; i++)
			{
                List<ListGroup_Table> listGroupID = DBListGroup.GetListGroup(gachaGroup[i].ListGroupID);
                for (int j = 0; j < listGroupID.Count; j++)
				{
                    gachaListGroupID.Add(listGroupID[j]);
				}
			}
		}

        return gachaListGroupID;
	}
    
}
