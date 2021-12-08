using GameDB;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBattlePassListAdapter : ZScrollAdapterBase<QuestEvent_Table, UIBattlePassDataHolder>
{
	protected override string AddressableKey => nameof(UIBattlePassListItem);

    private Action<QuestEvent_Table> onClick;

    public void Initialize(Action<QuestEvent_Table> _onClick)
    {
        onClick = _onClick;
        Initialize();
    }

    protected override void OnCreateHolder(UIBattlePassDataHolder holder)
    {
        base.OnCreateHolder(holder);
        holder.SetAction(OnClickListItem);
    }

    private void OnClickListItem(QuestEvent_Table data)
    {
        onClick?.Invoke(data);
    }
}