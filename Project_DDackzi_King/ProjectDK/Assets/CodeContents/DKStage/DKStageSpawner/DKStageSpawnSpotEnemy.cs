using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DKStageSpawnSpotEnemy : CStageSpawnSpotBase
{
	//------------------------------------------------
	protected override void OnSpawnSpotActivate()
	{
		DKUnitBase pDKUnit = m_pUnitInstance as DKUnitBase;

		if (pDKUnit == null) // 테이블 스폰의 경우 덱 정보를 기반으로 스폰  
		{

		}
		else // 시뮬레이션의 경우 씬에 할당된 인스턴스 사용
		{
			DKManagerUnit.Instance.DoMgrUnitSpawnByInstance(EUnitType.Enemy, 0, pDKUnit);
			DKManagerStageSpawner.Instance.SetMgrStageSpawnerFormation(false, pDKUnit, Reader);
		}
	}
}
