using Com.TheFallenGames.OSA.Core;
using Com.TheFallenGames.OSA.CustomParams;
using Com.TheFallenGames.OSA.DataHelpers;
using GameDB;
using System;
using System.Collections.Generic;

public class UIBossWarListScrollAdapter : OSA<BaseParamsWithPrefab, UIBossWarListItemHolder>
{
    public SimpleDataHelper<Stage_Table> Data;

	private Action<Stage_Table> ClickEvent;

    public void Initialize(Action<Stage_Table> clickEvent)
	{
		ClickEvent = clickEvent;

		Data = new SimpleDataHelper<Stage_Table>(this);
		Init();
	}

	protected override UIBossWarListItemHolder CreateViewsHolder(int itemIndex)
	{
		var instance = new UIBossWarListItemHolder();

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

	public void UpdateScrollItem(uint selectStageId)
	{
		for(int i = 0; i < base.GetItemsCount(); i++)
		{
			if(GetItemViewsHolder(i) != null)
			{
				GetItemViewsHolder(i).Item.ActiveSelectImage(selectStageId);
			}
		}
	}

	protected override void UpdateViewsHolder(UIBossWarListItemHolder holder)
	{
		holder.Item.SetData(Data[holder.ItemIndex], ClickEvent);
	}
}

public class UIBossWarListItemHolder : BaseItemViewsHolder
{
	public UIBossWarListItem Item { get; private set; }

	public override void CollectViews()
	{
		Item = root.GetComponent<UIBossWarListItem>();
		base.CollectViews();
	}
}