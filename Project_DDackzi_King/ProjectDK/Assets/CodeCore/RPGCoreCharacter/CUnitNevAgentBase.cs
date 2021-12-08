using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
/*
  1)  우선 순위가 높은 에이전트는 길을 찾을 때 우선 순위가 낮은 에이전트를 고려하지 않고 그냥 밀고 지나가버린다. 
  2)  우선 순위가 같으면 회피하려는 노력은 하지만 여의치 않을 때는 그냥 밀고 지나가게 된다. 
  3)  낮은 우선 순위의 에이전트는 높은 우선 순위의 에이전트를 밀어내지 못한다.
 */

[RequireComponent(typeof(NavMeshAgent))]
abstract public class CUnitNevAgentBase : CUnitBase
{
	public enum ENavAgentEvent
	{
		Finish,
		InterruptStop,
		PathFindFail,
	}

	private enum ENavAgentMoveType
	{
		None,
		Position,
		Object,
	}
	private float mMoveSpeed = 0;
	private Vector3 mDestPosition = Vector3.zero;
	private Vector3 mMoveDirection = Vector3.zero;

	private CUnitBase mDestObject = null;
	private ENavAgentMoveType mMoveType = ENavAgentMoveType.None;
	private UnityAction<ENavAgentEvent, Vector3> mEventFinish = null;
	private NavMeshAgent mNavAgent = null;
	//---------------------------------------------------
	protected override void OnUnityAwake()
	{
		base.OnUnityAwake();
		mNavAgent = GetComponent<NavMeshAgent>();
		mNavAgent.autoBraking = true;
		mNavAgent.acceleration = 10000f;
		mNavAgent.obstacleAvoidanceType = ObstacleAvoidanceType.GoodQualityObstacleAvoidance;
		mNavAgent.avoidancePriority = 50;
		mNavAgent.speed = 3f;
	}

	protected override void OnUnitUpdate()
	{
		base.OnUnitUpdate();
		if (mMoveType == ENavAgentMoveType.None) return;
		UpdateTransform();
		if (mMoveType == ENavAgentMoveType.Object)
		{
			UpdateNevTarget();
		}

		CheckNevMoveFinish();
	}

	public float GetNavRadius()
	{
		return mNavAgent.radius * transform.localScale.x;
	}

	//---------------------------------------------------
	protected void ProtNavMoveDestination(Vector3 vecDest, float fStopRange, UnityAction<ENavAgentEvent, Vector3> delFinish)
	{
		if (fStopRange < mNavAgent.radius)
		{
			fStopRange = mNavAgent.radius;
		}

		mEventFinish = delFinish;
		PrivNavAgentMoveStart(vecDest, fStopRange, ENavAgentMoveType.Position);
	}

	protected void ProtNavMoveObject(CUnitBase pTargetObject, float fStopRange, UnityAction<ENavAgentEvent, Vector3> delFinish)
	{
		CUnitNevAgentBase pNavAngent = pTargetObject as CUnitNevAgentBase;
		if (pNavAngent == null) return;

		float minimumStop = mNavAgent.radius + pNavAngent.GetNavRadius();
		if (fStopRange < minimumStop)
		{
			fStopRange = minimumStop * 1.1f; // 10%정도 여유가 있어야 멈춤
		}

		mEventFinish = delFinish;
		mDestObject = pTargetObject;
		PrivNavAgentMoveStart(pTargetObject.transform.position, fStopRange, ENavAgentMoveType.Object);
	}

	protected void ProtNavMoveFoward(Vector3 vecDirection)
	{
		Vector3 vecPrev = transform.position;
		mNavAgent.Move(vecDirection);
		mMoveDirection = transform.position - vecPrev;
	}

	protected void SetNavAgentMoveSpeed(float fMoveSpeed)
	{
		mMoveSpeed = fMoveSpeed;
		mNavAgent.speed = fMoveSpeed;
	}

	protected void SetNavAgenRotationSpeed(float fRotationSpeed)
	{
		if (fRotationSpeed == 0)
		{
			mNavAgent.updateUpAxis = false;
			mNavAgent.updateRotation = false;
		}
		mNavAgent.angularSpeed = fRotationSpeed;
	}

	protected void SetNavAgentShape(float fRadius, float fHeight)
	{
		mNavAgent.radius = fRadius;
		mNavAgent.height = fHeight;
	}

	protected Vector3 GetNavAgentDirection()
	{		
		return mMoveDirection;
	}

	//--------------------------------------------------------
	private void CheckNevMoveFinish()
	{
		if (mNavAgent.pathPending) return;

		if (mNavAgent.remainingDistance <= mNavAgent.stoppingDistance)
		{
			PrivNavAgentMoveFinish(ENavAgentEvent.Finish);
		}		
	}

	private void UpdateNevTarget()
	{
		if (mDestObject == null) return;
		mNavAgent.destination = mDestObject.transform.position;
	}

	private void UpdateTransform()
	{
		Vector3 vecPosition = transform.position;
		vecPosition.y = mNavAgent.destination.y;
		mMoveDirection = mNavAgent.destination - vecPosition;
	}

	private void PrivNavAgentMoveFinish(ENavAgentEvent eAgentEvent)
	{
		mMoveType = ENavAgentMoveType.None;
		mNavAgent.isStopped = true;
		mMoveDirection = Vector3.zero;
		OnUnitNavAgentMoveFinish(eAgentEvent, gameObject.transform.position);
		if (mEventFinish != null)
		{
			mEventFinish(eAgentEvent, gameObject.transform.position);
		}
	}

	private void PrivNavAgentMoveStart(Vector3 vecDest, float fStopRange, ENavAgentMoveType eMoveType)
	{
		mDestPosition = vecDest;
		mNavAgent.destination = vecDest;
		mNavAgent.stoppingDistance = fStopRange;
		mNavAgent.isStopped = false;
		mMoveType = eMoveType;
	}

	//--------------------------------------------------------------------------------
	protected virtual void OnUnitNavAgentMoveFinish(ENavAgentEvent eAgentEvent, Vector3 vecPosition) { }
}
