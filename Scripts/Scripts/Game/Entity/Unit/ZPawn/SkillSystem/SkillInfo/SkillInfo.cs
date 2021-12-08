using GameDB;
using ZDefine;

/// <summary> My Pc 스킬 정보 </summary>
public class SkillInfo
{
    /// <summary> MyPc용 스킬 시스템 </summary>
    public SkillSystem SkillSystem { get; private set; }
    /// <summary> 스킬 오너 </summary>
    public ZPawn Owner { get { return SkillSystem.Owner; } }
    /// <summary> 스킬 테이블 </summary>
    public Skill_Table SkillTable { get; private set; }
    /// <summary> 스킬 순서 데이터 </summary>
    public SkillOrderData SkillOrderData { get { return ZNet.Data.Me.FindCurCharData?.SkillUseOrder.Find(a => a.Tid == SkillId); } }
    /// <summary> 리소스 테이블 </summary>
    public Resource_Table ResourceTable { get { return SkillSystem.ResourceTable; } }
    /// <summary> 타겟 타입을 얻어온다 </summary>
    public E_TargetType TargetType { get { return SkillTable.TargetType; } }

    /// <summary> 스킬 테이블 id </summary>
    public uint SkillId { get { return SkillTable.SkillID; } }
    /// <summary> 사거리 </summary>
    public float Distance { get { return SkillDistance + Owner.GetSkillAbility(SkillId, E_AbilityType.SKILL_DISTANCE_PLUS); } }

    /// <summary> 스킬의 기본 사거리 </summary>
    //public float SkillDistance { get; private set; }
    private float SkillDistance;

    /// <summary>  </summary>
    public byte Combo { get { return SkillTable.SkillSort; } }

    /// <summary> 스킬 사용가능한 시간 </summary>
    public ulong EndCoolTimeMs { get { return ZNet.Data.Me.FindCurCharData?.GetSkillEndCoolTime(SkillId) ?? 0; } }

    public ulong EndCustomCoolTimeMs { get { return EndCoolTimeMs + (ulong)((SkillOrderData?.CoolTime ?? 0) * TimeHelper.Unit_SecToMs); } }

    /// <summary> 남은 쿨타임 </summary>
    public ulong RemainCoolTime { get { return EndCoolTimeMs < TimeManager.NowMs ? 0 : EndCoolTimeMs - TimeManager.NowMs; } }

    public ulong RemainCustomCoolTime { get { return EndCustomCoolTimeMs < TimeManager.NowMs ? 0 : EndCustomCoolTimeMs - TimeManager.NowMs; } }

    /// <summary> 마나 체크 </summary>
    public bool CheckMp { get { return SkillTable.UseMPCount + Owner.GetSkillAbility(SkillId, E_AbilityType.SKILL_USE_MP_MIMUS) <= Owner.GetAbility(E_AbilityType.FINAL_CURR_MP); } }

    public uint Order { get { return SkillOrderData == null ? SkillTable.SkillSort : SkillOrderData.Order; } }

    public ulong BuffEndTime
    { 
        get 
        {
            //변환된 스킬이면 변환된 스킬로 체크하자
            if(CheckChangeSkill())
                return ZNet.Data.Me.CurCharData.GetSkillBuffEndTime(SkillTable.ChangeSkillID);

            return ZNet.Data.Me.CurCharData.GetSkillBuffEndTime(SkillId); 
        }
    }
    
    public bool IsAvailableAbilityAction { get { return BuffEndTime <= TimeManager.NowSec; } }

    public E_QuickSlotAutoType QuickSlotAutoType { get { return SkillTable.QuickSlotAutoType; } }

    public SkillInfo(SkillSystem skillSystem, uint skillId)
    {
        SkillSystem = skillSystem;
        SkillTable = DBSkill.Get(skillId);
        SkillDistance = SkillTable.Distance;
        //switch (SkillTable.RangeType)
        //{
        //    case E_RangeType.Straight:
        //    case E_RangeType.Angle:
        //        {
        //            SkillDistance = SkillTable.Range;
        //        }
        //        break;
        //    default:
        //        {
        //            SkillDistance = SkillTable.Distance;
        //        }
        //        break;
        //}
    }

    /// <summary> 쿨타임 체크 </summary>
    public bool CheckCoolTime(bool isAuto = false)
    {
        bool isUseSkillCycle = SkillOrderData != null && SkillOrderData.IsUseSkillCycle != 0;
        return TimeManager.NowMs >= ((isAuto && isUseSkillCycle) ? EndCustomCoolTimeMs : EndCoolTimeMs);
    }

    /// <summary> 활성화된 스킬인지 </summary>
    public bool CheckEnable()
    {
        if (false == Owner.IsMyPc)
            return true;

        return ZNet.Data.Me.CurCharData.HasGainSkill(SkillId);
    }

	/// <summary> 스킬 변환 가능 상태인지 </summary>
	public bool CheckChangeSkill()
	{
		if (false == Owner.IsMyPc)
			return false;
		return 0 < SkillTable.SubSkillID && 0 < SkillTable.ChangeSkillID && ZNet.Data.Me.CurCharData.HasGainSkill(SkillTable.SubSkillID);
	}

	/// <summary> 무기 장착 여부 </summary>
	public bool CheckWeapon()
    {
        return Owner.CheckEquippedWeapon(SkillTable.WeaponType);
    }

    /// <summary> 클래스 체크 </summary>
    public bool CheckCharacterType()
    {
        return Owner.CheckCharacterType(SkillTable.CharacterType);
    }

    /// <summary> 오토인 경우 힐 가능여부 체크 </summary>
    public bool CheckUseAutoHeal(bool isAuto)
    {
        if (isAuto && Owner.IsMyPc) {
            if (SkillTable.HealRate > 0 || SkillTable.AddHeal > 0) {
                if (Owner.MaxHp == Owner.CurrentHp) {
                    return false;
                }
            }
        }
        return true;
    }

    /// <summary> 마을에서 사용가능한 스킬 체크 </summary>
    public bool CheckUseInTown()
    {
        if (ZGameModeManager.Instance.StageTid == DBConfig.Town_Stage_ID) {
            if (SkillTable.TownUseType == E_TownUseType.NotUse) {
                return false;
            }

        }
        return true;
    }
}
