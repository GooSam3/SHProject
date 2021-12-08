using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZDefine;

public class UIDispatchScrollAdapter : ZScrollAdapterBase<OSA_DispatchData, UIDispatchDataHolder>
{
    protected override string AddressableKey => nameof(UIDispatchListItem);

    private Action<OSA_DispatchData> onClick;
    private Action<ChangeQuestData> onClickReward;

    public OSA_DispatchData selectedData { get; private set; } = null;

    protected override void OnCreateHolder(UIDispatchDataHolder holder)
    {
        base.OnCreateHolder(holder);
        holder.SetAction(onClick, onClickReward);
    }

    public void Initialize(Action<OSA_DispatchData> _onClick, Action<ChangeQuestData> _onClickReward)
    {
        onClick = _onClick;
        onClickReward = _onClickReward;
        Initialize();
    }

    public void RefreshRemainTime()
    {
        for (int i = 0; i < VisibleItemsCount; i++)
        {
            var holder = base.GetItemViewsHolder(i);
            holder.RefreshRemainTime();
        }
    }

    public void SetSelectedData(OSA_DispatchData data)
    {
        if (selectedData != null)
            selectedData.isSelected = false;

        selectedData = data;

        if (selectedData != null)
        {
            selectedData.isSelected = true;
        }

        RefreshData();
    }

    protected override void OnResetData(List<OSA_DispatchData> _listData)
    {
        SetSelectedData(null);
        base.OnResetData(_listData);
    }
}

public class OSA_DispatchData
{
    public ChangeQuestData data;
    public bool isSelected;

    public OSA_DispatchData(ChangeQuestData _data)
    {
        data = _data;
        isSelected = false;
    }
}