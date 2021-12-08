using Com.TheFallenGames.OSA.CustomAdapters.GridView;
using Com.TheFallenGames.OSA.DataHelpers;
using System;
using System.Collections.Generic;
using ZNet.Data;

public class ScrollGuildSettingGridAdapter : GridAdapter<GridParams, ScrollGuildSettingGridHolder>
{
    private SimpleDataHelper<ScrollGuildSettingGuildMarkModel> Data;

    public Action<ScrollGuildSettingGridHolder> onClickedSlot;

    byte selectedMarkTID;

    bool init;

    #region Public Methods
    public void Initialize()
    {
        if (init)
            return;

        init = true;

        Data = new SimpleDataHelper<ScrollGuildSettingGuildMarkModel>(this);
        Init();
    }

    public void OnHide()
    {
        gameObject.SetActive(false);
    }

    public void RefreshData()
    {
        var data = DBGuild.GetGuildMarkData();
        List<ScrollGuildSettingGuildMarkModel> list = new List<ScrollGuildSettingGuildMarkModel>();

        if (data != null)
        {
            list.Capacity = data.Count;

            foreach (var t in data)
            {
                list.Add(new ScrollGuildSettingGuildMarkModel() { tid = t.GuildMarkID });
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
    private void RefreshData(List<ScrollGuildSettingGuildMarkModel> data)
    {
        Data.ResetItems(data);
    }

    private void OnClickedSlot(ScrollGuildSettingGridHolder holder)
    {
        Data[holder.ItemIndex].hasBeenSelected = true;
        onClickedSlot?.Invoke(holder);
    }
    #endregion

    #region OSA Overrides
    protected override void UpdateCellViewsHolder(ScrollGuildSettingGridHolder viewsHolder)
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

public class ScrollGuildSettingGuildMarkModel
{
    // GuidMark Table ID 
    public byte tid;

    public bool hasBeenSelected;
}