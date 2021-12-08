using Com.TheFallenGames.OSA.Core;
using Com.TheFallenGames.OSA.CustomParams;
using Com.TheFallenGames.OSA.DataHelpers;
using GameDB;
using System;
using System.Collections.Generic;

public class UIBossWarPortalListScrollAdapter : OSA<BaseParamsWithPrefab, UIBossWarPortalListHolder>
{
	private SimpleDataHelper<Portal_Table> Data;
	private Action<Portal_Table> ClickEvent;

	public void  Initialize(Action<Portal_Table> action)
	{
		ClickEvent = action;
		Data = new SimpleDataHelper<Portal_Table>(this);
		Init();
	}

	public void Refresh(List<Portal_Table> dataList)
	{
		if(dataList != null)
		{
			Data.ResetItems(dataList);
		}
	}

	protected override UIBossWarPortalListHolder CreateViewsHolder(int itemIndex)
	{
		var holder = new UIBossWarPortalListHolder();
		holder.Init(_Params.ItemPrefab, _Params.Content, itemIndex);
		return holder;
	}

	public void UpdateScrollItem()
	{
		for(int i = 0; i < base.GetItemsCount(); i++)
		{
			if(GetItemViewsHolder(i) != null)
			{
				GetItemViewsHolder(i).Item.ActiveSelectImage();
			}
		}
	}

	protected override void UpdateViewsHolder(UIBossWarPortalListHolder holder)
	{
		holder.Item.SetData(Data[holder.ItemIndex], ClickEvent);
	}
}

public class UIBossWarPortalListHolder : BaseItemViewsHolder
{
	public UIBossWarPortalList Item { get; private set; }

	public override void CollectViews()
	{
		Item = root.GetComponent<UIBossWarPortalList>();
		base.CollectViews();
	}
}