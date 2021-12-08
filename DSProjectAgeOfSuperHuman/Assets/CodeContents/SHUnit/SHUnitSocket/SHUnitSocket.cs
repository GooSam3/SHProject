using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHUnitSocket : CUnitSocketBase
{
	[SerializeField]
	private EUnitSocket TagType = EUnitSocket.None;
	//------------------------------------------------------------
	protected sealed override int OnGetTagID()
	{
		return (int)TagType;
	}
}
