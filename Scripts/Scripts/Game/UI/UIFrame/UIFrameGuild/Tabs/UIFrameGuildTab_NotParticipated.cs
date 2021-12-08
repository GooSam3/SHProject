using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIFrameGuildTab_NotParticipated : UIFrameGuildTabBase
{
    public enum ListTabType
    {
        None = 0,
        GuildSuggestions,
        Requests
    }

    [Serializable]
    public class ListTab
    {
        public ListTabType type;
        public Toggle toggle;
        public GameObject subActiveApplyObjs;
    }

    #region SerializedField
    #region Preference Variable
    [SerializeField, Tooltip("오픈시 보이게될 탭")] private ListTabType listTabOnOpen;
    #endregion

    #region UI Variables
    [SerializeField] private List<ListTab> listTabs;
    [SerializeField] private InputField guildSearchInputField;
    [SerializeField] private Text txtMaxGuildRequestAlarm;
    #endregion
    #endregion

    #region System Variables
    private ListTabType currentListTab;

    // 광클 방지용 
    private bool ignoreClickEvents;
    #endregion

    #region Properties 
    #endregion

    #region Unity Methods
    #endregion

    #region Public Methods
    #endregion

    #region Overrides 
    public override void Initialize(UIFrameGuild guildFrame, FrameGuildTabType type)
    {
        base.Initialize(guildFrame, type);
    }

    public override void OnOpen()
    {
        base.OnOpen();
        SetListTab(listTabOnOpen);
        ignoreClickEvents = false;
    }

    public override void OnClose()
    {
        base.OnClose();
        SetListTab(ListTabType.None);
        guildController.CloseOverlayPopup();
        this.guildSearchInputField.text = string.Empty;
    }

    public override void OnUpdateEventRise(UpdateEventType type, GuildDataUpdateEventParamBase param)
    {
        base.OnUpdateEventRise(type, param);

        if (type.Equals(UpdateEventType.JoinRequested) ||
            type.Equals(UpdateEventType.JoinRequestCanceled))
        {
            RefreshList_RecommendedAndRequested();
        }
        else if (type == UpdateEventType.DataRefreshed_GuildsRecommendedAndRequestList)
        {
            UpdateUI();
        }
    }
    #endregion

    #region Private Methods
    private void RefreshList_RecommendedAndRequested()
    {
        ignoreClickEvents = true;

        UIFrameGuildNetCapturer.ReqRecommendGuildInfo(
            (revPacket_rec, resList_rec) =>
            {
                UIFrameGuildNetCapturer.ReqGuildRequestListForChar(
                    (revPacket_req, resList_req) =>
                    {
                        ignoreClickEvents = false;
                        NotifyUpdateEvent(UpdateEventType.DataRefreshed_GuildsRecommendedAndRequestList);
                    },
                    (err, req, res) =>
                    {
                        ignoreClickEvents = false;
                        HandleError(err, req, res);
                    });
            },
            (err, req, res) =>
            {
                ignoreClickEvents = false;
                HandleError(err, req, res);
            });
    }


    //private void RefreshRecommendGuildList_Server()
    //{
    //    UIFrameGuildNetCapturer.ReqRecommendGuildInfo(
    //        (revPacket, resList) =>
    //        {
    //            if (clickUnlockCheck02)
    //                ignoreClickEvents = false;
    //            clickUnlockCheck01 = true;
    //            NotifyUpdateEvent(UpdateEventType.DataRefreshed_GuildsRecommended);
    //        },
    //        (err, req, res) =>
    //        {
    //            if (clickUnlockCheck02)
    //                ignoreClickEvents = false;
    //            clickUnlockCheck01 = true;
    //            HandleError(err, req, res);
    //        });
    //}

    //private void RefreshRequestList_Server()
    //{
    //    UIFrameGuildNetCapturer.ReqGuildRequestListForChar(
    //        (revPacket, resList) =>
    //        {
    //            if (clickUnlockCheck01)
    //                ignoreClickEvents = false;
    //            clickUnlockCheck02 = true;
    //            NotifyUpdateEvent(UpdateEventType.DataRefreshed_GuildsRequestList);
    //        },
    //        (err, req, res) =>
    //        {
    //            if (clickUnlockCheck01)
    //                ignoreClickEvents = false;
    //            clickUnlockCheck02 = true;
    //            HandleError(err, req, res);
    //        });
    //}

    private void CreateGuild()
    {
        guildController.OpenOverlayPopup(OverlayWindowPopUP.GuildCreate);
    }

    private void TabToggleChanged()
    {
        var t = listTabs.Find(_t => _t.toggle.isOn);
        SetListTab(t.type);
    }

    private void UpdateUI()
    {
        switch (currentListTab)
        {
            case ListTabType.None:
                break;
            case ListTabType.GuildSuggestions:
                guildSearchInputField.text = string.Empty;
                break;
            case ListTabType.Requests:
                {
                    txtMaxGuildRequestAlarm.text =
                        string.Format("길드 가입 신청은 최대 30개까지 가능합니다.  <color=#ffffff>({0}/{1})</color>"
                        , UIFrameGuildNetCapturer.GuildsRequestInfoForChar.Count, 30); // MaxCount Fix me 
                }
                break;
            default:
                break;
        }
    }

    private void SetListTab(ListTabType tabType)
    {
        currentListTab = tabType;

        switch (tabType)
        {
            case ListTabType.None:
                guildController.CloseTab(FrameGuildTabType.GuildSuggestion);
                guildController.CloseTab(FrameGuildTabType.RequestList);
                break;
            case ListTabType.GuildSuggestions:
                guildController.OpenTab(FrameGuildTabType.GuildSuggestion);
                break;
            case ListTabType.Requests:
                guildController.OpenTab(FrameGuildTabType.RequestList);
                break;
        }

        UpdateUI();

        foreach (var listTab in listTabs)
        {
            if (listTab.type == tabType)
            {
                if (listTab.toggle.isOn == false)
                {
                    listTab.toggle.isOn = true;
                }
            }

            listTab.subActiveApplyObjs.SetActive(listTab.type == tabType);
        }
    }
    #endregion

    #region Inspector Events (인스펙터 연결)
    #region OnClick
    public void OnClickRefreshBtn()
    {
        if (ignoreClickEvents)
        {
            return;
        }

        //clickUnlockCheck01 = false;
        //clickUnlockCheck02 = false;

        RefreshList_RecommendedAndRequested();
        this.guildSearchInputField.text = string.Empty;

        //        RefreshRecommendGuildList_Server();
        //      RefreshRequestList_Server();
    }

    public void OnClickCreateGuildBtn()
    {
        if (ignoreClickEvents)
            return;

        CreateGuild();
    }

    public void OnClickBenefitBtn()
    {
        if (ignoreClickEvents)
            return;

        guildController.OpenOverlayPopup(OverlayWindowPopUP.GuildBenefit);
    }

    public void OnClickGuildSearch()
    {
        if (ignoreClickEvents)
            return;

        string errMsg = string.Empty;

        if( UIFrameGuildNetCapturer.ValidateGuildName(guildSearchInputField.text, out errMsg) == false )
		{
            OpenNotiUp(errMsg, "확인");
            return; 
		}

        if(guildSearchInputField.text.Length > DBConfig.GuildName_Length_Max)
		{
			OpenNotiUp(string.Format("길드명은 {0}자 이하여야 합니다.", DBConfig.GuildName_Length_Max));
			return;
		}

        //  int resultGuildCount = 0;

        // guildController.RetrieveTabComponent<UIFrameGuildTab_GuildSuggestion>().RefreshScrollFilteredBy(guildSearchInputField.text, out resultGuildCount);

        ZWebManager.Instance.WebGame.REQ_FindGuildInfo(guildSearchInputField.text,
            (revPacket, resList) =>
            {
                if (resList.GuildInfo.HasValue == false || string.IsNullOrEmpty(resList.GuildInfo.Value.MasterCharNick))
                {
                    OpenNotiUp(DBLocale.GetText("Non_Existent_Guild_Message"), "알림");
                }
                else
                {
                    guildController.RetrieveTabComponent<UIFrameGuildTab_GuildSuggestion>().RefreshScrollSingle(new UIFrameGuildNetCapturer.GuildInfoConverted(resList.GuildInfo.Value));
                }
            });
    }
    #endregion
    #region Toggle
    public void OnTabToggleValueChanged()
    {
        if (guildController == null
            || guildController.IsTabStateInit == false)
            return; 

        TabToggleChanged();
    }
    #endregion
    #endregion
}
