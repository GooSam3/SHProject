using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DKFormationHero : DKFormationBase
{
	protected override void OnFormationReaderUnit(DKUnitBase pUnit)
	{
		DKManagerCamera.Instance.DoCameraAttachUnit(pUnit);
	}

	protected override void OnFormationAllDeath()
	{
		// 스테이지 실패
	}

}
