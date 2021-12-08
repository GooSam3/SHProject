using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class CStageSpawnerBase : CMonoBase
{
	[SerializeField]
	private int Order = 0;		public int GetStageSpawnerOrder() { return Order; }
	[SerializeField]
	private int NextOrder = 0;		protected void SetStageSpawnerNextOrder(int iNextOrder) { NextOrder = iNextOrder; }

	private bool m_bEmpty = false; public bool IsSpawnerEmpty { get { return m_bEmpty; } }
	protected bool m_bActivate = false;	
	protected List<CStageSpawnSpotBase> m_listSpawnSpotInstance = new List<CStageSpawnSpotBase>();
	
	//-------------------------------------------------
	protected override void OnUnityAwake()
	{
		base.OnUnityAwake();
		SetMonoActive(false);
	}

	protected override void OnUnityStart()
	{
		base.OnUnityStart();
		if (CManagerStageSpawnerBase.Instance != null)
		{
			CManagerStageSpawnerBase.Instance.ImportStageSpawnerRegist(this);
		}
	}

	private void Update()
	{
		if (m_bActivate)
		{
			OnUnityUpdate();
		}
	}
	//--------------------------------------------------
	public int GetStageSpawnerSpotCount() { return m_listSpawnSpotInstance.Count; }
	//--------------------------------------------------
	internal void ImportSpawnerInitialize()
	{
		SetMonoActive(false);
		GetComponentsInChildren(true, m_listSpawnSpotInstance);
		
		for (int i = 0; i < m_listSpawnSpotInstance.Count; i++)
		{
			m_listSpawnSpotInstance[i].ImportSpawnSpotInitialize();
		}
		PrivStageSpawnerEmptyCheck();
		OnSpawnerInitialize();
	}

	internal void ImportSpawnerStart()
	{
		m_bActivate = true;
		SetMonoActive(true);
		OnSpawnerStart();
	}

	internal void ImportSpawnerReset()
	{
		m_bActivate = false;
		SetMonoActive(false);
		for (int i = 0; i < m_listSpawnSpotInstance.Count; i++)
		{
			m_listSpawnSpotInstance[i].ImportSpawnSpotReset();
		}
		
		OnSpawnerReset();
	}

	internal void ImportSpawnerPause()
	{
		if (m_bActivate)
		{
			m_bActivate = false;

			for (int i = 0; i < m_listSpawnSpotInstance.Count; i++)
			{
				m_listSpawnSpotInstance[i].ImportSpawnSpotPause();
			}

			OnSpawnerPause();
		}
	}

	internal void ImportSpawnerResume()
	{
		if (m_bActivate == false)
		{			
			m_bActivate = true;

			for (int i = 0; i < m_listSpawnSpotInstance.Count; i++)
			{
				m_listSpawnSpotInstance[i].ImportSpawnSpotResume();
			}

			OnSpawnerResume();
		}
	}

	internal void ImportStageSpawnerDeAcvite(CUnitBase pUnit)
	{
		for (int i = 0; i < m_listSpawnSpotInstance.Count; i++)
		{
			if (m_listSpawnSpotInstance[i].GetStageSpawnSpotUnit() == pUnit)
			{
				m_bActivate = false;
				break;
			}
		}
	}
	//---------------------------------------------------
	private void PrivStageSpawnerEmptyCheck()
	{
		bool bEmpty = true;
		for (int i = 0; i < m_listSpawnSpotInstance.Count; i++)
		{
			if (m_listSpawnSpotInstance[i].GetStageSpawnSpotUnit() == null)
			{
				bEmpty = false;
				break;
			}
		}
		m_bEmpty = bEmpty;
	}


	//---------------------------------------------------
	protected void ProtStageSpawnerSetUnit(List<CUnitBase> pListUnit)
	{
		for (int i = 0; i < pListUnit.Count; i++)
		{
			if (i < m_listSpawnSpotInstance.Count)
			{
				m_listSpawnSpotInstance[i].ImportSpawnSpotSetUnit(pListUnit[i]);
			}
		}
	}

	protected void ProtStageSpawnerNextSpawn()
	{
		CManagerStageSpawnerBase.Instance.DoMgrStageSpawnerNext(NextOrder);
		OnSpawnerNextSpawn(); 
	}

	//---------------------------------------------------
	protected virtual void OnSpawnerInitialize() { }
	protected virtual void OnSpawnerReset() { }
	protected virtual void OnSpawnerStart() { }
	protected virtual void OnSpawnerPause() { }
	protected virtual void OnSpawnerResume() { }
	protected virtual void OnSpawnerNextSpawn() { }

	protected virtual void OnUnityUpdate() { }
}
