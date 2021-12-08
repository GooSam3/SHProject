using Com.TheFallenGames.OSA.CustomAdapters.GridView;
using System.Collections.Generic;

/// <summary> 요리 ui에서 해당 재료들을 선택한다 </summary>
public class TutorialSequence_TouchCookMaterial : TutorialSequence_FocusListItemByCellViewsHolder<UIFrameCook, ScrollCookInvenData>
{
	protected override string Path { get { return "ItemSlot_Share_Parts/Item_Icon"; } }

	protected override bool CheckAdapter()
	{
		return null != OwnerUI?.MaterialListScrollAdapter?.Data?.List;
	}
	protected override List<ScrollCookInvenData> GetDataList()
	{
		return OwnerUI?.MaterialListScrollAdapter?.Data?.List ?? null;
	}

	protected override CellViewsHolder GetVeiwHolder(int index)
	{
		return OwnerUI.MaterialListScrollAdapter.GetCellViewsHolderIfVisible(index);
	}

	protected override void ScrollTo(int index)
	{
		OwnerUI.MaterialListScrollAdapter.ScrollTo(index);
	}

	protected override bool CheckData(ScrollCookInvenData data, uint tid)
	{
		return data.Item.item_tid == tid;
	}

	protected override bool Check()
	{
		if(true == TryGetParam(out uint tid))
		{
			//요리 재료 체크
			if (true == ZNet.Data.Me.CurCharData.CheckCountInvenItemUsingMaterial(tid, 1))
				return true;
		}
		

		return false;
	}
}