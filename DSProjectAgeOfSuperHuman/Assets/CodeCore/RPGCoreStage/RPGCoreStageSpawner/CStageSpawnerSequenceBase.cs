using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CStageSpawnerSequenceBase : CStageSpawnerBase
{
	//------------------------------------------------------
	protected override void OnSpawnerInitialize()
	{
		base.OnSpawnerInitialize();
	}

	protected override void OnSpawnerStart() 
	{
		base.OnSpawnerStart();
		ProtStageSpawnerStart();
	}

	protected override void OnUnityUpdate()
	{
		base.OnUnityUpdate();

		bool bAlive = false;
		for (int i = 0; i < m_listSpawnSpotInstance.Count; i++)
		{
			if (m_listSpawnSpotInstance[i].GetSpawnSpotAlive())
			{
				bAlive = true;
				break;
			}
		}

		if (bAlive == false)
		{
			m_bActivate = false;
			ProtStageSpawnerNextSpawn();
		}
	}
	//----------------------------------------------------------------
	protected void ProtStageSpawnerStart()
	{
		for (int i = 0; i < m_listSpawnSpotInstance.Count; i++)
		{
			m_listSpawnSpotInstance[i].ImportSpawnSpotActivate();
		}
	}

	//---------------------------------------------------------------
	
	
}
