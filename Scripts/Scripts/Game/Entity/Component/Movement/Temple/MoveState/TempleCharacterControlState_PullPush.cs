using DG.Tweening;
using System.Collections;
using UnityEngine;

/// <summary> 사당용 밀기/당기기 상태 </summary>
public class TempleCharacterControlState_PullPush : TempleCharacterControlStateBase
{
    public override E_TempleCharacterControlState StateType { get { return E_TempleCharacterControlState.PullPush; } }

    private float PushSpeed = 2f;
     
    private float PullSpeed = 2f;

    private ZGimmick mGimmick = null;

    private ZGA_PullPush mPullPush = null;

    private ZGAPullPushTrigger mTrigger = null;

    private Rigidbody mGimmickRBody = null;

    private bool IsPrepared;

    private Coroutine CoPrepare = null;

    /// <summary> 이동 방향 변경 관련 dirty </summary>
    private ZDirty MoveDirDirty = new ZDirty(0.15f);

    private FixedJoint mJoint = null;

    private float DefaultMass = 0f;

    protected override void BeginStateImpl(params object[] args)
    {
        if(null == args || args.Length <= 0)
        {
            ChangeState(E_TempleCharacterControlState.Default);
            return;
        }

        mGimmick = args[0] as ZGimmick;
        mPullPush = args[1] as ZGA_PullPush;
        mTrigger = mPullPush.EnterTrigger;

        if(null == mTrigger)
        {
            ChangeState(E_TempleCharacterControlState.Default);
            return;
        }

        mGimmickRBody = mGimmick.GetComponent<Rigidbody>();
        DefaultMass = mGimmickRBody.mass;

        IsPrepared = false;

        ZGimmickManager.Instance.DoAddEventDespawnGimmick(HandelEventDespawnGimmick);

        if (null != CoPrepare)
            mOwnerComp.StopCoroutine(CoPrepare);

        CoPrepare = mOwnerComp.StartCoroutine(Co_Prepare());
    }

    protected override void EndStateImpl()
    {
        ZGimmickManager.Instance.DoRemoveEventDespawnGimmick(HandelEventDespawnGimmick);

        if (null != CoPrepare)
            mOwnerComp.StopCoroutine(CoPrepare);

        CoPrepare = null;

        mOwner.SetAnimParameter(E_AnimParameter.PullPush_001, false);
        mOwner.SetAnimParameter(E_AnimParameter.Dir, 0f);

        IsPrepared = false;
        mDir = Vector3.zero;

        if(null != mJoint)
        {
            GameObject.Destroy(mJoint);
        }

        mGimmickRBody.useGravity = true;

        mGimmickRBody.mass = DefaultMass;

        //모델 부모 리셋
        var modelGo = mOwner.ModelGo;        
        modelGo.transform.parent = mOwner.transform;
        modelGo.transform.localPosition = Vector3.zero;
        modelGo.transform.localRotation = Quaternion.identity;

        var subHudTemple = UIManager.Instance.Find<UISubHUDTemple>();
        subHudTemple?.ResetControlGimmick();
    }

    private IEnumerator Co_Prepare()
    {        
        Vector3 dir = mPullPush.CenterPosition - mTrigger.transform.position;
        dir.Normalize();

        Vector3 movePos = mTrigger.transform.position - (Vector3.up * mPullPush.HalfSize.y) - dir * 0.7f;

        Vector3 offset = movePos - mGimmick.Position;

        bool bWiatMove = true;

        mOwner.MoveAnim(true);

        mOwner.LookAt(movePos);

        //모델 분리
        var modelGo = mOwner.ModelGo;
        modelGo.transform.parent = null;

        //pawn 순간이동.
        mOwner.Warp(movePos);
        yield return null;

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

        mOwner.LookAt(mPullPush.CenterPosition);

        //시작 애니메이션 처리
        mOwner.SetAnimParameter(E_AnimParameter.PullPush_001, true);
        mOwner.SetAnimParameter(E_AnimParameter.Dir, 0f);

        //TODO :: 조작 풀기

        //mOwner.StopMove(mOwner.Position);

        IsPrepared = true;

        
        mOwnerComp.Invoke("ShowInterectionUI", 0.2f);

        mJoint = mOwner.gameObject.GetOrAddComponent<FixedJoint>();

        mJoint.autoConfigureConnectedAnchor = true;
        mJoint.connectedBody = mGimmickRBody;

        //mJoint.connectedAnchor = offset + Vector3.down * 0.05f;
        mJoint.autoConfigureConnectedAnchor = false;
        mJoint.anchor = Vector3.up * 0.1f;
        mGimmickRBody.useGravity = false;
        mGimmickRBody.mass = RBody.mass;

        //ui 출력
        var subHudTemple = UIManager.Instance.Find<UISubHUDTemple>();
        if (null != subHudTemple)
        {
            subHudTemple.SetControlGimmick(E_TempleUIType.Joystick_CancelButton, () =>
            {
                Cancel();
            });
        }   
    }

	protected override void ForceCancleImpl()
	{
		base.ForceCancleImpl();
        ChangeState(E_TempleCharacterControlState.Default);
    }

	protected override void CancelImpl()
    {
        if (false == IsPrepared)
            return;

        ChangeState(E_TempleCharacterControlState.Default);
    }

    private void HandelEventDespawnGimmick(ZGimmick gimmick)
    {
        if (null == gimmick)
            return;

        if (mGimmick != gimmick)
            return;

        Cancel();
    }

    protected override void UpdateStateImpl()
    {
#if UNITY_EDITOR
        if(Input.GetKeyDown(KeyCode.Space))
        { 
            Cancel();
        }
#endif
    }

    protected override void FixedUpdateStateImpl()
    {
        if (false == IsPrepared)
            return;

        mMover.CheckForGround();

        if(false == mMover.IsGrounded)
        {
            Cancel();
            return;
        }

        //이동 속도 변경
        if (MoveDirDirty.Update())
        {
            mOwner.SetAnimParameter(E_AnimParameter.Dir, MoveDirDirty.CurrentValue);
        }

        if (default !=  mDir)
        {
            Vector3 targetDir = mOwner.transform.forward;// mPullPush.CenterPosition - mOwner.Position;

            targetDir.y = 0f;

            targetDir.Normalize();

            bool bForward = Vector3.Dot(mDir, targetDir) > 0f;
            float speed = bForward ? PushSpeed : PullSpeed;

            Vector3 moveDir = bForward ? targetDir : -targetDir;
            Vector3 _velocity = moveDir * speed;

            mMover.SetVelocity(_velocity);            
            
            if (MoveDirDirty.CurrentValue != MoveDirDirty.GoalValue)
                return;

            if (bForward)
            {
                SetMoveDir(1f);
            }
            else
            {
                SetMoveDir(-1f);
            }
        }
        else
        {
            SetMoveDir(0f);
            mMover.SetVelocity(Vector3.zero);            
            return;
        }
    }

    public override Vector3? MoveToDirection(Vector3 curPosition, Vector3 dir, float speed, Vector2 joystickDir)
    {
        if (IsPrepared)
        {
            mDir = dir.normalized;
            mMoveSpeed = speed;

            if (Vector3.zero == mDir)
            {
                //조작 중지
                mOwnerComp.Invoke("ShowInterectionUI", 0.2f);
            }
        }

        return null;
    }
    
    private void ShowInterectionUI()
    {
        mOwnerComp.CancelInvoke("ShowInterectionUI");        
        //if (ZGameModeBase.HudPanel is HudDungeonPanel panel)
        //{
        //    if (UIManager.GetMyPawn.MyController.PawnInputControl is PawnInputControllerSingle controller)
        //    {
        //        panel.ShowEnableGimmickButton(() =>
        //        {
        //            Cancel();
        //        });
        //    }
        //}
    }

    private void HideInterectionUI()
    {
        mOwnerComp.CancelInvoke("ShowInterectionUI");        
        //if (ZGameModeBase.HudPanel is HudDungeonPanel panel)
        //{
        //    panel.HideEnableGimmickButton();
        //}
    }

    private void SetMoveDir(float value)
    {
        MoveDirDirty.GoalValue = value;
        MoveDirDirty.IsDirty = true;

        //Model.AnimController.SetParameter(E_AnimParameter.CarryDir, value);
    }

    protected override void HandleEventChangeAnimController()
    {
        base.HandleEventChangeAnimController();

        if (IsPrepared)
        {
            mOwner.SetAnimParameter(E_AnimParameter.Dir, MoveDirDirty.CurrentValue);
            mOwner.SetAnimParameter(E_AnimParameter.PullPush_001, true);
        }
    }
}
