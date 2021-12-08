using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using System.Reflection;
using NSkill;
public class DKScriptSkill : CScriptDataXMLBase
{
	private Dictionary<uint, SkillActive>	m_mapSkillActive = new Dictionary<uint, SkillActive>();
	private Dictionary<uint, SkillPassive>	m_mapSkillPassive = new Dictionary<uint, SkillPassive>();
	private Dictionary<uint, SkillAutoCast> m_mapSkillAutoCast = new Dictionary<uint, SkillAutoCast>();
	private List<uint> m_listCommonSkillID = new List<uint>();
	//----------------------------------------------------------
	protected override void OnScriptXMLRoot(XmlElement pRootElem)
	{
		RootSkillData pRoot = new RootSkillData();
		pRoot.DoScriptDataLoad(pRootElem,  "NSkill.", Assembly.GetExecutingAssembly());

		for (int i = 0; i < pRoot.m_listChildElement.Count; i++)
		{
			bool bCommon = false;

			SkillCommon pSkillCommon = pRoot.m_listChildElement[i] as SkillCommon;
			if (pSkillCommon != null)
			{
				bCommon = true;
			}

			for (int j = 0; j < pRoot.m_listChildElement[i].m_listChildElement.Count; j++)
			{
				SkillActive pSkillActive = pRoot.m_listChildElement[i].m_listChildElement[j] as SkillActive;
				if (pSkillActive != null)
				{
					m_mapSkillActive[pSkillActive.hSkillID] = pSkillActive;
					if (bCommon)
					{
						m_listCommonSkillID.Add(pSkillActive.hSkillID);
					}
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
	public DKSkillDataActive DoScriptSkillActive(uint hSkillID)
	{
		return PrivSkillDataActive(hSkillID);
	}

	public DKSkillDataPassive DoScriptSkillPassive(uint hSkillID)
	{
		return PrivSkillDataPassive(hSkillID);
	}

	public DKSkillDataAutoCast DoScriptSkillAutoCast(uint hSkillID)
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

	public void DoScriptSkillCommon(List<DKSkillDataActive> pOutSkillList)
	{
		for (int i = 0; i < m_listCommonSkillID.Count; i++)
		{
			DKSkillDataActive pSkillDataActive = PrivSkillDataActive(m_listCommonSkillID[i]);
			if (pSkillDataActive != null)
			{
				pOutSkillList.Add(pSkillDataActive);
			}
		}
	}

	//------------------------------------------------------------------------
	private DKSkillDataActive PrivSkillDataActive(uint hSkillID)
	{
		DKSkillDataActive pSkillDataActive = new DKSkillDataActive();
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

	private DKSkillDataPassive PrivSkillDataPassive(uint hSkillID)
	{
		DKSkillDataPassive pSkillDataPassive = new DKSkillDataPassive();
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

	private DKSkillDataAutoCast PrivSkillDataAutoCast(uint hSkillID)
	{
		DKSkillDataAutoCast pSkillDataAutoCast = new DKSkillDataAutoCast();
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
