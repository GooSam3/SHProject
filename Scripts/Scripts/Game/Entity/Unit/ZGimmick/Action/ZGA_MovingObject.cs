using System.Collections;
using UnityEngine;

/// <summary> 현재 위치에서 왔다갔다 하는 오브젝트 </summary>
public class ZGA_MovingObject : ZGimmickActionBase
{    
    [Header("목표 위치까지 이동되는 속도")]
    [SerializeField]
    private float MoveToGoalPositionSpeed = 5f;

    [Header("원래 위치로 돌아오는 속도")]
    [SerializeField]
    private float ResetPositionSpeed = 5f;

    [Header("목표위치에서 기다리는 시간")]
    [SerializeField]
    private float MoveToGoalPositionWaitTime = 3f;

    [Header("원래위치로 돌아온 후 기다리는 시간")]
    [SerializeField]
    private float ResetPositionWaitTime = 3f;

    [Header("목표 위치")]
    [SerializeField]
    private Transform TransGoalPosition = null;

    [Header("이동/복귀 관련 타입")]
    [SerializeField]
    private E_MovingObjectMoveType MoveType = E_MovingObjectMoveType.Loop;

    private bool IsMoving = false;

    /// <summary> 이동된 상태 </summary>
    private bool IsMoved = false;

    private Vector3 ResetPosition;
    private Vector3 GoalPosition;

    [Header("물리이동용 트리거")]
    [SerializeField]
    private TriggerArea mTrigerArea;

    [Header("취소시 멈추는게 아니라 원래 위치로 돌아간다.")]
    [SerializeField]
    private bool IsResetByCancel = false;

    private Rigidbody mRBody = null;

    private float WaitEndTime;

    protected override void InitializeImpl()
    {
        base.InitializeImpl();
        ResetPosition = Gimmick.Position;
        GoalPosition = TransGoalPosition.position;

        mRBody = Gimmick.GetComponent<Rigidbody>();

        if(null != mRBody)
        {
            mRBody.useGravity = false;
            mRBody.isKinematic = true;
            mRBody.interpolation = RigidbodyInterpolation.Interpolate;            
            StartCoroutine(LateFixedUpdate());
        }
        else
        {
            ZMonoManager.Instance.AddUpdateCall(ZMonoManager.UpdateMode.NormalUpdate, HandleUpdate);
        }
    }

    protected override void DestroyImpl()
    {
        if (false == ZMonoManager.hasInstance)
            return;

        if (null != mRBody)
        {
            StopCoroutine(LateFixedUpdate());
        }
        else
        {
            ZMonoManager.Instance.RemoveUpdateCall(ZMonoManager.UpdateMode.NormalUpdate, HandleUpdate);
        }
    }

    protected override void InvokeImpl()
    {
        if (true == IsMoving)
            return;

        IsMoving = true;
    }

    protected override void CancelImpl()
    {
        if(IsResetByCancel)
        {
            IsMoved = true;
            IsMoving = true;
        }
        else
        {
            IsMoving = false;
        }        
    }

    private IEnumerator LateFixedUpdate()
    {
        WaitForFixedUpdate _instruction = new WaitForFixedUpdate();
        while (true)
        {
            yield return _instruction;

            HandleUpdate();
        }
    }

    private void HandleUpdate()
    {
        if (false == IsMoving)
            return;

        if (WaitEndTime > Time.time)
            return;

        var dir = GetPosition() - Gimmick.Position;
        var distance = dir.magnitude;

        if (distance <= 0.1f)
        {
            ChangeDir();
            return;
        }

        dir.Normalize();
        var velocity = dir * GetSpeed();
        var offset = velocity * Time.deltaTime;

        if (null != mRBody)
        {
            mRBody.transform.position += offset;      

            if (null != mTrigerArea)
            {
                var list = mTrigerArea.GetEnteredRigidbody();

                foreach (var rBody in list)
                {
                    if (false == rBody.useGravity)
                        rBody.MovePosition(rBody.position + offset);
                    else if (0 > offset.y)
                    {
                        rBody.velocity += offset;
                    }
                }
            }
        }
        else
        {
            Gimmick.transform.position += offset;
        }
    }

    private void ChangeDir()
    {
        IsMoved = !IsMoved;

        WaitEndTime = GetWaitTime() + Time.time;

        switch (MoveType)
        {
            case E_MovingObjectMoveType.PingPong:
                {
                    if (false == IsMoved)
                        IsMoving = false;
                }
                break;
            case E_MovingObjectMoveType.Toggle:
                {
                    IsMoving = false;
                }
                break;
        }

        if (false == IsEnableAction)
            IsMoving = false;
    }

    private Vector3 GetPosition()
    {
        return true == IsMoved ? ResetPosition : GoalPosition;
    }

    private float GetSpeed()
    {
        return true == IsMoved ? ResetPositionSpeed : MoveToGoalPositionSpeed;
    }

    private float GetWaitTime()
    {
        return true == IsMoved ? ResetPositionWaitTime : MoveToGoalPositionWaitTime;
    }
}