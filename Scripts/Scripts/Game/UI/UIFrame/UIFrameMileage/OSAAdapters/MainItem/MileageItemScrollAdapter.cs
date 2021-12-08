using Com.TheFallenGames.OSA.CustomAdapters.GridView;
using Com.TheFallenGames.OSA.DataHelpers;
using System;
using System.Collections.Generic;
using ZNet.Data;
using UnityEngine.UI;
using UnityEngine;

public class MileageItemScrollAdapter : GridAdapter<GridParams, MileageItemScrollAdapterHolder>
{
    private SimpleDataHelper<MileageBaseDataIdentifier> Data;

    public Action<MileageBaseDataIdentifier> onClickedSlot;

    #region Public Methods
    protected override void Start()
    {
        //    base.Start();
    }

    public void Initialize()
    {
        Data = new SimpleDataHelper<MileageBaseDataIdentifier>(this);
        var slot = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(UIMileageShopItemGroup));
        Parameters.Grid.CellPrefab = slot.GetComponent<RectTransform>();
        var pf = Parameters.Grid.CellPrefab;
        pf.SetParent(transform);
        pf.localScale = Vector2.one;
        pf.localPosition = Vector3.zero;
        pf.gameObject.SetActive(false);

        Init();
    }

    public void RefreshData(List<MileageBaseDataIdentifier> list)
    {
        Data.ResetItems(list);
    }

    public void OnSlotClicked(MileageItemScrollAdapterHolder holder)
    {
        onClickedSlot?.Invoke(Data[holder.ItemIndex]);
    }

    //public void SetSelectedMarkID(byte tid, bool notifyDataUpdate)
    //{
    //    selectedMarkTID = tid;

    //    if (notifyDataUpdate)
    //        Data.NotifyListChangedExternally();
    //}
    #endregion

    #region Private Methods
    //private void OnClickedSlot(ScrollGuildCreateGuildGridHolder holder)
    //{
    //    Data[holder.ItemIndex].hasBeenSelected = true;
    //    onClickedSlot?.Invoke(holder);
    //}
    #endregion


    #region OSA Overrides
    protected override void UpdateCellViewsHolder(MileageItemScrollAdapterHolder viewsHolder)
    {
        var t = Data[viewsHolder.ItemIndex];

        viewsHolder.UpdateSlot(t, OnSlotClicked);
    }
    #endregion
}