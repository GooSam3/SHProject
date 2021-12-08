using Com.TheFallenGames.OSA.Core;
using Com.TheFallenGames.OSA.CustomParams;
using Com.TheFallenGames.OSA.DataHelpers;
using System;
using System.Collections.Generic;
using UnityEngine;
using WebNet;
using ZNet.Data;
using static UIFrameGuildNetCapturer;

public class ScrollGuildMemberListAdapter : OSA<BaseParamsWithPrefab, ScrollGuildMemberListHolder>
{
    private SimpleDataHelper<GuildMemberInfoConverted> Data;

    private Action<GuildMemberInfoConverted> onClicked;
    private Action<GuildMemberInfoConverted> onClicked_AddFriend;
    private Action<GuildMemberInfoConverted> onClicked_Ban;
    private Action<GuildMemberInfoConverted> onClicked_ViceMaster;
    private Action<GuildMemberInfoConverted> onClicked_DegradeViceMaster;
    private Action<GuildMemberInfoConverted> onClicked_Master;

    ulong selectedMemberID;

    bool init;

    #region Public Methods
    public void Initialize()
    {
        if (init)
            return;

        init = true;

        Data = new SimpleDataHelper<GuildMemberInfoConverted>(this);
        Init();
    }

    public void OnHide()
    {
        gameObject.SetActive(false);
    }

    public void AddListener_OnClick(Action<GuildMemberInfoConverted> callback)
    {
        onClicked += callback;
    }

    public void AddListener_OnClickAddFriend(Action<GuildMemberInfoConverted> callback)
    {
        onClicked_AddFriend += callback;
    }

    public void AddListener_OnClickBan(Action<GuildMemberInfoConverted> callback)
    {
        onClicked_Ban += callback;
    }

    public void AddListener_OnClickViceMaster(Action<GuildMemberInfoConverted> callback)
    {
        onClicked_ViceMaster += callback;
    }

    public void AddListener_OnClickDegradeViceMaster(Action<GuildMemberInfoConverted> callback)
    {
        onClicked_DegradeViceMaster += callback;
    }

    public void AddListener_OnClickMaster(Action<GuildMemberInfoConverted> callback)
    {
        onClicked_Master += callback;
    }

    public void SetSelectedMember(ulong id)
    {
        if (selectedMemberID == id)
        {
            id = 0;
        }

        this.selectedMemberID = id;
    }

    public void ApplySlots()
    {
        Data.NotifyListChangedExternally();
    }

    public void RefreshMemberList()
    {
        Refresh(UIFrameGuildNetCapturer.MyGuildData.members);
    }
    #endregion

    #region Private Methods
    private void Refresh(List<GuildMemberInfoConverted> memberInfo)
    {
        if (memberInfo != null)
        {
            RefreshData(memberInfo);
        }
    }

    private void RefreshData(List<GuildMemberInfoConverted> data)
    {
        Data.ResetItems(data);
    }

    private void OnClicked(GuildMemberInfoConverted data)
    {
        onClicked?.Invoke(data);
    }

    private void OnClicked_Ban(GuildMemberInfoConverted data)
    {
        onClicked_Ban?.Invoke(data);
    }

    private void OnClicked_AddFriend(GuildMemberInfoConverted data)
    {
        onClicked_AddFriend?.Invoke(data);
    }

    private void OnClicked_ViceMaster(GuildMemberInfoConverted data)
    {
        onClicked_ViceMaster?.Invoke(data);
    }

    private void OnClicked_DegradeViceMaster(GuildMemberInfoConverted data)
    {
        onClicked_DegradeViceMaster?.Invoke(data);
    }

    private void OnClicked_Master(GuildMemberInfoConverted data)
    {
        onClicked_Master?.Invoke(data);
    }
    #endregion

    #region OSA Overrides
    protected override ScrollGuildMemberListHolder CreateViewsHolder(int itemIndex)
    {
        var holder = new ScrollGuildMemberListHolder();
        holder.Init(_Params.ItemPrefab, _Params.Content, itemIndex);
        holder.Slot.AddListener_OnClicked(() => OnClicked(Data[holder.ItemIndex]));
        holder.Slot.AddListener_AddFriend(() => OnClicked_AddFriend(Data[holder.ItemIndex]));
        holder.Slot.AddListener_Ban(() => OnClicked_Ban(Data[holder.ItemIndex]));
        holder.Slot.AddListener_DegradeViceMaster(() => OnClicked_DegradeViceMaster(Data[holder.ItemIndex]));
        holder.Slot.AddListener_ApproveViceMaster(() => OnClicked_ViceMaster(Data[holder.ItemIndex]));
        holder.Slot.AddListener_ApproveMaster(() => OnClicked_Master(Data[holder.ItemIndex]));
        return holder;
    }

    protected override void UpdateViewsHolder(ScrollGuildMemberListHolder newOrRecycled)
    {
        var t = Data[newOrRecycled.ItemIndex];
        Sprite classSprite = null;

        var characterTableData = DBCharacter.Get(t.charTid);

        if (characterTableData != null)
        {
            classSprite = UICommon.GetClassIconSprite(characterTableData.CharacterType);
        }

        string grade = string.Empty;

        // TODO : Locale 로 해야하지않을까 ?? . 
        switch (t.grade)
        {
            case E_GuildMemberGrade.Normal:
                grade = "길드원";
                break;
            case E_GuildMemberGrade.SubMaster:
                grade = "부길드장";
                break;
            case E_GuildMemberGrade.Master:
                grade = "길드장";
                break;
        }

        newOrRecycled.Slot.SetData(
            t.charID
            , t.charID == Me.CurCharData.ID
            , selectedMemberID == t.charID
            , t.grade
            , classSprite
            , t.nick
            , grade
            , t.comment
            , t.exp
            , t.donateExp
            , t.weekExp
            , t.weekDonateExp
            , 0
            , TimeHelper.IsGivenDtToday(t.attendRewardDt, 5)
            , t.isLogin);
    }
    #endregion
}