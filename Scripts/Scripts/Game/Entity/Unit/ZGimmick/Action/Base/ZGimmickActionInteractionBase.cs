using UnityEngine;

/// <summary> 인터렉션 관련 액션 </summary>
public abstract class ZGimmickActionInteractionBase<COLLIDER_TYPE> : ZGimmickActionBase where COLLIDER_TYPE : Collider
{
    /// <summary> 상호작용 활성화 여부 </summary>
    protected bool IsEnableInteraction;

    /// <summary> 충돌체 타입 </summary>
    protected COLLIDER_TYPE mCollider;

    private UISubHUDTemple SubHudTemple;

    public Vector3 CenterPosition { get { return mCollider.bounds.center; }  }

    public Vector3 HalfSize { get { return mCollider.bounds.extents; } }

    /// <summary> 변경할 상태 </summary>
    protected abstract E_TempleCharacterControlState ChangeControlStateType { get; }

    protected override void InvokeImpl()
    {
        IsEnableInteraction = true;

        var colliders = Gimmick.GetComponentsInChildren<COLLIDER_TYPE>();

        foreach (var col in colliders)
        {
            if (false == col.isTrigger)
            {
                mCollider = col;
                break;
            }
        }
    }

    protected override void DestroyImpl()
    {
        HideInteractionUI();
    }

    public override void DisableAction()
    {
        IsEnableInteraction = false;
    }

    protected override void CancelImpl()
    {
        ZTempleHelper.CancelCharacterControlState(ChangeControlStateType);
    }
     
    protected void OnTriggerEnter(Collider other)
    {
        if (false == IsEnableInteraction)
            return;

        ZPawnMyPc pc = other.gameObject.GetComponent<ZPawnMyPc>();

        if (null == pc || true == pc.IsRiding)
            return;

        ShowInteractionUI();
    }

    protected void OnTriggerExit(Collider other)
    {
        if (false == IsEnableInteraction)
            return;

        ZPawnMyPc pc = other.gameObject.GetComponent<ZPawnMyPc>();

        if (null == pc)
            return;

        HideInteractionUI();
    }

    /// <summary> 인터렉션 </summary>
    protected abstract void HandleInteraction(TempleCharacterControlStateBase state);
    /// <summary> 인터렉션 ui 활성화 가능 여부 </summary>
    protected virtual bool CheckShowInteractionUI()
    {
        return true;
    }

    /// <summary> 인터렉션 버튼 활성화 </summary>
    private void ShowInteractionUI()
    {
        var pc = ZPawnManager.Instance.MyEntity;
        var state = pc.GetMovement<EntityComponentMovement_Temple>().CurrentState;

        var defaultState = state as TempleCharacterControlState_Default;

        //기본 상태가 아니면 패스
        if (null == defaultState || defaultState.StateType != E_TempleCharacterControlState.Default)
        {
            return;
        }

        //땅에 붙어있는 상태가 아니라면 패스
        if (defaultState.CurrentCharacterState != E_TempleCharacterState.Grounded)
        {
            return;
        }

        if(false == CheckShowInteractionUI())
        {
            return;
        }

        SubHudTemple = UIManager.Instance.Find<UISubHUDTemple>();
        if (null != SubHudTemple)
        {
            SubHudTemple.SetInteractionGimmick(true, () =>
            {
                HideInteractionUI();

                HandleInteraction(state);
            });
        }
        else if(false == ZWebManager.hasInstance || false == ZWebManager.Instance.WebGame.IsUsable)
        {
#if UNITY_EDITOR
            if (Gimmick.mTriggerEnableTimeForEditor > Time.time)
                return;
#endif
            HideInteractionUI();

            HandleInteraction(state);            
        }
    }

    /// <summary> 인터렉션 버튼 비활성화 </summary>
    protected void HideInteractionUI()
    {
        if (null == UIManager.Instance)
            return;

        SubHudTemple?.SetInteractionGimmick(false);
        SubHudTemple = null;
    }
}
