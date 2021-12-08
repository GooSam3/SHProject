using Com.TheFallenGames.OSA.CustomAdapters.GridView;
using Com.TheFallenGames.OSA.DataHelpers;
using System;
using System.Collections.Generic;
using ZNet.Data;

public class ScrollGuildCreateGridAdapter : GridAdapter<GridParams, ScrollGuildCreateGuildGridHolder>
{
    private SimpleDataHelper<ScrollGuildCreateGuildMarkModel> Data;

    public Action<ScrollGuildCreateGuildGridHolder> onClickedSlot;

    byte selectedMarkTID;

    bool init;

    #region Public Methods
    public void Initialize()
    {
        if (init)
            return;

        init = true;

        Data = new SimpleDataHelper<ScrollGuildCreateGuildMarkModel>(this);
        Init();
    }

    public void OnHide()
    {
        gameObject.SetActive(false);
    }

    public void RefreshData()
    {
        var data = DBGuild.GetGuildMarkData();
        List<ScrollGuildCreateGuildMarkModel> list = new List<ScrollGuildCreateGuildMarkModel>();

        if (data != null)
        {
            list.Capacity = data.Count;

            // 내 길드 있을떄 없을떄 

            //if (Me.CurCharData.GuildId != 0)
            //{
            //    ZLog.LogError(ZLogChannel.UI, " not imple ");
            //}
            //else
            //{
            //     foreach (var data in DBGuild.GetGuildMarkData())
            //      {
            //         guildmarkTidList.Add(data.GuildMarkID);
            //         }
            //            }

            foreach (var t in data)
            {
                // 조건추가 open level 1 인애만 창설시에 보여줌 
                if(t.GuildLevel == 1)
                    list.Add(new ScrollGuildCreateGuildMarkModel() { tid = t.GuildMarkID });
            }
        }

        RefreshData(list);
    }

    public void SetSelectedMarkID(byte tid, bool notifyDataUpdate)
    {
        selectedMarkTID = tid;

        if (notifyDataUpdate)
            Data.NotifyListChangedExternally();
    }
    #endregion

    #region Private Methods
    private void RefreshData(List<ScrollGuildCreateGuildMarkModel> data)
    {
        Data.ResetItems(data);
    }

    private void OnClickedSlot(ScrollGuildCreateGuildGridHolder holder)
    {
        Data[holder.ItemIndex].hasBeenSelected = true; 
        onClickedSlot?.Invoke(holder);
    }
    #endregion


    #region OSA Overrides
    protected override void UpdateCellViewsHolder(ScrollGuildCreateGuildGridHolder viewsHolder)
    {
        var t = Data[viewsHolder.ItemIndex];
        uint guildLevel = DBGuild.GetLevel(Me.CurCharData.GuildExp);

        viewsHolder.UpdateSlot(
            t
            , guildLevel
            , ZManagerUIPreset.Instance.GetSprite(DBGuild.GetGuildMark(t.tid))
            , selectedMarkTID == t.tid
            , OnClickedSlot);
    }
    #endregion
}

public class ScrollGuildCreateGuildMarkModel
{
    // GuidMark Table ID 
    public byte tid;

    // ui 조작에서 선택됐는지 
    public bool hasBeenSelected;
}