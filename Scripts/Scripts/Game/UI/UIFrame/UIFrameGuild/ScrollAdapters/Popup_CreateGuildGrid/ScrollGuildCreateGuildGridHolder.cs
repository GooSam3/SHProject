using Com.TheFallenGames.OSA.CustomAdapters.GridView;
using System;
using UnityEngine;

public class ScrollGuildCreateGuildGridHolder : CellViewsHolder
{
    private UIGuildCreateGuildSlot slot;
    public UIGuildCreateGuildSlot Slot { get { return slot; } }

    Action<ScrollGuildCreateGuildGridHolder> onClicked;
    public ScrollGuildCreateGuildMarkModel Data { get; private set; }

    public void UpdateSlot(ScrollGuildCreateGuildMarkModel data, uint curGuildLevel, Sprite sprite, bool selected, Action<ScrollGuildCreateGuildGridHolder> onClickedSlot)
    {
        var openLevel = DBGuild.GetGuildMarkOpenLevel(data.tid);
        bool isOpen = false;

        // 0 은 테이블서 못찾은거
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
        slot = root.GetComponent<UIGuildCreateGuildSlot>();
        slot.SetOnClickHandler(() => onClicked?.Invoke(this));
        base.CollectViews();
    }
}