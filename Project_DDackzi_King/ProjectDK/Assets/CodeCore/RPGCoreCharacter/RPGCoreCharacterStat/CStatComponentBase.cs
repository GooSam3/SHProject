using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class CStatGroupBase
{
	private CStatComponentBase m_pStatComponent = null;

	public void DoStatGroupInitialize(IStatOwner pStatOwner,  CStatComponentBase pStatComponent, List<CStatBase> pLitOutInstance)
	{
		m_pStatComponent = pStatComponent;
		OnStatGroupInitialize(pStatOwner, pLitOutInstance);
	}
	public void DoStatGroupRefresh()
	{
		OnStatGroupRefresh(m_pStatComponent);
	}

	protected virtual void OnStatGroupInitialize(IStatOwner pStatOwner, List<CStatBase> pLitOutInstance) { }
	protected virtual void OnStatGroupRefresh(CStatComponentBase pStatComponent) { }
}

// 다양한 스텟을 컴포넌트에서 볼수 있도록 노출
abstract public class CStatComponentBase : CMonoBase
{
	private Dictionary<int, CStatBase> m_mapStatInstance = new Dictionary<int, CStatBase>();
	protected IStatOwner m_pStatOwner = null;
	internal void ImportStatComponentArrange(IStatOwner pStatOwner) // 각 스텟간의 관계를 정의
	{
		m_pStatOwner = pStatOwner;
		List<CStatBase> pListStatInstance = new List<CStatBase>();
		OnStatComponentArrange(pStatOwner, pListStatInstance);

		for (int i = 0; i < pListStatInstance.Count; i++)
		{
			int hStatID = pListStatInstance[i].GetStatType();
			if (m_mapStatInstance.ContainsKey(hStatID))
			{
				//Error!
			}
			else
			{
				m_mapStatInstance[hStatID] = pListStatInstance[i];
			}
		}
	}

	internal void ImportStatReset()
	{
		OnStatComponentReset();
	}

	//----------------------------------------------------------
	protected CStatBase FindStatInstance(int hStatType)
	{
		CStatBase pFindStat = null;

		if (m_mapStatInstance.ContainsKey(hStatType))
		{
			pFindStat = m_mapStatInstance[hStatType];
		}

		return pFindStat;
	}
	
	//----------------------------------------------------------
	protected virtual void OnStatComponentArrange(IStatOwner pStatOwner, List<CStatBase> pLitOutInstance) { }
	protected virtual void OnStatComponentReset() { }
}
