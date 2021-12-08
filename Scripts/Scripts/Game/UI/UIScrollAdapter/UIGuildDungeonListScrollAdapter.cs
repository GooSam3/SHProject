using Com.TheFallenGames.OSA.Core;
using Com.TheFallenGames.OSA.CustomParams;
using Com.TheFallenGames.OSA.DataHelpers;
using GameDB;
using System;
using System.Collections.Generic;

public class UIGuildDungeonListScrollAdapter : OSA<BaseParamsWithPrefab, UIGuildDungeonListItemHolder>
{
	public SimpleDataHelper<Stage_Table> Data;
	private Action<Stage_Table> ClickEvent;

	public void Initialize(Action<Stage_Table> clickEvent)
	{
		Data = new SimpleDataHelper<Stage_Table>(this);
		ClickEvent = clickEvent;

		Init();
	}

	protected override UIGuildDungeonListItemHolder CreateViewsHolder(int itemIndex)
	{
		var instance = new UIGuildDungeonListItemHolder();

		instance.Init(_Params.ItemPrefab, _Params.Content, itemIndex);

		return instance;
	}

	public void Refresh(List<Stage_Table> dataList)
	{
		if(dataList != null)
		{
			Data.ResetItems(dataList);
		}
	}

	public void UpdateScrollItem(uint selectedStageId)
	{
		for (int i = 0; i < base.GetItemsCount(); i++)
		{
			if (GetItemViewsHolder(i) != null)
			{
				GetItemViewsHolder(i).Item.ActiveSelectImage(selectedStageId);
			}
		}
	}

	protected override void UpdateViewsHolder(UIGuildDungeonListItemHolder holder)
	{
		holder.Item.SetData(Data[holder.ItemIndex], ClickEvent);
	}
}

public class UIGuildDungeonListItemHolder : BaseItemViewsHolder
{
    public UIGuildDungeonListItem Item { get; private set; }

	public override void CollectViews()
	{
        Item = root.GetComponent<UIGuildDungeonListItem>();
		base.CollectViews();
	}
}