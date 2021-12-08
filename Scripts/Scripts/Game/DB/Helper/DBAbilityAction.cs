using GameDB;
using System.Collections.Generic;

[UnityEngine.Scripting.Preserve]
public class DBAbilityAction : IGameDBHelper
{
	public void OnReadyData()
	{
	}

	public static Dictionary<uint, AbilityAction_Table> DicAbilityAction
	{
		get { return GameDBManager.Container.AbilityAction_Table_data; }
	}

	public static bool TryGet(uint _tid, out AbilityAction_Table outTable)
	{
		return GameDBManager.Container.AbilityAction_Table_data.TryGetValue(_tid, out outTable);
	}

	public static AbilityAction_Table Get(uint _tid)
	{
		if (GameDBManager.Container.AbilityAction_Table_data.TryGetValue(_tid, out var foundTable))
		{
			return foundTable;
		}
		return null;
	}

    public static void GetAbilityTypeList(uint _tid, ref List<E_AbilityType> list)
    {
        if (false == GameDBManager.Container.AbilityAction_Table_data.TryGetValue(_tid, out var foundTable))
        {
            return;
        }

        GetAbilityTypeList(foundTable, ref list);
    }

    public static void GetAbilityTypeList(uint _tid, ref List<UIAbilityData> list)
    {
        if (false == GameDBManager.Container.AbilityAction_Table_data.TryGetValue(_tid, out var foundTable))
        {
            return;
        }

        GetAbilityTypeList(foundTable, ref list);
    }
    public static void GetAbilityTypeList(AbilityAction_Table table, ref List<E_AbilityType> list)
    {
        list.Clear();
        if (null != table)
        {
            if (0 < table.AbilityID_01) list.Add(table.AbilityID_01);
            if (0 < table.AbilityID_02) list.Add(table.AbilityID_02);
            if (0 < table.AbilityID_03) list.Add(table.AbilityID_03);
            if (0 < table.AbilityID_04) list.Add(table.AbilityID_04);
            if (0 < table.AbilityID_05) list.Add(table.AbilityID_05);
            if (0 < table.AbilityID_06) list.Add(table.AbilityID_06);
            if (0 < table.AbilityID_07) list.Add(table.AbilityID_07);
            if (0 < table.AbilityID_08) list.Add(table.AbilityID_08);
            if (0 < table.AbilityID_09) list.Add(table.AbilityID_09);

        }
    }

    public static void GetAbilityTypeList(AbilityAction_Table table, ref List<UIAbilityData> list)
    {
        if (0 < table.AbilityID_01) list.Add(new UIAbilityData() { type = table.AbilityID_01, value = table.AbilityPoint_01_Min, skillTarget = table.TargetSkillID }) ;
        if (0 < table.AbilityID_02) list.Add(new UIAbilityData() { type = table.AbilityID_02, value = table.AbilityPoint_02, skillTarget = table.TargetSkillID }) ;
        if (0 < table.AbilityID_03) list.Add(new UIAbilityData() { type = table.AbilityID_03, value = table.AbilityPoint_03, skillTarget = table.TargetSkillID }) ;
        if (0 < table.AbilityID_04) list.Add(new UIAbilityData() { type = table.AbilityID_04, value = table.AbilityPoint_04, skillTarget = table.TargetSkillID }) ;
        if (0 < table.AbilityID_05) list.Add(new UIAbilityData() { type = table.AbilityID_05, value = table.AbilityPoint_05, skillTarget = table.TargetSkillID }) ;
        if (0 < table.AbilityID_06) list.Add(new UIAbilityData() { type = table.AbilityID_06, value = table.AbilityPoint_06, skillTarget = table.TargetSkillID }) ;
        if (0 < table.AbilityID_07) list.Add(new UIAbilityData() { type = table.AbilityID_07, value = table.AbilityPoint_07, skillTarget = table.TargetSkillID }) ;
        if (0 < table.AbilityID_08) list.Add(new UIAbilityData() { type = table.AbilityID_08, value = table.AbilityPoint_08, skillTarget = table.TargetSkillID }) ;
        if (0 < table.AbilityID_09) list.Add(new UIAbilityData() { type = table.AbilityID_09, value = table.AbilityPoint_09, skillTarget = table.TargetSkillID });
    }

    public static void MergeAbilityTypeList(ref List<UIAbilityData> data, uint abilityActionTid)
    {

    }

    // 공통된 값 합한다.
    // 주의 : 정렬, 스킬 없음, 일단 컬렉션만
    public static List<UIAbilityData> GetMergedTypeList(List<UIAbilityData> list)
    {
        // 타입 : 값
        Dictionary<E_AbilityType, float> dicMergeAbility = new Dictionary<E_AbilityType, float>();

        List<UIAbilityData> listPair = new List<UIAbilityData>();

        foreach (var iter in list)
        {
            if (dicMergeAbility.ContainsKey(iter.type) == false)
                dicMergeAbility.Add(iter.type, 0f);

            dicMergeAbility[iter.type] += iter.value;   
        }

        foreach(var iter in dicMergeAbility)
        {
            listPair.Add(new UIAbilityData() { type = iter.Key, value = iter.Value });
        }
        return listPair;
    }
}
