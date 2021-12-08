using GameDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZDefine;
using ZNet.Data;

/// <summary>
/// UIAbilityListAdapter 와 쌍으로 사용
/// </summary>
[UnityEngine.Scripting.Preserve]
public class UIStatHelper
{
    public static List<UIAbilityData> GetChangeStat(uint tid)
    {
        if (DBChange.TryGet(tid, out var table) == false)
            return null;
        return GetChangeStat(table);
    }

    public static List<UIAbilityData> GetChangeStat(Change_Table table)
    {
        List<UIAbilityData> listsAbility = new List<UIAbilityData>();

        foreach (var ability in table.AbilityActionIDs)
        {
            if (DBAbilityAction.TryGet(ability, out var abilityTable) == false)
                continue;

            DBAbilityAction.GetAbilityTypeList(abilityTable, ref listsAbility);
        }

        // 스킬관련 능력치
        uint skillTarget = 0;

        for (int i = 0; i < listsAbility.Count; i++)
        {
            var data = listsAbility[i];

            if (data.skillTarget != skillTarget)
            {
                var temp = new UIAbilityData(data);

                skillTarget = temp.skillTarget;

                temp.useBySkillName = true;

                listsAbility.Insert(i, temp);
                i++;
                continue;
            }
            data.useBySkillName = false;
        }
        return listsAbility;
    }


    // 아직 장비추가안됨
    public static List<UIAbilityData> GetPetStat(uint tid, List<PetRuneData> switchRuneData = null, bool IncludeExtraStat = true)
    {
        if (DBPet.TryGet(tid, out var table) == false)
            return null;

        return GetPetStat(table, switchRuneData, IncludeExtraStat);
    }

    /// <summary>
    /// 해당 펫 능력치 가져옴
    /// 주의 : 탈것관련 이동속도는 따로처리해야됨
    /// </summary>
    /// <param name="data">테이블</param>
    /// <param name="switchRuneData">적용할 룬 리스트 없다면 현재 착용중인걸로</param>
    /// <param name="IncludeExtraStat">성장, 장비 등 추가능력치 포함?</param>
    /// <returns>최종 스텟</returns>
    public static List<UIAbilityData> GetPetStat(Pet_Table data, List<PetRuneData> switchRuneData = null, bool IncludeExtraStat = true)
    {
        List<UIAbilityData> listAbility = new List<UIAbilityData>();

        PetData myData = null;

        if (data.PetType == E_PetType.Pet)
            myData = Me.CurCharData.GetPetData(data.PetID);
        else if (data.PetType == E_PetType.Vehicle)
            myData = Me.CurCharData.GetRideData(data.PetID);

        // 기본 펫 능력치
        if (DBAbilityAction.TryGet(data.GrowthAbility, out AbilityAction_Table growthTable))
        {
            DBAbilityAction.GetAbilityTypeList(growthTable, ref listAbility);
        }

        // 해당펫을 소유하지 않을시 성장 및 장비능력치는 없음
        if (myData == null || IncludeExtraStat == false)
            return listAbility;

        // 성장 능력치
        var petLevel = DBPetLevel.GetLevel(data.PetExpGroup, myData.Exp);
        var levelTable = DBPetLevel.Get(data.PetExpGroup, petLevel);

        foreach (var iter in listAbility)
        {
            float value = DBPet.CalcedValue(iter.value, levelTable.RoundingType, levelTable.AbilityUpPer);
            iter.value = value;
        }

        // 착용 장비 능력치
        var equipList = switchRuneData ?? Me.CurCharData.GetEquipRuneList(data.PetID);

        //PER 능력치 후적용 때문에 따로저장 {targettype : PER value}
        var dicEquipAbilityPER = new Dictionary<E_AbilityType, float>();

        foreach (var iter in equipList)
        {
            var equipData = GetPetEquipStat(iter, false);
            foreach (var ability in equipData)
            {
                if (ability.type.ToString().Contains("PER"))
                {
                    SetPERAbilityTarget(ability, ref dicEquipAbilityPER);
                }
                else
                {
                    if (ability.type == E_AbilityType.RUNE_ATTACK_PLUS)
                    {
                        listAbility.Add(new UIAbilityData(E_AbilityType.RUNE_LONG_ATTACK_PLUS, ability.value));
                        listAbility.Add(new UIAbilityData(E_AbilityType.RUNE_SHORT_ATTACK_PLUS, ability.value));
                    }
                    else if (ability.type == E_AbilityType.RUNE_DEFENCE_PLUS)
                    {
                        listAbility.Add(new UIAbilityData(E_AbilityType.RUNE_MAGIC_DEFENCE_PLUS, ability.value));
                        listAbility.Add(new UIAbilityData(E_AbilityType.RUNE_MELEE_DEFENCE_PLUS, ability.value));
                    }
                    else
                        listAbility.Add(ability);
                }
            }
        }

        // 착용장비 세트 능력치
        var setList = GetPetEquipSetStat(switchRuneData ?? Me.CurCharData.GetEquipRuneList(data.PetID));

        if (setList != null)
        {
            foreach (var iter in setList)
            {
                if (iter.type.ToString().Contains("_PER"))
                {
                    SetPERAbilityTarget(iter, ref dicEquipAbilityPER);
                }
                else
                {
                    if (iter.type == E_AbilityType.RUNE_ATTACK_PLUS)
                    {
                        listAbility.Add(new UIAbilityData(E_AbilityType.RUNE_LONG_ATTACK_PLUS, iter.value));
                        listAbility.Add(new UIAbilityData(E_AbilityType.RUNE_SHORT_ATTACK_PLUS, iter.value));
                    }
                    else if (iter.type == E_AbilityType.RUNE_DEFENCE_PLUS)
                    {
                        listAbility.Add(new UIAbilityData(E_AbilityType.RUNE_MAGIC_DEFENCE_PLUS, iter.value));
                        listAbility.Add(new UIAbilityData(E_AbilityType.RUNE_MELEE_DEFENCE_PLUS, iter.value));
                    }
                    else
                        listAbility.Add(iter);
                }

            }
        }

        // PER적용

        var mergedAbility = DBAbilityAction.GetMergedTypeList(listAbility);

        foreach (var iter in dicEquipAbilityPER)
        {
            var ability = mergedAbility.Find(item => item.type == iter.Key);

            if (ability == null)
            {// 스텟 새로 추가되는놈
                mergedAbility.Add(new UIAbilityData(iter.Key,iter.Value));
            }
            else
            {
                if (IsPERAbilityApplyable(ability))
                {// per -> plus

                    ability.value = UnityEngine.Mathf.FloorToInt(ability.value + ability.value * (iter.Value * .01f));
                }
                else
                {// per ++
                    ability.value += iter.Value;
                }
            }
        }

        return mergedAbility;
    }

    public static void SetPERAbilityTarget(UIAbilityData ability, ref Dictionary<E_AbilityType, float> dic)
    {
        switch (ability.type)
        {
            case E_AbilityType.RUNE_MAX_HP_PER:             dic.TrySumValue(E_AbilityType.RUNE_MAX_HP_PLUS, ability.value); return;
            case E_AbilityType.RUNE_ATTACK_PER:             dic.TrySumValue(E_AbilityType.RUNE_LONG_ATTACK_PLUS, ability.value);
                                                            dic.TrySumValue(E_AbilityType.RUNE_SHORT_ATTACK_PLUS, ability.value); return;
            case E_AbilityType.RUNE_SHORT_ATTACK_PER:       dic.TrySumValue(E_AbilityType.RUNE_SHORT_ATTACK_PLUS, ability.value); return;
            case E_AbilityType.RUNE_LONG_ATTACK_PER:        dic.TrySumValue(E_AbilityType.RUNE_LONG_ATTACK_PLUS, ability.value); return;
            case E_AbilityType.RUNE_MAGIC_ATTACK_PER:       dic.TrySumValue(E_AbilityType.RUNE_MAGIC_ATTACK_PLUS, ability.value); return;
            case E_AbilityType.RUNE_ACCURACY_PER:           dic.TrySumValue(E_AbilityType.RUNE_ACCURACY_PLUS, ability.value); return;
            case E_AbilityType.RUNE_DEFENCE_PER:            dic.TrySumValue(E_AbilityType.RUNE_MAGIC_DEFENCE_PLUS, ability.value);
                                                            dic.TrySumValue(E_AbilityType.RUNE_MELEE_DEFENCE_PLUS, ability.value); return;
            case E_AbilityType.RUNE_MELEE_DEFENCE_PER:      dic.TrySumValue(E_AbilityType.RUNE_MELEE_DEFENCE_PLUS, ability.value); return;
            case E_AbilityType.RUNE_MAGIC_DEFENCE_PER:      dic.TrySumValue(E_AbilityType.RUNE_MAGIC_DEFENCE_PLUS, ability.value); return;
            case E_AbilityType.RUNE_REDUCTION_PER:          dic.TrySumValue(E_AbilityType.RUNE_REDUCTION_PLUS, ability.value); return;
            case E_AbilityType.RUNE_REDUCTION_IGNORE_PER:   dic.TrySumValue(E_AbilityType.RUNE_REDUCTION_IGNORE_PLUS, ability.value); return;
            case E_AbilityType.RUNE_EVASION_PER:            dic.TrySumValue(E_AbilityType.RUNE_EVASION_PLUS, ability.value); return;
        }

        // +능력치가 없는, %만적용되는 아이들은 그냥 더해버림(ex RUNE_MAZ_RATE_DOWN_PER)
        dic.TrySumValue(ability.type, ability.value);
    }

    // per 적용 가능한지
    // per능력치만 적용되는 애들 있음
    // (ex RUNE_MAZ_RATE_DOWN_PER)
    public static bool IsPERAbilityApplyable(UIAbilityData ability)
    {
        switch (ability.type)
        {
            case E_AbilityType.RUNE_MAX_HP_PLUS:
            case E_AbilityType.RUNE_LONG_ATTACK_PLUS:
            case E_AbilityType.RUNE_SHORT_ATTACK_PLUS:
            case E_AbilityType.RUNE_MAGIC_ATTACK_PLUS:
            case E_AbilityType.RUNE_ACCURACY_PLUS:
            case E_AbilityType.RUNE_MELEE_DEFENCE_PLUS:
            case E_AbilityType.RUNE_MAGIC_DEFENCE_PLUS:
            case E_AbilityType.RUNE_REDUCTION_PLUS:
            case E_AbilityType.RUNE_REDUCTION_IGNORE_PLUS:
            case E_AbilityType.RUNE_EVASION_PLUS:
                return true;
        }
        return false;
    }

    /// <summary>
    /// 펫 장비들의 세트스텟을 가져옴
    /// </summary>
    /// <param name=""> 장비 리스트</param>
    /// <returns>스텟</returns>
    public static List<UIAbilityData> GetPetEquipSetStat(List<PetRuneData> listEquip)
    {
        var listSetInfo = new List<UIAbilityData>();

        var listSetTable = DBRune.GetAppliedSetOptionList(listEquip);

        if ((listSetTable?.Count ?? 0) <= 0)
            return null;

        List<RuneSet_Table> listTable = new List<RuneSet_Table>();

        foreach (var iter in listSetTable)
        {
            for (int i = 0; i < iter.Value; i++)
                DBAbilityAction.GetAbilityTypeList(iter.Key.AbilityActionID, ref listSetInfo);
        }

        return listSetInfo;
    }

    /// <summary>
    /// 펫 장비의 능력치를 가져옴
    /// </summary>
    /// <param name="runeData">장비</param>
    /// <param name="addSetInfo">세트정보ui출력여부</param>
    /// <returns></returns>
    public static List<UIAbilityData> GetPetEquipStat(PetRuneData runeData, bool addSetInfo = true)
    {
        List<UIAbilityData> listAbility = new List<UIAbilityData>();

        DBRune.GetRuneAbilityValues(runeData, ref listAbility);

        if (addSetInfo)
        {
            listAbility.Add(new UIAbilityData(E_UIAbilityViewType.Blank));

            string[] setInfo = UICommon.GetRuneSetAbilityTextArray(DBItem.GetItem(runeData.RuneTid).RuneSetType);

            foreach (var iter in setInfo)
                listAbility.Add(new UIAbilityData(E_UIAbilityViewType.Text) { textLeft = UICommon.GetColoredText("#ffdc64", iter) });
        }

        return listAbility;
    }

    // 기존 능력치 리스트를 비교리스트로 변환 (레벨)
    // 비교대상으로 데이터 변경 후 기존레벨과 비교
    public static void SetPetCompareStat(ref List<UIAbilityData> listAbility, uint expGroup, byte level)
    {
        var compareLevelTable = DBPetLevel.Get(expGroup, level);

        foreach (var iter in listAbility)
        {
            float compareValue = DBPet.CalcedValue(iter.value, compareLevelTable.RoundingType, compareLevelTable.AbilityUpPer);
            // 비교대상은 기존에 있던 값
            iter.compareValue = iter.value;

            // 현재 보이는 값은 비교할 값으로 넣어줌
            iter.value = compareValue;
            iter.viewType = E_UIAbilityViewType.AbilityCompare;
        }
    }

    /// <summary>
    /// 비교 스텟이 동일한 스텟군이라 가정
    /// 비교스텟리스트를 만들어줌
    /// ex) origin : 공격력 2 , compare : 공격력 4
    /// => 공격력 4(+2);
    /// </summary>
    /// <param name="origin">원본</param>
    /// <param name="compareTarget">대상</param>
    public static void SetCompareStat(ref List<UIAbilityData> origin, List<UIAbilityData> compareTarget)
    {
        var dicCompare = new Dictionary<E_AbilityType, UIAbilityData>();
        // 비교용 딕셔너리 생성
        foreach (var iter in compareTarget)
        {
            if (dicCompare.ContainsKey(iter.type) == false)
                dicCompare.Add(iter.type, iter);
        }

        foreach (var iter in origin)
        {
            float compareValue = iter.value;
            float value = iter.value;
            iter.viewType = E_UIAbilityViewType.AbilityCompare;

            if (dicCompare.ContainsKey(iter.type))
            {
                compareValue = dicCompare[iter.type].value;
                dicCompare.Remove(iter.type);
            }

            iter.value = compareValue;
            iter.compareValue = value;
        }

        // 남은놈 처리
        foreach (var iter in dicCompare)
        {
            origin.Add(new UIAbilityData(E_UIAbilityViewType.AbilityCompare) { type = iter.Key, value = iter.Value.value, compareValue = 0 });
        }
    }

    public static void SetPetEquipCompareEnhanceStat(ref UIAbilityData mainAbility, ref UIAbilityData firstAbility, ref List<UIAbilityData> listSubAbility, RuneEnchant_Table nextEnchantTable)
    {
        if(mainAbility!=null)
		{
            mainAbility.viewType = E_UIAbilityViewType.AbilityCompare;
            mainAbility.compareValue = mainAbility.value;

            var compValue = DBRune.GetMainAbility(nextEnchantTable);

            if (compValue != null)
                mainAbility.value = compValue.value;
            
		}

        if(firstAbility!=null)
		{
            firstAbility.viewType = E_UIAbilityViewType.AbilityCompare;
            firstAbility.compareValue = firstAbility.value;
		}


        foreach (var iter in listSubAbility)
        {
            iter.compareValue = iter.value;
            iter.viewType = E_UIAbilityViewType.AbilityCompare;
        }
    }
}
