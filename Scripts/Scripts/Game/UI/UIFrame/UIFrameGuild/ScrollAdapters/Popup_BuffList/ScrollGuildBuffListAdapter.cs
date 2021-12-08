using Com.TheFallenGames.OSA.Core;
using Com.TheFallenGames.OSA.CustomParams;
using Com.TheFallenGames.OSA.DataHelpers;
using System.Collections.Generic;
using ZNet.Data;

public class ScrollGuildBuffListAdapter : OSA<BaseParamsWithPrefab, ScrollGuildBuffListHolder>
{
    private SimpleDataHelper<ScrollGuildBuffListModel> Data;

    bool init;

    #region Public Methods
    public void Initialize()
    {
        if (init)
            return;

        init = true;

        Data = new SimpleDataHelper<ScrollGuildBuffListModel>(this);
        Init();
    }

    public void Refresh(List<uint> buffTids)
    {
        List<ScrollGuildBuffListModel> list = new List<ScrollGuildBuffListModel>();

        for (int i = 0; i < buffTids.Count; i++)
        {
            list.Add(new ScrollGuildBuffListModel() { tid = buffTids[i] });
        }

        RefreshData(list);
    }

    #endregion

    #region Private Methods
    private void RefreshData(List<ScrollGuildBuffListModel> data)
    {
        Data.ResetItems(data);
    }
    #endregion


    #region OSA Overrides
    protected override ScrollGuildBuffListHolder CreateViewsHolder(int itemIndex)
    {
        var holder = new ScrollGuildBuffListHolder();
        holder.Init(_Params.ItemPrefab, _Params.Content, itemIndex);
        return holder;
    }

    protected override void UpdateViewsHolder(ScrollGuildBuffListHolder newOrRecycled)
    {
        var buffData = DBGuild.GetGuildBuffData(Data[newOrRecycled.ItemIndex].tid);

        if(buffData != null)
        {
            var guildData = DBGuild.GetGuildDataByLevel(buffData.OpenLevel);

            string benefit01 = guildData.ContentsTextID;
            string benefit02 = guildData.BuffTextID;
            bool obtained = false;

            if (Me.CurCharData.GuildExp != 0 && Me.CurCharData.GuildId != 0)
            {
                obtained = DBGuild.GetLevel(Me.CurCharData.GuildExp) >= guildData.GuildLevel;
            }

            newOrRecycled.Slot.SetData(
                guildData.GuildLevel
                , DBLocale.GetText(benefit01)
                , DBLocale.GetText(benefit02).Replace("\n", " / ")
                , obtained);
        }
    }
    #endregion
}

public class ScrollGuildBuffListModel
{
    // Guild Table 의 Tid 
    public uint tid;
}