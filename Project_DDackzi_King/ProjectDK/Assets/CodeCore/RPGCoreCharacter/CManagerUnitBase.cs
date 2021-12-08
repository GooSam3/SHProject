using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class CManagerUnitBase : CManagerTemplateBase<CManagerUnitBase>
{
	private CMultiSortedDictionary<uint, CUnitBase> m_mapUnitIntance = new CMultiSortedDictionary<uint, CUnitBase>();
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
		//동적 객체는 메모리 해재 
		//정적 객체는 리셋 

	}

	//----------------------------------------------------------------
	protected void ProtMgrUnitRegist(uint eCategory, CUnitBase pUnit)
	{
		if (pUnit == null) return;

		m_mapUnitIntance.Add(eCategory, pUnit);
		pUnit.ImportUnitIniailize();
	}

	protected void ProtMgrUnitUnRegist(CUnitBase pUnit)
	{
		IEnumerator<List<CUnitBase>> it = m_mapUnitIntance.value.GetEnumerator();
		while(it.MoveNext())
		{
			List<CUnitBase> pList = it.Current;
			pList.Remove(pUnit);
			pUnit.ImportUnitLeave();
		}
	}

	protected void FindMgrUnit(uint eCategory, List<CUnitBase> pOutList)
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
}
