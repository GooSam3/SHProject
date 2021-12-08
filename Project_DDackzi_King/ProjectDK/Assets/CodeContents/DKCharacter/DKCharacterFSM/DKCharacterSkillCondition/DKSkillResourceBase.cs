using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class DKSkillResourceBase : CSkillResourceBase
{
    
}

public class DKSkillResourceNone : DKSkillResourceBase
{
    protected override int OnSkillResource(CSkillUsage pUsage, ISkillProcessor pSkillProcessor) 
    {
        return 0; 
    }
}

public class DKSkillResourceCooltime : DKSkillResourceBase
{
	private const string const_GlobalCoolTimeName = "Global";
	private const float const_GlobalCoolTime = 1f;
	private string m_strCoolTimeName;
	private float m_fCoolTime = 0;

	public void SetResourceCoolTime(string strCoolTimeName, float fCoolTime)
	{
		m_strCoolTimeName = strCoolTimeName;
		m_fCoolTime = fCoolTime;
	}

	protected override int OnSkillResource(CSkillUsage pUsage, ISkillProcessor pSkillProcessor)
	{
		ESkillConditionResult Result = ESkillConditionResult.None;
		float fCoolTime = pSkillProcessor.IGetSkillCoolTimeGlobal(const_GlobalCoolTimeName);
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
				pSkillProcessor.ISetSkillCoolTimeGlobal(const_GlobalCoolTimeName, const_GlobalCoolTime);
				pSkillProcessor.ISetSkillCoolTime(m_strCoolTimeName, m_fCoolTime);
			}
		}

		return (int)Result;
	}
}

public class DKSkillResourceBuff : DKSkillResourceBase
{
	protected override int OnSkillResource(CSkillUsage pUsage, ISkillProcessor pSkillProcessor)
	{
		return 0;
	}
}

public class DKSkillResourceHP : DKSkillResourceBase
{
	protected override int OnSkillResource(CSkillUsage pUsage, ISkillProcessor pSkillProcessor)
	{
		return 0;
	}
}


