using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DKSkillConditionCrowdControl : DKSkillConditionBase
{
	public bool Stun = false;
	public bool Sleep = false;
	public bool KnockBack = false;
	public bool Silence = false;

	protected override int OnSkillCondition(ISkillProcessor pSkillProcessor) 
	{
		return 0; 
	}

}
