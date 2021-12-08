using System.Collections;
using UnityEngine;

/// <summary> 밀고 당기기 연출을 시작한다. </summary>
public class ZGA_PullPush : ZGimmickActionInteractionBase<BoxCollider>
{
    protected override E_TempleCharacterControlState ChangeControlStateType { get { return E_TempleCharacterControlState.PullPush; } }

    private Rigidbody mRBody;

    /// <summary> 셋팅된 트리거 </summary>
    public ZGAPullPushTrigger EnterTrigger { get; private set; }

    private void Start()
    {
        mRBody = gameObject.GetComponentInParent<Rigidbody>();
        mRBody.useGravity = true;
        mRBody.isKinematic = false;
        mRBody.mass = 100;
        mRBody.drag = 1;
        mRBody.angularDrag = 1f;
        mRBody.interpolation = RigidbodyInterpolation.Interpolate;
    }


    protected override void HandleInteraction(TempleCharacterControlStateBase state)
    {
        //밀기/당기기 상태로 변경
        state.ChangeState(E_TempleCharacterControlState.PullPush, Gimmick, this);

        StartCoroutine(Co_BoxCast());
    }

    protected override bool CheckShowInteractionUI()
    {
        //상자가 움직이는 중이면 패스
        // NOTE(JWK): 사용자 입장에서 눈에 보이지 않는 물리효과의 '힘' 보다 작은지 체크
        float minimalVector = 0.01f;
        if (minimalVector < mRBody.velocity.magnitude || minimalVector < mRBody.angularVelocity.magnitude )
		{
			return false;
		}

		return true;
    }

    private IEnumerator Co_BoxCast()
    {
        var timeInstruction = new WaitForSeconds(0.1f);

        while(true)
        {
            Vector3 center = transform.position + Vector3.up * 0.5f;
            if(false == Physics.Raycast(center, Vector3.down, out var hit, 0.7f))
            {
                CancelPullPush();
                yield break;
            }
            yield return timeInstruction;
        }
    }

    /// <summary> PushPull 액션을 취소 한다. </summary>
    public void CancelPullPush()
    {
        HideInteractionUI();
        var pc = ZPawnManager.Instance.MyEntity;
        var state = pc.GetMovement<EntityComponentMovement_Temple>().CurrentState;
        if (state is TempleCharacterControlState_PullPush pullPushState)
        {
            pullPushState.Cancel();
        }

        StopAllCoroutines();
    }

    public override void DisableAction()
    {
        base.DisableAction();
        CancelPullPush();
    }

    public void TriggerEnter(ZGAPullPushTrigger trigger, Collider other)
    {
        if (false == CheckTrigger(trigger))
            return;

        EnterTrigger = trigger;

        OnTriggerEnter(other);
    }

    public void TriggerExit(ZGAPullPushTrigger trigger, Collider other)
    {
        if (trigger != EnterTrigger)
            return;

        EnterTrigger = null;

        OnTriggerExit(other);
    }

    /// <summary> 사용가능한 트리거인지 판단 </summary>
    private bool CheckTrigger(ZGAPullPushTrigger trigger)
    {
        //기믹이 기울어졌는지 판단
        if (false == CheckGimmick())
            return false;

        var dir = trigger.transform.position - CenterPosition;

        var angle = Vector3.Angle(Vector3.up, dir.normalized);

        //트리거가 위나 아래에 있다면
        if(CheckAngle(angle))
            return false;

        return true;
    }

    /// <summary> 기믹이 기울어지지 않았는지 판단. </summary>
    private bool CheckGimmick()
    {
        var angle = Vector3.Angle(Vector3.up, Gimmick.transform.up);

        if (CheckAngle(Vector3.Angle(Vector3.up, Gimmick.transform.up)))
            return true;

        if (CheckAngle(Vector3.Angle(Vector3.up, Gimmick.transform.forward)))
            return true;

        if (CheckAngle(Vector3.Angle(Vector3.up, Gimmick.transform.right)))
            return true;

        return false;
    }

    private bool CheckAngle(float angle)
    {
        if (5 >= angle || 175 <= angle)
            return true;

        return false;
    }
}