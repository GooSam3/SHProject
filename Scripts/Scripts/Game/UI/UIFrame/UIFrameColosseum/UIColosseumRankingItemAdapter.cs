using Com.TheFallenGames.OSA.Core;
using Com.TheFallenGames.OSA.CustomParams;
using Com.TheFallenGames.OSA.DataHelpers;
using System.Collections.Generic;
using ZNet.Data;

public class UIColosseumRankingItemAdapter : OSA<BaseParamsWithPrefab, UIColosseumRankingItemHolder>
{
	private SimpleDataHelper<ColosseumRankInfoConverted> data;

	public void Initialize()
	{
		data = new SimpleDataHelper<ColosseumRankInfoConverted>(this);
		Init();
	}

	public void Refresh(List<ColosseumRankInfoConverted> dataList)
	{
		if (dataList != null) {
			data.ResetItems(dataList);
		}
	}

	protected override UIColosseumRankingItemHolder CreateViewsHolder(int itemIndex)
	{
		var holder = new UIColosseumRankingItemHolder();
		holder.Init(_Params.ItemPrefab, _Params.Content, itemIndex);
		return holder;
	}

	protected override void UpdateViewsHolder(UIColosseumRankingItemHolder holder)
	{
		holder.Item.SetData(data[holder.ItemIndex]);
	}
}

public class UIColosseumRankingItemHolder : BaseItemViewsHolder
{
	public UIColosseumRankingItem Item { get; private set; }

	public override void CollectViews()
	{
		Item = root.GetComponent<UIColosseumRankingItem>();
		base.CollectViews();
	}
}

