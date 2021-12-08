using GameDB;
using System.Collections.Generic;
using ZDefine;
using ZNet.Data;

[UnityEngine.Scripting.Preserve]
class DBChangeQuest : IGameDBHelper
{
    public void OnReadyData()
    {

    }

    public static bool GetChangeQuest(uint tid, out ChangeQuest_Table table)
    {
        return GameDBManager.Container.ChangeQuest_Table_data.TryGetValue(tid, out table);
    }

    public static bool GetChangeQuestLevel(uint tid, out ChangeQuestLevel_Table table)
    {
        return GameDBManager.Container.ChangeQuestLevel_Table_data.TryGetValue(tid, out table);
    }

    public static ChangeQuestLevel_Table GetChangeQuestLevelByLevel(uint level)
    {
        foreach (var table in GameDBManager.Container.ChangeQuestLevel_Table_data.Values)
        {
            if (table.ChangeQuestLevel != level)
                continue;

            return table;
        }

        return null;
    }

    /// <summary> 현재 레벨의 총 경험치량을 얻어온다. </summary>
    public static uint GetChangeQuestExpByCurrentLevel(uint level)
    {
        if (1 == level)
            return GetChangeQuestLevelByLevel(level).LevelUpCount;

        uint exp = GetChangeQuestLevelByLevel(level).LevelUpCount;
        exp = exp - GetChangeQuestLevelByLevel(level - 1).LevelUpCount;

        return exp;
    }

    /// <summary> 이전 레벨의 경험치량 </summary>
    public static uint GetChangeQuestExpByPreLevel(uint level)
    {
        level = level - 1;
        if (level > 0)
        {
            return GetChangeQuestLevelByLevel(level).LevelUpCount;
        }

        return 0;
    }

    public static bool CheckExpireTime(ulong expireTime)
    {
        System.DateTime LastCheckDate = TimeHelper.ParseTimeStamp((long)(TimeManager.NowSec));
        //다음날 초기화 까지 남은 시간
        long nextTime = (long)(TimeHelper.DaySecond - (ulong)(LastCheckDate.Hour * (int)TimeHelper.HourSecond + LastCheckDate.Minute * (int)TimeHelper.MinuteSecond + LastCheckDate.Second)) + DBConfig.Event_Reset_Time * (int)TimeHelper.HourSecond;
        //전날 초기화 시간
        long preTime = (long)TimeManager.NowSec + nextTime - (long)TimeHelper.DaySecond;

        return preTime > (long)expireTime;
    }

    public static bool CheckChangeDispatchable(ChangeData changeData, byte compGrade, E_ChangeQuestType compType)
    {
        if (changeData == null)
            return false;

        if (DBChange.TryGet(changeData.ChangeTid, out var table) == false)
            return false;

        if (changeData.ChangeId <= 0)
            return false;

        if (changeData.ChangeQuestTid > 0)//파견중 아웃!
            return false;

        if (CheckExpireTime(changeData.ChangeQuestExpireDt) == false)
            return false;

        if (table.Grade < compGrade)
            return false;

        if (compType.HasFlag(table.ChangeQuestType) == false)
            return false;
        return true;
    }

    public static Change_Table GetDisPatchableChange(E_ChangeQuestType type, byte compGrade, List<uint> listRegistedId)
    {
        var tempList = new List<Change_Table>();// 사용가능한 강림체 리스트

        // (type,grade):cnt
        var dicTypeCount = new Dictionary<(E_ChangeQuestType, byte), int>();

        bool isMultiType = type.HasFlag(E_ChangeQuestType.AttackLong) && type.HasFlag(E_ChangeQuestType.AttackShort);

        foreach (var iter in Me.CurCharData.GetChangeDataList())
        {
            if (DBChange.TryGet(iter.ChangeTid, out var changeTable) == false)
                continue;

            if (listRegistedId.Contains(iter.ChangeTid))
                continue;

            if (DBChangeQuest.CheckChangeDispatchable(iter, compGrade, type) == false)
                continue;

            if (isMultiType)
            {
                if (dicTypeCount.ContainsKey((changeTable.ChangeQuestType, changeTable.Grade)) == false)
                    dicTypeCount.Add((changeTable.ChangeQuestType, changeTable.Grade), 0);

                dicTypeCount[(changeTable.ChangeQuestType, changeTable.Grade)]++;
            }

            tempList.Add(changeTable);
        }

        if (tempList.Count <= 0)
        {
            return null;
        }

        tempList.Sort((x, y) =>
        {
            // 낮은순으로 정렬
            if (x.Grade < y.Grade)
                return -1;

            if (x.Grade > y.Grade)
                return 1;

            // 복수 타입일시
            if (isMultiType)
            {
                bool hasX = dicTypeCount.TryGetValue((x.ChangeQuestType, x.Grade), out int xCnt);
                bool hasY = dicTypeCount.TryGetValue((y.ChangeQuestType, y.Grade), out int yCnt);

                if (hasX && hasY)// 모두 있음
                {
                    // 모두 있다면 수량높은것 우선
                    if (xCnt > yCnt)
                        return -1;

                    if (xCnt < yCnt)
                        return 1;
                }
                else
                {
                    // 둘중하나만있다면 있는것 우선순위로 올림
                    if (hasX)
                        return -1;
                    if (hasY)
                        return 1;
                }
            }
            // 아닐시 등급만 비교
            return 0;
        });
        return tempList[0];
    }
}
