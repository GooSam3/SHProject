using Com.TheFallenGames.OSA.Core;
using Com.TheFallenGames.OSA.CustomParams;
using Com.TheFallenGames.OSA.DataHelpers;
using System.Collections.Generic;

public class UIBossWarPointRankingScrollAdapter : OSA<BaseParamsWithPrefab, UIBossWarPointRankingListItemHolder>
{
	public SimpleDataHelper<BossWarPointRanking> Data;

	public void Initialize()
	{
		Data = new SimpleDataHelper<BossWarPointRanking>(this);
		Init();
	}

	protected override UIBossWarPointRankingListItemHolder CreateViewsHolder(int itemIndex)
	{
		var instance = new UIBossWarPointRankingListItemHolder();

		instance.Init(_Params.ItemPrefab, _Params.Content, itemIndex);

		return instance;
	}

	public void Refresh(List<BossWarPointRanking> dataList)
	{
		if(dataList != null)
		{
			Data.ResetItems(dataList);
		}
	}

	public void RefreshData()
	{
		for (int i = 0; i < base.GetItemsCount(); i++)
		{
			if (GetItemViewsHolder(i) != null)
			{
				GetItemViewsHolder(i).Item.ResetByExitBossWar();
			}
		}
	}

	protected override void UpdateViewsHolder(UIBossWarPointRankingListItemHolder holder)
	{
		holder.Item.SetData(Data[holder.ItemIndex]);
	}
}

public class UIBossWarPointRankingListItemHolder : BaseItemViewsHolder
{
    public UIBossWarRankingListItem Item { get; private set; }

	public override void CollectViews()
	{
		Item = root.GetComponent<UIBossWarRankingListItem>();
		base.CollectViews();
	}
}