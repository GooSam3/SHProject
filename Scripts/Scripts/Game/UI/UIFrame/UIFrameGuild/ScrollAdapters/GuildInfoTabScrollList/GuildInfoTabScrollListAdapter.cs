using Com.TheFallenGames.OSA.Core;
using Com.TheFallenGames.OSA.CustomParams;
using Com.TheFallenGames.OSA.DataHelpers;
using Devcat;
using System;
using System.Collections.Generic;
using static UIFrameGuildTab_Info;

public class GuildInfoTabScrollListAdapter : OSA<BaseParamsWithPrefab, GuildInfoTabScrollListHolder>
{
    // private SimpleDataHelper<GuildRequestInfoForCharConverted> Data;
    private EnumDictionary<IntegratedScrollTab, SimpleDataHelper<IntegratedMemberListModel>> Data = new EnumDictionary<IntegratedScrollTab, SimpleDataHelper<IntegratedMemberListModel>>();

    //private Action<ulong> OnCanceled;

    private IntegratedScrollTab curTab;
    private Action<IntegratedScrollTab, IntegratedMemberListModel> onClicked_slot;
    private Action<IntegratedScrollTab, IntegratedMemberListModel> onClicked_acceptBtn;
    private Action<IntegratedScrollTab, IntegratedMemberListModel> onClicked_cancelBtn;
    private Action<IntegratedScrollTab, IntegratedMemberListModel> onClicked_chatBtn;

    // 현재 선택된 슬롯 관련 
    private IntegratedScrollTab selectedSlot_tab;
    private IntegratedMemberListModel selectedSlot_data;

    bool init;

    #region Public Methods
    public void Initialize()
    {
        if (init)
            return;

        init = true;

        Data.Add(IntegratedScrollTab.None, new SimpleDataHelper<IntegratedMemberListModel>(this));
        Data.Add(IntegratedScrollTab.EnemyGuild, new SimpleDataHelper<IntegratedMemberListModel>(this));
        Data.Add(IntegratedScrollTab.AllianceGuild, new SimpleDataHelper<IntegratedMemberListModel>(this));
        Data.Add(IntegratedScrollTab.AllianceChat, new SimpleDataHelper<IntegratedMemberListModel>(this));

        Init();
    }

    public void OnHide()
    {
        gameObject.SetActive(false);
    }

    public void AddListener_OnClickSlot(Action<IntegratedScrollTab, IntegratedMemberListModel> callback)
    {
        onClicked_slot += callback;
    }

    public void AddListener_OnClickSlotAcceptBtn(Action<IntegratedScrollTab, IntegratedMemberListModel> callback)
    {
        onClicked_acceptBtn += callback;
    }

    public void AddListener_OnClickSlotCancelBtn(Action<IntegratedScrollTab, IntegratedMemberListModel> callback)
    {
        onClicked_cancelBtn += callback;
    }

    public void AddListener_OnClickSlotChatBtn(Action<IntegratedScrollTab, IntegratedMemberListModel> callback)
    {
        onClicked_chatBtn += callback;
    }

    public void SetSelectedSlot(IntegratedScrollTab tab, IntegratedMemberListModel data)
    {
        selectedSlot_tab = tab;
        selectedSlot_data = data;
    }

    public void SetDataTab(IntegratedScrollTab tab)
    {
        curTab = tab;
    }

    public void OnlySetData(IntegratedScrollTab tab, List<IntegratedMemberListModel> data)
    {
        Data[tab].List.Clear();
        Data[tab].List.AddRange(data);
    }

    //public void RefreshData(IntegratedScrollTab tab, List<IntegratedMemberListModel> data)
    //{
    //    SetDataTab(tab);
    //    Data[tab].ResetItems(data);
    //}

    public void ApplySlot()
    {
        Data[curTab].NotifyListChangedExternally();
    }
    #endregion

    #region Private Methods
    private void OnClickedSlot(GuildInfoTabScrollListHolder holder)
    {
        onClicked_slot?.Invoke(curTab, GetCurrentData()[holder.ItemIndex]);
    }

    private void OnClickedSlotAcceptBtn(GuildInfoTabScrollListHolder holder)
    {
        onClicked_acceptBtn?.Invoke(curTab, GetCurrentData()[holder.ItemIndex]);
    }

    private void OnClickedSlotCancelBtn(GuildInfoTabScrollListHolder holder)
    {
        onClicked_cancelBtn?.Invoke(curTab, GetCurrentData()[holder.ItemIndex]);
    }

    private void OnClickedSlotChatBtn(GuildInfoTabScrollListHolder holder)
    {
        onClicked_chatBtn?.Invoke(curTab, GetCurrentData()[holder.ItemIndex]);
    }


    private SimpleDataHelper<IntegratedMemberListModel> GetCurrentData()
    {
        return Data[curTab];
    }
    #endregion

    #region OSA Overrides
    protected override GuildInfoTabScrollListHolder CreateViewsHolder(int itemIndex)
    {
        var holder = new GuildInfoTabScrollListHolder();
        holder.Init(_Params.ItemPrefab, _Params.Content, itemIndex);
        holder.Slot.AddListener_OnClickedSlot((guildID) => OnClickedSlot(holder));
        holder.Slot.AddListener_OnClickedAcceptBtn((guildID) => OnClickedSlotAcceptBtn(holder));
        holder.Slot.AddListener_OnClickedCancelBtn((guildID) => OnClickedSlotCancelBtn(holder));
        holder.Slot.AddListener_OnClickedChatBtn((guildID) => OnClickedSlotChatBtn(holder));
        return holder;
    }

    protected override void UpdateViewsHolder(GuildInfoTabScrollListHolder newOrRecycled)
    {
        var t = GetCurrentData()[newOrRecycled.ItemIndex];

        bool isSelected =
            selectedSlot_tab != IntegratedScrollTab.None &&
            selectedSlot_data == t;

        // 통합으로 처리함 
        if (curTab == IntegratedScrollTab.AllianceGuild ||
            curTab == IntegratedScrollTab.EnemyGuild ||
            curTab == IntegratedScrollTab.AllianceChat)
        {
            /// 내 길드가 ChattingMaster 면은 DataSource 가 달라지기에 이런식으로 가져옴  
            ulong guild_id = t.GetGuildID();
            ulong guild_chatId = t.GetGuildChatID();
            string guild_name = t.GetGuildName();
            string master_nick = t.GetMasterNick();
            WebNet.E_GuildAllianceState allianceState = t.GetAllianceState();
            WebNet.E_GuildAllianceChatState chatState = t.GetChatState();
            WebNet.E_GuildAllianceChatGrade chatGrade = t.GetChatGrade();
            ulong exp = t.GetExp();
            byte markTid = t.GetMarkTid();

			/// ChatRoom 인 경우 , 마스터를 찾아서 넘김 . 
			newOrRecycled.Slot.Set(
				guild_id, guild_chatId, guild_name, master_nick, allianceState, chatState, chatGrade
				//t.isChatRoom
				//, t.isChatRoom ? UIFrameGuildNetCapturer.MyGuildData.myGuildInfo : null
				//, t.allianceInfo
				, curTab
				, isSelected
				, ZManagerUIPreset.Instance.GetSprite(DBGuild.GetGuildMark(markTid))
				, DBGuild.GetLevel(exp));
		}
	}
    #endregion
}

public class GuildInfoTabScrollListHolder : BaseItemViewsHolder
{
    private GuildInfoTabScrollSlot slot;
    public GuildInfoTabScrollSlot Slot { get { return slot; } }

    public override void CollectViews()
    {
        slot = root.GetComponent<GuildInfoTabScrollSlot>();
        base.CollectViews();
    }
}