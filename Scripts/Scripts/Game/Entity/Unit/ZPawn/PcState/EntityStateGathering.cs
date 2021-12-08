using GameDB;
using System;
using UnityEngine;

public class EntityStateGathering : EntityStateBase
{
    /// <summary> 업데이트 활성화 여부 </summary>
    protected override bool EnableUpdate  => false;

    /// <summary> 업데이트 활성화 여부 </summary>
    protected override bool EnableLateUpdate => true;

    private EntityBase mCachedTarget;

    private Object_Table mObjectTable;

    public override void OnEnter(Action callback, params object[] args)
    {
        base.OnEnter(callback, args);
        CachedMyPosition = null;
        GoalPosition = null;
        SetTarget();
    }

    private void SetTarget()
    {
        mCachedTarget = Parent.GetTarget();

        if(null == mCachedTarget || mCachedTarget.EntityType != E_UnitType.Object)
        {
            Parent.ChangeState(E_EntityState.Empty);
            return;
        }

        DBObject.TryGet(mCachedTarget.TableId, out mObjectTable);        
    }
    /// <summary> 다음 이동 딜레이 타임 </summary>
    private const float MOVE_DELAY_TIME = 0.2f;
    /// <summary> 마지막 이동 시간 </summary>
    private float mLastMoveTime = 0f;
    /// <summary> 목적지 </summary>
    private Vector3? GoalPosition = null;
    /// <summary> 내 포지션 캐싱 </summary>
    private Vector3? CachedMyPosition = null;
    protected override void LateUpdateStateImpl()
    {
        if (Parent.IsDead)
        {
            Parent.ChangeState(E_EntityState.Empty);
            return;
        }

        //타겟 체크
        if (null == mCachedTarget)
        {
            Parent.StopMove(Parent.transform.position);
            Parent.ChangeState(E_EntityState.Empty);            
            return;
        }
        
        //방향키로 이동
        if (Parent.IsMovingDir())
        {
            Parent.ChangeState(E_EntityState.Empty);
            return;
        }

        Vector3 ownerPosition = Parent.Position;
        Vector3 targetPosition = mCachedTarget.Position;

        //사거리 체크 후 이동 처리
        float radius =  Mathf.Max(mObjectTable.CollisionRadius - 0.2f, 0.8f);
        float distance = (ownerPosition - targetPosition).magnitude;
        
        if (radius < distance)
        {
            //마지막 이동시간에서 딜레이 시간 처리
            if (mLastMoveTime + MOVE_DELAY_TIME > Time.time)
            {
                return;
            }

            if (null != GoalPosition && GoalPosition == targetPosition && null != CachedMyPosition && CachedMyPosition != ownerPosition)
            {
                return;
            }

            if (Parent.IsMezState(E_ConditionControl.NotMove))
            {
                //ShowMezNotify(E_ConditionControl.NotMove);
                return;
            }

            CachedMyPosition = ownerPosition;
            GoalPosition = targetPosition;
            //이동
            Parent.MoveTo(targetPosition, Parent.MoveSpeed);
            mLastMoveTime = Time.time;
            return;
        }

        GoalPosition = null;
        CachedMyPosition = null;

        //이동 정지
        Parent.StopMove(ownerPosition);

        //채집
        ReqGathering();
    }

    /// <summary> 채집 요청 </summary>
    private void ReqGathering()
    {
        Parent.IsGathering = true;

        ZMmoManager.Instance.Field.REQ_Gather(Parent.EntityId, mCachedTarget.EntityId);
        Parent.ChangeState(E_EntityState.Empty);
    }
}