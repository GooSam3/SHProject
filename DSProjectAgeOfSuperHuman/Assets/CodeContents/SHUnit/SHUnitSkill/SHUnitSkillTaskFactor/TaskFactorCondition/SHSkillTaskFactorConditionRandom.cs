using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHSkillTaskFactorConditionRandom : SHSkillTaskFactorConditionBase
{
	private uint m_iChance = 0; public void SetRandomChance(uint iChance) { m_iChance = iChance; }

	protected override bool OnTaskConditionCheck() 
	{
		return true; 
	}

}
