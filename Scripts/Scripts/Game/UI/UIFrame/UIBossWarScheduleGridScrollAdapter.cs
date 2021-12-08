using Com.TheFallenGames.OSA.DataHelpers;
using System.Collections.Generic;
using Com.TheFallenGames.OSA.CustomAdapters.GridView;

public class UIBossWarScheduleGridScrollAdapter : GridAdapter<GridParams, UIBossWarScheduleGridItemHolder>
{
    private SimpleDataHelper<UIBossWarScheduleGridItemData> Data;
	private int CurrentIndex = 0;

	public void Initialize()
	{
		Data = new SimpleDataHelper<UIBossWarScheduleGridItemData>(this);

		Init();
	}

	public void Refresh(List<ulong> dataList)
	{
		List<UIBossWarScheduleGridItemData> list = new List<UIBossWarScheduleGridItemData>();
		ulong currentSpawnTime = 0;
		
		for (int i = 0; i < dataList.Count; i++)
		{
			ulong curSecTime = (TimeManager.NowSec + TimeHelper.SecondOffset) % TimeHelper.DaySecond;

			if(currentSpawnTime == 0 && dataList[i] >= curSecTime)
			{
				currentSpawnTime = dataList[i];
				CurrentIndex = i;
			}

			list.Add(new UIBossWarScheduleGridItemData(dataList[i], dataList[i] < curSecTime, currentSpawnTime == dataList[i]));
		}
		
		if (dataList != null)
		{
			Data.ResetItems(list);
		}
	}

	public void SetPosition()
	{
		ScrollTo(CurrentIndex);
	}

	protected override void UpdateCellViewsHolder(UIBossWarScheduleGridItemHolder holder)
	{
		holder.Item.SetData(Data[holder.ItemIndex]);
	}
}

public class UIBossWarScheduleGridItemHolder : CellViewsHolder
{
	public UIBossWarScheduleGridItem Item { get; private set; }

	public override void CollectViews()
	{
		Item = root.GetComponent<UIBossWarScheduleGridItem>();
		base.CollectViews();
	}
}

public class UIBossWarScheduleGridItemData
{
	public ulong SpawnTime;
	public bool IsExpire;
	public bool IsCurrentSpawnTime;

	public UIBossWarScheduleGridItemData(ulong spawnTime, bool isExpire, bool isCurrentSpawnTime)
	{
		SpawnTime = spawnTime;
		IsExpire = isExpire;
		IsCurrentSpawnTime = isCurrentSpawnTime;
	}
}