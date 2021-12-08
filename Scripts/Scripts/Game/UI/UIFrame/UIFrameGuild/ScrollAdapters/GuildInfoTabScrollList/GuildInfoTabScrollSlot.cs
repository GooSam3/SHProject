using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZNet.Data;
using static UIFrameGuildTab_Info;

public class GuildInfoTabScrollSlot : MonoBehaviour
{
    #region SerializedField
    #region Preference Variable
    #endregion

    #region UI Variables
    [Header("동맹/적대 길드용")]
    [SerializeField] private Image imgIcon;
    [SerializeField] private Text txtLevel_Name;
    [SerializeField] private Text txtMasterName;

    [SerializeField] private GameObject btnRoot;
    [SerializeField] private Button btnAccept;
    [SerializeField] private Button btnCancel;
    [SerializeField] private Button btnChat;

    [Header("받음 오브젝트")]
    [SerializeField] private GameObject objReceived;
    [Header("보냄 오브젝트")]
    [SerializeField] private GameObject objSent;

    //[SerializeField] private List<SelectOptionObj> activeOnSelected;
    #endregion
    #endregion

    #region System Variables
    /// <summary>
    /// ulong : 길드 아이디 
    /// </summary>
    private Action<ulong> _OnClickedSlot;
    private Action<ulong> _OnClickedAcceptBtn;
    private Action<ulong> _OnClickedCancelBtn;
    private Action<ulong> _OnClickedChatBtn;
    #endregion

    #region Properties 
    public ulong GuildID { get; private set; }
    #endregion

    #region Unity Methods
    #endregion

    #region Overrides 
    #endregion

    #region Public Methods
    public void Set(
        ulong guild_id
        , ulong guild_chatId
        , string guild_name
        , string master_nick
        , WebNet.E_GuildAllianceState allianceState
        , WebNet.E_GuildAllianceChatState chatState
        , WebNet.E_GuildAllianceChatGrade chatGrade 
     //   bool isInfoGuildInfoOrAllianceInfo
     //   , UIFrameGuildNetCapturer.GuildInfoConverted guildInfo
     //   , UIFrameGuildNetCapturer.GuildAllianceInfoConverted allianceInfo
        , IntegratedScrollTab tab
        , bool isSelected
        , Sprite iconSprite
        , uint level)
    {
		//ulong guild_id = guildInfo != null ? guildInfo.guildID : allianceInfo.guild_id;
  //      string guild_name = guildInfo != null ? guildInfo.guildName : allianceInfo.name;
  //      string master_nick = guildInfo != null ? guildInfo.masterName : allianceInfo.master_char_nick;
  //      WebNet.E_GuildAllianceState allianceState = isInfoGuildInfoOrAllianceInfo == false ? allianceInfo.state : WebNet.E_GuildAllianceState.None;
  //      WebNet.E_GuildAllianceChatState chatState = guildInfo != null ? guildInfo.chatState : allianceInfo.chat_state;

        GuildID = guild_id;
        imgIcon.sprite = iconSprite;
        txtLevel_Name.text = string.Format("Lv.{0} {1}", level, guild_name);
        txtMasterName.text = master_nick;

        #region 보냄/받음 오브젝트 및 버튼 세팅 
        bool isMyGuild = GuildID == Me.CurCharData.GuildId;
        //bool amIGuildMaster = UIFrameGuildNetCapturer.AmIMaster;
        //bool amIGuildSubMaster = UIFrameGuildNetCapturer.AmISubMaster;
        bool isAuthorized = UIFrameGuildNetCapturer.AmIMaster || UIFrameGuildNetCapturer.AmISubMaster;
        bool isMyGuildMasterGuildInChatRoom = Me.CurCharData.GuildChatGrade == WebNet.E_GuildAllianceChatGrade.Master;
		bool isMyGuildJoinedChatRoom = Me.CurCharData.GuildChatId != 0 && Me.CurCharData.GuildChatState == WebNet.E_GuildAllianceChatState.Enter;
        bool isMyGuildChatIdleState = Me.CurCharData.GuildChatState == WebNet.E_GuildAllianceChatState.None;
        bool isTargetGuildChatIdleState = chatState == WebNet.E_GuildAllianceChatState.None;

        bool recvSentCommonCheckFlag =
			isMyGuild == false
            && isAuthorized
            && (tab == IntegratedScrollTab.AllianceGuild
			|| tab == IntegratedScrollTab.AllianceChat);
        /// [받음] 상태 
        bool isReceive = recvSentCommonCheckFlag
            /// 동맹 요청을 받음 
            && ((tab == IntegratedScrollTab.AllianceGuild && allianceState == WebNet.E_GuildAllianceState.ReceiveAlliance)
            /// 채팅 참여 요청을 받음 
            || (tab == IntegratedScrollTab.AllianceChat
            && Me.CurCharData.GuildChatId == guild_chatId
            && Me.CurCharData.GuildChatGrade == WebNet.E_GuildAllianceChatGrade.Master
            && chatState == WebNet.E_GuildAllianceChatState.Request)
            /// 채팅 초대 요청을 받음 
            || (tab == IntegratedScrollTab.AllianceChat
            && Me.CurCharData.GuildChatId == guild_chatId
            && Me.CurCharData.GuildChatState == WebNet.E_GuildAllianceChatState.Receive
            && chatGrade == WebNet.E_GuildAllianceChatGrade.Master));
        /// [보냄] 상태 
        bool isSend = recvSentCommonCheckFlag
            /// 동맹 요청을 보냈음 
            && ((tab == IntegratedScrollTab.AllianceGuild && allianceState == WebNet.E_GuildAllianceState.RequestAlliance)
            /// 채팅 초대를 보냈음 
            || (tab == IntegratedScrollTab.AllianceChat
            && Me.CurCharData.GuildChatId == guild_chatId
            && Me.CurCharData.GuildChatState == WebNet.E_GuildAllianceChatState.Enter
            && chatState == WebNet.E_GuildAllianceChatState.Receive)
            /// 채팅 요청을 보냈음 
            || (tab == IntegratedScrollTab.AllianceChat 
            && Me.CurCharData.GuildChatId == guild_chatId
            && Me.CurCharData.GuildChatState == WebNet.E_GuildAllianceChatState.Request
            && chatGrade == WebNet.E_GuildAllianceChatGrade.Master));
        /// 말풍선 버튼 출력 여부 
        //bool chatBtnActive =
        //          isMyGuild == false
        //          && tab == IntegratedScrollTab.AllianceGuild
        //          && allianceState == WebNet.E_GuildAllianceState.Alliance
        //          // 길드 마스터만 말풍선 Behaviour 권한 가짐 
        //          && isAuthorized;
        bool isEnemy = allianceState == WebNet.E_GuildAllianceState.Enemy;
       // bool isGuildJoinedInChat = tab == IntegratedScrollTab.AllianceChat && chatState == WebNet.E_GuildAllianceChatState.Enter;

        objReceived.SetActive(isReceive);
        objSent.SetActive(isSend);

        /// Accept , Cancel 의 Active 는 Context 따라 다름 . 
        /// 즉 , 적대 길드 탭에서는 Accept 는 안보일거고 
        /// Cancel 은 적대 길드에서 제거하는 용도일 것이며
        /// 동맹 길드에서의 Accept 는 요청을 수락하는 용도일거고 
        /// 동맹 요청을 받은 상태에서의 Cancel 은 거절일거고 
        /// 동맹 상태일때는 동맹을 해제하는 용도일거임 즉 . Context 따라 다름 . 
        /// 해당 분기 처리는 UIFrameGuildTab_Info 에 Handler 함수별로 동작 . 
        btnAccept.interactable = isReceive;
        btnCancel.interactable =
            // 적대길드탭아님 && 요청 받음 && 나 마스터 
            (tab != IntegratedScrollTab.EnemyGuild && isReceive)
            // 적대길드탭아님 && 요청 보냄 && 나 마스터 
            || (tab != IntegratedScrollTab.EnemyGuild && isSend)
            // 적 길드임 && 나 길마 or 부길마
            || (isEnemy && isAuthorized)
            // 동맹 리스트 탭 && 나 길마 or 부길마 
            || (tab == IntegratedScrollTab.AllianceGuild && isAuthorized)
            // 연맹채팅탭 && 나 길마 or 부길마 && 내 슬롯 => 연맹 채팅 탈퇴용 
            || (tab == IntegratedScrollTab.AllianceChat && isAuthorized && isMyGuild)
            // 연맹채팅탭 && 우리 길드가 채팅방 마스터 길드 && 나 길마 or 부길마 => 탈퇴/강퇴용 
            || (tab == IntegratedScrollTab.AllianceChat && isMyGuildMasterGuildInChatRoom && isAuthorized);

        btnChat.interactable =
			// 동맹관계 && 내 길드 Idle 스테이트 체킹 &&  나 길마 or 부길마 && 선택한 연맹 길드는 채팅방 참여중인데 , 거기서 마스터 길드이다 => 마스터인 타겟 연맹 길드에게 입장 요청 가능 
			(allianceState == WebNet.E_GuildAllianceState.Alliance && isMyGuildChatIdleState && isAuthorized && chatState == WebNet.E_GuildAllianceChatState.Enter && chatGrade == WebNet.E_GuildAllianceChatGrade.Master)
			// 동맹관계 && 내 길드가 채팅방 참여중이다 && 나 길마 or 부길마 && 현재 채팅방에서 마스터 길드이다 && 타겟 길드가 Idle 스테이트다 => 초대 요청 가능 
			|| (allianceState == WebNet.E_GuildAllianceState.Alliance && isMyGuildJoinedChatRoom && isAuthorized && isMyGuildMasterGuildInChatRoom && isTargetGuildChatIdleState);

        btnAccept.gameObject.SetActive(true);
        btnCancel.gameObject.SetActive(true);
        btnChat.gameObject.SetActive(true);

        //btnChat.gameObject.SetActive(chatBtnActive);
        btnRoot.SetActive(isSelected);

        #endregion
    }

    public void AddListener_OnClickedSlot(Action<ulong> callback)
    {
        _OnClickedSlot += callback;
    }

    public void AddListener_OnClickedAcceptBtn(Action<ulong> callback)
    {
        _OnClickedAcceptBtn += callback;
    }

    public void AddListener_OnClickedCancelBtn(Action<ulong> callback)
    {
        _OnClickedCancelBtn += callback;
    }

    public void AddListener_OnClickedChatBtn(Action<ulong> callback)
    {
        _OnClickedChatBtn += callback;
    }
    #endregion

    #region Private Methods
    #endregion

    #region OnClick Event (인스펙터 연결)
    public void OnClicked()
    {
        _OnClickedSlot?.Invoke(GuildID);
    }

    public void OnClickAcceptBtn()
	{
        _OnClickedAcceptBtn?.Invoke(GuildID);
	}

    public void OnClickXbtn()
    {
        _OnClickedCancelBtn?.Invoke(GuildID);
    }

    public void OnClickChatBtn()
    {
        _OnClickedChatBtn?.Invoke(GuildID);
    }
    #endregion
}
