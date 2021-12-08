using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DKStageSpawnerEnemyNormal : CStageSpawnerSequenceBase
{
	[SerializeField]
	private float EncountLength = 3f;

	//------------------------------------------------------------
	protected override void OnSpawnerInitialize()
	{
		base.OnSpawnerInitialize();
		// 스폰 끝나면 대형 이동을 지시
		DKManagerStageSpawner.Instance.DoMgrStageSpawnerMoveEnemyFormation(EncountLength);
	}
}
