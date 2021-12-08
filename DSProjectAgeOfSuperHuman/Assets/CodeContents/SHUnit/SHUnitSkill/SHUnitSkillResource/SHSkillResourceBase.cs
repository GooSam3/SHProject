using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SHSkillResourceBase : CSkillResourceBase
{

}

public class SHSkillResourceNone : SHSkillResourceBase
{
	protected override int OnSkillResource(CSkillUsage pUsage, ISkillProcessor pSkillProcessor)
	{
		return 0;
	}
}

public class SHSkillResourceCoolTime : SHSkillResourceBase
{
	private string	m_strCoolTimeName;
	private float		m_fCoolTime;
	private float		m_fGlobalCoolTime;
	private float		m_fFeverPoint;
	//------------------------------------------------------------------
	public void SetResourceCoolTime(string strCoolTimeName, float fCoolTime, float fGlobalCoolTime, float fFeverPoint)
	{
		m_strCoolTimeName = strCoolTimeName;
		m_fCoolTime = fCoolTime;
		m_fGlobalCoolTime = fGlobalCoolTime;
		m_fFeverPoint = fFeverPoint;
	}

	protected override int OnSkillResource(CSkillUsage pUsage, ISkillProcessor pSkillProcessor)
	{
		ESkillConditionResult Result = ESkillConditionResult.None;
		ISHSkillProcessor pSHSkill = pSkillProcessor as ISHSkillProcessor;

		float fCoolTime = pSkillProcessor.IGetSkillCoolTimeGlobal(SHSkillUsage.g_GlobalCoolTime);
		if (fCoolTime > 0)
		{
			Result = ESkillConditionResult.CoolTimeGlobal;
		}
		else
		{
			fCoolTime = pSkillProcessor.IGetSkillCoolTime(m_strCoolTimeName);
			if (fCoolTime > 0)
			{
				Result = ESkillConditionResult.CoolTimeSkill;
			}
			else
			{
				pSHSkill.ISetSkillCoolTimeGlobal(SHSkillUsage.g_GlobalCoolTime, m_fGlobalCoolTime);
				pSHSkill.ISetSkillCoolTime(m_strCoolTimeName, m_fCoolTime);
				pSHSkill.ISHSkillRageGain(m_fFeverPoint);
			}
		}

		return (int)Result;
	}
}