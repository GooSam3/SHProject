using Com.TheFallenGames.OSA.Core;
using Com.TheFallenGames.OSA.CustomParams;
using Com.TheFallenGames.OSA.DataHelpers;
using System;
using System.Collections.Generic;

public class UIDropItemListScrollAdapter : OSA<BaseParamsWithPrefab, UIDropListItemHolder>
{
	public SimpleDataHelper<RewardItem> Data;
	private Action<uint> ClickEvent;

	public void Initialize(Action<uint> clickEvent = null)
	{
		Data = new SimpleDataHelper<RewardItem>(this);
		ClickEvent = clickEvent;

		Init();
	}

	protected override UIDropListItemHolder CreateViewsHolder(int itemIndex)
	{
		var instance = new UIDropListItemHolder();

		instance.Init(_Params.ItemPrefab, _Params.Content, itemIndex);

		return instance;
	}

	public void Refresh(List<RewardItem> dataList)
	{
		if (dataList != null)
		{
			Data.ResetItems(dataList);
		}
	}

	protected override void UpdateViewsHolder(UIDropListItemHolder holder)
	{
		holder.Item.SetData(Data[holder.ItemIndex], ClickEvent);
	}
}

public class UIDropListItemHolder : BaseItemViewsHolder
{
	public UIRewardableListItem Item { get; private set; }

	public override void CollectViews()
	{
		Item = root.GetComponent<UIRewardableListItem>();
		base.CollectViews();
	}
}