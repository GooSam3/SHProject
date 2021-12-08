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

	protected CStateBase m_pStateCurrent = null;
	protected CStateBase m_pStatePrev = null;
	private bool m_bEmpry = true; public bool IsEmpty { get { return m_bEmpry; } }
	//-------------------------------------------------------------
	private void LateUpdate()
	{   //일반 업데이트가 끝난 이후 스테이트 교체가 발생
		if (m_pStateCurrent == null)
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
		if (m_pStateCurrent != pState) return;

		pState.ImportStateLeave();
		m_pStateCurrent = null;

		OnFSMStateLeave(pState);
	}

	protected void ProtStatClearAll()
	{
		m_bEmpry = true;
		m_stackInterruptedState.Clear();
		m_queueState.Clear();

		if (m_pStateCurrent != null)
		{
			m_pStateCurrent.ImportStateLeave();
			m_pStateCurrent.ImportStateLeaveForce(m_pStatePrev);
		}

		m_pStateCurrent = null;
		m_pStatePrev = null;
	}

	//------------------------------------------------------------
	private void PrivStateActionEnter(CStateBase pState)
	{
		if (m_pStateCurrent != null)
		{
			m_pStateCurrent.ImportStateEnterAnother(pState);
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
		if (m_pStateCurrent != null)
		{
			m_stackInterruptedState.Push(m_pStateCurrent);
			m_pStateCurrent.ImportStateInterrupted(pState);
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

		if (m_pStateCurrent != null)
		{
			m_pStateCurrent.ImportStateLeave();
			m_pStateCurrent.ImportStateLeaveForce(pStateNew);
		}
	}

	private void PrivStateActivate(CStateBase pState)
	{
		m_bEmpry = false;
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
			pState.ImportStateInterruptedResume(m_pStatePrev);
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
			m_bEmpry = true;
			OnFSMStateEmpty();
		}
	}

	private void PrivStateCurrent(CStateBase pState)
	{
		int iEnterFailType = pState.ImportStateCanEnter(m_pStateCurrent); // 스테이트 진입 평가
		if (iEnterFailType == 0)
		{
			m_pStatePrev = m_pStateCurrent;
			m_pStateCurrent = pState;
			StopAllCoroutines();
			pState.ImportStateEnter(m_pStatePrev);

			if (m_pStateCurrent != null)
			{
				StartCoroutine(m_pStateCurrent.OnStateUpdate());
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
