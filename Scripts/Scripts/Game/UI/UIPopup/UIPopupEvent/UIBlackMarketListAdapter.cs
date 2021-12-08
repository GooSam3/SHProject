using GameDB;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBlackMarketListAdapter : ZGridScrollAdapter<SpecialShop_Table, UIBlackMarketViewHolder>
{
	public override string AddressableKey => nameof(UIBlackMarketListItem);

    private Action<SpecialShop_Table> onClick;

    public void SetAction(Action<SpecialShop_Table> _onClick)
    {
        onClick = _onClick;
    }

    protected override void OnUpdateViewHolder(UIBlackMarketViewHolder holder)
    {
        base.OnUpdateViewHolder(holder);
        holder.SetAction(onClick);
    }

    public void Initialize(Action<SpecialShop_Table> _onClick)
    {
        SetAction(_onClick);

        Initialize();
    }
}
