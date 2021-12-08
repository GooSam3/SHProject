using UnityEngine;
using System.Collections;

namespace IngameEvent
{
	/// <summary> 대기 </summary>
	public class Sequence_WaitForSeconds : IngameEventSequenceBase
	{
		[Header("대기 시간")]
		[SerializeField]
		private float WaitDuration = 1f;
		protected override void BeginEventImpl()
		{
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
