using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ��� �ɸ��͸� �ѹ��� ���� 
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
