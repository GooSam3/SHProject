using UnityEngine;
using System.Collections.Generic;
using System.Collections;

abstract public class CFiniteStateMachineBase : CMonoBase
{
	public enum EStateEnterType
	{
		Enter,          //새로운 스테이트는 큐에 대기
		Interrupt,      //새로운 스테이트는 스텍에 보존되며 새로운 스테이트가 활성화된다.
		EnterForce,     //새로운 스테이트는 즉시 활성화되며 모든 큐와 스텍이 강제 종료된다.
	}

	private Stack<CStateBase> m_stackInterruptedState = new Stack<CStateBase>();
	private Queue<CStateBase> m_queueState = new Queue<CStateBase>();

	protected CStateBase mStateCurrent = null;
	protected CStateBase mStatePrev = null;
	private bool mIsEmpry = true; public bool IsEmpty { get { return mIsEmpry; } }
	//-------------------------------------------------------------
	private void LateUpdate()
	{   //일반 업데이트가 끝난 이후 스테이트 교체가 발생
		if (mStateCurrent == null)
		{
			PrivLateUpdateAction();
		}
	}

	internal void ImportStateLeave(CStateBase pStateEnd)
	{
		ProtStateLeave(pStateEnd);
	}

	//----------------------------------------------------------
	protected void ProtStateAction(CStateBase pState, EStateEnterType eStateAction)
	{
		pState.ImportStateInitialize(this);

		switch (eStateAction)
		{
			case EStateEnterType.Enter:
				PrivStateActionEnter(pState);
				break;
			case EStateEnterType.EnterForce:
				PrivStateActionEnterForce(pState);
				break;
			case EStateEnterType.Interrupt:
				PrivStateActionInterrupt(pState);
				break;
		}		
	}

	protected void ProtStateLeave(CStateBase pState)
	{
		if (mStateCurrent != pState) return;

		pState.ImportStateLeave();
		mStateCurrent = null;

		OnFSMStateLeave(pState);
	}

	//------------------------------------------------------------
	private void PrivStateActionEnter(CStateBase pState)
	{
		if (mStateCurrent != null)
		{
			mStateCurrent.ImportStateEnterAnother(pState);
		}

		PrivStateActivate(pState);
	}

	private void PrivStateActionEnterForce(CStateBase pState)
	{
		PrivStateClearAll(pState);
		PrivStateActivate(pState);
	}

	private void PrivStateActionInterrupt(CStateBase pState)
	{
		if (mStateCurrent != null)
		{
			m_stackInterruptedState.Push(mStateCurrent);
			mStateCurrent.ImportStateInterrupted(pState);
		}

		PrivStateCurrent(pState);
	}

	private void PrivStateClearAll(CStateBase pStateNew)
	{
		Stack<CStateBase>.Enumerator itStack = m_stackInterruptedState.GetEnumerator();
		while (itStack.MoveNext())
		{
			itStack.Current.ImportStateLeaveForce(pStateNew);
		}
		m_stackInterruptedState.Clear();

		Queue<CStateBase>.Enumerator itQueue = m_queueState.GetEnumerator();
		while (itQueue.MoveNext())
		{
			itQueue.Current.ImportStateLeaveForce(pStateNew);
		}
		m_queueState.Clear();

		mStateCurrent.ImportStateLeaveForce(pStateNew);
	}

	private void PrivStateActivate(CStateBase pState)
	{
		mIsEmpry = false;
		m_queueState.Enqueue(pState);
	}

	//-------------------------------------------------------------
	private void PrivLateUpdateAction()
	{
		if (PrivStateUpdateInterrupt() == false)
		{
			PrivStateUpdateQueue();
		}
	}

	private bool PrivStateUpdateInterrupt()
	{
		bool bUpdate = false;
		if (m_stackInterruptedState.Count > 0)
		{
			bUpdate = true;
			CStateBase pState = m_stackInterruptedState.Pop();
			pState.ImportStateInterruptedResume(mStatePrev);
			PrivStateCurrent(pState);
		}
		return bUpdate;
	}

	private void PrivStateUpdateQueue()
	{
		if (m_queueState.Count > 0)
		{
			CStateBase pState = m_queueState.Dequeue();			
			PrivStateCurrent(pState);
		}
		else
		{
			mIsEmpry = true;
			OnFSMStateEmpty();
		}
	}

	private void PrivStateCurrent(CStateBase pState)
	{
		int iEnterFailType = pState.ImportStateCanEnter(mStateCurrent); // 스테이트 진입 평가
		if (iEnterFailType == 0)
		{
			mStatePrev = mStateCurrent;
			mStateCurrent = pState;
			StopAllCoroutines();
			pState.ImportStateEnter(mStatePrev);

			if (mStateCurrent != null)
			{
				StartCoroutine(mStateCurrent.OnStateUpdate());
			}
		}
		else
		{
			PrivStateUpdateQueue();
		}
	

		OnFSMStateEnter(pState, iEnterFailType);
	}

	//---------------------------------------------------------------------
	protected virtual void OnFSMStateEnter(CStateBase pState, int iEnterFailType) { }
	protected virtual void OnFSMStateLeave(CStateBase pState) { }
	protected virtual void OnFSMStateEmpty() { }
}
