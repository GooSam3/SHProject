using Com.TheFallenGames.OSA.CustomAdapters.GridView;
using Com.TheFallenGames.OSA.DataHelpers;
using GameDB;
using System;
using System.Collections.Generic;
using UnityEngine;

public class UIItemShopScrollAdapter : GridAdapter<GridParams, UIScrollShopItemSlot>
{
    public SimpleDataHelper<ScrollShopItemData> Data { get; private set; }

    private Action<ScrollShopItemData> onClickSlot;

    private bool isAutoBuy = false;

    // 수동 init
    protected override void Start() { }

    protected override void UpdateCellViewsHolder(UIScrollShopItemSlot _holder)
    {
        if (_holder == null)
            return;

        ScrollShopItemData data = Data[_holder.ItemIndex];
        _holder.UpdateItemSlot(data, onClickSlot, isAutoBuy);
    }

    public void RefreshData()
    {
        for (int i = 0; i < base.CellsCount; i++)
            UpdateCellViewsHolder(base.GetCellViewsHolder(i));
    }

    public void Initialize(Action<ScrollShopItemData> _onClickSlot, bool _isAutoBuy)
    {
        if (Data == null)
            Data = new SimpleDataHelper<ScrollShopItemData>(this);

        isAutoBuy = _isAutoBuy;

        GameObject itemslot = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(UIShopItemSlot));
        Parameters.Grid.CellPrefab = itemslot.GetComponent<RectTransform>();
        Parameters.Grid.CellPrefab.SetParent(GetComponent<Transform>());
        Parameters.Grid.CellPrefab.transform.localScale = Vector2.one;
        Parameters.Grid.CellPrefab.transform.localPosition = new Vector3(0, 0, 0);
        Parameters.Grid.CellPrefab.gameObject.SetActive(false);

        onClickSlot = _onClickSlot;

        Init();
    }

    public void ResetData(List<ScrollShopItemData> dataList)
    {
        try
        {
            Data.ResetItems(dataList);

        }
        catch
        {
            int i = 0;
        }
    }
}

public class ScrollShopItemData : ScrollItemData
{
    public uint shopItemTid;
    public NormalShop_Table tableData;

    public ScrollShopItemData() : base() { }

    public ScrollShopItemData(uint tid, uint count) : base(tid, count) { }

    public void Reset(ScrollShopItemData data)
    {
        shopItemTid = data.shopItemTid;
        tableData = data.tableData;

        base.Reset(data);
    }
}