using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class CUnitSocketAssistBase : CMonoBase
{
	private Dictionary<int, CUnitSocketBase> m_mapCharTag = new Dictionary<int, CUnitSocketBase>();
	//----------------------------------------------------------
	protected override void OnUnityAwake()
	{
		base.OnUnityAwake();
		CUnitSocketBase[] aTag = GetComponentsInChildren<CUnitSocketBase>();
		for (int i = 0; i < aTag.Length; i++)
		{
			m_mapCharTag[aTag[i].GetSocketID()] = aTag[i];
		}
	}

	protected CUnitSocketBase FindSocket(int iTagID)
	{
		CUnitSocketBase FindTag = null;

		if (m_mapCharTag.ContainsKey(iTagID))
		{
			FindTag = m_mapCharTag[iTagID];
		}

		return FindTag;
	}
}
