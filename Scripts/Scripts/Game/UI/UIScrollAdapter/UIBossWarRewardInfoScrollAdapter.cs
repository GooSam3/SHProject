using Com.TheFallenGames.OSA.Core;
using Com.TheFallenGames.OSA.CustomParams;
using Com.TheFallenGames.OSA.DataHelpers;
using GameDB;
using System.Collections.Generic;

public class UIBossWarRewardInfoScrollAdapter : OSA<BaseParamsWithPrefab, UIBossWarRewardInfoItemHolder>
{
    public SimpleDataHelper<BossWar_Table> Data;

	public void Initialize()
	{
		Data = new SimpleDataHelper<BossWar_Table>(this);
		Init();
	}

	protected override UIBossWarRewardInfoItemHolder CreateViewsHolder(int itemIndex)
	{
		var instance = new UIBossWarRewardInfoItemHolder();

		instance.Init(_Params.ItemPrefab, _Params.Content, itemIndex);

		return instance;
	}

	public void Refresh(List<BossWar_Table> dataList)
	{
		if(dataList != null)
		{
			Data.ResetItems(dataList);
		}
	}

	protected override void UpdateViewsHolder(UIBossWarRewardInfoItemHolder holder)
	{
		holder.Item.SetData(Data[holder.ItemIndex]);
	}
}

public class UIBossWarRewardInfoItemHolder : BaseItemViewsHolder
{
	public UIBossWarRewardInfoListItem Item { get; private set; }

	public override void CollectViews()
	{
		Item = root.GetComponent<UIBossWarRewardInfoListItem>();
		base.CollectViews();
	}
}