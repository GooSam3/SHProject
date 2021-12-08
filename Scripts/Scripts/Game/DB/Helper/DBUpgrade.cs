using GameDB;
using System.Collections.Generic;

[UnityEngine.Scripting.Preserve]
public class DBUpgrade : IGameDBHelper
{
    static Dictionary<uint, List<UpgradeList_Table>> UpgradeGroupDic = new Dictionary<uint, List<UpgradeList_Table>>();

    public void OnReadyData()
    {
        UpgradeGroupDic.Clear();

        foreach (UpgradeList_Table tableData in GameDBManager.Container.UpgradeList_Table_data.Values)
        {
            if (!UpgradeGroupDic.ContainsKey(tableData.GroupID))
                UpgradeGroupDic.Add(tableData.GroupID, new List<UpgradeList_Table>());
            UpgradeGroupDic[tableData.GroupID].Add(tableData);
        }
    }

    public static IList<UpgradeList_Table> GetUpgradeList(uint GroupId)
    {
        if (UpgradeGroupDic.ContainsKey(GroupId))
            return UpgradeGroupDic[GroupId].AsReadOnly();

        //ZLog.LogError("GetUpgradeList - Can't Find UpgradeGroup : " + GroupId);
        return null;
    }

    public static uint GetItemTid(uint UpgradeListTid)
    {
        if (!GameDBManager.Container.UpgradeList_Table_data.ContainsKey(UpgradeListTid))
        {
            //ZLog.LogError("GetItemTid - can't find upgradeData : " + UpgradeListTid);
            return 0;
        }

        return GameDBManager.Container.UpgradeList_Table_data[UpgradeListTid].UpgradeItemID;
    }
}
