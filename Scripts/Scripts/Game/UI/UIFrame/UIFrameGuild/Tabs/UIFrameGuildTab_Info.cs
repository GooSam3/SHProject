using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WebNet;
using ZNet.Data;
using static UIFrameGuildNetCapturer;

public class UIFrameGuildTab_Info : UIFrameGuildTabBase
{
    public enum IntegratedScrollTab
    {
        None = 0,
        EnemyGuild,
        AllianceGuild,
        AllianceChat
    }

    // 적대길드/동맹길드/연맹채팅 을 하나의 adapter 로 통합해 사용하기 위하여 Model 을 통합함 
    public class IntegratedMemberListModel
    {
        public IntegratedScrollTab tab;
        public int sortPriority;
        public GuildInfoConverted guildInfo; // 나의 길드를 띄어줘야할때는 AllianceInfo 데이터 타입을 사용할수없어 + 추가.
        public GuildAllianceInfoConverted allianceInfo; // 적대길드면 적대길드 리스트 들어가있을거고 .. 이런식 

        public ulong GetGuildID()
		{
            return this.guildInfo != null ? guildInfo.guildID : allianceInfo.guild_id;
		}

        public ulong GetGuildChatID()
        {
            return this.guildInfo != null ? guildInfo.chatID : allianceInfo.chat_id;
        }

        public string GetGuildName()
		{
            return this.guildInfo != null ? guildInfo.guildName : allianceInfo.name;
        }

        public string GetMasterNick()
		{
            return this.guildInfo != null ? guildInfo.masterName : allianceInfo.master_char_nick;
        }

        public ulong GetExp()
        {
            return this.guildInfo != null ? guildInfo.exp : allianceInfo.exp;
        }

        public E_GuildAllianceState GetAllianceState()
		{
            return this.guildInfo != null ? E_GuildAllianceState.None: allianceInfo.state;
        }

        public E_GuildAllianceChatState GetChatState()
		{
            return this.guildInfo != null ? guildInfo.chatState : allianceInfo.chat_state;
        }

        public E_GuildAllianceChatGrade GetChatGrade()
		{
            return this.guildInfo != null ? guildInfo.chatGrade : allianceInfo.chat_grade;
		}

        public byte GetMarkTid()
		{
            return this.guildInfo != null ? guildInfo.markTid : allianceInfo.mark_tid;
        }
	}

    [Serializable]
    public class IntegratedScrollBtnTab
    {
        public IntegratedScrollTab tab;
        public Toggle toggle;
        public GameObject objRedDot;
        /// public List<GameObject> activeObjOnOpen;
    }

    #region SerializedField
    #region Preference Variable
    [SerializeField] private IntegratedScrollTab defaultScrollTab = IntegratedScrollTab.EnemyGuild;
    #endregion

    #region UI Variables
    [SerializeField] private GuildInfoTabScrollListAdapter ScrollAdapter;

    [SerializeField] private List<GameObject> objActiveIfMaster;
    [SerializeField] private List<GameObject> objActiveIfSubmaster;
    [SerializeField] private List<GameObject> objActiveIfNormal;

    [Header("왼쪽  UI")]
    [SerializeField] private Image imgGuildMark;
    [SerializeField] private Text txtGuildName;
    [SerializeField] private Text txtGuildLevel;
    [SerializeField] private Text txtGuildExp; // e.g (30%)
    [SerializeField] private Slider sliderExpGauge;

    /// <summary>
    /// [가입 신청] 버튼 레드닷 
    /// </summary>
    [SerializeField] private GameObject objGuildJoinRequestBtnRedDot_Master;
    [SerializeField] private GameObject objGuildJoinRequestBtnRedDot_SubMaster;

    [SerializeField] private Text txtAttendStatusCnt;

    [Header("오른쪽 UI")]
    [SerializeField] private List<IntegratedScrollBtnTab> integratedScrollBtnTabs;

    [SerializeField] private Image imgGuildMoneyIcon;

    [SerializeField] private Text txtGuildMaster;
    [SerializeField] private Text txtMemberCnt;
    [SerializeField] private Text txtRanking;
    [SerializeField] private Text txtMoney;
    [SerializeField] private Text txtIntroduction;
    [SerializeField] private Text txtNoti;
    [SerializeField] private Text txtWeeklyContributeNum;

    [Header("스크롤쪽 슬롯 제외 UI")]
    [SerializeField] private Text txtScrollContentCnt;
    [SerializeField] private Button btnReqAllianceGuild;
    [SerializeField] private Button btnRegisterEnemyGuild;
    [SerializeField] private Button btnCreateAllianceChatRoom;
    [SerializeField] private Button btnLeaveAllianceChatRoom;
    #endregion
    #endregion

    #region System Variables
    // Scroll 관련 
    private IntegratedScrollTab curTab;

    private IntegratedScrollTab selectedSlotTab;
    private IntegratedMemberListModel selectedSlotModel;

    private bool ignoreClickEvent;
    #endregion

    #region Properties 
    #endregion

    #region Public Methods
    private void UpdateUI_MainInfo()
    {
        var guildInfo = UIFrameGuildNetCapturer.MyGuildData.myGuildInfo;
        var meMemberInfo = UIFrameGuildNetCapturer.MyGuildData.MyMemberInfo;
        ulong maxExp = DBGuild.GetGuildLvMaxExp(guildInfo.exp);
        var myGrade = Me.CurCharData.GuildGrade;

        // 왼쪽 UI 세팅 
        txtGuildName.text = guildInfo.guildName;
        txtGuildLevel.text = string.Format("Lv.{0}", guildInfo.level);  // FIX ME : LOCALE 
        var markSpriteID = DBGuild.GetGuildMark(guildInfo.markTid);
        imgGuildMark.sprite = ZManagerUIPreset.Instance.GetSprite(markSpriteID);
        txtGuildExp.text = string.Format("{0}%", ((guildInfo.exp / (float)maxExp) * 100).ToString("0.00"));
        sliderExpGauge.value = (guildInfo.exp / (float)maxExp);
        txtMoney.text = guildInfo.money.ToString("n0");
                txtAttendStatusCnt.text = string.Format("{0}/{1}", UIFrameGuildNetCapturer.TodayAttendanceCount, guildInfo.curTotalMemberCount);

        objActiveIfMaster.ForEach(t => t.SetActive(myGrade == E_GuildMemberGrade.Master));
        objActiveIfSubmaster.ForEach(t => t.SetActive(myGrade == E_GuildMemberGrade.SubMaster));
        objActiveIfNormal.ForEach(t => t.SetActive(myGrade == E_GuildMemberGrade.Normal));

		if (myGrade == E_GuildMemberGrade.Master || myGrade == E_GuildMemberGrade.SubMaster)
		{
			GameObject redDot = myGrade == E_GuildMemberGrade.Master ? objGuildJoinRequestBtnRedDot_Master : objGuildJoinRequestBtnRedDot_SubMaster;
            redDot.SetActive(UIFrameGuildNetCapturer.IsTargetRedDotFlagOn(GuildRedDotStatusFlag.GuildInfo_ReceivedGuildJoinRequest));
		}

        // 오른쪽 UI 세팅 
        txtGuildMaster.text = guildInfo.masterName;
        txtMemberCnt.text = string.Format("{0}/{1}", guildInfo.curTotalMemberCount, guildInfo.maxMemberCntBySystem);
        txtRanking.text = guildInfo.rank.ToString();
        txtIntroduction.text = guildInfo.introduction;
        txtNoti.text = guildInfo.notice;

        if (meMemberInfo != null)
            txtWeeklyContributeNum.text = meMemberInfo.weekDonateExp.ToString();
    }

    private void UpdateUI_ScrollSub()
    {
		//foreach (var integTab in integratedScrollBtnTabs)
		//{
  //          bool isCurTab = curTab == integTab.tab;
  //          integTab.activeObjOnOpen.ForEach(t => t.SetActive(isCurTab));
		//}

        /// 버튼 처리 
        bool isGuildMaster = Me.CurCharData.GuildGrade == E_GuildMemberGrade.Master;
        bool isMyGuildJoinedChatRoom = Me.CurCharData.GuildChatId != 0;

        btnReqAllianceGuild.interactable = isGuildMaster;
        btnReqAllianceGuild.gameObject.SetActive(curTab == IntegratedScrollTab.AllianceGuild);
        btnRegisterEnemyGuild.interactable = isGuildMaster;
        btnRegisterEnemyGuild.gameObject.SetActive(curTab == IntegratedScrollTab.EnemyGuild);

		btnCreateAllianceChatRoom.gameObject.SetActive(isGuildMaster && curTab == IntegratedScrollTab.AllianceChat && isMyGuildJoinedChatRoom == false);
        btnLeaveAllianceChatRoom.gameObject.SetActive(isGuildMaster && curTab == IntegratedScrollTab.AllianceChat && isMyGuildJoinedChatRoom);

        /// Red Dot 처리 
        if (UIFrameGuildNetCapturer.AmIMaster || UIFrameGuildNetCapturer.AmISubMaster)
        {
            bool showAllianceTabRedDot = UIFrameGuildNetCapturer.IsTargetRedDotFlagOn(GuildRedDotStatusFlag.GuildInfo_AllianceTab);
            bool showAllianceChatTabRedDot = UIFrameGuildNetCapturer.IsTargetRedDotFlagOn(GuildRedDotStatusFlag.GuildInfo_AllianceChatTab);

            var targetTab = integratedScrollBtnTabs.Find(t => t.tab == IntegratedScrollTab.AllianceGuild);
            if (targetTab != null)
            {
                targetTab.objRedDot.SetActive(showAllianceTabRedDot);
            }

            targetTab = integratedScrollBtnTabs.Find(t => t.tab == IntegratedScrollTab.AllianceChat);
            if (targetTab != null)
            {
                targetTab.objRedDot.SetActive(showAllianceChatTabRedDot);
            }
		}
		else
		{
            integratedScrollBtnTabs.ForEach(t => t.objRedDot.SetActive(false));
        }

        // TODO : 맥시멈 카운트 ?? 하다말음 일단 .
        /*        var allianceInfo = UIFrameGuildNetCapturer.MyGuildData.myGuildAllianceInfo;
                int curContentCnt = 0;
                int maxContentCnt = 0;

                if (allianceInfo != null)
                {
                    if (allianceInfo.ContainsKey(E_GuildAllianceState.Alliance))
                    {
                        foreach (var guild in allianceInfo[E_GuildAllianceState.Alliance])
                        {
                            if (guild.chat_state == E_GuildAllianceChatState.Enter)
                                curContentCnt++;
                        }
                    }
                }
                else
                {


                }

                txtScrollContentCnt.text = string.Format("{0}/{1}"); */
    }
    #endregion

    #region Overrides 
    public override void Initialize(UIFrameGuild guildFrame, FrameGuildTabType type)
    {
        base.Initialize(guildFrame, type);

        var slot = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(GuildInfoTabScrollSlot));
        ScrollAdapter.Parameters.ItemPrefab = slot.GetComponent<RectTransform>();
        var pf = ScrollAdapter.Parameters.ItemPrefab;
        pf.SetParent(transform);
        pf.localScale = Vector2.one;
        pf.localPosition = Vector3.zero;
        pf.gameObject.SetActive(false);

        ScrollAdapter.AddListener_OnClickSlot(HandleAllianceSectionSlotClicked);
        ScrollAdapter.AddListener_OnClickSlotAcceptBtn(HandleAllianceSectionSlotClicked_AcceptBtn);
        ScrollAdapter.AddListener_OnClickSlotCancelBtn(HandleAllianceSectionSlotClicked_CancelBtn);
        ScrollAdapter.AddListener_OnClickSlotChatBtn(HandleAllianceSectionSlotClicked_ChatBtn);
        ScrollAdapter.Initialize();
    }

    public override void OnUpdateEventRise(UpdateEventType type, GuildDataUpdateEventParamBase param)
    {
        base.OnUpdateEventRise(type, param);

        bool updateMainInfo = type == UpdateEventType.DataAllRefreshed
            || type == UpdateEventType.DataRefreshed_GuildInfo
            || type == UpdateEventType.DataRefreshed_GuildInfoAndMemberInfo
			|| type == UpdateEventType.DataRefreshed_ReceivedGuildJoinRequests
			|| type == UpdateEventType.ObtainedGuildReward;
        bool updateAllianceScrollData = type == UpdateEventType.DataAllRefreshed
            || type == UpdateEventType.DataRefreshed_AllianceGuildInfo;
            // || type == UpdateEventType.CreatedAllianceChatRoom;

        if (updateMainInfo)
		{
            UpdateUI_MainInfo();
		}

        if(updateAllianceScrollData)
		{
            RefreshAllianceRelatedScrollData(false);
		}
	}

	public override void OnOpen()
    {
        base.OnOpen();
        ignoreClickEvent = false;
        UpdateUI_MainInfo();
        UpdateUI_ScrollSub();
        RefreshAllianceRelatedScrollData();
    }
    #endregion

    #region Private Methods
    private void RefreshAllianceRelatedScrollData(bool reset = true)
    {
		UpdateOnlyScrollData();

        if (reset)
        {
            SetSelectedSlot(IntegratedScrollTab.None, null);
        }

		SetScrollTab(reset ? defaultScrollTab : curTab, true);
	}
    
    private void TryPromtLeaveChatRoom()
	{
        /// 내 길드가 마스터 길드면 
        if (UIFrameGuildNetCapturer.IsMyGuildChatRoomMaster)
        {
            var joinList = UIFrameGuildNetCapturer.GuildIDListParticipatedInChat;

            if (joinList != null)
            {
                /// 만약 조인한 길드가 하나다 => 나 혼자임 , 즉 탈퇴(해산) 가능 
                if (joinList.Count == 1)
                {
                    LeaveChatRoomWithAgree();
                }
                /// 만약 조인한 길드가 하나 이상이다 => 마스터 위임전까지는 탈퇴/해산 불가 
                else if (joinList.Count > 1)
                {
                    OpenNotiUp("참여중인 길드가 존재합니다.");
                }
            }
        }
        /// 아니면 그냥 탈퇴 로직
        else
        {
            LeaveChatRoomWithAgree();
        }
    }

    private void LeaveChatRoomWithAgree()
	{
        OpenTwoButtonQueryPopUp("확인", DBLocale.GetText("Alliance_Chat_Exit_Confirm")
            , onConfirmed: () =>
            {
                UIFrameGuildNetCapturer.ReqGuildAllianceLeaveChat(Me.CurCharData.GuildId, Me.CurCharData.GuildChatId, Me.CurCharData.GuildChatGrade
                         , (revPacketReq, resListReq) =>
                         {
                             guildController.NotifyUpdateEvent(UpdateEventType.RequestAllianceInfo, new EventParam_ReqAllianceState(
                                 new E_GuildAllianceState[] { E_GuildAllianceState.Alliance }));

                             ZWebManager.Instance.WebChat.CheckEnterChannel();
                         });
            });
    }

    /// <summary>
    /// 스크롤 탭마다 데이터 세팅 
    /// </summary>
	private void UpdateOnlyScrollData()
    {
        List<IntegratedMemberListModel> dataList = new List<IntegratedMemberListModel>();

        var sourceDic = UIFrameGuildNetCapturer.MyGuildData.myGuildAllianceInfo;

        /// CAUTION : E_GuildAllianceState 는 Sort 순서이기도함 
        
        /// 적대 길드 탭 데이터 세팅 
        SetAllianceDataListByState(dataList, null, E_GuildAllianceState.Enemy);
        ScrollAdapter.OnlySetData(IntegratedScrollTab.EnemyGuild, dataList);

        /// 연맹 길드 탭 데이터 세팅 
        SetAllianceDataListByState(dataList, null, E_GuildAllianceState.Alliance, E_GuildAllianceState.ReceiveAlliance, E_GuildAllianceState.RequestAlliance);
        ScrollAdapter.OnlySetData(IntegratedScrollTab.AllianceGuild, dataList);

        dataList.Clear();

        /// 연맹 채팅 탭 데이터 세팅. 
        // bool amIMaster = AmIMaster;
        bool amIAuthorized = AmIMaster || AmISubMaster;

        /// 내 길드가 참여중인 채팅방이 없다면 
        /// 요청 정보 데이터 세팅 
        if (Me.CurCharData.GuildChatState != E_GuildAllianceChatState.Enter)
        {
            if(amIAuthorized)
            { 
                /// 내가 보낸 요청 존재 || 내게 보낸 초대 존재 
                if(Me.CurCharData.GuildChatState == E_GuildAllianceChatState.Request
                    || Me.CurCharData.GuildChatState == E_GuildAllianceChatState.Receive)
                {
                    SetAllianceDataListByState(dataList,
                        (queryAllianceInfo) =>
                        {
                            /// 나랑 ChatID 가 같으면서 , 
					        return Me.CurCharData.GuildChatId == queryAllianceInfo.chat_id
                            && queryAllianceInfo.chat_grade == E_GuildAllianceChatGrade.Master;
                        }
                        , new E_GuildAllianceState[] { E_GuildAllianceState.Alliance });
                }
            }

            /// 마스터이면서 내가 요청받았거나 보낸경우만 체킹 
			//if (amIMaster && (Me.CurCharData.GuildChatState == E_GuildAllianceChatState.Request || Me.CurCharData.GuildChatState == E_GuildAllianceChatState.Receive))
			//{
			//	SetAllianceDataListByState(dataList,
			//	(queryAllianceInfo) =>
			//	{
   //                 /// 나랑 ChatID 가 같으면서 , 
			//		return Me.CurCharData.GuildChatId == queryAllianceInfo.chat_id
			//			&& (queryAllianceInfo.chat_state == E_GuildAllianceChatState.Enter
   //                     || queryAllianceInfo.chat_state == E_GuildAllianceChatState.Receive
			//			|| queryAllianceInfo.chat_state == E_GuildAllianceChatState.Request);
			//	}
			//	, new E_GuildAllianceState[] { E_GuildAllianceState.Alliance });
			//}
		}
		/// 나의 연맹 채팅방이 존재할때는
		/// 채팅방에 참여하고있는 길드 및 내가 길장 && 마스터 길드면은 초대들 띄움 
		else
		{
			var chatRoomGuildIDs = UIFrameGuildNetCapturer.GuildIDListParticipatedInChat;

            /// 참여중인 길드 ID 순회 및 리스트에 Add
			foreach (var guildID in chatRoomGuildIDs)
			{
                var model = new IntegratedMemberListModel();

                model.tab = IntegratedScrollTab.AllianceChat;
                
                /// 내 길드라면 AllianceInfo 가 아닌 GuildInfo 로 세팅 
                if (Me.CurCharData.GuildId == guildID)
				{
                    model.guildInfo = UIFrameGuildNetCapturer.MyGuildData.myGuildInfo;
				}
				else
				{
                    model.allianceInfo = UIFrameGuildNetCapturer.FindAllianceInfoByID(guildID);

                    if(model.allianceInfo == null)
					{
                        ZLog.LogError(ZLogChannel.UI, "Failed to Retrieve AllianceInfo For ChattingRoomGuildList , TargetGuildID : " + guildID);
					}
				}

                /// 둘다 NULL 이다 -> 뭔가 에러가 난거임. 
				if (model.guildInfo != null || model.allianceInfo != null)
					dataList.Add(model);
			}

            /// 길드방에서 마스터 길드 && 나 길마 or 부길마 => 보낸 참여 요청 및 받은 참여 요청 Add
			if (UIFrameGuildNetCapturer.IsMyGuildChatRoomMaster && amIAuthorized)
			{
                UIFrameGuildNetCapturer.MyGuildData.ForeachAlliance((state, allianceInfo) =>
                {
                    /// 동맹 && 채팅방 ID 체킹 && 상대 동맹 길드가 연맹 채팅 요청 상태 && 동맹 길드가 연맹 채팅 요청 받은 상태  
                    if(state == E_GuildAllianceState.Alliance 
                    && Me.CurCharData.GuildChatId == allianceInfo.chat_id
                    && (allianceInfo.chat_state == E_GuildAllianceChatState.Request || allianceInfo.chat_state == E_GuildAllianceChatState.Receive))
					{
                        var model = new IntegratedMemberListModel() { tab = IntegratedScrollTab.AllianceChat, allianceInfo = allianceInfo };
                        dataList.Add(model);
					}
                });
			}
        }

        ScrollAdapter.OnlySetData(IntegratedScrollTab.AllianceChat, dataList);

		// 동맹 길드 추가 
		//if (sourceDic.ContainsKey(E_GuildAllianceState.Alliance))
		//{
		//    dataList.Capacity = sourceDic[E_GuildAllianceState.Alliance].Count;

		//    foreach (var t in sourceDic[E_GuildAllianceState.Alliance])
		//    {
		//        dataList.Add(new IntegratedMemberListModel() { allianceInfo = t });
		//    }

		//    ScrollAdapter.OnlySetData(IntegratedScrollTab.AllianceGuild, dataList);
		//}

		//dataList.Clear();

		//// 적 길드 추가 
		//if (sourceDic.ContainsKey(E_GuildAllianceState.Enemy))
		//{
		//    dataList.Capacity = sourceDic[E_GuildAllianceState.Enemy].Count;

		//    foreach (var t in sourceDic[E_GuildAllianceState.Enemy])
		//    {
		//        dataList.Add(new IntegratedMemberListModel() { allianceInfo = t });
		//    }

		//    ScrollAdapter.OnlySetData(IntegratedScrollTab.EnemyGuild, dataList);
		//}

		//dataList.Clear();

		//// 연맹 채팅 추가 
		//if (sourceDic.ContainsKey(E_GuildAllianceState.Alliance))
		//{
		//    dataList.Capacity = sourceDic[E_GuildAllianceState.Alliance].Count;

		//    foreach (var t in sourceDic[E_GuildAllianceState.Alliance])
		//    {
		//        // Enter 상태일때만 연맹 추가 . ( 이게맞나 ? 기획 확인필요 ) 
		//        if (t.chat_state == E_GuildAllianceChatState.Enter)
		//        {
		//            dataList.Add(new IntegratedMemberListModel() { allianceInfo = t });
		//        }
		//    }

		//    ScrollAdapter.OnlySetData(IntegratedScrollTab.AllianceChat, dataList);
		//}
	}

    /// <summary>
    /// Add 할 연맹 정보 States 들 . Sort 기준으로도 사용됨 
    /// </summary>
    /// <param name="targetList"></param>
    /// <param name="states"></param>
    private void SetAllianceDataListByState(
        List<IntegratedMemberListModel> targetList
        , Predicate<GuildAllianceInfoConverted> filterer
        , params E_GuildAllianceState[] states)
	{
        var sourceDic = UIFrameGuildNetCapturer.MyGuildData.myGuildAllianceInfo;

        targetList.Clear();

		foreach (var allianceInfo in sourceDic)
		{
            if(Array.Exists(states, (stateType) => stateType == allianceInfo.Key))
			{
                int sortPriority = Array.FindIndex(states, t => t == allianceInfo.Key);
                allianceInfo.Value.ForEach(t =>
                {
                    bool add = true;
                    
                    if(filterer != null && filterer(t) == false)
					{
                        add = false;
					}

                    if (add)
					{
						targetList.Add(new IntegratedMemberListModel() { allianceInfo = t, sortPriority = sortPriority });
					}
                });
			}
		}

		targetList.Sort((t01, t02) =>
		{
            return t01.sortPriority.CompareTo(t02.sortPriority);
		});
	}

    private void SetObjActive_ByAuthority()
    {
        // Me.CurCharData.GuildId
        __NoImplement__();
    }

    private void SetScrollTab(IntegratedScrollTab tab, bool forceUpdate = false)
    {
        var target = integratedScrollBtnTabs.Find(t => t.tab == tab);

        if (target == null)
        {
            target = integratedScrollBtnTabs.Find(t => t.tab == defaultScrollTab);
        }

        target.toggle.isOn = true;

        if (forceUpdate)
        {
            UpdateScrollTab();
        }
    }

    private void SetSelectedSlot(IntegratedScrollTab tab, IntegratedMemberListModel data)
    {
        selectedSlotTab = tab;
        selectedSlotModel = data;
    }

    void UpdateScrollTab(bool tabChanged = true) // , bool selected = true)
    {
        bool adapterApply = false;

        adapterApply = true;
        ScrollAdapter.SetSelectedSlot(selectedSlotTab, selectedSlotModel);

        if (tabChanged)
        {
            adapterApply = true;
            var targetTab = integratedScrollBtnTabs.Find(t => t.toggle.isOn);

            if (targetTab == null)
            {
                targetTab = integratedScrollBtnTabs.Find(t => t.tab.Equals(defaultScrollTab));
                curTab = defaultScrollTab;
            }
            else
            {
                curTab = targetTab.tab;
            }

            ScrollAdapter.SetNormalizedPosition(1d);
            ScrollAdapter.SetDataTab(curTab);
        }

        if (adapterApply)
        {
            ScrollAdapter.ApplySlot();
        }

        UpdateUI_ScrollSub();
    }

    private void RemoveAllianceOrEnemy(IntegratedMemberListModel target)
    {
        bool isEnemy = target.allianceInfo.state == E_GuildAllianceState.Enemy;
        string targetGuildName = target.allianceInfo.name;

        UIFrameGuildNetCapturer.ReqGuildAllianceRemove(
            Me.CurCharData.GuildId
            , UIFrameGuildNetCapturer.MyGuildName
            , isEnemy
            , target.allianceInfo.guild_id
            , target.allianceInfo.name
            , (revPacketReq, resListReq) =>
            {
                E_GuildAllianceState[] updateStates;
                string displayText = string.Empty;

                if (isEnemy)
                {
                    updateStates = new E_GuildAllianceState[] { E_GuildAllianceState.Alliance, E_GuildAllianceState.Enemy };
                    displayText = "Wguid_EnemyClear_Notice";
                }
				else
                {
                    updateStates = new E_GuildAllianceState[] { E_GuildAllianceState.Alliance, E_GuildAllianceState.ReceiveAlliance , E_GuildAllianceState.RequestAlliance };
                    displayText = "Wguid_AlliClear_Notice";
                }

                guildController.NotifyUpdateEvent(UpdateEventType.RequestAllianceInfo, new EventParam_ReqAllianceState(updateStates));

                OpenNotiUp(string.Format(DBLocale.GetText(displayText), targetGuildName));
            }, (err, req, res) =>
            {
                ZWebManager.ShowErrorPopup(res.ErrCode);
                guildController.NotifyUpdateEvent(UpdateEventType.RequestAllianceInfo, new EventParam_ReqAllianceState(new E_GuildAllianceState[]
                    {
                        E_GuildAllianceState.Alliance,
                        E_GuildAllianceState.Enemy,
                        E_GuildAllianceState.ReceiveAlliance,
                        E_GuildAllianceState.RequestAlliance
					}));
			});
    }

    #region Slot Btn Click Handling Logic

    /// <summary>
    /// 슬롯 자체 클릭 핸들링 
    /// </summary>
    private void HandleAllianceSectionSlotClicked(IntegratedScrollTab tab, IntegratedMemberListModel data)
    {
        bool isSelected = selectedSlotTab == IntegratedScrollTab.None;
        if(isSelected)
		{
            SetSelectedSlot(tab, data);
		}
		else
		{
            SetSelectedSlot(IntegratedScrollTab.None, null);
        }

        UpdateScrollTab(tabChanged: false); // , selected: isSelected);
    }

    /// <summary>
    /// 슬롯의 수락 버튼 클릭 핸들링 
    /// </summary>
    private void HandleAllianceSectionSlotClicked_AcceptBtn(IntegratedScrollTab tab, IntegratedMemberListModel data)
    {
        /// 길드 요청 수락
        if (tab == IntegratedScrollTab.AllianceGuild)
        {
            OpenTwoButtonQueryPopUp("확인", DBLocale.GetText("Alliance_Join_Accept_Confirm")
                , onConfirmed: () =>
                {
                    UIFrameGuildNetCapturer.ReqGuildAllianceAccept(Me.CurCharData.GuildId, Me.CurCharData.GuildName, data.allianceInfo.guild_id
                        , (revPacketReq, resListReq) =>
                        {
                            /// Refresh 데이터 요청 
                            guildController.NotifyUpdateEvent(UpdateEventType.RequestAllianceInfo, new EventParam_ReqAllianceState(
                                new E_GuildAllianceState[] { 
                                    E_GuildAllianceState.Alliance
									, E_GuildAllianceState.RequestAlliance
									, E_GuildAllianceState.ReceiveAlliance }));

                            OpenNotiUp(string.Format(DBLocale.GetText("Wguid_Alli_Notice"), data.allianceInfo.name));
						}, (err, req, res) =>
						{
							ZWebManager.ShowErrorPopup(res.ErrCode);

							/// Refresh 데이터 요청 
							guildController.NotifyUpdateEvent(UpdateEventType.RequestAllianceInfo, new EventParam_ReqAllianceState(
								new E_GuildAllianceState[] {
									E_GuildAllianceState.Alliance
									, E_GuildAllianceState.RequestAlliance
									, E_GuildAllianceState.ReceiveAlliance }));
						});
				});
		}
        /// 받은 채팅방 입장 요청 및 초대 처리 
        else if (tab == IntegratedScrollTab.AllianceChat)
        {
            /// 내 길드가 채팅방 마스터길드 => 참여요청 수락처리 
            if(Me.CurCharData.GuildChatGrade == E_GuildAllianceChatGrade.Master)
			{
                OpenTwoButtonQueryPopUp("확인", DBLocale.GetText("Alliance_Chat_Join_Accept_Confirm")
                    , onConfirmed: () =>
                    {
                        UIFrameGuildNetCapturer.ReqGuildAllianceAcceptChat(Me.CurCharData.GuildId, data.allianceInfo.guild_id
                            , (revPacketReq, resListReq) =>
                            {
                                /// Refresh 데이터 요청 
                                guildController.NotifyUpdateEvent(UpdateEventType.RequestAllianceInfo, new EventParam_ReqAllianceState(
                                    new E_GuildAllianceState[]
                                    {
                                        E_GuildAllianceState.Alliance
                                    }));
                            }, (err, req, res) =>
                            {
                                ZWebManager.ShowErrorPopup(res.ErrCode);

                                guildController.NotifyUpdateEvent(UpdateEventType.RequestAllianceInfo, new EventParam_ReqAllianceState(
                                    new E_GuildAllianceState[]
                                    {
                                        E_GuildAllianceState.Alliance
                                    }));
                            });
                    });
			}
			/// 초대 요청 수락 처리 
			else
			{
				OpenTwoButtonQueryPopUp("확인", DBLocale.GetText("Alliance_Chat_Invite_Accept_Confirm")
                    , onConfirmed: () =>
                    {
						UIFrameGuildNetCapturer.ReqGuildAllianceInviteAcceptChat(Me.CurCharData.GuildId
							, (revPacketReq, resListReq) =>
							{
                                /// Refresh 데이터 요청 
                                guildController.NotifyUpdateEvent(UpdateEventType.RequestAllianceInfo, new EventParam_ReqAllianceState(
                                    new E_GuildAllianceState[]
                                    {
                                        E_GuildAllianceState.Alliance
                                    }));

                                ZWebManager.Instance.WebChat.CheckEnterChannel();
                            }, (err,req,res) =>
                            {
                                ZWebManager.ShowErrorPopup(res.ErrCode);

								/// Refresh 데이터 요청 
								guildController.NotifyUpdateEvent(UpdateEventType.RequestAllianceInfo, new EventParam_ReqAllianceState(
									new E_GuildAllianceState[]
									{
										E_GuildAllianceState.Alliance
									}));
							});
					});
			}
		}
		else
        {
            ZLog.LogError(ZLogChannel.UI, "This Function is not supposed to be called on other case");
        }
    }

    /// <summary>
    /// 슬롯의 Cancel 버튼 클릭 핸들링
    /// </summary>
    private void HandleAllianceSectionSlotClicked_CancelBtn(IntegratedScrollTab tab, IntegratedMemberListModel data)
    {
        /// 동맹 길드 탭
		if (tab == IntegratedScrollTab.AllianceGuild)
		{
            /// CAUTION : 보낸 연맹 요청 취소, 받은 연맹 요청 거절 둘다 같은 프로토콜 사용 
            
            /// 내가 해당 길드로 연맹 요청을 보낸경우 -> 취소 
			if (data.allianceInfo.state == E_GuildAllianceState.RequestAlliance)
			{
                OpenTwoButtonQueryPopUp("확인", DBLocale.GetText("Alliance_Invite_Cancel_Confirm")
                    , onConfirmed: () =>
					{
                        UIFrameGuildNetCapturer.ReqGuildAllianceReject(Me.CurCharData.GuildId, data.allianceInfo.guild_id
							, (revPacketReq, resListReq) =>
							{
								/// 받은 연맹 요청 Refresh 
								guildController.NotifyUpdateEvent(UpdateEventType.RequestAllianceInfo, new EventParam_ReqAllianceState(
									new E_GuildAllianceState[] { E_GuildAllianceState.RequestAlliance, E_GuildAllianceState.ReceiveAlliance }));
							}, (err, req, res) =>
							{
								ZWebManager.ShowErrorPopup(res.ErrCode);

								/// 받은 연맹 요청 Refresh 
								guildController.NotifyUpdateEvent(UpdateEventType.RequestAllianceInfo, new EventParam_ReqAllianceState(
									new E_GuildAllianceState[] { E_GuildAllianceState.RequestAlliance, E_GuildAllianceState.ReceiveAlliance }));
							});
					});
			}
            /// 내가 해당 길드로부터 연맹 요청을 받은 경우 -> 거절 
            else if (data.allianceInfo.state == E_GuildAllianceState.ReceiveAlliance)
			{
				OpenTwoButtonQueryPopUp("확인", DBLocale.GetText("Alliance_Invite_Refuse_Confirm")
                    , onConfirmed: () =>
					{
						UIFrameGuildNetCapturer.ReqGuildAllianceReject(Me.CurCharData.GuildId, data.allianceInfo.guild_id
							, (revPacketReq, resListReq) =>
							{
								/// 받은 연맹 요청 Refresh 
								guildController.NotifyUpdateEvent(UpdateEventType.RequestAllianceInfo, new EventParam_ReqAllianceState(
                                    new E_GuildAllianceState[] { E_GuildAllianceState.ReceiveAlliance, E_GuildAllianceState.RequestAlliance }));
							}, (err, req, res) =>
                            {
                                ZWebManager.ShowErrorPopup(res.ErrCode);

                                /// 받은 연맹 요청 Refresh 
								guildController.NotifyUpdateEvent(UpdateEventType.RequestAllianceInfo, new EventParam_ReqAllianceState(
                                    new E_GuildAllianceState[] { E_GuildAllianceState.ReceiveAlliance, E_GuildAllianceState.RequestAlliance }));
                            });
					});
			}
			/// 이미 연맹이다 -> 연맹 삭제 
			else if (data.allianceInfo.state == E_GuildAllianceState.Alliance)
			{
				string popUpMsg = "연맹을 끊으시겠습니까?";

				OpenTwoButtonQueryPopUp("확인", popUpMsg
					, onConfirmed: () =>
					{
						RemoveAllianceOrEnemy(data);
					});
			}
			else
			{
				ZLog.LogError(ZLogChannel.UI, "Not other case supported or update");
			}
		}
        /// 적대 길드 탭 
        else if(tab == IntegratedScrollTab.EnemyGuild)
		{
            /// 적대 길드 해제 
            OpenTwoButtonQueryPopUp("확인", DBLocale.GetText("Enemy_Guild_Set_Cancel_Confirm")
                , onConfirmed: () =>
                {
                    RemoveAllianceOrEnemy(data);
                });
        }
        /// 연맹 채팅 탭 
		else if (tab == IntegratedScrollTab.AllianceChat)
		{
            /// 내가 보낸 요청 취소 및 초대 거절 처리 (이때는 참여한 채팅방이없음)
            if (Me.CurCharData.GuildChatState == E_GuildAllianceChatState.Request
                || Me.CurCharData.GuildChatState == E_GuildAllianceChatState.Receive)
			{
                string comment = Me.CurCharData.GuildChatState == E_GuildAllianceChatState.Request ?
                    DBLocale.GetText("Alliance_Chat_Join_Cancel_Confirm")
                    : DBLocale.GetText("Alliance_Chat_Invite_Cancel_Confirm");

                OpenTwoButtonQueryPopUp("확인", comment
                    , onConfirmed: () =>
                    {
                        /// 보낸 요청 취소, 받은 요청 거절 즉 두가지 액션을 한번에함 
                        UIFrameGuildNetCapturer.ReqGuildAllianceCancelChat(Me.CurCharData.GuildId
                            , (revPacketReq, resListReq) =>
                            {
                                guildController.NotifyUpdateEvent(UpdateEventType.RequestAllianceInfo, new EventParam_ReqAllianceState(
                                    new E_GuildAllianceState[] { E_GuildAllianceState.Alliance }));
                            }, (err, req ,res) =>
                            {
                                ZWebManager.ShowErrorPopup(res.ErrCode);
                                guildController.NotifyUpdateEvent(UpdateEventType.RequestAllianceInfo, new EventParam_ReqAllianceState(
                                    new E_GuildAllianceState[] { E_GuildAllianceState.Alliance }));
                            });
                    });
			}
            /// 채팅방 참여중일때
			else if(Me.CurCharData.GuildChatState == E_GuildAllianceChatState.Enter)
			{
                bool isSelectedSlotMyGuild = data.guildInfo != null;

                /// 내 길드일때, 탈퇴 또는 길드방 해체
                if (isSelectedSlotMyGuild)
                {
                    TryPromtLeaveChatRoom();
                }
                /// 연맹 길드를 눌렀을때, 즉 요청거절/강퇴
                else
                {
                    /// 입장 요청을 한 상태 => 거절 
                    if(data.allianceInfo.chat_state == E_GuildAllianceChatState.Request)
					{
						OpenTwoButtonQueryPopUp("확인", DBLocale.GetText("Alliance_Join_Refuse_Confirm")
                            , onConfirmed: () =>
							{
								UIFrameGuildNetCapturer.ReqGuildAllianceRejectChat(Me.CurCharData.GuildId, data.allianceInfo.guild_id
									, (revPacketReq, resListReq) =>
									{
										guildController.NotifyUpdateEvent(UpdateEventType.RequestAllianceInfo, new EventParam_ReqAllianceState(
											new E_GuildAllianceState[] { E_GuildAllianceState.Alliance }));
									}, (err,req,res)=>
                                    {
                                        ZWebManager.ShowErrorPopup(res.ErrCode);
                                        guildController.NotifyUpdateEvent(UpdateEventType.RequestAllianceInfo, new EventParam_ReqAllianceState(
                                            new E_GuildAllianceState[] { E_GuildAllianceState.Alliance }));
                                    });
							});
					}
					/// 입장 초대를 받은 상태 => 초대 취소 
					if (data.allianceInfo.chat_state == E_GuildAllianceChatState.Receive)
					{
                        OpenTwoButtonQueryPopUp("확인", DBLocale.GetText("Alliance_Invite_Refuse_Confirm")
                            , onConfirmed: () =>
                            {
                                UIFrameGuildNetCapturer.ReqGuildAllianceInviteCancelChat(
                                    Me.CurCharData.GuildId
                                    , data.allianceInfo.guild_id
                                    , (revPacketReq, resListReq) =>
                                    {
                                        guildController.NotifyUpdateEvent(UpdateEventType.RequestAllianceInfo, new EventParam_ReqAllianceState(
                                            new E_GuildAllianceState[] { E_GuildAllianceState.Alliance }));
                                    }, (err, req, res)=>
                                    {
                                        ZWebManager.ShowErrorPopup(res.ErrCode);
                                        guildController.NotifyUpdateEvent(UpdateEventType.RequestAllianceInfo, new EventParam_ReqAllianceState(
                                            new E_GuildAllianceState[] { E_GuildAllianceState.Alliance }));
                                    });
                            });
                    }
					/// 이미 들어와있는 상태 => 강퇴 
					else if (data.allianceInfo.chat_state == E_GuildAllianceChatState.Enter)
					{
						OpenTwoButtonQueryPopUp("확인", DBLocale.GetText("Alliance_Guild_Ban_Confirm")
                            , onConfirmed: () =>
							{
								UIFrameGuildNetCapturer.ReqGuildAllianceBanChat(Me.CurCharData.GuildId, data.allianceInfo.guild_id
									, (revPacketReq, resListReq) =>
									{
										guildController.NotifyUpdateEvent(UpdateEventType.RequestAllianceInfo, new EventParam_ReqAllianceState(
											new E_GuildAllianceState[] { E_GuildAllianceState.Alliance }));
									}, (err, req, res)=>
                                    {
                                        ZWebManager.ShowErrorPopup(res.ErrCode);
                                        guildController.NotifyUpdateEvent(UpdateEventType.RequestAllianceInfo, new EventParam_ReqAllianceState(
                                            new E_GuildAllianceState[] { E_GuildAllianceState.Alliance }));
                                    });
							});
					}
				}
			}
		}
        else
        {
            ZLog.LogError(ZLogChannel.UI, "This Function is not supposed to be called on other case");
        }
    }

	/// <summary>
	/// 슬롯의 채팅 버튼 클릭 핸들링 
	/// </summary>
	private void HandleAllianceSectionSlotClicked_ChatBtn(IntegratedScrollTab tab, IntegratedMemberListModel data)
	{
        /// 기본적인 조건문들은 버튼 출력해주는 선에서 해주므로 최소한의 예외처리만함 
		if (tab != IntegratedScrollTab.AllianceGuild)
		{
			ZLog.LogError(ZLogChannel.UI, "Must be only able to click chatBtn in AllianceGuild Tab");
			return;
		}
		else if (data.GetGuildID() == Me.CurCharData.GuildId)
		{
			ZLog.LogError(ZLogChannel.UI, "ChatBtn Must be invisible(so not clickable) when the guild is myGuild");
			return;
		}

		bool isMyGuildJoinedChatRoom = Me.CurCharData.GuildChatId != 0 && Me.CurCharData.GuildChatState == WebNet.E_GuildAllianceChatState.Enter;

		/// 내 길드가 채팅방 참여중이 아니다 && 상대 길드가 참여중이면서 마스터 길드이다 => 참여 요청 
		if (isMyGuildJoinedChatRoom == false && data.allianceInfo.chat_state == E_GuildAllianceChatState.Enter && data.allianceInfo.chat_grade == E_GuildAllianceChatGrade.Master)
		{
			OpenTwoButtonQueryPopUp("확인", DBLocale.GetText("Alliance_Chat_Join_Confirm")
                , onConfirmed: () =>
				{
					UIFrameGuildNetCapturer.ReqGuildAllianceRequestChat(
						Me.CurCharData.GuildId
						, data.allianceInfo.guild_id
						, (revPacketReq, resListReq) =>
						{
							guildController.NotifyUpdateEvent(UpdateEventType.RequestAllianceInfo, new EventParam_ReqAllianceState(
                                new E_GuildAllianceState[] { E_GuildAllianceState.Alliance }));
						}, (err, req, res) =>
                        {
                            ZWebManager.ShowErrorPopup(res.ErrCode);
                            guildController.NotifyUpdateEvent(UpdateEventType.RequestAllianceInfo, new EventParam_ReqAllianceState(
                                new E_GuildAllianceState[] { E_GuildAllianceState.Alliance }));
                        });
				});
		}
        /// 내 길드 채팅방 없음 && 상대도 없음 => 개설을 먼저 한 다음 초대를 해야함. 
		else if (isMyGuildJoinedChatRoom == false && data.allianceInfo.chat_state == E_GuildAllianceChatState.None)
		{
            OpenNotiUp(DBLocale.GetText("Alliance_Chat_Create_Error"));

            /// TODO : LOCALE 
            //OpenTwoButtonQueryPopUp("확인", "연맹 채팅을 개설하시겠습니까?."
            //    , onConfirmed: () =>
            //     {

            //     });
        }
		/// 내 길드가 채팅방 참여중이다 => 초대 
		else if (isMyGuildJoinedChatRoom)
		{
			OpenTwoButtonQueryPopUp("확인", DBLocale.GetText("Alliance_Chat_Invite_Confirm")
                , onConfirmed: () =>
				{
					UIFrameGuildNetCapturer.ReqGuildAllianceInviteRequestChat(
						Me.CurCharData.GuildId
						, data.allianceInfo.guild_id
						, (revPacketReq, resListReq) =>
						{
							guildController.NotifyUpdateEvent(UpdateEventType.RequestAllianceInfo, new EventParam_ReqAllianceState(
                                new E_GuildAllianceState[] { E_GuildAllianceState.Alliance }));
						}, (err, req, res) =>
                        {
                            ZWebManager.ShowErrorPopup(res.ErrCode);
                            guildController.NotifyUpdateEvent(UpdateEventType.RequestAllianceInfo, new EventParam_ReqAllianceState(
                                new E_GuildAllianceState[] { E_GuildAllianceState.Alliance }));
                        });
                });
		}
		else
		{
            ZLog.LogError(ZLogChannel.UI, "No action , why ChatButton is Active??");
		}
    }
	#endregion
	#endregion

	#region Inspector Linked Events 
	public void OnScrollTabValueChanged(Toggle toggle)
    {
        if (toggle.isOn)
        {
            SetSelectedSlot(IntegratedScrollTab.None, null);
            UpdateScrollTab();
        }
    }

    public void OnClickAttendStatusQueryBtn()
    {
        __NoImplement__();
    }

    public void OnClickGuildLeaveBtn()
    {
        if (ignoreClickEvent)
            return;

        ignoreClickEvent = true;

        OpenTwoButtonQueryPopUp("확인", "길드를 탈퇴/해산 하면 24시간 동안 다시 가입할 수 없습니다. 정말 탈퇴하시겠습니까?"
            , onConfirmed: () =>
            {
                // 길드 탈퇴 
                UIFrameGuildNetCapturer.ReqGuildMemberLeave(
                    Me.CurCharData.GuildId
                    , (revPacketReq, resListReq) =>
                    {
                        ignoreClickEvent = false;
                        guildController.NotifyUpdateEvent(UpdateEventType.GuildLeftAsMember);
                    },
                    (err, req, res) =>
                    {
                        ignoreClickEvent = false;
                        HandleError(err, req, res);
                    });
            }
            , onCanceled: () =>
            {
                ignoreClickEvent = false;
            });
    }

    public void OnClickGuildBenefitBtn()
    {
        guildController.OpenOverlayPopup(OverlayWindowPopUP.GuildBenefit);
    }
    public void OnClickRequestJoin()
    {
        guildController.OpenOverlayPopup(OverlayWindowPopUP.GuildJoin);
    }

    public void OnClickRequestAlliance()
    {
        guildController.OpenOverlayPopup(OverlayWindowPopUP.RequestAlly);
    }

    public void OnClickGuildManagement()
    {
        guildController.OpenOverlayPopup(OverlayWindowPopUP.GuildSetting);
    }

    public void OnClickContributeBtn()
    {
        guildController.OpenOverlayPopup(OverlayWindowPopUP.GuildDonation);
    }

    /// <summary>
    /// 동맹 길드로 추가 요청하기 
    /// </summary>
    public void OnClickRegisterAllianceGuild()
    {
		UIMessagePopup.ShowInputPopup("연맹 신청", "연맹 신청할 길드 이름을 적어주세요.", (guildName) =>
		{
            string errMsg = string.Empty;

            if (UIFrameGuildNetCapturer.ValidateGuildName(guildName, out errMsg) == false)
            {
                OpenNotiUp(errMsg, "확인");
                return;
            }
            /// 내 길드 이름과 같음 
            else if(guildName.Equals(Me.CurCharData.GuildName))
			{
                /// TODO : LOCALE
				OpenNotiUp("자신의 길드에게 연맹 요청을 할 수 없습니다.");
				return;
			}

			ZWebManager.Instance.WebGame.REQ_FindGuildInfo(guildName,
				(revPacket_findGuild, resList_findGuild) =>
				{
                    if (resList_findGuild.GuildInfo.HasValue == false || string.IsNullOrEmpty(resList_findGuild.GuildInfo.Value.MasterCharNick))
                    {
                        OpenNotiUp(DBLocale.GetText("Non_Existent_Guild_Message"), "알림");
                    }
                    else
                    {
                        ulong t_guildId = resList_findGuild.GuildInfo.Value.GuildId;
                        bool isEnemy = UIFrameGuildNetCapturer.IsEnemy(t_guildId);
                        bool isAllianceGuild = UIFrameGuildNetCapturer.IsAllianceGuild(t_guildId);
                        bool isAllianceReqSent = UIFrameGuildNetCapturer.IsAllianceGuildTargetState(t_guildId, E_GuildAllianceState.RequestAlliance);
                        bool isAllianceReqReceived = UIFrameGuildNetCapturer.IsAllianceGuildTargetState(t_guildId, E_GuildAllianceState.ReceiveAlliance);

                        /// 이미 동맹임 -> 동맹 요청 불가
                        if (isAllianceGuild)
                        {
                            OpenNotiUp(DBLocale.GetText("Already_Alliance_Guild_Error"));
                        }
                        /// 적대 길드임 -> 동맹 요청 불가 
                        else if (isEnemy)
                        {
                            OpenNotiUp(DBLocale.GetText("Already_Enemy_Guild_Error"));
						}
						/// 이미 요청을 보냈음 -> 동맹 요청 불가 
						else if (isAllianceReqSent)
						{
                            OpenNotiUp(DBLocale.GetText("Already_Alliance_Invite_Error"));
						}
						/// 상대로 부터 요청을 받았음 -> 동맹 요청 불가 
						else if (isAllianceReqReceived)
						{
                            OpenNotiUp(DBLocale.GetText("Already_Alliance_Join_Request_Error"));
						}
						else
						{
							UIFrameGuildNetCapturer.ReqGuildAllianceRequest(
								Me.CurCharData.GuildId
								, false
								, resList_findGuild.GuildInfo.Value.Name
								, (revPacketRec_reqAlliance, resListRec_reqAlliance) =>
								{
                                    OpenNotiUp(DBLocale.GetText("Alliance_Invite_Request"));

									/// 연맹 데이터 Update 요청 
									base.NotifyUpdateEvent(UpdateEventType.RequestAllianceInfo
											, new EventParam_ReqAllianceState(new E_GuildAllianceState[] { E_GuildAllianceState.RequestAlliance, E_GuildAllianceState.ReceiveAlliance }));
								});
						}
					}
				});
		});
    }

    /// <summary>
    /// [적대 길드로 등록하기] 버튼 클릭 이벤트 
    /// </summary>
    public void OnClickAddEnemyGuildBtn()
    {
        UIMessagePopup.ShowInputPopup("적대 길드 추가", DBLocale.GetText("Enemy_Guild_Name_Request"), (guildName) =>
        {
            string errMsg = string.Empty;

            if (UIFrameGuildNetCapturer.ValidateGuildName(guildName, out errMsg) == false)
            {
                OpenNotiUp(errMsg, "확인");
                return;
            }
            /// 내 길드 이름과 같음 
            else if (guildName.Equals(Me.CurCharData.GuildName))
            {
                /// TODO : LOCALE
				OpenNotiUp("자신의 길드를 적대 길드로 추가할 수 없습니다.");
                return;
            }
            
            var mainTab = guildController.RetrieveTabComponent<UIFrameGuildTab_Main>();

			if (mainTab != null)
			{
                mainTab.FindGuild(guildName
                    , (revPacket_findGuild, resList_findGuild) =>
					{
                        ulong t_guildId = resList_findGuild.GuildInfo.Value.GuildId;
						bool isEnemy = UIFrameGuildNetCapturer.IsEnemy(t_guildId);
                        bool isAllianceGuild = UIFrameGuildNetCapturer.IsAllianceGuild(t_guildId);
                        bool isAllianceReqSent = UIFrameGuildNetCapturer.IsAllianceGuildTargetState(t_guildId, E_GuildAllianceState.RequestAlliance);
                        bool isAllianceReqReceived = UIFrameGuildNetCapturer.IsAllianceGuildTargetState(t_guildId, E_GuildAllianceState.ReceiveAlliance);

                        /// 동맹 길드임 -> 적대길드 등록 불가
                        if (isAllianceGuild)
                        {
                            OpenNotiUp(DBLocale.GetText("Alliance_Guild_Error"));
                        }
                        /// 이미 적대 길드임 -> 적대길드로 등록 불가 
						else if (isEnemy)
                        {
                            OpenNotiUp(DBLocale.GetText("Enemy_Guild_Error"));
						}
                        /// 현재 적대 길드로 등록하려는 길드에게 동맹 길드로 신청된 상태 -> 적대길드로 등록 불가
						else if (isAllianceReqSent)
						{
                            /// TODO : LOCALE 
                            OpenNotiUp(DBLocale.GetText("Already_Alliance_Invite_Error02"));
						}
                        /// 현재 적대 길드로 등록하려는 길드로 부터 동맹 길드 요청을 받은 상태 -> 적대길드로 등록 불가 
						else if (isAllianceReqReceived)
						{
                            OpenNotiUp(DBLocale.GetText("Already_Alliance_Join_Request_Error02"));
						}
						/// 적대 길드 등록 진행 
						else
						{
							UIFrameGuildNetCapturer.ReqGuildAllianceRequest(
                                Me.CurCharData.GuildId
                                , true
                                , resList_findGuild.GuildInfo.Value.Name
                                , (revPacketRec_reqAlliance, resListRec_reqAlliance) =>
								{
                                    OpenNotiUp(string.Format(DBLocale.GetText("Wguid_Enemy_Notice"), resListRec_reqAlliance.GuildAllianceInfo.Value.Name));

                                    /// 연맹 데이터 Update 요청 
                                    base.NotifyUpdateEvent(UpdateEventType.RequestAllianceInfo
											, new EventParam_ReqAllianceState(new E_GuildAllianceState[] { E_GuildAllianceState.Enemy, E_GuildAllianceState.Alliance }));
								});
						}
					});
			}
		});
	}

	/// <summary>
	/// [연맹 채팅방 생성] 버튼 클릭 이벤트 
	/// </summary>
	public void OnClickCreateAllianceChatRoom()
	{
		CheckGuildCreateChatRoomResult resultType;

		if (UIFrameGuildNetCapturer.CanCreateAllianceChatRoom(out resultType) == false)
		{
			switch (resultType)
			{
				case CheckGuildCreateChatRoomResult.Denied_AlreadyJoined:
					{
						OpenNotiUp(DBLocale.GetText("Already_Alliance_Chat_Join_Error"));
					}
					break;
				case CheckGuildCreateChatRoomResult.Denied_Invited:
					{
                        /// TODO : LOCALE
                        OpenNotiUp("이미 채팅방에 초대받은 상태입니다");
                    }
                    break;
                case CheckGuildCreateChatRoomResult.Denied_Requested:
					{
                        /// TODO : LOCALE 
                        OpenNotiUp("이미 채팅방 참여 요청을 하였습니다.");
					}
                    break;
				default:
					{
                        OpenNotiUp("연맹 채팅 생성을 할 수 없습니다.");
                        ZLog.LogError(ZLogChannel.UI, "Please Add Error Handlin Type");
					}
					break;
			}
		}
		else
		{
            OpenTwoButtonQueryPopUp("확인", DBLocale.GetText("Create_Alliance_Chat_Confirm")
                , onConfirmed: () =>
                {
                    UIFrameGuildNetCapturer.ReqGuildAllianceCreateChat(Me.CurCharData.GuildId
                        , (revPacketReq, resListReq) =>
                        {
                            guildController.NotifyUpdateEvent(UpdateEventType.RequestAllianceInfo, new EventParam_ReqAllianceState(
                            new E_GuildAllianceState[] { E_GuildAllianceState.Alliance }));

                            ZWebManager.Instance.WebChat.CheckEnterChannel();
                        });
                });
		}
	}

    public void OnClickGuildChatLeave()
	{
        TryPromtLeaveChatRoom();
    }
	#endregion
}
