using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHStatComponentEnemy : SHStatComponentBase
{

	//-------------------------------------------------------------------
	protected override void OnStatComponentArrange(CUnitStatBase pStatOwner, List<CStatBase> pLitOutInstance)
	{
		base.OnStatComponentArrange(pStatOwner, pLitOutInstance);
	}

	protected override void OnStatComponentReset()
	{
		if (UnitStatID != 0 && UnitLevel != 0)
		{
			CombatStat = SHManagerScriptData.Instance.ExtractTableStatEnemy().GetUnitStatEnemy(UnitStatID, UnitLevel);
		}
		base.OnStatComponentReset();
	}

	//-------------------------------------------------------------------
}
