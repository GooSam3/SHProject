using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SHCombatAIEnemyBase : SHCombatAIBase
{
	//------------------------------------------------------------------------------------
	protected override bool OnSHCombatAISkillSlotTry() 
	{
		SHUnitSkillFSMBase.SSkillExportInfo pSkillInfo = null;
		for (int i = 0; i < m_listSkillExportInfo.Count; i++)
		{
			if (m_listSkillExportInfo[i].SkillType == ESkillType.SKillSlot)
			{
				float fCoolTime = m_pSHUnit.IGetSkillCoolTime(m_listSkillExportInfo[i].CoolTimeName);
				if (fCoolTime == 0)
				{
					pSkillInfo = m_listSkillExportInfo[i];
					break;
				}
			}
		}

		if (pSkillInfo == null)
		{
			return false;
		}
		else
		{
			PrivCombAUEnemySkillUse(pSkillInfo.SkillID);
			return true;
		}
	}

	protected override void OnSHCombatAISkillNormal()
	{
		for (int i = 0; i < m_listSkillExportInfo.Count; i++)
		{
			if (m_listSkillExportInfo[i].SkillType == ESkillType.SkillNormal)
			{
				float fCoolTime = m_pSHUnit.IGetSkillCoolTime(m_listSkillExportInfo[i].CoolTimeName);
				if (fCoolTime == 0)
				{
					PrivCombAUEnemySkillUse(m_listSkillExportInfo[i].SkillID);
					break;
				}
			}
		}
	}

	//-----------------------------------------------------------------------------------
	private void PrivCombAUEnemySkillUse(uint hSkillID)
	{
		SHUnitHero pHero = SHManagerUnit.Instance.GetUnitHero();
		if (pHero != null)
		{
			if (pHero.IsAlive)
			{
				SHSkillUsage pUsage = new SHSkillUsage();
				pUsage.UsageSkillID = hSkillID;
				pUsage.UsageTarget = pHero;
				m_pSHUnit.ISkillPlay(pUsage);
			}
		}
	}
}
