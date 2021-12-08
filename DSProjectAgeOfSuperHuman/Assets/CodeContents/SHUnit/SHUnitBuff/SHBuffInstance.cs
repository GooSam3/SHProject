using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NBuff;


public class SHBuffInstance : CBuffBase
{
	//-----------------------------------------------------------
	public void SetBuffInitialize(uint hBuffID, uint hBuffInstanceID, string strBuffName, string strBuffIcon, EBuffType eBuffType,  EBuffUIType eBuffUIType, int iStackCount, float fEventOverTime, bool bExclusive, bool bCountUp, bool bTimeReset, bool bPowerUp, bool bTimeOverUp)
	{
		ProtBuffInitialize(hBuffID, hBuffInstanceID, strBuffName, strBuffIcon, (uint)eBuffType, eBuffUIType, fEventOverTime, iStackCount, bExclusive, bCountUp, bTimeReset, bPowerUp, bTimeOverUp);
	}

	public void SetBuffTaskCondition(EBuffTaskEventType eBuffTaskEvent, CBuffTaskConditionBase pBuffTaskCondition)
	{
		ProtBuffTaskConditionAdd((uint)eBuffTaskEvent, pBuffTaskCondition);
	}

	public EBuffType GetSHBuffType()
	{
		return (EBuffType)GetBuffType(); 
	}
}
