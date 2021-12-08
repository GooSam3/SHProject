using UnityEngine;
using System.Collections;

namespace IngameEvent
{
	/// <summary> 애니메이션을 플레이한다. </summary>
	public class Sequence_ActorPlayAnim : Sequence_ActorBase
	{
		[Header("플레이할 애니메이션 키")]
		[SerializeField]
		private string CustomAnimationKey;

		[Header("대기 시간")]
		[SerializeField]
		private float WaitDuration = 0f;

		protected override void BeginEventImpl()
		{
			Actor.PlayCustomAnimation(CustomAnimationKey);		
		}

		protected override void EndEventImpl()
		{			
		}

		protected override IEnumerator Co_Update()
		{
			yield return new WaitForSeconds(WaitDuration);
			EndEvent();
		}
	}
}
