using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NBuff;

public abstract class SHBuffTaskConditionBase : CBuffTaskConditionBase
{

}

public class SHBuffTaskConditionNone : SHBuffTaskConditionBase
{

}

public class SHBuffTaskConditionRandom : SHBuffTaskConditionBase
{
	private int m_iChance = 0;
	//--------------------------------------------------------------
	public void SetBuffTaskConditionRandom(int iChance)
	{
		m_iChance = iChance;
	}

	protected override bool OnBuffTaskCondition(CBuffBase pBuff, CBuffComponentBase pBuffOwner, CBuffComponentBase pBuffOrigin, params object[] aParams) 
	{
		bool bChance = false;
		int iSeed = Random.Range(0, 10000);
		if (iSeed < m_iChance)
		{
			bChance = true;
		}

		return bChance; 
	}
}

public class SHBuffTaskConditionStackCount : SHBuffTaskConditionBase
{
	private int m_iStackCount = 0;
	private bool m_bActiveCheck = false;
	//--------------------------------------------------------------
	public void SetBuffTaskConditionStackCount(int iStackCount)
	{
		m_iStackCount = iStackCount;
		m_bActiveCheck = true;
	}
	protected override bool OnBuffTaskCondition(CBuffBase pBuff, CBuffComponentBase pBuffOwner, CBuffComponentBase pBuffOrigin, params object[] aParams)
	{
		bool bCondition = false;
		if (m_iStackCount == pBuff.GetBuffStackCount() && m_bActiveCheck == true)
		{
			m_bActiveCheck = false;
			bCondition = true;
		}

		return bCondition;
	}

	protected override void OnBuffTaskEnd(CBuffTaskBase pBuffTask)
	{
		base.OnBuffTaskEnd(pBuffTask);
		m_bActiveCheck = false;
	}
}