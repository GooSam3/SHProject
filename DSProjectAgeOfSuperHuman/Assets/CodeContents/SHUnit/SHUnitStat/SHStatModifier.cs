using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EStatModifierType
{
	None,
	Buff,
	LinkedStat,
	Equip,
}

public class SHStatModifier : CStatModifierBase
{
	//---------------------------------------------------------------------------
	public void DoSHStatModifierConst(ESHStatType eSHStatType, EStatModifierType eModifierType, float fValue, IStatModifierOwner pStatModifierOwner)
	{
		ProtStatModifierConst((uint)eSHStatType, (uint)eModifierType, fValue, pStatModifierOwner);
	}


}
