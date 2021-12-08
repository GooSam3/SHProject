using UnityEngine;
using System.Collections;

namespace IngameEvent
{
	/// <summary> 액터 소환 </summary>
	public abstract class Sequence_ActorBase : IngameEventSequenceBase
	{
		[Header("관리할 이벤트 액터 셋팅")]
		[SerializeField]
		protected IngameEventActorBase Actor;

		protected ZPawn Pawn { get { return Actor?.Pawn; } }

	}
}
