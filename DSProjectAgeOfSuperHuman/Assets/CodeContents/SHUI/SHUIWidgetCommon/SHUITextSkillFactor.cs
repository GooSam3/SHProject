using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHUITextSkillFactor : CUITextTagTextBase
{
	private uint m_iLevelFactor = 0;
	private uint m_hSkillID = 0;
	//------------------------------------------------------------------
	protected override string OnUITagText(string strContents, int iIndex) 
	{
		string strText = string.Empty;
		float fBaseValue = 0;
		if (float.TryParse(strContents, out fBaseValue))
		{
			float fLevelFactor = ExtractSkillFactor(iIndex);
			fBaseValue = fBaseValue + fLevelFactor;
			strText = string.Format("{0:0.#}", fBaseValue);
		}

		return strText; 
	}
	//---------------------------------------------------------------
	public void SetTextSkillFactor(uint hSkillID, uint iLevel)
	{
		m_hSkillID = hSkillID;
		m_iLevelFactor = iLevel;
	}

	//-----------------------------------------------------------------
	private float ExtractSkillFactor(int iIndex)
	{
		return SHManagerScriptData.Instance.ExtractTableSkill().GetTableDescSkillFactor(m_hSkillID, iIndex) * m_iLevelFactor;
	}


}
