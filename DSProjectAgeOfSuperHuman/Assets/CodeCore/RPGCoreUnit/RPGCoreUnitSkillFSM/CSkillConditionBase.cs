using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class CSkillConditionBase 
{
	
	//------------------------------------------------------
	public int DoCheckCondition(ISkillProcessor pSkillProcessor)
	{
		return OnSkillCondition(pSkillProcessor);
	}

	protected virtual int OnSkillCondition(ISkillProcessor pSkillProcessor) { return 0; }
}

abstract public class CSkillResourceBase
{
	public int DoCheckSkillResource(CSkillUsage pUsage, ISkillProcessor pSkillProcessor)
	{
		return OnSkillResource(pUsage, pSkillProcessor);
	}

	protected virtual int OnSkillResource(CSkillUsage pUsage, ISkillProcessor pSkillProcessor) { return 0; }
}