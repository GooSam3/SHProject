using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DKManagerScriptData : CManagerScriptDataBase
{   public static new DKManagerScriptData Instance { get { return CManagerScriptDataBase.Instance as DKManagerScriptData; } }

	private DKScriptSkill m_pScriptSkill = null;
	//------------------------------------------------------------------
	protected override void OnScriptDataInitialize(CScriptDataBase pScriptData) 
	{
		DKScriptSkill pScriptSkill = pScriptData as DKScriptSkill;
		if (pScriptSkill != null)
		{
			m_pScriptSkill = pScriptSkill;
		}
	}
	//-------------------------------------------------------------------
	public DKSkillDataActive DoLoadSkillActive(uint hSkillID)
	{
		return m_pScriptSkill.DoScriptSkillActive(hSkillID);
	}

	public DKSkillDataPassive DoLoadSkillPassive(uint hSkillID)
	{
		return m_pScriptSkill.DoScriptSkillPassive(hSkillID);
	}

	public DKSkillDataAutoCast DoLoadSkillAutoCastr(uint hSkillID)
	{
		return m_pScriptSkill.DoScriptSkillAutoCast(hSkillID);
	}

	public void DoLoadSkillCommon(List<DKSkillDataActive> pListSkillData)
	{
		m_pScriptSkill.DoScriptSkillCommon(pListSkillData);
	}
}
