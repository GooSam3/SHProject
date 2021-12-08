using GameDB;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// </summary>
[UnityEngine.Scripting.Preserve]
public class DBChange : IGameDBHelper
{
    static Dictionary<byte, List<Change_Table>> changeDicByGrade = new Dictionary<byte, List<Change_Table>>();

    static Dictionary<uint, List<ChangeList_Table>> changeListGroupDic = new Dictionary<uint, List<ChangeList_Table>>();

    public void OnReadyData()
    {
        changeDicByGrade.Clear();

        foreach (Change_Table tableData in GameDBManager.Container.Change_Table_data.Values)
        {
            if (!changeDicByGrade.ContainsKey(tableData.Grade))
                changeDicByGrade.Add(tableData.Grade, new List<Change_Table>());

            changeDicByGrade[tableData.Grade].Add(tableData);
        }

        changeListGroupDic.Clear();

        foreach (var tableData in GameDBManager.Container.ChangeList_Table_data.Values)
        {
            if (!changeListGroupDic.ContainsKey(tableData.GroupID))
                changeListGroupDic.Add(tableData.GroupID,new List<ChangeList_Table>());

            changeListGroupDic[tableData.GroupID].Add(tableData);
        }
    }

	public static bool TryGet(uint tid, out Change_Table table)
	{
		return GameDBManager.Container.Change_Table_data.TryGetValue(tid, out table);
	}

	public static Change_Table Get(uint tid)
	{
        if (GameDBManager.Container.Change_Table_data.ContainsKey(tid))
            return GameDBManager.Container.Change_Table_data[tid];

        //ZLog.Log("Get - Can't Find Change Data : "+tid);

        return null;
    }

    public static int GetAllChangeCount()
    {
        return GameDBManager.Container.Change_Table_data.Count;
    }

    public static E_CharacterType GetCharacterType(uint tid)
    {
        if (GameDBManager.Container.Change_Table_data.ContainsKey(tid))
            return GameDBManager.Container.Change_Table_data[tid].UseAttackType;

        //ZLog.Log("GetAttackType - Can't Find Change Data : " + tid);

        return default;
    }
    /*
    public static uint GetResourceID(uint changeId, E_CharacterType charType)
	{
		if (!Get(changeId, out var changeTable))
			return 0;

		uint resourceId = 0;
		switch (charType)
		{
			case E_CharacterType.Archer:
				resourceId = changeTable.Archer_ResID;
				break;
			case E_CharacterType.Assassin:
				resourceId = changeTable.Assassin_ResID;
				break;
			case E_CharacterType.Knight:
				resourceId = changeTable.Knight_ResID;
				break;
			case E_CharacterType.Wizard:
				resourceId = changeTable.Wizard_ResID;
				break;
			default:
				resourceId = changeTable.Knight_ResID;
				break;
		}

		return resourceId;
	}
    */
    public static byte GetChangeGrade(uint tid)
    {
        if (GameDBManager.Container.Change_Table_data.ContainsKey(tid))
            return GameDBManager.Container.Change_Table_data[tid].Grade;

        //ZLog.Log("GetChangeGrade - Can't Find Change Data : " + tid);

        return 0;
    }

    public static string GetChangeIcon(uint tid)
    {
        if (GameDBManager.Container.Change_Table_data.ContainsKey(tid))
            return GameDBManager.Container.Change_Table_data[tid].Icon;

        //ZLog.Log("GetChangeIcon - Can't Find Change Data : " + tid);

        return "";
    }

    public static string GetClassIcon(uint tid)
    {
        if (GameDBManager.Container.Change_Table_data.ContainsKey(tid))
            return GameDBManager.Container.Change_Table_data[tid].ClassIcon;

        //ZLog.Log("GetClassIcon - Can't Find Change Data : " + tid);

        return "";
    }

    public static string GetChangeFullName(uint tid, int enchant = 0)
    {
        if(GameDBManager.Container.Change_Table_data.TryGetValue(tid, out var table))
        {
            return DBUIResouce.GetChangeGradeFormat(DBLocale.GetText(table.ChangeTextID), table.Grade);
        }
        //ZLog.LogError("GetChangeName - ChangeID Find Fail : " + tid);
        return "";
    }

    public static Dictionary<uint, Change_Table>.ValueCollection GetAllChangeDatas()
    {
        return GameDBManager.Container.Change_Table_data.Values;
    }

    public static Dictionary<byte, List<Change_Table>>.KeyCollection GetChangeGrades()
    {
        return changeDicByGrade.Keys;
    }

    static List<Change_Table> totalchangeList = new List<Change_Table>();
    public static IList<Change_Table> GetChangeDatas(byte Grade)//0: all
    {
        if (Grade == 0)
        {
            if (totalchangeList.Count <= 0)
                totalchangeList.AddRange(GetAllChangeDatas());
            return totalchangeList.AsReadOnly();
        }

        if (changeDicByGrade.ContainsKey(Grade))
            return changeDicByGrade[Grade].AsReadOnly();

        return null;
    }

    public static Change_Table GetRandomChange(byte Grade)
    {
        if (changeDicByGrade.ContainsKey(Grade))
        {
            int countdown = 100;
            while (countdown > 0)
            {
                int index = Random.Range(0, changeDicByGrade[Grade].Count - 1);
                Change_Table table = changeDicByGrade[Grade][index];

                --countdown;

                if (table.ViewType == E_ViewType.NotView)
                    continue;

                return table;
            }
        }

        return null;
    }

    public static List<ChangeList_Table> GetChangeList(uint GroupID)
    {
        if (changeListGroupDic.ContainsKey(GroupID))
            return changeListGroupDic[GroupID];

        //ZLog.LogError("GetChangeList - can't Find ChangeGroup : " + GroupID);
        return null;
    }

    /// <summary> 강림에게 적용된 어빌리티 정보를 얻어온다. <minvalue, maxvalue, AbilityActionId> </summary>
    //public static StatForUI GetChangeAbilityValues(uint changeTid, bool bIncludeTable = true, bool bIncludeNetData = true)
    //{
    //    return GetChangeAbilityValues(Get(changeTid), NetData.Instance.GetChange(NetData.UserID, NetData.CharID, changeTid), bIncludeTable, bIncludeNetData);
    //}

    ///// <summary> 강림에게 적용된 어빌리티 정보를 얻어온다. <minvalue, maxvalue, AbilityActionId> </summary>
    //public static StatForDefaultUI GetChangeAbilityValues(Change_Table changeTable, ZDefine.ChangeData changeData, bool bIncludeTable = true, bool bIncludeNetData = true)
    //{
    //    StatForDefaultUI stats = new StatForDefaultUI();

    //    //테이블 어빌리티
    //    if(bIncludeTable)
    //    {
    //        DBAbility.SetAbilityValues(changeTable.AbilityActionID_01, ref stats);
    //        DBAbility.SetAbilityValues(changeTable.AbilityActionID_02, ref stats);
    //    }

    //    //NetData 어빌리티
    //    if (bIncludeNetData)
    //    {            
    //        if (changeData != null)
    //        {
    //            foreach (uint abilId in changeData.AbilityActionIds)
    //            {
    //                DBAbility.SetAbilityValues(abilId, ref stats);
    //            }
    //        }
    //    }

    //    return stats;
    //}
}
