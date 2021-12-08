using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHStageSpawnSpotHero : CStageSpawnSpotBase
{
	//------------------------------------------------------
	protected override void OnSpawnSpotActivate()
	{
		if (m_pUnitInstance == null) return;

		SHUnitHero pHero = m_pUnitInstance as SHUnitHero;
		SHManagerUnit.Instance.DoMgrUnitRegist(pHero);
	}


}
