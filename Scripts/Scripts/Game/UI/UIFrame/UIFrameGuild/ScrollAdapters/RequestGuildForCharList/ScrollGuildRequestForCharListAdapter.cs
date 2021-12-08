using Com.TheFallenGames.OSA.Core;
using Com.TheFallenGames.OSA.CustomParams;
using Com.TheFallenGames.OSA.DataHelpers;
using System;
using System.Collections.Generic;
using static UIFrameGuildNetCapturer;

public class ScrollGuildRequestForCharListAdapter : OSA<BaseParamsWithPrefab, ScrollGuildRequestForCharListHolder>
{
    private SimpleDataHelper<GuildRequestInfoForCharConverted> Data;

    private Action<ulong> OnCanceled;
    bool init;

    #region Public Methods
    public void Initialize()
    {
        if (init)
            return;

        init = true;

        Data = new SimpleDataHelper<GuildRequestInfoForCharConverted>(this);
        Init();
    }

    public void OnHide()
    {
        gameObject.SetActive(false);
    }

    public void AddListener_OnCanceled(Action<ulong> onCanceled)
    {
        OnCanceled += onCanceled;
    }

    public void RefreshRequestList()
    {
        Refresh(UIFrameGuildNetCapturer.GuildsRequestInfoForChar);
    }
    #endregion

    #region Private Methods
    private void Refresh(List<GuildRequestInfoForCharConverted> guildInfo)
    {
        RefreshData(guildInfo);
    }

    private void RefreshData(List<GuildRequestInfoForCharConverted> data)
    {
        Data.ResetItems(data);
    }
    #endregion


    #region OSA Overrides
    protected override ScrollGuildRequestForCharListHolder CreateViewsHolder(int itemIndex)
    {
        var holder = new ScrollGuildRequestForCharListHolder();
        holder.Init(_Params.ItemPrefab, _Params.Content, itemIndex);
        holder.Slot.AddListener_OnCanceled(OnCanceled);
        return holder;
    }

    protected override void UpdateViewsHolder(ScrollGuildRequestForCharListHolder newOrRecycled)
    {
        var t = Data[newOrRecycled.ItemIndex];
        newOrRecycled.Slot.SetData(
            t.guildID, ZManagerUIPreset.Instance.GetSprite(DBGuild.GetGuildMark(t.markTid))
         , t.level, t.guildName, t.curMemberCnt, t.maxMemberCnt, t.masterCharNick, t.guildIntro);
    }
    #endregion
}