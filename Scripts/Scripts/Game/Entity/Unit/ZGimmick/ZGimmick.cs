using GameDB;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

/// <summary> 사당용 기믹!!! </summary>
public class ZGimmick : EntityBase
{
    /// <summary> 기믹 찾기용 ID </summary>
    [Header("Gimmick 을 찾기위해 사용됨(필요한 경우 사용, 중복 가능)")]
    [SerializeField]
    private string mGimmickId;

    [Header("Gimmick의 반지름 (충돌체가 없을 경우 사용)")]
    [SerializeField]
    private float mGimmickRaius = 0.5f;

    [Header("Gimmick의 재질")]
    [SerializeField]
    private E_TempleGimmickMeterial mMeterial = E_TempleGimmickMeterial.None;

    public E_TempleGimmickMeterial Meterial { get { return mMeterial; } }

    public string GimmickId { get { return mGimmickId; } }

    public override E_UnitType EntityType { get { return E_UnitType.Gimmick; } }

    public ZGimmickDataBase ZGimmickData { get; private set; }

    /// <summary> 활성화되어있는 기믹 여부 </summary>
    [Header("시작시 활성화 여부")]
    [SerializeField]
    private bool IsEnableOnAwake = true;

    /// <summary> 해당 트리거에 접근했을 경우. 발동되는 타입 </summary>
    [Header("해당 트리거에 접근했을 경우 발동되는 타입")]
    [SerializeField]
    private E_GimmickTriggerType TriggerType;

    /// <summary> 기믹 발동 횟수 타입 </summary>
    [Header("발동 횟수 타입. loop 일 경우 MaxInvokeCount 만큼 발동")]
    [SerializeField]
    private E_GimmickInvokeCountType InvokeCountType;

    /// <summary> 0일 경우 무제한 발동 </summary>
    [Header("발동하는 횟수. 0이고 Loop 타입일 경우 무제한 발동")]
    [SerializeField]
    private int MaxInvokeCount = 0;

    /// <summary> 타겟 가능 여부 </summary>
    [Header("타겟 가능 여부")]
    [SerializeField]
    public bool IsTargetable = false;

    /// <summary> 최초 강제 활성화/비활성화 연출 </summary>    
    [Header("최초 강제 활성화/비활성화 연출")]
    [SerializeField]
    private bool IsForceEnableOrDisableInvoke;

    [Header("최초 시작시 활성화/비활성화 될 때 발동할 속성 레벨")]
    [SerializeField]
    public E_AttributeLevel InvokeAttributeLevel = E_AttributeLevel.Level_1;



    /// <summary> 기믹 활성화 여부 </summary>
    public bool IsEnabled { get; private set; }

    /// <summary> 각 액션 타입별 발동 횟수 </summary>
    private Dictionary<E_GimmickActionInvokeType, int> m_dicInvokeCount = new Dictionary<E_GimmickActionInvokeType, int>();

    /// <summary> 각 발동 타입에 따른 액션 모음 </summary>
    private Dictionary<E_GimmickActionInvokeType, List<ZGimmickActionBase>> m_dicAction = new Dictionary<E_GimmickActionInvokeType, List<ZGimmickActionBase>>();
    /// <summary> 각 발동 타입에 따른 취소 액션 모음 </summary>
    private Dictionary<E_GimmickActionInvokeType, List<ZGimmickActionBase>> m_dicCancelAction = new Dictionary<E_GimmickActionInvokeType, List<ZGimmickActionBase>>();
    
    private Action<ZGimmick, E_GimmickActionInvokeType> mEventActionInvoke { get; set; }

    private Action<ZGimmick, E_UnitAttributeType> mEventTakeAttribute { get; set; }

    /// <summary> 네비 메시 길막용 </summary>
    private NavMeshObstacle mObstacle = null;

    private E_TempleGimmickState GimmickState = E_TempleGimmickState.None;

    [Header("Obstacle On/Off 기능")]
    [SerializeField]
    private bool IsUseableObstacle = false;

    private UISubHUDTemple SubHudTemple = null;

    [Header("속성 피해를 입을 경우 지속적으로 입는다. (화염방사기 등)")]
    [SerializeField]
    public bool IsTakeAttributeLoop = false;

    [Header("탈것을 탄 상태로 상호작용 가능여부")]
    [SerializeField]
    public bool IsActiveRiding = false;

    /// <summary>
    /// 얼음상태 , 화염상태 중인 기믹인지
    /// </summary>
    [HideInInspector]
    public bool IsUseAwakeAttribute = false;

#if UNITY_EDITOR
    public float mTriggerEnableTimeForEditor;
#endif

    protected override void OnInitializeEntityData(EntityDataBase data)
    {
        ZGimmickData = data.To<ZGimmickDataBase>();
        SetGimmickData(ZGimmickData);
        //기믹 기본 셋팅한다.
    }

    private void SetGimmickData(ZGimmickDataBase ZPawnData)
    {
    }

    protected override void OnInitializeImpl()
    {
    }

    protected override void OnInitializeTableDataImpl()
    {
    }

    protected override void OnPostInitializeImpl()
    {

    }
    void Start()
    {
        // NOTE(JWK): 최초 시작 속성 셋팅
        AddAwakeAttribute();

        var colliders = GetComponentsInChildren<Collider>();

        float maxSize = mGimmickRaius;

        foreach (var col in colliders)
        {
            if (true == col.isTrigger)
                continue;

            maxSize = Mathf.Max(maxSize, Mathf.Max(col.bounds.extents.x, col.bounds.extents.z));
        }

        var rBody = GetComponentInChildren<Rigidbody>();

        if(null != rBody)
        {
            rBody.interpolation = RigidbodyInterpolation.Interpolate;
        }

        ZGimmickManager.Instance.Add(this);

        SetDefaultComponent();

        SetModel();

        AnimComponent.InitAnim(null);

        var list = new List<ZGimmickActionBase>(GetComponentsInChildren<ZGimmickActionBase>());

        m_dicAction.Clear();
        m_dicCancelAction.Clear();
        m_dicInvokeCount.Clear();

        foreach (var action in list)
        {
            action.Initialize(this);
            var actionInvokeType = action.InvokeType;
            var cancelInvokeType = action.CancelInvokeType;

            foreach (E_GimmickActionInvokeType type in Enum.GetValues(typeof(E_GimmickActionInvokeType)))
            {
                if ((type & actionInvokeType) > 0)
                {
                    if (false == m_dicAction.TryGetValue(type, out var actions))
                    {
                        m_dicAction[type] = new List<ZGimmickActionBase>();
                        actions = m_dicAction[type];

                        //발동 횟수 등록
                        m_dicInvokeCount[type] = 0;
                    }

                    m_dicAction[type].Add(action);
                }
                else if((type & cancelInvokeType) > 0)
                {
                    if (false == m_dicCancelAction.TryGetValue(type, out var actions))
                    {
                        m_dicCancelAction[type] = new List<ZGimmickActionBase>();
                        actions = m_dicCancelAction[type];
                    }

                    m_dicCancelAction[type].Add(action);
                }                
            }
        }
        
        SetEnable(IsEnableOnAwake, InvokeAttributeLevel, IsForceEnableOrDisableInvoke);

        mObstacle = gameObject.GetComponent<NavMeshObstacle>();
        SetNavmeshObstacle(IsUseableObstacle);

        //레이어 변경
        gameObject.SetLayersRecursively(UnityConstants.Layers.Entity);

        //gameObject.SetLayersRecursively(UnityConstants.Layers.Gimmick);

        ZGimmickManager.Instance.PostInitializeGimmick(this);
    }
    protected override void OnSetDefaultComponentImpl()
    {
    }

    protected override void OnDestroyImpl()
    {
        if (false == ZGimmickManager.hasInstance)
            return;

        HideInteractionUI();
        ZGimmickManager.Instance.Remove(this);
    }

    protected override void OnLoadedModelImpl()
    {
    }

    /// <summary> 기믹 ID 셋팅 </summary>
    public void SetGimmickId(string gimmickId)
    {
        mGimmickId = gimmickId;
        ZGimmickManager.Instance.Add(this);
    }

    public override bool IsDead => GimmickState.HasFlag(E_TempleGimmickState.Die);

    #region Component
    ///// <summary> 모델 컴포넌트 셋팅 </summary>
    protected override EntityComponentModelBase OnSetModelComponent()
    {
        return GetOrAddComponent<EntityComponentModelGimmick>();
    }

    ///// <summary> 애니메이션 컴포넌트 셋팅 </summary>
    protected override EntityComponentAnimationBase OnSetAnimComponent()
    {
        return GetOrAddComponent<EntityComponentAnimation_Animator>();
    }

    /// <summary> 이동 컴포넌트 셋팅 </summary>
    protected override EntityComponentMovementBase OnSetMovementComponent()
    {
        return null;
    }

    /// <summary> 스탯 컴포넌트 셋팅 </summary>
    protected override EntityComponentStatBase OnSetStatComponent()
    {
        return null;
    }
    #endregion

    /// <summary> 해당 기믹을 활성화 시킨다. </summary>
    public void SetEnable(bool bActive, E_AttributeLevel level, bool bForce = false)
    {
        Debug.Log($"Name : {gameObject.name}");
        // NOTE(JWK): 최초 시작 속성 적용 체크
        if (true == IsUseAwakeAttribute)
            return;

        bool bInvoke = IsEnabled != bActive;
        IsEnabled = bActive;

        //활성화/비활성화는 invokeCount 체크할 필요 없을듯?
        if (bInvoke || bForce)
        {
            if (IsEnabled)
            {
                InvokeAction(E_GimmickActionInvokeType.Enable, level);                
            }   
            else
            {
                InvokeAction(E_GimmickActionInvokeType.Disable, level);                
            }   
        }
        else
        {
            //활성화/비활성화 할 필요가 없다면 속성 레벨 변경
            ChangeAttributeLevel(level);
        }

        //TODO :: 활성화/비활성화 이펙트?
    }

    /// <summary> 발동 횟수를 체크한다. </summary>
    private bool CheckInvokeCount(E_GimmickActionInvokeType type)
    {
        if(false == m_dicInvokeCount.TryGetValue(type, out var cnt))
        {
            return false;
        }

        switch(InvokeCountType)
        {
            case E_GimmickInvokeCountType.OneShot:
                return 0 >= cnt;
            case E_GimmickInvokeCountType.Loop:
                return MaxInvokeCount == 0 || MaxInvokeCount > cnt; 
        }

        return false;
    }

    /// <summary>
    /// 캐릭터가 NearTarget 을 검색할때 발동횟수와 조건을 토대로 타겟이 가능한지 검색 
    /// - OneShot Door 같은경우, 1번만 타겟이 지정되고 그 이후론 지정이 되면 안되므로 추가함.
    /// </summary>
    /// <returns></returns>
    public bool IsRemainInvokeCount()
	{
        // 무한으로 발동가능한 기믹
        if(E_GimmickInvokeCountType.Loop == InvokeCountType && MaxInvokeCount == 0)
            return true;

        int totalInvokeCount = 0;
        // MaxInvokeCount 보다 Action 의 발동횟수가 작은게 있으면 발동가능
        foreach (KeyValuePair<E_GimmickActionInvokeType, int> action in m_dicInvokeCount)
		{
            if (action.Value < MaxInvokeCount)
                return true;

            if(action.Key != E_GimmickActionInvokeType.Enable)
                totalInvokeCount += action.Value;
        }

        // 발동타입이 'OneShot' 이고 발동횟수 총합이 0 인 경우 발동가능,
        if (E_GimmickInvokeCountType.OneShot == InvokeCountType && totalInvokeCount == 0)
            return true;

        return false;
	}

    public void OnTriggerEnter(Collider other)
    {
        InvokeTrigger(other, E_GimmickActionInvokeType.Enter);
    }

    public void OnTriggerExit(Collider other)
    {
        InvokeTrigger(other, E_GimmickActionInvokeType.Exit);
    }

    private void OnTriggerStay(Collider other)
    {
        InvokeTrigger(other, E_GimmickActionInvokeType.Stay);
    }
    
    private void InvokeTrigger(Collider other, E_GimmickActionInvokeType invokeType)
    {
        if (false == IsEnabled)
            return;

        var pc = other.gameObject.GetComponent<ZPawnMyPc>();
        if (null == pc || false == pc.IsMyPc || false == other.isTrigger)
            return;

        if (false == IsCheckRiding())
            return;

        E_AttributeLevel level = pc.AttributeLevel;

        // NOTE(JWK): 최초 얼음, 불 등 방해요소가 적용되어있는지 검사
        if (true == CheckAwakeAttribute(invokeType))
            return;

        switch (TriggerType)
        {
            case E_GimmickTriggerType.Immediate:
                {
                    InvokeAction(invokeType, level);
                }
                break;
            case E_GimmickTriggerType.ActiveTouchUI:
                {                    
                    SubHudTemple = UIManager.Instance.Find<UISubHUDTemple>();

                    if(null != SubHudTemple)
                    {
                        switch (invokeType)
                        {
                            case E_GimmickActionInvokeType.Enter:
                                {
                                    if (false == CheckInvokeCount(invokeType))
                                        return;

                                    SubHudTemple.SetInteractionGimmick(true, () =>
                                    {   
                                        HideInteractionUI();

                                        if (false == IsCheckRiding())
                                            return;

                                        InvokeAction(invokeType, level);                                        
                                    });
                                }
                                break;
                            case E_GimmickActionInvokeType.Exit:
                                {
                                    HideInteractionUI();
                                }
                                break;
                            default:
                                {
                                    if (false == CheckInvokeCount(invokeType))
                                        return;

                                    if (false == IsCheckRiding())
                                        return;
                                    InvokeAction(invokeType, level);
                                }
                                break;
                        }
                    }
                    else if(false == ZWebManager.hasInstance || false == ZWebManager.Instance.WebGame.IsUsable)
                    {
                        if (false == CheckInvokeCount(invokeType))
                            return;

                        if (false == IsCheckRiding())
                            return;

#if UNITY_EDITOR
                        if (mTriggerEnableTimeForEditor > Time.time)
                            return;
#endif

                        InvokeAction(invokeType, level);
                    }
                }
                break;
        }
    }

    /// <summary>
    /// 기믹의 '탈것을 타고있는지' 에 대한 체크
    /// </summary>
    /// <returns></returns>
    private bool IsCheckRiding()
	{
        var myPc = ZPawnManager.Instance.MyEntity;

        // 안타고 있을떄 - 가능
        if (false == myPc.IsRiding || (true == myPc.IsRiding && true == IsActiveRiding))
            return true;

        return false;
    }

    private void HideInteractionUI()
    {
        if(SubHudTemple == null)
        {
            return;
        }

        SubHudTemple.SetInteractionGimmick(false);
        SubHudTemple = null;
    }

    public void TakeAttribute(E_UnitAttributeType type, E_AttributeLevel level)
    {
        E_GimmickActionInvokeType actionType = E_GimmickActionInvokeType.None;
        switch (type)
        {
            case E_UnitAttributeType.Fire:
                actionType = E_GimmickActionInvokeType.Attribute_Fire;
                break;
            case E_UnitAttributeType.Water:
                actionType = E_GimmickActionInvokeType.Attribute_Water;
                break;
            case E_UnitAttributeType.Electric:
                actionType = E_GimmickActionInvokeType.Attribute_Electric;
                break;
            case E_UnitAttributeType.Light:
                actionType = E_GimmickActionInvokeType.Attribute_Light ;
                break;
            case E_UnitAttributeType.Dark:
                actionType = E_GimmickActionInvokeType.Attribute_Dark;
                break;
            case E_UnitAttributeType.None:
                actionType = E_GimmickActionInvokeType.None;
                break;
        }

        // NOTE(JWK): 최초 얼음, 불 등 방해요소가 적용되어있는지 검사
        if (true == CheckAwakeAttribute(actionType))
            return;

        if (actionType != E_GimmickActionInvokeType.None)
        {
            InvokeAction(actionType, level);
            mEventTakeAttribute?.Invoke(this, type);
        }
    }

    /// <summary> 해당 타입을 발동시킨다. </summary>
    private void InvokeAction(E_GimmickActionInvokeType type, E_AttributeLevel level)
    {
        if (true == CheckInvokeCount(type))
        {
            if (false == m_dicAction.TryGetValue(type, out var actions))
            {
                return;
            }

            InvokeAction(ref actions, level);
            ++m_dicInvokeCount[type];

            mEventActionInvoke?.Invoke(this, type);

            ZGimmickManager.Instance.InvokeAction(this, type);
        }

        //취소 관련 액션 발동
        CancelAction(type);

        var pc = ZPawnManager.Instance.MyEntity;
        pc?.SetTarget(null);
    }

    /// <summary> 해당 타입을 발동시킨다. </summary>
    public void InvokeActions(E_GimmickActionInvokeType flag, E_AttributeLevel level)
    {
        foreach (E_GimmickActionInvokeType type in Enum.GetValues(typeof(E_GimmickActionInvokeType)))
        {
            if ((type & flag) > 0)
            {
                InvokeAction(type, level);
            }
        }
    }

    /// <summary> GimmickAction에 Cancel로 등록된 type으로 발동 </summary>
    private void CancelAction(E_GimmickActionInvokeType type)
    {
        if (false == m_dicCancelAction.TryGetValue(type, out var actions))
        {
            return;
        }

        CancelAction(ref actions);
    }

    private void InvokeAction(ref List<ZGimmickActionBase> actions, E_AttributeLevel level)
    {
        if (actions.Count <= 0)
            return;

        foreach (var action in actions)
        {
            action.Invoke(level);
        }
    }

    private void CancelAction(ref List<ZGimmickActionBase> actions)
    {
        if (actions.Count <= 0)
            return;

        foreach (var action in actions)
        {
            action.Cancel();
        }
    }

    /// <summary> 모든 액션을 취소한다. </summary>
    public void CancelActionAll()
    {
        foreach(var actions in m_dicAction)
        {
            foreach(var action in actions.Value)
            {
                action.Cancel();
            }
        }
    }

    /// <summary> 모든 액션을 비활성화 한다. </summary>
    public void DisableActionAll()
    {
        foreach (var actions in m_dicAction)
        {
            foreach (var action in actions.Value)
            {
                action.DisableAction();
            }
        }
    }

    /// <summary> 작동 속성 레벨 변경 </summary>
    public void ChangeAttributeLevel(E_AttributeLevel level)
    {
        foreach(var actions in m_dicAction)
        {
            foreach(var action in actions.Value)
            {
                action.ChangeInvokeAttributeLevel(level);
            }
        }
    }

    // NOTE(JWK): 속성레벨을 업, 다운 시킬경우 사용
    /// <summary> 작동 속성 레벨 변경 </summary>
    public void ChangeAttributeLevel(bool isUpgrade)
    {
        var list = GetComponentsInChildren<ZGimmickActionBase>();
        foreach(var actionBase in list)
		{
            actionBase.ChangeInvokeAttributeLevel(isUpgrade);
        }
    }

    /// <summary> 플레이어의 NavmeshAgent가 활성화되었을 경우 처리해줘야한다. </summary>
    public void SetNavmeshObstacle(bool bObstacle)
    {
        if (null == mObstacle)
            return;

        mObstacle.enabled = bObstacle;
    }

    public void SetGimmickState(E_TempleGimmickState state, bool bApply)
    {
        if (bApply)
        {
            GimmickState |= state;
        }
        else
        {
            GimmickState &= ~state;
        }
    }

    /// <summary> 기믹을 제거한다. </summary>
    public void DestroyGimmick(float dissolveDuration, float delayTime, Action onFinish = null)
    {
        SetGimmickState(E_TempleGimmickState.Die, true);
        DisableActionAll();
        Dissolve(true, dissolveDuration, delayTime, () =>
        {
            ZGimmickManager.Instance.RemoveSpreadAttribute(this);
            onFinish?.Invoke();
            GameObject.Destroy(gameObject);
        });        
    }

    /// <summary>
    /// 최초 시작시 속성이 적용되어 있는지 검사
    /// </summary>
    /// <param name="invokeType"></param>
    /// <returns></returns>
    private bool CheckAwakeAttribute(E_GimmickActionInvokeType invokeType)
	{
        if (false == IsUseAwakeAttribute)
            return false;

        var actionBases = m_dicAction.FirstOrDefault(d => d.Key == invokeType).Value;
        if (null == actionBases)
            return false;

        var awakeAttributeAction = actionBases.FirstOrDefault(d => d.IsAwakeAttribute == true);
        if (null == awakeAttributeAction)
            return false;

        ZGA_AwakeAttribute action = (ZGA_AwakeAttribute)awakeAttributeAction;
        action.AwakeAttributeCompleteAction = () =>
        {
            SetEnable(IsEnableOnAwake, InvokeAttributeLevel, IsForceEnableOrDisableInvoke);
        };
        action.Invoke(E_AttributeLevel.Level_1);

        return true;
    }

    /// <summary>
    /// 최초시작 속성 값 셋팅
    /// </summary>
    private void AddAwakeAttribute()
	{
        ZGA_AwakeAttribute awakeAction = GetComponentInChildren<ZGA_AwakeAttribute>();
        if (null == awakeAction)
			return;

        awakeAction.InvokeType = E_GimmickActionInvokeType.None;
        if (awakeAction.AwakeAttributeType == E_UnitAttributeType.None)
		{
            IsUseAwakeAttribute = false;
            return;
		}
        else
		{
            IsUseAwakeAttribute = true;
        }
		
		switch (awakeAction.AwakeAttributeType)
		{
			case E_UnitAttributeType.Fire: awakeAction.InvokeType = E_GimmickActionInvokeType.Attribute_Water; break;
			case E_UnitAttributeType.Water: awakeAction.InvokeType = E_GimmickActionInvokeType.Attribute_Fire; break;
			default:
                IsUseAwakeAttribute = false;
                break;
		}
	}

    /// <summary>
    /// 속성 상태에 따라 메테리얼 쉐이더 변경
    /// </summary>
    /// <param name="type"></param>
    public void SetAttributeMaterialColor(E_UnitAttributeType type)
	{
        ModelComponent?.Attribute_Effect(type);
    }

    #region ===== :: 타겟팅 관련 처리 :: =====

    private int Gimmick_RIM_COLOR_ID = Shader.PropertyToID("_GimmickColor");
    private int Gimmick_RIM_STRENGTH_ID = Shader.PropertyToID("_GimmickStrength");

    private bool mIsTargeted = false;
    public void SetTargeted(bool bTargeted)
    {
        mIsTargeted = bTargeted;
        ModelComponent?.ChangeMaterialColor(Gimmick_RIM_COLOR_ID, ResourceSetManager.Instance.SettingRes.Palette.Gimmick_Target);
        ModelComponent?.ChangeMaterialFloat(Gimmick_RIM_STRENGTH_ID, bTargeted ? 1f : 0f);
    }

    public void SetGrivityControlTargeted(bool bTargeted)
    {
        if(true == bTargeted)
        {
            switch (mMeterial)
            {
                case E_TempleGimmickMeterial.None:
                    {
                        ModelComponent?.ChangeMaterialColor(Gimmick_RIM_COLOR_ID, ResourceSetManager.Instance.SettingRes.Palette.Gimmick_None);
                    }
                    break;
                case E_TempleGimmickMeterial.Wood:
                    {
                        ModelComponent?.ChangeMaterialColor(Gimmick_RIM_COLOR_ID, ResourceSetManager.Instance.SettingRes.Palette.Gimmick_Wood);
                    }
                    break;
                case E_TempleGimmickMeterial.Ston:
                    {
                        ModelComponent?.ChangeMaterialColor(Gimmick_RIM_COLOR_ID, ResourceSetManager.Instance.SettingRes.Palette.Gimmick_Ston);
                    }
                    break;
                case E_TempleGimmickMeterial.Metal:
                    {
                        ModelComponent?.ChangeMaterialColor(Gimmick_RIM_COLOR_ID, ResourceSetManager.Instance.SettingRes.Palette.Gimmick_Metal);
                    }
                    break;
            }

            ModelComponent?.ChangeMaterialFloat(Gimmick_RIM_STRENGTH_ID, 1f);
        }
        else
        {
            if (mIsTargeted)
                SetTargeted(true);
            else
                ModelComponent?.ChangeMaterialFloat(Gimmick_RIM_STRENGTH_ID, bTargeted ? 1f : 0f);
        }
    }
    #endregion

    #region ===== :: Event :: =====
    public void DoAddEventActionInvoke(Action<ZGimmick, E_GimmickActionInvokeType> action)
    {
        DoRemoveEventActionInvoke(action);
        mEventActionInvoke += action;
    }

    public void DoRemoveEventActionInvoke(Action<ZGimmick, E_GimmickActionInvokeType> action)
    {
        mEventActionInvoke -= action;
    }

    public void DoAddEventTakeAttribute(Action<ZGimmick, E_UnitAttributeType> action)
    {
        DoRemoveEventTakeAttribute(action);
        mEventTakeAttribute += action;
    }

    public void DoRemoveEventTakeAttribute(Action<ZGimmick, E_UnitAttributeType> action)
    {
        mEventTakeAttribute -= action;
    }
    #endregion
}
