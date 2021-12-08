using GameDB;
using System.Collections.Generic;

[UnityEngine.Scripting.Preserve]
public class DBAbility : IGameDBHelper
{
	static public Ability_Table TABLE_State_Not_Move;
	static public Ability_Table TABLE_State_Not_Skill;
	static public Ability_Table TABLE_State_Not_Potion;
	static public Ability_Table TABLE_State_Not_Return;
	static public Ability_Table TABLE_State_Stun;
	static public Ability_Table TABLE_State_Fear;
	static public Ability_Table TABLE_State_Vision;
	static public Ability_Table TABLE_State_Special_Vision;

	/// <summary>
	///  <see cref="E_AbilityType"/>에서 STATE_ 관련된 타입들에 들어있는 상태이상값 정보.
	/// </summary>
	static public Devcat.EnumDictionary<E_AbilityType, E_ConditionControl> CC_StateTables = new Devcat.EnumDictionary<E_AbilityType, E_ConditionControl>();

	public void OnReadyData()
	{
		GetAbility(E_AbilityType.STATE_NOT_MOVE, out TABLE_State_Not_Move);
		GetAbility(E_AbilityType.STATE_NOT_SKILL, out TABLE_State_Not_Skill);
		GetAbility(E_AbilityType.STATE_NOT_POTION, out TABLE_State_Not_Potion);
		GetAbility(E_AbilityType.STATE_NOT_RETURN, out TABLE_State_Not_Return);
		GetAbility(E_AbilityType.STATE_STUN, out TABLE_State_Stun);
		GetAbility(E_AbilityType.STATE_FEAR, out TABLE_State_Fear);
		GetAbility(E_AbilityType.STATE_VISION, out TABLE_State_Vision);
		GetAbility(E_AbilityType.STATE_SPECIAL_VISION, out TABLE_State_Special_Vision);

        CC_StateTables.Clear();

        CC_StateTables.Add(E_AbilityType.STATE_NOT_MOVE, TABLE_State_Not_Move.ConditionControl);
		CC_StateTables.Add(E_AbilityType.STATE_NOT_SKILL, TABLE_State_Not_Skill.ConditionControl);
		CC_StateTables.Add(E_AbilityType.STATE_NOT_POTION, TABLE_State_Not_Potion.ConditionControl);
		CC_StateTables.Add(E_AbilityType.STATE_NOT_RETURN, TABLE_State_Not_Return.ConditionControl);
		CC_StateTables.Add(E_AbilityType.STATE_STUN, TABLE_State_Stun.ConditionControl);
		CC_StateTables.Add(E_AbilityType.STATE_FEAR, TABLE_State_Fear.ConditionControl);
		CC_StateTables.Add(E_AbilityType.STATE_VISION, TABLE_State_Vision.ConditionControl);
		CC_StateTables.Add(E_AbilityType.STATE_SPECIAL_VISION, TABLE_State_Special_Vision.ConditionControl);
	}

    #region AbilityAction
    public static bool GetAction(uint _tid, out AbilityAction_Table _table)
	{
		return GameDBManager.Container.AbilityAction_Table_data.TryGetValue(_tid, out _table);
	}

    public static string GetActionName(uint _tid)
    {
        if (GameDBManager.Container.AbilityAction_Table_data.ContainsKey(_tid))
            return GameDBManager.Container.AbilityAction_Table_data[_tid].NameText;

        //ZLog.LogError("GetActionName - Can't Find ActionID : " + _tid);
        return "";
    }

    public static E_AbilityViewType GetActionParseType(uint _tid)
    {
        if (GameDBManager.Container.AbilityAction_Table_data.ContainsKey(_tid))
            return GameDBManager.Container.AbilityAction_Table_data[_tid].AbilityViewType;

        //ZLog.LogError("GetActionParseType - Can't Find ActionID : " + _tid);
        return default;
    }

    public static string GetActionTooltip(uint _tid)
    {
        if (GameDBManager.Container.AbilityAction_Table_data.ContainsKey(_tid))
            return GameDBManager.Container.AbilityAction_Table_data[_tid].ToolTip;

        //ZLog.LogError("GetActionTooltip - Can't Find ActionID : " + _tid);
        return "";
    }

    public static string GetActionIcon(uint _tid)
    {
        if (GameDBManager.Container.AbilityAction_Table_data.ContainsKey(_tid))
            return GameDBManager.Container.AbilityAction_Table_data[_tid].BuffIconString;

        //ZLog.LogError("GetActionIcon - Can't Find ActionID : "+_tid);
        return "";
    }

    public static ulong GetMaxBuffStackTime(uint _tid)
    {
        if (GameDBManager.Container.AbilityAction_Table_data.ContainsKey(_tid))
            return GameDBManager.Container.AbilityAction_Table_data[_tid].MaxBuffStack * (ulong)GameDBManager.Container.AbilityAction_Table_data[_tid].MinSupportTime;

        //ZLog.LogError("GetMaxBuffStackTime - Can't Find ActionID : " + _tid);
        return 0;
    }

    public static bool OverMaxBuffStackTime(uint _tid,uint RemainTime)
    {
        if (GameDBManager.Container.AbilityAction_Table_data.ContainsKey(_tid))
        {
            if (GameDBManager.Container.AbilityAction_Table_data[_tid].MaxBuffStack <= 0)
                return false;

            return GameDBManager.Container.AbilityAction_Table_data[_tid].MaxBuffStack <= (UnityEngine.Mathf.CeilToInt((float)RemainTime /(float)GameDBManager.Container.AbilityAction_Table_data[_tid].MinSupportTime));
        }

        //ZLog.LogError("GetMaxBuffStackTime - Can't Find ActionID : " + _tid);
        return false;
    }

    public static AbilityAction_Table GetAction(uint _tid)
	{
        if(GameDBManager.Container.AbilityAction_Table_data.ContainsKey(_tid))
		    return GameDBManager.Container.AbilityAction_Table_data[_tid];

        //ZLog.LogError("AbilityAction_Table GetAction - Can't Find AbilityActionTid : "+ _tid);
        return null;
    }

    public static E_AbilityActionType GetActionType(uint _tid)
    {
        if (GameDBManager.Container.AbilityAction_Table_data.ContainsKey(_tid))
            return GameDBManager.Container.AbilityAction_Table_data[_tid].AbilityActionType;

        //ZLog.LogError("GetActionType GetAction - Can't Find AbilityActionTid : " + _tid);
        return default;
    }

    public static E_AbilityType[] GetAllAbility(uint _tid)
    {
        List<E_AbilityType> retVal = new List<E_AbilityType>();
        if (GameDBManager.Container.AbilityAction_Table_data.ContainsKey(_tid))
        {
            if (GameDBManager.Container.AbilityAction_Table_data[_tid].AbilityID_01 != 0)
                retVal.Add(GameDBManager.Container.AbilityAction_Table_data[_tid].AbilityID_01);
            if (GameDBManager.Container.AbilityAction_Table_data[_tid].AbilityID_02 != 0)
                retVal.Add(GameDBManager.Container.AbilityAction_Table_data[_tid].AbilityID_02);
            if (GameDBManager.Container.AbilityAction_Table_data[_tid].AbilityID_03 != 0)
                retVal.Add(GameDBManager.Container.AbilityAction_Table_data[_tid].AbilityID_03);
            if (GameDBManager.Container.AbilityAction_Table_data[_tid].AbilityID_04 != 0)
                retVal.Add(GameDBManager.Container.AbilityAction_Table_data[_tid].AbilityID_04);
            if (GameDBManager.Container.AbilityAction_Table_data[_tid].AbilityID_05 != 0)
                retVal.Add(GameDBManager.Container.AbilityAction_Table_data[_tid].AbilityID_05);
            if (GameDBManager.Container.AbilityAction_Table_data[_tid].AbilityID_06 != 0)
                retVal.Add(GameDBManager.Container.AbilityAction_Table_data[_tid].AbilityID_06);
            if (GameDBManager.Container.AbilityAction_Table_data[_tid].AbilityID_07 != 0)
                retVal.Add(GameDBManager.Container.AbilityAction_Table_data[_tid].AbilityID_07);
            if (GameDBManager.Container.AbilityAction_Table_data[_tid].AbilityID_08 != 0)
                retVal.Add(GameDBManager.Container.AbilityAction_Table_data[_tid].AbilityID_08);
            if (GameDBManager.Container.AbilityAction_Table_data[_tid].AbilityID_09 != 0)
                retVal.Add(GameDBManager.Container.AbilityAction_Table_data[_tid].AbilityID_09);
        }

        return retVal.ToArray();
    }

    public static E_AbilityType GetFirstAbility(uint _tid)
    {
        if (false == GameDBManager.Container.AbilityAction_Table_data.ContainsKey(_tid))
            return 0;

        return GameDBManager.Container.AbilityAction_Table_data[_tid].AbilityID_01;
    }

    public static Dictionary<E_AbilityType, System.ValueTuple<float,float>> GetAllAbilityData(uint _tid, float fWeight = 0.5f)
    {
        Dictionary<E_AbilityType, System.ValueTuple<float, float>> retValue = new Dictionary<E_AbilityType, System.ValueTuple<float, float>>();

        if (GameDBManager.Container.AbilityAction_Table_data.ContainsKey(_tid))
        {
            E_AbilityType key = default;
            float value = default;

            if (GameDBManager.Container.AbilityAction_Table_data[_tid].AbilityID_01 != 0)
            {
                key = GameDBManager.Container.AbilityAction_Table_data[_tid].AbilityID_01;

                if (!retValue.ContainsKey(key))
                    retValue.Add(key, (0,0));
                retValue[key] = (retValue[key].Item1 + GameDBManager.Container.AbilityAction_Table_data[_tid].AbilityPoint_01_Min,
                                 retValue[key].Item2 + GameDBManager.Container.AbilityAction_Table_data[_tid].AbilityPoint_01_Max);
            }

            if (GameDBManager.Container.AbilityAction_Table_data[_tid].AbilityID_02 != 0)
            {
                key = GameDBManager.Container.AbilityAction_Table_data[_tid].AbilityID_02;
                value = GameDBManager.Container.AbilityAction_Table_data[_tid].AbilityPoint_02;

                if (!retValue.ContainsKey(key))
                    retValue.Add(key, (0,0));
                retValue[key] = (retValue[key].Item1 + value, retValue[key].Item2);
            }

            if (GameDBManager.Container.AbilityAction_Table_data[_tid].AbilityID_03 != 0)
            {

                key = GameDBManager.Container.AbilityAction_Table_data[_tid].AbilityID_03;
                value = GameDBManager.Container.AbilityAction_Table_data[_tid].AbilityPoint_03;

                if (!retValue.ContainsKey(key))
                    retValue.Add(key, (0, 0));
                retValue[key] = (retValue[key].Item1 + value, retValue[key].Item2);
            }

            if (GameDBManager.Container.AbilityAction_Table_data[_tid].AbilityID_04 != 0)
            {
                key = GameDBManager.Container.AbilityAction_Table_data[_tid].AbilityID_04;
                value = GameDBManager.Container.AbilityAction_Table_data[_tid].AbilityPoint_04;

                if (!retValue.ContainsKey(key))
                    retValue.Add(key, (0, 0));
                retValue[key] = (retValue[key].Item1 + value, retValue[key].Item2);
            }

            if (GameDBManager.Container.AbilityAction_Table_data[_tid].AbilityID_05 != 0)
            {
                key = GameDBManager.Container.AbilityAction_Table_data[_tid].AbilityID_05;
                value = GameDBManager.Container.AbilityAction_Table_data[_tid].AbilityPoint_05;

                if (!retValue.ContainsKey(key))
                    retValue.Add(key, (0, 0));
                retValue[key] = (retValue[key].Item1 + value, retValue[key].Item2);
            }

            if (GameDBManager.Container.AbilityAction_Table_data[_tid].AbilityID_06 != 0)
            {
                key = GameDBManager.Container.AbilityAction_Table_data[_tid].AbilityID_06;
                value = GameDBManager.Container.AbilityAction_Table_data[_tid].AbilityPoint_06;

                if (!retValue.ContainsKey(key))
                    retValue.Add(key, (0, 0));
                retValue[key] = (retValue[key].Item1 + value, retValue[key].Item2);
            }

            if (GameDBManager.Container.AbilityAction_Table_data[_tid].AbilityID_07 != 0)
            {
                key = GameDBManager.Container.AbilityAction_Table_data[_tid].AbilityID_07;
                value = GameDBManager.Container.AbilityAction_Table_data[_tid].AbilityPoint_07;

                if (!retValue.ContainsKey(key))
                    retValue.Add(key, (0, 0));
                retValue[key] = (retValue[key].Item1 + value, retValue[key].Item2);
            }

            if (GameDBManager.Container.AbilityAction_Table_data[_tid].AbilityID_08 != 0)
            {
                key = GameDBManager.Container.AbilityAction_Table_data[_tid].AbilityID_08;
                value = GameDBManager.Container.AbilityAction_Table_data[_tid].AbilityPoint_08;

                if (!retValue.ContainsKey(key))
                    retValue.Add(key, (0, 0));
                retValue[key] = (retValue[key].Item1 + value, retValue[key].Item2);
            }

            if (GameDBManager.Container.AbilityAction_Table_data[_tid].AbilityID_09 != 0)
            {
                key = GameDBManager.Container.AbilityAction_Table_data[_tid].AbilityID_09;
                value = GameDBManager.Container.AbilityAction_Table_data[_tid].AbilityPoint_09;

                if (!retValue.ContainsKey(key))
                    retValue.Add(key, (0, 0));
                retValue[key] = (retValue[key].Item1 + value, retValue[key].Item2);
            }
        }

        return retValue;
    }

    public static bool IsDeBuff(uint AbilityActionId)
    {
        if (GameDBManager.Container.AbilityAction_Table_data.ContainsKey(AbilityActionId))
            return GameDBManager.Container.AbilityAction_Table_data[AbilityActionId].BuffType == E_BuffType.DeBuff;

        //ZLog.LogError("IsDeBuff - Can't Find AbilityAction : " + AbilityActionId);
        return false;
    }

    public static string ParseAbilityActions(string strSplitVale = " ", string strSplitAbility = "\n", params uint[] actionIds)
    {
        System.Text.StringBuilder builder = new System.Text.StringBuilder();

        Dictionary<E_AbilityType, System.ValueTuple<float, float>> abilitys = new Dictionary<E_AbilityType, System.ValueTuple<float, float>>();

        Dictionary<uint, Dictionary<E_AbilityType, System.ValueTuple<float, float>>> changeAbilitys = new Dictionary<uint, Dictionary<E_AbilityType, (float, float)>>();

        foreach (var actionId in actionIds)
        {
            if (actionId == 0)
                continue;

            var abilityActionData = GetAction(actionId);
            if (abilityActionData.AbilityViewType == GameDB.E_AbilityViewType.ToolTip)
            {
                if (builder.Length > 0)
                    builder.Append(strSplitAbility);
                builder.AppendFormat("{0}{1}", abilityActionData.ChangeArtifactCheck != 0 ? string.Format("({0})", DBChange.GetChangeFullName(abilityActionData.ChangeArtifactCheck)) : "", DBLocale.ParseAbilityTooltip(abilityActionData, "1", "2"));
            }
            else
            {
                var enumer = GetAllAbilityData(actionId).GetEnumerator();
                while (enumer.MoveNext())
                {
                    if (abilityActionData.ChangeArtifactCheck != 0)
                    {
                        if (!changeAbilitys.ContainsKey(abilityActionData.ChangeArtifactCheck))
                            changeAbilitys.Add(abilityActionData.ChangeArtifactCheck, new Dictionary<E_AbilityType, (float, float)>());

                        if (changeAbilitys[abilityActionData.ChangeArtifactCheck].ContainsKey(enumer.Current.Key))
                            changeAbilitys[abilityActionData.ChangeArtifactCheck][enumer.Current.Key] = (changeAbilitys[abilityActionData.ChangeArtifactCheck][enumer.Current.Key].Item1 + enumer.Current.Value.Item1, changeAbilitys[abilityActionData.ChangeArtifactCheck][enumer.Current.Key].Item2 + enumer.Current.Value.Item2);
                        else
                            changeAbilitys[abilityActionData.ChangeArtifactCheck].Add(enumer.Current.Key, enumer.Current.Value);
                    }
                    else
                    {
                        if (abilitys.ContainsKey(enumer.Current.Key))
                            abilitys[enumer.Current.Key] = (abilitys[enumer.Current.Key].Item1 + enumer.Current.Value.Item1, abilitys[enumer.Current.Key].Item2 + enumer.Current.Value.Item2);
                        else
                            abilitys.Add(enumer.Current.Key, enumer.Current.Value);
                    }
                }
            }
        }

        foreach (var key in abilitys.Keys)
        {
            if (!IsParseAbility(key))
                continue;

            if (builder.Length > 0)
                builder.Append(strSplitAbility);
            builder.AppendFormat("{0}{1}{2}", DBLocale.GetText(GetAbilityName(key)), strSplitVale, ParseAbilityValue(key, abilitys[key].Item1, abilitys[key].Item2));
        }

        foreach (var classTid in changeAbilitys.Keys)
        {
            foreach (var key in changeAbilitys[classTid].Keys)
            {
                if (!IsParseAbility(key))
                    continue;

                if (builder.Length > 0)
                    builder.Append(strSplitAbility);
                builder.AppendFormat("({0}){1}{2}{3}", DBChange.GetChangeFullName(classTid), DBLocale.GetText(GetAbilityName(key)), strSplitVale, ParseAbilityValue(key, abilitys[key].Item1, abilitys[key].Item2));
            }
        }

        return builder.ToString();
    }
    #endregion

    #region Ability
    public static bool GetAbility(E_AbilityType type, out Ability_Table _table)
	{
		return GameDBManager.Container.Ability_Table_data.TryGetValue((uint)type, out _table);
	}

	public static Ability_Table GetAbility(E_AbilityType type)
	{
		if (GameDBManager.Container.Ability_Table_data.ContainsKey((uint)type))
			return GameDBManager.Container.Ability_Table_data[(uint)type];

		//ZLog.LogError("Ability_Table GetAbility - Can't Find AbilityID : " + type);
		return null;
	}

    public static string GetAbilityName(E_AbilityType getType)
    {
        if (GameDBManager.Container.Ability_Table_data.ContainsKey((uint)getType))
        {
            return GameDBManager.Container.Ability_Table_data[(uint)getType].StringName;
        }

        //ZLog.LogError("GetAbilityName - Can't Find AbilityType : "+getType);
        return getType.ToString();
    }

    public static E_LocaleType GetAbilityLocaleType(E_AbilityType getType)
    {
        if (GameDBManager.Container.Ability_Table_data.ContainsKey((uint)getType))
            return GameDBManager.Container.Ability_Table_data[(uint)getType].LocaleType;

        //ZLog.LogError("GetAbilityLocaleType - Can't Find AbilityType : " + getType);
        return default;
    }

    public static bool IsParseAbility(E_AbilityType abilityType)
    {
        if (GameDBManager.Container.Ability_Table_data.TryGetValue((uint)abilityType,out var abilityData))
        {
            switch (abilityData.LocaleType)
            {
                case E_LocaleType.AbilityPoint:
                case E_LocaleType.SupportTime:
                case E_LocaleType.None:
                    return true;
                case E_LocaleType.Empty:
                    return false;
            }
        }

        return false;
    }

    //public static string ParseAbility(int viewSize,E_AbilityType abilityType, float MinValue,float MaxValue=0)
    //{
    //    if (!GameDBManager.Container.Ability_Table_data.ContainsKey((uint)abilityType))
    //    {
    //        //ZLog.LogError("ParseAbility - Can't Find Ability : " + abilityType);

    //        if(MaxValue > 0 && MinValue != MaxValue)
    //            return string.Format("{0} {1,"+viewSize+"}~{2}",abilityType.ToString(), MinValue,MaxValue);
    //        else
    //            return string.Format("{0} {1," + viewSize + "}", abilityType.ToString(), MinValue.ToString());
    //    }

    //    var abilityData = GameDBManager.Container.Ability_Table_data[(uint)abilityType];

    //    string abilityName = DBLocale.GetLocaleText(abilityData.StringName);

        
    //    int widthCount = viewSize - System.Text.Encoding.Default.GetBytes(abilityName).Length;

    //    string strSingleformat = "{0,-" + widthCount + "}{1,"+ viewSize + "}";
    //    string strMultiformat = "{0,-" + widthCount + "}{1,"+ viewSize+"}~{2}";

    //    switch (abilityData.LocaleType)
    //    {
    //        case E_LocaleType.None:
    //            return DBLocale.GetLocaleText(abilityData.StringName);
    //        case E_LocaleType.AbilityPoint:
    //            if (MaxValue > 0 && MinValue != MaxValue)
    //                return string.Format(strMultiformat, DBLocale.GetLocaleText(abilityData.StringName), ParseAbilityValue((uint)abilityType, MinValue), ParseAbilityValue((uint)abilityType, MaxValue));
    //            else
    //                return string.Format(strSingleformat, DBLocale.GetLocaleText(abilityData.StringName), ParseAbilityValue((uint)abilityType, MinValue));
    //        case E_LocaleType.SupportTime:
    //            if (MaxValue > 0 && MinValue != MaxValue)
    //                return string.Format(strMultiformat, DBLocale.GetLocaleText(abilityData.StringName), ParseAbilityValue((uint)abilityType, MinValue), ParseAbilityValue((uint)abilityType, MaxValue));
    //            else
    //                return string.Format(strSingleformat, DBLocale.GetLocaleText(abilityData.StringName), ParseAbilityValue((uint)abilityType, MinValue));
    //        case E_LocaleType.Empty:
    //        default:
    //            return "";
    //    }
    //}

    public static string ParseAbilityValue(E_AbilityType abilityType,float MinValue, float MaxValue = 0,string Prefix = "")
    {
        if (!GameDBManager.Container.Ability_Table_data.ContainsKey((uint)abilityType))
        {
            //ZLog.LogError("ParseAbility - Can't Find Ability : " + abilityType);

            if (MaxValue > 0 && MinValue != MaxValue)
                return string.Format("{0}{1}~{2}", Prefix,abilityType.ToString(), MinValue, MaxValue);
            else
                return string.Format("{0}{1}", Prefix, abilityType.ToString(), MinValue.ToString());
        }

        var abilityData = GameDBManager.Container.Ability_Table_data[(uint)abilityType];

        switch (abilityData.LocaleType)
        {
            case E_LocaleType.AbilityPoint:
                {
                    if (MaxValue > 0 && MinValue != MaxValue)
                        return string.Format("{0}{1}~{2}", Prefix, ParseAbilityValue((uint)abilityType, MinValue), ParseAbilityValue((uint)abilityType, MaxValue));
                    else
                        return string.Format("{0}{1}", Prefix, ParseAbilityValue((uint)abilityType, MinValue));
                }
            case E_LocaleType.SupportTime:
            case E_LocaleType.None:
            case E_LocaleType.Empty:
            default:
                return "";
        }
    }

    public static string ParseAbilityValue(uint AbilityId, float Value, float ApplyPerValue = 1f)
    {
        if (!GameDBManager.Container.Ability_Table_data.ContainsKey(AbilityId))
        {
           // ZLog.LogError("ParseAbilityValue - Can't Find Ability : " + AbilityId);
            return Value.ToString();
        }

        var abilityData = GameDBManager.Container.Ability_Table_data[AbilityId];

        //펫 마법 공격력 표시는 그대로
        //switch((E_AbilityType)AbilityId)
        //{
        //    case E_AbilityType.RUNE_MAGIC_ATTACK_PLUS:
        //        {
        //            if(1 <= Value)
        //            {
        //                Value = UnityEngine.Mathf.Max(1, Value/DBConfig.MagicAttackViewValue);
        //            }
        //        }
        //        break;
        //}

        if (abilityData.Unit >= 1)
            Value -= Value % abilityData.Unit;

        int Cnt = 0;
        if (abilityData.Unit > 0)
            Cnt = (int)UnityEngine.Mathf.Log10(1 / abilityData.Unit);

        if (abilityData.MarkType == E_MarkType.Per)
        {
            int ivalue = (int)(Value * ApplyPerValue * 1.000001f);

            if (abilityData.PlusOutput == E_PlusOutput.Output)
            {
                if(ivalue > 0)
                    return string.Format("+{0:0}%", ivalue);
                else
                    return string.Format("{0:0}%", ivalue);
            }
            else
                return string.Format("{0:0}%", ivalue);
        }
        else
        {
            if (abilityData.PlusOutput == E_PlusOutput.Output)
            {
                if ((Value * ApplyPerValue) > 0)
                    return string.Format("+{0}",Value.ToString("F" + Cnt));
                else
                    return Value.ToString("F" + Cnt);
            }
            else
                return Value.ToString("F" + Cnt);
        }
    }

    /// <summary> AbilityActionTid로 <AbilityType, <MinValue, MaxValue, AbilityActionTid>> 를 얻어온다. </summary>
    //public static void SetAbilityValues(uint abilityActionTid, ref StatForPetUI stat)
    //{
    //    if (abilityActionTid <= 0)
    //        return;

    //    stat.Add(abilityActionTid);
    //}

    //public static void SetAbilityValues(uint abilityActionTid, ref StatForDefaultUI stat)
    //{
    //    if (abilityActionTid <= 0)
    //        return;

    //    stat.Add(abilityActionTid);
    //}
    #endregion

    #region Parse
    //public static string ParseUseAbilitys(List<uint> abilityActions, List<uint> removeabilityActions = null, string splitValue = " ",string splitAbility = "\n")
    //{
    //    System.Text.StringBuilder strBuilder = new System.Text.StringBuilder();

    //    Dictionary<GameDB.E_AbilityType, System.ValueTuple<float, float>> baseabilitys = new Dictionary<GameDB.E_AbilityType, System.ValueTuple<float, float>>();

    //    Dictionary<GameDB.E_AbilityType, System.ValueTuple<float, float>> abilitys = new Dictionary<GameDB.E_AbilityType, System.ValueTuple<float, float>>();

    //    Dictionary<uint, Dictionary<GameDB.E_AbilityType, System.ValueTuple<float, float>>> changebaseAbilitys = new Dictionary<uint, Dictionary<E_AbilityType, (float, float)>>();

    //    Dictionary<uint, Dictionary<GameDB.E_AbilityType, System.ValueTuple<float, float>>> changeAbilitys = new Dictionary<uint, Dictionary<E_AbilityType, (float, float)>>();

    //    if (removeabilityActions != null)
    //    {
    //        foreach (var baseActionId in removeabilityActions)
    //        {
    //            var abilityActionData = DBAbility.GetAction(baseActionId);
    //            if (abilityActionData.AbilityViewType != GameDB.E_AbilityViewType.ToolTip)
    //            {
    //                var enumer = DBAbility.GetAllAbilityData(baseActionId).GetEnumerator();
    //                while (enumer.MoveNext())
    //                {
    //                    if (abilityActionData.ChangeArtifactCheck != 0)
    //                    {
    //                        if (!changebaseAbilitys.ContainsKey(abilityActionData.ChangeArtifactCheck))
    //                            changebaseAbilitys.Add(abilityActionData.ChangeArtifactCheck,new Dictionary<E_AbilityType, (float, float)>());

    //                        if (changebaseAbilitys[abilityActionData.ChangeArtifactCheck].ContainsKey(enumer.Current.Key))
    //                            changebaseAbilitys[abilityActionData.ChangeArtifactCheck][enumer.Current.Key] = (changebaseAbilitys[abilityActionData.ChangeArtifactCheck][enumer.Current.Key].Item1 + enumer.Current.Value.Item1, changebaseAbilitys[abilityActionData.ChangeArtifactCheck][enumer.Current.Key].Item2 + enumer.Current.Value.Item2);
    //                        else
    //                            changebaseAbilitys[abilityActionData.ChangeArtifactCheck].Add(enumer.Current.Key, enumer.Current.Value);
    //                    }
    //                    else
    //                    {
    //                        if (baseabilitys.ContainsKey(enumer.Current.Key))
    //                            baseabilitys[enumer.Current.Key] = (baseabilitys[enumer.Current.Key].Item1 + enumer.Current.Value.Item1, baseabilitys[enumer.Current.Key].Item2 + enumer.Current.Value.Item2);
    //                        else
    //                            baseabilitys.Add(enumer.Current.Key, enumer.Current.Value);
    //                    }
    //                }
    //            }
    //        }
    //    }

    //    foreach (var abilityActionId in abilityActions)
    //    {
    //        var abilityActionData = DBAbility.GetAction(abilityActionId);
    //        if (abilityActionData.AbilityViewType == GameDB.E_AbilityViewType.ToolTip)
    //        {
    //            if (strBuilder.Length > 0)
    //                strBuilder.Append(splitAbility);

    //            if (!removeabilityActions.Contains(abilityActionId))
    //            {
    //                if (abilityActionData.ChangeArtifactCheck != 0)
    //                {
    //                    strBuilder.AppendFormat(DBLocale.GetLocaleText("AbilityChangeText"), DBChange.GetChangeFullName(abilityActionData.ChangeArtifactCheck));
    //                    if (strBuilder.Length > 0)
    //                        strBuilder.Append(splitAbility);
    //                }

    //                strBuilder.Append(DBLocale.ParseAbilityTooltip(abilityActionData, "2", "1"));
    //            }
    //        }
    //        else
    //        {
    //            var enumer = DBAbility.GetAllAbilityData(abilityActionId).GetEnumerator();
    //            while (enumer.MoveNext())
    //            {
    //                if (abilityActionData.ChangeArtifactCheck != 0)
    //                {
    //                    if (!changeAbilitys.ContainsKey(abilityActionData.ChangeArtifactCheck))
    //                        changeAbilitys.Add(abilityActionData.ChangeArtifactCheck, new Dictionary<E_AbilityType, (float, float)>());

    //                    if (changeAbilitys[abilityActionData.ChangeArtifactCheck].ContainsKey(enumer.Current.Key))
    //                        changeAbilitys[abilityActionData.ChangeArtifactCheck][enumer.Current.Key] = (changeAbilitys[abilityActionData.ChangeArtifactCheck][enumer.Current.Key].Item1 + enumer.Current.Value.Item1, changeAbilitys[abilityActionData.ChangeArtifactCheck][enumer.Current.Key].Item2 + enumer.Current.Value.Item2);
    //                    else
    //                        changeAbilitys[abilityActionData.ChangeArtifactCheck].Add(enumer.Current.Key, enumer.Current.Value);
    //                }
    //                else
    //                {
    //                    if (abilitys.ContainsKey(enumer.Current.Key))
    //                        abilitys[enumer.Current.Key] = (abilitys[enumer.Current.Key].Item1 + enumer.Current.Value.Item1, abilitys[enumer.Current.Key].Item2 + enumer.Current.Value.Item2);
    //                    else
    //                        abilitys.Add(enumer.Current.Key, enumer.Current.Value);
    //                }
    //            }
    //        }
    //    }

    //    foreach (var key in abilitys.Keys)
    //    {
    //        if (!DBAbility.IsParseAbility(key))
    //            continue;

    //        float abilityminValue = (uint)abilitys[key].Item1;
    //        float abilitymaxValue = (uint)abilitys[key].Item2;

    //        if (abilityminValue <= 0 && abilitymaxValue <= 0)
    //            continue;

    //        if (baseabilitys.ContainsKey(key) && ((uint)baseabilitys[key].Item1 > 0 || (uint)baseabilitys[key].Item2 > 0))
    //        {
    //            float baseAbilityminValue = (uint)baseabilitys[key].Item1;
    //            float baseAbilitymaxValue = (uint)baseabilitys[key].Item2;

    //            var basevalue = DBAbility.ParseAbilityValue(key, baseAbilityminValue, baseAbilitymaxValue);

    //            if (baseAbilityminValue != abilityminValue || baseAbilitymaxValue != abilitymaxValue)
    //            {
    //                var addValue = DBAbility.ParseAbilityValue(key, abilityminValue - baseAbilityminValue, abilitymaxValue - baseAbilitymaxValue);

    //                if(strBuilder.Length > 0)
    //                    strBuilder.Append(splitAbility);
    //                strBuilder.AppendFormat("{0}{1}{2}", DBLocale.GetLocaleText(DBAbility.GetAbilityName(key)), splitValue, addValue);
    //            }
    //        }
    //        else
    //        {
    //            var newValue = DBAbility.ParseAbilityValue(key, abilityminValue, abilitymaxValue);

    //            if (strBuilder.Length > 0)
    //                strBuilder.Append(splitAbility);
    //            strBuilder.AppendFormat("{0}{1}{2}", DBLocale.GetLocaleText(DBAbility.GetAbilityName(key)), splitValue, newValue);
    //        }
    //    }

    //    foreach (var classTid in changeAbilitys.Keys)
    //    {
    //        if (strBuilder.Length > 0)
    //            strBuilder.Append(splitAbility);
    //        strBuilder.AppendFormat(DBLocale.GetLocaleText("AbilityChangeText"), DBChange.GetChangeFullName(classTid));

    //        foreach (var key in changeAbilitys[classTid].Keys)
    //        {
    //            if (!DBAbility.IsParseAbility(key))
    //                continue;

    //            float abilityminValue = (uint)changeAbilitys[classTid][key].Item1;
    //            float abilitymaxValue = (uint)changeAbilitys[classTid][key].Item2;

    //            if (abilityminValue <= 0 && abilitymaxValue <= 0)
    //                continue;

    //            if (changebaseAbilitys.ContainsKey(classTid) && changebaseAbilitys[classTid].ContainsKey(key) && ((uint)changebaseAbilitys[classTid][key].Item1 > 0 || (uint)changebaseAbilitys[classTid][key].Item2 > 0))
    //            {
    //                float baseAbilityminValue = (uint)changebaseAbilitys[classTid][key].Item1;
    //                float baseAbilitymaxValue = (uint)changebaseAbilitys[classTid][key].Item2;

    //                var basevalue = DBAbility.ParseAbilityValue(key, baseAbilityminValue, baseAbilitymaxValue);

    //                if (baseAbilityminValue != abilityminValue || baseAbilitymaxValue != abilitymaxValue)
    //                {
    //                    var addValue = DBAbility.ParseAbilityValue(key, abilityminValue - baseAbilityminValue, abilitymaxValue - baseAbilitymaxValue);

    //                    if (strBuilder.Length > 0)
    //                        strBuilder.Append(splitAbility);
    //                    strBuilder.AppendFormat("{0}{1}{2}", DBLocale.GetLocaleText(DBAbility.GetAbilityName(key)), splitValue, addValue);
    //                }
    //            }
    //            else
    //            {
    //                var newValue = DBAbility.ParseAbilityValue(key, abilityminValue, abilitymaxValue);

    //                if (strBuilder.Length > 0)
    //                    strBuilder.Append(splitAbility);
    //                strBuilder.AppendFormat("{0}{1}{2}", DBLocale.GetLocaleText(DBAbility.GetAbilityName(key)), splitValue, newValue);
    //            }
    //        }
    //    }

    //    return strBuilder.ToString();
    //}
    #endregion
}
