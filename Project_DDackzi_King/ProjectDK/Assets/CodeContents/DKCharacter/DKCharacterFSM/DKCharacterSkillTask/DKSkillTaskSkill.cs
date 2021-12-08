using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DKSkillTaskSkill : DKSkillTaskBase
{
	private uint m_hSkillID = 0;

	//---------------------------------------------------------------
	public void SetSkillTaskSkill(uint hSkillID)
	{
		m_hSkillID = hSkillID;
	}
}
