using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class CUnitSocketBase : CMonoBase
{
	public int GetSocketID()
	{
		return OnGetTagID();
	}

	public Vector3 GetSocketWorldPosition()
	{
		return transform.position;
	}

	//--------------------------------------------------------
	protected virtual int OnGetTagID() { return 0;}
}
