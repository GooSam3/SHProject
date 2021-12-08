using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NBuff;
public class SHBuffTaskBuffUse : SHBuffTaskBase
{
	private EBuffTaskTarget m_eBuffTaskTarget = EBuffTaskTarget.None;
	private uint m_hBuffID = 0;
	private float m_fPower = 0;
	private float m_fDuration = 0;
	//------------------------------------------------------
	public void SetBuffTaskBuffUse(EBuffTaskTarget eBuffTaskTarget, uint hBuffID, float fPower, float fDuration)
	{
		m_eBuffTaskTarget = eBuffTaskTarget;
		m_hBuffID = hBuffID;
		m_fPower = fPower;
		m_fDuration = fDuration;
	}

	protected override void OnBuffTask(CBuffBase pBuff, CBuffComponentBase pBuffOwner, CBuffComponentBase pBuffOrigin) 
	{
		SHUnitBase pBuffTarget = null;
		SHUnitBase pBuffFrom = pBuffOwner.GetBuffOwner() as SHUnitBase;
		if (m_eBuffTaskTarget == EBuffTaskTarget.BuffOrigin)
		{
			pBuffTarget = pBuffOrigin.GetBuffOwner() as SHUnitBase;
		}
		else if (m_eBuffTaskTarget == EBuffTaskTarget.BuffOwner)
		{
			pBuffTarget = pBuffFrom;
		}

		pBuffFrom.ISkillBuffTo(pBuffTarget, m_hBuffID, m_fDuration, m_fPower);
	}
}
