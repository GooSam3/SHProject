using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SHCombatAIHeroBase : SHCombatAIBase
{

	private bool m_bLeft = true;
	//-----------------------------------------------------
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
		ESkillType eSkillType = ESkillType.None;
		if (m_bLeft)
		{
			eSkillType = ESkillType.SkillNormalLeft;
		}
		else
		{
			eSkillType = ESkillType.SkillNormalRight;
		}

		for (int i = 0; i < m_listSkillExportInfo.Count; i++)
		{


			if (m_listSkillExportInfo[i].SkillType == eSkillType)
			{
				float fCoolTime = m_pSHUnit.IGetSkillCoolTime(m_listSkillExportInfo[i].CoolTimeName);
				if (fCoolTime == 0)
				{
					PrivCombAUEnemySkillUse(m_listSkillExportInfo[i].SkillID);

					if (m_bLeft)
					{
						m_bLeft = false;
					}
					else
					{
						m_bLeft = true;
					}

					break;
				}
			}
		}
	}

	//-----------------------------------------------------------------------------------
	private void PrivCombAUEnemySkillUse(uint hSkillID)
	{
		SHUnitEnemy pEnemy = SHManagerUnit.Instance.GetUnitEnemy();
		if (pEnemy != null)
		{
			if (pEnemy.IsAlive)
			{
				SHSkillUsage pUsage = new SHSkillUsage();
				pUsage.UsageSkillID = hSkillID;
				pUsage.UsageTarget = pEnemy;
				m_pSHUnit.ISkillPlay(pUsage);
			}
		}
	}
}
