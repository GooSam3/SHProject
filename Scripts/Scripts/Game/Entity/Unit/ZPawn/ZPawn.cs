using GameDB;
using System;
using UnityEngine;

/// <summary> 모든 유닛의 기본 class </summary>
public abstract class ZPawn : EntityBase
{
    /// <summary> 유닛의 타입 </summary>
    public override E_UnitType EntityType => throw new NotImplementedException();

    /// <summary> 스킬 연출중인지 여부 </summary>
    public bool IsSkillAction { get; set; }

    /// <summary> hit 모션을 스킵할지 </summary>
    public bool IsSkipHitMotion { get; set; }

    /// <summary> 자신의 매즈 상태 </summary>
    public E_ConditionControl MezState { get { return PawnData.MezState; } }

    /// <summary> 클라용 상태 체크 </summary>
    public E_CustomConditionControl ConditionControl { get; private set; }

    /// <summary> 자신의 성향치 </summary>
    public int Tendency { get { return PawnData.Tendency; } }

    public ZPawnDataBase PawnData { get; private set; }

    public E_UnitAttributeType UnitAttributeType { get; protected set; }

    public E_AttributeLevel AttributeLevel { get; protected set; } = E_AttributeLevel.Level_1;

    public virtual bool IsRiding { get { return false; } }

    /// <summary> 원격이동 여부 </summary>
    public bool IsRemoteMove { get; protected set; } = true;

    protected override void OnInitializeEntityData(EntityDataBase data)
    {
        PawnData = data.To<ZPawnDataBase>();
        SetEntityData(PawnData);        
    }

    private void SetEntityData(ZPawnDataBase ZPawnData)
    {
        ChangeMezState((uint)ZPawnData.MezState);

        SetAttributeType();
    }

    /// <summary> 속성 셋팅 </summary>
    protected abstract void SetAttributeType();

    /// <summary> 테이블 셋팅 이후 호출 </summary>
    protected override void OnPostInitializeImpl()
    {
        if (0 < PawnData.DestPositions.Count)
        {
            MoveTo(PawnData.DestPositions, PawnData.MoveSpeed);
        }

        //스킬 시스템 셋팅
        SetSkillSystem();
    }

    protected override void OnInitializeImpl() { }
    protected override void OnDestroyImpl() { }
    /// <summary> 테이블 셋팅 </summary>
    protected override void OnInitializeTableDataImpl() { }

    protected override void OnSetDefaultComponentImpl()
    {
        CombatComponent = OnSetCombatComponent();
        AbilityActionComponent = OnSetAbilityActionComponent();
        EffectComponent = OnSetEffectComponent();
        AIComponent = OnSetAIComponent();
    }


    #region ===== :: Component :: =====
    /// <summary> 전투 관련 컴포넌트 </summary>
    protected EntityComponentCombat CombatComponent = null;
    /// <summary> 어빌리티 액션 관련 컴포넌트 </summary>
    protected EntityComponentAbilityAction AbilityActionComponent = null;
    /// <summary> ZPawn 이펙트 관리용 </summary>
    protected EntityComponentEffect EffectComponent = null;
    /// <summary> AI 관련 컴포넌트 </summary>
    protected EntityComponentAI AIComponent = null;


    /// <summary> 전투 컴포넌트 셋팅 </summary>
    protected virtual EntityComponentCombat OnSetCombatComponent()
    {
        return GetOrAddPawnComponent<EntityComponentCombat>();
    }
    /// <summary> 어빌리티 액션 관련 컴포넌트 </summary>
    protected virtual EntityComponentAbilityAction OnSetAbilityActionComponent()
    {
        return GetOrAddPawnComponent<EntityComponentAbilityAction>();
    }

    /// <summary> 어빌리티 액션 관련 컴포넌트 </summary>
    protected virtual EntityComponentEffect OnSetEffectComponent()
    {
        return GetOrAddPawnComponent<EntityComponentEffect>();
    }

    /// <summary> AI 관련 컴포넌트 </summary>
    protected virtual EntityComponentAI OnSetAIComponent()
    {
        return GetOrAddPawnComponent<EntityComponentAI>();
    }

    protected COMPONENT_TYPE GetOrAddPawnComponent<COMPONENT_TYPE>() where COMPONENT_TYPE : EntityComponentBase<ZPawn>
    {
        COMPONENT_TYPE comp = gameObject.GetOrAddComponent<COMPONENT_TYPE>();
        comp.InitializeComponent(this);
        return comp;
    }
    #endregion

    #region ===== :: Movement:: =====
    /// <summary> 강제 이동 </summary>
    public override void ForceMove(Vector3 position, float duration, E_PosMoveType moveType)
    {
        base.ForceMove(position, duration, moveType);
        CombatComponent?.ForceMove(position, duration, moveType);
    }
    #endregion

    #region ===== :: Model :: =====
    protected override void OnLoadedModelImpl()
    {
        if(IsDead)
        {
            DieAnim(true, 1f);
        }
    }


    /// <summary> 모델 및 네임테그 활성화 비활성화 처리 </summary>
    public void SetActive(bool bActive)
    {
        ModelComponent?.SetActiveRenderers(bActive);

        UIManager.Instance.Find<UIFrameNameTag>()?.DoUINameTagActive(this, bActive);
    }


    #endregion

    #region ===== :: Combat :: =====

    /// <summary> 마지막으로 나를 공격한 Entity 의 id </summary>
    public uint AttackerEntityId { get; private set; }    

    public void UseSkill(Vector3 pos, uint targetEntityId, uint skillId, float attackSpeed, float dir, uint endTick)
    {
        //내 pc가 아닐 경우 타겟 셋팅
        if(false == IsMyPc)
		{
            SetTarget(targetEntityId);
        }
        CombatComponent?.UseSkill(pos, targetEntityId, skillId, attackSpeed, dir, endTick);
    }
    public virtual void SkillCancel()
    {
        CombatComponent?.SkillCancel();
    }

    /// <summary> 마지막으로 나를 공격한 Entity 의 id를 셋팅한다. </summary>
    public void SetAttackerEntityId(uint entityId)
    {
        CancelInvoke(nameof(ResetAttackerEntityId));

        //2초뒤 리셋
        Invoke(nameof(ResetAttackerEntityId), 2f);

        AttackerEntityId = entityId;
    }

    /// <summary> 마지막으로 나를 공격한 Entity 의 id를 초기화한다. </summary>
    private void ResetAttackerEntityId()
    {
        CancelInvoke(nameof(ResetAttackerEntityId));
        AttackerEntityId = 0;
    }

    #endregion

    #region ===== :: AbilityAction :: =====

    public void DoAddOnAblityActionChanged(Action<System.Collections.Generic.Dictionary<uint, EntityAbilityAction>> onChanged)
    {
        if (AbilityActionComponent == null) return;
        AbilityActionComponent.OnAblityActionChanged += onChanged;
        RemoveAbilityAction(0);// 갱신용
    }

    public void DoRemoveOnAblityActionChanged(Action<System.Collections.Generic.Dictionary<uint, EntityAbilityAction>> onChanged)
    {
        if (AbilityActionComponent == null) return;
        AbilityActionComponent.OnAblityActionChanged -= onChanged;
    }

    public void AddAbilityAction(uint abilityActionId, ulong endTime, bool bNotConsume)
    {
        AbilityActionComponent?.AddAbilityAction(abilityActionId, endTime, bNotConsume);
    }

    public void RemoveAbilityAction(uint abilityActionId)
    {
        AbilityActionComponent?.RemoveAbilityAction(abilityActionId);
    }

    public EntityAbilityAction FindAbilityAction( uint abilityActionId )
    {
        return AbilityActionComponent?.FindAbilityAction( abilityActionId );
    }

    public EntityAbilityAction GetGodBuffAbilityAction()
    {
        var buff = FindAbilityAction(DBConfig.GodPower_AbilityActionID);

        if (null == buff)
            buff = FindAbilityAction(DBConfig.GodBless_AbilityActionID);            

        return buff;
    }

    #endregion

    #region ===== :: Stat :: =====

    public float CurrentHp => StatComponent?.CurrentHp ?? 0;
    public float MaxHp => StatComponent?.MaxHp ?? 0;

    public float CurrentMp => StatComponent?.CurrentMp ?? 0;
    public float MaxMp => StatComponent?.MaxMp ?? 0;

    public void DoAddEventHpUpdated(Action<float, float> action)
    {
        StatComponent?.DoAddEventHpUpdated(action);
    }

    public void DoRemoveEventHpUpdated(Action<float, float> action)
    {
        StatComponent?.DoRemoveEventHpUpdated(action);
    }

    public void DoRemoveAllEventHpUpdated()
    {
        StatComponent?.DoRemoveAllEventHpUpdated();
    }

    public void DoAddEventMpUpdated(Action<float, float> action)
    {
        StatComponent?.DoAddEventMpUpdated(action);
    }

    public void DoRemoveEventMpUpdated(Action<float, float> action)
    {
        StatComponent?.DoRemoveEventMpUpdated(action);
    }
    #endregion
    #region ===== :: SkillSystem :: ===== 
    /// <summary> 스킬 시스템 </summary>
    public SkillSystem SkillSystem { get; private set; }

    protected void SetSkillSystem()
    {
        //내 pc가 아니거나 신전 모드가 아니면 스킬 정보 셋팅하지 않음.
        if (false == IsMyPc && ZGameModeManager.Instance.CurrentGameModeType != E_GameModeType.Temple)
            return;

        if (null == SkillSystem)
        {
            SkillSystem = new SkillSystem(this);
        }

        SkillSystem.Initialize();
    }
    #endregion

    #region ===== :: AI 관련 처리 :: =====
    /// <summary> 해당 AI가 실행중인지 반환한다. </summary>
    public bool IsRunning { get { return AIComponent?.IsRunning ?? false; } }

    public E_PawnAIType CurrentAIType { get { return AIComponent?.CurrentAIType ?? E_PawnAIType.None; } }
    public bool IsCurrentAI(E_PawnAIType type)
    {
        if(AIComponent?.IsCurrentAI(type) ?? false)
        {
            return true;
        }

        return false;
    }

    /// <summary> 해당 AI를 실행한다. </summary>
    public virtual void StartAI(E_PawnAIType aiType)
    {
        AIComponent?.StartAI(aiType);
    }

    /// <summary> AI 정지 </summary>
    public void StopAI(E_PawnAIType aiType)
    {
        AIComponent?.StopAI(aiType);
    }
    public void DoAddEventStopQuestAI(Action<E_PawnAIType> action)
    {
        AIComponent?.DoAddEventStopAI(action);
    }

    public void DoRemoveEventStopQuestAI(Action<E_PawnAIType> action)
    {
        AIComponent?.DoRemoveEventStopAI(action);
    }

    public void DoAddEventStartQuestAI(Action<E_PawnAIType> action)
    {
        AIComponent?.DoAddEventStartAI(action);
    }

    public void DoRemoveEventStartQuestAI(Action<E_PawnAIType> action)
    {
        AIComponent?.DoRemoveEventStartAI(action);
    }

    #endregion

    #region ===== :: Virtual Function :: =====

    /// <summary>  pawn 사망시 알림 (attackerEntityId, 죽은대상)</summary>
    private Action<uint, ZPawn> mEventDie;

    /// <summary> 데미지 입었을 때 알림. (attackerEntityId, amount, critical, dot) </summary>
    private Action<uint, uint, bool, bool> mEventTakeDamage;

    public void DoAddEventDie(Action<uint, ZPawn> action)
    {
        DoRemoveEventDie(action);
        mEventDie += action;
    }

    public void DoRemoveEventDie(Action<uint, ZPawn> action)
    {
        mEventDie -= action;
    }

    public void DoAddEventTakeDamage(Action<uint, uint, bool, bool> action)
    {
        DoRemoveEventTakeDamage(action);
        mEventTakeDamage += action;
    }

    public void DoRemoveEventTakeDamage(Action<uint, uint, bool, bool> action)
    {
        mEventTakeDamage -= action;
    }
    
    /// <summary> 데미지를 입음 </summary>
    public void TakeDamage(uint attackerId, uint skillId, byte damageType, uint damage, float curHp)
    {
        //hit effect
        //skill table -> resource table -> common effect 우선순위
        uint hitEffectId = DBResource.Fx_Hit.EffectID;
        E_MissileType projectileType = E_MissileType.Not;

        ///현재 damagetype을 크리티컬로 사용함.
        bool bCritical = 0 < damageType;

        DBSkill.TryGet(skillId, out Skill_Table skillTable);

        ZPawnManager.Instance.TryGetEntity(attackerId, out ZPawn attacker);

        if (null != skillTable && 0 < skillTable.HitEffectID)
        {
            hitEffectId = skillTable.HitEffectID;
        }
        else if(null != attacker && 0 < attacker.ResourceTable.HitEffect)
        {
            hitEffectId = attacker.ResourceTable.HitEffect;
        }

        //[박윤성] 피격당했을시 옵션 처리
        //TakeDotDamage에도 같은게 있음
        if (null != attacker && attacker.EntityType == E_UnitType.Character)
        {
            if (IsMyPc)
            {
                if (ZGameModeManager.Instance.Table.StageType != E_StageType.Colosseum)
                {
                    if (ZGameOption.Instance.bAlramBeAttacked_PC)
                    {
                        UICommon.SetAlramBeAttacked();
                    }

                    if(ZGameOption.Instance.bAuto_Wakeup_ScreenSaver && UIManager.Instance.ScreenSaver != null)
					{
                        if (UIManager.Instance.Find(out UIFrameHUD hud)) hud.LastInputTime = TimeManager.NowSec;
                        UIManager.Instance.ScreenSaver.CloseScreenSaverPanel();
					}
                }
            }
        }

        projectileType = skillTable?.MissileType ?? E_MissileType.Not;

        if( null != skillTable && attacker != null && attacker.IsMyPc ) {
            //데미지 폰트 출력
            if( bCritical ) {
                if (ZGameOption.Instance.bShowDamageEffect)
                    SpawnDamageEffect( attacker, DBResource.Fx_Critical );
            }
            else if( damage == 0 ) {
                if (ZGameOption.Instance.bShowDodgeEffect)
                    SpawnDamageEffect(attacker, DBResource.Fx_Miss);
            }

            if (damage > 0)
            {
                //카메라 쉐이크
                CameraShake(attacker, skillTable, bCritical);

                //경직
                ApplySpasiticity(attacker, skillTable, bCritical);

                //히트 림 처리
                ApplyHitRimLight(attacker, skillTable, bCritical);
            }
        }

        if (projectileType == E_MissileType.Not)
        {
            if (ZGameOption.Instance.bShowDamageEffect)
            {
                //발동 이펙트 출력
                SpawnHitEffect(attacker, hitEffectId);
                if (null != skillTable)
                {
                    SpawnHitEffect(attacker, skillTable.ActuationEffectID);
                }
            }
        }
        else
        {
            //발사체는 기본 이펙트 위치로 출력
            SpawnEffect(hitEffectId, 0f, 1f);

            if (null != skillTable)
            {
                SpawnEffect(skillTable.ActuationEffectID);
            }
        }

        mEventTakeDamage?.Invoke(attackerId, damage, bCritical, false);

        //사망 처리
        if (0 >= curHp)
        {
            //따로 사망 패킷 없음.
            Die(attackerId);
        }
        else 
        {
            if(false == IsSkipHitMotion && false == IsMoving() && false == IsRiding && damage > 0)
            {
                SetAnimParameter(E_AnimParameter.Hit_001);
            }
        }
    }

    /// <summary> 데미지를 입음 </summary>
    public void TakeDotDamage(uint attackerId, uint abilityActionTid, uint damage, float curHp)
    {
        if(DBAbilityAction.TryGet(abilityActionTid, out var abilityActionTable))
        {
            SpawnEffect(abilityActionTable.DotEffectID, 0f, 1f);
        }

        ZPawnManager.Instance.TryGetEntity(attackerId, out ZPawn attacker);

        //[박윤성] 피격당했을시 옵션 처리
        //TakeDamage에도 같은게 있음.
        if (null != attacker && attacker.EntityType == E_UnitType.Character)
        {
            if (IsMyPc)
            {
                if (ZGameModeManager.Instance.Table.StageType != E_StageType.Colosseum)
                {
                    if (ZGameOption.Instance.bAlramBeAttacked_PC)
                    {
                        UICommon.SetAlramBeAttacked();
                    }

                    if (ZGameOption.Instance.bAuto_Wakeup_ScreenSaver && UIManager.Instance.ScreenSaver != null)
                    {
                        if (UIManager.Instance.Find(out UIFrameHUD hud)) hud.LastInputTime = TimeManager.NowSec;
                        UIManager.Instance.ScreenSaver.CloseScreenSaverPanel();
                    }
                }
            }
        }

        mEventTakeDamage?.Invoke(attackerId, damage, false, true);

        //사망 처리
        if (0 >= curHp)
        {
            //따로 사망 패킷 없음.
            Die(attackerId);
        }
        //Dot는 히트 모션 없음
    }

    /// <summary> 사망 처리 </summary>
    protected virtual void Die(uint attackerEntityId)
    {
        //사망시 서버에서 패킷 안옴. 강제로 적용
        AbilityNotify(E_AbilityType.FINAL_CURR_HP, 0f);

        ResetAnim();
        DieAnim(true, 0f);
        StopMove(transform.position);

        mEventDie?.Invoke( attackerEntityId, this );

		//디졸브 연출
        float animLength = AnimComponent?.GetAnimLength(E_AnimStateName.Die_001) ?? 2f;
        float dissolveStartDelay = animLength + 3f;
        Dissolve(true, 4f, dissolveStartDelay);

        PreClearEffect();
        Invoke(nameof(ClearEffect), animLength);

        //게임모드에 전달한다
        ZGameModeManager.Instance.GameMode.DieEntity( attackerEntityId, IsMyPc, this );

        //PawnManager에 사망했다고 알린다.
        ZPawnManager.Instance.DieEntity(this);
    }

    private void PreClearEffect()
    {
        EffectComponent?.PreClear();
    }

    private void ClearEffect()
    {
        EffectComponent?.Clear(true);
    }

    /// <summary> 해당 장비 장착여부 (현재 내 pc만 사용한다.) </summary>
    public virtual bool CheckEquippedWeapon(E_WeaponType weaponType)
    {
        return true;
    }

    /// <summary> 해당 클래스 인지 여부 (현재 Character만 사용한다.) </summary>
    public virtual bool CheckCharacterType(E_CharacterType characterType)
    {
        return true;
    }
    #endregion

    #region ===== :: 이펙트 :: =====
    /// <summary> 이펙트 출력 </summary>
    public void SpawnEffect(Effect_Table table, float duration = 0f, float speed = 1f, Action<ZEffectComponent> onFinish = null)
    {
        EffectComponent?.SpawnEffect(table, duration, speed, onFinish);
    }

    public void SpawnEffect(uint effectTid, float duration = 0f, float speed = 1f, Action<ZEffectComponent> onFinish = null)
    {
        EffectComponent?.SpawnEffect(effectTid, duration, speed, onFinish);
    }

    /// <summary> 히트 이펙트 연출시 사용 </summary>
    public void SpawnHitEffect(ZPawn attacker, uint effectTid, Action<ZEffectComponent> onFinish = null)
    {
        EffectComponent?.SpawnHitEffect(attacker, effectTid, onFinish);
    }

    private void SpawnDamageEffect( ZPawn attacker, Effect_Table table )
	{
        EffectComponent?.SpawnDamageEffect( attacker, table );
    }

    /// <summary> 카메라 쉐이크 처리 : 평타1,2,3, 크리, 스킬등 다섯가지 설정값으로 처리 (기획의도) </summary>
    private void CameraShake( ZPawn attacker, Skill_Table skillTable, bool critical )
    {
        float amplitude = 0.0f;

        if( skillTable.SkillType == E_SkillType.ActiveSkill ) {
            amplitude = DBConfig.Camshake_Strength5;
        }
        else if( skillTable.SkillType == E_SkillType.Normal ) {
            if( critical ) {
                amplitude = DBConfig.Camshake_Strength4;
            }
            else {
                switch( skillTable.SkillSort ) {
                    case 1: amplitude = DBConfig.Camshake_Strength1; break;
                    case 2: amplitude = DBConfig.Camshake_Strength2; break;
                    case 3: amplitude = DBConfig.Camshake_Strength3; break;
                }
            }
        }

        if( amplitude > 0 ) {
            CameraManager.Instance.DoShake( attacker.transform.position, attacker.transform.forward,
                amplitude, 100, DBConfig.Camshake_Time );
        }
    }

    /// <summary> 히트시 경직 : 평타1,2,3 크리1,2,3 마다 다르게 적용하고 원거리공격자와 스킬에는 적용안함(기획의도) </summary>
    private void ApplySpasiticity( ZPawn attacker, Skill_Table skillTable, bool critical )
    {
        bool isApply = false;

        if( skillTable.SkillType == E_SkillType.Normal ) {
            if( critical ) {
                switch( skillTable.SkillSort ) {
                    case 1: isApply = DBConfig.Spasiticity_Actuation_Type1 == 1; break;
                    case 2: isApply = DBConfig.Spasiticity_Actuation_Type2 == 1; break;
                    case 3: isApply = DBConfig.Spasiticity_Actuation_Type3 == 1; break;
                }
            }
            else {
                switch( skillTable.SkillSort ) {
                    case 1: isApply = DBConfig.Spasiticity_Actuation_Type1 == 1; break;
                    case 2: isApply = DBConfig.Spasiticity_Actuation_Type2 == 1; break;
                    case 3: isApply = DBConfig.Spasiticity_Actuation_Type3 == 1; break;
                }
            }
        }

        if( isApply ) {
            if( DBConfig.PC_Spasiticity_Time > 0 ) {
                // 근접만 경직 적용
                if( attacker.CheckCharacterType( E_CharacterType.Knight ) || attacker.CheckCharacterType( E_CharacterType.Assassin ) ) {
                    attacker.Spasiticity( DBConfig.PC_Spasiticity_Time, DBConfig.Spasiticity_SwingTime );
                }
            }

            // 히트 대상은 전부 적용
            if( DBConfig.Spasiticity_Time > 0 ) {
                this.Spasiticity( DBConfig.Spasiticity_Time, 0 );
            }
        }
    }

    /// <summary> 크리와 3타에만 강도 다르게 적용(기획의도) </summary>
    private void ApplyHitRimLight( ZPawn attacker, Skill_Table skillTable, bool critical )
    {
        if( ModelComponent == null ) {
            return;
        }

        Color hitColor;
        if( DBConfig.Color_Conversion_State == 0 ) {
            hitColor = ParadoxNotion.ColorUtils.HexToColor( DBConfig.Color_Conversion_DefaultColor );
        }
        else {
            switch( attacker.UnitAttributeType ) {
                case E_UnitAttributeType.Fire: hitColor = ParadoxNotion.ColorUtils.HexToColor( DBConfig.Color_Conversion_Fire ); break;
                case E_UnitAttributeType.Water: hitColor = ParadoxNotion.ColorUtils.HexToColor( DBConfig.Color_Conversion_Water ); break;
                case E_UnitAttributeType.Electric: hitColor = ParadoxNotion.ColorUtils.HexToColor( DBConfig.Color_Conversion_Eletricity ); break;
                case E_UnitAttributeType.Light: hitColor = ParadoxNotion.ColorUtils.HexToColor( DBConfig.Color_Conversion_Light ); break;
                case E_UnitAttributeType.Dark: hitColor = ParadoxNotion.ColorUtils.HexToColor( DBConfig.Color_Conversion_Dark ); break;
                default: hitColor = ParadoxNotion.ColorUtils.HexToColor( DBConfig.Color_Conversion_DefaultColor ); break;
            }
        }

        float hitStrength = DBConfig.Color_Conversion_Strength1;
        if( critical || ( skillTable.SkillType == E_SkillType.Normal && skillTable.SkillSort == 3 ) ) {
            hitStrength = DBConfig.Color_Conversion_Strength2;
        }

        ModelComponent.HitRimLight( hitColor, hitStrength, DBConfig.Color_Conversion_Time, false );
    }

    #endregion

    #region ===== :: Condition Control :: =====
    /// <summary>  pawn 상태가 변경되었을 경우 알림 </summary>
    private Action<E_CustomConditionControl, bool> mEventUpdateCustomConditionControl;

    /// <summary> 메즈 상태 변경시 알림 </summary>
    private Action<E_ConditionControl> mEventChangeConditionControl;

    public void DoAddEventUpdateCustomConditionControl(Action<E_CustomConditionControl, bool> action)
    {
        DoRemoveEventUpdateCustomConditionControl(action);
        mEventUpdateCustomConditionControl += action;
    }

    public void DoRemoveEventUpdateCustomConditionControl(Action<E_CustomConditionControl, bool> action)
    {
        mEventUpdateCustomConditionControl -= action;
    }

    public void DoAddEventChangeConditionControl(Action<E_ConditionControl> action)
    {
        DoRemoveEventChangeConditionControl(action);
        mEventChangeConditionControl += action;
    }

    public void DoRemoveEventChangeConditionControl(Action<E_ConditionControl> action)
    {
        mEventChangeConditionControl -= action;
    }

    public virtual void ChangeMezState(uint mezState)
    {
        //현재 상태와 변경된 상태 체크.
        E_ConditionControl changeFlag = (E_ConditionControl)(mezState ^ (uint)MezState);

        PawnData.MezState = (E_ConditionControl)mezState;

        mEventChangeConditionControl?.Invoke(changeFlag);
    }

    /// <summary> 서버에 의해 관리되는 ConditionControl </summary>
    public virtual bool IsMezState(E_ConditionControl type)
    {
        return MezState.HasFlag(type);
    }

    /// <summary> 클라에 의해 관리되는 ConditionControl </summary>
    public bool IsCustomConditionControl(E_CustomConditionControl control)
    {
        return ConditionControl.HasFlag(control);
    }

    public void SetCustomConditionControl(E_CustomConditionControl control, bool bApply)
    {
        if(bApply)
        {
            ConditionControl |= control;
        }
        else
        {
            ConditionControl &= ~control;
        }
        
        mEventUpdateCustomConditionControl?.Invoke(control, bApply);
    }
    #endregion

    #region ===== :: 성향 :: =====
    private Action<int> mEventChangeTendency;

    public void DoAddEventChangeTendency(Action<int> action)
    {
        DoRemoveEventChangeTendency(action);
        mEventChangeTendency += action;
    }

    public void DoRemoveEventChangeTendency(Action<int> action)
    {
        mEventChangeTendency -= action;
    }

    /// <summary> 성향 변경시 처리 </summary>
    public void SetTendency(int value)
    {
        PawnData.Tendency = value;
        mEventChangeTendency?.Invoke(value);
    }
    #endregion
}
