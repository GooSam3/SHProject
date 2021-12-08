using Com.TheFallenGames.OSA.CustomAdapters.GridView;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary> 인벤토리에서 아이템을 선택한다. </summary>
public class TutorialSequence_TouchInventoryItem : TutorialSequence_FocusListItemByCellViewsHolder<UIFrameInventory, ScrollInvenData>
{
	protected override string Path { get { return ""; } }

	protected override Button GetButton(GameObject item)
	{
		return item.GetComponent<Button>();
	}

	protected override bool CheckAdapter()
	{
		return null != OwnerUI?.ScrollAdapter?.Data?.List;
	}
	protected override List<ScrollInvenData> GetDataList()
	{
		return OwnerUI?.ScrollAdapter?.Data?.List ?? null;
	}

	protected override CellViewsHolder GetVeiwHolder(int index)
	{
		return OwnerUI.ScrollAdapter.GetCellViewsHolderIfVisible(index);
	}

	protected override void ScrollTo(int index)
	{
		OwnerUI.ScrollAdapter.ScrollTo(index);
	}

	protected override bool CheckData(ScrollInvenData data, uint tid)
	{
		return data.Item.item_tid == tid;
	}

	protected override bool Check()
	{
		if(true == TryGetParam(out uint tid))
		{
			//요리 재료 체크
			if (0 < ZNet.Data.Me.CurCharData.GetInvenCntUsingMaterial(tid))
				return true;
		}
		

		return false;
	}

	protected override bool IsReady()
	{
		return OwnerUI.IsInitialize;
	}
}