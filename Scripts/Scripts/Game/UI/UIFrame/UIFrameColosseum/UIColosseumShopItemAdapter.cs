using Com.TheFallenGames.OSA.DataHelpers;
using System.Collections.Generic;
using Com.TheFallenGames.OSA.CustomAdapters.GridView;
using System;
using UnityEngine;
using static SpecialShopCategoryDescriptor;

public class UIColosseumShopItemAdapter : GridAdapter<GridParams, UIColosseumShopItemHolder>
{
	[SerializeField] private SpecialShopItemSlot shopItemPf;

	private SimpleDataHelper<SingleDataInfo> data;
	private Action<SingleDataInfo> clickItem;

	public void Initialize(Action<SingleDataInfo> _clickItem)
	{
		clickItem = _clickItem;

		data = new SimpleDataHelper<SingleDataInfo>(this);

		Parameters.Grid.CellPrefab = shopItemPf.GetComponent<RectTransform>();
		var pf = Parameters.Grid.CellPrefab;
		pf.SetParent(transform);
		pf.localScale = Vector2.one;
		pf.localPosition = Vector3.zero;
		pf.gameObject.SetActive(false);

		Init();
	}

	public void RefreshShopItemList(List<SingleDataInfo> dataList)
	{
		if (dataList != null) {
			data.ResetItems(dataList);
		}
	}

	protected override void UpdateCellViewsHolder(UIColosseumShopItemHolder holder)
	{
		holder.UpdateSlot(data[holder.ItemIndex], OnSlotClicked);
	}

	public void OnSlotClicked(UIColosseumShopItemHolder holder)
	{
		clickItem?.Invoke(data[holder.ItemIndex]);
	}
}

public class UIColosseumShopItemHolder : CellViewsHolder
{
	private SpecialShopItemSlot item;
	private Action<UIColosseumShopItemHolder> clickAction;

	public void UpdateSlot(SingleDataInfo data, Action<UIColosseumShopItemHolder> _clickAction)
	{
		item.SetUI(data);
		clickAction = _clickAction;
	}

	public override void CollectViews()
	{
		item = root.GetComponent<SpecialShopItemSlot>();
		item.SetOnClickHandler(() => clickAction?.Invoke(this));
		base.CollectViews();
	}
}

