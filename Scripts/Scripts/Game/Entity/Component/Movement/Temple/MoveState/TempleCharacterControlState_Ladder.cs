using DG.Tweening;
using System.Collections;
using UnityEngine;

/// <summary> 사당용 사다리 타기 </summary>
public class TempleCharacterControlState_Ladder : TempleCharacterControlStateBase
{
    public override E_TempleCharacterControlState StateType { get { return E_TempleCharacterControlState.Ladder; } }

    private float ClimbingSpeed = 2f;

    private float SlidingSpeed = 7f;

    private ZGA_Ladder mLadder = null;

    private bool IsPrepared = false;

    private Coroutine CoPrepare = null;
    private Coroutine CoEndLadder = null;

    protected override void BeginStateImpl(params object[] args)
    {
        if(null == args || args.Length <= 0)
        {
            ChangeState(E_TempleCharacterControlState.Default);
            return;
        }

        mLadder = args[0] as ZGA_Ladder;

        IsPrepared = false;

        StopMove(mOwner.Position);
        mOwner.SetAnimParameter(E_AnimParameter.Sliding_Ladder_001, false);

        if (null != CoPrepare)
            mOwnerComp.StopCoroutine(CoPrepare);

        if (null != CoEndLadder)
            mOwnerComp.StopCoroutine(CoEndLadder);

        CoPrepare = mOwnerComp.StartCoroutine(Co_Prepare());
        CoEndLadder = null;
    }

    protected override void EndStateImpl()
    {
        if (null != CoPrepare)
            mOwnerComp.StopCoroutine(CoPrepare);

        CoPrepare = null;

        if (null != CoEndLadder)
            mOwnerComp.StopCoroutine(CoEndLadder);

        CoEndLadder = null;

        mOwner.SetAnimParameter(E_AnimParameter.Climbing_001, false);
        mOwner.SetAnimParameter(E_AnimParameter.Sliding_Ladder_001, false);

        IsPrepared = false;
        mDir = Vector3.zero;
    }

    private IEnumerator Co_Prepare()
    {
        Vector3 movePos = mLadder.StartPosition.position;

        bool bWiatMove = true;
        mOwner.IsBlockMoveMyPc = true;

        mOwner.MoveAnim(true);
        mOwner.transform.forward = mLadder.StartPosition.forward;
        //mOwner.LookAt(movePos);

        //모델 분리
        var modelGo = mOwner.ModelGo;
        modelGo.transform.parent = null;

        //pawn 순간이동.
        mOwner.Warp(movePos);

        //모델 이동
        modelGo.transform.DOMove(movePos, 0.5f).OnComplete(() =>
        {
            bWiatMove = false;
        });

        while (bWiatMove)
        {
            yield return null;
        }

        mOwner.MoveAnim(false);

        //모델 부모 리셋
        modelGo.transform.parent = mOwner.transform;
        modelGo.transform.localPosition = Vector3.zero;
        modelGo.transform.localRotation = Quaternion.identity;

        //mOwner.LookAt(mLadder.Gimmick.Position);
        mOwner.transform.forward = mLadder.StartPosition.forward;

        mOwner.SetAnimParameter(E_AnimParameter.Climbing_001, true);

        mOwner.IsBlockMoveMyPc = false;

        IsPrepared = true;
    }

    protected override void CancelImpl()
    {
        ChangeState(E_TempleCharacterControlState.Default);
    }

    protected override void UpdateStateImpl()
    {
        if (false == IsPrepared)
            return;

        if (default != mDir)
        {
            bool bForward = 0 < mDir.y;
            float speed = bForward ? ClimbingSpeed : SlidingSpeed;

            Vector3 moveDir = bForward ? Vector3.up : Vector3.down;
            Vector3 _velocity = moveDir * speed;

            mMover.SetVelocity(_velocity);

            if (bForward)
            {
                mOwner.SetAnimParameter(E_AnimParameter.Sliding_Ladder_001, false);
                mOwner.MoveAnim(true);
            }
            else
            {
                mOwner.SetAnimParameter(E_AnimParameter.Sliding_Ladder_001, true);
                mOwner.MoveAnim(false);
                mMover.CheckForGround();
                //바닥이면 사다리 내리기
                if(mMover.IsGrounded)
                {
                    ChangeState(E_TempleCharacterControlState.Default);
                }
            }
        }
        else
        {
            mOwner.SetAnimParameter(E_AnimParameter.Sliding_Ladder_001, false);
            mOwner.MoveAnim(false);
        }
    }

    public override Vector3? MoveToDirection(Vector3 curPosition, Vector3 dir, float speed, Vector2 joystickDir)
    {
        if (IsPrepared)
        {
            mDir = joystickDir;

            if (Vector3.zero == mDir)
            {
                //조작 중지 - 애니메이션 멈추기
                
            }
        }

        return null;
    }    
    
    /// <summary> 사다리 타기 종료 </summary>
    public void EndLadder()
    {
        if (false == IsPrepared)
            return;

        IsPrepared = false;
        CoEndLadder = mOwnerComp.StartCoroutine(Co_EndLadder());
    }

    private IEnumerator Co_EndLadder()
    {
        mOwner.SetAnimParameter(E_AnimParameter.Sliding_Ladder_001, false);
        mOwner.SetAnimParameter(E_AnimParameter.Climbing_001, false);
        mOwner.SetAnimParameter(E_AnimParameter.Climbing_End_001);

        mOwner.MoveAnim(false);

        float duration = mOwner.GetAnimLength(E_AnimStateName.Climbing_End_001);

        Vector3 movePos = mLadder.EndPosition.position;

        bool bWiatMove = true;

        //모델 분리
        var modelGo = mOwner.ModelGo;
        modelGo.transform.parent = null;

        CameraManager.Instance.DoSetTarget(modelGo.transform);

        //pawn 순간이동.
        mOwner.Warp(movePos);

        //모델 이동
        modelGo.transform.DOMove(movePos, duration).OnComplete(() =>
        {
            bWiatMove = false;
        });

        while (bWiatMove)
        {
            yield return null;
        }

        //모델 부모 리셋
        modelGo.transform.parent = mOwner.transform;
        modelGo.transform.localPosition = Vector3.zero;
        modelGo.transform.localRotation = Quaternion.identity;

        CameraManager.Instance.DoSetTarget(mOwner.transform);

        yield return new WaitForSeconds(0.1f);

        ChangeState(E_TempleCharacterControlState.Default);
    }

    protected override void HandleEventChangeAnimController()
    {
        base.HandleEventChangeAnimController();

        if (IsPrepared)
        {
            mOwner.SetAnimParameter(E_AnimParameter.Climbing_001, true);
        }
    }
}
