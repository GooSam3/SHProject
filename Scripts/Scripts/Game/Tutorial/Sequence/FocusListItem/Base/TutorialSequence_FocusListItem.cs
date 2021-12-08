using Com.TheFallenGames.OSA.Core;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary> 리스트 아이템 기반 </summary>
public abstract class TutorialSequence_FocusListItem<UI_FRAME_TYPE, SIMPLEDATA_TYPE, VIEW_HOLDER> : TutorialSequence_UIFrameBase<UI_FRAME_TYPE>
	where UI_FRAME_TYPE : CUIFrameBase
	where SIMPLEDATA_TYPE : class
	where VIEW_HOLDER : AbstractViewsHolder
{
	/// <summary> 해당 ui에서 버튼을 찾는데 사용할 경로 </summary>
	protected abstract string Path { get; }

	protected virtual string HighlightPath { get { return Path; } }

	protected Button mButton { get; private set; }

	//protected int CurrentIndex { get; private set; }

	/// <summary> 해당 버튼 가지고 오기 </summary>
	protected virtual Button GetButton(GameObject item)
	{
		return item.transform.Find(Path).GetComponent<Button>();
	}

	/// <summary> 아답타 체크 </summary>
	protected abstract bool CheckAdapter();

	/// <summary> osa 아답터 데이터 오기 </summary>
	protected abstract List<SIMPLEDATA_TYPE> GetDataList ();

	/// <summary> 찾으려는 데이터 체크 </summary>
	protected abstract bool CheckData(SIMPLEDATA_TYPE data, uint tid);

	/// <summary> item 가지고 오기 </summary>
	protected abstract RectTransform GetItemTrans(int index);

	protected abstract VIEW_HOLDER GetVeiwHolder(int index);
	/// <summary> 스크롤 위치 변경 </summary>
	protected abstract void ScrollTo(int index);

	/// <summary> 찾으려는 데이터 인덱스 가지고 오기 </summary>
	private int GetItemIndex(uint tid)
	{
		var list = GetDataList();

		for(int i = 0; i < list.Count; ++i)
		{
			if (false == CheckData(list[i], tid))
				continue;
			
			return i;
		}

		return -1;
	}

	protected override void StartGuide()
	{
		CancelInvoke(nameof(StartGuide));

		//예외사항 체크
		if (false == Check())
		{
			EndSequence(true);
			return;
		}			

		if (false == CheckStartGuideInvoke())
			return;

		if (false == CheckAdapter())
		{
			Invoke(nameof(StartGuide), 0.1f);
			return;
		}

		if(false == IsReady())
		{
			Invoke(nameof(StartGuide), 0.1f);
			return;
		}

		//파라미터에 해당하는 tid를 찾는다.
		if (true == TryGetParam(out uint tid))
		{
			int index = GetItemIndex(tid);

			if (0 > index)
			{
				Invoke(nameof(StartGuide), 0.1f);
				return;
			}

			//해당 위치로 스크롤
			ScrollTo(index);

			var item = GetItemTrans(index);

			if (null == item)
			{
				Invoke(nameof(StartGuide), 0.1f);
				return;
			}

			if (null == mButton)
				mButton = GetButton(item.gameObject);

			if (null == mButton || false == mButton.gameObject.activeSelf)
			{
				Invoke(nameof(StartGuide), 0.1f);
				return;
			}

		}
		//버튼 가이드 시작
		ShowGuide(mButton.gameObject, ButtonAction, HandleEventPointUp, HandleEventPointDown);
	}

	protected virtual void ButtonAction()
	{
		mButton?.onClick.Invoke();

		//if (CurrentIndex < TutorialTable.GuideParams.Count)
		//{
		//	StartGuide();
		//}
		//else
		{
			EndSequence(false);
		}
	}

	protected virtual void HandleEventPointUp(PointerEventData eventData)
	{

	}

	protected virtual void HandleEventPointDown(PointerEventData eventData)
	{

	}

	/// <summary> 예외 체크용. 아이템 존재 여부 체크 </summary>
	protected abstract bool Check();

	/// <summary> 필요한 데이터 땡겨가기 </summary>
	protected virtual bool TryGetParam(out uint tid)
	{
		return uint.TryParse(TutorialTable.GuideParams[0], out tid);
	}

	protected virtual bool IsReady()
	{
		return true;
	}
}