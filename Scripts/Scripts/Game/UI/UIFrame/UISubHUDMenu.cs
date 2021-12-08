using GameDB;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using ZNet.Data;

public class UISubHUDMenu : ZUIFrameBase
{
    public enum E_TopMenuButton
    {
        Hamburger,
        Quest,
        Bag,
        Skill,
        SpecialShop,
        Bless,
        Max,
    }

    #region UI Variable
    [SerializeField] private GameObject[] TopMenuButtons;
    [SerializeField] private GameObject[] TopMenuAlarm;
    [SerializeField] private GameObject HamburgerMenu;
    [SerializeField] private GameObject CheatIcon;
    [SerializeField] private GameObject[] HamburgerSubNew = new GameObject[ZUIConstant.HUD_MENU_HAMBURGER_SUB_MENU_COUNT];
    [SerializeField] private Image WeightIcon;
    [SerializeField] private Text Weight;

    // 축복 관련

    private const string BLESS_VFX_NONE = "Fx_Ui_HUD_ArlBlessNon";
    private const string BLESS_VFX_LEVEL = "Fx_Ui_HUD_ArlBlessStep";//1~5
    [SerializeField] private Image BlessItemIcon;
    [SerializeField] private Text BlessItemTime;
    [SerializeField] private Transform BlessVFXRoot;
    private GameObject CurBlessVFX;
    private int curBlessLevel;

    #endregion


    private void Awake()
    {
#if UNITY_EDITOR || ZCHEAT
		CheatIcon.SetActive(true);
#else
        CheatIcon.SetActive(false);
#endif

        CancelInvoke(nameof(UpdateBlessItem));
        InvokeRepeating(nameof(UpdateBlessItem), 0, 1f);
    }

    protected override void OnShow(int _LayerOrder)
    {
        base.OnShow(_LayerOrder);

        //내 캐릭터가 생성되지 않았다면 처리
        ZPawnManager.Instance.DoAddEventCreateMyEntity(HandleCreateMyEntity);

        // 게임모드에 따라 오픈할 메뉴버튼을 달리하자
        if( ZGameModeManager.Instance.CurrentGameModeType == E_GameModeType.Colosseum ) {
            for( int i = 0; i < ( int )E_TopMenuButton.Max; ++i ) {
                var curButtonType = ( E_TopMenuButton )i;
                SetActiveButton( curButtonType, curButtonType == E_TopMenuButton.Bag );
            }
        }
        else {
            SetActiveAllButtons( true );
        }
        //[구삼] 퀘스트 알람 전달 
        UIManager.Instance.Find<UIFrameQuest>().SetAlarmObject(TopMenuAlarm[(int)E_TopMenuButton.Quest]);
    }

    protected override void OnHide()
    {
        RemoveEvent();
        base.OnHide();
        HamburgerMenu.SetActive(false);
    }

    protected override void OnRemove()
    {
        RemoveEvent();
        base.OnRemove();
    }

    public void ActiveNewAlarm(E_TopMenuButton _content, bool _active)
    {
        if (TopMenuAlarm[(int)_content] != null)
            TopMenuAlarm[(int)_content].SetActive(_active);
    }

    private void ActiveAllNewAlarm(bool _active)
    {
        for (int i = 0; i < TopMenuAlarm.Length; i++)
            TopMenuAlarm[i].SetActive(_active);
    }

    /// <summary> 캐릭터 생성(이미 생성되어 있다면 바로 호출) 이벤트 </summary>
    private void HandleCreateMyEntity()
    {
        var myPc = ZPawnManager.Instance.MyEntity;

        //일단 한번 갱신하고
        RefreshWeight();

        // 축복 갱신
        UpdateBlessItem();

        //이벤트 등록
        myPc.DoAddEventWeightUpdated(RefreshWeight);
    }
    
    /// <summary> ui가 사라질 때 이벤트 제거 </summary>
    private void RemoveEvent()
    {
        ZPawnManager.Instance.DoRemoveEventCreateMyEntity(HandleCreateMyEntity);

        var myPc = ZPawnManager.Instance.MyEntity;
        if (null != myPc)
        {
            myPc.DoRemoveEventWeightUpdated(RefreshWeight);
        }
    }

    /// <summary> 무게 갱신 </summary>
    private void RefreshWeight()
    {
        var myPc = ZPawnManager.Instance.MyEntity;
        float weight = myPc.GetAbility(E_AbilityType.ITEM_WEIGH) * 100f / myPc.GetAbility(E_AbilityType.FINAL_MAX_WEIGH);
        Weight.text = weight.ToString("F2") + "%";
        WeightIcon.fillAmount = weight / 100.0f;
        WeightIcon.color = weight >= 100f ? WeightIcon.color = new Color(255 / 255f, 0, 0) : new Color(255 / 255f, 255 / 255f, 255 / 255f);
    }

    public void CheckSlotWeight()
	{
        if(!IsInvoking(nameof(UpdateWarnMessageWeight)))
            InvokeRepeating(nameof(UpdateWarnMessageWeight), 0f, 3f);

        if(Me.CurCharData.InvenMaxCnt <= Me.CurCharData.GetShowInvenItems().Count)
		{
            UICommon.SetNoticeMessage(DBLocale.GetText("Item_Inven_Full"), new Color(255, 0, 116), 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);
            setAutoTimer();
            return;
        }

        void setAutoTimer()
		{
            if(!IsInvoking(nameof(CheckSlotWeight)))
                Invoke(nameof(CheckSlotWeight), 5.0f);
        }
    }

    /// <summary> 무게/인벤 패널티 관련 메시지 처리 시간 </summary>
    //float WeightWarnMessageLastTime = 0;

    /// <summary>
    /// [박윤성] 옵션창 무게알림 관련 메세지
    /// </summary>
    public void UpdateWarnMessageWeight()
    {
        if (ZGameOption.Instance.AlramWeight >= 0.5f)
        {
            var myPc = ZPawnManager.Instance.MyEntity;

            if (myPc == null)
                return;

            float weightRate = myPc.GetAbility(E_AbilityType.ITEM_WEIGH) / myPc.GetAbility(E_AbilityType.FINAL_MAX_WEIGH);
            float weightPercent = (float)System.Math.Truncate(weightRate * 1000f) / 10f;
            bool isPanelty = DBWeightPenalty.TryGetPaneltyData(weightPercent, out var weightTableData);

            if(isPanelty)
            {
                if (weightRate >= ZGameOption.Instance.AlramWeight)
                {
                    //WeightWarnMessageLastTime = Time.time;

                    //로케일로 변경해야됨
                    string noticeStr = string.Format(DBLocale.GetText("Alert_Warning_Now_Weight"), weightPercent);
                    UICommon.SetNoticeMessage(noticeStr, Color.red, 2f, UIMessageNoticeEnum.E_MessageType.BackNotice);
                }
            }

        }
    }

    public void ActiveRedDot(E_HUDMenu _type, bool _active)
    {
        HamburgerSubNew[(int)_type].SetActive(_active);
    }

    public void ShowBless()
    {
        UIManager.Instance.Open<UIFrameBlessBuff>();
    }

    public void UpdateBlessItem()
    {
        if (ZPawnManager.Instance.MyEntity == null)
            return;
		//var blessbuffData = Me.CurCharData.GetBuffData(DBConfig.GodBless_AbilityActionID);
		var blessbuffData = ZPawnManager.Instance.MyEntity.GetGodBuffAbilityAction();

		if (blessbuffData != null)
        {
            // 현재 UI 쪽 미완성이라 추후 작업할것.
            // TODO : 축복 아이템 이미지(오브젝트) 활성화.
            // TODO : 축복 부족(없음) 이미지(오브젝트) 비활성화.

            var blessremainTime = (blessbuffData.EndServerTime - TimeManager.NowSec);

            var tabledata = DBAbility.GetAction(DBConfig.GodBless_AbilityActionID);

            float stackCount = (float)(blessbuffData.EndServerTime - TimeManager.NowSec) / (float)tabledata.MinSupportTime;
            uint blessCnt = (uint)Mathf.CeilToInt(stackCount);

            int blessLevel = DBGodBuff.GetBuffLevel(E_GodBuffType.GodBless,blessCnt);// 임시값

            BlessItemTime.text = blessCnt.ToString();

            if (blessremainTime <= DBConfig.ArelAlert_Time_Check)
            {
                // TODO : 축복시간이 끝나갈때(10초 남았을시) UI 처리.
            }
            else
            {
                // 이펙트 처리
                if (curBlessLevel < blessLevel)
                {
                    SwapBlessVFX(blessLevel);
                }
                // TODO : 축복시간 일때 UI 처리.
            }

            curBlessLevel = blessLevel;
        }
        else
        {
            // TODO : 축복시간이 끝났을때 UI 처리.
            BlessItemTime.text = "";
            if (curBlessLevel > -1)
            {
                curBlessLevel = -1;
                SwapBlessVFX(-1);
            }

        }

    }

    private void SwapBlessVFX(int level)
    {
        if (CurBlessVFX != null)
            Addressables.ReleaseInstance(CurBlessVFX);

        string vfxName = BLESS_VFX_NONE;

        if (level > 0)
        {
            vfxName = $"{BLESS_VFX_LEVEL}{Mathf.Clamp(level, 1, 5)}";
        }

        Addressables.InstantiateAsync(vfxName).Completed += (obj) =>
        {
            if (CurBlessVFX != null)
                Addressables.ReleaseInstance(CurBlessVFX);

            if (obj.Status != UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                return;

            CurBlessVFX = obj.Result;
            CurBlessVFX.transform.SetParent(BlessVFXRoot);
            CurBlessVFX.transform.SetLocalTRS(Vector3.zero, Quaternion.identity, Vector3.one);

            CurBlessVFX.SetLayersRecursively("UI");
        };
    }

    public void ShowSpecialShop()
    {
        if (UIManager.Instance.Find<UIFrameInventory>() != null)
            UIManager.Instance.Close<UIFrameInventory>();

        if (UIManager.Instance.Find<UIFrameSkill>() != null)
            UIManager.Instance.Close<UIFrameSkill>();

        UIManager.Instance.Open<UIFrameSpecialShop>();
    }

    public void CloseHamburgerMenu()
	{
        if (HamburgerMenu.activeSelf)
            HamburgerMenu.SetActive(false);
    }

    public void ShowSkill()
    {
        if (HamburgerMenu.activeSelf)
            HamburgerMenu.SetActive(false);

        UIFrameSkill skill = UIManager.Instance.Find<UIFrameSkill>();

        if (skill == null)
        {
            if (UIManager.Instance.Find<UIFrameInventory>() != null)
                UIManager.Instance.Close<UIFrameInventory>();

            UIManager.Instance.Open<UIFrameSkill>();
        }
        else
		{
            if (!skill.Show)
            {
                UIManager.Instance.Open<UIFrameSkill>();

                if (HamburgerMenu.activeSelf)
                    HamburgerMenu.SetActive(false);

                if (UIManager.Instance.Find<UIFrameInventory>() != null)
                    UIManager.Instance.Close<UIFrameInventory>();
            }
            else
                UIManager.Instance.Close<UIFrameSkill>();
        }
    }

    public void ShowBag()
    {
        if (HamburgerMenu.activeSelf)
            HamburgerMenu.SetActive(false);

        UIFrameInventory inventory = UIManager.Instance.Find<UIFrameInventory>();

        if (inventory == null)
        {
            if (UIManager.Instance.Find<UIFrameSkill>() != null)
                UIManager.Instance.Close<UIFrameSkill>();

            UIManager.Instance.Load(nameof(UIFrameInventory), (_loadName, _loadFrame) => {
                UIManager.Instance.Open<UIFrameInventory>((_name, _frame) => {
                    //_frame.Init();
                });
            });
        }
        else
		{
            if (!inventory.Show)
            {
                UIManager.Instance.Open<UIFrameInventory>();

                if (HamburgerMenu.activeSelf)
                    HamburgerMenu.SetActive(false);

                if (UIManager.Instance.Find<UIFrameSkill>() != null)
                    UIManager.Instance.Close<UIFrameSkill>();
            }
            else
                UIManager.Instance.Close<UIFrameInventory>();
        }
    }

    public void ShowQuest()
    {
        UIManager.Instance.Open<UIFrameQuest>((_name, _frameQuest) => { _frameQuest.DoUIQuestPanel(); });
    }

    public void ShowHamburgerMenu()
    {
        HamburgerMenu.SetActive(!HamburgerMenu.activeSelf);

        if (UIManager.Instance.Find<UIFrameInventory>() != null)
            UIManager.Instance.Close<UIFrameInventory>();

        if (UIManager.Instance.Find<UIFrameSkill>() != null)
            UIManager.Instance.Close<UIFrameSkill>();

        CheckGodLandRedDot(false);
    }

    private void SetActiveButton( E_TopMenuButton buttonType, bool bActive )
    {
        TopMenuButtons[ ( int )buttonType ].SetActive( bActive );
    }

    private void SetActiveAllButtons( bool bActive )
    {
        for( int i = 0; i < ( int )E_TopMenuButton.Max; ++i ) {
            TopMenuButtons[ i ].SetActive( bActive );
        }
    }

    #region Hamburger Menu
    public void ShowMailbox()
    {
        HamburgerSubNew[(int)E_HUDMenu.Mailbox].SetActive(false);
        UIManager.Instance.Open<UIFrameMailbox>();
    }

    public void ShowFriend()
    {
        if (UIManager.Instance.Find(out UIFrameHUD _hud))
            _hud.SetSubHudFrame(E_UIStyle.Normal);
        UIManager.Instance.Open<UIFrameFriend>();
    }

    public void ShowCraft()
    {
        if (UIManager.Instance.Find(out UIFrameHUD _hud))
            _hud.SetSubHudFrame(E_UIStyle.Normal);
        UIManager.Instance.Open<UIFrameItemMake>();
    }

    public void ShowDungeon()
    {
        UIManager.Instance.Open<UIFrameDungeon>();
    }

    public void ShowGem()
    {
        UIManager.Instance.Open<UIFrameItemGem>();
    }

    public void ShowArena()
    {
        if (Me.CurCharData.LastLevel < DBConfig.WPvP_Duel_Level)
        {
            UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
            {
                _popup.Open(ZUIString.WARRING, string.Format( DBLocale.GetText("Instance_Dungeon_Close_Level") , DBConfig.WPvP_Duel_Level),
                    new string[] { ZUIString.LOCALE_OK_BUTTON },
                    new Action[] { delegate { _popup.Close(); } });
            });
            return;
        }
        else
        {
            UIManager.Instance.Open<UIFrameColosseum>();
        }
    }

    public void ShowPoint()
    {
        UIManager.Instance.Open<UIFrameMileage>();
    }

    public void ShowAttendance()
    {
        //UICommon.SetNoticeMessage(DBLocale.GetText("WTrade_Close"), new Color(255, 255, 255), 1f, E_MessageType.SimpleNotice);

        // Test
        UIManager.Instance.Open<UIFrameGacha>();
        if (UIManager.Instance.Find(out UIFrameGacha _gacha))
		{
            _gacha.StartGacha();
        }
    }

    public void ShowGodLand()
	{
        UIManager.Instance.Open<UIFrameGodLandMap>();
    }

    public void CheckGodLandRedDot(bool fromGameState)
    {
        // 성지 생산아이템이 가득찼거나 전투기록 변동이 있을때 레드닷 On
        if (fromGameState) {
            Me.CurCharData.GodLandContainer.REQ_GetSelfGodLandInfo((isHave, myData) => {
                bool bFull = Me.CurCharData.GodLandContainer.IsMyGatheringFulled(myData);
                if (bFull) {
                    ActiveRedDot(E_HUDMenu.GodLand, true);
                    return;
                }

                Me.CurCharData.GodLandContainer.REQ_GetGodLandFightRecord(false, (list) => {
                    bool bChange = Me.CurCharData.GodLandContainer.IsBattleRecordChanged(list);
                    ActiveRedDot(E_HUDMenu.GodLand, bChange);
                });
            });
        }
        else {
            var frame = UIManager.Instance.Find<UIFrameGodLandMap>();
            if (frame != null) {
                ActiveRedDot(E_HUDMenu.GodLand, frame.CheckRedDot());
            }
        }
    }

    public void CheckGuildRedDot()
	{
        /// 길드 레드닷 세팅 
        ActiveRedDot(E_HUDMenu.Guild, false);

        /// 길드 레드닷은 길마/부길마 아니면 출력 X 
        if (Me.CurCharData.GuildGrade == WebNet.E_GuildMemberGrade.Master || Me.CurCharData.GuildGrade == WebNet.E_GuildMemberGrade.SubMaster)
        {
            /// 연맹 채팅 요청을 받은경우 
            if (Me.CurCharData.GuildChatState == WebNet.E_GuildAllianceChatState.Receive)
            {
                ActiveRedDot(E_HUDMenu.Guild, true);
            }
            else
            {
                /// 가입 신청건 체킹 
                ZWebManager.Instance.WebGame.REQ_GuildRequestListForGuild(Me.CurCharData.GuildId, (revPacket, resList) =>
                {
                    /// 가입 신청건이 없으면 , 연맹 체킹 
                    if (resList.GuildRequestInfosLength == 0)
                    {
                        ZWebManager.Instance.WebGame.REQ_GetGuildAllianceList(Me.CurCharData.GuildId
                            , new WebNet.E_GuildAllianceState[]
                            {
										/// 나에게 연맹 요청을 보낸 길드만 찾음 
										WebNet.E_GuildAllianceState.ReceiveAlliance
                            }
                            , (revPacket_02, resList_02) =>
                            {
                                if (resList_02.GuildAllianceInfosLength > 0)
                                {
                                    ActiveRedDot(E_HUDMenu.Guild, true);
                                }
                            });
                    }
                    else
                    {
                        ActiveRedDot(E_HUDMenu.Guild, true);
                    }
                });
            }
        }
    }

    public void ShowRanking()
    {
        if (UIManager.Instance.Find(out UIFrameHUD _hud))
            _hud.SetSubHudFrame(E_UIStyle.FullScreen);
        UIManager.Instance.Open<UIFrameRank>();
    }

    public void ShowArtifact()
    {
        if (Me.CurUserData.MaxLevel < DBConfig.Artifact_Use_Level)
        {
            UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
            {
                _popup.Open(ZUIString.WARRING, "아티팩트를 이용하려면 캐릭터의 레벨이 " + DBConfig.Artifact_Use_Level  + "이상 되어야합니다.",
                    new string[] { ZUIString.LOCALE_OK_BUTTON },
                    new Action[] { delegate { _popup.Close(); } });
            });
            return;
        }
        else
        {
            if (UIFrameArtifact.Temp_IsOpening == false)
            {
                UIFrameArtifact.Temp_IsOpening = true;

                if (UIManager.Instance.Find<UIFrameArtifact>() == null)
                {
                    UIFrameArtifact.TryShortCutIfLastTabCaptured(true);
                }

                UIManager.Instance.Open<UIFrameArtifact>();
            }        
        }
    }

    public void ShowCollection()
    {
        if (UIManager.Instance.Find(out UIFrameHUD _hud))
            _hud.SetSubHudFrame(E_UIStyle.FullScreen);
        UIManager.Instance.Open<UIFrameItemCollection>();
    }

    public void ShowChange()
    {
        HamburgerSubNew[(int)E_HUDMenu.Change].SetActive(false);
        HamburgerMenu.SetActive(false);
        UIManager.Instance.Open<UIFrameChange>();
    }

    // ------- 삭제될 녀석들, NPC와 상호작용으로 열림, 테스트용 --------
    public void ShowStorage()
    {
        HamburgerMenu.SetActive(false);
        UIFrameStorage storage = UIManager.Instance.Find<UIFrameStorage>();

        if (storage == null)
        {
            UIManager.Instance.Load<UIFrameStorage>(nameof(UIFrameStorage), (_loadName, _loadFrame) => {
                _loadFrame.Init(() => UIManager.Instance.Open<UIFrameStorage>());
            });
        }
        else
        {
            if (!storage.Show)
            {
                UIManager.Instance.Open<UIFrameStorage>();

            }
            else
                UIManager.Instance.Close<UIFrameStorage>();
        }
    }

    public void ShowItemShop()
    {
        HamburgerMenu.SetActive(false);
        UIFrameItemShop shop = UIManager.Instance.Find<UIFrameItemShop>();

        UIFrameItemShop.E_ShopFrameType shopType = UIFrameItemShop.E_ShopFrameType.Item;

        if (shop == null)
        {
            UIManager.Instance.Load<UIFrameItemShop>(nameof(UIFrameItemShop), (_loadName, _loadFrame) => {
                _loadFrame.Init(() => UIManager.Instance.Open<UIFrameItemShop>((str, frame) =>
                {
                    frame.SetShopType(shopType);
                }));
            });
        }
        else
        {
            if (!shop.Show)
            {
                UIManager.Instance.Open<UIFrameItemShop>((str, frame) => frame.SetShopType(shopType));

            }
            else
                UIManager.Instance.Close<UIFrameItemShop>();
        }
    }

    public void ShowSkillBookShop()
    {
        HamburgerMenu.SetActive(false);
        UIFrameItemShop shop = UIManager.Instance.Find<UIFrameItemShop>();

        UIFrameItemShop.E_ShopFrameType shopType = UIFrameItemShop.E_ShopFrameType.Skill;

        if (shop == null)
        {
            UIManager.Instance.Load<UIFrameItemShop>(nameof(UIFrameItemShop), (_loadName, _loadFrame) => {
                _loadFrame.Init(() => UIManager.Instance.Open<UIFrameItemShop>((str, frame) =>
                {
                    frame.SetShopType(shopType);
                }));
            });
        }
        else
        {
            if (!shop.Show)
            {
                UIManager.Instance.Open<UIFrameItemShop>((str, frame) => frame.SetShopType(shopType));

            }
            else
                UIManager.Instance.Close<UIFrameItemShop>();
        }
    }
    //------------------------------------------------------------
    public void ShowPet()
    {
        HamburgerSubNew[(int)E_HUDMenu.Pet].SetActive(false);
        HamburgerMenu.SetActive(false);
        UIManager.Instance.Open<UIFramePet>();
    }

    public void ShowRide()
    {
        HamburgerSubNew[(int)E_HUDMenu.Ride].SetActive(false);
        HamburgerMenu.SetActive(false);
        UIManager.Instance.Open<UIFrameRide>();
    }

    public void ShowTemple()
    {
        HamburgerMenu.SetActive(false);
        UIManager.Instance.Open<UIFrameTemple>();

    }

    public void ShowCheat()
    {
#if UNITY_EDITOR || ZCHEAT
        UIManager.Instance.Open<UICheatPopup>();
#endif
    }

    public void ShowElemental()
    {
        if (Me.CurUserData.MaxLevel < DBConfig.Attribute_Require_Level)
        {
            UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
            {
                _popup.Open(ZUIString.WARRING, "속성 강화를 이용하려면 캐릭터의 레벨이 " + DBConfig.Attribute_Require_Level + "이상 되어야합니다.",
                    new string[] { ZUIString.LOCALE_OK_BUTTON },
                    new Action[] { delegate { _popup.Close(); } });
            });
            return;
        }
        else
        {
            /// TODO : 추후에 수정해야함. 프레임 중복 생성해 발생하는 이슈 임시 수정 .
            if (UIEnhanceElement.Temp_IsOpening == false)
            {
                UIEnhanceElement.Temp_IsOpening = true;
                UIManager.Instance.Open<UIEnhanceElement>();
            }
        }
    }

    public void ShowGuild()
    {
        ZLog.Log(ZLogChannel.UI, "Show Guild Button Clicked");

        UIManager.Instance.Open<UIFrameGuild>();
    }

    public void ShowOption()
    {
        UIManager.Instance.Open<UIFrameOption>();

        // UICommon.SetNoticeMessage("준비중입니다.", new Color(255, 0, 116), 1f, E_MessageType.SimpleNotice);
        //if (UICommon.noticeOption == E_NoticeOption.TypeA)
        //{
        //    UICommon.noticeOption = E_NoticeOption.TypeB;
        //}
        //else
        //    UICommon.noticeOption = E_NoticeOption.TypeA;

        //UICommon.FadeInOut(delegate
        //{

        //}, E_UIFadeType.FadeIn, 5.0f);
    }

    public void ShowTrade()
	{
        //if (UIManager.Instance.Find(out UIFrameHUD _hud)) 
        //    _hud.HideAllContentFrame();

        if(Me.CurUserData.MaxLevel < DBConfig.Exchange_OpenLv)
		{
            UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
            {
                _popup.Open(ZUIString.WARRING, "거래소를 이용하려면 캐릭터의 레벨이 " + DBConfig.Exchange_OpenLv + "이상 되어야합니다.",
                    new string[] { ZUIString.LOCALE_OK_BUTTON },
                    new Action[] { delegate { _popup.Close(); } });
            });
            return;
        }
            
        if (UIManager.Instance.Find(out UIFrameHUD _hud)) 
            _hud.SetSubHudFrame(E_UIStyle.FullScreen);
        UIManager.Instance.Open<UIFrameTrade>();
    }

    public void ShowMark()
	{
        if (Me.CurUserData.MaxLevel < DBConfig.Mark_Use_Level)
        {
            UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
            {
                _popup.Open(ZUIString.WARRING, "문장을 이용하려면 캐릭터의 레벨이 " + DBConfig.Mark_Use_Level + "이상 되어야합니다.",
                    new string[] { ZUIString.LOCALE_OK_BUTTON },
                    new Action[] { delegate { _popup.Close(); } });
            });

            return;
        }

        UIManager.Instance.Open<UIFrameMark>();
    }

    public void ShowCook()
	{
        if (Me.CurUserData.MaxLevel < DBConfig.Cooking_Require_Level)
        {
            UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
            {
                _popup.Open(ZUIString.WARRING, "요리를 진행하려면 캐릭터의 레벨이 " + DBConfig.Cooking_Require_Level + "이상 되어야합니다.",
                    new string[] { ZUIString.LOCALE_OK_BUTTON },
                    new Action[] { delegate { _popup.Close(); } });
            });
            return;
        }

        if (UIManager.Instance.Find(out UIFrameHUD _hud))
            _hud.SetSubHudFrame(E_UIStyle.FullScreen);
        UIManager.Instance.Open<UIFrameCook>();
    }

    public void Logout()
    {
		UICommon.OpenPopup_GoCharacterSelectState();
	}
    #endregion
}