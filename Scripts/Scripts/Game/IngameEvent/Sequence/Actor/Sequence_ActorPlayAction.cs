using UnityEngine;
using System.Collections;

namespace IngameEvent
{
	/// <summary> 액터에게 특수한 액션을 수행시킨다. </summary>
	public class Sequence_ActorPlayAction : Sequence_ActorBase
	{
		[Header("수행시킬 특수한 액션(EventActorAction~~~으로 찾을 것)")]
		[SerializeField]
		private EventActorActionBase Action;

		[Header("수행시킨 액션이 종료될 때까지 대기할지 여부")]
		[SerializeField]
		private bool IsWaitForEndAction;

		protected override void BeginEventImpl()
		{
			
		}

		protected override void EndEventImpl()
		{
		}

		protected override IEnumerator Co_Update()
		{
			bool bWaitAction = true;

			Actor.PlayActorAction(Action, () =>
			{
				bWaitAction = false;
			});

			if (true == IsWaitForEndAction)
			{
				while(true == bWaitAction)
				{
					yield return null;
				}
			}	

			EndEvent();
		}
	}
}
