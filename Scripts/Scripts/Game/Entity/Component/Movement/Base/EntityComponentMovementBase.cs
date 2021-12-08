using GameDB;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/// <summary> 네비메시 이동 </summary>
public abstract class EntityComponentMovementBase : EntityComponentBase<EntityBase>
{
    protected const float STOPPING_DISTANCE = 0.2f;

    /// <summary> 이동 상태가 변경되었을 경우 알림. </summary>
    protected Action<bool> mEventMoveState;

    protected bool IsMyPc { get { return Owner.IsMyPc; } }

    protected Vector3 Position { get { return CachedTransform.position; } }
    protected Quaternion Rotation { get { return CachedTransform.rotation; } }

    /// <summary> 탑승 가능 여부 </summary>
    public abstract bool IsPossibleRide { get; }

    /// <summary> input으로 이동하고 있는지 여부 </summary>
    public bool IsInputMove { get; protected set; }

    /// <summary> 모델이 변경되었을 때 호출 </summary>
    public virtual void OnChangeModel()
    {

    }
       
    public virtual bool IsMoving()
    {
        return false;
    }

    public virtual bool IsMovingDir()
    {
        return false;
    }

    public virtual Vector3? MoveTo(Vector3 destPosition, float speed, bool bInputMove)
    {
        return null;
    }

    public virtual Vector3? MoveTo(List<Vector3> path, float speed)
    {
        return null;
    }

    public virtual Vector3? MoveToDirection(Vector3 curPosition, Vector3 dir, float speed, Vector2 joystickDir)
    {
        return null;
    }

    public virtual void StopMove(Vector3 curPosition)
    {
    }

    public virtual void ForceMove(Vector3 position, float duration, E_PosMoveType moveType)
    {   
    }

    public virtual void Warp(Vector3 position)
    {
    }

    public virtual void SetSpeed(float speed)
    {
        Owner.SetAnimMoveSpeed(speed);
    }

    public void DoAddEventMoveState(Action<bool> action)
    {
        DoRemoveEventMoveState(action);
        mEventMoveState += action;
    }

    public void DoRemoveEventMoveState(Action<bool> action)
    {
        mEventMoveState -= action;
    }
}
