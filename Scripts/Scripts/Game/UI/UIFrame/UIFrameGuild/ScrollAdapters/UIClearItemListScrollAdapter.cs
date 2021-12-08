using Com.TheFallenGames.OSA.Core;
using Com.TheFallenGames.OSA.CustomParams;
using Com.TheFallenGames.OSA.DataHelpers;
using System;
using System.Collections.Generic;

public class UIClearItemListScrollAdapter : OSA<BaseParamsWithPrefab, UIClearListItemHolder>
{
	public SimpleDataHelper<RewardItem> Data;
	private Action<uint> ClickEvent;

	public void Initialize(Action<uint> clickEvent = null)
	{
		Data = new SimpleDataHelper<RewardItem>(this);
		ClickEvent = clickEvent;

		Init();
	}

	protected override UIClearListItemHolder CreateViewsHolder(int itemIndex)
	{
		var instance = new UIClearListItemHolder();

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

	protected override void UpdateViewsHolder(UIClearListItemHolder holder)
	{
		holder.Item.SetData(Data[holder.ItemIndex], ClickEvent);
	}
}

public class UIClearListItemHolder : BaseItemViewsHolder
{
    public UIRewardableListItem Item { get; private set; }

	public override void CollectViews()
	{
		Item = root.GetComponent<UIRewardableListItem>();
		base.CollectViews();
	}
}