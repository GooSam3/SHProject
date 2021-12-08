using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CUnitMovableBase : CUnitBase
{
	private enum ENavAgentMoveType
	{
		None,
		Position,
		Object,
	}

	public enum ENavAgentEvent
	{
		Finish,
		InterruptStop,
		PathFindFail,
	}



	protected void ProtNavMoveDestination(Vector3 vecDest, float fStopRange, UnityAction<CNavAgentBase.ENavAgentEvent, Vector3> delFinish)
	{
		
	}

	protected void ProtNavMoveObject(CUnitBase pTargetObject, float fStopRange, UnityAction<CNavAgentBase.ENavAgentEvent, Vector3> delFinish)
	{
		
	}

	protected void ProtNavMoveForward(Vector3 vecDirection)
	{
		
	}
}
