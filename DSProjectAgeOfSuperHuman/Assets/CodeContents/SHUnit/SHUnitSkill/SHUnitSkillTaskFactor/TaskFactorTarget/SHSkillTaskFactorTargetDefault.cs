using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NSkill;

public class SHSkillTaskFactorTargetDefault : SHSkillTaskFactorTargetBase
{
	private ERelationType m_eRelationType = ERelationType.None;
	//-----------------------------------------------------------------
	public void SetTaskTargetDefault(ERelationType eRelation)
	{
		m_eRelationType = eRelation;
	}

	protected override void OnTaskTarget(CSkillUsage pUsage, ISkillProcessor pSkillProcessor, List<CUnitBase> pListTarget)
	{
		CUnitBase pSkillOnwer = pSkillProcessor.IGetUnit();
		if (m_eRelationType == ERelationType.Relation_Me)
		{
			pListTarget.Add(pSkillProcessor.IGetUnit());
		}
		else if (m_eRelationType == ERelationType.Relation_Target)
		{
			if (pUsage.UsageTarget != null)
			{
				pListTarget.Add(pUsage.UsageTarget.IGetUnit());
			}
		}
		else if (m_eRelationType == ERelationType.Relation_EnemyAll)
		{
			CUnitBase.EUnitRelationType eUnitRelationType = pSkillOnwer.GetUnitRelationForPlayer();
			if (eUnitRelationType == CUnitBase.EUnitRelationType.Enemy)
			{
				PrivTaskFactorTarget(pListTarget, true);
			}
			else if (eUnitRelationType == CUnitBase.EUnitRelationType.Hero)
			{
				PrivTaskFactorTarget(pListTarget, false);
			}
		}
		else if (m_eRelationType == ERelationType.Relation_FriendAll)
		{
			CUnitBase.EUnitRelationType eUnitRelationType = pSkillOnwer.GetUnitRelationForPlayer();
			if (eUnitRelationType == CUnitBase.EUnitRelationType.Enemy)
			{
				PrivTaskFactorTarget(pListTarget, false);
			}
			else if (eUnitRelationType == CUnitBase.EUnitRelationType.Hero)
			{
				PrivTaskFactorTarget(pListTarget, true);
			}
		}
		else
		{
			if (pUsage.UsageTarget != null)
			{
				pListTarget.Add(pUsage.UsageTarget.IGetUnit());
			}
		}
	}
	//-----------------------------------------------------------------------------------------
	private void PrivTaskFactorTarget(List<CUnitBase> pListTarget, bool bHero)
	{
		if (bHero)
		{
			List<SHUnitHero> pHeroAll = SHManagerUnit.Instance.GetMgrUnitHeroAll();
			for (int i = 0; i < pHeroAll.Count; i++)
			{
				pListTarget.Add(pHeroAll[i]);
			}
		}
		else
		{
			List<SHUnitEnemy> pEnemy = SHManagerUnit.Instance.GetMgrUnitEnemyBossAll();
			for (int i = 0; i < pEnemy.Count; i++)
			{
				pListTarget.Add(pEnemy[i]);
			}
		}
	}
}
