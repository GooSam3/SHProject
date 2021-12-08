using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public abstract class CManagerStageSpawnerBase : CManagerTemplateBase<CManagerStageSpawnerBase>
{
	protected SortedDictionary<int, CStageSpawnerBase> m_mapStageSpawnerInstance = new SortedDictionary<int, CStageSpawnerBase>();
	private CStageSpawnerBase m_pCurrentStageSpawner = null;  
	private int m_iFirstSpawnOrder = 0;						public int GetStageSpanwerFirstOrder() { return m_iFirstSpawnOrder; }
	
	//----------------------------------------------------------------------------
	protected override void OnUnityAwake()
	{
		base.OnUnityAwake();
	}

	//------------------------------------------------------------------------------
	public void DoMgrStageSpawnerInitialize()
	{
		PrivStageSpawnerCollectSpawner();
	}

	public void DoMgrStageSpawnerStart(bool bReset = false)
	{
		if (bReset)
		{
			PrivStageSpawnerReset();
		}
		PrivStageSpawnerFirst();
		OnStageSpawnerStart();
	}

	public void DoMgrStageSpawnerNext(int iNextOrder)
	{
		PrivStageSpawnerNext(iNextOrder);
	}

	public void DoMgrStageSpawnerPause()
	{
		if (m_pCurrentStageSpawner != null)
		{
			m_pCurrentStageSpawner.ImportSpawnerPause();
			OnStageSpawnerPause(m_pCurrentStageSpawner);
		}
	}

	public void DoMgrStageSpawnerResume()
	{
		if (m_pCurrentStageSpawner != null)
		{
			m_pCurrentStageSpawner.ImportSpawnerResume();
			OnStageSpawnerResume(m_pCurrentStageSpawner);
		}
	}

	public void DoMgrStageSpawnerEnd()
	{
		PrivStageSpawnerEnd();
	}

	public void DoMgrStageSpawnerDeActive(CUnitBase pUnit)
	{
		SortedDictionary<int, CStageSpawnerBase>.Enumerator it = m_mapStageSpawnerInstance.GetEnumerator();
		while (it.MoveNext())
		{
			it.Current.Value.ImportStageSpawnerDeAcvite(pUnit);
		}
	}

	//---------------------------------------------------------
	internal void ImportStageSpawnerRegist(CStageSpawnerBase pSpawner)
	{
		PrivStageSpawnerRegist(pSpawner);
	}

	//----------------------------------------------------------------------------
	private void PrivStageSpawnerCollectSpawner()
	{
		m_mapStageSpawnerInstance.Clear();
		m_pCurrentStageSpawner = null;
		m_iFirstSpawnOrder = 0;

		CStageSpawnerBase[] aSpawner = FindObjectsOfType<CStageSpawnerBase>(true);
		for (int i = 0; i < aSpawner.Length; i++)
		{
			PrivStageSpawnerRegist(aSpawner[i]);
		}
	}

	private void PrivStageSpawnerRegist(CStageSpawnerBase pSpawner)
	{
		int Order = pSpawner.GetStageSpawnerOrder();
		if (m_mapStageSpawnerInstance.ContainsKey(Order))
		{
			return;
		}

		pSpawner.ImportSpawnerInitialize();
		m_mapStageSpawnerInstance.Add(Order, pSpawner);
	}

	private void PrivStageSpawnerNext(int iOrder)
	{
		if (m_mapStageSpawnerInstance.ContainsKey(iOrder))
		{
			CStageSpawnerBase pStageSpawner = m_mapStageSpawnerInstance[iOrder];
			m_pCurrentStageSpawner = pStageSpawner;
			pStageSpawner.ImportSpawnerStart();
			OnStageSpawnerActive(pStageSpawner);
		}
		else
		{
			PrivStageSpawnerEnd();
		}		
	}

	private void PrivStageSpawnerReset()
	{		
		m_pCurrentStageSpawner = null;
		SortedDictionary<int, CStageSpawnerBase>.Enumerator it = m_mapStageSpawnerInstance.GetEnumerator();
		while(it.MoveNext())
		{
			it.Current.Value.ImportSpawnerReset();
		}
		OnStageSpawnerReset();
	}

	private void PrivStageSpawnerFirst()
	{
		SortedDictionary<int, CStageSpawnerBase>.Enumerator it = m_mapStageSpawnerInstance.GetEnumerator();
		if (it.MoveNext())
		{
			m_iFirstSpawnOrder = it.Current.Key;
			PrivStageSpawnerNext(m_iFirstSpawnOrder);
		}  
	}

	private void PrivStageSpawnerEnd()
	{
		m_pCurrentStageSpawner = null;
		OnStageSpawnerEnd();
	}

	//--------------------------------------------------------------------------------
	protected List<CStageSpawnerBase> ExtractStageSpawner()
	{
		return m_mapStageSpawnerInstance.Values.ToList();
	}

	protected CStageSpawnerBase GetStageSpawnerCurrent()
	{
		return m_pCurrentStageSpawner;
	}
	//--------------------------------------------------------------------------------
	protected virtual void OnStageSpawnerStart() { }
	protected virtual void OnStageSpawnerReset() { }
	protected virtual void OnStageSpawnerActive(CStageSpawnerBase pSpawner) { }
	protected virtual void OnStageSpawnerPause(CStageSpawnerBase pSpawner) { }
	protected virtual void OnStageSpawnerResume(CStageSpawnerBase pSpawner) { }
	protected virtual void OnStageSpawnerEnd() { }
}
