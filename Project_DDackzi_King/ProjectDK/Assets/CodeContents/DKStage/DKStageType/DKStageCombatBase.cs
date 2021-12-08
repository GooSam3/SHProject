using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DKStageCombatBase : DKStageBase
{
	//--------------------------------------------------------
	protected override void OnStageEnter()
	{
		base.OnStageEnter();
		DKManagerStageSpawner.Instance.DoMgrStageSpawnerStart();

	}
}
