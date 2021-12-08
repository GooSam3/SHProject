using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHUnitSkillFSMEnemy : SHUnitSkillFSMBase
{
	[SerializeField]
	private SUnitSkillInfo NormalSkill = new SUnitSkillInfo();

	//------------------------------------------------------------------------
	protected override void OnFSMSkillInitialize(ISkillProcessor pSkillOnwer)
	{
		base.OnFSMSkillInitialize(pSkillOnwer);

		SHSkillDataActive pSkillInstance = null;
		if (NormalSkill.SkillID != 0)
		{
			pSkillInstance = SHManagerScriptData.Instance.DoLoadSkillActive(NormalSkill.SkillID);
			ProtFSMSkillDataLoad(pSkillInstance);
			ProtUnitSkillExportInfo(ESkillType.SkillNormal, pSkillInstance);
		}
	}
}
