using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DKTaskConditionBase : CTaskConditionBase
{
   
}

public class DKTaskConditionNone : DKTaskConditionBase {}
public class DKTaskConditionRandom : DKTaskConditionBase
{
	private uint m_iChance = 0; public void SetConditionChance(uint iValue) { m_iChance = iValue; }
	protected override bool OnTaskConditionCheck() 
	{
		bool Excute = false;
		int iValue = Random.Range(1, 10000);
		if (iValue <= m_iChance)
		{
			Excute = true;
		}				
		return Excute; 
	}
}

public class DKTaskConditionLessHP : DKTaskConditionBase
{
	private float m_fHPRate = 0; public void SetConditionHPRate(float fValue) { m_fHPRate = fValue; }
}