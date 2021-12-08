using System.Collections.Generic;
using Com.TheFallenGames.OSA.Core;
using Com.TheFallenGames.OSA.DataHelpers;
using Com.TheFallenGames.OSA.CustomParams;
using System;
using UnityEngine;

public class SpecialShopArchiveListAdapter : OSA<BaseParamsWithPrefab, SpecialShopArchiveItemHolder>
{
    private SimpleDataHelper<SpecialShopArchiveListItemModel> Data;

    bool init;

    private Action<SpecialShopArchiveListItemModel> onClickedReceive;

    #region Public Methods
    public void Initialize()
    {
        if (init)
            return;

        init = true;

        var slot = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(SpecialShopArchiveItemSlot));
        Parameters.ItemPrefab = slot.GetComponent<RectTransform>();
        var pf = Parameters.ItemPrefab;
        pf.SetParent(transform);
        pf.localScale = Vector2.one;
        pf.localPosition = Vector3.zero;
        pf.gameObject.SetActive(false);

        Data = new SimpleDataHelper<SpecialShopArchiveListItemModel>(this);
        Init();
    }

    public uint CurDataShownCount
    {
        get
        {
            return (uint)Data.Count;
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Refresh(List<SpecialShopArchiveListItemModel> list)
    {
        Data.ResetItems(list);
    }

    public void SetListenerOnClickReceive(Action<SpecialShopArchiveListItemModel> callback)
    {
        this.onClickedReceive = callback;
    }

    public void RemoveListenerOnClickReceive(Action<SpecialShopArchiveListItemModel> callback)
    {
        this.onClickedReceive = callback;
    }

    #endregion

    #region Private Methods
    #endregion

    #region OSA Overrides
    protected override SpecialShopArchiveItemHolder CreateViewsHolder(int itemIndex)
    {
        var holder = new SpecialShopArchiveItemHolder();
        holder.Init(_Params.ItemPrefab, _Params.Content, itemIndex);
        holder.Slot.AddListener_OnClicked(() => onClickedReceive?.Invoke(Data[holder.ItemIndex]));
        return holder;
    }

    protected override void UpdateViewsHolder(SpecialShopArchiveItemHolder newOrRecycled)
    {
        var t = Data[newOrRecycled.ItemIndex];
        newOrRecycled.Slot.SetUI(t);
    }
    #endregion
}

public class SpecialShopArchiveItemHolder : BaseItemViewsHolder
{
    private SpecialShopArchiveItemSlot slot;
    public SpecialShopArchiveItemSlot Slot { get { return slot; } }

    public override void CollectViews()
    {
        slot = root.GetComponent<SpecialShopArchiveItemSlot>();
        base.CollectViews();
    }
}

public class SpecialShopArchiveListItemModel
{
    public ulong mailIdx;
    public uint specialShopTid;

    public SpecialShopArchiveListItemModel(ulong mailIdx, uint specialShopTid)
    {
        this.mailIdx = mailIdx;
        this.specialShopTid = specialShopTid;
    }
}