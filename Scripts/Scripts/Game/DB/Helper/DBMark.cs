using GameDB;
using System.Collections.Generic;

[UnityEngine.Scripting.Preserve]
public class DBMark : IGameDBHelper
{
    static Dictionary<GameDB.E_MarkAbleType, List<Mark_Table>> markableTypeDic = new Dictionary<GameDB.E_MarkAbleType, List<Mark_Table>>();

    public void OnReadyData()
    {
        markableTypeDic.Clear();

        foreach (Mark_Table tableData in GameDBManager.Container.Mark_Table_data.Values)
        {
            if (!markableTypeDic.ContainsKey(tableData.MarkAbleType))
                markableTypeDic.Add(tableData.MarkAbleType, new List<Mark_Table>());

            markableTypeDic[tableData.MarkAbleType].Add(tableData);
        }
    }

    public static Mark_Table GetMarkData(uint MarkTid)
    {
        if (GameDBManager.Container.Mark_Table_data.ContainsKey(MarkTid))
            return GameDBManager.Container.Mark_Table_data[MarkTid];

        //ZLog.LogError("GetMarkData - Can't Find MarkData : "+MarkTid);
        return null;
    }

    /// <summary>
    /// 해당 Mark 를 포함한 하위 마크들의 모든 AbilityAction ID 를 Add (Step기준)
    /// </summary>
    public static bool AddAbilityActionsStackedAscending(uint tid, ref List<uint> outputAbilityActionIDs)
    {
        var targetMark = GetMarkData(tid);

        if (targetMark == null
            || targetMark.MarkAbleType == GameDB.E_MarkAbleType.None)
            return false;

        var targetList = markableTypeDic[targetMark.MarkAbleType];

        if (targetList == null)
            return false;

        if (outputAbilityActionIDs == null)
            outputAbilityActionIDs = new List<uint>();

        for (int i = 0; i < targetList.Count; i++)
        {
            /// Target 의 이하 레벨인 경우 ++ 
            if (targetMark.Step >= targetList[i].Step)
            {
                if (targetList[i].AbilityActionID_01 > 0)
                {
                    outputAbilityActionIDs.Add(targetList[i].AbilityActionID_01);
                }
                if (targetList[i].AbilityActionID_02 > 0)
                {
                    outputAbilityActionIDs.Add(targetList[i].AbilityActionID_02);
                }
            }
        }

        return true;
    }

    public static string GetToolTipTextID(GameDB.E_MarkAbleType type)
    {
        if (markableTypeDic.ContainsKey(type) == false)
            return string.Empty;
        if (markableTypeDic[type].Count == 0)
            return string.Empty;

        return markableTypeDic[type][0].ToolTipID;
    }

    public static Dictionary<GameDB.E_MarkAbleType, List<Mark_Table>>.KeyCollection GetMarkAbleTypes()
    {
        return markableTypeDic.Keys;
    }

    public static IList<Mark_Table> GetMarkAbleTypeDatas(GameDB.E_MarkAbleType getType)
    {
        if (markableTypeDic.ContainsKey(getType))
            return markableTypeDic[getType].AsReadOnly();

        // ZLog.LogError("GetMarkAbleTypeDatas - Can't Find MarkAbleType : "+getType);
        return null;
    }

    public static IList<Mark_Table> GetMarkAbleTypeUniqueDatas(GameDB.E_MarkAbleType getType)
    {
        if (markableTypeDic.ContainsKey(getType))
            return markableTypeDic[getType].FindAll(item => item.MarkUniqueType == E_MarkUniqueType.Unique).AsReadOnly();

        //ZLog.LogError("GetMarkAbleTypeUniqueDatas - Can't Find MarkAbleType : " + getType);
        return null;
    }

    public static byte GetMarkTypeNormalMinStep(GameDB.E_MarkAbleType getType)
    {
        byte MinStep = byte.MaxValue;
        foreach (Mark_Table tableData in GetMarkAbleTypeDatas(getType))
        {
            if (tableData.MarkUniqueType == E_MarkUniqueType.Unique)
                continue;

            if (tableData.Step < MinStep)
                MinStep = tableData.Step;
        }

        return MinStep;
    }

    public static byte GetMarkTypeNormalMaxStep(GameDB.E_MarkAbleType getType)
    {
        byte MaxStep = 0;
        foreach (Mark_Table tableData in GetMarkAbleTypeDatas(getType))
        {
            if (tableData.MarkUniqueType == E_MarkUniqueType.Unique)
                continue;

            if (tableData.Step > MaxStep)
                MaxStep = tableData.Step;
        }

        return MaxStep;
    }

    public static Mark_Table GetMarkAbleTypeData(GameDB.E_MarkAbleType getType, byte Step)
    {
        foreach (Mark_Table tableData in GetMarkAbleTypeDatas(getType))
        {
            if (tableData.Step == Step)
                return tableData;
        }

        //ZLog.LogError("GetMarkAbleTypeData - Can't Find MarkAbleType : " + getType+" Step : "+Step);
        return null;
    }

    public static GameDB.E_MarkAbleType GetMarkType(uint MarkTid)
    {
        return GetMarkData(MarkTid)?.MarkAbleType ?? default;
    }

    public static byte GetMarkStep(uint MarkTid)
    {
        return GetMarkData(MarkTid)?.Step ?? default;
    }

    public static uint GetMarkTidByStep(GameDB.E_MarkAbleType type, byte step)
    {
        if (markableTypeDic.ContainsKey(type) == false)
            return 0;

        foreach (var mark in markableTypeDic[type])
        {
            if (mark.Step == step)
            {
                return mark.MarkID;
            }
        }

        return 0;
    }

    public static Mark_Table GetMarkDataByStep(GameDB.E_MarkAbleType type, byte step)
    {
        if (markableTypeDic.ContainsKey(type) == false)
            return null;

        foreach (var mark in markableTypeDic[type])
        {
            if (mark.Step == step)
            {
                return mark;
            }
        }

        return null;
    }

    public static bool IsUniqueType(uint MarkTid)
    {
        return GetMarkData(MarkTid)?.MarkUniqueType == E_MarkUniqueType.Unique;
    }
}
