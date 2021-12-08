using Com.TheFallenGames.OSA.Core;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary> 튜토리얼 기반 </summary>
public class TutorialSequence_QuickSlotBase : TutorialSequence_FocusListItemByBaseViewHolder<UISubHUDQuickSlot, QuickSlotScrollData>
{
	protected override string Path { get { return ""; } }

	private int GuideParam = 0;

	/// <summary> 각 퀵슬롯의 index </summary>
	private int QuickSlotIndex { get { return GuideParam % 4; } }

	/// <summary> 퀵슬롯 아답터 인덱스 </summary>
	private int AdapterIndex { get { return GuideParam / 4; } }

	protected UIQuickItemSlot QuickSlot { get; private set; }

	/// <summary> 해당 버튼 가지고 오기 </summary>
	protected override Button GetButton(GameObject item)
	{
		var list = item.GetComponentsInChildren<UIQuickItemSlot>();

		QuickSlot = list[QuickSlotIndex];

		return QuickSlot.GetComponent<Button>();
	}

	protected override bool CheckAdapter()
	{
		return null != OwnerUI?.ScrollAdapter[AdapterIndex]?.Data?.List;
	}
	protected override List<QuickSlotScrollData> GetDataList()
	{
		return OwnerUI?.ScrollAdapter[AdapterIndex]?.Data?.List ?? null;
	}

	protected override BaseItemViewsHolder GetVeiwHolder(int index)
	{
		return OwnerUI.ScrollAdapter[AdapterIndex].GetBaseItemViewsHolderIfVisible(index);
	}

	protected override void ScrollTo(int index)
	{
		OwnerUI.ScrollAdapter[AdapterIndex].BaseParameters.Snapper._LastSnappedItemIndex = index;
		OwnerUI.ScrollAdapter[AdapterIndex].ScrollTo(index);
	}

	protected override bool CheckData(QuickSlotScrollData data, uint tid)
	{
		return data.HoldIdx == AdapterIndex;
	}

	protected override void SetParams(List<string> args)
	{
		if(0 >= args.Count)
		{
			ZLog.LogError(ZLogChannel.Quest, "GuideParams 셋팅되지 않음");
			return;
		}	
		int.TryParse(args[0], out GuideParam);
	}

	protected override bool Check()
	{
		// TODO : 수정바람
		return true;
	}

	private float ReadyEndTime = 0f;
	protected override bool IsReady()
	{
		if(0 == ReadyEndTime)
		{
			ReadyEndTime = Time.time;
		}

		if(ReadyEndTime + 0.5f > Time.time)
		{
			return false;
		}

		return true;
	}
}