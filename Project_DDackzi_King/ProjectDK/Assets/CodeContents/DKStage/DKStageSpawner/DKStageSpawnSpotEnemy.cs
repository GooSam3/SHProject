using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DKStageSpawnSpotEnemy : CStageSpawnSpotBase
{
	//------------------------------------------------
	protected override void OnSpawnSpotActivate()
	{
		DKUnitBase pDKUnit = m_pUnitInstance as DKUnitBase;

		if (pDKUnit == null) // ���̺� ������ ��� �� ������ ������� ����  
		{

		}
		else // �ùķ��̼��� ��� ���� �Ҵ�� �ν��Ͻ� ���
		{
			DKManagerUnit.Instance.DoMgrUnitSpawnByInstance(EUnitType.Enemy, 0, pDKUnit);
			DKManagerStageSpawner.Instance.SetMgrStageSpawnerFormation(false, pDKUnit, Reader);
		}
	}
}
