using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.TeleTrust;
using FSM;
using GameDB;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using ZNet.Data;


/// <summary> Pc </summary>
public class ZPawnMyPc : ZPawnCharacter
{
    public bool IsGathering = false;

    private Action mEventSetTargetList;
    private Action mEventClickSearchTarget;

    /// <summary> 스킬 쿨타임 변경시 알림 </summary>
    private Action<uint, float> mEventChangeSkillCoolTime;

    private EntityComponentControllerBase mController = null;
    private EntityComponentAutoBattle mBattleSystem = null;
    private EntityComponentAutoSkill mAutoSkillController = null;

    private List<ZPawn> mTargetSearchList = new List<ZPawn>();
    public List<ZPawn> TargetSearchList { get { return mTargetSearchList; } }

    /// <summary> MyPc용 상태 머신 </summary>
    public FSM<E_GameEvent, E_EntityState, ZPawnMyPc> FSM { get; private set; }

    public E_EntityState CurrentState { get { return FSM?.Current_State ?? E_EntityState.Empty; } }

    public E_EntityState PreviousState { get; private set; } = E_EntityState.Empty;

    public override bool IsMyPc { get { return true; } }

    /// <summary> 내 캐릭터 테이블 데이터 </summary>
    private Character_Table mMyTable;

    /// <summary> 내 캐릭터 테이블 데이터 </summary>
    public Character_Table MyTable { get { return mMyTable; } }

    private EntityBase FirstTarget { get { return mTarget; } set { mTarget = value; } }

    protected uint FirstTargetId { get { return mTarget?.EntityId ?? 0; } }

    public EntityBase SecondTarget = null;
    public uint SecondTargetId { get { return SecondTarget?.EntityId ?? 0; } }

    /// <summary> my pc 용 이동 막기 </summary>
    public bool IsBlockMoveMyPc { get; set; }

    /// <summary> my pc 용 변신 막기 </summary>
    public bool IsNotChangeMyPc { get; set; }

    /// <summary> 공격하고 있는 상태인지 여부 </summary>
    public bool IsAttacking
    {
        get { return CurrentState == E_EntityState.Attack || CurrentState == E_EntityState.Skill; }
    }

    /// <summary> 탑승 가능 여부 </summary>
    public bool IsPossibleRide { get { return MoveComponent?.IsPossibleRide ?? false; } }

    /// <summary> 자동 전투 </summary>
    public bool IsAutoPlay { 
        get
        {
            if (mBattleSystem == null) {
                return false;
            }

            return mBattleSystem.isAutoPlay;
        }
        set
        {
            if (mBattleSystem != null) {
                mBattleSystem.isAutoPlay = value;
            }
        }
    }

    /// <summary> 현재 이동 타입 가져오기 </summary>
    public MOVEMENT_TYPE GetMovement<MOVEMENT_TYPE>() where MOVEMENT_TYPE : EntityComponentMovementBase
    {
        return MoveComponent as MOVEMENT_TYPE;
    }

    protected override void OnInitializeImpl()
    {
        base.OnInitializeImpl();

        //무게 셋팅
        Weight = DBConfig.Temple_PCWeight;

        //트리거 충돌을 위해 추가
        var rBody = gameObject.GetOrAddComponent<Rigidbody>();
        rBody.freezeRotation = true;
        rBody.useGravity = false;
        rBody.isKinematic = ZGameModeManager.Instance.CurrentGameModeType != E_GameModeType.Temple;
        rBody.interpolation = RigidbodyInterpolation.Interpolate;
    }

    protected override void OnPostInitializeImpl()
    {
        base.OnPostInitializeImpl();

#if UNITY_EDITOR
        gameObject.name = $"{gameObject.name}_MyPc";
#endif

        DBCharacter.TryGet(TableId, out mMyTable);

        if (ZGameModeManager.Instance.Table != null && 
            ZGameModeManager.Instance.Table.StageType == E_StageType.GodLand) {

            //특정모드등에서 내pc가 원격조정 되야할때
            ChangeController<EntityComponentController_Empty>();
            IsRemoteMove = true;
        }
        else {
            ChangeController<EntityComponentController>();
            IsRemoteMove = false;

            mBattleSystem = GetOrAddMyPcComponent<EntityComponentAutoBattle>();
        }
        mAutoSkillController = GetOrAddMyPcComponent<EntityComponentAutoSkill>();

#if UNITY_EDITOR
        //테스트 스킬 사용
        GetOrAddComponent<EntityComponentTestUseSkill>();
#endif
        //상태머신 셋팅
        SetFSM();

        AddEvent();

        StartDefaultAI();
    }

    protected override EntityComponentMovementBase OnSetMovementComponent()
    {
        if (null != ZGameModeManager.Instance.Table)
        {
            switch (ZGameModeManager.Instance.Table.StageType)
            {
                case E_StageType.Temple:
                    return GetOrAddComponent<EntityComponentMovement_Temple>();
                case E_StageType.GodLand:
                    return GetOrAddComponent<EntityComponentMovement_NavMesh>();
                default:
                    return GetOrAddComponent<EntityComponentMovement_NavMeshForPlayer>();
            }
        }

        return GetOrAddComponent<EntityComponentMovement_NavMeshForPlayer>();
    }

    protected override EntityComponentAI OnSetAIComponent()
    {
        return GetOrAddPawnComponent<EntityComponentAIForPlayer>();
    }

    protected override EntityComponentStatBase OnSetStatComponent()
    {
        return GetOrAddComponent<EntityComponentStat_Player>();
    }

    protected COMPONENT_TYPE GetOrAddMyPcComponent<COMPONENT_TYPE>() where COMPONENT_TYPE : EntityComponentBase<ZPawnMyPc>
    {
        COMPONENT_TYPE comp = gameObject.GetOrAddComponent<COMPONENT_TYPE>();
        comp.InitializeComponent(this);
        return comp;
    }

    protected override void OnDestroyImpl()
    {
        RemoveEvent();
        base.OnDestroyImpl();
    }

    public void ChangeController<CONTROLLER_TYPE>() where CONTROLLER_TYPE : EntityComponentControllerBase
    {
        if(null != mController)
        {
            GameObject.Destroy(mController);
        }

        mController = GetOrAddPawnComponent<CONTROLLER_TYPE>();
    }

    private void AddEvent()
    {
        ZPartyManager.Instance.DoAddEventChangePartyTargetEntityId(HandlePartyTarget);

        ZPawnManager.Instance.DoAddEventCreateEntity(HandleCreatePawn);
        ZPawnManager.Instance.DoAddEventDieEntity(HandleDieEntity);
        
        StatComponent?.DoAddEventWeightUpdated(CheckAutoReturnByWeight);
    }

    private void RemoveEvent()
    {
        if(ZPartyManager.hasInstance )
            ZPartyManager.Instance.DoRemoveEventChangePartyTargetEntityId(HandlePartyTarget);

        if (ZPawnManager.hasInstance)
        {
            ZPawnManager.Instance.DoRemoveEventCreateEntity(HandleCreatePawn);
            ZPawnManager.Instance.DoRemoveEventDieEntity(HandleDieEntity);
        }

        StatComponent?.DoAddEventWeightUpdated(CheckAutoReturnByWeight);
    }

    #region FSM
    private void SetFSM()
    {
        FSM = new FSM<E_GameEvent, E_EntityState, ZPawnMyPc>(this);

        //State 등록
        FSM.AddState(E_EntityState.Empty, gameObject.GetOrAddComponent<EntityStateEmpty>());
        FSM.AddState(E_EntityState.Attack, gameObject.GetOrAddComponent<EntityStateAttack>());
        FSM.AddState(E_EntityState.Skill, gameObject.GetOrAddComponent<EntityStateSkill>());
        FSM.AddState(E_EntityState.Gathering, gameObject.GetOrAddComponent<EntityStateGathering>());

        FSM.Enable(E_EntityState.Empty);
        PreviousState = E_EntityState.Empty;
    }

    public void ChangeState(E_EntityState state, params object[] args)
    {
        var preState = CurrentState;
        if(true == FSM?.ChangeState(state, false, args))
        {
            PreviousState = preState;
        }
    }

    #endregion

    #region ===== :: Combat :: =====

    public void StopCombat()
    {
        ChangeState(E_EntityState.Empty);
        //TODO :: 추후 수정하자
        if (mBattleSystem?.isAutoPlay ?? false)
        {
            var actionUI = UIManager.Instance.Find<UISubHUDCharacterAction>();

            if (null != actionUI)
                actionUI.SelectAuto();
        }
    }
    public void UseNormalAttack(bool isSecondTarget = false)
    {
        if (IsAttacking && !isSecondTarget)
            return;

        if (true == IsMezState(E_ConditionControl.NotAttack))
            return;

        if(true == IsCustomConditionControl(E_CustomConditionControl.Temple_Control))
            return;
        
        ChangeState(E_EntityState.Attack);
    }

    public E_SkillSystemError UseSkillBySkillId(uint skillId, bool isAuto = false)
    {
        if (true == IsCustomConditionControl(E_CustomConditionControl.Temple_Control))
            return E_SkillSystemError.TempleControlState;

        E_SkillSystemError error = SkillSystem.CheckUseSkill(skillId, isAuto);
        
        switch (error)
        {
            case E_SkillSystemError.None:
                {
                    ChangeState(E_EntityState.Skill, skillId);
                }
                break;
            default:
                {
                    if (!IsAutoPlay)
                        ShowSkillErrorMessage(error);
                }
                break;
        }

        return error;
    }

    public void UseSkillForGimmick(EntityBase target, uint skillId, float attackSpeed, uint endTime)
    {
        CombatComponent?.UseSkillForGimmick(target, skillId, attackSpeed, endTime);
    }

    /// <summary> 스킬 쿨타임 변경 </summary>
    public void SetSkillCoolTime(uint skillId, float remainTime)
    {
        SkillSystem.SetCoolTime(skillId, remainTime);

        mEventChangeSkillCoolTime?.Invoke(skillId, remainTime);
    }

    /// <summary> 스킬 쿨이 끝나는 서버 시각 (ms) </summary>
    public ulong EndSkillCoolTimeMs(uint skillId)
    {
        SkillInfo skill = SkillSystem.GetSkillInfoById(skillId);
        return skill?.EndCoolTimeMs ?? 0ul;
    }

    /// <summary> 스킬 쿨 남은 시간 </summary>
    public float RemainSkillCoolTime(uint skillId)
    {
        SkillInfo skill = SkillSystem.GetSkillInfoById(skillId);
        return skill?.RemainCoolTime ?? 0f;
    }

    /// <summary> 스킬 쿨 체크 </summary>
    public bool CheckSkillCoolTime(uint skillId)
    {
        SkillInfo skill = SkillSystem.GetSkillInfoById(skillId);
        return skill?.CheckCoolTime() ?? true;
    }

    /// <summary> 스킬 쿨 변경시 알림 </summary>
    public void DoAddEventChangeSkillCoolTime(Action<uint, float> action)
    {
        DoRemoveEventChangeSkillCoolTime(action);
        mEventChangeSkillCoolTime += action;
    }

    /// <summary> 스킬 쿨 변경시 알림 제거 </summary>
    public void DoRemoveEventChangeSkillCoolTime(Action<uint, float> action)
    {
        mEventChangeSkillCoolTime -= action;
    }

    /// <summary> 스킬 에러 메시지 출력 </summary>
    public void ShowSkillErrorMessage(E_SkillSystemError error)
    {
        switch (error)
        {
            case E_SkillSystemError.InvalidWeaponType:
                {
                    var data = Me.CurCharData.GetEquippedItem(E_EquipSlotType.Weapon);
                    if (null == data)
                    {
                        //무기를 착용해 주세요.
                        UICommon.SetNoticeMessage(DBLocale.GetText("Unmount_Weapon_Message"), Color.red, 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);
                    }
                    else
                    {
                        //직업에 맞는 무기를 착용해 주세요.
                        UICommon.SetNoticeMessage(DBLocale.GetText("Job_Match_Weapon_Message"), Color.red, 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);

                    }

                    IsAutoPlay = false;
                }
                break;
            case E_SkillSystemError.InvalidCharacterType:
                {
                    //직업에 맞지 않는 스킬은 사용할 수 없습니다.
                    UICommon.SetNoticeMessage(DBLocale.GetText("Job_Match_Skill_Message"), Color.red, 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);
                }
                break;
			case E_SkillSystemError.NotEnoughMp:
				{
					UICommon.SetNoticeMessage(DBLocale.GetText("Not_MP_Message"), Color.red, 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);
				}
				break;
			case E_SkillSystemError.CoolTime:
				{
					UICommon.SetNoticeMessage(DBLocale.GetText("Skill_Cooltime_Message"), Color.red, 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);
				}
				break;
			case E_SkillSystemError.NotExistTarget:
				{
					UICommon.SetNoticeMessage(DBLocale.GetText("Skill_Target_Nothing"), Color.red, 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);
				}
				break;
            case E_SkillSystemError.CantUseInTowen: {
                    UICommon.SetNoticeMessage(DBLocale.GetText("Town_Skill_Unavailable"), Color.red, 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);
                }
                break;

            default:
                {
                    //임시 에러 표시
                    UICommon.SetNoticeMessage(error.ToString(), Color.red, 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);
                }
                break;
        }
    }

    #endregion

    #region ===== :: MoveComponent :: =====
    /// <summary> 이동 가능 여부 </summary>
    public override bool IsBlockMove()
    {
        return IsSkillAction || IsDead || IsMezState(E_ConditionControl.NotMove) || IsBlockMoveMyPc;
    }
    #endregion

    #region ===== :: 강림 :: =====   
    private Action<uint> mEventUpdateChange;

    public void DoAddEventUpdateChange(Action<uint> action)
    {
        mEventUpdateChange += action;
    }

    public void RemoveEventUpdateChange(Action<uint> action)
    {
        mEventUpdateChange -= action;
    }

    public override bool SetChangeChange(uint changeTid)
    {
        //유적에서 셋팅된 고정변신 처리
        if(ZGameModeManager.Instance.CurrentGameModeType == E_GameModeType.Temple)
		{
            if(0 < ZPawnManager.Instance.TempleFixedChangeTid)
                changeTid = ZPawnManager.Instance.TempleFixedChangeTid;
		}

        bool ret = base.SetChangeChange(changeTid);
        //if (ret)
        //{
        //    SetSkillSystem();
        //}

        mEventUpdateChange?.Invoke(changeTid);

        return ret;
    }
    #endregion

    #region ===== :: 펫 :: =====
    private Action<uint> mEventUpdatePet;

    public void DoAddEventUpdatePet(Action<uint> action)
    {
        mEventUpdatePet += action;
    }

    public void RemoveEventUpdatePet(Action<uint> action)
    {
        mEventUpdatePet -= action;
    }

    public override void SetChangePet(uint petTid)
    {
        base.SetChangePet(petTid);

        mEventUpdatePet?.Invoke(petTid);
    }
    #endregion

    #region ===== :: 탈 것 :: =====
    private Action<uint> mEventUpdateVehicle;

    public void DoAddEventUpdateVehicle(Action<uint> action)
    {
        mEventUpdateVehicle += action;
    }

    public void RemoveEventUpdateVehicle(Action<uint> action)
    {
        mEventUpdateVehicle -= action;
    }

    public override void SetChangeVehicle(uint vehicleTid)
    {
        base.SetChangeVehicle(vehicleTid);
        mEventUpdateVehicle?.Invoke(vehicleTid);
    }
    #endregion
      
    #region ===== :: Stat :: =====
    public void DoAddEventWeightUpdated(Action action)
    {
        StatComponent?.DoAddEventWeightUpdated(action);
    }

    public void DoRemoveEventWeightUpdated(Action action)
    {
        StatComponent?.DoRemoveEventWeightUpdated(action);
    }
    #endregion

    /// <summary> 사망 처리 </summary>
    protected override void Die(uint attackerEntityId)
    {
        base.Die(attackerEntityId);

        if (UIManager.Instance.Find(out UISubHUDMenu _menu)) _menu.CloseHamburgerMenu();
        if (UIManager.Instance.Find(out UIFrameHUD _hud)) _hud.HideAllContentFrame();

        // 시련의성역, 주신의탑에서 사망 시 던전 실패 팝업으로 대체
        if(ZGameModeManager.Instance.CurrentGameModeType == E_GameModeType.TrialSanctuary || ZGameModeManager.Instance.CurrentGameModeType == E_GameModeType.Infinity)
		{
            UIManager.Instance.Open<UIFrameDungeonFailPopup>((assetName, frame) =>
            {
                frame.Init(ZGameModeManager.Instance.CurrentGameModeType);
            });

            return;
		}

        // 콜로세움/성지는 부활창 띄우지 말자
        if( ZGameModeManager.Instance.CurrentGameModeType != E_GameModeType.Colosseum &&
            ZGameModeManager.Instance.CurrentGameModeType != E_GameModeType.GodLand) {
            UIManager.Instance.Open<UIFrameDeathInfo>( ( assetName, frame ) => {
                frame.Init( attackerEntityId );
            } );
        }
    }

    public E_EntityAttackTargetType GetAttackTargetType(EntityBase entity)
    {
        //if (entity == mTarget) { return E_EntityAttackTargetType.MainTarget; }
        //else if (entity == SecondTarget) { return E_EntityAttackTargetType.SecondTarget; }
        //else { return E_EntityAttackTargetType.None; }

        if(entity == FirstTarget)
        {
            return E_EntityAttackTargetType.MainTarget;
        }
        else if(entity == SecondTarget)
        {
            return E_EntityAttackTargetType.SecondTarget;
        }
        else
        {
            return E_EntityAttackTargetType.None;
        }
    }

    public bool isSecondTargetAttack()
    {
        if(FirstTarget == null || SecondTarget == null)
        {
            return false;
        }

        if(FirstTarget.IsDead && SecondTarget != null)
        {
            return true;
        }

        return false;

        //if (mTarget == null || SecondTarget == null) return false;
        // if ( mTarget.IsDead == true && SecondTarget != null)
        // {
        //    return true;
        // }         
        //return false;
    }

    /// <summary> 내 캐릭터가 상태이상에 걸렸을 경우 처리 </summary>    
    public override void ChangeMezState(uint mezState)
    {
        //현재 상태와 변경된 상태 체크.
        E_ConditionControl changeFlag = (E_ConditionControl)(mezState ^ (uint)MezState);

        base.ChangeMezState(mezState);

        //변경된 상태만 처리
        if (changeFlag.HasFlag(E_ConditionControl.NotMove))
        {
            if (IsMezState(E_ConditionControl.NotMove))
            {
                //이동 중지.
                StopMove(Position);
            }
            else
            {
                //이동 중지 해제.
            }
        }
        if (changeFlag.HasFlag(E_ConditionControl.Vision))
        {
            if (IsMezState(E_ConditionControl.Vision))
            {
                //시야 감소                
            }
            else
            {
                //시야 감소 해제.
            }
        }
        if (changeFlag.HasFlag(E_ConditionControl.SpecialVision))
        {
            if (IsMezState(E_ConditionControl.SpecialVision))
            {
                //스페셜 시야 감소                
            }
            else
            {
                //스페셜 시야 감소 해제.
            }
        }
    }

    public void MoveToInputPosition(Vector3 destPosition, float speed)
    {
        //공격중일 경우에는 이동 예약
        if (CurrentState == E_EntityState.Attack || CurrentState == E_EntityState.Skill)
        {
            (FSM.Current as EntityStateSkillBase).SetInputPosition(destPosition, speed);
        }
        else
        {
            //채집중일 경우에는 걍 상태변경
            if(CurrentState == E_EntityState.Gathering)// || IsGathering)
            {
                ChangeState(E_EntityState.Empty);
            }

            MoveTo(destPosition, speed, true);
        }
    }

    #region ===== :: 애니메이션  :: =====
    
    
    /// <summary> 사당용 애니메이터 컨트롤러로 변경한다. </summary>
    public void ChangeAnimatorForTemple()
    {
        string controllerName = (AnimComponent as EntityComponentAnimation_Animator).Controller.name;

        if (controllerName.Contains("_Temple"))
            return;

        var handle = Addressables.LoadAssetAsync<AnimatorOverrideController>($"{controllerName}_Temple");

        handle.Completed += (data) =>
        {
            ChangeAnimController(data.Result);
        };
    }
    #endregion
    /// <summary> 모델 로드 완료시 처리 </summary>
    protected override void OnLoadedModelImpl()
    {
		base.OnLoadedModelImpl();

        gameObject.GetOrAddComponent<GrassBending.BendGrassWhenEnabled>();

        //사당일 경우 애니메이터 변경
        if (DBStage.TryGet(ZGameModeManager.Instance.StageTid, out var table))
        {
            switch (table.StageType)
            {
                case E_StageType.Temple:
                    {
                        ChangeAnimatorForTemple();
                    }
                    break;
            }
        }
    }

    public EntityComponentAutoSkill GetAutoSkillController()
    {
        return mAutoSkillController;
    }

    /// <summary> 해당 장비를 장착중인지 여부 </summary>
    public override bool CheckEquippedWeapon(E_WeaponType weaponType)
    {
        //유적 테스트라면 패스
        if (ZGameManager.Instance.StarterData is ZGimmickStarterData)
            return true;

        var data = Me.CurCharData.GetEquippedItem(E_EquipSlotType.Weapon);

        if(null != data && 0 < data.ItemTid)
        {            
            if(weaponType == E_WeaponType.None)
            {
                return true;
            }

            if(DBItem.GetItem(data.ItemTid, out var item))
            {                
                return DBItem.GetItemWeaponType(item.ItemSubType) == weaponType;
            }

            return false;
        }

        return false;
        //return null != data && data.ItemTid > 0;
    }

    #region ===== :: 채집 :: =====
    private Action<float, ZObject> mEventGatherInterface;
    private Action mEventGatherEnd;
    public void DoAddEventGatherInterface(Action<float, ZObject> action)
    {
        DoRemoveEventGatherInterFace(action);
        mEventGatherInterface += action;
    }

    public void DoRemoveEventGatherInterFace(Action<float, ZObject> action)
    {
        mEventGatherInterface -= action;
    }

    public void DoAddEventGatherEnd(Action action)
    {
        DoRemoveEventGatherEnd(action);
        mEventGatherEnd += action;
    }

    public void DoRemoveEventGatherEnd(Action action)
    {
        mEventGatherEnd -= action;
    }

    public void ObjectGathering(EntityBase entity)
    {
        var currentTarget = GetTarget();
        if (IsGathering && null != currentTarget && currentTarget.EntityId == entity.EntityId)
        {
            return;
        }

        SetTarget(entity);
        ChangeState(E_EntityState.Empty);
        ChangeState(E_EntityState.Gathering);
    }

    public void ObjectGatheringEnd()
    {
        IsGathering = false;
        mEventGatherEnd?.Invoke();
    }

    public void ShowGatherInterface(float _duration, ZObject gatherObj)
    {
        mEventGatherInterface?.Invoke(_duration, gatherObj);
    }

    #endregion
        
    #region ===== :: 타겟 관련 :: =====
    /// <summary> 타겟 변경시 (이전 타겟, 현재 타겟) </summary>
    private Action<uint, uint> mEventChangeTarget;
    /// <summary> 타겟 선택 이펙트 </summary>
    private ZEffectComponent mSelectedTargetEffect = null;
    /// <summary> 파티 타겟용 이펙트 </summary>
    private ZEffectComponent mPartyTargetEffect = null;
    public void DoAddEventChangeTarget(Action<uint, uint> action)
    {
        DoRemoveEventChangeTarget(action);
        mEventChangeTarget += action;
    }

    public void DoRemoveEventChangeTarget(Action<uint, uint> action)
    {
        mEventChangeTarget -= action;
    }

    /// <summary> 타겟을 셋팅함 </summary>
    public override void SetTarget(uint targetEntityId)
    {
        if (ZPawnManager.Instance.TryGetEntity(targetEntityId, out ZPawn target))
        {
            // 공격중이 아닐때 몬스터 클릭
            if (IsAttacking == false)
            {
                SetTarget(target);
            }
            else
            {
                // mTarget있다는 말이고
                // if (mTarget == target)
					//return;

                SecondTarget = target;
            }
        }

        //mEventChangeTarget?.Invoke(prevSelectedTargetId, SelectedTargetId);
    }

    /// <summary> 타겟을 셋팅함 </summary>
    public override void SetTarget(EntityBase target)
    {
        uint preTargetId = FirstTargetId;
        uint preSencondTargetId = SecondTargetId;
        bool removeOldTarget = false;

        if (null != FirstTarget)
        {
            if (FirstTarget.EntityType == E_UnitType.Gimmick) {
                FirstTarget.To<ZGimmick>().SetTargeted(false);
            }
            else if(FirstTarget.EntityType == E_UnitType.NPC) {
                removeOldTarget = true;
            }
        }

        if (target == null ||
            target.EntityType == E_UnitType.Object ||
            target.EntityType == E_UnitType.Gimmick) {
            removeOldTarget = true;
        }

        if (removeOldTarget)
        {
            FirstTarget = null;
            SecondTarget = null;
            mEventChangeTarget?.Invoke(preTargetId, FirstTargetId);
            mEventChangeTarget?.Invoke(preSencondTargetId, SecondTargetId);

            if(null != target)
            {
                FirstTarget = target;
            }
        }
        else
        {
            if (!IsAttacking && !IsMoving())
            {
                FirstTarget = target;
                mEventChangeTarget?.Invoke(preTargetId, target.EntityId);
            }
            else
            {
                if (FirstTarget == null)
                {
                    FirstTarget = target;
                    mEventChangeTarget?.Invoke(preTargetId, FirstTargetId);
                }
                else
                {
                    if (FirstTarget == target)
                    {
                        return;
                    }

                    SecondTarget = target;
                    mEventChangeTarget?.Invoke(preSencondTargetId, SecondTargetId);
                }
            }
        }
        
        CheckTargetSelectFX();
    }

    public override EntityBase GetTarget()
    {
        if(null != FirstTarget && FirstTarget.IsDead)
        {
            if (null != SecondTarget)
            {
                return SecondTarget;
            }

            FirstTarget = null;
        }

        return FirstTarget;

        //if (null != mTarget && mTarget.IsDead)
        //{
        //    if (null != SecondTarget)
        //    {
        //        return SecondTarget;
        //    }

        //    mTarget = null;
        //}

        //return mTarget;
    }

    public void DoTargetSwitch()
    {
        var mainTarget = FirstTarget;

        FirstTarget = SecondTarget;        
        SecondTarget = mainTarget;

        CheckTargetSelectFX();

        if (IsMoving())
        {
            StopMove();
            ChangeState(E_EntityState.Empty);
        }
        else
        {
            ChangeState(E_EntityState.Empty);
            UseNormalAttack(true);
        }
    }

	/// <summary> 타겟에게 타겟팅 이펙트를 따라가도록 설정 </summary>
	public void CheckTargetSelectFX()
	{
		//if (null == mTarget || mTarget.IsDead)
        if(null == FirstTarget || FirstTarget.IsDead)
		{
			if (null != mSelectedTargetEffect)
			{
				mSelectedTargetEffect.Despawn();
				mSelectedTargetEffect = null;
			}

			// NameTag까지 한번에 지우기위해...
			//mEventChangeTarget?.Invoke(mTarget?.EntityId ?? 0, 0);
            
			return;
		}

        var effectTable = DBResource.Fx_Target;
        //var position = mTarget.transform.position;
        var position = FirstTarget.transform.position;

        //TODO :: 기믹은 예외처리        
        if (FirstTarget.EntityType == E_UnitType.Gimmick)
        {
            //effectTable = DBResource.Fx_GimmickTarget;            
            //position = FirstTarget.To<ZGimmick>().TempTargetEffectOffset + FirstTarget.transform.position;
            FirstTarget.To<ZGimmick>().SetTargeted(true);

            return;
        }

        //ZEffectManager.Instance.SpawnEffect(effectTable, mTarget.transform, position, Quaternion.identity, -1f, 1f, (comp) =>
        ZEffectManager.Instance.SpawnEffect(effectTable, FirstTarget.transform, position, Quaternion.identity, -1f, 1f, (comp)
            =>
        {
            if (null != mSelectedTargetEffect)
            {
                mSelectedTargetEffect.Despawn();
            }

            //if (null != mTarget)
            if (null != FirstTarget)
                mSelectedTargetEffect = comp;
            else
                comp.Despawn();
        });
    }

    public void SetTargetByPartyTarget()
    {
        if (false == ZPartyManager.Instance.IsParty)
            return;

        //if(ZPartyManager.Instance.IsMaster && null != mTarget)
        if (ZPartyManager.Instance.IsMaster && null != FirstTarget)
        {
            //내가 마스터일 경우 파티 타겟 변경 요청
            //ZPartyManager.Instance.Req_PartyTarget(mTarget?.EntityId ?? 0);
            ZPartyManager.Instance.Req_PartyTarget(FirstTarget?.EntityId ?? 0);
        }
        else if(0 < ZPartyManager.Instance.PartyTargetEntityId)
        {
            //내가 파티원일 경우(혹은 파티장인데 현재 내 타겟이 없을 경우) 파티타겟으로 타겟 변경
            SetTarget(ZPartyManager.Instance.PartyTargetEntityId);
        }
        
    }
    #endregion

    #region ===== :: 타겟 관련 (스캔) :: =====

    public void DoAddEventClickSearchTarget(Action action)
    {
        DoRemoveEventClickSearchTarget(action);
        mEventClickSearchTarget += action;
    }

    public void DoRemoveEventClickSearchTarget(Action action)
    {
        mEventClickSearchTarget -= action;
    }

    public void DoAddEventSetTargetList(Action action)
    {
        DoRemoveEventSetTargetList(action);
        mEventSetTargetList += action;
    }

    public void DoRemoveEventSetTargetList(Action action)
    {
        mEventSetTargetList -= action;
    }

    public void SetTargetList()
    {
        ZPawn target = null;
        
        /// <summary> 스캔버튼을 이미 눌러 타겟리스트가 존재하면 타겟 랜덤 검색 </summary>
        if (TargetSearchList != null && TargetSearchList.Count > 0)
        {
            ZPawnTargetHelper.SearchRandomTargetList(this, ref mTargetSearchList);
        }
        /// <summary> 타겟리스트가 없으면 거리순으로 타겟 검색  </summary>
        else
        {
            for (int i = 0; i < DBConfig.SearchTargetCount; i++)
            {
                target = ZPawnTargetHelper.SearchNearTargetList(this, ref mTargetSearchList);

                if (target == null)
                    break;

                TargetSearchList.Add(target);
            }
        }

        /// <summary> Quest 패널과 상호작용을 위한 Aciton 호출 </summary>
        mEventSetTargetList?.Invoke();
        mEventClickSearchTarget?.Invoke();
    }
    #endregion

    #region ===== :: AI 관련 처리 :: =====    

    public override void StartAI(E_PawnAIType aiType)
    {
        ZLog.LogError(ZLogChannel.Entity, "MyPc에서 StartAI 지원하지 않음");
    }
    public void StartDefaultAI()
    {
        if (E_PawnAIType.AutoBattle != CurrentAIType)
            StopMove();

        AIComponent?.StartAI(E_PawnAIType.AutoBattle);
    }
    public void StartAIForBattle(uint stageTid, Vector3 goalPosition, uint monsterTid)
    {        
        StopAllAction();
        (AIComponent as EntityComponentAIForPlayer)?.StartAI(E_PawnAIType.QuestBattle, stageTid, goalPosition, monsterTid);
    }

    public void StartAIForGathering(uint stageTid, Vector3 goalPosition, uint objectTid)
    {
        StopAllAction();
        (AIComponent as EntityComponentAIForPlayer)?.StartAI(E_PawnAIType.QuestGathering, stageTid, goalPosition, objectTid);
    }

    public void StartAIForMoveTo(uint stageTid, Vector3 goalPosition)
    {
        StopAllAction();
        (AIComponent as EntityComponentAIForPlayer)?.StartAI(E_PawnAIType.MoveTo, stageTid, goalPosition, 0);
    }

    public void StartAIForTalkNpc(uint stageTid, Vector3 goalPosition, uint npcTid)
    {
        StopAllAction();
        (AIComponent as EntityComponentAIForPlayer)?.StartAI(E_PawnAIType.TalkNpc, stageTid, goalPosition, npcTid);
    }

    /// <summary> npc에게 자동이동후 상호작용 </summary>
    public void InteractionToNpc(uint targetTid)
	{
        var npcInfo = ZGameModeManager.Instance.GetNpcInfo(targetTid);

        if(null == npcInfo)
		{
            ZLog.LogError(ZLogChannel.Entity, $"현재 맵에서 해당 Npc({targetTid})를 찾을 수 없습니다.");
            return;
		}

        InteractionToNpc(npcInfo.TableTID, npcInfo.Position);
    }

    /// <summary> npc에게 자동이동후 상호작용 </summary>
    public void InteractionToNpc(EntityBase entity)
    {
        if(null == entity || entity.EntityType != E_UnitType.NPC)
		{
            return;
		}
        InteractionToNpc(entity.TableId, entity.Position);
    }

    public void InteractionToNpc(uint npcTid, Vector3 goalPosition)
	{
        StopAllAction();
        (AIComponent as EntityComponentAIForPlayer)?.StartAI(E_PawnAIType.TalkNpc, ZGameModeManager.Instance.StageTid, goalPosition, npcTid, true);
    }

    /// <summary> 현재 진행중인 모든 액션 취소 (이동, 전투등) </summary>
    public void StopAllAction(bool bBlockMove = false)
    {
        //자동 전투 취소
        IsAutoPlay = false;
        //전투 중지
        ChangeState(E_EntityState.Empty);
        //이동 중지
        StopMove();

        if (bBlockMove)
		{
            IsBlockMoveMyPc = true;
        }
            
    }

    #endregion

    #region ===== :: Event Handle :: =====
    /// <summary> 폰 생성시 처리 </summary>
    private void HandleCreatePawn(uint entityId, ZPawn pawn)
    {
        //파티 타겟 관련 처리
        if(ZPartyManager.Instance.PartyTargetEntityId == entityId)
        {
            HandlePartyTarget(entityId);
        }
    }

    /// <summary> mmo에서 파티 타겟이 변경되었을 경우 알림 </summary>
    private void HandlePartyTarget(uint targetEntityId)
    {
        if(targetEntityId == ZPartyManager.Instance.PartyTargetEntityId && null != mPartyTargetEffect)
        {
            return;
        }

        if (null != mPartyTargetEffect)
        {
            mPartyTargetEffect.Despawn();
        }

        mPartyTargetEffect = null;

        if (ZPawnManager.Instance.TryGetEntity(targetEntityId, out var partyTarget))
        {
            ZEffectManager.Instance.SpawnEffect(DBResource.Fx_PartyTarget, partyTarget.transform, -1f, 1f, (comp) =>
            {
                if (null != mPartyTargetEffect)
                {
                    mPartyTargetEffect.Despawn();
                }

                //if (null != mTarget)
                if (null != FirstTarget)
                    mPartyTargetEffect = comp;
                else if (null != comp)
                    comp.Despawn();
            });
        }
    }

    private void CheckAutoReturnByWeight()
    {
        float weight = GetAbility(E_AbilityType.ITEM_WEIGH) * 100f / GetAbility(E_AbilityType.FINAL_MAX_WEIGH);
        
        if (DBStage.TryGet(ZGameModeManager.Instance.StageTid, out var stageTable))
        {
            if (stageTable.StageType != E_StageType.Town && ZGameOption.Instance.Auto_Return_Town_Weight_Per != 0 && IsAutoPlay)
            {
                if (weight > ((float)ZGameOption.Instance.Auto_Return_Town_Weight_Per))
                {
                    if (ZGameManager.Instance.TryEnterStage(DBConfig.Town_Portal_ID, false, 0, 0))
                    {
                        // Do Action
                    }
                }
            }
        }
    }

    private void HandleDieEntity(uint entityId, ZPawn pawn)
    {
        if (ZPawnManager.Instance.TryGetEntity(entityId, out ZPawn target))
        {
            if(FirstTarget == target)
            {
                FirstTarget = null;

                mEventChangeTarget?.Invoke(entityId, 0);
                CheckTargetSelectFX();

                if(SecondTarget != null)
                {
                    SetTarget(SecondTarget);
                    SecondTarget = null;
                }
            }
        }
    }
    #endregion
}
