using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]
public abstract  class CNavAgentBase : CMonoBase
{
	public enum ENavAgentEvent
	{
		Finish,
		InterruptStop,
		PathFindFail,
	}

	private enum ENavAgentMoveType
	{
		None,
		Position,
		Object,
	}
}
