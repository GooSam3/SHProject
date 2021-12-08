using FSM;
using System;

public abstract class EntityStateBase : BaseState<ZPawnMyPc>
{
    /// <summary> 업데이트 활성화 여부 </summary>
    protected abstract bool EnableUpdate { get; }

    /// <summary> 업데이트 활성화 여부 </summary>
    protected abstract bool EnableLateUpdate { get; }

    /// <summary> pc 전용 스킬 시스템 </summary>
    protected SkillSystem SkillSystem { get { return Parent.SkillSystem; } }

    public override void OnEnter(Action callback, params object[] args)
    {
        base.OnEnter(callback, args);        
        AddUpdateCall();
    }

    public override void OnExit(Action callback)
    {
        RemoveUpdateCall();
        base.OnExit(callback);
    }

    private void OnDestroy()
    {        
        RemoveUpdateCall();
        DestroyImpl();
    }

    /// <summary> Mono Update 등록 </summary>
    private void AddUpdateCall()
    {
        if (false == ZMonoManager.hasInstance)
        {
            return;
        }

        if (EnableUpdate)
        {
            ZMonoManager.Instance.AddUpdateCall(ZMonoManager.UpdateMode.NormalUpdate, UpdateState);
        }

        if(EnableLateUpdate)
        {
            ZMonoManager.Instance.AddUpdateCall(ZMonoManager.UpdateMode.LateUpdate, LateUpdateState);
        }
    }

    /// <summary> Mono Update 등록 해제 </summary>
    private void RemoveUpdateCall()
    {
        if (false == ZMonoManager.hasInstance)
        {
            return;
        }
            
        if (EnableUpdate)
        {
            ZMonoManager.Instance.RemoveUpdateCall(ZMonoManager.UpdateMode.NormalUpdate, UpdateState);
        }

        if (EnableLateUpdate)
        {
            ZMonoManager.Instance.RemoveUpdateCall(ZMonoManager.UpdateMode.LateUpdate, LateUpdateState);
        }
    }

    private void UpdateState()
    {
        if (false == enabled)
        {
            return;
        }

        UpdateStateImpl();
    }

    private void LateUpdateState()
    {
        if (false == enabled)
        {
            return;
        }

        LateUpdateStateImpl();
    }

    protected virtual void UpdateStateImpl() { }
    protected virtual void LateUpdateStateImpl() { }
    protected virtual void DestroyImpl() { }
}