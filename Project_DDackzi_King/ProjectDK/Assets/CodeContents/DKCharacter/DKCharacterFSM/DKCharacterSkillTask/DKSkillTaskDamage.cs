using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NSkill;

public class DKSkillTaskDamage : DKSkillTaskBase
{
	private EDamageType m_eDamageType = EDamageType.None;
	private bool m_bDamageOrHeal = false;
	private float m_fPower = 0;
	private float m_fAggro = 0;
	//----------------------------------------------------
	public void SetSkillTaskDamage(EDamageType eDamageType, bool bDamageOrHeal, float fPower, float fAggro)
	{
		m_eDamageType = eDamageType;
		m_bDamageOrHeal = bDamageOrHeal;
		m_fPower = fPower;
		m_fAggro = fAggro;
	}

	protected override void OnSkillTaskUse(CSkillUsage pSkillUsage, ISkillProcessor pSkillOwner, List<CUnitBase> pListTarget) 
	{
		for (int i = 0; i < pListTarget.Count; i++)
		{
			CUnitBuffBase.SDamageResult rResult = new CUnitBuffBase.SDamageResult();
			rResult.hSkillID = pSkillUsage.UsageSkillID;
			rResult.pAttacker = pSkillOwner.IGetUnit();
			rResult.pDefender = pListTarget[i];
			rResult.eDamageType = (int)m_eDamageType;
			rResult.fDamageRate = m_fPower;
			if (m_bDamageOrHeal)
			{			
				pSkillOwner.ISkillDamageTo(pListTarget[i], rResult);
			}
			else
			{
				pSkillOwner.ISkillHealTo(pListTarget[i], rResult);
			}
		}
	}
}
