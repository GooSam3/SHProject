using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSkillTaskFactorTargetBase 
{
    //-------------------------------------------------------------------
    public void DoTaskTarget(CSkillUsage pUsage, ISkillProcessor pSkillProcessor, List<CUnitBase> pListTarget)
	{
		OnTaskTarget(pUsage, pSkillProcessor, pListTarget);
	}

	//---------------------------------------------------------------------
	protected virtual void OnTaskTarget(CSkillUsage pUsage, ISkillProcessor pSkillProcessor, List<CUnitBase> pListTarget) { }
}
