using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DKSkillConditionCoolTime : DKSkillConditionBase
{
	public string CoolTimeName;
	public string GlobalCoolTime;

	protected override int OnSkillCondition(ISkillProcessor pSkillProcessor) 
	{
		float fCoolTime =	pSkillProcessor.IGetSkillCoolTimeGlobal(GlobalCoolTime);

		if (fCoolTime != 0)
		{
			return (int)ESkillConditionResult.CoolTimeGlobal;
		}

		fCoolTime = pSkillProcessor.IGetSkillCoolTime(CoolTimeName);

		if (fCoolTime != 0)
		{
			return (int)ESkillConditionResult.CoolTimeSkill;
		}
		
		return 0; 
	}
}
