using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHSkillTaskStateEnd : SHSkillTaskBase
{
	protected override void OnSkillTaskUse(CStateSkillBase pOwnerState, CSkillUsage pSkillUsage, ISkillProcessor pSkillOwner, List<CUnitBase> pListTarget)
	{
		SHUnitBase pUnit = pSkillOwner.IGetUnit() as SHUnitBase;
		pUnit.DoUnitSkillResetAnimation();
		pOwnerState.DoStateSelfEnd(); 
	}
}
