using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class CStageSpawnerBase : CMonoBase
{
	[SerializeField]
	private uint Order = 0; public uint GetStageSpawnerOrder() { return Order; }
	protected bool m_bActivate = false;
	protected List<CStageSpawnSpotBase> m_listSpawnSpotInstance = new List<CStageSpawnSpotBase>();
	//-------------------------------------------------
	protected override void OnUnityStart()
	{
		base.OnUnityStart();	
		GetComponentsInChildren(true, m_listSpawnSpotInstance);
		if (CManagerStageSpawnerBase.Instance != null)
		{
			CManagerStageSpawnerBase.Instance.ImportStageSpawnerRegist(this);
		}

		SetMonoActive(false);
	}

	private void Update()
	{
		if (m_bActivate)
		{
			OnUnityUpdate();
		}
	}

	//--------------------------------------------------
	internal void ImportSpawnerInitialize()
	{
		m_bActivate = true;
		SetMonoActive(true);
		OnSpawnerInitialize();
	}

	internal void ImportSpawnerReset()
	{
		m_bActivate = false;
		SetMonoActive(false);
		OnSpawnerReset();
	}

	//---------------------------------------------------
	protected virtual void OnSpawnerInitialize() { }
	protected virtual void OnSpawnerReset() { }
	protected virtual void OnUnityUpdate() { }
}
