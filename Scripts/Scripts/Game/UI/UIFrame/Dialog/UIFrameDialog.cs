using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary> 공용 다이얼로그 처리. 연속된 메시지를 처리하진 않는다. 종료시 외부에서 닫아줘야한다. </summary>
public class UIFrameDialog : ZUIFrameBase
{
	[SerializeField]
	private Text TextName;

	[SerializeField]
	private Text TextDesc;

	[SerializeField]
	private ZRawImage NpcImage;

	[SerializeField]
	private Image DimmedImage;

	[SerializeField]
	private GameObject ObjBoard;

	[SerializeField]
	private CAnimationController AnimController;

	private List<UIFrameDialogChoiseItem> m_listChoiseItem = new List<UIFrameDialogChoiseItem>();

	private Action<int> mEventFinish;

	private float mStartTime;

	/// <summary> 선택지가 있다면 선택한 index. 없다면 -1 </summary>
	private int ChoiseIndex = -1;

	private bool IsChoise = false;

	private bool IsShowDialog = false;

	protected override void OnShow(int _LayerOrder)
	{
		base.OnShow(_LayerOrder);
		GetComponentsInChildren<UIFrameDialogChoiseItem>(true, m_listChoiseItem);

		UIManager.Instance.Open<UIFrameNameTag>();
	}

	protected override void OnHide()
	{
		base.OnHide();
		IsShowDialog = false;
	}

	protected override void OnRemove()
	{
		base.OnRemove();
		IsShowDialog = false;
	}

	/// <summary> 일반 다이얼로그 출력 </summary>
	public void Set(string name, string desc, string resourceName, Action<int> onFinish)
	{
		Set(name, desc, resourceName, new List<string>(), onFinish);		
	}

	/// <summary> 선택지가 포함된 다이얼로그 출력 </summary>
	public void Set(string name, string desc, string resourceName, List<string> choises, Action<int> onFinish)
	{
		ChoiseIndex = -1;
		
		IsChoise = null != choises && 0 < choises.Count;

		SetActiveSubUI(true);

		TextDesc.text = DBLocale.GetText(desc);
		TextName.text = DBLocale.GetText(name);

		mEventFinish = onFinish;

		mStartTime = Time.time;

		NpcImage.LoadTexture(resourceName);

		//선택지 처리
		SetChoises(choises);

		PostSetDialog();
	}

	/// <summary> 선택지만 나오게 할 경우 </summary>
	public void Set(List<string> choises, Action<int> onFinish)
	{
		mEventFinish = onFinish;

		SetActiveSubUI(false);

		//선택지 처리
		SetChoises(choises);

		PostSetDialog();
	}

	private void SetActiveSubUI(bool bActive)
	{
		NpcImage.gameObject.SetActive(bActive);
		ObjBoard.SetActive(bActive);
		DimmedImage.gameObject.SetActive(bActive);
	}

	private void PostSetDialog()
	{
		if (false == IsShowDialog)
		{
			AnimController.DoAnimationPlay("Ani_Quest_Dialog_Appear_Solo", (_name) =>
			{
			});
		}
		else
		{
			AnimController.DoAnimationPlay("Ani_Quest_Dialog_Appear", (_name) =>
			{
			});
		}

		IsShowDialog = true;
	}

	private void SetChoises(List<string> choises)
	{
		for(int i = 0; i < m_listChoiseItem.Count; ++i)
		{
			if(i < choises.Count)
			{
				m_listChoiseItem[i].gameObject.SetActive(true);
				m_listChoiseItem[i].Set(choises[i], i, OnChoiseItem);
			}
			else
			{
				m_listChoiseItem[i].gameObject.SetActive(false);
			}
		}
	}

	private void OnChoiseItem(int index)
	{
		ChoiseIndex = index;
		SetChoises(new List<string>());
		OnClickDialogue();
	}

	/// <summary> 다이얼로그를 닫는다. </summary>
	public void CloseDialog(Action onFinish)
	{		
		AnimController.DoAnimationPlay("Ani_Quest_Dialog_Disappear", (_name) =>
		{
			IsShowDialog = false;

			UIManager.Instance.Close<UIFrameDialog>(true);
			onFinish?.Invoke();
		});
	}

	/// <summary> 다이얼로그 클릭! </summary>
	public void OnClickDialogue()
	{
		if (AnimController.IsPlaying)
			return;

		//선택지가 있는데 선택을 안했다면 패스
		if (true == IsChoise && 0 > ChoiseIndex)
			return;

		//1초는 봐야지
		if (mStartTime + 1f > Time.time)
			return;

		mEventFinish?.Invoke(ChoiseIndex);
	}
}
