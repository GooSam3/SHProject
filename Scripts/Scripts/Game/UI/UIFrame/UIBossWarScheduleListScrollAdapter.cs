using Com.TheFallenGames.OSA.Core;
using Com.TheFallenGames.OSA.CustomParams;
using Com.TheFallenGames.OSA.DataHelpers;
using System.Collections.Generic;

public class UIBossWarScheduleListScrollAdapter : OSA<BaseParamsWithPrefab, UIBossWarScheduleListItemHolder>
{
	public SimpleDataHelper<ulong> Data;
	private int CurrentItemIndex = 0;

	public void Initialize()
	{
		Data = new SimpleDataHelper<ulong>(this);
		Init();
	}

	protected override UIBossWarScheduleListItemHolder CreateViewsHolder(int itemIndex)
	{
		var instance = new UIBossWarScheduleListItemHolder();

		instance.Init(_Params.ItemPrefab, _Params.Content, itemIndex);

		return instance;
	}

	public void Refresh(List<ulong> dataList)
	{
		for(int i = 0; i < dataList.Count; i++)
		{
			ulong curSecTime = (TimeManager.NowSec + TimeHelper.SecondOffset) % TimeHelper.DaySecond;

			if(dataList[i] >= curSecTime)
			{
				CurrentItemIndex = i;
				break;
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

	protected override void UpdateViewsHolder(UIBossWarScheduleListItemHolder holder)
	{
		holder.Item.SetData(Data[holder.ItemIndex]);
	}
}

public class UIBossWarScheduleListItemHolder : BaseItemViewsHolder
{
	public UIBossWarScheduleListItem Item { get; private set; }

	public override void CollectViews()
	{
		Item = root.GetComponent<UIBossWarScheduleListItem>();
		base.CollectViews();
	}
}