using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 모든 케릭터를 한번에 스폰 
public abstract class CStageSpawnerAllBase : CStageSpawnerBase
{
	protected override void OnSpawnerInitialize()
	{
		base.OnSpawnerInitialize();
	}

	protected override void OnSpawnerStart()
	{
		base.OnSpawnerStart();
		for (int i = 0; i < m_listSpawnSpotInstance.Count; i++)
		{
			m_listSpawnSpotInstance[i].ImportSpawnSpotActivate();
		}

		ProtStageSpawnerNextSpawn();
	}

}
