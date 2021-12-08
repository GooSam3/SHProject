using UnityEngine;

/// <summary> Entity에 부착되는 component base class </summary>
public abstract class EntityComponentBase<T> : MonoBehaviour where T : MonoBehaviour
{
    public T Owner { get; private set; }
    protected Transform CachedTransform = null;

    /// <summary> Update를 활성화 할 건지 여부 </summary>
    protected virtual bool EnableUpdate => false;
    /// <summary> Late Update를 활성화 할 건지 여부 </summary>
    protected virtual bool EnableLateUpdate => false;
    /// <summary> Fixed Update를 활성화 할 건지 여부 </summary>
    protected virtual bool EnableFixedUpdate => false;

    private bool IsQuit = false;

    public void InitializeComponent(T owner)
    {
        Owner = owner;
        CachedTransform = Owner.transform;

        OnInitializeComponentImpl();
        AddUpdateCall();
    }
        
    private void OnApplicationQuit()
    {        
        IsQuit = true;
    }

    public void OnDestroy()
    {
        if (true == IsQuit)
            return;

        RemoveUpdateCall();
        OnDestroyImpl();
    }

    /// <summary> 활성화된 업데이트 타입에 따라 Update 등록 </summary>
    private void AddUpdateCall()
    {
        if(EnableUpdate)
        {
            ZMonoManager.Instance.AddUpdateCall(ZMonoManager.UpdateMode.NormalUpdate, ComponentUpdate);
        }
        if(EnableLateUpdate)
        {
            ZMonoManager.Instance.AddUpdateCall(ZMonoManager.UpdateMode.LateUpdate, ComponentLateUpdate);
        }
        if(EnableFixedUpdate)
        {
            ZMonoManager.Instance.AddUpdateCall(ZMonoManager.UpdateMode.FixedUpdate, ComponentFixedUpdate);
        }
    }

    /// <summary> 활성화된 업데이트 타입에 따라 Update 해제 </summary>
    private void RemoveUpdateCall()
    {
        if (false == ZMonoManager.hasInstance)
        {
            return;
        }
            

        if (EnableUpdate)
        {
            ZMonoManager.Instance.RemoveUpdateCall(ZMonoManager.UpdateMode.NormalUpdate, ComponentUpdate);
        }
        if (EnableLateUpdate)
        {
            ZMonoManager.Instance.RemoveUpdateCall(ZMonoManager.UpdateMode.LateUpdate, ComponentLateUpdate);
        }
        if (EnableFixedUpdate)
        {
            ZMonoManager.Instance.RemoveUpdateCall(ZMonoManager.UpdateMode.FixedUpdate, ComponentFixedUpdate);
        }
    }

    private void ComponentUpdate()
    {
        if (false == enabled)
        {
            return;
        }
            
        OnUpdateImpl();
    }

    private void ComponentLateUpdate()
    {
        if (false == enabled)
        {
            return;
        }

        OnLateUpdateImpl();
    }

    private void ComponentFixedUpdate()
    {
        if (false == enabled)
        {
            return;
        }

        OnFixedUpdateImpl();
    }

    protected virtual void OnInitializeComponentImpl() { }
    protected virtual void OnDestroyImpl() { }
    protected virtual void OnUpdateImpl() { }
    protected virtual void OnLateUpdateImpl() { }
    protected virtual void OnFixedUpdateImpl() { }
}
