using GameDB;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZDefine;

//410 : 128

public class UIPetAdventureScrollAdapter : ZScrollAdapterBase<OSA_AdventureData, UIPetAdventureViewHolder>
{
    protected override string AddressableKey => nameof(UIPetAdventureListItem);

    private Action<OSA_AdventureData> onClick;

    public OSA_AdventureData selectedData { get; private set; } = null;

    public void Initialize(Action<OSA_AdventureData> _onClick)
    {
        onClick = _onClick;
        Initialize();
    }

    protected override void OnCreateHolder(UIPetAdventureViewHolder holder)
    {
        base.OnCreateHolder(holder);
        holder.SetAction(OnClickListItem);
    }

    public void SetSelectedData(OSA_AdventureData data)
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
            SetSelectedData(Data.List[0]);
    }

    private void OnClickListItem(OSA_AdventureData data)
    {
        SetSelectedData(data);
        onClick?.Invoke(data);
    }

    protected override void OnResetData(List<OSA_AdventureData> _listData)
    {
        SetSelectedData(null);
        base.OnResetData(_listData);
    }
}

public class OSA_AdventureData
{
    public PetAdventure_Table table;
    public PetAdvData advData;

    public bool isSelected;

    public OSA_AdventureData(PetAdvData _advData)
    {
        Reset(_advData);
        isSelected = false;
    }

    public OSA_AdventureData(OSA_AdventureData _advData)
	{
        Reset(_advData);
	}

    public void Reset(OSA_AdventureData data)
    {
        advData = data.advData;
        DBPetAdventure.Get(advData.AdvTid, out table);
        isSelected = data.isSelected;
    }

    public void Reset(PetAdvData _advData)
    {
        advData = _advData;
        DBPetAdventure.Get(advData.AdvTid, out table);
    }

}