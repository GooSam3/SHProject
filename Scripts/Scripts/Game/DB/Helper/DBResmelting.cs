using GameDB;
using System.Collections.Generic;
using UnityEngine;

[UnityEngine.Scripting.Preserve]
public class DBResmelting : IGameDBHelper
{
    private static Dictionary<uint, List<SmeltScrollOption_Table>> CachedResmeltingScrollGroups = new Dictionary<uint, List<SmeltScrollOption_Table>>();
    public void OnReadyData()
    {
        CachedResmeltingScrollGroups.Clear();

        foreach(var pair in GameDBManager.Container.SmeltScrollOption_Table_data)
        {
            if(false == CachedResmeltingScrollGroups.ContainsKey(pair.Value.GroupID))
                CachedResmeltingScrollGroups.Add(pair.Value.GroupID, new List<SmeltScrollOption_Table>() {});

            CachedResmeltingScrollGroups[pair.Value.GroupID].Add(pair.Value);
        }
    }

    public static bool GetResmeltScrollOption(uint tid, out SmeltScrollOption_Table table)
    {
        return GameDBManager.Container.SmeltScrollOption_Table_data.TryGetValue(tid, out table);
    }

    public static SmeltScrollOption_Table GetResmeltingScrollOption(uint tid)
    {
        if (GameDBManager.Container.Pet_Table_data.ContainsKey(tid))
            return GameDBManager.Container.SmeltScrollOption_Table_data[tid];

       // ZLog.LogError("Get - GetResmeltingScrollOption Id Find Fail : " + tid);
        return null;
    }

    public static bool GetResmeltingScroll(uint tid, out SmeltScroll_Table table)
    {
        return GameDBManager.Container.SmeltScroll_Table_data.TryGetValue(tid, out table);
    }

    public static SmeltScroll_Table GetResmeltingScroll(uint tid)
    {
        if (GameDBManager.Container.Pet_Table_data.ContainsKey(tid))
            return GameDBManager.Container.SmeltScroll_Table_data[tid];

       // ZLog.LogError("Get - GetResmeltingScroll Id Find Fail : " + tid);
        return null;
    }

    /// <summary> 스크롤 Item Tid와 재련할 Item Type으로 테이블을 얻는다. </summary>
    public static SmeltScroll_Table GetResmeltingScroll(uint scrollItemTid, E_ItemType itemType)
    {
        foreach(var table in GameDBManager.Container.SmeltScroll_Table_data)
        {
            if (false == table.Value.SmeltItemID.Contains(scrollItemTid))
                continue;

            if (table.Value.ItemType != itemType)
                continue;

            return table.Value;
        }

        return null;
    }

    /// <summary> 유효한 확률정보를 얻어온다. 0% 가 아닌 </summary>
    public static List<SmeltOptionRate_Table> GetVaildResmeltingOptionRateData(uint groupId)
    {
        List<SmeltOptionRate_Table> list = new List<SmeltOptionRate_Table>();
        foreach(var table in GameDBManager.Container.SmeltOptionRate_Table_data)
        {
            if (table.Value.SmeltOptionRateGroupID != groupId)
                continue;

            if (0 >= table.Value.Rate)
                continue;

            list.Add(table.Value);
        }

        return list;
    }

    public static KeyValuePair<int, int> GetResmeltingOptionGroupOrder(uint tid)
    {
        var table = GetResmeltingScrollOption(tid);
        
        return GetResmeltingOptionGroupOrder(table);
    }

    public static KeyValuePair<int, int> GetResmeltingOptionGroupOrder(SmeltScrollOption_Table table)
    {
        uint groupId = table.GroupID;
        int max = CachedResmeltingScrollGroups[groupId].Count;
        int current = CachedResmeltingScrollGroups[groupId].FindIndex(0, max, (value) =>
        {
            return value.SmeltScrollOptionID == table.SmeltScrollOptionID;
        });

        return new KeyValuePair<int, int>(Mathf.Min(current + 1, max), max);
    }

    public static List<SmeltScrollOption_Table> GetScrollOptionByGroup(uint groupId)
    {
        return CachedResmeltingScrollGroups[groupId];
    }

    public static uint GetResmeltingAbilityActionId(uint smeltOptionTid)
    {
        if (0 >= smeltOptionTid)
            return 0;
        return GameDBManager.Container.SmeltScrollOption_Table_data[smeltOptionTid].AbilityActionID;
    }   
}
