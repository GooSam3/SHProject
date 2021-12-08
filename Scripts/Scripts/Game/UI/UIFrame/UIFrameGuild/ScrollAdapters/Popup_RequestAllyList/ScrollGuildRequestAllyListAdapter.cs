using Com.TheFallenGames.OSA.Core;
using Com.TheFallenGames.OSA.CustomParams;
using Com.TheFallenGames.OSA.DataHelpers;
using System;
using System.Collections.Generic;
using WebNet;
using static UIFrameGuildNetCapturer;

public class ScrollGuildRequestAllyListAdapter : OSA<BaseParamsWithPrefab, ScrollGuildRequestAllyListHolder>
{
    private SimpleDataHelper<GuildAllianceInfoConverted> Data;

    private Action<GuildAllianceInfoConverted> onClickedReject;
    private Action<GuildAllianceInfoConverted> onClickedAccept;

    bool init;

    #region Public Methods
    public void Initialize()
    {
        if (init)
            return;

        init = true;

        Data = new SimpleDataHelper<GuildAllianceInfoConverted>(this);
        Init();
    }

    public void OnHide()
    {
        gameObject.SetActive(false);
    }

    public void AddListener_OnClickedReject(Action<GuildAllianceInfoConverted> callback)
    {
        onClickedReject += callback;
    }

    public void AddListener_OnClickedAccept(Action<GuildAllianceInfoConverted> callback)
    {
        onClickedAccept += callback;
    }
    public void RefreshList()
    {
        if (UIFrameGuildNetCapturer.MyGuildData.myGuildAllianceInfo.ContainsKey(E_GuildAllianceState.ReceiveAlliance) == false)
        {
            RefreshData(new List<GuildAllianceInfoConverted>());
        }
        else
        {
            RefreshData(UIFrameGuildNetCapturer.MyGuildData.myGuildAllianceInfo[E_GuildAllianceState.ReceiveAlliance]);
        }
    }
    #endregion

    #region Private Methods
    private void RefreshData(List<GuildAllianceInfoConverted> info)
    {
        Data.ResetItems(info);
    }

    #endregion

    #region OSA Overrides
    protected override ScrollGuildRequestAllyListHolder CreateViewsHolder(int itemIndex)
    {
        var holder = new ScrollGuildRequestAllyListHolder();
        holder.Init(_Params.ItemPrefab, _Params.Content, itemIndex);
        holder.Slot.AddListener_OnClickedReject(() => onClickedReject?.Invoke(Data[holder.ItemIndex]));
        holder.Slot.AddListener_OnClickedAccept(() => onClickedAccept?.Invoke(Data[holder.ItemIndex]));
        return holder;
    }

    protected override void UpdateViewsHolder(ScrollGuildRequestAllyListHolder newOrRecycled)
    {
        var t = Data[newOrRecycled.ItemIndex];
        newOrRecycled.Slot.SetData(
            ZManagerUIPreset.Instance.GetSprite(DBGuild.GetGuildMark(t.mark_tid))
            , DBGuild.GetLevel(t.exp)
            , t.name
            , t.member_cnt
            , DBConfig.Guild_Max_Character
            , t.master_char_nick);
    }
    #endregion
}
public class ScrollGuildRequestAllyListHolder : BaseItemViewsHolder
{
    private ScrollGuildRequestAllyListSlot slot;
    public ScrollGuildRequestAllyListSlot Slot { get { return slot; } }

    public override void CollectViews()
    {
        slot = root.GetComponent<ScrollGuildRequestAllyListSlot>();
        base.CollectViews();
    }
}