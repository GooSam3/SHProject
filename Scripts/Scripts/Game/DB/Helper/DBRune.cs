using GameDB;
using System.Collections.Generic;
using ZDefine;
using ZNet.Data;

[UnityEngine.Scripting.Preserve]
public class DBRune : IGameDBHelper
{
    /// <summary> 세트 타입을 Key로 테이블 세팅해 놓는다. </summary>
    static Dictionary<E_RuneSetType, RuneSet_Table> CachedSetTableDict = new Dictionary<E_RuneSetType, RuneSet_Table>();
    /// <summary> Group Id로 해당 RuneEnchant_Table을 셋팅해 놓는다. </summary>
    static Dictionary<uint, List<RuneEnchant_Table>> CachedEnchatGoupDict = new Dictionary<uint, List<RuneEnchant_Table>>();

    public void OnReadyData()
    {
        CachedSetTableDict.Clear();

        foreach (var data in GameDBManager.Container.RuneSet_Table_data)
        {
            if (false == CachedSetTableDict.ContainsKey(data.Value.RuneSetType))
            {
                CachedSetTableDict.Add(data.Value.RuneSetType, data.Value);
            }
        }

        foreach (var data in GameDBManager.Container.RuneEnchant_Table_data)
        {
            if (false == CachedEnchatGoupDict.ContainsKey(data.Value.GroupID))
            {
                CachedEnchatGoupDict.Add(data.Value.GroupID, new List<RuneEnchant_Table>() { data.Value });
            }
            else
            {
                CachedEnchatGoupDict[data.Value.GroupID].Add(data.Value);
            }
        }
    }

    //   public static RuneComponent_Table GetRuneComponet(uint RunecomponentId)
    //   {
    //       if (GameDBManager.Container.RuneComponent_Table_data.TryGetValue(RunecomponentId, out var tableData))
    //           return tableData;

    //       return null;
    //   }

    public static RuneSet_Table GetSetTable(E_RuneSetType type)
    {
        if (type == E_RuneSetType.None)
            return null;

        return CachedSetTableDict[type];
    }

    public static Dictionary<E_RuneSetType, RuneSet_Table>.ValueCollection GetRuneSetList()
    {
        return CachedSetTableDict.Values;
    }

    public static RuneOption_Table GetRuneOptionTable(uint tid)
    {
        if (tid <= 0)
            return null;

        if (false == GameDBManager.Container.RuneOption_Table_data.ContainsKey(tid))
        {
            ZLog.LogError(ZLogChannel.System, "GetRuneOptionTable - Can't Find tid " + tid);
            return null;
        }
        return GameDBManager.Container.RuneOption_Table_data[tid];
    }

    //   public static List<RuneOption_Table> GetRuneOptionTables(uint groupId)
    //   {
    //       List<RuneOption_Table> list = new List<RuneOption_Table>();

    //       if (groupId <= 0)
    //           return list;

    //       foreach (var data in GameDBManager.Container.RuneOption_Table_data)
    //       {
    //           if (groupId != data.Value.GroupID)
    //               continue;

    //           list.Add(data.Value);
    //       }

    //       return list;
    //   }

    public static bool GetRuneEnchantTable(uint tid, out RuneEnchant_Table enchantTable)
    {
        return GameDBManager.Container.RuneEnchant_Table_data.TryGetValue(tid, out enchantTable);
    }

    public static RuneEnchant_Table GetRuneEnchantTable(uint tid)
    {
        if (false == GameDBManager.Container.RuneEnchant_Table_data.ContainsKey(tid))
        {
            ZLog.LogError(ZLogChannel.System, "GetRuneEnchantTable - Can't Find tid " + tid);
            return null;
        }
        return GameDBManager.Container.RuneEnchant_Table_data[tid];
    }

    public static IList<RuneEnchant_Table> GetRuneEnchantGroupTables(uint groupId)
    {
        if (false == CachedEnchatGoupDict.ContainsKey(groupId))
        {
            ZLog.LogError(ZLogChannel.System, "GetRuneEnchantGroupTables - Can't Find groupId " + groupId);
            return null;
        }

        return CachedEnchatGoupDict[groupId].AsReadOnly();
    }

    /// <summary> 다음 단계의 강화 정보를 얻어온다. </summary>
    public static RuneEnchant_Table GetNextRuneEnchantTable(uint tid, uint groupId)
    {
        IList<RuneEnchant_Table> list = GetRuneEnchantGroupTables(groupId);

        if (null == list)
            return null;

        RuneEnchant_Table runeTable = GetRuneEnchantTable(tid);

        if (null == runeTable)
            return null;

        foreach (var data in list)
        {
            if (data.RuneEnchantID == tid)
                continue;

            if (runeTable.EnchantStep + 1 != data.EnchantStep)
                continue;

            return data;
        }

        return null;
    }

    /// <summary> 적용된 룬세트테이블과 몇 번적용되야하는지. <RuneSet_Tabe, count> </summary>
    public static List<KeyValuePair<RuneSet_Table, int>> GetAppliedSetOptionList(uint petTid)
    {
        var runeDataList = Me.CurCharData.GetEquipRuneList(petTid);
        return GetAppliedSetOptionList(runeDataList);
    }

    /// <summary> 적용된 룬세트테이블과 몇 번적용되야하는지. <RuneSet_Tabe, count> </summary>
    public static List<KeyValuePair<RuneSet_Table, int>> GetAppliedSetOptionList(List<ZDefine.PetRuneData> runeDataList)
    {
        Dictionary<E_RuneSetType, int> runeSetTypeCountDict = new Dictionary<E_RuneSetType, int>();

        if (null == runeDataList)
            return null;

        foreach (var runeData in runeDataList)
        {
            var itemTable = DBItem.GetItem(runeData.RuneTid);

            if (null == itemTable)
                continue;

            E_RuneSetType setType = itemTable.RuneSetType;

            if (false == runeSetTypeCountDict.ContainsKey(setType))
            {
                runeSetTypeCountDict.Add(setType, 1);
            }
            else
            {
                ++runeSetTypeCountDict[setType];
            }
        }

        List<KeyValuePair<RuneSet_Table, int>> runeSetTables = new List<KeyValuePair<RuneSet_Table, int>>();

        foreach (var value in runeSetTypeCountDict)
        {
            if (CachedSetTableDict[value.Key].SetCompleteCount > value.Value)
                continue;
            // 적용되야하는 테이블과 몇 번 적용되야하는지에 대한 Count
            runeSetTables.Add(new KeyValuePair<RuneSet_Table, int>(CachedSetTableDict[value.Key], value.Value / CachedSetTableDict[value.Key].SetCompleteCount));
        }

        return runeSetTables;
    }

    //   public static void SetRuneAbilityValues(List<PetRuneData> runeDatas, ref StatForPetUI stat)
    //   {
    //       foreach (var runeData in runeDatas)
    //       {
    //           DBRune.SetRuneAbilityValues(runeData, ref stat);
    //       }

    //       //세트 옵션 
    //       var setTables = DBRune.GetAppliedSetOptionList(runeDatas);

    //       foreach(var table in setTables)
    //       {
    //           for(int i = 0; i < table.Value; ++i)
    //           {
    //               DBAbility.SetAbilityValues(table.Key.AbilityActionID, ref stat);
    //           }
    //       }
    //   }

    // 룬의 메인능력치 가져옴(1개 고정)
    public static UIAbilityData GetMainAbility(RuneEnchant_Table enchantTable)
	{
        List<UIAbilityData> abilityList = new List<UIAbilityData>();

        DBAbilityAction.GetAbilityTypeList(enchantTable.AbilityActionID, ref abilityList);

        if (abilityList.Count > 0)
            return abilityList[0];
        else
            return null;
    }

    // 룬의 접두어 능력치 가져옴(1개 고정)
    public static UIAbilityData GetFirstAbility(PetRuneData runeData)
	{
        List<UIAbilityData> abilityList = new List<UIAbilityData>();

        SetRuneOptingAbilityValue(runeData.FirstOptTid, ref abilityList);

        if (abilityList.Count > 0)
            return abilityList[0];
        else
            return null;
    }

    public static List<UIAbilityData> GetSubAbility(PetRuneData runeData)
	{
        List<UIAbilityData> abilityList = new List<UIAbilityData>();

        DBRune.SetRuneOptingAbilityValues(ref runeData.OptTidList_01, ref abilityList);
        DBRune.SetRuneOptingAbilityValues(ref runeData.OptTidList_02, ref abilityList);
        DBRune.SetRuneOptingAbilityValues(ref runeData.OptTidList_03, ref abilityList);
        DBRune.SetRuneOptingAbilityValues(ref runeData.OptTidList_04, ref abilityList);
        
        return DBAbilityAction.GetMergedTypeList(abilityList);
    }

    public static void GetRuneAbilityValues(PetRuneData runeData, ref List<UIAbilityData> stat)
    {
        var enchantTable = DBRune.GetRuneEnchantTable(runeData.BaseEnchantTid);

        if (null != enchantTable)
        {
            DBAbilityAction.GetAbilityTypeList(enchantTable.AbilityActionID, ref stat);
        }
        SetRuneOptingAbilityValue(runeData.FirstOptTid, ref stat);

        DBRune.SetRuneOptingAbilityValues(ref runeData.OptTidList_01, ref stat);
        DBRune.SetRuneOptingAbilityValues(ref runeData.OptTidList_02, ref stat);
        DBRune.SetRuneOptingAbilityValues(ref runeData.OptTidList_03, ref stat);
        DBRune.SetRuneOptingAbilityValues(ref runeData.OptTidList_04, ref stat);
    }

    public static E_AbilityType ComareAbility(E_RuneAbilityViewType type)
	{
		switch (type)
		{
			case E_RuneAbilityViewType.RUNE_MAX_HP_PLUS:             return E_AbilityType.RUNE_MAX_HP_PLUS;
			case E_RuneAbilityViewType.RUNE_MAX_HP_PER:              return E_AbilityType.RUNE_MAX_HP_PER;
			case E_RuneAbilityViewType.RUNE_SHORT_ATTACK_PLUS:       return E_AbilityType.RUNE_SHORT_ATTACK_PLUS;
			case E_RuneAbilityViewType.RUNE_SHORT_ATTACK_PER:        return E_AbilityType.RUNE_SHORT_ATTACK_PER;
			case E_RuneAbilityViewType.RUNE_LONG_ATTACK_PLUS:        return E_AbilityType.RUNE_LONG_ATTACK_PLUS;
			case E_RuneAbilityViewType.RUNE_LONG_ATTACK_PER:         return E_AbilityType.RUNE_LONG_ATTACK_PER;
			case E_RuneAbilityViewType.RUNE_MAGIC_ATTACK_PLUS:       return E_AbilityType.RUNE_MAGIC_ATTACK_PLUS;
			case E_RuneAbilityViewType.RUNE_MAGIC_ATTACK_PER:        return E_AbilityType.RUNE_MAGIC_ATTACK_PER;
			case E_RuneAbilityViewType.RUNE_MELEE_DEFENCE_PLUS:      return E_AbilityType.RUNE_MELEE_DEFENCE_PLUS;
			case E_RuneAbilityViewType.RUNE_MELEE_DEFENCE_PER:       return E_AbilityType.RUNE_MELEE_DEFENCE_PER;
			case E_RuneAbilityViewType.RUNE_MAGIC_DEFENCE_PLUS:      return E_AbilityType.RUNE_MAGIC_DEFENCE_PLUS;
			case E_RuneAbilityViewType.RUNE_MAGIC_DEFENCE_PER:       return E_AbilityType.RUNE_MAGIC_DEFENCE_PER;
			case E_RuneAbilityViewType.RUNE_MAZ_RATE_DOWN_PER:       return E_AbilityType.RUNE_MAZ_RATE_DOWN_PER;
			case E_RuneAbilityViewType.RUNE_MAZ_RATE_UP_PER:         return E_AbilityType.RUNE_MAZ_RATE_UP_PER;
			case E_RuneAbilityViewType.RUNE_POTION_RECOVERY_PLUS:    return E_AbilityType.RUNE_POTION_RECOVERY_PLUS;
			case E_RuneAbilityViewType.RUNE_ACCURACY_PLUS:           return E_AbilityType.RUNE_ACCURACY_PLUS;
			case E_RuneAbilityViewType.RUNE_EVASION_PLUS:            return E_AbilityType.RUNE_EVASION_PLUS;
			case E_RuneAbilityViewType.RUNE_REDUCTION_PLUS:          return E_AbilityType.RUNE_REDUCTION_PLUS;
			case E_RuneAbilityViewType.RUNE_REDUCTION_IGNORE_PLUS:   return E_AbilityType.RUNE_REDUCTION_IGNORE_PLUS;
		}

		return (E_AbilityType)0;
	}

	//   public static void SetRuneAbilityValues(uint baseEnchantTid, uint firstOptTid, uint subOptionTid_01, uint subOptionTid_02, uint subOptionTid_03, uint subOptionTid_04, ref StatForDefaultUI stat)
	//   {
	//       var enchantTable = DBRune.GetRuneEnchantTable(baseEnchantTid);

	//       if (null != enchantTable)
	//       {
	//           DBAbility.SetAbilityValues(enchantTable.AbilityActionID, ref stat);
	//       }
	//       DBRune.SetRuneOptingAbilityValue(firstOptTid, ref stat);

	//       DBRune.SetRuneOptingAbilityValue(subOptionTid_01, ref stat);
	//       DBRune.SetRuneOptingAbilityValue(subOptionTid_02, ref stat);
	//       DBRune.SetRuneOptingAbilityValue(subOptionTid_03, ref stat);
	//       DBRune.SetRuneOptingAbilityValue(subOptionTid_04, ref stat);
	//   }

	public static void SetRuneOptingAbilityValues(ref List<uint> optionTids, ref List<UIAbilityData> stat)
    {
        foreach (var tid in optionTids)
        {
            SetRuneOptingAbilityValue(tid, ref stat);
        }
    }

    public static void SetRuneOptingAbilityValue(uint optionTid, ref List<UIAbilityData> stat)
    {
        if (optionTid <= 0)
            return;

        RuneOption_Table table = DBRune.GetRuneOptionTable(optionTid);

        if (null == table)
            return;

        DBAbilityAction.GetAbilityTypeList(table.AbilityActionID, ref stat);
    }

	//   public static void SetRuneOptingAbilityValue(uint optionTid, ref StatForDefaultUI stat)
	//   {
	//       if (optionTid <= 0)
	//           return;

	//       RuneOption_Table table = DBRune.GetRuneOptionTable(optionTid);

	//       if (null == table)
	//           return;

	//       DBAbility.SetAbilityValues(table.AbilityActionID, ref stat);
	//   }

	//   public static Color GetRuneGradeColor(GameDB.E_RuneGradeType type)
	//   {
	//       switch (type)
	//       {
	//           case GameDB.E_RuneGradeType.Normal:
	//               return ZGameSettings.Palette.Rune_Normal;
	//           case GameDB.E_RuneGradeType.HighClass:
	//               return ZGameSettings.Palette.Rune_HighClass;
	//           case GameDB.E_RuneGradeType.Rare:
	//               return ZGameSettings.Palette.Rune_Rare;
	//           case GameDB.E_RuneGradeType.Legend:
	//               return ZGameSettings.Palette.Rune_Legend;
	//           case GameDB.E_RuneGradeType.Myth:
	//               return ZGameSettings.Palette.Rune_Myth;
	//       }

	//       return Color.white;
	//   }

	//   public static Color GetRuneGradeBGColor(GameDB.E_RuneGradeType type)
	//   {
	//       switch (type)
	//       {
	//           case GameDB.E_RuneGradeType.Normal:
	//               return ZGameSettings.Palette.Rune_Normal_BG;
	//           case GameDB.E_RuneGradeType.HighClass:
	//               return ZGameSettings.Palette.Rune_HighClass_BG;
	//           case GameDB.E_RuneGradeType.Rare:
	//               return ZGameSettings.Palette.Rune_Rare_BG;
	//           case GameDB.E_RuneGradeType.Legend:
	//               return ZGameSettings.Palette.Rune_Legend_BG;
	//           case GameDB.E_RuneGradeType.Myth:
	//               return ZGameSettings.Palette.Rune_Myth_BG;
	//       }

	//       return Color.white;
	//   }

	//   public static Color GetRuneGradeTextColor(GameDB.E_RuneGradeType type)
	//   {
	//       switch (type)
	//       {
	//           case GameDB.E_RuneGradeType.Normal:
	//               return ZGameSettings.Palette.Rune_Normal_Text;
	//           case GameDB.E_RuneGradeType.HighClass:
	//               return ZGameSettings.Palette.Rune_HighClass_Text;
	//           case GameDB.E_RuneGradeType.Rare:
	//               return ZGameSettings.Palette.Rune_Rare_Text;
	//           case GameDB.E_RuneGradeType.Legend:
	//               return ZGameSettings.Palette.Rune_Legend_Text;
	//           case GameDB.E_RuneGradeType.Myth:
	//               return ZGameSettings.Palette.Rune_Myth_Text;
	//       }

	//       return Color.white;
	//   }

	//   public static string GetRuneGradeTextColorToHex(GameDB.E_RuneGradeType type)
	//   {
	//       Color color = GetRuneGradeTextColor(type);
	//       return ColorUtility.ToHtmlStringRGB(color);
	//   }

	//   public static string GetRuneBackgroundImageName(E_EquipSlotType type)
	//   {
	//       switch(type)
	//       {
	//           case E_EquipSlotType.Rune_01:
	//               return "Consume_Rune2_001";
	//           case E_EquipSlotType.Rune_02:
	//               return "Consume_Rune2_002";
	//           case E_EquipSlotType.Rune_03:
	//               return "Consume_Rune2_003";
	//           case E_EquipSlotType.Rune_04:
	//               return "Consume_Rune2_004";
	//           case E_EquipSlotType.Rune_05:
	//               return "Consume_Rune2_005";
	//           case E_EquipSlotType.Rune_06:
	//               return "Consume_Rune2_006";
	//       }
	//       return "";
	//   }

	//   static readonly Vector2[] RuneSetMarkPosition = {
	//       new Vector2(0, 6.7f),
	//       new Vector2(7, 4.7f),
	//       new Vector2(7, -4.3f),
	//       new Vector2(0, -8.3f),
	//       new Vector2(-6, -4.3f),
	//       new Vector2(-6, 4.7f),
	//   };

	//   /// <summary> Rune setMark 포지션을 얻어온다. </summary>
	//   public static Vector2 GetRuneSetMarkPosition(E_EquipSlotType slotType)
	//   {
	//       int index = slotType - E_EquipSlotType.Rune_01;

	//       return RuneSetMarkPosition[index];
	//   }

	//   /// <summary> 각 룬 슬롯에 맞는 Quaternio을 얻어온다. ui용 </summary>
	//   public static Quaternion GetRuneRotate(E_EquipSlotType slotType)
	//   {
	//       int index = slotType - E_EquipSlotType.Rune_01;
	//       float rotateZ = (index) * -60f;

	//       return Quaternion.Euler(0f, 0f, rotateZ);
	//   }

	//   /// <summary> Rune Grade Tile Image의 size를 얻어온다. </summary>
	//   public static float GetRuneGradeSize(byte grade, float baseSize)
	//   {
	//       return baseSize * (int)grade;
	//   }

	//   /// <summary> Rune 세트 아이콘 네임을 얻어온다. </summary>
	//   public static string GetRuneSetIconName(E_RuneSetType setType)
	//   {
	//       if (setType == E_RuneSetType.None)
	//           return "";

	//       if(DBItem.GetItem(CachedSetTableDict[setType].RuneItemId, out var tableData))
	//       {
	//           return tableData.IconID;
	//       }
	//       return "";        
	//   }

	//   public static string GetRuneGradeName(E_RuneGradeType type)
	//   {
	//       if (type == E_RuneGradeType.None)
	//           return "-";
	//       return DBLocale.GetLocaleText($"RuneGradeType_{type}");
	//   }

	//   public static E_RuneGradeType GetRuneGrade(uint runeTid, ulong runeId = 0)
	//   {
	//       Item_Table table = DBItem.GetItem(runeTid);

	//       if(0 < runeId)
	//       {
	//           var runeData = NetData.Instance.GetRune(NetData.UserID, NetData.CharID, runeId);
	//           var encahntTable = GetRuneEnchantTable(runeData.BaseEnchantTid);

	//           int type = ((encahntTable.EnchantStep / 3) + 1);

	//           type = Mathf.Max((int)table.RuneGradeType, type);

	//           type = Mathf.Min((int)E_RuneGradeType.Myth, type);

	//           return (E_RuneGradeType)type;
	//       }

	//       return table.RuneGradeType;
	//   }
}
