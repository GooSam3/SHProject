using GameDB;
using System.Collections.Generic;
using UnityEngine;

[UnityEngine.Scripting.Preserve]
public class DBPet : IGameDBHelper
{
    static Dictionary<byte, List<Pet_Table>> petDicByGrade = new Dictionary<byte, List<Pet_Table>>();

    static Dictionary<uint, List<PetList_Table>> petListGroupDic = new Dictionary<uint, List<PetList_Table>>();

    static Dictionary<byte, List<Pet_Table>> rideDicByGrade = new Dictionary<byte, List<Pet_Table>>();

    static Dictionary<uint, List<PetList_Table>> rideListGroupDic = new Dictionary<uint, List<PetList_Table>>();

    static List<Pet_Table> listPetData = new List<Pet_Table>();
    static List<Pet_Table> listRideData = new List<Pet_Table>();

    public void OnReadyData()
    {
        petDicByGrade.Clear();
        rideDicByGrade.Clear();

        listPetData.Clear();
        listRideData.Clear();

        foreach (Pet_Table tableData in GameDBManager.Container.Pet_Table_data.Values)
        {
            if(tableData.PetType == E_PetType.Pet)
            {
                if (!petDicByGrade.ContainsKey(tableData.Grade))
                    petDicByGrade.Add(tableData.Grade, new List<Pet_Table>());

                petDicByGrade[tableData.Grade].Add(tableData);
                listPetData.Add(tableData);
            }
            else if(tableData.PetType == E_PetType.Vehicle)
            {
                if (!rideDicByGrade.ContainsKey(tableData.Grade))
                    rideDicByGrade.Add(tableData.Grade, new List<Pet_Table>());

                rideDicByGrade[tableData.Grade].Add(tableData);
                listRideData.Add(tableData);
            }

        }

        petListGroupDic.Clear();
        rideListGroupDic.Clear();

        // 탈것은 다시확ㅇ니

        foreach (var tableData in GameDBManager.Container.PetList_Table_data.Values)
        {
            if (!petListGroupDic.ContainsKey(tableData.GroupID))
                petListGroupDic.Add(tableData.GroupID, new List<PetList_Table>());

            petListGroupDic[tableData.GroupID].Add(tableData);
        }
    }

    public static bool TryGet(uint tid, out Pet_Table table)
    {
        return GameDBManager.Container.Pet_Table_data.TryGetValue(tid, out table);
    }

    public static Pet_Table GetPetData(uint tid)
    {
        if (GameDBManager.Container.Pet_Table_data.ContainsKey(tid))
            return GameDBManager.Container.Pet_Table_data[tid];

        return null;
    }

    public static string GetPetIcon(uint tid)
    {
        if (GameDBManager.Container.Pet_Table_data.ContainsKey(tid))
            return GameDBManager.Container.Pet_Table_data[tid].Icon;

        return "";
    }

    public static byte GetPetGrade(uint tid)
    {
        if (GameDBManager.Container.Pet_Table_data.ContainsKey(tid))
            return GameDBManager.Container.Pet_Table_data[tid].Grade;

        return 0;
    }

    public static string GetPetName(uint tid)
    {
        if (GameDBManager.Container.Pet_Table_data.ContainsKey(tid))
            return GameDBManager.Container.Pet_Table_data[tid].PetTextID;

        return "";
    }

    public static string GetPetFullName(uint tid, int enchant = 0)
    {
        if (GameDBManager.Container.Pet_Table_data.TryGetValue(tid, out var table))
        {
            return DBUIResouce.GetPetGradeFormat(DBLocale.GetText(table.PetTextID), table.Grade);
        }

        //ZLog.LogError("GetPetName - PetID Find Fail : " + tid);
        return "";
    }

    public static List<Pet_Table> GetAllPetData()
    {
        return listPetData;
    }

    public static List<Pet_Table> GetAllRideData()
    {
        return listRideData;
    }

    public static Dictionary<byte, List<Pet_Table>>.KeyCollection GetPetGrades()
    {
        return petDicByGrade.Keys;
    }

    static List<Pet_Table> totalPetList = new List<Pet_Table>();
    public static IList<Pet_Table> GetPetDatas(byte Grade)//0: all
    {
        if (Grade == 0)
        {
            if (totalPetList.Count <= 0)
                totalPetList.AddRange(GetAllPetData());
            return totalPetList.AsReadOnly();
        }

        if (petDicByGrade.ContainsKey(Grade))
            return petDicByGrade[Grade].AsReadOnly();

        return null;
    }

    static List<Pet_Table> totalRideList = new List<Pet_Table>();
    public static IList<Pet_Table> GetRideDatas(byte grade)
    {
        if (grade == 0)
        {
            if (totalRideList.Count <= 0)
                totalRideList.AddRange(GetAllRideData());
            return totalRideList.AsReadOnly();
        }

        if (rideDicByGrade.ContainsKey(grade))
            return rideDicByGrade[grade].AsReadOnly();

        return null;
    }

    public static Pet_Table GetRandomPet(byte Grade)
    {
        if (petDicByGrade.ContainsKey(Grade))
        {
            int countdown = 100;
            while (countdown > 0)
            {
                int index = Random.Range(0, petDicByGrade[Grade].Count - 1);
                Pet_Table table = petDicByGrade[Grade][index];

                --countdown;

                if (table.ViewType == E_ViewType.NotView)
                    continue;

                return table;
            }
        }

        return null;
    }

    public static List<PetList_Table> GetPetList(uint GroupID)
    {
        if (petListGroupDic.ContainsKey(GroupID))
            return petListGroupDic[GroupID];

        return null;
    }

    ///// <summary> 펫에게 적용된 어빌리티 정보를 얻어온다. <minvalue, maxvalue, AbilityActionId> </summary>
    //public static StatForPetUI GetPetAbilityValues(uint petTid, bool bIncludeTable = true, bool bIncludeNetData = true, bool bInclueRune = true, bool bDefaultLevel = false, byte customPetLevel = 0)
    //{
    //    return GetPetAbilityValues(GetPetData(petTid), NetData.Instance.GetPet(NetData.UserID, NetData.CharID, petTid), bIncludeTable, bIncludeNetData, bInclueRune, bDefaultLevel, customPetLevel);
    //}

    ///// <summary> 펫에게 적용된 어빌리티 정보를 얻어온다. <minvalue, maxvalue, AbilityActionId> </summary>
    //public static StatForPetUI GetPetAbilityValues(Pet_Table petTable, ZDefine.PetData petData, bool bIncludeTable = true, bool bIncludeNetData = true, bool bInclueRune = true, bool bDefaultLevel = false, byte customPetLevel = 0)
    //{
    //    if (null == petTable)
    //        return null;

    //    List<ZDefine.PetRuneData> RuneDatas = null;

    //    //Rune 어빌리티
    //    if (bInclueRune)
    //    {
    //        RuneDatas = NetData.Instance.GetEquipRuneList(NetData.UserID, NetData.CharID, petTable.PetID);
    //    }

    //    return GetPetAbilityValues(petTable, petData, RuneDatas, bIncludeTable, bIncludeNetData, bDefaultLevel, customPetLevel);

    //}

    ///// <summary> 펫에게 적용된 어빌리티 정보를 얻어온다. <minvalue, maxvalue, AbilityActionId> </summary>
    //public static StatForPetUI GetPetAbilityValues(Pet_Table petTable, ZDefine.PetData petData, List<ZDefine.PetRuneData> petRuneStats, bool bIncludeTable = true, bool bIncludeNetData = true, bool bDefaultLevel = false, byte customPetLevel = 0)
    //{
    //    StatForPetUI stat = new StatForPetUI();

    //    //테이블 어빌리티
    //    if (bIncludeTable)
    //    {
    //        //성장 관련 어빌리티
    //        stat.SetPetAbility(petTable, bDefaultLevel, customPetLevel);
    //    }

    //    //NetData 어빌리티
    //    if (bIncludeNetData)
    //    {
    //        if (petData != null)
    //        {
    //            foreach (uint abilityActionId in petData.AbilityActionIds)
    //            {
    //                stat.Add(abilityActionId);
    //            }
    //        }
    //    }

    //    //Rune 어빌리티
    //    if (null != petRuneStats)
    //    {
    //        DBRune.SetRuneAbilityValues(petRuneStats, ref stat);
    //    }

    //    return stat;
    //}

    ///// <summary> 해당 Stat 정보에 펫에게 적용된 어빌리티 정보를 추가. <minvalue, maxvalue, AbilityActionId> </summary>
    //public static void SetPetAbilityValues(ref StatForPetUI stat, uint petTid)
    //{
    //    if (0 >= petTid)
    //        return;

    //    var petTable = GetPetData(petTid);
    //    var petData = NetData.Instance.GetPet(NetData.UserID, NetData.CharID, petTid);
    //    var RuneDatas = NetData.Instance.GetEquipRuneList(NetData.UserID, NetData.CharID, petTable.PetID);

    //    SetPetAbilityValues(ref stat, petTable, petData, RuneDatas, true, true, false);
    //}

    ///// <summary> 해당 Stat 정보에 펫에게 적용된 어빌리티 정보를 추가. <minvalue, maxvalue, AbilityActionId> </summary>
    //public static void SetPetAbilityValues(ref StatForPetUI stat, Pet_Table petTable, ZDefine.PetData petData, List<ZDefine.PetRuneData> petRuneStats, bool bIncludeTable = true, bool bIncludeNetData = true, bool bDefaultLevel = false)
    //{
    //    if (null == petTable)
    //        return;
    //    //테이블 어빌리티
    //    if (bIncludeTable)
    //    {
    //        //성장 관련 어빌리티
    //        stat.SetPetAbility(petTable, bDefaultLevel, 0);
    //    }

    //    //NetData 어빌리티
    //    if (bIncludeNetData)
    //    {
    //        if (petData != null)
    //        {
    //            foreach (uint abilityActionId in petData.AbilityActionIds)
    //            {
    //                stat.Add(abilityActionId);
    //            }
    //        }
    //    }

    //    //Rune 어빌리티
    //    if (null != petRuneStats)
    //    {
    //        DBRune.SetRuneAbilityValues(petRuneStats, ref stat);
    //    }
    //}

    public static float CalcedValue(float defaultValue, E_RoundingType roundingType, float perUpValue)
    {
        return roundingType == E_RoundingType.Down ?
            defaultValue += Mathf.Floor(defaultValue * perUpValue) :
            defaultValue += Mathf.Ceil(defaultValue * perUpValue);
    }

    //public static string GetPetTypeIconName(E_PetType_02 type)
    //{
    //    return $"img_type_{type}";
    //}
}
