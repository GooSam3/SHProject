using GameDB;
using System.Collections.Generic;
using UnityEngine;

[UnityEngine.Scripting.Preserve]
public class DBPetLevel : IGameDBHelper
{
    private static Dictionary<uint, List<PetLevel_Table>> LevelDic = new Dictionary<uint, List<PetLevel_Table>>();

    public void OnReadyData()
    {
        LevelDic.Clear();

        foreach (PetLevel_Table table in GameDBManager.Container.PetLevel_Table_data.Values)
        {
            if (!LevelDic.ContainsKey(table.PetExpGroup))
                LevelDic.Add(table.PetExpGroup, new List<PetLevel_Table>());

            LevelDic[table.PetExpGroup].Add(table);
        }

        foreach (var key in LevelDic.Keys)
        {
            LevelDic[key].Sort((x, y) => {
                if (x.PetLevel < y.PetLevel)
                    return -1;
                else if (x.PetLevel > y.PetLevel)
                    return 1;

                return 0;
            });
        }
    }

    public static PetLevel_Table Get(uint PetExpGroup, uint level)
    {
        int findIndex = (int)level - 1; // index 0이 1 Level이다.

        if (findIndex >= 0 && findIndex < LevelDic[PetExpGroup].Count)
        {
            return LevelDic[PetExpGroup][findIndex];
        }
        else
        {
            return null;
        }
    }

    public static bool IsMaxLevel(uint PetExpGroup, uint level)
    {
        int levelIdx = Mathf.Clamp((int)level - 1, 0, LevelDic[PetExpGroup].Count - 1);

        return LevelDic[PetExpGroup][levelIdx].PetLevelUpType == E_PetLevelUpType.End;
    }

    /// <summary>다음 레벨업이 존재하는지 여부</summary>
    public static bool IsExistLevelUp(uint PetExpGroup, uint level)
    {
        int levelIdx = Mathf.Clamp((int)level - 1, 0, LevelDic[PetExpGroup].Count);

        return LevelDic[PetExpGroup][levelIdx].PetLevelUpType == E_PetLevelUpType.Up;
    }

    public static ulong GetNeedExp(uint PetExpGroup, uint level)
    {
        int findIndex = (int)level - 1; // index 0이 1 Level이다.

        if (findIndex >= 0 && findIndex < LevelDic[PetExpGroup].Count)
        {
            return LevelDic[PetExpGroup][findIndex].PetExp;
        }
        else
        {
            return 0;
        }
    }

    public static byte GetLevel(uint PetExpGroup, ulong Exp)
    {
        byte returnLv = 0;
        foreach (var tableData in LevelDic[PetExpGroup])
        {
            if (tableData.PetExp <= Exp && tableData.PetLevel > returnLv)
                returnLv = tableData.PetLevel;
        }
        return returnLv;
    }

    public static double GetExpRate(ulong CheckExp, uint CheckLv, uint PetExpGroup)
    {
        ulong curLevelUpExp = GetNeedExp(PetExpGroup, CheckLv);
        ulong nextLevelUpExp = GetNeedExp(PetExpGroup, CheckLv + 1);

        ulong checkMaxExp = 0;

        if (nextLevelUpExp == 0)
        {
            //만렙
            checkMaxExp = curLevelUpExp;
        }
        else
            checkMaxExp = nextLevelUpExp - curLevelUpExp;

        if (checkMaxExp < CheckExp)
        {
            return 1 + GetExpRate(CheckExp - checkMaxExp, CheckLv + 1, PetExpGroup);
        }

        return (double)CheckExp / (double)checkMaxExp;
    }
}