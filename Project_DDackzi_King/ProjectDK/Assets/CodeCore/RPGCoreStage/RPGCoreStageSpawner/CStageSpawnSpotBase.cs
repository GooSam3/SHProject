using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CStageSpawnSpotBase : CMonoBase
{
	[SerializeField]
	private float		SpawnDelay = 0;
	[SerializeField]
	protected bool	Reader = false;

	protected CUnitBase m_pUnitInstance = null;
	//---------------------------------------------
	protected override void OnUnityAwake()
	{
		base.OnUnityAwake();
		m_pUnitInstance = GetComponentInChildren<CUnitBase>();
		if (m_pUnitInstance != null)
		{
			m_pUnitInstance.SetMonoActive(false);
		}
	}


	internal void ImportSpawnSpotActivate()
	{
		if (SpawnDelay > 0)
		{
			Invoke("HandleSpawnDelay", SpawnDelay);
		}
		else
		{
			HandleSpawnDelay();
		}
	}

	public bool GetSpawnSpotAlive()
	{
		bool bAlive = false;
		if (m_pUnitInstance)
		{
			bAlive = m_pUnitInstance.IsAlive;
		}

		return bAlive;
	}

	//-----------------------------------------------
	private void HandleSpawnDelay()
	{
		OnSpawnSpotActivate();
	}

	//------------------------------------------------
	protected virtual void OnSpawnSpotActivate() { }
}
