using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EUnitSocket
{
	None,
	Head,
	ArmLeft,
	ArmRight,
	LegLeft,
	LegRight,
	Body,
	Ground,
	Random_All,
}

public class SHUnitSocketAssist : CUnitSocketAssistBase
{

	public Transform GetUnitSocketTrasform(EUnitSocket eSocketID)
	{
		int iSocketID = (int)eSocketID;
		if (eSocketID == EUnitSocket.Random_All)
		{
			iSocketID = Random.Range((int)EUnitSocket.Head, (int)EUnitSocket.Ground);
		}

		return GetUnitSocketTrasform(iSocketID);
	}
}
