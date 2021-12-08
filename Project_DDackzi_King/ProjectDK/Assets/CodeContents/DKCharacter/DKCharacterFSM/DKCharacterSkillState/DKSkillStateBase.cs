using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NSkill;
public class DKSkillStateBase : CStateSkillBase
{
	protected DKSkillUsage		 m_pSkillUsage = null;
	protected IDKSkillProcessor m_pSkillProcessor = null;
	//----------------------------------------------------------
	protected override void OnStateInitialize(CSkillUsage pSkillUsage, ISkillProcessor pSkillProcessor) 
	{
		m_pSkillUsage = pSkillUsage as DKSkillUsage;
		m_pSkillProcessor = pSkillProcessor as IDKSkillProcessor;
	}

	protected override void OnStateEnter(CStateBase pStatePrev)
	{
		base.OnStateEnter(pStatePrev);
		DoStateTaskEvent((int)ETaskEventType.TaskEvent_Enter, this);
	}

	protected override void OnStateLeave()
	{
		base.OnStateLeave();
		DoStateTaskEvent((int)ETaskEventType.TaskEvent_Exit, this);
	}

	protected override void OnStateLeaveForce(CStateBase pStatePrev)
	{
		base.OnStateLeaveForce(pStatePrev);
		DoStateTaskEvent((int)ETaskEventType.TaskEvent_ExitForce, this);
	}
}
