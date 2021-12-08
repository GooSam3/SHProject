using System;
using UnityEngine;

public class UIRewardableListItem : UIItemSlot
{
	private Action<uint> ClickEvent;
	private uint SelectedItemId = 0;

    public void SetData(RewardItem rewardItem, Action<uint> clickEvent)
	{
		SelectedItemId = rewardItem.ItemId;
		base.SetItem(rewardItem.ItemId, rewardItem.ItemCount);

		ClickEvent = clickEvent;
	}

	public void ClickItem()
	{
		ClickEvent?.Invoke(SelectedItemId);
	}
}
