using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIEventViewListAdapter : ZScrollAdapterBase<OSA_UIEventData, UIEventViewHolder>
{
	protected override string AddressableKey => nameof(UIEventViewListItem);

	private Action<OSA_UIEventData> onClick;

	public OSA_UIEventData selectedData { get; private set; } = null;

	public void Initialize(Action<OSA_UIEventData> _onClick)
	{
		onClick = _onClick;
		Initialize();
	}

    protected override void OnCreateHolder(UIEventViewHolder holder)
    {
        base.OnCreateHolder(holder);
        holder.SetAction(OnClickListItem);
    }

    public void SetSelectedData(OSA_UIEventData data)
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

    public void SelectFirst()
    {
        if (Data.Count > 0)
        {
            OnClickListItem(Data.List[0]);
        }
    }

    private void OnClickListItem(OSA_UIEventData data)
    {
        SetSelectedData(data);
        onClick?.Invoke(data);
    }

    protected override void OnResetData(List<OSA_UIEventData> _listData)
    {
        SetSelectedData(null);
        base.OnResetData(_listData);
    }
}

public class OSA_UIEventData
{
    public bool isSelected;

    public IngameEventInfoConvert eventData;

    public OSA_UIEventData(IngameEventInfoConvert data)
	{
        eventData = data;
        isSelected = false;
	}
}
