using Com.TheFallenGames.OSA.CustomAdapters.GridView;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary> 스페셜 상점 ui에서 해당 아이템을 선택한다. </summary>
public class TutorialSequence_TouchSpecialShopItem : TutorialSequence_FocusListItemByCellViewsHolder<UIFrameSpecialShop, SpecialShopCategoryDescriptor.SingleDataInfo>
{
	protected override string Path { get { return ""; } }

	protected override Button GetButton(GameObject item)
	{
		return item.GetComponent<Button>();
	}

	protected override bool CheckAdapter()
	{
		return null != OwnerUI?.ItemScrollAdapter?.SingleData?.List;
	}
	protected override List<SpecialShopCategoryDescriptor.SingleDataInfo> GetDataList()
	{
		return OwnerUI?.ItemScrollAdapter?.SingleData?.List ?? null;
	}

	protected override CellViewsHolder GetVeiwHolder(int index)
	{
		return OwnerUI.ItemScrollAdapter.GetCellViewsHolderIfVisible(index);
	}

	protected override void ScrollTo(int index)
	{
		OwnerUI.ItemScrollAdapter.ScrollTo(index);
	}

	protected override bool CheckData(SpecialShopCategoryDescriptor.SingleDataInfo data, uint tid)
	{
		return data.specialShopId == tid;
	}

	protected override bool Check()
	{
		// TODO :: 재화 체크해야하나?

		return true;
	}
}