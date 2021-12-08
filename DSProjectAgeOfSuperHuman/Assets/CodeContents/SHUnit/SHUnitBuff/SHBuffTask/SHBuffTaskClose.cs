using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHBuffTaskClose : SHBuffTaskBase
{
	protected override void OnBuffTask(CBuffBase pBuff, CBuffComponentBase pBuffOwner, CBuffComponentBase pBuffOrigin)
	{
		SHUnitBase pUnitOwner = pBuffOwner.GetBuffOwner() as SHUnitBase;
		pUnitOwner.ISkillBuffEnd(pBuff.GetBuffID());
	}
}