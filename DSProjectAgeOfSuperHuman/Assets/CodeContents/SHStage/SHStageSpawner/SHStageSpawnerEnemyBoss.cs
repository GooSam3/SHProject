using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHStageSpawnerEnemyBoss : SHStageSpawnerEnemy
{
	//------------------------------------------------------
	protected override void OnSpawnerInitialize()
	{
		base.OnSpawnerInitialize();
		List<SHStageSpawnSpotEnemy> pListSpot = new List<SHStageSpawnSpotEnemy>();
		GetComponentsInChildren(true, pListSpot);
		for (int i = 0; i < pListSpot.Count; i++)
		{
			pListSpot[i].SetSpawnSpotBoss(true);
		}
	}

	protected override void OnSpawnerNextSpawn()
	{
		if (m_bSpawnEnd)
		{
			SHManagerStage.Instance.GetMgrStageCurrent().DoStageEnd();
		}
		else
		{
			ProtStageSpawnerMoveForward(null);
		}
	}

	protected override void OnSpawnerStart()
	{
		base.OnSpawnerStart();

	}
	//------------------------------------------------------
	public void DoStageSpawnerBossStart()
	{
		SHManagerUnit.Instance.DoMgrUnitDisableCurrent();
		UIManager.Instance.DoUIMgrFind<SHUIFrameCombatHero>().DoUIFrameCombatEnemyGaugeReset(null);
		ProtStageSpawnerMoveForward(() =>
		{
			SHManagerStageSpawner.Instance.DoMgrStageSpawnerNext(GetStageSpawnerOrder());
		});
	}

	//-------------------------------------------------------
	
}
