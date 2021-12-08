using Com.TheFallenGames.OSA.Core;
using Com.TheFallenGames.OSA.CustomParams;
using Com.TheFallenGames.OSA.DataHelpers;
using System;
using System.Collections.Generic;

public class UIGodLandLocalViewerItemAdapter : OSA<BaseParamsWithPrefab, UIGodLandLocalViewerItemHolder>
{
	private SimpleDataHelper<GodLandSpotInfoConverted> data;
	private Action<uint> clickItem;

	public void Initialize(Action<uint> _clickItem)
	{
		clickItem = _clickItem;

		data = new SimpleDataHelper<GodLandSpotInfoConverted>(this);
		Init();
	}

	public void Refresh(List<GodLandSpotInfoConverted> dataList)
	{
		if (dataList != null) {
			data.ResetItems(dataList);
		}
	}

	protected override UIGodLandLocalViewerItemHolder CreateViewsHolder(int itemIndex)
	{
		var holder = new UIGodLandLocalViewerItemHolder();
		holder.Init(_Params.ItemPrefab, _Params.Content, itemIndex);
		return holder;
	}

	protected override void UpdateViewsHolder(UIGodLandLocalViewerItemHolder holder)
	{
		holder.Item.SetData(data[holder.ItemIndex], clickItem);
	}
}

public class UIGodLandLocalViewerItemHolder : BaseItemViewsHolder
{
	public UIGodLandLocalViewerItem Item { get; private set; }

	public override void CollectViews()
	{
		Item = root.GetComponent<UIGodLandLocalViewerItem>();
		base.CollectViews();
	}
}

