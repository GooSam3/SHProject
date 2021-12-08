using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DKSkillTaskBuff : DKSkillTaskBase
{
	private uint m_hBuffID = 0;
	private float m_fDuration = 0;
	//---------------------------------------------------
	public void SetSkillTaskBuff(uint hBuffID, float fDuration)
	{
		m_hBuffID = hBuffID;
		m_fDuration = fDuration;
	}

}
