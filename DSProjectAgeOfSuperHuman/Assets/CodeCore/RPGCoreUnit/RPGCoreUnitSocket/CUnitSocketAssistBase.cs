using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class CUnitSocketAssistBase : CMonoBase
{
	private Dictionary<int, CUnitSocketBase> m_mapUnitSocket = new Dictionary<int, CUnitSocketBase>();
	//----------------------------------------------------------
	protected override void OnUnityAwake()
	{
		base.OnUnityAwake();
		CUnitSocketBase[] aTag = GetComponentsInChildren<CUnitSocketBase>();
		for (int i = 0; i < aTag.Length; i++)
		{
			m_mapUnitSocket[aTag[i].GetSocketID()] = aTag[i];
		}
	}

	protected CUnitSocketBase FindSocket(int hSocketID)
	{
		CUnitSocketBase FindTag = null;

		if (m_mapUnitSocket.ContainsKey(hSocketID))
		{
			FindTag = m_mapUnitSocket[hSocketID];
		}

		return FindTag;
	}

	public Transform GetUnitSocketTrasform(int hSocketID)
	{
		Transform pFindTransform = null;

		if (m_mapUnitSocket.ContainsKey(hSocketID))
		{
			pFindTransform = m_mapUnitSocket[hSocketID].transform;
		}

		return pFindTransform;
	}
}
