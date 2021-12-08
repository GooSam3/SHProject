using Com.TheFallenGames.OSA.CustomAdapters.GridView;
using Com.TheFallenGames.OSA.DataHelpers;
using frame8.Logic.Misc.Other.Extensions;
using System.Collections.Generic;
using UnityEngine.UI;
using ZDefine;

public class UIItemRewardScrollAdapter : GridAdapter<GridParams, ItemRewardViewsHolder>
{
	public SimpleDataHelper<ItemRewardData> Data { get; private set; }

	protected override void UpdateCellViewsHolder(ItemRewardViewsHolder _holder)
	{
		if (_holder == null)
			return;

		ItemRewardData data = Data[_holder.ItemIndex];
		_holder.UpdateHolder(data);
	}

	public void RefreshData()
	{
		for (int i = 0; i < base.CellsCount; i++)
			UpdateCellViewsHolder(base.GetCellViewsHolder(i));
	}

	public void Initialize()
	{
		if (!IsInitialized)
			Init();
	}

	public void SetScrollData(GainInfo _item)
	{
		if (Data == null)
			Data = new SimpleDataHelper<ItemRewardData>(this);

		Data.List.Clear();

		if (!IsInitialized)
			Initialize();

		Data.InsertOneAtEnd(new ItemRewardData() { ItemTid = _item.ItemTid, ItemCount = _item.Cnt });

		Data.NotifyListChangedExternally();
		RefreshData();
	}

	public void SetScrollData(List<GainInfo> _itemList)
	{
		if (Data == null)
			Data = new SimpleDataHelper<ItemRewardData>(this);

		Data.List.Clear();

		if (!IsInitialized)
			Initialize();

		for (int i = 0; i < _itemList.Count; i++)
		{
			Data.InsertOneAtEnd(new ItemRewardData() { ItemTid = _itemList[i].ItemTid, ItemCount = _itemList[i].Cnt });
		}

		Data.NotifyListChangedExternally();
		RefreshData();
	}

	public void SetScrollData(List<GuildDungeonClearReward> _itemList)
	{
		if (Data == null)
			Data = new SimpleDataHelper<ItemRewardData>(this);

		Data.List.Clear();

		if (!IsInitialized)
			Initialize();

		for (int i = 0; i < _itemList.Count; i++)
		{
			Data.InsertOneAtEnd(new ItemRewardData() { ItemTid = _itemList[i].ItemTid, ItemCount = _itemList[i].Cnt });
		}

		Data.NotifyListChangedExternally();
		RefreshData();
	}
}

public class ItemRewardData
{
	public uint ItemTid;
	public ulong ItemCount;
}

public class ItemRewardViewsHolder : CellViewsHolder
{
	private Image ItemIcon;
	private Text ItemNum;

	public override void CollectViews()
	{
		base.CollectViews();

		views.GetComponentAtPath("ItemSlot_Share_Parts/Item_Icon", out ItemIcon);
		views.GetComponentAtPath("ItemSlot_Inven_Parts/Num/Txt_Num", out ItemNum);
	}

	public void UpdateHolder(ItemRewardData _data)
	{
		ItemIcon.sprite = UICommon.GetItemIconSprite(_data.ItemTid);
		ItemNum.text = _data.ItemCount.ToString();
	}
}
