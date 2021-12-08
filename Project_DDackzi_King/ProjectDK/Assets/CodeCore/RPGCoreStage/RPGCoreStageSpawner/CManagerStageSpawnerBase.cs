using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CManagerStageSpawnerBase : CManagerTemplateBase<CManagerStageSpawnerBase>
{
	private Dictionary<uint, CStageSpawnerBase> m_mapStageSpawnerInstance = new Dictionary<uint, CStageSpawnerBase>();
	private uint m_iCurrentSpawner = 0;
	private CStageSpawnerBase m_pCurrentStageSpawner = null;
	//----------------------------------------------------------------------------
	protected override void OnUnityAwake()
	{
		base.OnUnityAwake();
	}

	protected override void OnManagerScriptLoaded()
	{
		base.OnManagerScriptLoaded();
		CStageSpawnerBase[] aSpawner = FindObjectsOfType<CStageSpawnerBase>();

		for (int i = 0; i < aSpawner.Length; i++)
		{
			PrivStageSpawnerRegist(aSpawner[i]);
		}
	}
	//----------------------------------------------------------------------------
	public void DoMgrStageSpawnerStart(bool bReset = false)
	{
		if (bReset)
		{
			PrivStageSpawnerReset();
		}
		PrivStageSpawnerNext();
		OnStageSpawnerStart();
	}

	public void DoMgrStageSpawnerNext()
	{
		PrivStageSpawnerNext();
	}

	internal void ImportStageSpawnerRegist(CStageSpawnerBase pSpawner)
	{
		PrivStageSpawnerRegist(pSpawner);
	}

	//----------------------------------------------------------------------------
	private void PrivStageSpawnerRegist(CStageSpawnerBase pSpawner)
	{
		uint Order = pSpawner.GetStageSpawnerOrder();
		if (m_mapStageSpawnerInstance.ContainsKey(Order))
		{
			return;
		}

		m_mapStageSpawnerInstance.Add(Order, pSpawner);
	}

	private void PrivStageSpawnerNext()
	{
		if (m_mapStageSpawnerInstance.ContainsKey(m_iCurrentSpawner))
		{
			CStageSpawnerBase pStageSpawner = m_mapStageSpawnerInstance[m_iCurrentSpawner];
			m_pCurrentStageSpawner = pStageSpawner;
			m_iCurrentSpawner++;

			pStageSpawner.ImportSpawnerInitialize();
			OnStageSpawnerActive(pStageSpawner);
		}
		else
		{
			//Error!
		}	
	}

	private void PrivStageSpawnerReset()
	{		
		m_iCurrentSpawner = 0;
		m_pCurrentStageSpawner = null;
		Dictionary<uint, CStageSpawnerBase>.Enumerator it = m_mapStageSpawnerInstance.GetEnumerator();
		while(it.MoveNext())
		{
			it.Current.Value.ImportSpawnerReset();
		}
		OnStageSpawnerReset();
	}

	//--------------------------------------------------------------------------------
	protected void OnStageSpawnerStart() { }
	protected void OnStageSpawnerReset() { }
	protected void OnStageSpawnerActive(CStageSpawnerBase pActiveSpawner) { }
}
