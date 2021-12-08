using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHStageSpawnSpotEnemy : CStageSpawnSpotBase
{
	private bool m_bSpawnBoss = false;
	//-------------------------------------------------------
	protected override void OnSpawnSpotActivate()
	{
		if (m_pUnitInstance == null) return;

		SHUnitEnemy pEnemy = m_pUnitInstance as SHUnitEnemy;
		SHManagerUnit.Instance.DoMgrUnitRegist(pEnemy, m_bSpawnBoss);
		UIManager.Instance.DoUIMgrFind<SHUIFrameCombatHero>()?.DoUIFrameCombatEnemyGaugeReset(pEnemy);
	}

	protected override void OnSpawnSpotPause()
	{
		base.OnSpawnSpotPause();
		if (m_pUnitInstance)
		{
			m_pUnitInstance.SetMonoActive(false);
			SHManagerUnit.Instance.DoMgrUnitUnRegist(m_pUnitInstance as SHUnitEnemy);
		}
	}

	//------------------------------------------------------------------------
	public void SetSpawnSpotBoss(bool bBoss)
	{
		m_bSpawnBoss = bBoss;
	}

}
