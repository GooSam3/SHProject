using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace IngameEvent
{
	/// <summary> 대사창 출력 </summary>
	public class Sequence_ActorDialog : Sequence_ActorBase
	{
		[Header("선택지 리스트")]
		[SerializeField]
		private List<IngameEventDialogChoice> Choices = new List<IngameEventDialogChoice>();

		[Header("마지막 다이얼로그에 셋팅할 것!(다이얼로그 닫는 용도)")]
		[SerializeField]
		private bool IsLastDialog = false;

		[Header("대사")]
		[SerializeField]
		private string DescLocaleId;

		[Header("플레이할 animation (없으면 패스)")]
		[SerializeField]
		private string AnimationKey;

		[Header("말풍선으로 표시할지 여부")]
		[SerializeField]
		private bool IsSpeechBubble = false;

		[Header("말풍선 텍스트 색상")]
		[SerializeField]
		private Color SpeechBubbleColor = Color.white;

		[Header("Actor가 셋팅 안됐을 경우 출력할 name")]
		[SerializeField]
		private string DialogName;

		[Header("Actor가 셋팅 안됐을 경우 출력할 Image")]
		[SerializeField]
		private string DialogImageName;

		protected override void BeginEventImpl()
		{
		}

		protected override void EndEventImpl()
		{
		}

		protected override IEnumerator Co_Update()
		{
			bool bWait = true;

			List<string> choises = new List<string>();

			foreach(var choise in Choices)
			{
				choises.Add(choise.LocaleId);
			}

			string dialogName = Actor?.ActorName ?? DialogName;
			string dialogIamageName = Actor?.ActorImageName ?? DialogImageName;

			UIManager.Instance.Open<UIFrameDialog>((assetName, frame) =>
			{
				if(false == IsSpeechBubble)
				{
					frame.Set(dialogName, DescLocaleId, dialogIamageName, choises, (select) =>
					{
						if(0 < select)
						{
							EventPlayer.ChangeEventGroup(Choices[select].SelectEventGroup);
						}

						bWait = false;
					});
				}
				else
				{
					if(0 < choises.Count && null != Pawn)
					{
						UIManager.Instance.Find<UIFrameNameTag>()?.DoUINameTagChattingMessage(Pawn, DBLocale.GetText(DescLocaleId), SpeechBubbleColor, 99999999);

						frame.Set(choises, (select) =>
						{
							if (0 < select)
							{
								EventPlayer.ChangeEventGroup(Choices[select].SelectEventGroup);
							}

							bWait = false;							
						});
					}
					else
					{
						bWait = false;
					}
				}
			});

			//애니메이션 플레이
			if (null != Actor)
				Actor.PlayCustomAnimation(AnimationKey);

			while (true == bWait)
				yield return null;

			if(null != Pawn && true == IsSpeechBubble && 0 < choises.Count)
				UIManager.Instance.Find<UIFrameNameTag>()?.DoUINameTagChattingMessageHide(Pawn);

			var uiframe = UIManager.Instance.Find<UIFrameDialog>();
			if (null != uiframe)
				uiframe.CloseDialog(()=>
				{
					EndEvent();
				});
			else
			{
				EndEvent();
			}
		}
	}
}
