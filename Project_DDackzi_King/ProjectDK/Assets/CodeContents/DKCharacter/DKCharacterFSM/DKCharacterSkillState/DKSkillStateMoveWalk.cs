using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
// 목적지에 도착하면 종료

public class DKSkillStateMoveWalk : DKSkillStateAnimation
{
	private float m_fStopRange = 0;
	//------------------------------------------------------------------
	protected override void OnStateEnter(CStateBase pStatePrev)
	{
		base.OnStateEnter(pStatePrev);
		m_pSkillProcessor.ISkillMoveToTarget(m_pSkillUsage.UsageTarget.IGetUnit(), m_fStopRange, HandleMoveFinish);
	}
	//-------------------------------------------------------------------
	private void HandleMoveFinish(CUnitNevAgentBase.ENavAgentEvent eNavEvnet, Vector3 vecPosition)
	{
		ProtStateSelfEnd();
	}
	//-------------------------------------------------------------------
	public void SetMoveWalkStopRange(float fStopRange) { m_fStopRange = fStopRange; }
}
