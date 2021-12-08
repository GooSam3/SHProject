using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHSkillConditionNone : CSkillConditionBase
{

}

public class SHSkillConditionCC : CSkillConditionBase
{
	private bool m_bStun = false;
	private bool m_bSlience = false;

	//-------------------------------------------------------------
	public void SetSkillCondition(bool bStun, bool bSlience)
	{
		m_bStun = bStun;
		m_bSlience = bSlience;
	}

}

public class SHSkillConditionCoolTime : CSkillConditionBase
{
	public string CoolTimeName;
	protected override int OnSkillCondition(ISkillProcessor pSkillProcessor)
	{
		float fCoolTime = pSkillProcessor.IGetSkillCoolTimeGlobal(SHSkillUsage.g_GlobalCoolTime);

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
