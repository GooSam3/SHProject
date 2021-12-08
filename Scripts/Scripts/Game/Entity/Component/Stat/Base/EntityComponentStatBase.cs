using GameDB;
using System;
using System.Collections.Generic;
using SkillAbilityType = System.Collections.Generic.Dictionary<uint, System.Collections.Generic.Dictionary<GameDB.E_AbilityType, float>>;

/// <summary> entity stat 관련 처리 </summary>
public class EntityComponentStatBase : EntityComponentBase<EntityBase>
{
    /// <summary> Late Update 등록 </summary>
    protected override bool EnableLateUpdate => true;

	/// <summary> 
	/// 스탯 변경시 통지 
	/// </summary>
	private event Action<Dictionary<E_AbilityType, float>> mEventAbilityUpdated;

    private event Action<SkillAbilityType> mEventSkillAbilityUpdated;

    private Action<float, float> mEventHpUpdated;
    private Action<float, float> mEventMpUpdated;

    private Action mEventWeightUpdated;

    private bool WeightUpdatedDirty;

    private bool HpUpdatedDirty;
    private bool MpUpdatedDirty;

    /// <summary> 캐릭터의 어빌리티 </summary>
    private Dictionary<E_AbilityType, float> m_dicAbility = new Dictionary<E_AbilityType, float>();

    /// <summary> 캐릭터 어빌리티 변경 통지 </summary>
    private Dictionary<E_AbilityType, float> m_dicAbilityDirty = new Dictionary<E_AbilityType, float>();

    /// <summary> 스킬로 인해 추가된 ability </summary>
    private SkillAbilityType m_dicSkillAbility = new SkillAbilityType();

    /// <summary> 스킬로 인해 추가/제거 된 ability 통지 </summary>
    private SkillAbilityType m_dicSkillAbilityDirty = new SkillAbilityType();

    public int Level { get; private set; }

    public float MaxHp { get { return m_dicAbility[E_AbilityType.FINAL_MAX_HP]; } }

    public float CurrentHp { get { return m_dicAbility[E_AbilityType.FINAL_CURR_HP]; } }

    public float MaxMp { get { return m_dicAbility[E_AbilityType.FINAL_MAX_MP]; } }
    
    public float CurrentMp { get { return m_dicAbility[E_AbilityType.FINAL_CURR_MP]; } }

    //public float MaxWeight { get { return m_dicAbility[E_AbilityType.FINAL_MAX_WEIGH]; } }

    //public float CurrentWeight { get { return m_dicAbility[E_AbilityType.ITEM_WEIGH]; } }

    public float AttackSpeedRate
    {
        get
        {
            if(m_dicAbility.TryGetValue(E_AbilityType.FINAL_ATTACK_SPEED, out float value))
            {
                return value;
            }
            return DefaultAttackSpeedRate;
        }
    }

    public float SkillSpeedRate
    {
        get
        {
            if (m_dicAbility.TryGetValue(E_AbilityType.FINAL_SKILL_SPEED, out float value))
            {
                return value;
            }
            return DefaultAttackSpeedRate;
        }
    }

    /// <summary> 기본 공격 속도 </summary>
    public float DefaultAttackSpeedRate = 1f;

    public float MoveSpeed
    {
        get
        {
            if (m_dicAbility.TryGetValue(E_AbilityType.FINAL_MOVE_SPEED, out float value))
            {
                return value;
            }
            return DefaultMoveSpeed;
        }
    }

    public float MoveSpeedRate
    {
        get
        {
            return MoveSpeed / DefaultMoveSpeed;
        }
    }

    /// <summary> 기본 이동 속도 </summary>
    public float DefaultMoveSpeed = 5f;

    protected override void OnInitializeComponentImpl()
    {
        m_dicAbility[E_AbilityType.FINAL_MAX_HP] = Owner.EntityData.MaxHp;
        m_dicAbility[E_AbilityType.FINAL_MAX_MP] = Owner.EntityData.MaxMp;
        m_dicAbility[E_AbilityType.FINAL_CURR_HP] = Owner.EntityData.CurrentHp;
        m_dicAbility[E_AbilityType.FINAL_CURR_MP] = Owner.EntityData.CurrentMp;

        SetDefaultStat();        
    }

    /// <summary> 테이블에 따라 기본 스탯을 셋팅한다. </summary>
    private void SetDefaultStat()
    {        
        DefaultMoveSpeed = 5f;        
        DefaultAttackSpeedRate = 1f;

        switch (Owner.EntityType)
        {
            case E_UnitType.Character:
                {
                    if(DBCharacter.TryGet(Owner.TableId, out var table))
                    {
                        DefaultMoveSpeed = table.RunSpeed;                        
                    }
                }
                break;
            case E_UnitType.Monster:
                {
                    if (DBMonster.TryGet(Owner.TableId, out var table))
                    {
                        DefaultMoveSpeed = table.RunSpeed;
                    }
                }
                break;
            case E_UnitType.NPC:
                {
                    if (DBNpc.TryGet(Owner.TableId, out var table))
                    {
                        DefaultMoveSpeed = table.RunSpeed;
                    }
                }
                break;
            case E_UnitType.Pet:
                {
                    if(DBPet.TryGet(Owner.TableId, out var table))
                    {
                        DefaultMoveSpeed = table.RunSpeed;
                    }
                }
                break;
        }        
    }

    protected override void OnDestroyImpl()
    {
    }

    protected override void OnLateUpdateImpl()
    {
        //tick당 한번만 업데이트 한다.
        if(m_dicAbilityDirty.Count > 0)
        {
            mEventAbilityUpdated?.Invoke(m_dicAbilityDirty);
            m_dicAbilityDirty.Clear();
        }

        if (m_dicSkillAbilityDirty.Count > 0)
        {
            mEventSkillAbilityUpdated?.Invoke(m_dicSkillAbilityDirty);
            m_dicSkillAbilityDirty.Clear();
        }

        if(WeightUpdatedDirty)
        {
            mEventWeightUpdated?.Invoke();
            WeightUpdatedDirty = false;
        }

        if(HpUpdatedDirty)
        {
            mEventHpUpdated?.Invoke(CurrentHp, MaxHp);
            HpUpdatedDirty = false;
        }

        if (MpUpdatedDirty)
        {
            mEventMpUpdated?.Invoke(CurrentMp, MaxMp);
            MpUpdatedDirty = false;
        }
    }

    /// <summary> TODO :: 일단 기본 속도 변경 </summary>    
    public void SetMoveSpeed(float value)
    {
        DefaultMoveSpeed = value;
    }

    public void SetAbility(E_AbilityType abilityType, float value)
    {        
        if(abilityType == E_AbilityType.FINAL_MAX_MAGIC_ATTACK)
        {
            //마법 공격력 표시용 예외처리
            value /= DBConfig.MagicAttackViewValue;
        }

        m_dicAbility[abilityType] = value;
                
        switch(abilityType)
        {
            //무게는 따로 처리한다.
            case E_AbilityType.FINAL_MAX_WEIGH:
            case E_AbilityType.ITEM_WEIGH:
                {
                    WeightUpdatedDirty = true;
                }
                break;
            case E_AbilityType.FINAL_MAX_HP:
            case E_AbilityType.FINAL_CURR_HP:
                {
                    HpUpdatedDirty = true;
                }
                break;
            case E_AbilityType.FINAL_MAX_MP:
            case E_AbilityType.FINAL_CURR_MP:
                {
                    MpUpdatedDirty = true;
                }
                break;
            default:
                {
                    m_dicAbilityDirty[abilityType] = value;
                }
                break;
        }
	}

    /// <summary> 스킬 어빌리티 셋팅 </summary>
    public void SetSkillAbility(uint skillTid, E_AbilityType abilityType, float value)
    {
        if(false == m_dicSkillAbility.ContainsKey(skillTid))
        {
            m_dicSkillAbility.Add(skillTid, new Dictionary<E_AbilityType, float>());
        }

        if (false == m_dicSkillAbilityDirty.ContainsKey(skillTid))
        {
            m_dicSkillAbilityDirty.Add(skillTid, new Dictionary<E_AbilityType, float>());
        }

        if (abilityType == E_AbilityType.FINAL_MAX_MAGIC_ATTACK)
        {
            //마법 공격력 표시용 예외처리
            value /= DBConfig.MagicAttackViewValue;
        }

        m_dicSkillAbility[skillTid][abilityType] = value;
        m_dicSkillAbilityDirty[skillTid][abilityType] = value;
    }

    public float GetAbility(E_AbilityType abilityType)
	{
		float retValue = 0;

		m_dicAbility.TryGetValue(abilityType, out retValue);

		return retValue;
	}

    public bool GetSkillAbilitys(uint skillTid, out Dictionary<E_AbilityType, float> values)
    {
        return m_dicSkillAbility.TryGetValue(skillTid, out values);
    }

    public float GetSkillAbility(uint skillTid, E_AbilityType abilityType)
    {
        float retValue = 0;

        if(m_dicSkillAbility.TryGetValue(skillTid, out var dict))
        {
            dict.TryGetValue(abilityType, out retValue);
        }

        return retValue;
    }

    #region Event
    public void DoAddEventAbilityUpdated(Action<Dictionary<E_AbilityType, float>> action, bool bGetCurrentValue)
    {
        DoRemoveEventAbilityUpdated(action);
        mEventAbilityUpdated += action;

        if(bGetCurrentValue)
        {
            action?.Invoke(m_dicAbility);
        }
    }

    public void DoRemoveEventAbilityUpdated(Action<Dictionary<E_AbilityType, float>> action)
    {
        mEventAbilityUpdated -= action;
    }

    public void DoAddEventSkillAbilityUpdated(Action<SkillAbilityType> action, bool bGetCurrentValue)
    {
        DoRemoveEventSkillAbilityUpdated(action);
        mEventSkillAbilityUpdated += action;

        if (bGetCurrentValue)
        {
            action?.Invoke(m_dicSkillAbility);
        }
    }

    public void DoRemoveEventSkillAbilityUpdated(Action<SkillAbilityType> action)
    {
        mEventSkillAbilityUpdated -= action;
    }

    public void DoAddEventWeightUpdated(Action action)
    {
        DoRemoveEventWeightUpdated(action);
        mEventWeightUpdated += action;
    }

    public void DoRemoveEventWeightUpdated(Action action)
    {
        mEventWeightUpdated -= action;
    }

    public void DoAddEventHpUpdated(Action<float, float> action)
    {
        DoRemoveEventHpUpdated(action);
        mEventHpUpdated += action;
    }

    public void DoRemoveEventHpUpdated(Action<float, float> action)
    {
        mEventHpUpdated -= action;
    }

    public void DoRemoveAllEventHpUpdated()
    {
        mEventHpUpdated = null;
    }

    public void DoAddEventMpUpdated(Action<float, float> action)
    {
        DoRemoveEventMpUpdated(action);
        mEventMpUpdated += action;
    }

    public void DoRemoveEventMpUpdated(Action<float, float> action)
    {
        mEventMpUpdated -= action;
    }
    #endregion;
}
