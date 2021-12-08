using GameDB;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary> 네비메시 이동 </summary>
public class EntityComponentMovement_NavMesh : EntityComponentMovementBase
{
    /// <summary> Normal Update 등록 </summary>
    protected override bool EnableUpdate => true;

    public override bool IsPossibleRide => true;

    protected NavMeshAgent mNavAgent = null;

    protected Vector3 mDir = Vector3.zero;
    protected float mSpeed = 0f;

    protected bool IsMoveState = false;
        
    protected bool IsSyncRemoteEntityPosition;

    /// <summary> 예상되는 현재 위치 </summary>
    private Vector3? PredictionCurrentPosition = null;

    protected Queue<Vector3> m_queMovePath = new Queue<Vector3>();

    /// <summary> 방향키 이동시 히트 여부 </summary>
    protected bool IsRaycastHit { get; private set; }
    
    public override bool IsMovingDir()
    {
        return mDir != Vector3.zero;
    }

    protected override void OnInitializeComponentImpl()
    {
        base.OnInitializeComponentImpl();

        mNavAgent = Owner.gameObject.GetOrAddComponent<NavMeshAgent>();
        mNavAgent.enabled = true;
        mNavAgent.updateRotation = true;
        mNavAgent.angularSpeed = 720;
        mNavAgent.acceleration = 50f;
        mNavAgent.stoppingDistance = 0.1f;
        mNavAgent.autoBraking = true;
        mNavAgent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
		mNavAgent.avoidancePriority = 99;
	}   

    protected override void OnDestroyImpl()
    {
        base.OnDestroyImpl();        
    }

    protected override void OnUpdateImpl()
    {
        if (false == mNavAgent.isOnNavMesh)
        {
            return;
        }

        if (mNavAgent.pathPending)
        {
            return;
        }

        if (mNavAgent.remainingDistance > STOPPING_DISTANCE)
        {
            return;
        }

        if (mDir != Vector3.zero)
        {
            if (IsSyncRemoteEntityPosition)
            {
                IsSyncRemoteEntityPosition = false;
                InvokeMoveToDirection();
            }
            return;
        }

        if (IsMoveState)
        {
            SetMoveState(false);
        }
    }
    
    public override bool IsMoving()
    {
        return IsMoveState;
    }

    public override Vector3? MoveTo(Vector3 destPosition, float speed, bool bInputMove)
    {
        return MoveTo(new List<Vector3>() { destPosition }, speed);
    }

    public override Vector3? MoveTo(List<Vector3> path, float speed)
    {
        InitMoveData();

        m_queMovePath = new Queue<Vector3>(path);
        mSpeed = speed;

        return MoveNext();
    }

    protected virtual Vector3? MoveNext()
    {
        if (0 >= m_queMovePath.Count)
        {
            mNavAgent.autoBraking = true;
            return null;
        }

        Vector3? goal = null;

        while (0 < m_queMovePath.Count)
        {
            Vector3 dest = m_queMovePath.Dequeue();
            goal = Move(dest, mSpeed, true);
            if (null != goal)
            {
                mNavAgent.autoBraking = m_queMovePath.Count <= 1;
                return goal;
            }
        }

        return null;
    }

    public override Vector3? MoveToDirection(Vector3 curPosition, Vector3 dir, float speed, Vector2 joystickDir)
    {
        bool bBlockMove = Owner.IsBlockMove();
        InitMoveData(bBlockMove);

        if (true == bBlockMove)
            return null;

        mDir = dir;
        mSpeed = speed;

        float distance = (curPosition - Position).magnitude;
        PredictionCurrentPosition = curPosition + (dir * speed * 0.1f);        

        if (distance > 5f)
        {
            PredictionCurrentPosition = curPosition;
            mNavAgent.Warp(curPosition);
            CancelInvokeMoveToDirection();
            IsSyncRemoteEntityPosition = true;
        }
        else if (distance > 1f)
        {
            PredictionCurrentPosition = Move(PredictionCurrentPosition.Value, distance * 10f);
            CancelInvokeMoveToDirection();
            IsSyncRemoteEntityPosition = true;
        }
        else
        {
            if(false == IsInvoking("InvokeMoveToDirection") && false == IsSyncRemoteEntityPosition)
            {                
                SetSpeed(mSpeed);
                mNavAgent.SetDestination(curPosition);

                PredictionCurrentPosition = curPosition;
                IsSyncRemoteEntityPosition = true;
            }
        }

        return PredictionCurrentPosition;
    }

    public override void ForceMove(Vector3 dest, float duration, E_PosMoveType moveType)
    {
        InitMoveData();

        if (NavMesh.SamplePosition(dest, out var hit, 5f, NavMesh.AllAreas))
            dest = hit.position;
        else
            ZLog.LogError(ZLogChannel.Entity, $"[목표 위치 : {dest}] SamplePosition 실패!!!!");


        if (0 < duration)
        {
            var dist = (dest - Owner.Position).magnitude;

            if (moveType == E_PosMoveType.TargetKnockBack)
            {
                mNavAgent.updateRotation = false;
                Invoke(nameof(InvokeResetNavAgentRotation), duration + 0.3f);
            }

            MoveTo(dest, dist / duration, false);
        }
        else
        {
            Warp(dest);
        }         
    }

    private void InvokeResetNavAgentRotation()
	{
        CancelInvoke(nameof(InvokeResetNavAgentRotation));
        mNavAgent.updateRotation = true;
    }

    public override void Warp(Vector3 position)
    {
        InitMoveData();

        mNavAgent.Warp(position);
    }

    public override void StopMove(Vector3 curPosition)
    {
        InitMoveData();

        if (mNavAgent.isOnNavMesh)
        {
            float distance = (curPosition - Position).magnitude;
            if (distance < 0.1f)
            {
                mNavAgent.SetDestination(curPosition);
                SetMoveState(false);
            }
            else if(distance < 5f)
            {                
                SetSpeed(10);
                mNavAgent.SetDestination(curPosition);
                SetMoveState(true);
            }
            else
            {
                SetMoveState(false);
                mNavAgent.Warp(curPosition);
            }
        }
    }

    protected Vector3? Move(Vector3 dest, float speed, bool bForce = false)
    {
        IsRaycastHit = true;

        if (mNavAgent.isOnNavMesh)
        {   
            if (false == bForce)
            {
                int count = 10;
                NavMeshHit hit;
                while (mNavAgent.Raycast(dest, out hit) && 0 < count)
                {
                    Vector3 dir = (dest - Position).normalized;
                    Vector3 normal = hit.normal;

                    IsRaycastHit = true;

                    dest = hit.position + Vector3.Lerp(dir, normal, Time.deltaTime * 10f);
                    --count;                    
                }

                if(mNavAgent.Raycast(dest, out hit))
                {
                    dest = hit.position;
                    IsRaycastHit = true;
                }
            }

            if ((dest - Position).magnitude <= 0.1f)
            {
                SetMoveState(false);
                return null;
            }
                                    
            SetSpeed(speed);
            mNavAgent.SetDestination(dest);
            SetMoveState(true);
            return dest;
        }

        return dest;
    }

    private void InitMoveData(bool bCancelInvoke = true)
    {
        if(bCancelInvoke)
        {
            CancelInvokeMoveToDirection();
        }

        m_queMovePath.Clear();
        mDir = Vector3.zero;
    }

    /// <summary> 예상되는 현재 위치와 방향으로 지속 이동 시킨다. </summary>
    private void InvokeMoveToDirection()
    {
        CancelInvokeMoveToDirection();

        if (null == PredictionCurrentPosition)
            return;
        
        Vector3? nextPosition = PredictionCurrentPosition.Value + mDir * 2f;
        nextPosition = Move(nextPosition.Value, mSpeed);

        if (null != nextPosition)
        {
            PredictionCurrentPosition = nextPosition.Value + (mDir * 0.1f * mSpeed);
        }
            
        Invoke("InvokeMoveToDirection", 0.1f);
    }

    private void CancelInvokeMoveToDirection()
    {
        CancelInvoke("InvokeMoveToDirection");        
    }

    protected void SetMoveState(bool bMove)
    {        
        if (IsMoveState == bMove)
            return;

        IsMoveState = bMove;                        
        
        if(false == bMove)
        {
            Vector3? next = MoveNext();

            if(null == next)
            {
                IsInputMove = false;
                Owner.MoveAnim(false);
                mEventMoveState?.Invoke(bMove);
            }
        }
        else
        {
            Owner.MoveAnim(true);
            mEventMoveState?.Invoke(bMove);
        }   
    }

    public override void SetSpeed(float speed)
    {        
        Owner.SetAnimMoveSpeed(speed);
        mNavAgent.speed = speed;
    }

    /// <summary> 모델이 변경되었을 때 처리 </summary>
    public override void OnChangeModel()
    {
        //이동중이면 애니메이션을 변경한다.
        if(IsMoveState)
        {
            Owner.MoveAnim(true, true);
        }        
    }
}
