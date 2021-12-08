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
		// ���� ������ ���� �̵��� ����
		DKManagerStageSpawner.Instance.DoMgrStageSpawnerMoveEnemyFormation(EncountLength);
	}
}
