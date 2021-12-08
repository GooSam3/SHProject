using System;
using System.Collections.Generic;
using ZDefine;
using ZNet.Data;

// Pet
// Change
// Ride
// => PCR

// 가챠 교체창 스크롤 어댑터
public class UIPCRReplaceScrollAdapter : ZScrollAdapterBase<GachaKeepData, UIPetChangeReplaceViewHolder>
{
    private Action<GachaKeepData> onReplace;
    private Action<GachaKeepData> onConfirm;
    private Action<GachaKeepData> onDetail;

    protected override string AddressableKey => nameof(UIPetChangeReplaceListItem);

    protected override void OnCreateHolder(UIPetChangeReplaceViewHolder holder)
    {
        base.OnCreateHolder(holder);

        holder.SetEvent(onReplace, onConfirm, onDetail);
    }

    public void SetEvent(Action<GachaKeepData> _onReplace, Action<GachaKeepData> _onConfirm, Action<GachaKeepData> _onDetail)
    {
        onReplace = _onReplace;
        onConfirm = _onConfirm;
        onDetail = _onDetail;
    }

    public void ResetDataList(E_PetChangeViewType viewType)
    {
        List<GachaKeepData> listKeepData = new List<GachaKeepData>();

        switch (viewType)
        {
            case E_PetChangeViewType.Change:
                listKeepData.AddRange(Me.CurCharData.GetChangeKeepDataList());
                break;
            case E_PetChangeViewType.Pet:
                listKeepData.AddRange(Me.CurCharData.GetPetKeepList());
                break;
            case E_PetChangeViewType.Ride:
                listKeepData.AddRange(Me.CurCharData.GetRideKeepList());
                break;
        }
        Data.ResetItems(listKeepData);

        SetNormalizedPosition(0);
    }

    public void RefreshRemainTime()
    {
        if (IsInitialized == false)
            return;

        if (VisibleItemsCount <= 0)
            return;

        for (int i = 0; i < VisibleItemsCount; i++)
        {
            base.GetItemViewsHolder(i).RefreshRemainTime();
        }
    }
}