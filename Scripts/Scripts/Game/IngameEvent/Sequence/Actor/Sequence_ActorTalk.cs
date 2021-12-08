using UnityEngine;
using System.Collections;

namespace IngameEvent
{
	/// <summary> 대사 출력 </summary>
	public class Sequence_ActorTalk : Sequence_ActorBase
	{
		[Header("대사")]
		[SerializeField]
		private string LocaleId;

		[Header("대사 출력 시간")]
		[SerializeField]
		private float TalkDuration = 3f;

		[Header("대기 시간")]
		[SerializeField]
		private float WaitDuration = 1f;

		[Header("텍스트 색상")]
		[SerializeField]
		private Color TextColor = Color.white;

		protected override void BeginEventImpl()
		{
			UIManager.Instance.Find<UIFrameNameTag>()?.DoUINameTagChattingMessage(Pawn, DBLocale.GetText(LocaleId), TextColor, TalkDuration);
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
