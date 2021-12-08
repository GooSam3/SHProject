using Com.TheFallenGames.OSA.Core;
using Com.TheFallenGames.OSA.CustomParams;
using Com.TheFallenGames.OSA.DataHelpers;
using GameDB;
using System.Collections.Generic;

public class UIInfinityTowerDailyRewardScrollAdapter : OSA<BaseParamsWithPrefab, UIInfinityTowerDailyRewardListHolder>
{
    private SimpleDataHelper<InfinityDungeon_Table> Data;
	private int CurrentItemIndex = 0;

    public void Initialize()
	{
		Data = new SimpleDataHelper<InfinityDungeon_Table>(this);
		Init();
	}

	protected override UIInfinityTowerDailyRewardListHolder CreateViewsHolder(int itemIndex)
	{
		var holder = new UIInfinityTowerDailyRewardListHolder();

		holder.Init(_Params.ItemPrefab, _Params.Content, itemIndex);

		return holder;
	}

	protected override void UpdateViewsHolder(UIInfinityTowerDailyRewardListHolder holder)
	{
		holder.Item.SetData(Data[holder.ItemIndex]);
	}

	public void Refresh(List<InfinityDungeon_Table> dataList)
	{
		bool isRewarded = TimeHelper.IsGivenDtToday(ZNet.Data.Me.CurUserData.InfinityDungeonRewardTime, 5);

		for (int i = 0; i < dataList.Count; i++)
		{
			if (isRewarded)
			{
				if(dataList[i].DungeonID == ZNet.Data.Me.CurUserData.LastRewardedStageTid)
				{
					CurrentItemIndex = i;
					break;
				}
			}
			else
			{
				if (dataList[i].DungeonID == ZNet.Data.Me.CurUserData.CurrentInfinityDungeonId)
				{
					CurrentItemIndex = i;
					break;
				}
			}
		}

		if (dataList != null)
		{
			Data.ResetItems(dataList);
		}
	}

	public void SetPosition()
	{
		ScrollTo(CurrentItemIndex);
	}

	public void UpdateScrollItem()
	{
		for (int i = 0; i < base.GetItemsCount(); i++)
		{
			if (GetItemViewsHolder(i) != null)
			{
				GetItemViewsHolder(i).Item.DailyRewardedCheck();
			}
		}
	}
}

public class UIInfinityTowerDailyRewardListHolder : BaseItemViewsHolder
{
	public UIInfinityRewardItemList Item { get; private set; }

	public override void CollectViews()
	{
		Item = root.GetComponent<UIInfinityRewardItemList>();
		base.CollectViews();
	}
}