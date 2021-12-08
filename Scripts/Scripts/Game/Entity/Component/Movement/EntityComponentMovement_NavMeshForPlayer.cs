using GameDB;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary> 플레이어 네비메시 이동 </summary>
public class EntityComponentMovement_NavMeshForPlayer : EntityComponentMovement_NavMesh
{
    private Vector3 mCachedDir;
    private float mCachedMoveSpeed;
    private float mLastSendTime;

    private ZPawnMyPc MyPc = null;

    private Vector3? DestPosition = null;

    private const float MIN_SEND_TIME_INTERVAL = 0.1f;
    private const float MAX_SEND_TIME_INTERVAL = 0.5f;

    protected override void OnInitializeComponentImpl()
    {
        base.OnInitializeComponentImpl();

        MyPc = Owner.To<ZPawnMyPc>();

        //스탯 변경 대응
        MyPc?.DoAddEventStatUpdated(HandleStatUpdated);
    }

    protected override void OnDestroyImpl()
    {
        base.OnDestroyImpl();
        MyPc?.DoRemoveEventStatUpdated(HandleStatUpdated);
    }
    
    public override Vector3? MoveTo(Vector3 destPosition, float speed, bool bInputMove)
    {
        if (Owner.IsMovingDir())
            return null;

        IsInputMove = bInputMove;

        NavMeshPath navPath = new NavMeshPath();
        List<Vector3> path = new List<Vector3>();

        DestPosition = destPosition;

        //경로를 생성한다.
        if (mNavAgent.CalculatePath(destPosition, navPath))
        {
            mSpeed = speed;            
            foreach (Vector3 corner in navPath.corners)
            {
                path.Add(corner);
            }
            //해당 경로로 이동을 시작한다.
            return MoveTo(path, speed);
        }

        return null;
    }

    public override Vector3? MoveToDirection(Vector3 curPosition, Vector3 dir, float speed, Vector2 joystickDir)
    {        
        m_queMovePath.Clear();

        mDir = dir;
        mSpeed = speed;
        DestPosition = null;
        IsInputMove = false;

        if (Owner.IsBlockMove())
            return null;

        Vector3? dest = Move(Position + mDir, speed);

        if (null != dest)
        {
            REQMoveToDir(dest.Value);
        }
        else
        {            
            REQStopMove();
        }

        return dest;
    }

    public override void Warp(Vector3 position)
    {
        //TODO :: 순간이동 패킷 처리
        DestPosition = null;
        IsInputMove = false;
        base.Warp(position);
    }

    public override void StopMove(Vector3 curPosition)
    {
        DestPosition = null;
        IsInputMove = false;

        REQStopMove();
        base.StopMove(curPosition);        
    }

    protected override Vector3? MoveNext()
    {
        Vector3? ret = base.MoveNext();
        if (null != ret)
        {
            //이동했다고 알린다.
            ZMmoManager.Instance.Field.REQ_MoveToDest(Owner.EntityId, Position, ret.Value, mSpeed);
        }

        return ret;
    }

    /// <summary>  </summary>
    private void REQStopMove()
    {
        //가만히 있을 경우는 패스
        if ((Vector3.zero == mCachedDir && 0f == mCachedMoveSpeed) && false == IsMoving())
            return;

        mDir = Vector3.zero;
        mSpeed = 0f;
        mCachedDir = Vector3.zero;
        mCachedMoveSpeed = 0f;
        mLastSendTime = 0f;

        ZMmoManager.Instance.Field.REQ_MoveStop(Owner.EntityId, CachedTransform.position);
    }

    /// <summary> TODO :: Player용 MoveComponent쪽으로 옮겨야함. 실제 이동방향으로 dir 셋팅해야함 </summary>
    private void REQMoveToDir(Vector3 dest)
    {
        mDir = (dest - Position).normalized;

        bool bChangeDir = Vector3.Angle(mDir, mCachedDir) > 10;
        //최소 보내는 시간 체크
        if (mLastSendTime + MIN_SEND_TIME_INTERVAL > Time.time)
        {
            //이동 속도가 같거나 방향이 크게 변하지 않았다면 패스
            if (mSpeed == mCachedMoveSpeed && (false == bChangeDir || true == IsRaycastHit))
            {
                return;
            }
        }

        //가만히 있을 경우는 패스
        if (mDir == Vector3.zero && mCachedDir == Vector3.zero)
        {
            return;
        }

        //방향이 같은데 최대 보내는 시간이 지나지 않았을 경우 패스
        if (false == bChangeDir && mLastSendTime + MAX_SEND_TIME_INTERVAL > Time.time)
        {
            return;
        }

        mCachedDir = mDir;
        mCachedMoveSpeed = mSpeed;
        mLastSendTime = Time.time;
        float angle = VectorHelper.Axis3Angle(mDir);

        ZMmoManager.Instance.Field.REQ_MoveToDir(Owner.EntityId, CachedTransform.position, angle, mSpeed, angle);
    }

    #region Event
    private void HandleStatUpdated(Dictionary<E_AbilityType, float> stats)
    {
        foreach(var stat in stats)
        {
            if(stat.Key == E_AbilityType.FINAL_MOVE_SPEED)
            {
                //이속이 변경되었고 내가 목적지를 가지고 이동중일 경우 처리.
                if(null != DestPosition && true == DestPosition.HasValue)
                {
                    MoveTo(DestPosition.Value, MyPc.MoveSpeed, IsInputMove);
                }
                break;
            }
        }
    }
    #endregion
}
