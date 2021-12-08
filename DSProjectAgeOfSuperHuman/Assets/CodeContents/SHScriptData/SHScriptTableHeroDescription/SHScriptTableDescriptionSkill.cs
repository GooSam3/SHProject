using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHScriptTableDescriptionSkill : CScriptDataTableBase
{
	public class SDescriptionSkill : CObjectInstanceBase
	{
		public uint SkillID = 0;
		public string IconName;
		public string SkillName;
		public string Description;
		public ESkillDescriptionType ESkillDescriptionType = ESkillDescriptionType.None;
		public float Arg1;
		public float Arg2;
		public float Arg3;
		public float Arg4;
	}

	private Dictionary<uint, SDescriptionSkill> m_mapTableSkill = new Dictionary<uint, SDescriptionSkill>();
	//-----------------------------------------------------------------------
	protected override void OnScriptDataInitialize(string strTextData)
	{
		base.OnScriptDataInitialize(strTextData);
		PrivScriptTableLoad();
	}
	//-----------------------------------------------------------------------
	public string GetTableDescSkillIcon(uint hSkillID)
	{
		string strIconName = "";
		if (m_mapTableSkill.ContainsKey(hSkillID))
		{
			strIconName = m_mapTableSkill[hSkillID].IconName;
		}

		return strIconName;
	}

	public string GetTableDescSkillName(uint hSkillID)
	{
		string strSkillName = "";
		if (m_mapTableSkill.ContainsKey(hSkillID))
		{
			strSkillName = m_mapTableSkill[hSkillID].SkillName;
		}
		return strSkillName;
	}

	public SDescriptionSkill GetTableDescSkillCopy(uint hSkillID)
	{
		SDescriptionSkill pCopyData = null;
		if (m_mapTableSkill.ContainsKey(hSkillID))
		{
			pCopyData = m_mapTableSkill[hSkillID].CopyInstance<SDescriptionSkill>();
		}
		return pCopyData;
	}

	public float GetTableDescSkillFactor(uint hSkillID, int iIndex)
	{
		float fFactor = 0;
		if (m_mapTableSkill.ContainsKey(hSkillID))
		{
			SDescriptionSkill pSkillData = m_mapTableSkill[hSkillID];
			if (iIndex == 0)
			{
				fFactor = pSkillData.Arg1;
			}
			else if (iIndex == 1)
			{
				fFactor = pSkillData.Arg2;
			}
			else if (iIndex == 2)
			{
				fFactor = pSkillData.Arg3;
			}
			else if (iIndex == 3)
			{
				fFactor = pSkillData.Arg4;
			}
		}

		return fFactor;
	}

	//------------------------------------------------------------------------
	private void PrivScriptTableLoad()
	{
		List<SDescriptionSkill> pListTableLoad = ProtDataTableRead<SDescriptionSkill>();
		for (int i = 0; i < pListTableLoad.Count; i++)
		{
			SDescriptionSkill pTable = pListTableLoad[i];
			m_mapTableSkill[pTable.SkillID] = pTable;
		}
	}
}
