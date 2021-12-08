using Com.TheFallenGames.OSA.Core;
using Com.TheFallenGames.OSA.CustomParams;
using Com.TheFallenGames.OSA.DataHelpers;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UIFrameGuildNetCapturer;

public class ScrollGuildJoinListAdapter : OSA<BaseParamsWithPrefab, ScrollGuildJoinListHolder>
{
    private SimpleDataHelper<GuildRequestListForGuildConverted> Data;
    bool init;

    private Action<GuildRequestListForGuildConverted> onClickedApprove;
    private Action<GuildRequestListForGuildConverted> onClickedReject;

    #region Public Methods
    public void Initialize()
    {
        if (init)
            return;

        init = true;

        Data = new SimpleDataHelper<GuildRequestListForGuildConverted>(this);
        Init();
    }

    public void RefreshRankingList()
    {
        Refresh(UIFrameGuildNetCapturer.MyGuildData.receivedJoinRequestList);
    }

    public void AddListener_OnClickedApprove(Action<GuildRequestListForGuildConverted> callback)
    {
        onClickedApprove += callback;
    }

    public void AddListener_OnClickedReject(Action<GuildRequestListForGuildConverted> callback)
    {
        onClickedReject += callback;
    }
    #endregion

    #region Private Methods
    private void Refresh(List<GuildRequestListForGuildConverted> info)
    {
        if (info != null)
        {
            RefreshData(info);
        }
    }

    private void RefreshData(List<GuildRequestListForGuildConverted> data)
    {
        Data.ResetItems(data);
    }
    #endregion

    #region OSA Overrides
    protected override ScrollGuildJoinListHolder CreateViewsHolder(int itemIndex)
    {
        var holder = new ScrollGuildJoinListHolder();
        holder.Init(_Params.ItemPrefab, _Params.Content, itemIndex);
        holder.Slot.AddListener_OnClickApprove(() => onClickedApprove?.Invoke(Data[holder.ItemIndex]));
        holder.Slot.AddListener_OnClickReject(() => onClickedReject?.Invoke(Data[holder.ItemIndex]));
        return holder;
    }

    protected override void UpdateViewsHolder(ScrollGuildJoinListHolder newOrRecycled)
    {
        var t = Data[newOrRecycled.ItemIndex];
        var characterData = DBCharacter.Get(t.char_tid);
        Sprite iconSprite = null;
        if (characterData != null)
        {
            iconSprite = ZManagerUIPreset.Instance.GetSprite(characterData.Icon);
        }

        newOrRecycled.Slot.SetData(
            iconSprite
            , t.char_lv
            , t.char_nick
            , t.comment);
    }
    #endregion
}
public class ScrollGuildJoinListHolder : BaseItemViewsHolder
{
    private ScrollGuildJoinListSlot slot;
    public ScrollGuildJoinListSlot Slot { get { return slot; } }

    public override void CollectViews()
    {
        slot = root.GetComponent<ScrollGuildJoinListSlot>();
        base.CollectViews();
    }
}