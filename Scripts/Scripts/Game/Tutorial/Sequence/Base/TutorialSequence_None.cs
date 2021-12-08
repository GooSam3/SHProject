using GameDB;
using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

/// <summary> 튜토리얼 개별 시퀀스 </summary>
public class TutorialSequence_None : MonoBehaviour
{
	private Action<bool> OnFinish;
	protected Tutorial_Table TutorialTable;

	protected ZUIFrameBase UIFrame = null;

	protected TutorialPlayer Owner;

	public E_TutorialType TutorialType { get { return TutorialTable.TutorialType; } }

	/// <summary> 찾을 UI </summary>
	protected virtual string FindUIName { get; } = string.Empty;

	/// <summary> 시작 딜레이 타임 </summary>
	protected virtual float StartDelayTime { get; } = 0.2f;

	/// <summary> 강조할 UI </summary>
	protected virtual Transform GetHighlightObject()
	{
		return null;
	}

	protected virtual bool IsStartScreenLock { get; } = true;

	private int InvokeCount = 0;

	private const int MaxInvokeCount = 50;

	public void Initialize(TutorialPlayer owner, Tutorial_Table table, Action<bool> onFinish)
	{
		TutorialTable = table;
		OnFinish = onFinish;

		Owner = owner;

		SetParams(TutorialTable.GuideParams);
	}

	/// <summary> 플레이 </summary>
	public void PlaySequence()
	{
		CancelInvoke(nameof(PlaySequence));

		if(string.IsNullOrEmpty(FindUIName))
		{
			//찾을 ui가 없다면 바로 처리
			UIFrame = null;
			StartSequence();
		}
		else
		{
			//찾을 ui가 있다면 open될 때까지 반복
			UIFrame = UIManager.Instance.Find<ZUIFrameBase>(FindUIName);
			if (null == UIFrame)
			{
				Invoke(nameof(PlaySequence), 0.1f);
			}
			else
			{
				StartSequence();
			}
		}
	}

	/// <summary> 시작! </summary>
	private void StartSequence()
	{
		//열린 프레임 셋팅
		Owner.SetLastOpenedUIFrame(UIFrame);

		SetBlockScereen(IsStartScreenLock, true);

		Invoke(nameof(InvokeDelayStartSequence), StartDelayTime);
	}

	private void InvokeDelayStartSequence()
	{
		CancelInvoke(nameof(InvokeDelayStartSequence));

		if(false == IsStartScreenLock)
			SetBlockScereen(true, true);

		switch (TutorialTable.TutorialType)
		{
			case E_TutorialType.Dialogue:       //다이얼로그만 출력하자!!!
				StartDialogue();
				break;
			case E_TutorialType.Guide:          //각 타입에 맞는 가이드를 실행하자!!
				StartGuide();
				break;
		}
	}

	/// <summary> Dialogue 출력 </summary>
	private void StartDialogue()
	{
		if (true == string.IsNullOrEmpty(TutorialTable.Text))
		{
			ZLog.LogError(ZLogChannel.Quest, $"[{TutorialTable.TutorialID}] 표시할 다이얼로그가 없음");
			EndSequence();
			return;
		}

		Owner.ShowDialogueUI(TutorialTable.NpcNameText, TutorialTable.Text, TutorialTable.DialogueResource, EndSequence);
	}

	/// <summary> 가이드 시작 </summary>
	protected virtual void StartGuide()
	{
		
	}

	protected bool CheckStartGuideInvoke()
	{
		if (InvokeCount >= MaxInvokeCount)
		{
			ZLog.LogError(ZLogChannel.Quest, $"튜토리얼 진행 불가 상태. 다음 스텝 진행. {GetType().Name}");

			EndSequence(false);

			return false;
		}

		++InvokeCount;

		return true;
	}

	/// <summary> 가이드 시작 각 시퀀스에서 호출하자 </summary>
	protected void ShowGuide(GameObject target, Action onFocusClick, Action<PointerEventData> onFocusPointUp = null, Action<PointerEventData> onFocusPointDown = null)
	{
		//가이드 노출
		ShoGuideUI();

		//포커스 노출
		FocusUI(target, onFocusClick, onFocusPointUp, onFocusPointDown);
	}

	/// <summary> 가이드 ui 연출 처리 </summary>
	private void ShoGuideUI()
	{
		if (true == string.IsNullOrEmpty(TutorialTable.Text))
		{
			ZLog.Log(ZLogChannel.Quest, $"[{TutorialTable.TutorialID}] 표시할 가이드가 없음");
			return;
		}	

		Owner.ShowGuideUI(TutorialTable.NpcNameText, TutorialTable.Text, TutorialTable.GuideResource);
	}

	protected void HideGuideUI()
	{
		Owner.HideGuideUI();
		HideFocusUI();
	}

	/// <summary> UI 포커스 </summary>
	protected void FocusUI(GameObject target, Action onClick, Action<PointerEventData> onFocusPointUp = null, Action<PointerEventData> onFocusPointDown = null)
	{
		Owner.FocusUI(target, GetHighlightObject(), onClick, onFocusPointUp, onFocusPointDown);
	}

	/// <summary> UI 포커스 숨김 </summary>
	protected void HideFocusUI()
	{
		Owner.HideFocusUI();
	}

	/// <summary> 화면 블락UI 셋팅 </summary>
	protected void SetBlockScereen(bool bActive, bool bAlpha)
	{
		Owner.SetBockScreen(bActive, bAlpha);
	}

	/// <summary> 종료! </summary>
	private void EndSequence()
	{
		EndSequence(false);
	}

	/// <summary> 종료 </summary>
	protected void EndSequence(bool bSkip)
	{
		if(true == bSkip)
		{
			//스킵용 메지시 출력 후 종료
			Owner.ShowDialogueUI(TutorialTable.NpcNameText, TutorialTable.SkipText, TutorialTable.DialogueResource, () =>
			{				
				OnFinish?.Invoke(true);
			});
		}
		else
		{
			OnFinish?.Invoke(false);
		}
	} 

	/// <summary> 넘어온 옵션값 셋팅 </summary>
	protected virtual void SetParams(List<string> args)
	{
	}
}