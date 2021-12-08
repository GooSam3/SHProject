using Com.TheFallenGames.OSA.Core;
using Com.TheFallenGames.OSA.CustomParams;
using Com.TheFallenGames.OSA.DataHelpers;
using GameDB;
using System.Collections.Generic;

public class UIInfinityFloorScrollAdapter : OSA<BaseParamsWithPrefab, UIInfinityFloorPageHolder>
{
    public SimpleDataHelper<InfinityDungeon_Table> Data;
    private int CurrentItemIndex = 0;

    public void Initialize()
	{
        Data = new SimpleDataHelper<InfinityDungeon_Table>(this);
        Init();
	}

    protected override UIInfinityFloorPageHolder CreateViewsHolder(int itemIndex)
    {
        var instance = new UIInfinityFloorPageHolder();

        instance.Init(_Params.ItemPrefab, _Params.Content, itemIndex);

        return instance;
    }

    protected override void UpdateViewsHolder(UIInfinityFloorPageHolder holder)
    {
        holder.Item.SetData(Data[holder.ItemIndex]);
    }

    public void Refresh(List<InfinityDungeon_Table> dataList)
    {
        List<InfinityDungeon_Table> tempList = new List<InfinityDungeon_Table>();
        tempList.Add(null);
        tempList.AddRange(dataList);

        if(dataList != null)
		{
            for(int i = 0; i < dataList.Count; i++)
			{
                if(dataList[i].DungeonID == ZNet.Data.Me.CurUserData.CurrentInfinityDungeonId)
				{
                    CurrentItemIndex = i;
                    break;
				}
			}

            Data.ResetItems(tempList);
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
                GetItemViewsHolder(i).Item.SetFloorUI();
            }
        }
    }
}

public class UIInfinityFloorPageHolder : BaseItemViewsHolder
{
    public UIInfinityTowerListItem Item { get; private set; }

	public override void CollectViews()
	{
        Item = root.GetComponent<UIInfinityTowerListItem>();
		base.CollectViews();
	}
}