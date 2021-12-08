using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHSkillTaskUseBuff : SHSkillTaskBase
{
    private uint    m_hBuffID = 0;
    private float   m_fDuration = 0;
    private string  m_strPropertyName;

    //---------------------------------------------------------------------
    public void SetTaskUseBuff(uint hBuffID, float fDuration, string strPropertyName)
	{
        m_hBuffID = hBuffID;
        m_fDuration = fDuration;
        m_strPropertyName = strPropertyName;
	}

	protected override void OnSkillTaskUse(CStateSkillBase pOwnerState, CSkillUsage pSkillUsage, ISkillProcessor pSkillOwner, List<CUnitBase> pListTarget)
	{
        float fPower = pOwnerState.GetStatePropertyValue(m_strPropertyName);

        for (int i = 0; i < pListTarget.Count; i++)
		{
            pSkillOwner.ISkillBuffTo(pListTarget[i], m_hBuffID, m_fDuration, fPower);
		}
	}
}
