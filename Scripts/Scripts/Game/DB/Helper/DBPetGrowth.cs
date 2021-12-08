using GameDB;
using System.Collections.Generic;
using UnityEngine;

[UnityEngine.Scripting.Preserve]
public class DBPetGrowth : IGameDBHelper
{
    // {group:{level:table}}
    private static Dictionary<uint, Dictionary<int,PetGrowth_Table>> dicListGroup = new Dictionary<uint, Dictionary<int, PetGrowth_Table>>();

    public void OnReadyData()
    {
        dicListGroup.Clear();

        foreach(var iter in GameDBManager.Container.PetGrowth_Table_data.Values)
        {
            if (dicListGroup.ContainsKey(iter.PetGrowthGroup) == false)
                dicListGroup.Add(iter.PetGrowthGroup,new  Dictionary<int, PetGrowth_Table>());

            if (dicListGroup[iter.PetGrowthGroup].ContainsKey((int)iter.PetLevel) == true)
                continue;

            dicListGroup[iter.PetGrowthGroup].Add((int)iter.PetLevel, iter);
        }
    }

    public static int GetMaxLevel(uint group)
    {
        if (dicListGroup.ContainsKey(group) == false)
            return 0;

        return dicListGroup[group].Count;   
    }

    public static bool GetData(uint group, int level, out PetGrowth_Table table)
    {
        if (dicListGroup.ContainsKey(group) == false)
        {
            table = null;
            return false;
        }

        return dicListGroup[group].TryGetValue(level, out table);
    }
}