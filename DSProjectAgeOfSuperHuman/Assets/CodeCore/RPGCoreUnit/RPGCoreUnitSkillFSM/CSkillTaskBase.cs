using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSkillTaskBase
{
	private CSkillTaskFactorConditionBase m_pTaskCondition = null;
	private CSkillTaskFactorTargetBase m_pTaskTarget = null;
	//-------------------------------------------------
	public void DoSkillTaskUse(CStateSkillBase pOwnerState, CSkillUsage pSkillUsage, ISkillProcessor pSkillOwner)
	{
		List<CUnitBase> pListTarget = new List<CUnitBase>();
		if (m_pTaskTarget != null)
		{
			m_pTaskTarget.DoTaskTarget(pSkillUsage, pSkillOwner, pListTarget);
		}
		else
		{
			pListTarget.Add(pSkillUsage.UsageTarget.IGetUnit());
		}

		if (m_pTaskCondition != null)
		{
			if (m_pTaskCondition.DoTaskConditionCheck(pSkillOwner))
			{
				OnSkillTaskUse(pOwnerState, pSkillUsage, pSkillOwner, pListTarget);
			}
		}
		else
		{
			OnSkillTaskUse(pOwnerState, pSkillUsage, pSkillOwner, pListTarget);
		}
	}

	public void SetSkillTaskCondition(CSkillTaskFactorConditionBase pTaskCondition)
	{
		m_pTaskCondition = pTaskCondition;
	}

	public void SetSkillTaskTarget(CSkillTaskFactorTargetBase pTaskTarget)
	{
		m_pTaskTarget = pTaskTarget;
	}

	//--------------------------------------------------------
	protected virtual void OnSkillTaskUse(CStateSkillBase pOwnerState, CSkillUsage pSkillUsage, ISkillProcessor pSkillOwner, List<CUnitBase> pListTarget) { }
	protected virtual void OnSkillTaskStateEnd(CStateSkillBase pOwnerState) { }
}

