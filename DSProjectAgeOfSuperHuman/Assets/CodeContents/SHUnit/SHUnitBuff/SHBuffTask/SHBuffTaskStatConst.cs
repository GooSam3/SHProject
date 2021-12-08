using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHBuffTaskStatConst : SHBuffTaskBase, IStatModifierOwner
{
	private ESHStatType m_eStatType = ESHStatType.None;
	private float m_fValue = 0;
	private SHStatModifier m_pModifier = null;
	//---------------------------------------------------
	public void SetBuffTaskStatConst(ESHStatType eStatType, float fValue)
	{
		m_eStatType = eStatType;
		m_fValue = fValue;
	}

	protected override void OnBuffTask(CBuffBase pBuff, CBuffComponentBase pBuffOwner, CBuffComponentBase pBuffOrigin)
	{
		base.OnBuffTask(pBuff, pBuffOwner, pBuffOrigin);
		SHStatComponentBase pStatComp = pBuffOwner.ExtractStatComponent() as SHStatComponentBase;
		if (pStatComp)
		{
			m_pModifier = new SHStatModifier();
			m_pModifier.DoSHStatModifierConst(m_eStatType, EStatModifierType.Buff, m_fValue, this);
			pStatComp.DoStatComponentModifierAdd(m_pModifier);
		}
	}
	protected override void OnBuffTaskEnd(CBuffBase pBuff, CBuffComponentBase pBuffOwner, CBuffComponentBase pBuffOrigin)
	{
		base.OnBuffTaskEnd(pBuff, pBuffOwner, pBuffOrigin);
		if (m_pModifier != null)
		{
			pBuffOwner.ExtractStatComponent().DoStatComponentModifierDelete(m_pModifier);
		}
	}
}
