using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NSkill;

public class DKAnimationEventHandler : CAnimationEventHandlerBase
{
	private DKUnitBase m_pAnimationEventHandle = null;
	//---------------------------------------------------------
	protected override void OnEventHandlerOwner(CUnitBase pOwner) 
	{
		m_pAnimationEventHandle = pOwner as DKUnitBase;
	}

	//-----------------------------------------------------------
	public void EventAnimEventAttack(int Index)
	{
		m_pAnimationEventHandle.EventCharAnimation(EAnimEventType.AnimEvent_Attack, Index);
	}

	public void EventAnimEventSkill(int Index)
	{
		m_pAnimationEventHandle.EventCharAnimation(EAnimEventType.AnimEvent_Skill, Index);
	}

	public void EventAnimEventAnimationEnd()
	{
		m_pAnimationEventHandle.EventCharAnimationEnd();
	}

}
