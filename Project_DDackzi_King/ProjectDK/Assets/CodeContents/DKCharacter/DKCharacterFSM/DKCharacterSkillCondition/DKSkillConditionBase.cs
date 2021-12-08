using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ESkillConditionResult
{
	None,
	CoolTimeGlobal,
	CoolTimeSkill,
	CrowdControll,

	Invalid,
}


abstract public class DKSkillConditionBase : CSkillConditionBase
{
   

}

public class DKSkillConditionNone : DKSkillConditionBase
{
	protected override int OnSkillCondition(ISkillProcessor pSkillProcessor) { return 0; }
}