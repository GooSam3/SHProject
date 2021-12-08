using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DKUnitSocket : CUnitSocketBase
{
	[SerializeField]
	private EUnitSocket TagType = EUnitSocket.None;

	//------------------------------------------------------------
	protected sealed override int OnGetTagID()
	{
		return (int)TagType;
	}
}
