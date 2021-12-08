using UnityEngine;
using System.Collections.Generic;

namespace IngameEvent
{
	/// <summary> Ai를 실행한다. Blackboard 추가할까?</summary>
	public class EventActorActionAI : EventActorActionBase
	{
		[Header("Ai Type")]
		[SerializeField]
		private E_PawnAIType AIType;

		protected override void BeginActionImpl()
		{
		}

		protected override void StopActionImpl()
		{

		}
	}
}
