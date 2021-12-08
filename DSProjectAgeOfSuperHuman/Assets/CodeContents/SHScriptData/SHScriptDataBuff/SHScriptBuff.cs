using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Reflection;
using NBuff;

public class SHScriptBuff : CScriptDataXMLBase
{
	private Dictionary<uint, BuffData> m_mapBuffData = new Dictionary<uint, BuffData>();
	//------------------------------------------------------------------------------
	protected override void OnScriptXMLRoot(XmlElement pRootElem)
	{
		RootBuffData pRoot = new RootBuffData();
		pRoot.DoScriptDataLoad(pRootElem, "NBuff.", Assembly.GetExecutingAssembly());

		for (int j = 0; j < pRoot.m_listChildElement.Count; j++)
		{
			for (int i = 0; i < pRoot.m_listChildElement[j].m_listChildElement.Count; i++)
			{
				BuffData pBuffData = pRoot.m_listChildElement[j].m_listChildElement[i] as BuffData;
				if (pBuffData != null)
				{
					m_mapBuffData[pBuffData.hBuffID] = pBuffData;
				}
			}
		}
	}

	//--------------------------------------------------------------------------------
	public SHBuffInstance DoScriptBuffMakeInstance(uint hBuffID)
	{
		SHBuffInstance pNewInstance = null;

		if (m_mapBuffData.ContainsKey(hBuffID))
		{
			BuffData pBuffData = m_mapBuffData[hBuffID];
			pNewInstance = pBuffData.MakeBuffInstance();
		}
		else
		{
			Debug.LogError("[Buff] Invalid BuffID : " + hBuffID);
		}


		return pNewInstance;
	}
}
