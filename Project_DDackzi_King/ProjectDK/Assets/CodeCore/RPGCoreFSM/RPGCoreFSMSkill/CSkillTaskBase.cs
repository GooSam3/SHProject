using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSkillTaskBase
{
	private CTaskConditionBase m_pTaskCondition = null;
	private CTaskTargetBase m_pTaskTarget = null;
	private CTaskEventGenerator m_pTaskEvent = null;
	//-------------------------------------------------
	public void DoSkillTaskUse(CStateSkillBase pOwnerState, CSkillUsage pSkillUsage, ISkillProcessor pSkillOwner)
	{
		List<CUnitBase> pListTarget = new List<CUnitBase>();
		if (m_pTaskTarget != null)
		{
			m_pTaskTarget.DoTaskTarget(pSkillUsage, pSkillOwner, pListTarget);
		}

		if (m_pTaskCondition != null)
		{
			if (m_pTaskCondition.DoTaskConditionCheck(pSkillOwner))
			{
				OnSkillTaskUse(pSkillUsage, pSkillOwner, pListTarget);
			}
		}
		else
		{
			OnSkillTaskUse(pSkillUsage, pSkillOwner, pListTarget);
		}

		//if (m_pTaskEvent != null)
		//{
		//	pOwnerState.DoStateTaskEvent(m_pTaskEvent.eEventType, m_pTaskEvent.iArg, m_pTaskEvent.fArg);
		//}
	}

	public void SetSkillTaskCondition(CTaskConditionBase pTaskCondition)
	{
		m_pTaskCondition = pTaskCondition;
	}

	public void SetSkillTaskTarget(CTaskTargetBase pTaskTarget)
	{
		m_pTaskTarget = pTaskTarget;
	}

	public void SetSkillTaskEvent(CTaskEventGenerator pTaskEvent)
	{ 
	}

	//--------------------------------------------------------
	protected virtual void OnSkillTaskUse(CSkillUsage pSkillUsage, ISkillProcessor pSkillOwner, List<CUnitBase> pListTarget) { }
}

