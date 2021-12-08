using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CStageSpawnSpotBase : CMonoBase
{
	[SerializeField]
	private float		SpawnDelay = 0;
	
	protected CUnitBase m_pUnitInstance = null;  public CUnitBase GetStageSpawnSpotUnit() { return m_pUnitInstance; }
	//---------------------------------------------
	protected override void OnUnityAwake()
	{
		base.OnUnityAwake();
	}
	internal void ImportSpawnSpotInitialize()
	{
		m_pUnitInstance = GetComponentInChildren<CUnitBase>(true);
		if (m_pUnitInstance != null)
		{
			m_pUnitInstance.DoUnitIniailize();
			m_pUnitInstance.SetMonoActive(false);
		}

		OnSpawnSpotInitialize();
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

	internal void ImportSpawnSpotSetUnit(CUnitBase pUnit)
	{
		if (m_pUnitInstance != null)
		{
			m_pUnitInstance.SetMonoActive(false);
		}
		m_pUnitInstance = pUnit;
		pUnit.SetMonoActive(false);
		pUnit.DoUnitIniailize();
		pUnit.transform.SetParent(transform, false);

		OnSpawnSpotSetUnit(pUnit);
	}

	internal void ImportSpawnSpotPause()
	{
		OnSpawnSpotPause();
	}

	internal void ImportSpawnSpotResume()
	{
		OnSpawnSpotResume();
	}

	internal void ImportSpawnSpotReset()
	{
		if (m_pUnitInstance != null)
		{
			m_pUnitInstance.DoUnitReset();
		}
		OnSpawnSpotReset();
	}

	//----------------------------------------------------------------
	public bool GetSpawnSpotAlive()
	{
		bool bAlive = true;
		if (m_pUnitInstance)
		{
			CUnitBase.EUnitState eState = m_pUnitInstance.GetUnitState();
			if (eState == CUnitBase.EUnitState.Remove)
			{
				bAlive = false;
			}
		}
		else
		{
			bAlive = false;
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
	protected virtual void OnSpawnSpotPause() { }
	protected virtual void OnSpawnSpotResume() { }
	protected virtual void OnSpawnSpotInitialize() { }
	protected virtual void OnSpawnSpotReset() { }
	protected virtual void OnSpawnSpotSetUnit(CUnitBase pUnit) { }
}
