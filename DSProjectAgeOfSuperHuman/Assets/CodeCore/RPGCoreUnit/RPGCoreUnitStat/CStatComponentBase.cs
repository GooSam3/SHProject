using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class CStatGroupBase : CObjectInstanceBase
{
	public void DoStatGroupInitialize(IStatOwner pStatOwner, List<CStatBase> pListOutInstance)
	{
		OnStatGroupInitialize(pStatOwner, pListOutInstance);
	}
	public void DoStatGroupRefresh(IStatOwner pStatOwner)
	{
		OnStatGroupRefresh(pStatOwner);
	}

	protected virtual void OnStatGroupInitialize(IStatOwner pStatOwner, List<CStatBase> pListOutInstance) { }
	protected virtual void OnStatGroupRefresh(IStatOwner pStatOwner) { }
}

// 다양한 스텟을 컴포넌트에서 볼수 있도록 노출
abstract public class CStatComponentBase : CMonoBase
{
	public const float c_GlobalChance = 10000f;
	private Dictionary<uint, CStatBase> m_mapStatInstance = new Dictionary<uint, CStatBase>();
	protected CUnitStatBase m_pStatOwner = null;
	bool m_bInitialize = false;
	internal void ImportStatInitialize(CUnitStatBase pStatOwner)
	{
		m_pStatOwner = pStatOwner;
		if (m_bInitialize == false)
		{
			m_bInitialize = true;
			DoStatMakeInstance();
			DoStatReset();
			OnStatComponentInitialize();
		}
	}

	public void DoStatMakeInstance() // 각 스텟간의 관계를 정의
	{
		m_mapStatInstance.Clear();		
		List<CStatBase> pListStatInstance = new List<CStatBase>();
		OnStatComponentArrange(m_pStatOwner, pListStatInstance);

		for (int i = 0; i < pListStatInstance.Count; i++)
		{
			uint hStatID = pListStatInstance[i].GetStatType();
			if (m_mapStatInstance.ContainsKey(hStatID))
			{
				//Error!
			}
			else
			{
				m_mapStatInstance[hStatID] = pListStatInstance[i];
			}
		}

		OnStatComponentChainLink();
	}

	public void DoStatReset()
	{
		OnStatComponentReset();
	}

	public void DoStatComponentModifierAdd(CStatModifierBase pStatModifier)
	{
		CStatBase pFindStat = FindStatInstance(pStatModifier.hStatID);
		if (pFindStat != null)
		{
			pFindStat.DoStatModifierAdd(pStatModifier);
		}
	}

	public void DoStatComponentModifierDelete(CStatModifierBase pStatModifier)
	{
		CStatBase pFindStat = FindStatInstance(pStatModifier.hStatID);
		if (pFindStat != null)
		{
			pFindStat.DoStatModifierRemove(pStatModifier);
		}
	}
	//----------------------------------------------------------
	protected CStatBase FindStatInstance(uint hStatType)
	{
		CStatBase pFindStat = null;

		if (m_mapStatInstance.ContainsKey(hStatType))
		{
			pFindStat = m_mapStatInstance[hStatType];
		}

		return pFindStat;
	}

	//----------------------------------------------------------
	protected virtual void OnStatComponentArrange(CUnitStatBase pStatOwner, List<CStatBase> pLitOutInstance) { }
	protected virtual void OnStatComponentChainLink() { }
	protected virtual void OnStatComponentReset() { }
	protected virtual void OnStatComponentInitialize() { }
}
