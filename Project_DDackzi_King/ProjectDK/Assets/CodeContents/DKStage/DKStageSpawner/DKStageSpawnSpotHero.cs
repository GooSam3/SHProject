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

		if (pDKUnit == null) // ���̺� ������ ��� �� ������ ������� ����  
		{
			
		}
		else // �ùķ��̼��� ��� ���� �Ҵ�� �ν��Ͻ� ���
		{
			pDKUnit.transform.position = transform.position;
			DKManagerUnit.Instance.DoMgrUnitSpawnByInstance(EUnitType.Firend, DeckSlot, pDKUnit);
			DKManagerStageSpawner.Instance.SetMgrStageSpawnerFormation(true, pDKUnit, Reader);
		}
	}
}
