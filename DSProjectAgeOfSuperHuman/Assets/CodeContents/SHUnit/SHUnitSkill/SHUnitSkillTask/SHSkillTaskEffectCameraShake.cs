using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHSkillTaskEffectCameraShake : SHSkillTaskBase
{
	private string		m_strEffectName;
	private float			m_fDuration;
	//-----------------------------------------------------
	public void SetTaskEffectCameraShake(string strEffectName, float fDuration)
	{
		m_strEffectName = strEffectName;
		m_fDuration = fDuration;

		SHManagerEffect.Instance.DoMgrEffectPreLoadListAdd(m_strEffectName);
	}

	protected override void OnSkillTaskUse(CStateSkillBase pOwnerState, CSkillUsage pSkillUsage, ISkillProcessor pSkillOwner, List<CUnitBase> pListTarget)
	{

	}

}
