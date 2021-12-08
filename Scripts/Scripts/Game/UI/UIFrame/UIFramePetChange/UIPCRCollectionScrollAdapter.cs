using System;
using System.Collections.Generic;

public class UIPCRCollectionScrollAdapter : ZFittedScrollerAdapterBase<ScrollPCRCollectionData, PCRCollectionHolder>
{
    protected override string AddressableKey => nameof(UIPCRCollectionListItem);

    private Action<ScrollPCRCollectionData> onClickSlotToggle;// open toggle -> refresh
    private Action<ScrollPCRCollectionData, C_PetChangeData> onClickSlotPCR; // click pcr -> model

    public ScrollPCRCollectionData SelectedData { get; private set; } = null;

    protected override void OnCreateHolder(PCRCollectionHolder holder)
    {
        base.OnCreateHolder(holder);
        holder.SetEvent(onClickSlotToggle, onClickSlotPCR);
    }

    public void SetEvent(Action<ScrollPCRCollectionData> _onClickSlotToggle, Action<ScrollPCRCollectionData, C_PetChangeData> _onClickSlotPCR)
    {
        onClickSlotToggle = _onClickSlotToggle;
        onClickSlotPCR = _onClickSlotPCR;
    }

    public void SetSelectedPCR(ScrollPCRCollectionData data, C_PetChangeData pcrData)
    {
        SelectedData?.SetDefault(false);

        SelectedData = data;

        if (SelectedData != null)
        {
            SelectedData.IsSelected = true;
            SelectedData.SelectedTid = pcrData.Tid;
        }
        RefreshData();
    }

    public void SmoothScrollTo(ScrollPCRCollectionData data)
    {
        var index = listData.FindIndex(item => item.Tid == data.Tid);

        if (index < 0) return;

        if (index <0 && data.IsOpened == false) return;

        if (index == 0)
            base.SetNormalizedPosition(1);
        //else
        //    base.SmoothScrollTo(index, .2f);
    }

    public override ScrollPCRCollectionData CreateModel(int itemIndex)
    {
        return listData[itemIndex];
    }

    protected override void OnResetData(List<ScrollPCRCollectionData> _listData)
    {
        foreach (var iter in listData)
        {
            iter.SetDefault();
        }

        base.OnResetData(_listData);
    }

}

public class ScrollPCRCollectionData : ZFittedAdapterData
{
    public E_PetChangeViewType ViewType;
    public int DataIndex = 0;
    public uint SortOrder = 0;
    public uint Tid; // collection
    public bool IsOpened = false;
    public bool IsSelected = false;
    public uint SelectedTid = 0;// pcr

    public ScrollPCRCollectionData(E_PetChangeViewType viewType, uint tid, uint sortOrder)
    {
        ViewType = viewType;
        Tid = tid;
        IsOpened = false;
        IsSelected = false;
        SelectedTid = 0;
        SortOrder = sortOrder;

        Reset();
    }

    public void Reset(ScrollPCRCollectionData data)
    {
        ViewType = data.ViewType;
        Tid = data.Tid;
        IsOpened = data.IsOpened;
        IsSelected = data.IsSelected;
        SelectedTid = data.SelectedTid;
        SortOrder = data.SortOrder;
        Reset();
    }

    public void SetDefault(bool refreshOpen = true)
    {
        if (refreshOpen == true)
            IsOpened = false;
        IsSelected = false;
        SelectedTid = 0;
    }
}