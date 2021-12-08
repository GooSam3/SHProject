using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NSkill;

public abstract class SHSkillStateBase : CStateSkillBase
{
	protected SHSkillUsage m_pSkillUsage = null;
	protected ISHSkillProcessor m_pSkillProcessor = null;

	//------------------------------------------------------------------
	protected override void OnStateInitialize(CSkillUsage pSkillUsage, ISkillProcessor pSkillProcessor)
	{
		m_pSkillUsage = pSkillUsage as SHSkillUsage;
		m_pSkillProcessor = pSkillProcessor as ISHSkillProcessor;
	}

	protected override void OnStateEnter(CStateBase pStatePrev)
	{
		base.OnStateEnter(pStatePrev);
		DoStateTaskEvent((int)ETaskEventType.State_Enter, this);
	}

	protected override void OnStateLeave()
	{
		base.OnStateLeave();
		DoStateTaskEvent((int)ETaskEventType.State_Exit, this);
	}

	protected override void OnStateLeaveForce(CStateBase pStatePrev)
	{
		base.OnStateLeaveForce(pStatePrev);
		DoStateTaskEvent((int)ETaskEventType.State_ExitForce, this);
	}
	//--------------------------------------------------------------------

}