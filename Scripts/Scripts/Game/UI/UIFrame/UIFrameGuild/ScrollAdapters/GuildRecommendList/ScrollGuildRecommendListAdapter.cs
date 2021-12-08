using Com.TheFallenGames.OSA.Core;
using Com.TheFallenGames.OSA.CustomParams;
using Com.TheFallenGames.OSA.DataHelpers;
using System.Collections.Generic;
using System.Linq;
using static UIFrameGuildNetCapturer;
using static UIFrameGuildTab_GuildSuggestion;

public class ScrollGuildRecommendListAdapter : OSA<BaseParamsWithPrefab, ScrollGuildRecommendListHolder>
{
    private SimpleDataHelper<GuildInfoConverted> Data;
    List<GuildInfoConverted> oriData;
    OnRequestJoin onRequestJoin;

    bool init;

    #region Public Methods
    public void Initialize()
    {
        if (init)
            return;

        init = true;

        Data = new SimpleDataHelper<GuildInfoConverted>(this);
        Init();
    }

    public void OnHide()
    {
        gameObject.SetActive(false);
    }

    public void RefreshListSingle(GuildInfoConverted guildInfo)
    {
        Refresh(new List<GuildInfoConverted>() { guildInfo });
    }

    public void RefreshRecommendList()
    {
        // 전체 추천길드 리스트에서 현재 이미 신청했던 리스트에 없는애만 추려서 Refresh 함 
        var list = GuildsRecommended.Where(
            t_rec => GuildsRequestInfoForChar.Exists(t_req => t_req.guildID == t_rec.guildID) == false).ToList();

        Refresh(list);
        oriData = list;
    }

    public void RefreshFilteredBy(string words, out int resultSearchedGuildCount)
    {
        resultSearchedGuildCount = 0;

        if (string.IsNullOrEmpty(words))
        {
            Refresh(oriData);
        }

        List<GuildInfoConverted> filteredList = new List<GuildInfoConverted>();

        foreach (var t in oriData)
        {
            if (t.guildName.IndexOf(words, System.StringComparison.OrdinalIgnoreCase) >= 0
                && GuildsRequestInfoForChar.Exists(t_req => t.guildID == t_req.guildID) == false)
            {
                filteredList.Add(t);
            }
        }

        resultSearchedGuildCount = filteredList.Count;

        Refresh(filteredList);
    }

    public void AddListener_OnRequestJoin(OnRequestJoin onRequestJoin)
    {
        this.onRequestJoin += onRequestJoin;
    }
    #endregion

    #region Private Methods
    private void Refresh(List<GuildInfoConverted> guildInfo)
    {
        RefreshData(guildInfo);
    }

    private void RefreshData(List<GuildInfoConverted> data)
    {
        Data.ResetItems(data);
    }

    private void OnRequestJoin(ulong guildID, string comment, bool isQuickJoin)
    {
        onRequestJoin?.Invoke(guildID, comment, isQuickJoin);
    }
    #endregion


    #region OSA Overrides
    protected override ScrollGuildRecommendListHolder CreateViewsHolder(int itemIndex)
    {
        var holder = new ScrollGuildRecommendListHolder();
        holder.Init(_Params.ItemPrefab, _Params.Content, itemIndex);
        holder.Slot.AddListener_OnRequestJoin(OnRequestJoin);
        return holder;
    }

    protected override void UpdateViewsHolder(ScrollGuildRecommendListHolder newOrRecycled)
    {
        var t = Data[newOrRecycled.ItemIndex];

        newOrRecycled.Slot.SetData(
            t.guildID, ZManagerUIPreset.Instance.GetSprite(DBGuild.GetGuildMark(t.markTid))
            , t.level, t.guildName, t.curTotalMemberCount, t.maxMemberCntBySystem, t.masterName, t.introduction, t.isQuickJoin);
    }
    #endregion
}

//public class ScrollGuildRecommendListModel
//{
//    public string guildName;
//    public string masterName;
//    public string introduction;

//    public uint level;
//    public uint curMemberCnt;
//    public uint maxMemberCnt;
//}