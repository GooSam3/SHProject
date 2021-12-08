using Com.TheFallenGames.OSA.Core;
using Com.TheFallenGames.OSA.CustomParams;
using Com.TheFallenGames.OSA.DataHelpers;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UIFrameGuildNetCapturer;

public class ScrollGuildRankingListAdapter : OSA<BaseParamsWithPrefab, ScrollGuildRankingListHolder>
{
    private SimpleDataHelper<GuildRankInfoConverted> Data;

    // 랭킹 , 반환 스프라이트 
    private Func<uint, Sprite> rankSpriteGetter;

    public List<Sprite> rankingSpritesByOrder;

    bool init;

    #region Public Methods
    public void Initialize()
    {
        if (init)
            return;

        init = true;

        Data = new SimpleDataHelper<GuildRankInfoConverted>(this);
        Init();
    }

    public void SetRankSpriteGetter(Func<uint , Sprite> getter)
    {
        rankSpriteGetter = getter;
    }

    public void OnHide()
    {
        gameObject.SetActive(false);
    }

    public void RefreshRankingList()
    {
        Refresh(UIFrameGuildNetCapturer.GuildRankInfoList);
    }
    #endregion

    #region Private Methods
    private void Refresh(List<GuildRankInfoConverted> memberInfo)
    {
        if (memberInfo != null)
        {
            RefreshData(memberInfo);
        }
    }

    private void RefreshData(List<GuildRankInfoConverted> data)
    {
        Data.ResetItems(data);
    }
    #endregion

    #region OSA Overrides
    protected override ScrollGuildRankingListHolder CreateViewsHolder(int itemIndex)
    {
        var holder = new ScrollGuildRankingListHolder();
        holder.Init(_Params.ItemPrefab, _Params.Content, itemIndex);
        return holder;
    }

    protected override void UpdateViewsHolder(ScrollGuildRankingListHolder newOrRecycled)
    {
        var t = Data[newOrRecycled.ItemIndex];
        // 일단 10 위 이전까지는 추가적으로 스프라이트를 넣을수 있도록 처리함.
        Sprite sprRanking = rankSpriteGetter(10);

        newOrRecycled.Slot.SetData(
            ZManagerUIPreset.Instance.GetSprite(DBGuild.GetGuildMark(t.mark_tid))
            , sprRanking
            , t.rank
            , DBGuild.GetLevel(t.exp)
            , t.name
            , t.member_cnt
            , DBConfig.Guild_Max_Character
            , t.master_char_nick
            , t.intro);
    }
    #endregion
}
public class ScrollGuildRankingListHolder : BaseItemViewsHolder
{
    private ScrollGuildRankingListSlot slot;
    public ScrollGuildRankingListSlot Slot { get { return slot; } }

    public override void CollectViews()
    {
        slot = root.GetComponent<ScrollGuildRankingListSlot>();
        base.CollectViews();
    }
}