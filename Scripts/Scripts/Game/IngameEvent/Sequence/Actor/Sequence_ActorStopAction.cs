using UnityEngine;
using System.Collections;

namespace IngameEvent
{
	/// <summary> 수행중인 액션이 있다면 종료한다. </summary>
	public class Sequence_ActorStopAction : Sequence_ActorBase
	{		
		protected override void BeginEventImpl()
		{
			Actor.StopActorAction();
			EndEvent();
		}

		protected override void EndEventImpl()
		{
		}
	}
}
