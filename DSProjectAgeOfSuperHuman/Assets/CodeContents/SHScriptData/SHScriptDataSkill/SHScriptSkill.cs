using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using System.Reflection;
using NSkill;
public class SHScriptSkill : CScriptDataXMLBase
{
	private Dictionary<uint, SkillActive>	m_mapSkillActive = new Dictionary<uint, SkillActive>();
	private Dictionary<uint, SkillPassive>	m_mapSkillPassive = new Dictionary<uint, SkillPassive>();
	private Dictionary<uint, SkillAutoCast> m_mapSkillAutoCast = new Dictionary<uint, SkillAutoCast>();
	
	//----------------------------------------------------------
	protected override void OnScriptXMLRoot(XmlElement pRootElem)
	{
		RootSkillData pRoot = new RootSkillData();
		pRoot.DoScriptDataLoad(pRootElem,  "NSkill.", Assembly.GetExecutingAssembly());

		for (int i = 0; i < pRoot.m_listChildElement.Count; i++)
		{
			for (int j = 0; j < pRoot.m_listChildElement[i].m_listChildElement.Count; j++)
			{
				SkillActive pSkillActive = pRoot.m_listChildElement[i].m_listChildElement[j] as SkillActive;
				if (pSkillActive != null)
				{
					m_mapSkillActive[pSkillActive.hSkillID] = pSkillActive;
				}

				SkillPassive pSkillPassive = pRoot.m_listChildElement[i].m_listChildElement[j] as SkillPassive;
				if (pSkillPassive != null)
				{
					m_mapSkillPassive[pSkillPassive.hSkillID] = pSkillPassive;
				}

				SkillAutoCast pSkillAutoCast = pRoot.m_listChildElement[i].m_listChildElement[j] as SkillAutoCast;
				if (pSkillAutoCast != null)
				{
					m_mapSkillAutoCast[pSkillAutoCast.hSkillID] = pSkillAutoCast;
				}
			}
		}
	}
	//----------------------------------------------------------
	public SHSkillDataActive DoScriptSkillActive(uint hSkillID)
	{
		return PrivSkillDataActive(hSkillID);
	}

	public SHSkillDataPassive DoScriptSkillPassive(uint hSkillID)
	{
		return PrivSkillDataPassive(hSkillID);
	}

	public SHSkillDataAutoCast DoScriptSkillAutoCast(uint hSkillID)
	{
		return PrivSkillDataAutoCast(hSkillID);
	}

	public CSkillDataBase DoScriptSkill(uint hSkillID)
	{
		CSkillDataBase pSkillData = DoScriptSkillActive(hSkillID);
		if (pSkillData == null)
		{
			pSkillData = DoScriptSkillPassive(hSkillID);
			if (pSkillData == null)
			{
				pSkillData = DoScriptSkillAutoCast(hSkillID);
			}
		}
		return pSkillData;
	}


	//------------------------------------------------------------------------
	private SHSkillDataActive PrivSkillDataActive(uint hSkillID)
	{
		SHSkillDataActive pSkillDataActive = new SHSkillDataActive();
		if (m_mapSkillActive.ContainsKey(hSkillID))
		{
			SkillActive pSkillActive = m_mapSkillActive[hSkillID];
			pSkillActive.ReadSkillActive(pSkillDataActive);
		}
		else
		{
			Debug.LogError($"[SkillData] Invalid SkillID {hSkillID}");
		}

		return pSkillDataActive;
	}

	private SHSkillDataPassive PrivSkillDataPassive(uint hSkillID)
	{
		SHSkillDataPassive pSkillDataPassive = new SHSkillDataPassive();
		if (m_mapSkillPassive.ContainsKey(hSkillID))
		{
			SkillPassive pSkillPassive = m_mapSkillPassive[hSkillID];
			
		}
		else
		{
			Debug.LogError($"[SkillData] Invalid SkillID {hSkillID}");
		}

		return pSkillDataPassive;
	}

	private SHSkillDataAutoCast PrivSkillDataAutoCast(uint hSkillID)
	{
		SHSkillDataAutoCast pSkillDataAutoCast = new SHSkillDataAutoCast();
		if (m_mapSkillAutoCast.ContainsKey(hSkillID))
		{
			SkillAutoCast pSkillAutoCast = m_mapSkillAutoCast[hSkillID];
			
		}
		else
		{
			Debug.LogError($"[SkillData] Invalid SkillID {hSkillID}");
		}

		return pSkillDataAutoCast;
	}

}
