using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHSkillTaskUseSkill : SHSkillTaskBase
{
	private uint m_hSkillID = 0;
	//----------------------------------------------------------
	public void SetTaskUseSkill(uint hSkillID)
	{
		m_hSkillID = hSkillID;
	}
}
