using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary> 특정 위치로 이동하고 이동이 중지된다면 quest hud를 포커싱한다. </summary>
public class TutorialSequence_MoveTo : TutorialSequence_FocusButton<UISubHUDQuest>
{
	/// <summary> 해당 ui에서 버튼을 찾는데 사용할 경로 </summary>
	protected override string Path { get { return "HUD_Quest_Sub/Body/Quest_List/Scroll View/Viewport/Content/HUD_Quest_Slot/Btn_QuestMainUI"; } }

	protected override string HighlightPath { get { return "HUD_Quest_Sub/Body/Quest_List/Scroll View/Viewport/Content/HUD_Quest_Slot"; } }

	/// <summary> 도착 위치 </summary>
	protected Vector3? DestPosition = null;

	/// <summary> 도착 범위 </summary>
	protected float DestRange = 5f * 5f;

	private bool IsFucus = true;

	private UISubHUDQuestItem QuestItem;

	/// <summary> 해당 버튼 가지고 오기 </summary>
	protected override Selectable GetSelectable()
	{
		var selectable = base.GetSelectable();

		QuestItem = selectable.GetComponentInParent<UISubHUDQuestItem>();

		return selectable;
	}

	private void OnDestroy()
	{
		QuestItem?.DoResetEventClickButtonQuestOpen();
	}

	//protected override Transform GetHighlightObject()
	//{
	//	return mSelectable.transform;
	//}

	protected override void ButtonAction()
	{
		if (Check())
		{
			IsFucus = false;

			HideGuideUI();
			QuestItem?.DoSubHudQuestItemAutoPilot(true);
			//hud click 이벤트 추가
			QuestItem.DoSetEventClickButtonQuestOpen(ButtonAction);


			SetBlockScereen(false, false);

			if(false == CheckArrive())
			{
				ZPawnManager.Instance.MyEntity.StartAIForMoveTo(ZGameModeManager.Instance.StageTid, DestPosition.Value);
			}

			InvokeCheckPosition();
		}
		else
		{
			QuestItem?.DoSubHudQuestItemAutoPilot(false);
			//hud click 이벤트 제거
			QuestItem.DoResetEventClickButtonQuestOpen();
			EndSequence(true);
		}
	}

	protected override bool Check()
	{
		return true;
	}

	private void InvokeCheckPosition()
	{
		CancelInvoke(nameof(InvokeCheckPosition));

		//도착했는지 판단. 도착했으면 클리어. 아니라면 hud 포커스
		var myPc = ZPawnManager.Instance.MyEntity;

		//도착 체크
		if (true == CheckArrive())
		{
			QuestItem?.DoSubHudQuestItemAutoPilot(false);
			//기본 ai로 바꿈 - 내부에서 이동 중지 시킴
			myPc.StartDefaultAI();
			//hud click 이벤트 제거
			QuestItem.DoResetEventClickButtonQuestOpen();

			PostArrive();
		}
		else
		{
			//조작으로 이동중인지 여부
			if (ZPawnManager.Instance.MyEntity.IsInputMove() || ZPawnManager.Instance.MyEntity.IsMovingDir() || myPc.CurrentAIType != E_PawnAIType.MoveTo)
			{
				if (false == IsFucus)
				{
					IsFucus = true;
					QuestItem?.DoSubHudQuestItemAutoPilot(false);
					//가이드 다시 시작
					//StartGuide();
				}
			}

			Invoke(nameof(InvokeCheckPosition), 0.5f);
		}
	}

	/// <summary> 도착 체크 </summary>
	protected virtual bool CheckArrive()
	{
		return null == DestPosition || (ZPawnManager.Instance.MyEntity.Position - DestPosition.Value).sqrMagnitude <= DestRange;
	}

	/// <summary> 도착 이후 처리 </summary>
	protected virtual void PostArrive()
	{
		EndSequence(false);
	}

	protected override void SetParams(List<string> args)
	{
		//목적지 셋팅
		DestPosition = new Vector3(float.Parse(args[0]), float.Parse(args[1]), float.Parse(args[2]));
	}
}