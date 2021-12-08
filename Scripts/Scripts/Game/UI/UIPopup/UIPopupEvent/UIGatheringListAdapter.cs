using GameDB;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGatheringListAdapter : ZScrollAdapterBase<OSA_EventGatheringData, UIGatheringDataHolder>
{
	protected override string AddressableKey => nameof(UIGatheringListItem);

	private Action<OSA_EventGatheringData> onConfirm;


    public void Initialize(Action<OSA_EventGatheringData> _onClick)
    {
        onConfirm = _onClick;
        Initialize();
    }

    protected override void OnCreateHolder(UIGatheringDataHolder holder)
    {
        base.OnCreateHolder(holder);
        holder.SetAction(OnClickListItem);
    }

    private void OnClickListItem(OSA_EventGatheringData data)
    {
        onConfirm?.Invoke(data);
    }
}

public class OSA_EventGatheringData
{
    public SpecialShop_Table table;
    public int selectValue;// 선택한 수

    public OSA_EventGatheringData(SpecialShop_Table _table)
	{
        table = _table;
        selectValue = 0;
	}
}