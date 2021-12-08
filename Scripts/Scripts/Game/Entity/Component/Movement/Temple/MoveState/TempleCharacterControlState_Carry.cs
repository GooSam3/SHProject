using DG.Tweening;
using GameDB;
using System.Collections;
using UnityEngine;

/// <summary> 사당용 물건 옮기기 </summary>
public class TempleCharacterControlState_Carry : TempleCharacterControlState_Default
{
    public override E_TempleCharacterControlState StateType { get { return E_TempleCharacterControlState.Carry; } }


    /// <summary> 던지기시 발생할 힘 </summary>
    private const float THROW_POWER = 10f;
         
    /// <summary> 이동시 스피드 </summary>
    private float CarrySpeed = 2f;
    
    /// <summary> 이동시킬 기믹 </summary>
    private ZGimmick mGimmick = null;
    private Rigidbody mGimmickRBody = null;

    private bool IsPrepared = false;
    private bool IsPreparing = false;

    private Coroutine CoPrepare = null;
    private Coroutine CoPutDown = null;
    private Coroutine CoThrow = null;

    private float DefaultDrag = 0f;
    private float DefaultAngularDrag = 0f;
        
    /// <summary> DoTween 연출 </summary>
    private Tweener TweenCarry;

    private float mGimmickOffsetY;
    private float mGimmickOffsetZ;

    private Vector3 CachedDir = default;

    private int DefaultLayer;
    private float GimmickWeight;

    private bool IsSetWeight = false;

    protected override void BeginStateImpl(params object[] args)
    {
        if(null == args || args.Length <= 0)
        {
            ChangeState(E_TempleCharacterControlState.Default);
            return;
        }
        base.BeginStateImpl(args);
        //CurrentCharacterState = E_TempleCharacterState.Grounded;

        mGimmick = args[0] as ZGimmick;

        //레이어 변경
        DefaultLayer = mGimmick.gameObject.layer;        
        mGimmick.gameObject.layer = UnityConstants.Layers.IgnoreCollision;

        ZGimmickManager.Instance.DoAddEventDespawnGimmick(HandelEventDespawnGimmick);

        //기본 무게 셋팅
        GimmickWeight = mGimmick.Weight;

        mGimmickRBody = mGimmick.GetComponent<Rigidbody>();
        CachedDir = default;

        DefaultDrag = mGimmickRBody.drag;
        DefaultAngularDrag = mGimmickRBody.angularDrag;
        
        IsPrepared = false;
        IsPreparing = false;
        mGimmickOffsetY = 0f;
        mGimmickOffsetZ = 0f;

        TweenCarry?.Kill(false);

        ResetCoroutine();

        var subHudTemple = UIManager.Instance.Find<UISubHUDTemple>();
        subHudTemple?.SetControlGimmick(E_TempleUIType.Joystick_CancelActionButton, PutDown, Throw);

        CoPrepare = mOwnerComp.StartCoroutine(Co_Prepare(args));
    }
        
    protected override void EndStateImpl()
    {
        ZGimmickManager.Instance.DoRemoveEventDespawnGimmick(HandelEventDespawnGimmick);

        ResetCoroutine();

        mOwner.SetAnimParameter(E_AnimParameter.Lift_001, false);
        mOwner.SetAnimParameter(E_AnimParameter.Lift_Start_001, false);

        SetRBody(false);

        if(null != mGimmick)
        {
            mGimmick.transform.parent = null;
            //레이어 원복
            mGimmick.gameObject.layer = DefaultLayer;
        }

        ResetWeight();

        mGimmick = null;
        mGimmickRBody = null;

        IsPrepared = false;
        IsPreparing = false;
        mDir = Vector3.zero;

        TweenCarry?.Kill(false);

        var subHudTemple = UIManager.Instance.Find<UISubHUDTemple>();
        subHudTemple?.ResetControlGimmick();

        base.EndStateImpl();
    }

    private void HandelEventDespawnGimmick(ZGimmick gimmick)
    {
        if (null == gimmick)
            return;

        if (mGimmick != gimmick)
            return;

        Cancel();
    }

    /// <summary> 기믹과 캐릭터의 무게 셋팅 </summary>
    private void SetWeight()
    {
        if (true == IsSetWeight)
            return;

        //캐릭터 무게 변경
        mOwner.Weight = mOwner.Weight + GimmickWeight;
        //기믹 무게 변경
        mGimmick.Weight = 0f;

        IsSetWeight = true;
    }

    /// <summary> 기믹과 캐릭터의 무게 원복 </summary>
    private void ResetWeight()
    {
        if (false == IsSetWeight)
            return;

        //캐릭터 무게 변경
        mOwner.Weight = mOwner.Weight - GimmickWeight;
        //기믹 무게 변경
        mGimmick.Weight = GimmickWeight;

        IsSetWeight = false;
    }

    private void ResetCoroutine()
    {
        if (null != CoPrepare)
            mOwnerComp.StopCoroutine(CoPrepare);

        if (null != CoPutDown)
            mOwnerComp.StopCoroutine(CoPutDown);

        if (null != CoThrow)
            mOwnerComp.StopCoroutine(CoThrow);

        CoPrepare = null;
        CoPutDown = null;
        CoThrow = null;
    }

    protected override void CancelImpl()
    {
        ChangeState(E_TempleCharacterControlState.Default);
    }

    private void SetRBody(bool bCarry)
    {
        if (bCarry)
        {
            mGimmickRBody.useGravity = false;
            mGimmickRBody.detectCollisions = false;
            mGimmickRBody.isKinematic = true;
            mGimmickRBody.interpolation = RigidbodyInterpolation.None;
            mGimmickRBody.drag = 0;
            mGimmickRBody.angularDrag = 0;

            var collider = mGimmick.GetComponent<Collider>();
            collider.isTrigger = true;

            if(collider is BoxCollider box)
            {                
                mGimmickOffsetY = (box.size.y * 0.5f) - box.center.y;
                mGimmickOffsetZ = box.size.z;

                mGimmickOffsetY *= collider.transform.localScale.y;
                mGimmickOffsetZ *= collider.transform.localScale.z;
            }
            else if(collider is CapsuleCollider capsule)
            {
                mGimmickOffsetY = (capsule.height * 0.5f) - capsule.center.y;
                mGimmickOffsetZ = capsule.radius;

                mGimmickOffsetY *= collider.transform.localScale.y;
                mGimmickOffsetZ *= collider.transform.localScale.z;
            }
            else if(collider is SphereCollider sphere)
            {
                mGimmickOffsetY = sphere.radius - sphere.center.y;
                mGimmickOffsetZ = sphere.radius;

                mGimmickOffsetY *= collider.transform.localScale.y;
                mGimmickOffsetZ *= collider.transform.localScale.z;
            }
            else
            {
                mGimmickOffsetY = 0f;
                mGimmickOffsetZ = collider.bounds.extents.z;
                mGimmickOffsetZ += mGimmickOffsetZ * 0.5f;
            }
        }
        else
        {
            mGimmickRBody.useGravity = true;
            mGimmickRBody.detectCollisions = true;
            mGimmickRBody.isKinematic = false;
            mGimmickRBody.interpolation = RigidbodyInterpolation.Interpolate;
            mGimmickRBody.drag = DefaultDrag;
            mGimmickRBody.angularDrag = DefaultAngularDrag;

            var collider = mGimmick.GetComponent<Collider>();
            collider.isTrigger = false;

            mGimmickOffsetY = 0f;
            mGimmickOffsetZ = 0f;
        }
    }
    
    private IEnumerator Co_Prepare(params object[] args)
    {

        float duration = mOwner.GetAnimLength(E_AnimStateName.Lift_Start_001);

        IsPreparing = true;
        SetRBody(true);

        mOwner.StopMove();

        mOwner.LookAt(mGimmick.transform);

        mOwner.SetAnimParameter(E_AnimParameter.Lift_Start_001, true);
        mOwner.SetAnimParameter(E_AnimParameter.Lift_001, false);
        yield return null;
        mOwner.SetAnimParameter(E_AnimParameter.Lift_001, true);

        //물건 붙이기
        mGimmick.transform.parent = mOwner.transform;
        mGimmick.transform.DOLocalRotate(Vector3.zero, 0.5f);
        TweenCarry = mGimmick.transform.DOLocalMove(new Vector3(0f, mGimmickOffsetY, mGimmickOffsetZ * 0.5f), 0.5f);
                
        yield return new WaitForSeconds(0.5f);

        //무게 변경
        SetWeight();

        mGimmick.transform.localRotation = Quaternion.identity;
        TweenCarry?.Kill(false);
        TweenCarry = mGimmick.transform.DOLocalMove (new Vector3(0f, 1f + mGimmickOffsetY, mGimmickOffsetZ * 0.5f), 1f);

        yield return new WaitForSeconds(duration);

        mOwner.SetAnimParameter(E_AnimParameter.Lift_Start_001, false);


        IsPreparing = false;
        IsPrepared = true;              
    }

    /// <summary> 내려놓기 </summary>
    private void PutDown()
    {
        if (false == IsPrepared)
            return;

        IsPrepared = false;

        mOwner.StopMove();
        mOwner.SetAnimParameter(E_AnimParameter.Lift_001, false);
        mOwner.SetAnimParameter(E_AnimParameter.Lift_Start_001, false);
        mOwner.SetAnimParameter(E_AnimParameter.Lift_End_001);

        if (null != CoPutDown)
            mOwnerComp.StopCoroutine(CoPutDown);

        CoPutDown = mOwnerComp.StartCoroutine(Co_PutDown());

    }

    private IEnumerator Co_PutDown()
    {
        float duration = mOwner.GetAnimLength(E_AnimStateName.Lift_End_001);

        yield return new WaitForSeconds(0.5f);

        var putdownVector = new Vector3(0f, mGimmickOffsetY, mGimmickOffsetZ * 1.5f);
//#if UNITY_EDITOR
//        putdownVector = new Vector3(0f, mGimmickOffsetY, mGimmickOffsetZ * 4);
//#endif

        TweenCarry?.Kill(false);
        TweenCarry = mGimmick.transform.DOLocalMove(putdownVector, 0.5f);

        yield return new WaitForSeconds(duration - 0.5f);


#if UNITY_EDITOR
        //에디터에서 유적 씬에서 바로 플레이할 경우 내려놓고 바로 드는 문제 예외처리
        if (null != mGimmick && (false == ZWebManager.hasInstance || false == ZWebManager.Instance.WebGame.IsUsable))
        {
            mGimmick.mTriggerEnableTimeForEditor = Time.time + 2f;
        }
#endif

        ChangeState(E_TempleCharacterControlState.Default);
    }

    /// <summary> 던지기 </summary>
    private void Throw()
    {
        if (false == IsPrepared)
            return;

        IsPrepared = false;

        mOwner.StopMove();
        mOwner.SetAnimParameter(E_AnimParameter.Lift_001, false);
        mOwner.SetAnimParameter(E_AnimParameter.Lift_Start_001, false);
        mOwner.SetAnimParameter(E_AnimParameter.Throw_001);

        if (null != CoThrow)
            mOwnerComp.StopCoroutine(CoThrow);

        CoThrow = mOwnerComp.StartCoroutine(Co_Throw());
    }

    private IEnumerator Co_Throw()
    {
        float duration = mOwner.GetAnimLength(E_AnimStateName.Throw_001);

        mGimmick.transform.parent = mOwner.GetSocket(E_ModelSocket.Riding);

        yield return new WaitForSeconds(0.7f);

        float powerRate = 1f;
        var carryActionComp = mGimmick.GetComponentInChildren<ZGA_Carry>();

        if (null != carryActionComp)
            powerRate = carryActionComp.ThrowPowerRate;

        Vector3 velocity = (mOwner.transform.forward + Vector3.up * 0.5f).normalized;
        velocity *= THROW_POWER;
        velocity *= powerRate;
        //방향키를 누르고 있으면 1.5배!
        if ((CachedDir != Vector3.zero))
            velocity *= 1.5f;

        SetRBody(false);
        mGimmick.transform.parent = null;
        mGimmickRBody.velocity  = velocity;

        ResetWeight();

        yield return new WaitForSeconds(duration - 0.5f);

        ChangeState(E_TempleCharacterControlState.Default);
    }

    protected override void UpdateStateImpl()
    {
        base.UpdateStateImpl();

#if UNITY_EDITOR
        if (Input.GetKey(KeyCode.R))
        {
            PutDown();
        }
        else if(Input.GetKey(KeyCode.F))
        {
            Throw();
        }
#endif
    }

    protected override void LateUpdateStateImpl()
    {
        base.LateUpdateStateImpl();
        if (CurrentCharacterState != E_TempleCharacterState.Grounded && CurrentCharacterState != E_TempleCharacterState.Sliding)
            Cancel();
    }
    public override Vector3? MoveTo(Vector3 destPosition, float speed)
    {
        if (false == IsPrepared)
            return null;

        return base.MoveTo(destPosition, CarrySpeed);
    }

    public override void StopMove(Vector3 curPosition)
    {
        base.StopMove(curPosition);
        CachedDir = default;
    }

    public override Vector3? MoveToDirection(Vector3 curPosition, Vector3 dir, float speed, Vector2 joystickDir)
    {
        CachedDir = dir;

        if (false == IsPrepared)
            return null;

        return base.MoveToDirection(curPosition, dir, CarrySpeed, joystickDir);
    }

    protected override void HandleEventChangeAnimController()
    {
        base.HandleEventChangeAnimController();

        if(IsPrepared || IsPreparing)
        {
            mOwner.SetAnimParameter(E_AnimParameter.Lift_Start_001, false);
            mOwner.SetAnimParameter(E_AnimParameter.Lift_001, true);
        }
    }
}
