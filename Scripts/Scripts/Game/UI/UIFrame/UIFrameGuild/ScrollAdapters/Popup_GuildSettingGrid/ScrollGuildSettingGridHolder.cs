using Com.TheFallenGames.OSA.CustomAdapters.GridView;
using System;
using UnityEngine;

public class ScrollGuildSettingGridHolder : CellViewsHolder
{
    private ScrollGuildSettingGridSlot slot;
    public ScrollGuildSettingGridSlot Slot { get { return slot; } }

    Action<ScrollGuildSettingGridHolder> onClicked;
    public ScrollGuildSettingGuildMarkModel Data { get; private set; }

    public void UpdateSlot(ScrollGuildSettingGuildMarkModel data, uint curGuildLevel, Sprite sprite, bool selected, Action<ScrollGuildSettingGridHolder> onClickedSlot)
    {
        var openLevel = DBGuild.GetGuildMarkOpenLevel(data.tid);
        bool isOpen = false;

        if (openLevel == 0)
        {
            isOpen = false;
        }
        else if (curGuildLevel >= openLevel)
        {
            isOpen = true;
        }

        Data = data;
        slot.UpdateUI(sprite, isOpen == false, openLevel);
        slot.SetSelect(isOpen == false, selected, data.hasBeenSelected);
        onClicked = onClickedSlot;
    }

    public override void CollectViews()
    {
        slot = root.GetComponent<ScrollGuildSettingGridSlot>();
        slot.SetOnClickHandler(() => onClicked?.Invoke(this));
        base.CollectViews();
    }
}