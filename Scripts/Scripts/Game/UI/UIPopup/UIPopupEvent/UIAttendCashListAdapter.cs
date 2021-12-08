using GameDB;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIAttendCashListAdapter : ZScrollAdapterBase<EventReward_Table, UIAttendCashDataHolder>
{
	protected override string AddressableKey => nameof(UIAttendCashListItem);

	private Action<EventReward_Table> onClick;

    public void Initialize(Action<EventReward_Table> _onClick)
    {
        onClick = _onClick;
        Initialize();
    }

    protected override void OnCreateHolder(UIAttendCashDataHolder holder)
    {
        base.OnCreateHolder(holder);
        holder.SetAction(OnClickListItem);
    }

    private void OnClickListItem(EventReward_Table data)
    {
        onClick?.Invoke(data);
    }
}