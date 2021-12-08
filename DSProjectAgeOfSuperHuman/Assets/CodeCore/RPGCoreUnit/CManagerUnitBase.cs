using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class CManagerUnitBase : CManagerTemplateBase<CManagerUnitBase>
{
	private CMultiSortedDictionary<CUnitBase.EUnitRelationType, CUnitBase> m_mapUnitIntance = new CMultiSortedDictionary<CUnitBase.EUnitRelationType, CUnitBase>();
	//---------------------------------------------------------------
	protected override void OnUnityAwake()
	{
		base.OnUnityAwake();		
	}

	protected override void OnManagerScriptLoaded()
	{
		base.OnManagerScriptLoaded();		
	}
	//---------------------------------------------------------------
	public void DoMgrUnitClearAll()
	{
		m_mapUnitIntance.Clear();
		OnMgrUnitClearAll();
	}

	//----------------------------------------------------------------
	protected void ProtMgrUnitRegist(CUnitBase pUnit)
	{
		if (pUnit == null) return;
		
		m_mapUnitIntance.Add(pUnit.GetUnitRelationForPlayer(), pUnit);
	}

	protected void ProtMgrUnitUnRegist(CUnitBase pUnit)
	{
		IEnumerator<List<CUnitBase>> it = m_mapUnitIntance.value.GetEnumerator();
		while(it.MoveNext())
		{
			List<CUnitBase> pList = it.Current;
			pList.Remove(pUnit);
		}
	}

	protected void FindMgrUnit(CUnitBase.EUnitRelationType eCategory, ref List<CUnitBase> pOutList)
	{
		if (m_mapUnitIntance.ContainsKey(eCategory))
		{
			List<CUnitBase> pListUnit = m_mapUnitIntance[eCategory];
			for (int i = 0; i < pListUnit.Count; i++)
			{
				pOutList.Add(pListUnit[i]);
			}		
		}
	}

	//------------------------------------------------------------------
	protected virtual void OnMgrUnitInitialize(CUnitBase pRegistUnit) { }
	protected virtual void OnMgrUnitClearAll() { }
}
