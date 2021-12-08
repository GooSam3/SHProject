using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DKUnitSocketAssist : CUnitSocketAssistBase
{
	//-----------------------------------------------------------
	public Vector3 GetSocketWorldPosition(EUnitSocket eUnitSocket, bool bScreenPosition = false)
	{
		Vector3 vecPosition = Vector3.zero;

		CUnitSocketBase pSocket = FindSocket((int)eUnitSocket);
		if (pSocket)
		{
			vecPosition = pSocket.GetSocketWorldPosition();

			if (bScreenPosition)
			{
				vecPosition = Camera.main.WorldToScreenPoint(vecPosition);
			}
		}
		return vecPosition;
	}

	public Transform GetSocketTransform(EUnitSocket eUnitSocket)
	{
		CUnitSocketBase pSocket = FindSocket((int)eUnitSocket);
		if (pSocket)
		{
			return pSocket.transform;
		}
		else
		{
			return null;
		}
	}
}
