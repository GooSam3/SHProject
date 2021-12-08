using UnityEngine;
using System.Collections;

namespace IngameEvent
{
	/// <summary> 액터 소환 </summary>
	public class Sequence_ActorSpawn : Sequence_ActorBase
	{
		protected override void BeginEventImpl()
		{
			Actor.Spawn(() =>
			{
				EndEvent();
			});
		}

		protected override void EndEventImpl()
		{
		}
	}
}
