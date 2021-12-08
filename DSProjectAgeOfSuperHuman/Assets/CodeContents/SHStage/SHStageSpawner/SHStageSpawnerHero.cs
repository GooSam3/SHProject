using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHStageSpawnerHero : CStageSpawnerAllBase
{
	//-------------------------------------------------
	public void DoStageSpawnerHero(List<SHUnitHero> pListHero)
	{
		List<CUnitBase> pListUnit = new List<CUnitBase>();
		for(int i = 0; i < pListHero.Count; i++)
		{
			pListUnit.Add(pListHero[i]);
		}

		ProtStageSpawnerSetUnit(pListUnit);
	}
}
