using UnityEngine;

/// <summary> 사당용 캐릭터 컨트롤 상태 </summary>
public abstract class TempleCharacterControlStateBase
{
    public abstract E_TempleCharacterControlState StateType { get; }
    protected ZPawnMyPc mOwner;
    protected EntityComponentMovement_Temple mOwnerComp;
    public Transform CachedTransform { get; private set; }
    public Rigidbody RBody { get; private set; }    
    public Mover mMover { get; private set; }

    public Vector3 Position { get { return CachedTransform.position; } }
    public Quaternion Rotation { get { return CachedTransform.rotation; } }
    

    /// <summary>  이동 방향 </summary>
    protected Vector3 mDir = default;

    /// <summary> 이동 속도 </summary>
    protected float mMoveSpeed = default;

    public virtual bool IsPossibleRide { get { return false; } }

    public void InitializeController(ZPawn pawn, EntityComponentMovement_Temple comp)
    {
        mOwner = pawn.To<ZPawnMyPc>();
        mOwnerComp = comp;

        CachedTransform = mOwner.transform;
        RBody = mOwnerComp.RBody;
        mMover = mOwnerComp.mMover;
    }

    public void DestroyController()
    {
        mOwner?.DoRemoveEventChangeAnimController(HandleEventChangeAnimController);
        mOwner?.DoRemoveEventTakeDamage(HandleEventTakeDamage);
    }

    public void BeginState(params object[] args)
    {
        mOwner.DoAddEventChangeAnimController(HandleEventChangeAnimController);
        mOwner.DoAddEventTakeDamage(HandleEventTakeDamage);
        mOwner.StopCombat();
        BeginStateImpl(args);
    }

    public void EndState()
    {
        mOwner.DoRemoveEventChangeAnimController(HandleEventChangeAnimController);
        mOwner.DoRemoveEventTakeDamage(HandleEventTakeDamage);
        EndStateImpl();
    }

    public void Cancel(bool isForced = false)
    {
        if (true == isForced)
            ForceCancleImpl();
        else
            CancelImpl();
    }

    public void UpdateState()
    {
        UpdateStateImpl();
    }

    public void LateUpdateState()
    {
        LateUpdateStateImpl();
    }

    public void FixedUpdateState()
    {
        FixedUpdateStateImpl();
    }

    public void ChangeState(E_TempleCharacterControlState state, params object[] args)
    {
        mOwnerComp.ChangeCharacterControlState(state, args);
    }

    public virtual bool IsMoving()
    {
        return false;
    }

    public virtual bool IsMovingDir()
    {
        return mDir != Vector3.zero;
    }

    public virtual Vector3? MoveTo(Vector3 destPosition, float speed)
    {
        //mRBody.MovePosition(path);
        return null;
    }

    public virtual Vector3? MoveToDirection(Vector3 curPosition, Vector3 dir, float speed, Vector2 joystickDir)
    {
        mDir = dir.normalized;
        mMoveSpeed = speed;

        return null;
    }

    public virtual void Warp(Vector3 position)
    {
        mDir = default;
        mMoveSpeed = default;


        mOwner.transform.position = position;
        RBody.velocity = Vector3.zero;
        RBody.angularVelocity = Vector3.zero;
    }

    public virtual void StopMove(Vector3 curPosition)
    {
        mDir = default;
        mMoveSpeed = default;

        mOwner.transform.position = curPosition;
        RBody.velocity = Vector3.zero;
        RBody.angularVelocity = Vector3.zero;
    }

    protected abstract void BeginStateImpl(params object[] args);    
    protected abstract void EndStateImpl();
    protected abstract void CancelImpl();

    protected virtual void ForceCancleImpl() { }
    protected virtual void UpdateStateImpl() { }
    protected virtual void LateUpdateStateImpl() { }
    protected virtual void FixedUpdateStateImpl() { }

    /// <summary> 애님 컨트롤러가 변경되었을 때 처리 (모델이 변경될때, 유적일 경우에만 처리됨. - 유적 애니메이터가 변경될때) </summary>
    protected virtual void HandleEventChangeAnimController() { }


    private void HandleEventTakeDamage(uint attackerEntityId, uint amount, bool bCritical, bool bDot)
    {
        Cancel();
    }
}
