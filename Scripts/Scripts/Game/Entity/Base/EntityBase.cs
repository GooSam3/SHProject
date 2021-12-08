using GameDB;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary> 모든 유닛의 기본 class </summary>
public abstract class EntityBase : MonoBehaviour
{
    private Action mEventLoadedModel;
    private Action<EntityBase> mEventDestroyPawn;

    /// <summary> 유니크한 EntityId </summary>
    [SerializeField]
    [ReadOnly]
    private uint mEntityId;

    /// <summary> 유니크한 EntityId </summary>
    public uint EntityId { get { return mEntityId; } }

    /// <summary> 테이블 id </summary>
    [SerializeField]
    [ReadOnly]
    private uint mTableId;

    /// <summary> 테이블 id </summary>
    public uint TableId { get { return mTableId; } }

    /// <summary> Net Entity 여부 </summary>
    [SerializeField]
    [ReadOnly]
    private bool mIsNetEntity;

    /// <summary> Net Entity 여부 </summary>
    public bool IsNetEntity { get { return mIsNetEntity; } }

    /// <summary> 유닛의 타입 </summary>
    public abstract E_UnitType EntityType { get; }

    /// <summary> 내 PC 인지 여부 </summary>
    public virtual bool IsMyPc { get { return false; } }

    public Vector3 Position { get { return transform?.position ?? Vector3.zero; } }
    public Quaternion Rotation { get { return transform?.rotation ?? Quaternion.identity; } }

    /// <summary> Entity 기본 데이터 </summary>
    public EntityDataBase EntityData { get; protected set; }

    /// <summary> TODO :: 임시 - 추후 정리 및 수정 필요 </summary>
    public FollowTarget mTargetUI { get; protected set; }

    /// <summary> 셋팅되어 있는 타겟 </summary>
    protected EntityBase mTarget = null;

    /// <summary> 유닛 반지름 </summary>
    public float Radius { get; protected set; } = 0.5f;

    /// <summary> 해당 타입으로 반환 </summary>
    public T To<T>() where T : EntityBase
    {
        return this as T;
    }

    /// <summary> 해당 타입으로 반환 </summary>
    public T ToData<T>() where T : EntityDataBase
    {
        return EntityData as T;
    }

    public void DoInitialize(EntityDataBase data)
    {
        EntityData = data;
        mEntityId = data.EntityId;
        mTableId = data.TableId;
        mIsNetEntity = data.EntityId > 0;

        OnInitializeEntityData(data);

        if (true == IsMyPc)
            gameObject.layer = UnityConstants.Layers.Player;        
        else
            gameObject.layer = UnityConstants.Layers.Entity;

        // TODO :: 기본 컴포넌트 셋팅 (컴포넌트 생성 정리 필요)
        SetDefaultComponent();

        OnSetDefaultComponentImpl();

        ModelComponent?.DoAddEventLoadedModel(HandelLoadedModel);
        SetModel();

        CreateTargetUI();

        OnInitializeImpl();
        OnInitializeTableDataImpl();
        OnPostInitializeImpl();
    }

	/// <summary> TODO :: 임시 - 추후 정리 및 수정 필요 </summary>
	private void OnDestroy()
    {
        if (false == Application.isPlaying)
            return;

        if (null != mTargetUI)
        {
            GameObject.Destroy(mTargetUI.gameObject);
            mTargetUI = null;
        }

        OnDestroyImpl();
        mEventDestroyPawn?.Invoke(this); 
    }

    public void DestroyEntity()
    {
        var effects = GetComponentsInChildren<ZEffectComponent>();

        foreach (var effect in effects)
        {
            //TODO :: 사망이 아닐 경우 유지되야하는 이펙트 처리해야하할듯?            
            effect.Despawn();
        }

        GameObject.Destroy(gameObject);
    }

    /// <summary> EntityData를 토대로 초기화 </summary>
    protected abstract void OnInitializeEntityData(EntityDataBase data);
    /// <summary> 테이블 셋팅 이전 호출 </summary>
    protected abstract void OnInitializeImpl();
    /// <summary> 테이블 셋팅 이후 호출 </summary>
    protected abstract void OnPostInitializeImpl();
    /// <summary> 테이블 셋팅 </summary>
    protected abstract void OnInitializeTableDataImpl();
    /// <summary> 기본 컴포넌트 셋팅 </summary>
    protected abstract void OnSetDefaultComponentImpl();
    
    /// <summary> 게임 오브젝트 제거시 호출 </summary>
    protected abstract void OnDestroyImpl();

    #region Component
    /// <summary> 모델 관련 컴포넌트 </summary>
    protected EntityComponentModelBase ModelComponent = null;
    /// <summary> 애니메이션 관련 컴포넌트 </summary>
    protected EntityComponentAnimationBase AnimComponent = null;
    /// <summary> 이동 관련 컴포넌트 </summary>
    protected EntityComponentMovementBase MoveComponent = null;
    /// <summary> 스탯 관련 컴포넌트 </summary>
    protected EntityComponentStatBase StatComponent = null;
	/// <summary> </summary>
	public BoxCollider Collider { get; private set; }

	/// <summary> 기본 적인 컴포넌트 셋팅 </summary>
	protected void SetDefaultComponent()
    {
        StatComponent = OnSetStatComponent();
        ModelComponent = OnSetModelComponent();
        AnimComponent = OnSetAnimComponent();
        MoveComponent = OnSetMovementComponent();        
    }

    /// <summary> 모델 컴포넌트 셋팅 </summary>
    protected virtual EntityComponentModelBase OnSetModelComponent()
    {
        return GetOrAddComponent<EntityComponentModelPawn>();
    }

    /// <summary> 애니메이션 컴포넌트 셋팅 </summary>
    protected virtual EntityComponentAnimationBase OnSetAnimComponent()
    {
        return GetOrAddComponent<EntityComponentAnimation_Animator>();
    }

    /// <summary> 이동 컴포넌트 셋팅 </summary>
    protected virtual EntityComponentMovementBase OnSetMovementComponent()
    {
        return GetOrAddComponent<EntityComponentMovement_NavMesh>();
    }

    /// <summary> 스탯 컴포넌트 셋팅 </summary>
    protected virtual EntityComponentStatBase OnSetStatComponent()
    {
        return GetOrAddComponent<EntityComponentStatBase>();
    }

    protected COMPONENT_TYPE GetOrAddComponent<COMPONENT_TYPE>() where COMPONENT_TYPE : EntityComponentBase<EntityBase>
    {        
        COMPONENT_TYPE comp = gameObject.GetOrAddComponent<COMPONENT_TYPE>();
        comp.InitializeComponent(this);
        return comp;
    }

    public void ChangeMovementComponent<COMPONENT_TYPE>() where COMPONENT_TYPE : EntityComponentMovementBase
    {
        if (null != MoveComponent)
        {
            GameObject.Destroy(MoveComponent);
        }

        MoveComponent = GetOrAddComponent<COMPONENT_TYPE>();
    }

	public void SetCollider(Vector3 size)
	{
        //TODO :: 캡슐컬리젼이 있을 경우 박스는 트리거로 사용 (사당쪽 이동이랑 충돌남.)
        bool bTrigger = null != gameObject.GetComponent<CapsuleCollider>();
		if (null == Collider)
		{
			Collider = gameObject.GetOrAddComponent<BoxCollider>();
		}

		Collider.size = size;
		Collider.center = new Vector3(0, size.y * 0.5f, 0);
        Collider.isTrigger = true;

        //TODO :: 유닛 반지름. 일단 작은 값으로 셋팅
        Radius = Mathf.Min(size.x, size.z) * 0.5f;
        //ZLog.Log(ZLogChannel.Default, $"@scale:{EntityData.Name}/{sizeAvg}/{transform.localScale.x}/{Radius}/{sizeAvg * 0.5f}");
    }

    #endregion

    #region MoveComponent    

    /// <summary> 이동 가능 여부 </summary>
    public virtual bool IsBlockMove()
    {
        return false;
    }
    /// <summary> 이동중인지 여부 </summary>
    public bool IsMoving()
    {
        return MoveComponent?.IsMoving() ?? false;
    }

    /// <summary> input으로 이동중인지 여부 </summary>
    public bool IsInputMove()
    {
        return MoveComponent?.IsInputMove ?? false;
    }

    /// <summary> 방향키로 이동중인지 여부 </summary>
    public bool IsMovingDir()
    {
        return MoveComponent?.IsMovingDir() ?? false;
    }

    /// <summary> 해당 위치로 이동 </summary>
    public Vector3? MoveTo(Vector3 destPosition, float speed, bool bInputMove =false)
    {
        if (IsBlockMove())
            return null;

        return MoveComponent?.MoveTo(destPosition, speed, bInputMove) ?? null;
    }

    /// <summary> 해당 경로로 이동 </summary>
    public Vector3? MoveTo(List<Vector3> path, float speed)
    {
        if (IsBlockMove())
            return null;

        return MoveComponent?.MoveTo(path, speed) ?? null;
    }

    /// <summary> 해당 방향으로 이동 </summary>
    public Vector3? MoveToDirection(Vector3 curPosition, Vector3 dir, float speed, Vector2 joystickDir = default)
    {
        //업데이트 순서때문에 따로 처리해야할듯
        //if (IsBlockMove())
        //    return null;

        return MoveComponent?.MoveToDirection(curPosition, dir, speed, joystickDir) ?? null;
    }

    /// <summary> 강제 이동 </summary>
    public virtual void ForceMove(Vector3 position, float duration, E_PosMoveType moveType)
    {
        MoveComponent?.ForceMove(position, duration, moveType);        
    }

    public void Warp(Vector3 position)
    {
        MoveComponent?.Warp(position);
    }

    /// <summary> 이동 중지 </summary>
    public void StopMove(Vector3 curPosition)
    {
        MoveComponent?.StopMove(curPosition);
    }

    /// <summary> 이동 중지 </summary>
    public void StopMove()
    {
        MoveComponent?.StopMove(Position);
    }

    public void DoAddEventMoveState(Action<bool> action)
	{
        MoveComponent?.DoAddEventMoveState(action);
    }

    public void DoRemoveEventMoveState(Action<bool> action)
    {
        MoveComponent?.DoRemoveEventMoveState(action);
    }

    #endregion

    #region Animator

    private Action<E_AnimParameter, object> mEventChangeAnimParameter;

    private Action<bool> mEventChangeMoveMotion;

    /// <summary> 애니메이션 컨트롤러가 변경됨 </summary>
    private Action mEventChangeAnimController;

    public float MoveSpeedRate => AnimComponent?.MoveSpeedRate ?? 1f;
    public void DoAddEventChangeAnimController(Action action)
    {
        DoRemoveEventChangeAnimController(action);
        mEventChangeAnimController += action;
    }

    public void DoRemoveEventChangeAnimController(Action action)
    {
        mEventChangeAnimController -= action;
    }

    public void DoAddEventChangeAnimParameter(Action<E_AnimParameter, object> action)
    {
        DoRemoveEventChangeAnimParameter(action);
        mEventChangeAnimParameter += action;
    }

    public void DoRemoveEventChangeAnimParameter(Action<E_AnimParameter, object> action)
    {
        mEventChangeAnimParameter -= action;
    }

    public void DoAddEventChangeMoveMotion(Action<bool> action)
    {
        DoRemoveEventChangeMoveMotion(action);
        mEventChangeMoveMotion += action;
    }

    public void DoRemoveEventChangeMoveMotion(Action<bool> action)
    {
        mEventChangeMoveMotion -= action;
    }

    /// <summary> 이동 모션을 변경한다. </summary>
    public void MoveAnim(bool bMove, bool bForce = false)
    {
        AnimComponent?.Move(bMove, bForce);
        mEventChangeMoveMotion?.Invoke(bMove);
    }

    public void DieAnim(bool bDie, float normalizeTime = 0f)
    {
        AnimComponent?.Die(bDie, normalizeTime);
    }

    /// <summary> 해당 animstate의 normailize time을 변경한다. </summary>
    public void PlayByNormalizeTime(E_AnimStateName state, float normalizeTime)
    {
        AnimComponent?.PlayByNormalizeTime(state, normalizeTime);
    }

    public void Spasiticity( float time, float speed  )
    {
        AnimComponent?.Spasiticity( time, speed );
    }

    /// <summary> Animator Parameter를 셋팅한다. </summary>
    public void SetAnimParameter(E_AnimParameter parameter, bool value)
    {
        AnimComponent?.SetAnimParameter(parameter, value);
        mEventChangeAnimParameter?.Invoke(parameter, value);
    }

    /// <summary> Animator Parameter를 셋팅한다. </summary>
    public void SetAnimParameter(E_AnimParameter parameter, float value)
    {
        AnimComponent?.SetAnimParameter(parameter, value);
        mEventChangeAnimParameter?.Invoke(parameter, value);
    }

    /// <summary> Animator Parameter를 셋팅한다. </summary>
    public void SetAnimParameter(E_AnimParameter parameter, int value)
    {
        AnimComponent?.SetAnimParameter(parameter, value);
        mEventChangeAnimParameter?.Invoke(parameter, value);
    }

    /// <summary> Animator Parameter를 셋팅한다. </summary>
    public void SetAnimParameter(E_AnimParameter parameter)
    {
        AnimComponent?.SetAnimParameter(parameter);
        mEventChangeAnimParameter?.Invoke(parameter, null);
    }

    public bool GetBool(E_AnimParameter type)
    {
        return AnimComponent != null ? AnimComponent.GetBool(type) : false;
    }

    /// <summary> 애니메이션의 이동 속도 비율을 변경한다. </summary>    
    public virtual void SetAnimMoveSpeed(float value)
    {
        AnimComponent?.SetMoveSpeed(value);
    }

    /// <summary> 공격 속도 비율을 변경한다. </summary>    
    public void SetAttackSpeedRate(float value)
    {
        AnimComponent?.SetAttackSpeedRate(value);
    }

    public void ResetAnim()
    {
        AnimComponent?.ResetAnim();
    }

    /// <summary> 케릭터 애니메이션 변경 </summary>    
    public void ChangeAnimController(AnimatorOverrideController controller)
    {
        AnimComponent?.ChangeController(controller);
        mEventChangeAnimController?.Invoke();
    }
    
    /// <summary> 애니메이션 길이를 얻어온다. </summary>
    public float GetAnimLength(E_AnimStateName animName)
    {
        return AnimComponent?.GetAnimLength(animName) ?? 1f;
    }

    /// <summary> 애니메이션 길이를 얻어온다. </summary>
    public float GetAnimLength(string animName)
    {
        return AnimComponent?.GetAnimLength(animName) ?? 1f;
    }

    public void ChangeAnimationClip(E_AnimStateName state, AnimationClip clip)
	{
        AnimComponent?.ChangeClip(state, clip);
	}

    public void ChangeAnimationClip(E_AnimStateName state, string clipName)
    {
        ZResourceManager.Instance.Load<AnimationClip>(clipName, (assetName, clip) =>
        {
            if(null != clip)
                AnimComponent?.ChangeClip(state, clip);
        });
    }

    #endregion

    #region Model
    public Resource_Table ResourceTable
    {
        get
        {
            return ModelComponent?.ResourceTable;
        }
    }

    public GameObject ModelGo { get { return ModelComponent?.ModelGo; } }

    public void SetModel()
    {
        ModelComponent?.SetModel();
    }

    public void SetModel(string assetName)
    {
        ModelComponent?.SetModel(assetName);
    }

    public Transform GetSocket(E_ModelSocket socket)
    {
        return ModelComponent?.GetSocket(socket);
    }

    private void HandelLoadedModel(GameObject modelGo)
    {
        if (null == modelGo)
        {
            //if (null != ModelComponent)
            //{
            //    GameObject.Destroy(ModelComponent);
            //}
            return;
        }

        Animator animator = modelGo.GetComponent<Animator>();

        if (animator)
        {
            AnimComponent.InitAnim(modelGo);            

            EntityAnimatorEvent animEvent = animator.gameObject.GetOrAddComponent<EntityAnimatorEvent>();
            animEvent.Initialize(this);
        }
        else
        {
            Animation animation = modelGo.GetComponent<Animation>();

            //
        }

        if (null != mTargetUI)
        {
            mTargetUI.Init(GetSocket(E_ModelSocket.Head));
        }

        MoveComponent?.OnChangeModel();

        OnLoadedModelImpl();
        mEventLoadedModel?.Invoke();
    }

    /// <summary> 디졸브 연출 </summary>
    public void Dissolve(bool bDissolve, float duration = 3f, float delayTime = 0f, Action onFinish = null)
    {
        ModelComponent?.Dissolve(bDissolve, duration, delayTime, onFinish);
    }

    /// <summary> 모델 로드 완료시 알림 추가 </summary>
    public void DoAddEventLoadedModel(Action action, bool bAlreadyInvoke = true)
    {        
        DoRemoveEventLoadedModel(action);
        mEventLoadedModel += action;


        //모델이 로드된 상태라면 바로 갱신
        if(bAlreadyInvoke && ModelComponent?.ModelGo)
        {
            action?.Invoke();
        }
    }

    /// <summary> 모델 로드 완료시 알림 제거 </summary>
    public void DoRemoveEventLoadedModel(Action action)
    {
        mEventLoadedModel -= action;
    }

    /// <summary> 모델 로드 완료시 </summary>
    protected abstract void OnLoadedModelImpl();
    #endregion

    #region Stat
    /// <summary> 사망 여부 </summary>
    public virtual bool IsDead { get { return StatComponent?.CurrentHp <= 0; } }

    /// <summary> 이동 속도 </summary>
    public float MoveSpeed { get { return StatComponent?.MoveSpeed ?? 5f; } }

    /// <summary> 공격 속도 </summary>
    public float AttackSpeedRate { get { return StatComponent?.AttackSpeedRate ?? 1f; } }

    /// <summary> 스킬 속도 </summary>
    public float SkillSpeedRate { get { return StatComponent?.SkillSpeedRate ?? 1f; } }

    public void SetMoveSpeed(float value)
    {
        StatComponent?.SetMoveSpeed(value);
    }

    /// <summary> 변경된 어빌리티 셋팅 </summary>
    public void AbilityNotify(E_AbilityType abilityType, float value)
    {
        StatComponent?.SetAbility(abilityType, value);
    }

    /// <summary> 변경된 스킬 어빌리티 셋팅 </summary>
    public void SkillAbilityNotify(uint skillTid, E_AbilityType abilityType, float value)
    {
        StatComponent?.SetSkillAbility(skillTid, abilityType, value);
    }

    public float GetAbility(E_AbilityType abilityType)
    {
        return StatComponent?.GetAbility(abilityType) ?? 0;
    }

    public bool GetSkillAbilitys(uint skillTid, out Dictionary<E_AbilityType, float> values)
    {
        if(null == StatComponent)
        {
            values = null;
            return false;
        }
        return StatComponent.GetSkillAbilitys(skillTid, out values);
    }

    public float GetSkillAbility(uint skillTid, E_AbilityType abilityType)
    {
        return StatComponent?.GetSkillAbility(skillTid, abilityType) ?? 0;
    }

    /// <summary> 스탯 변경시 이벤트 등록 </summary>
    public void DoAddEventStatUpdated(Action<Dictionary<E_AbilityType, float>> action, bool bGetCurrentValue = false)
    {
        StatComponent?.DoAddEventAbilityUpdated(action, bGetCurrentValue);
    }

    /// <summary> 스탯 변경시 이벤트 등록 해제 </summary>
    public void DoRemoveEventStatUpdated(Action<Dictionary<E_AbilityType, float>> action)
    {
        StatComponent?.DoRemoveEventAbilityUpdated(action);
    }

    /// <summary> 스킬 스탯 변경시 이벤트 등록 </summary>
    public void DoAddEventSkillAbilityUpdated(Action<Dictionary<uint, Dictionary<E_AbilityType, float>>> action, bool bGetCurrentValue = false)
    {
        StatComponent?.DoAddEventSkillAbilityUpdated(action, bGetCurrentValue);
    }

    /// <summary> 스킬 스탯 변경시 이벤트 등록 해제 </summary>
    public void DoRemoveEventSkillAbilityUpdated(Action<Dictionary<uint, Dictionary<E_AbilityType, float>>> action)
    {
        StatComponent?.DoRemoveEventSkillAbilityUpdated(action);
    }
    #endregion

    #region ===== :: 무게 관련 :: =====    
    [SerializeField]
    [Header("Entity의 무게")]
    private float mWeight = 0f;

    /// <summary> 기믹 무게 </summary>
    public float Weight
    {
        get
        {
            return mWeight;
        }
        set
        {
            ZPawnManager.Instance.ChangeWeight(this, mWeight, value);
            mWeight = value;
        }
    }
    #endregion

    #region Virtual Function
    public virtual void LookAt(float dir)
    {
        Quaternion rot = Quaternion.Euler(0, dir, 0);

        var forward = rot * Vector3.forward;

        transform.forward = forward;
    }

    public virtual void LookAt(Vector3 pos)
    {
        pos.y = transform.position.y;
        transform.transform.LookAt(pos);
    }

    public virtual void LookAt(Transform trans)
    {
        LookAt(trans.position);
    }

    /// <summary> TODO :: 임시 타겟 ui 노출 </summary>
    protected virtual void CreateTargetUI()
    {
        //UIFrameHUD frame = UIManager.Instance.Find<UIFrameHUD>();

        //if (null != frame)
        //{
        //    if (null != mTargetUI)
        //    {
        //        GameObject.Destroy(mTargetUI.gameObject);
        //    }

        //    frame.CreateTargetUI(EntityType, (targetUI) =>
        //    {
        //        mTargetUI = targetUI;
        //        mTargetUI?.Init(GetSocket(E_ModelSocket.Head));
        //        mTargetUI?.SetData(this);
        //    });
        //}
    }

    /// <summary> 타겟을 셋팅함 </summary>
    public virtual void SetTarget(uint targetEntityId)
    {
        if (ZPawnManager.Instance.TryGetEntity(targetEntityId, out ZPawn target))
        {
            SetTarget(target);
        }
    }

    /// <summary> 타겟을 셋팅함 </summary>
    public virtual void SetTarget(EntityBase target)
    {
        mTarget = target;
    }

    /// <summary> 현재 내 타겟을 얻어옴. 유효하지 않다면 null </summary>
    public virtual EntityBase GetTarget()
    {
        if(null != mTarget && mTarget.IsDead)
        {
            mTarget = null;
        }

        return mTarget;
    }
    #endregion

    #region ===== :: Event :: =====
    /// <summary> 폰 제거시  </summary>
    public void DoAddEventDestroyPawn(Action<EntityBase> action)
    {
        DoRemoveEventDestroyPawn(action);
        mEventDestroyPawn += action;
    }

    public void DoRemoveEventDestroyPawn(Action<EntityBase> action)
    {
        mEventDestroyPawn -= action;
    }
    #endregion
}
