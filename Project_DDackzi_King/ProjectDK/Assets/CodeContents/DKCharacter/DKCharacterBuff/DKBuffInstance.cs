using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EBuffTaskEventType
{
	None,
	Start,
	End,
	OverTime,
	DamageTo,
	DamageFrom,
	HealTo,
	HealFrom,
	CrowdControlTo,
	CrowdControlFrom,
}

public class DKBuffInstance : CBuffBase
{
	//-------------------------------------------------------------
	public void SetBuffInitializeInfo(uint hBuffID, float fEventOverTime, EBuffUIType eBuffUIType, int iStackAccumulate, bool bExclusive, bool bCountUp, bool bTimeReset, bool bPowerUp)
	{
		ProtBuffInitialize(hBuffID, fEventOverTime, eBuffUIType, iStackAccumulate, bExclusive, bCountUp, bTimeReset, bPowerUp);
	}

	public void SetBuffTaskAdd(EBuffTaskEventType eTaskEventType, CBuffTaskConditionBase pBuffTaskCondition)
	{
		ProtBuffTaskAdd((uint)eTaskEventType, pBuffTaskCondition);
	}
}
