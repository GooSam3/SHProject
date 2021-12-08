using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZDefine;
using ZNet.Data;

public class UIFrameChatting : ZUIFrameBase
{
    private enum E_UserChatOption
    {
        AddFriend = 1,
        InviteParty = 2,
        Block = 3,
        Whisper = 4,
    }

    public enum E_ChatSendType
    {
        View = 0,
        Normal = 1,
        All = 2,
        Whisper = 3,
        Guild = 4,
        Party = 5,
        Allience = 6,
    }

    public enum E_ChatTabType
    {
        Chatting = 1,
        Trade = 2,
        Macro = 3,
        Block = 4
    }

    [Serializable]
    private class CActiveGroup
    {
        public E_ChatTabType type;
        public GameObject activeObject;
    }

    #region # SerialzeField
    private const string MACRO_DEFAULT_FORMAT = "WChat_Macro_Default{0}";
    private const string MACRO_SAVE_ID = "Chat_Macro_{0}";
    private const string SEND_TYPE_FORMAT = "Chatting_SendType_{0}";

    // 각 모드별로 꺼지거나 켜질친구들

    [SerializeField] List<CActiveGroup> listActiveGroup = new List<CActiveGroup>();

    [SerializeField] ZUIButtonRadio radioChatTab;

    // -- 채팅 & 거래 --

    [Header("Chatting & Trade"), Space(10)]
    [SerializeField] private GameObject objSendTypeOption;
    [SerializeField] private GameObject objSendType;
    [SerializeField] private Text txtSendType;
    [SerializeField] private ZButton btnGuild;
    [SerializeField] private ZButton btnParty;
    [SerializeField] private ZButton btnAllience;
    [SerializeField] private ZButton btnWhisper;

    [SerializeField] private InputField inputChat;
    [SerializeField] private Text inputPlaceholder;

    [SerializeField] private UIChatScrollAdapter scrollChat;

    // -- 매크로 --

    [Header("Macro"), Space(10)]
    // 고정인 관계로 osa안씁
    [SerializeField] private List<Text> listMacroSlot = new List<Text>();
    [SerializeField] private GameObject objBlock;
    [SerializeField] private Text txtBlock;

    // -- 차단 -- 
    [Header("Block"), Space(10)]
    [SerializeField] private UIBlockUserScrollAdapter scrollBlockUser;
    [SerializeField] private Text txtBlockUserAmount;

    // -- 세팅팝업 --
    [Header("SettingPopup"), Space(10)]
    [SerializeField] private UISubChatSetting popupSetting;

    // -- 체팅 플레이엉 옵션 --
    [Header("ChatPlayerOption"), Space(10)]
    [SerializeField] private GameObject objOption;
    [SerializeField] private Text txtPlayerName;

    #endregion SerializeField #

    #region # FrameVariable

    private Dictionary<E_ChatTabType, CActiveGroup> dicActiveGroup = new Dictionary<E_ChatTabType, CActiveGroup>();

    // -- 매크로 -- 
    private Coroutine coBlock = null;
    private ulong lastSendMacroSec = 0;

    private E_ChatTabType curOpenTabType = E_ChatTabType.Chatting;

    // 보내기 모드
    private E_ChatSendType curSendType = E_ChatSendType.Normal;

    // 귓속말 대상이 지정됬습니까?
    private bool isSetWhisperTarget = false;

    // 귓속말 관련
    private string whisperTargetNickname = string.Empty;
    private ulong whisperTargetID = 0;

    // 채팅슬롯 옵션관련
    private ChatData selectChatSlotData;
    public override bool IsBackable => true;
    private ChatFilter curFilterType
    {
        get
        {
            if (curOpenTabType == E_ChatTabType.Trade)
            {
                return ChatFilter.TYPE_TRADE;
            }
            else if (curOpenTabType == E_ChatTabType.Chatting)
            {
                if (Me.CurCharData.chatFilter.HasFlag(ChatFilter.TYPE_TRADE))
                    Me.CurCharData.chatFilter &= ~ChatFilter.TYPE_TRADE;

                return Me.CurCharData.chatFilter;
            }

            return ChatFilter.TYPE_NONE;
        }
    }

    public bool IsInitialized { get; private set; } = false;

    #endregion FrameVariable #


    public void Init(Action onEndInit = null)
    {
        ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIBlockUserListItem), delegate
        {
            Initialize();

            onEndInit?.Invoke();
        });
    }

    private void Initialize()
    {
        foreach (var iter in listActiveGroup)
        {
            if (dicActiveGroup.ContainsKey(iter.type)) continue;
            dicActiveGroup.Add(iter.type, iter);
        }

        inputChat.characterLimit = (int)WebNet.E_CHAT_MSG_LIMIT.LIMIT;

        popupSetting.Initialize();

        InitializeChatting();
        InitializeMacro();
        InitializeBlock();
        IsInitialized = true;
    }

    protected override void OnInitialize()
    {
        base.OnInitialize();
        ZWebChatData.OnClearMsg += OnClearChatData;
    }

    protected override void OnRemove()
    {
        base.OnRemove();
        ZWebChatData.OnClearMsg -= OnClearChatData;
    }

    protected override void OnShow(int _LayerOrder)
    {
        base.OnShow(_LayerOrder);

        popupSetting.OnClickClose();
        OnClickUserChatOptionClose();

		/// 현재 연맹 채팅 탭인데, 연맹채팅이 없다 -> 일반으로 돌림 
		if (curSendType == E_ChatSendType.Allience
			&& Me.CurCharData.GuildChatState != WebNet.E_GuildAllianceChatState.Enter)
		{
			SetSendType(E_ChatSendType.Normal);
		}
		else if (btnAllience.interactable && Me.CurCharData.GuildChatState != WebNet.E_GuildAllianceChatState.Enter)
		{
            btnAllience.interactable = false;
        }

		Me.CurCharData.ChatFilterUpdate += OnUpdateChatFilter;
        
        ZWebChatData.OnAddMsg += OnAddChatMessage;
        
        CheckMacroBlock();

        radioChatTab.DoUIButtonClickEvent();

        //scrollChat.ScheduleForceRebuildLayout();
        //scrollChat.SetNormalizedPosition(1);
    }

    protected override void OnHide()
    {
        Me.CurCharData.ChatFilterUpdate -= OnUpdateChatFilter;
        ZWebChatData.OnAddMsg -= OnAddChatMessage;

        scrollChat.OnHide();

        base.OnHide();
    }

    private void OnClearChatData()
    {
        scrollChat.RefreshData();
    }

    public void OnClickChatTab(int index)
    {
        //일단 다꺼주고
        listActiveGroup.ForEach(item => item.activeObject.SetActive(false));

        if (dicActiveGroup.TryGetValue((E_ChatTabType)index, out CActiveGroup group) == false)
            return;

        // 알아서 초기화 후 켜줌
        RefreshTabGroup(group);
    }

    public void SetDefaultChatTab()
	{
        OnClickChatTab((int)UIFrameChatting.E_ChatTabType.Chatting);
    }

    /// <summary>
    /// 현재 탭이 checkTab 이라면은 intendedTargetTab 으로 변경함 .
    /// </summary>
    public void SwitchTabIfTargetTab(E_ChatSendType checkTab, E_ChatSendType intendedTargetTab)
	{
        if (curSendType == checkTab)
        {
            SetSendType(intendedTargetTab);
        }
    }

    // 탭 열릴때마다 호출됨
    private void RefreshTabGroup(CActiveGroup obj)
    {
        curOpenTabType = obj.type;
        popupSetting.OnClickClose();
        OnClickUserChatOptionClose();

        switch (curOpenTabType)
        {
            case E_ChatTabType.Chatting:
                if (Me.CurCharData.chatFilter.HasFlag(ChatFilter.TYPE_TRADE))
                    Me.CurCharData.chatFilter &= ~ChatFilter.TYPE_TRADE;

                scrollChat.ResetData(curFilterType);
                OnClickSendType((int)curSendType);

                objSendType.SetActive(true);

                scrollChat.ScheduleForceRebuildLayout();

                break;
            case E_ChatTabType.Trade:
                scrollChat.ResetData(curFilterType);

                objSendType.SetActive(false);

                SetInputType(DBLocale.GetText("Chatting_Placeholder_Trade"), ZWebChatData.GetChatViewColor(ChatViewType.TYPE_TRADE_CHAT));

                scrollChat.ScheduleForceRebuildLayout();

                break;
            case E_ChatTabType.Macro:
                break;
            case E_ChatTabType.Block:
                RefreshBlock();
                break;
        }

        obj.activeObject.SetActive(true);
    }

    #region # Chatting & Trade

    private void InitializeChatting()
    {
        scrollChat.InitScrollData(OnClickChatSlot, false);
    }

    public void OnUpdateChatFilter()
    {
        if (curOpenTabType != E_ChatTabType.Chatting)
            return;

        if (Me.CurCharData.chatFilter.HasFlag(ChatFilter.TYPE_TRADE))
            Me.CurCharData.chatFilter &= ~ChatFilter.TYPE_TRADE;

        scrollChat.ResetData(Me.CurCharData.chatFilter);
    }

    // WebChatData는 WebChatData대로 채팅데이터 저장, UI(adapter)는 UI대로저장
    // 갱신(탭이동 및 껐다켜짐)시 WebChatData에서 리스트 동기화 후 다시 각자저장함
    // => 현재 채팅타입데이터 리스트에 대한 참조가 필요한데, WebChatData에선 요청시 매번 생성하기때문에 가볍지않다
    public void OnAddChatMessage(ChatFilter filter, ChatData newMsg)
    {
        if (scrollChat.IsInitialized == false)
            return;

        // 채팅이나 거래 열려있지 않음
        if (curFilterType == ChatFilter.TYPE_NONE)
            return;

        // 필터에 없으면 지나가시고
        if (curFilterType.HasFlag(filter) == false)
            return;

        scrollChat.AddChatData(newMsg);
    }

    // 보내기모드
    // 0일시 드롭다운 출력기능
    /// <see cref="E_ChatSendType"/>
    public void OnClickSendType(int index)
    {
        SetSendType((E_ChatSendType)index);
    }

    // 친구에서 귓속말 버튼 콜백
    public void SetWhisperUser(string _nick, ulong _charID)
    {
        btnWhisper.gameObject.SetActive(true);
        SetInputType(_nick, ZWebChatData.GetChatViewColor(ChatViewType.TYPE_TRADE_CHAT));
        SetSendType(E_ChatSendType.Whisper, _nick, _charID);
    }

    private void SetInputType(string placeholder, Color viewColor)
    {
        inputPlaceholder.text = placeholder;
        inputPlaceholder.color = viewColor;
        inputChat.textComponent.color = viewColor;
        inputChat.caretColor = viewColor;
        inputChat.selectionColor = viewColor;
    }

    // 옵션클릭
    public void OnClickChatOption()
    {
        popupSetting.Open();
        SetSendTypeDropdownState(false);
    }

    public void OnChatInputValueChanged(string msg)
    {
        // 공백
        if (string.IsNullOrEmpty(msg.Trim()))
            return;

        // 글자 길이
        if (msg.Length > (int)WebNet.E_CHAT_MSG_LIMIT.LIMIT)
        {
            UIMessagePopup.ShowPopupOk(string.Format(DBLocale.GetText("Chatting_Error_Message_ToLong"), (int)WebNet.E_CHAT_MSG_LIMIT.LIMIT));
            return;
        }

#if UNITY_EDITOR
        // 수정이 끝난상태(포커스 변경)는 2가지로 나뉨
        // 유저가 정상적으로 입력을 완료했다 or 입력중 다른곳을 클릭했다
        // 에디터는 포커스 변경시에 그냥 엔터라 판단하고 채팅을 보냄
        // 스마트폰 빌드시엔 터치스크린키보드로 판단한다.
#elif UNITY_ANDROID || UNITY_IOS

        //정상완료가 아닐시 취소~
        if (inputChat.touchScreenKeyboard.status != TouchScreenKeyboard.Status.Done)
            return;
#endif

        if (curOpenTabType == E_ChatTabType.Trade)
        {
            ZWebManager.Instance.WebChat.REQ_SendTradeChatMessage(
                Me.CharID,
                Me.CurCharData.Nickname,
                Me.CurCharData.GuildId,
                Me.CurCharData.GuildMarkTid,
                Me.CurCharData.GuildName,
                msg,
                null);
        }
        else if (curOpenTabType == E_ChatTabType.Chatting)
        {
            switch (curSendType)
            {
                case E_ChatSendType.Normal:
                    ulong channelId = ZGameModeManager.Instance.CurrentGameModeType == E_GameModeType.GuildDungeon ? ZGameManager.Instance.GuildDungeonRoomNo : Me.CurCharData.LastChannelId;

                    ZWebManager.Instance.WebChat.REQ_SendNormalChatMessage(
                        Me.CharID,
                        Me.CurCharData.Nickname,
                        Me.CurCharData.GuildId,
                        Me.CurCharData.GuildMarkTid,
                        Me.CurCharData.GuildName,
                        msg,
                        Me.CurCharData.LastStageTID,
                        channelId,
                        //Me.CurCharData.LastChannelId,
                        null);
                    break;
                case E_ChatSendType.All:
                    ZWebManager.Instance.WebChat.REQ_SendServerChatMessage(
                        Me.CharID,
                        Me.CurCharData.Nickname,
                        Me.CurCharData.GuildId,
                        Me.CurCharData.GuildMarkTid,
                        Me.CurCharData.GuildName,
                        msg,
                        null);
                    break;

                case E_ChatSendType.Whisper:

                    if (isSetWhisperTarget == false)// 대상이 설정안된상태
                    {
                        if (msg.Length < DBConfig.NickName_Length_Min ||
                           msg.Length > DBConfig.NickName_Length_Max)
                        {
                            // 닉네임 길이 존중
                            UIMessagePopup.ShowPopupOk(string.Format(DBLocale.GetText("Chatting_Error_Whisper_NickLength"),
                                                     DBConfig.NickName_Length_Min, DBConfig.NickName_Length_Max));
                            return;
                        }

                        if (msg == Me.CurCharData.Nickname)
                        {
                            // 본인에게 귓속말 안됨
                            UIMessagePopup.ShowPopupOk(DBLocale.GetText("Chatting_Error_Whisper_NotMe"));
                            return;
                        }

                        ZWebManager.Instance.WebGame.REQ_FindFriend(msg, (recvPacket, recvMsgPacket) =>
                        {
                            // 대상이 없음
                            if (recvPacket.ErrCode != WebNet.ERROR.NO_ERROR)
                            {
                                UIMessagePopup.ShowPopupOk(DBLocale.GetText("Chatting_Error_Whisper_NoTarget"));
                                return;
                            }

                            // 로그안하지 않음
                            if (recvMsgPacket.IsLogin == false)
                            {
                                UIMessagePopup.ShowPopupOk(DBLocale.GetText("Chatting_Error_Whisper_NoLogin"));
                                return;
                            }

                            isSetWhisperTarget = true;
                            whisperTargetNickname = recvMsgPacket.FindNick;
                            whisperTargetID = recvMsgPacket.FindCharId;

                            SetInputType(string.Format(DBLocale.GetText("Chatting_Placeholder_Whisper_Target"), whisperTargetNickname),
                                         ZWebChatData.GetChatViewColor(ChatViewType.TYPE_WHISPER_SEND_CHAT));
                            return;
                        }, (error, reqPacket, recvPacket) =>
                        {
                            if (error == ZNet.ZWebCommunicator.E_ErrorType.Receive)
                            {
                                if (recvPacket.ErrCode != WebNet.ERROR.NO_ERROR)
                                {
                                    UIMessagePopup.ShowPopupOk(DBLocale.GetText(recvPacket.ErrCode.ToString()));
                                }
                            }

                        });
                    }
                    else// 대상 설정됨
                    {
                        // 혹시모르니 예외처리
                        if (whisperTargetID == 0)
                            return;
                        if (string.IsNullOrEmpty(whisperTargetNickname))
                            return;

                        ZWebManager.Instance.WebChat.REQ_SendWhisperMessage(
                            Me.CharID,
                            Me.CurCharData.Nickname,
                            Me.CurCharData.GuildId,
                            Me.CurCharData.GuildMarkTid,
                            Me.CurCharData.GuildName,
                            msg,
                            whisperTargetID,
                            whisperTargetNickname,
                            (recvPacket, recvMsgPacket) =>
                            {
                                // 정상아니면 리턴
                                if (recvPacket.ErrCode != WebNet.ERROR.NO_ERROR)
                                {
                                    UIMessagePopup.ShowPopupOk(DBLocale.GetText("Chatting_Error_Whisper_NoTarget"), () =>
                                    {
                                        SetSendType(E_ChatSendType.Normal);
                                    });

                                    return;
                                }

                                // 상대방에게 귓속말 보낸건 브로드캐스팅 안옴
                                // 수동으로 채팅데이터 넣어줌
                                ZWebChatData.AddChatMsg(ChatViewType.TYPE_WHISPER_SEND_CHAT,
                                                            ChatFilter.TYPE_WHISPER,
                                                            Me.CharID,
                                                            Me.CurCharData.Nickname,
                                                            msg,
                                                            Me.CurCharData.GuildId,
                                                            Me.CurCharData.GuildMarkTid,
                                                            Me.CurCharData.GuildName);
                            });
                    }

                    break;

                case E_ChatSendType.Guild:
                    ZWebManager.Instance.WebChat.REQ_SendGuildChatMessage(
                            Me.CharID,
                            Me.CurCharData.Nickname,
                            Me.CurCharData.GuildId,
                            Me.CurCharData.GuildMarkTid,
                            Me.CurCharData.GuildName,
                            msg,
                            null);

                    break;

                case E_ChatSendType.Party:
                    ZWebManager.Instance.WebChat.REQ_SendPartyChatMessage(
                            Me.CharID,
                            Me.CurCharData.Nickname,
                            Me.CurCharData.GuildId,
                            Me.CurCharData.GuildMarkTid,
                            Me.CurCharData.GuildName,
                            msg,
                            ZPartyManager.Instance.PartyUid,
                            null);
                    break;

                case E_ChatSendType.Allience:
					{
                        ZWebManager.Instance.WebChat.REQ_SendAllianceChatMessage(
                            Me.CharID,
                            Me.CurCharData.Nickname,
                            Me.CurCharData.GuildId,
                            Me.CurCharData.GuildMarkTid,
                            Me.CurCharData.GuildName,
                            msg,
                            Me.CurCharData.GuildChatId,
                            null);
                    }
                    break;

                default:
                    return;
            }
        }

        inputChat.SetTextWithoutNotify(string.Empty);
    }

    public void SetSendTypeDropdownState(bool state)
    {
        objSendTypeOption.SetActive(state);
    }

    // wTargetNick,wTargetID:귓속말대상
    // 둘다 기본값이면 대상 없다고 판단
    public void SetSendType(E_ChatSendType type, string wTargetNick = "", ulong wTargetID = 0)
    {
        string placeholder = string.Empty;
        Color viewColor = Color.white;

        switch (type)
        {
            case E_ChatSendType.View:// 선택매뉴 토글형식으로 작동
                bool activeState = !objSendTypeOption.activeSelf;

                if (activeState)
                {
                    btnGuild.interactable = (Me.CurCharData.GuildId != 0);
                    btnParty.interactable = (ZPartyManager.Instance.IsParty);
                    btnAllience.interactable = (Me.CurCharData.GuildId != 0) && (Me.CurCharData.GuildChatState == WebNet.E_GuildAllianceChatState.Enter); // (Me.CurCharData.GetAllienceGuildCount() > 0);
                }
                SetSendTypeDropdownState(activeState);
                return; //더이상 할것없는관계로 리턴~

            case E_ChatSendType.Normal:

                placeholder = DBLocale.GetText("Chatting_Placeholder_Normal");
                viewColor = ZWebChatData.GetChatViewColor(ChatViewType.TYPE_NORMAL_CHAT);
                break;
            case E_ChatSendType.All:
                if (Me.CurCharData.Level < DBConfig.Chat_All_Level)
                {
                    UIMessagePopup.ShowPopupOk(string.Format(DBLocale.GetText("Trade_OpenLevel_Des"), DBConfig.Chat_All_Level));
                    return;
                }

                placeholder = DBLocale.GetText("Chatting_Placeholder_Normal");
                viewColor = ZWebChatData.GetChatViewColor(ChatViewType.TYPE_SERVER_CHAT);
                break;
            case E_ChatSendType.Whisper: // 귓속말은 누를때마다 대상 설정하게
                isSetWhisperTarget = (!string.IsNullOrEmpty(wTargetNick) && wTargetID != 0);

                whisperTargetNickname = wTargetNick;
                whisperTargetID = wTargetID;

                if (isSetWhisperTarget == false)
                {
                    placeholder = DBLocale.GetText("Chatting_Placeholder_Whisper_Set");
                    viewColor = ZWebChatData.GetChatViewColor(ChatViewType.TYPE_WHISPER_SEND_CHAT);
                }
                else
                {
                    placeholder = string.Format(DBLocale.GetText("Chatting_Placeholder_Whisper_Target"), whisperTargetNickname);
                    viewColor = ZWebChatData.GetChatViewColor(ChatViewType.TYPE_WHISPER_SEND_CHAT);
                }
                break;
            case E_ChatSendType.Guild:
                placeholder = DBLocale.GetText("Chatting_Placeholder_Normal");
                viewColor = ZWebChatData.GetChatViewColor(ChatViewType.TYPE_GUILD_CHAT);
                break;
            case E_ChatSendType.Party:
                placeholder = DBLocale.GetText("Chatting_Placeholder_Normal");
                viewColor = ZWebChatData.GetChatViewColor(ChatViewType.TYPE_PARTY_CHAT);
                break;
            case E_ChatSendType.Allience:
                placeholder = DBLocale.GetText("Chatting_Placeholder_Normal");
                viewColor = ZWebChatData.GetChatViewColor(ChatViewType.TYPE_ALLIANCE_CHAT);
                break;
            default:
                break;
        }
        SetInputType(placeholder, viewColor);

        curSendType = type;

        txtSendType.text = DBLocale.GetText(string.Format(SEND_TYPE_FORMAT, curSendType));

        SetSendTypeDropdownState(false);
    }

    #endregion Chatting & Trade #

    #region # ChatUserOption

    // -- 채팅플레이어 옵션 -- 

    public void OnClickUserChatOption(int _option)
    {
        // 데이터가없으면 할수있는게없음
        if (selectChatSlotData == null)
            return;


        ZWebManager.Instance.WebGame.REQ_FindFriend(selectChatSlotData.CharNick, (recvPacket, recvMsgPacket) =>
        {
            // 대상이 없음
            if (recvPacket.ErrCode != WebNet.ERROR.NO_ERROR)
            {
                UIMessagePopup.ShowPopupOk(DBLocale.GetText("Chatting_Error_Whisper_NoTarget"));
                return;
            }

            // 로그안하지 않음
            if (recvMsgPacket.IsLogin == false)
            {
                UIMessagePopup.ShowPopupOk(DBLocale.GetText("Chatting_Error_Whisper_NoLogin"));
                return;
            }
            string targetNickname = recvMsgPacket.FindNick;
            ulong targetID = recvMsgPacket.FindCharId;

            switch ((E_UserChatOption)_option)
            {
                case E_UserChatOption.AddFriend:

                    // 친구 꽉참
                    if (Me.CurCharData.friendList.Count >= DBConfig.Friend_Max_Character)
                    {
                        UIMessagePopup.ShowPopupOk(DBLocale.GetText("AddFriendAlret_Max"));
                        return;
                    }

                    // 요청꽉참
                    if (Me.CurCharData.requestfriendList.Count >= DBConfig.Friend_Invite_Max)
                    {
                        UIMessagePopup.ShowPopupOk(DBLocale.GetText("AddFriendAlret_ReqMax"));
                        return;
                    }

                    // 이미 친구임
                    if (Me.CurCharData.GetFirend(targetID) != null)
                    {
                        UIMessagePopup.ShowPopupOk(DBLocale.GetText("ALREADY_FRIEND"));
                        return;
                    }

                    // 이미 요청함
                    if (Me.CurCharData.GetRequestFriend(targetID) != null)
                    {
                        UIMessagePopup.ShowPopupOk(DBLocale.GetText("ALREADY_REQUEST_FRIEND"));
                        return;
                    }

                    ZWebManager.Instance.WebGame.REQ_AddFriend(targetID, (friendRecvPacket, friendRecvMsgPacket) =>
                    {
                        // 대상 친구 꽉참
                        if (friendRecvPacket.ErrCode == WebNet.ERROR.NO_MORE_FRIEND_TARGET)
                        {
                            UIMessagePopup.ShowPopupOk(DBLocale.GetText("NO_MORE_FRIEND_TARGET"));
                            return;
                        }

                        UIMessagePopup.ShowPopupOk(string.Format(DBLocale.GetText("AddFriendAlret_Success"), targetNickname));
                    });

                    break;
                case E_UserChatOption.InviteParty:
                    //파티 가득참

                    // 파티가 아니다
                    if (ZPartyManager.Instance.IsParty == false)
                    {
                        //파티생성
                        ZPartyManager.Instance.Req_PartyCreate((partyCreatePacket, partyCreateMsg) =>
                        {
                            // 후 초대
                            ZPartyManager.Instance.Req_InviteParty(targetNickname, (recvParty, recvPartyMsg) =>
                            {
                                UIMessagePopup.ShowPopupOk(string.Format(DBLocale.GetText("Chatting_Party_Invite"), recvPartyMsg.ReceiverNick));
                            });
                        }, (errorType, errReqPacket, errRecvPacket) =>
                        {
                            UIMessagePopup.ShowPopupOk(DBLocale.GetText(errorType.ToString()));
                        });
                    }
                    else// 파티중
                    {
                        // 파티장 아님
                        if (ZPartyManager.Instance.IsMaster == false)
                        {
                            UIMessagePopup.ShowPopupOk(DBLocale.GetText("Party_Error_Invite_NotMaster"));
                            return;
                        }

                        // 이미 파티멤버
                        if (ZPartyManager.Instance.m_dicMember.ContainsKey(targetID))
                        {
                            UIMessagePopup.ShowPopupOk(DBLocale.GetText("PARTY_ALREADY_JOINED"));
                            return;
                        }

                        //인원꽉참
                        if (ZPartyManager.Instance.m_dicMember.Count >= DBConfig.Party_Max_Character)
                        {
                            UIMessagePopup.ShowPopupOk(DBLocale.GetText("PARTY_IS_FULL"));
                            return;
                        }

                        ZPartyManager.Instance.Req_InviteParty(targetNickname, (recvParty, recvPartyMsg) =>
                        {
                            UIMessagePopup.ShowPopupOk(string.Format(DBLocale.GetText("Chatting_Party_Invite"), recvPartyMsg.ReceiverNick));
                        });
                    }
                    break;
                case E_UserChatOption.Block:
                    // 이미 차단중
                    if (Me.CurCharData.IsBlockUser(targetID))
                    {
                        UIMessagePopup.ShowPopupOk(DBLocale.GetText("ALREADY_CHAT_BLOCK"));
                        return;
                    }

                    // 차단목록 꽉참
                    if (Me.CurCharData.BlockCharList.Count >= DBConfig.Block_Max_Character)
                    {
                        UIMessagePopup.ShowPopupOk(DBLocale.GetText("CHAT_BLOCK_LIST_LIMIT"));
                        return;
                    }

                    ZWebManager.Instance.WebGame.REQ_AddBlockCharacter(targetNickname, (blockRecvPacket, blockRecvMsgPacket) =>
                    {
                        UIMessagePopup.ShowPopupOk(string.Format(DBLocale.GetText("Block_Success_Character"), targetNickname));
                    });

                    break;
                case E_UserChatOption.Whisper:
                    // 거래채팅중이라면 일반탭으로 옮겨줌
                    if (curOpenTabType == E_ChatTabType.Trade)
                    {
                        radioChatTab.DoUIButtonClickEvent();
                    }

                    SetSendType(E_ChatSendType.Whisper, targetNickname, targetID);

                    break;
                default:
                    break;
            }

            return;
        }, (error, reqPacket, recvPacket) =>
        {
            if (error == ZNet.ZWebCommunicator.E_ErrorType.Receive)
            {
                if (recvPacket.ErrCode != WebNet.ERROR.NO_ERROR)
                {
                    UIMessagePopup.ShowPopupOk(DBLocale.GetText(recvPacket.ErrCode.ToString()));
                }
            }

        });


    }

    public void OnClickChatSlot(ScrollChatData data)
    {
        if(data.chatData.type == ChatViewType.TYPE_SYSTEM_GUILD_GREETING)
        {
            SendGuildGreeting(data);
        }
        else
        {
            OpenUserChatOption(data);
        }
    }
    // 채팅슬롯클릭 - 길드인사
    private void SendGuildGreeting(ScrollChatData data)
    {
        ZWebManager.Instance.WebChat.REQ_SendGuildChatMessage(
        Me.CharID,
        Me.CurCharData.Nickname,
        Me.CurCharData.GuildId,
        Me.CurCharData.GuildMarkTid,
        Me.CurCharData.GuildName,
        Me.CurCharData.GetOptionInfo(WebNet.E_CharacterOptionKey.GUILD_GREETING).OptionValue,
        null);

        ZWebChatData.RemoveChatMsg(ChatFilter.TYPE_GUILD, data.chatData);
        scrollChat.ResetData(curFilterType);
    }

    // 채팅슬롯 클릭 - 옵션팝업
    private void OpenUserChatOption(ScrollChatData data)
    {
        // 캐릭터 아이디가 0이다 == 시스템 메세지다 == 눌리지않는다
        if (data.chatData.CharId <= 0)
            return;

        // 본인의 슬롯을 클릭시 반응하지않음
        if (data.chatData.CharId == Me.CharID)
            return;

        // 이미 선택된놈의 채팅도 반응하지않음
        if (selectChatSlotData?.CharId == data.chatData.CharId)
            return;

        selectChatSlotData = data.chatData;
        txtPlayerName.text = data.chatData.CharNick;

        objOption.SetActive(true);
    }

    public void OnClickUserChatOptionClose()
    {
        selectChatSlotData = null;

        objOption.SetActive(false);
    }

    #endregion ChatUserOption #

    #region # Macro

    private void InitializeMacro()
    {
        // 매크로
        // 0 내용을입력하ㅔ숑
        // 1 2 3 기본텍스트
        for (int i = 0; i < listMacroSlot.Count; i++)
        {
            string defaultText = string.Format(MACRO_DEFAULT_FORMAT, 0);

            // 매크로 기본텍스트
            if (i<3)
            {
                defaultText = string.Format(MACRO_DEFAULT_FORMAT, i+1);

                string msg = DeviceSaveDatas.LoadData(string.Format(MACRO_SAVE_ID, i), string.Empty);
                if(msg.Trim().Length<=0)
                {
                    DeviceSaveDatas.SaveData(string.Format(MACRO_SAVE_ID, i) , DBLocale.GetText(defaultText));
                }
            }

            listMacroSlot[i].text = DeviceSaveDatas.LoadData(string.Format(MACRO_SAVE_ID, i), defaultText);
        }

        objBlock.SetActive(false);
    }

    public void OnClickMacroSlot(int idx)
    {
        var msg = DeviceSaveDatas.LoadData(string.Format(MACRO_SAVE_ID, idx), string.Empty);

        if(msg.Trim().Length<=0)
        {
            UIMessagePopup.ShowPopupOk("Chatting_Macro_Error_Nothing");
            return;
        }

        ZWebManager.Instance.WebChat.REQ_SendNormalChatMessage(
            Me.CharID,
            Me.CurCharData.Nickname,
            Me.CurCharData.GuildId,
            Me.CurCharData.GuildMarkTid,
            Me.CurCharData.GuildName,
            msg,
            Me.CurCharData.LastStageTID,
            Me.CurCharData.LastChannelId,
            null);

        SetMacroBlock(DBConfig.Chatting_Macro_Cooltime);
    }

    public void OnClickMacroEdit(int idx)
    {
        UIMessagePopup.ShowInputPopup(DBLocale.GetText("Chatting_Macro_Set_Title"),
                                       DBLocale.GetText("Chatting_Macro_Set_Holder"), (macro) =>
                                       {
                                           if (macro.Trim().Length <= 0)
                                           {
                                               UIMessagePopup.ShowPopupOk("Chatting_Macro_Error_Nothing");
                                               return;
                                           }
                                           listMacroSlot[idx].text = macro;
                                           DeviceSaveDatas.SaveData(string.Format(MACRO_SAVE_ID, idx), macro);
                                       });
    }

    // 우 하단방향으로 읽음, Emoticon_Table의 key 받아옴
    public void OnClickEmoticon(int idx)
    {
        if (DBEmoticon.Get((uint)idx, out GameDB.Emoticon_Table table) == false)
            return;

        ZWebManager.Instance.WebChat.REQ_SendNormalChatMessage(
            Me.CharID,
            Me.CurCharData.Nickname,
            Me.CurCharData.GuildId,
            Me.CurCharData.GuildMarkTid,
            Me.CurCharData.GuildName,
            DBLocale.GetText(table.EmoticonTextID),
            Me.CurCharData.LastStageTID,
            Me.CurCharData.LastChannelId,
            null);
        //출력
        SetMacroBlock(DBConfig.Chatting_Macro_Cooltime);
    }

    private void SetMacroBlock(float coolTime)
    {
        if (coBlock != null)
            StopCoroutine(coBlock);

        lastSendMacroSec = TimeManager.NowSec;

        coBlock = StartCoroutine(CoMacroBlock(coolTime));
    }

    private void CheckMacroBlock()
    {
        if(coBlock!=null)
        {
            StopCoroutine(coBlock);
            coBlock = null;
            objBlock.SetActive(false);
        }

        ulong passedSec = (ulong)DBConfig.Chatting_Macro_Cooltime - (TimeManager.NowSec - lastSendMacroSec);

        if (passedSec > DBConfig.Chatting_Macro_Cooltime||passedSec<=0)
            return;
       
        SetMacroBlock((float)passedSec);
    }

    private IEnumerator CoMacroBlock(float coolTime)
    {
        float remainTime = coolTime;
        txtBlock.text = string.Format(DBLocale.GetText("Chatting_Macro_Cooltime"), (int)remainTime);

        objBlock.SetActive(true);

        while (remainTime > 0)
        {
            yield return new WaitForSeconds(1f);

            remainTime -= 1f;

            txtBlock.text = string.Format(DBLocale.GetText("Chatting_Macro_Cooltime"), (int)remainTime);
        }

        objBlock.SetActive(false);
    }


    #endregion Macro #

    #region # Block

    public void InitializeBlock()
    {
        scrollBlockUser.SetScrollData(OnClickUnBlock);
    }

    private void RefreshBlock()
    {
        txtBlockUserAmount.text = string.Format(DBLocale.GetText("Block_Character_Amount"), Me.CurCharData.BlockCharList.Count, DBConfig.Block_Max_Character);
        scrollBlockUser.ResetData();
    }

    private void OnClickUnBlock(BlockCharacterData data)
    {
        ZWebManager.Instance.WebGame.REQ_RemoveBlockCharacter(data.CharID, (recvPacket, recvMsgPacket) =>
        {
            RefreshBlock();
        });
    }

    public void OnClickAddBlock()
    {
        UIMessagePopup.ShowInputPopup(DBLocale.GetText("Ban_Button"),
                               DBLocale.GetText("Chatting_Block_Add"), (block) =>
                               {
                                   if (block.Length < DBConfig.NickName_Length_Min ||
                                       block.Length > DBConfig.NickName_Length_Max)
                                   {
                                       // 닉네임 길이 존중
                                       UIMessagePopup.ShowPopupOk(string.Format(DBLocale.GetText("Chatting_Error_Whisper_NickLength"),
                                                                DBConfig.NickName_Length_Min, DBConfig.NickName_Length_Max));
                                       return;
                                   }

                                   if (block == Me.CurCharData.Nickname)
                                   {
                                       // 본인 안됨
                                       UIMessagePopup.ShowPopupOk(DBLocale.GetText("Chatting_Error_Whisper_NotMe"));
                                       return;
                                   }

                                   // 이미 차단중
                                   if (Me.CurCharData.IsBlockUser(block))
                                   {
                                       UIMessagePopup.ShowPopupOk(DBLocale.GetText("ALREADY_CHAT_BLOCK"));
                                       return;
                                   }

                                   // 차단목록 꽉참
                                   if (Me.CurCharData.BlockCharList.Count >= DBConfig.Block_Max_Character)
                                   {
                                       UIMessagePopup.ShowPopupOk(DBLocale.GetText("CHAT_BLOCK_LIST_LIMIT"));
                                       return;
                                   }

                                   ZWebManager.Instance.WebGame.REQ_FindFriend(block, (recvPacket, recvMsgPacket) =>
                                   {
                                       // 대상이 없음
                                       if (recvPacket.ErrCode != WebNet.ERROR.NO_ERROR)
                                       {
                                           UIMessagePopup.ShowPopupOk(DBLocale.GetText("WChat_Whisper_NoUser"));
                                           return;
                                       }

                                       ZWebManager.Instance.WebGame.REQ_AddBlockCharacter(block, (blockRecvPacket, blockRecvMsgPacket) =>
                                       {
                                           RefreshBlock();
                                           UIMessagePopup.ShowPopupOk(string.Format(DBLocale.GetText("Block_Success_Character"), block));
                                       });
                                   }, (error, reqPacket, recvPacket) =>
                                   {
                                       if (error == ZNet.ZWebCommunicator.E_ErrorType.Receive)
                                       {
                                           if (recvPacket.ErrCode != WebNet.ERROR.NO_ERROR)
                                           {
                                               UIMessagePopup.ShowPopupOk(DBLocale.GetText(recvPacket.ErrCode.ToString()));
                                           }
                                       }

                                   });
                               });

    }
    #endregion Block #
}
