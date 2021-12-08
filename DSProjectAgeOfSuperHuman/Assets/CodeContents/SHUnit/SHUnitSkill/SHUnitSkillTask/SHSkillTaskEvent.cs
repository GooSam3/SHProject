using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NSkill;

public class SHSkillTaskEvent : SHSkillTaskBase
{
	private ETaskEventCustomType m_eTaskEventCustom = ETaskEventCustomType.None;
	private int m_iArg = 0;
	private float m_fArg = 0;
	//------------------------------------------------------------------------
	public void SetTaskEvent(ETaskEventCustomType eTaskEventCustom, int iArg, float fArg)
	{
		m_eTaskEventCustom = eTaskEventCustom;
		m_iArg = iArg;
		m_fArg = fArg;
	}
}
