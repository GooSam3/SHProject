using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using ZNet.Data;

public class UIFrameGuildPopup_GuildSetting : UIFrameGuildOverlayPopupBase
{
    public enum AutoKickOutTimeOption
    {
        Disable = 0, // 기능사용안함은 0 임 
        Option01 = 1, // 현재 기준 5 일 
        Option02 = 2, // 7
        Option03 = 3, // 15 
    }

    [System.Serializable]
    public class AutoKickOutToggle
    {
        public AutoKickOutTimeOption option;
        public ZToggle toggle;
    }

    //[Serializable]
    //public class AutoKickOutTab
    //{
    //    public AutoKickOutTimeOption option;
    //    public Toggle toggle;
    //}

    [Serializable]
    public class InputFieldPop
    {
        public Text txtCount;
        public InputField inputField;
    }

    #region SerializedField
    #region Preference Variable
    #endregion

    #region UI Variables
    [SerializeField] private ScrollGuildSettingGridAdapter ScrollAdapter;
    [SerializeField] private InputFieldPop inputField_intro;
    [SerializeField] private InputFieldPop inputField_notice;

    // 길드 세팅창은 길마/부길마만 볼 창임. 일반 플레이어는 해당사항없음 
    [SerializeField] private List<GameObject> activeOnMaster;
    [SerializeField] private List<GameObject> activeOnSubmaster;

    [SerializeField] private ZToggle quickOrApprove_quickToggle;
    [SerializeField] private ZToggle quickOrApprove_notQuickToggle;

    [Header("서버로 보내는 Enum 순서와 일치하게 세팅해야함. Config 참고 , 0 : Disable")]
    [SerializeField] private List<AutoKickOutToggle> autoKickOutAbsenceToggles;
    [SerializeField] private List<AutoKickOutToggle> autoKickOutContibuteToggles;

    [SerializeField] private Text txtGuildName;

    [SerializeField] private ZButton btnSave;

    //   [SerializeField] private List<AutoKickOutTab> autoKickOutTab_continuousAbsence;
    //  [SerializeField] private List<AutoKickOutTab> autoKickOutTab_contributeDenied;
    #endregion
    #endregion

    #region System Variables
    private byte selectedMarkTid;
    private bool ignoreClickEvents;
    #endregion

    #region Properties 
    #endregion

    #region Unity Methods
    #endregion

    #region Overrides 
    public override void Initialize(UIFrameGuild guildController)
    {
        base.Initialize(guildController);

        inputField_intro.inputField.characterLimit = (int)DBConfig.GuildIntroduce_Length_Max;
        inputField_notice.inputField.characterLimit = (int)DBConfig.GuildNotice_Length_Max;

        UpdateInputFieldTextCount();

        var slot = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(ScrollGuildSettingGridSlot));
        ScrollAdapter.Parameters.Grid.CellPrefab = slot.GetComponent<RectTransform>();
        var pf = ScrollAdapter.Parameters.Grid.CellPrefab;
        pf.SetParent(transform);
        pf.localScale = Vector2.one;
        pf.localPosition = Vector3.zero;
        pf.gameObject.SetActive(false);

        ScrollAdapter.onClickedSlot = OnClickedMarkSlot;
        ScrollAdapter.Initialize();
    }

    public override void Open()
    {
        base.Open();

        ignoreClickEvents = false;

        // 마크 세팅 
        var myMark = Me.CurCharData.GuildMarkTid;

        if (string.IsNullOrEmpty(DBGuild.GetGuildMark(myMark)))
        {
            myMark = DBGuild.GetGuildMarkFirstData().GuildMarkID;
        }

        selectedMarkTid = myMark;

        txtGuildName.text = UIFrameGuildNetCapturer.MyGuildData.myGuildInfo.guildName;

        inputField_intro.inputField.text = UIFrameGuildNetCapturer.MyGuildData.myGuildInfo.introduction;
        inputField_notice.inputField.text = UIFrameGuildNetCapturer.MyGuildData.myGuildInfo.notice;

        quickOrApprove_quickToggle.isOn = UIFrameGuildNetCapturer.MyGuildData.myGuildInfo.isQuickJoin;
        quickOrApprove_quickToggle.GraphicUpdateComplete();
        //quickOrApprove_notQuickToggle.isOn = UIFrameGuildNetCapturer.MyGuildData.myGuildInfo.isQuickJoin == false;

        activeOnMaster.ForEach(t => t.SetActive(false));
        activeOnSubmaster.ForEach(t => t.SetActive(false));

        if (Me.CurCharData.GuildGrade == WebNet.E_GuildMemberGrade.Master)
        {
            activeOnMaster.ForEach(t => t.SetActive(Me.CurCharData.GuildGrade == WebNet.E_GuildMemberGrade.Master));
        }
        else if (Me.CurCharData.GuildGrade == WebNet.E_GuildMemberGrade.SubMaster)
        {
            activeOnSubmaster.ForEach(t => t.SetActive(Me.CurCharData.GuildGrade == WebNet.E_GuildMemberGrade.SubMaster));
        }

        ushort loginBanStep = UIFrameGuildNetCapturer.MyGuildData.myGuildInfo.loginBanStep;
        ushort donateBanStep = UIFrameGuildNetCapturer.MyGuildData.myGuildInfo.donateBanStep;

        /// 자동 추방 토글 옵션 가져와서 세팅 
        AutoKickOutTimeOption absenceBanOption = (AutoKickOutTimeOption)loginBanStep;
        AutoKickOutTimeOption contributeBanOption = (AutoKickOutTimeOption)donateBanStep;

        /// 혹시 모를 예외처리 . 서버에서 항상 이 enum 타입 값에 맞게 준다는 보장이 없음 
        try
        {
            absenceBanOption = (AutoKickOutTimeOption)loginBanStep;
        }
        catch (Exception exp)
        {
            absenceBanOption = AutoKickOutTimeOption.Disable;
        }

        try
        {
            contributeBanOption = (AutoKickOutTimeOption)donateBanStep;
        }
        catch (Exception exp)
        {
            contributeBanOption = AutoKickOutTimeOption.Disable;
        }

        autoKickOutAbsenceToggles.Find(t => t.option == absenceBanOption).toggle.SelectToggle(); // isOn = true;
        autoKickOutContibuteToggles.Find(t => t.option == contributeBanOption).toggle.SelectToggle(); // isOn = true;

        UpdateInputFieldTextCount();

        UpdateSaveBtnInteractable();

        RefreshScroll();
        ScrollAdapter.SetNormalizedPosition(1);
    }

    public override void Close()
    {
        quickOrApprove_quickToggle.isOn = UIFrameGuildNetCapturer.MyGuildData.myGuildInfo.isQuickJoin;
        quickOrApprove_quickToggle.GraphicUpdateComplete();
        //quickOrApprove_notQuickToggle.isOn = UIFrameGuildNetCapturer.MyGuildData.myGuildInfo.isQuickJoin == false;

        base.Close();
    }
    #endregion

    #region Public Methods
    #endregion

    #region Private Methods
    void RefreshScroll()
    {
        ScrollAdapter.SetSelectedMarkID(selectedMarkTid, false);
        ScrollAdapter.RefreshData();
    }

    private void UpdateInputFieldTextCount()
    {
        inputField_intro.txtCount.text = string.Format("{0}/{1}", inputField_intro.inputField.text.Length, DBConfig.GuildIntroduce_Length_Max);
        inputField_notice.txtCount.text = string.Format("{0}/{1}", inputField_notice.inputField.text.Length, DBConfig.GuildNotice_Length_Max);
    }

    private void UpdateSaveBtnInteractable()
    {
        btnSave.interactable = IsMainSettingDirty();
    }

    private void OnClickedMarkSlot(ScrollGuildSettingGridHolder holder)
    {
        selectedMarkTid = holder.Data.tid;
        ScrollAdapter.SetSelectedMarkID(selectedMarkTid, true);
        UpdateSaveBtnInteractable();
    }

    private bool IsMainSettingDirty()
    {
        var guildInfo = UIFrameGuildNetCapturer.MyGuildData.myGuildInfo;

        var onAutoKickLoginBanStepToggle = autoKickOutAbsenceToggles.Find(t => t.toggle.isOn);
        var onAutoKickBanDonateToggle = autoKickOutContibuteToggles.Find(t => t.toggle.isOn);

        ushort compareLoginBanStep = onAutoKickLoginBanStepToggle == null ? ushort.MaxValue : (ushort)onAutoKickLoginBanStepToggle.option;
        ushort compareBanDonate = onAutoKickBanDonateToggle == null ? ushort.MaxValue : (ushort)onAutoKickBanDonateToggle.option;

        return guildInfo.markTid != selectedMarkTid ||
            guildInfo.introduction.Equals(inputField_intro.inputField.text) == false ||
            guildInfo.notice.Equals(inputField_notice.inputField.text) == false ||
            guildInfo.isQuickJoin != quickOrApprove_quickToggle.isOn ||
            guildInfo.loginBanStep != compareLoginBanStep ||
            guildInfo.donateBanStep != compareBanDonate;
    }

    #endregion

    #region OnClick Event / Etc events (인스펙터 연결)
    public void OnIntroOrNoticeInputFieldChanged()
    {
        int noticeSpaceCount = inputField_notice.inputField.text.Count(
            (c) =>
            {
                return c == '\n';
            });

        if (noticeSpaceCount > 2)
        {
            inputField_notice.inputField.text = inputField_notice.inputField.text.TrimEnd('\n');
        }

        UpdateInputFieldTextCount();
        UpdateSaveBtnInteractable();
    }

    public void OnJoinSettingToggleChanged()
    {
        UpdateSaveBtnInteractable();
    }

    public void OnClickAutoBanAbsenceToggleValueChanged()
    {
        UpdateSaveBtnInteractable();
    }

    public void OnClickAutoBanRejectContibuteToggleValueChanged()
    {
        UpdateSaveBtnInteractable();
    }

    //public void OnUpdateToggle_JoinConfig(bool quickOrApprove)
    //{

    //}

    //public void OnUpdateToggle_AutoKickOut_ContinuousAbsence(int optionIndex)
    //{
    //    autoKickOutOption_continuousAbcense = (AutoKickOutTimeOption)optionIndex;
    //}

    //public void OnUpdateToggle_AutoKickOut_ContributeDenied(int optionIndex)
    //{
    //    autoKickOutOption_continuousAbcense = (AutoKickOutTimeOption)optionIndex;
    //}

    public void OnClickDismissGuildBtn()
    {
        if (ignoreClickEvents)
            return;

        ignoreClickEvents = true;

#if _GTEST_
        GuildController.NotifyUpdateEvent(UpdateEventType.GuildDismissed);
#else

        UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
        {
            // TODO : Locale
            _popup.Open("확인", "정말 길드를 해체하시겠습니까?", new string[] { "취소", "확인" },
                new Action[] {
                            () =>
                            {
                                ignoreClickEvents = false;
                                _popup.Close();
                            }
                            ,
                            () =>
                            {
                                UIFrameGuildNetCapturer.ReqDismissGuild(
                                    Me.CurCharData.GuildId
                                    , Me.CurCharData.GuildChatId
                                    , Me.CurCharData.GuildChatGrade
                                    , (revPacketReq, resListReq) =>
                                    {
                                        ignoreClickEvents = false;
                                        GuildController.NotifyUpdateEvent(UpdateEventType.GuildDismissed);
                                    },
                                    (error, req, res) =>
                                    {
                                        ignoreClickEvents=false;
                                        UIFrameGuildTabBase.HandleError(error,req,res);
                                    });

                                _popup.Close();
                            } });
        });
#endif
    }

    public void OnClickSaveBtn()
    {
        if (ignoreClickEvents)
            return;

        ulong secGuildMarkResetRemainedTime;
        uint guildLevel = DBGuild.GetLevel(Me.CurCharData.GuildExp);
        bool changeGuildMark = Me.CurCharData.GuildMarkTid != selectedMarkTid;

        if (changeGuildMark)
        {
            bool resetScroll = false;

            if (UIFrameGuildNetCapturer.CanChangeGuildMark(out secGuildMarkResetRemainedTime) == false)
            {
                resetScroll = true;
                /// TODO : LOCALE 
                UIFrameGuildTabBase.OpenNotiUp(string.Format("길드 마크 변경 대기시간 {0} ", TimeSpan.FromSeconds(secGuildMarkResetRemainedTime)), title: "알림");
            }
            else if (DBGuild.GetGuildMarkOpenLevel(selectedMarkTid) > guildLevel)
            {
                resetScroll = true;
                /// TODO: LOCAL 
                UIFrameGuildTabBase.OpenNotiUp("길드 마크를 다시 선택해주세요.");
            }

            if (resetScroll)
            {
                selectedMarkTid = Me.CurCharData.GuildMarkTid;
                RefreshScroll();
                UpdateSaveBtnInteractable();
                return;
            }
        }

        bool isQuickOrApprove = quickOrApprove_quickToggle.isOn;
        AutoKickOutTimeOption autoKickOut_absence = AutoKickOutTimeOption.Disable;
        AutoKickOutTimeOption autoKickOut_contibute = AutoKickOutTimeOption.Disable;

        var absenceToggle = autoKickOutAbsenceToggles.Find(t => t.toggle.isOn);
        var contributeToggle = autoKickOutContibuteToggles.Find(t => t.toggle.isOn);

        if (absenceToggle != null)
        {
            autoKickOut_absence = absenceToggle.option;
        }
        else
        {
            ZLog.LogError(ZLogChannel.UI, "toggle not found");
        }

        if (contributeToggle != null)
        {
            autoKickOut_contibute = contributeToggle.option;
        }
        else
        {
            ZLog.LogError(ZLogChannel.UI, "toggle not found");
        }

        bool operation01Done = false;
        bool operation02Done = false;
        bool nothingChanged = true;

        bool isMainDirty = IsMainSettingDirty();

        if (isMainDirty)
        {
            string intro = inputField_intro.inputField.text;
            string notice = inputField_notice.inputField.text;

            if (string.IsNullOrEmpty(intro) || string.IsNullOrEmpty(notice))
            {
                UIFrameGuildTabBase.OpenNotiUp("소개 또는 공지사항이 비어있습니다.");
                return;
            }
        }

        ignoreClickEvents = true;

        if (changeGuildMark)
        {
            nothingChanged = false;

            UIFrameGuildNetCapturer.ReqUpdateGuildMark(
                Me.CurCharData.GuildId
                , selectedMarkTid
                , (revPacketRec, resListRec) =>
                {
                    operation01Done = true;

                    if (operation02Done)
                    {
                        ignoreClickEvents = false;
                        GuildController.NotifyUpdateEvent(UpdateEventType.DataRefreshed_GuildInfo);
                        GuildController.CloseOverlayPopup(OverlayWindowPopUP.GuildSetting);
                    }
                },
                (error, req, res) =>
                {
                    operation01Done = true;
                    ignoreClickEvents = false;
                    UIFrameGuildTabBase.HandleError(error, req, res);
                });
        }
        else
        {
            operation01Done = true;
        }

        if (isMainDirty)
        {
            nothingChanged = false;

            UIFrameGuildNetCapturer.ReqUpdateGuildInfo(
                Me.CurCharData.GuildId
                , inputField_intro.inputField.text
                , inputField_notice.inputField.text
                , isQuickOrApprove
                , (ushort)autoKickOut_absence
                , (ushort)autoKickOut_contibute
                , (revPacketRec, resListRec) =>
                {
                    operation02Done = true;

                    if (operation01Done)
                    {
                        ignoreClickEvents = false;
                        GuildController.NotifyUpdateEvent(UpdateEventType.DataRefreshed_GuildInfo);
                        GuildController.CloseOverlayPopup(OverlayWindowPopUP.GuildSetting);
                    }
                },
                (error, req, res) =>
                {
                    ignoreClickEvents = false;
                    operation02Done = true;
                    UIFrameGuildTabBase.HandleError(error, req, res);
                });
        }
        else
        {
            operation02Done = true;
        }

        if (nothingChanged)
        {
            ignoreClickEvents = false;
            GuildController.CloseOverlayPopup(OverlayWindowPopUP.GuildSetting);
        }
    }
    #endregion
}
