using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DKStageSpawnSpotHero : CStageSpawnSpotBase
{
	[SerializeField]
	private int DeckSlot = 0;

	//---------------------------------------------------------------
	protected override void OnSpawnSpotActivate()
	{
		DKUnitBase pDKUnit = m_pUnitInstance as DKUnitBase;

		if (pDKUnit == null) // 테이블 스폰의 경우 덱 정보를 기반으로 스폰  
		{
			
		}
		else // 시뮬레이션의 경우 씬에 할당된 인스턴스 사용
		{
			pDKUnit.transform.position = transform.position;
			DKManagerUnit.Instance.DoMgrUnitSpawnByInstance(EUnitType.Firend, DeckSlot, pDKUnit);
			DKManagerStageSpawner.Instance.SetMgrStageSpawnerFormation(true, pDKUnit, Reader);
		}
	}
}
