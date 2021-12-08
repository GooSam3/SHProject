using Com.TheFallenGames.OSA.CustomAdapters.GridView;
using System;
using UnityEngine;

public class MileageItemScrollAdapterHolder : CellViewsHolder
{
    private UIMileageShopItemGroup slot;
    public UIMileageShopItemGroup Slot { get { return slot; } }

    Action<MileageItemScrollAdapterHolder> onClicked;

    public void UpdateSlot(MileageBaseDataIdentifier data, Action<MileageItemScrollAdapterHolder> onClickedSlot)
    {
        // TODO 
        slot.SetUI(data);
        onClicked = onClickedSlot;
    }

    public override void CollectViews()
    {
        slot = root.GetComponent<UIMileageShopItemGroup>();
        slot.SetOnClickHandler(() => onClicked?.Invoke(this));
        base.CollectViews();
    }
}