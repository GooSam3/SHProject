using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZNet.Data;

public class UIFrameGuildTab_Participated : UIFrameGuildTabBase
{
    public enum Tab
    {
        None = 0,
        Info,
        Members,
        Ranking,
        BattlePointRanking,
        Dungeon
    }

    [Serializable]
    public class TabRadioBtn
    {
        public FrameGuildTabType subTab;
        public Toggle toggle;
    }

    #region SerializedField
    #region Preference Variable
    [Header("디폴트 탭")]
    [SerializeField] private FrameGuildTabType defaultTab = FrameGuildTabType.Info;
    #endregion

    #region UI Variables
    [SerializeField] private List<TabRadioBtn> tabBtns;
    #endregion
    #endregion

    #region System Variables
    private FrameGuildTabType curTab;
    #endregion

    #region Properties 
    #endregion

    #region Unity Methods
    #endregion

    #region Public Methods
    #endregion

    #region Overrides 
    public override void OnOpen()
    {
        base.OnOpen();

        SetTab(defaultTab, true);

        //// 연맹 정보 전부 갱신 
        //// 동맹 길드 , 적대 길드, 내 길드가 받은 동맹 요청 (ReceiveAlliance) , 내 길드가 한 동맹 요청 (RequestAlliance)
        //UIFrameGuildNetCapturer.ReqGetGuildAllianceList(
        //    Me.CurCharData.GuildId
        //    , new WebNet.E_GuildAllianceState[] {
        //        WebNet.E_GuildAllianceState.Alliance
        //        , WebNet.E_GuildAllianceState.Enemy
        //    , WebNet.E_GuildAllianceState.RequestAlliance
        //    , WebNet.E_GuildAllianceState.ReceiveAlliance}
        //    , (revPacketRec, resListRec) =>
        //    {
        //        // 길드 랭킹 리스트 요청함 . 
        //        UIFrameGuildNetCapturer.ReqGetGuildExpRank(
        //            (revPacketRec_02, resListRec_02) =>
        //            {
        //                // 길드입장서 가입 요청 요청함. 
        //                UIFrameGuildNetCapturer.ReqGuildRequestListForGuild(
        //                    Me.CurCharData.GuildId
        //                    , (revPacketRec_03, resListRec_03) =>
        //                    {
        //                        SetTab(defaultTab, true);
        //                    });
        //            }, HandleError);
        //    });
    }

    public override void OnClose()
    {
        base.OnClose();

        guildController.CloseOverlayPopup();

        foreach (var tab in tabBtns)
        {
            if (guildController.IsTabOpen(tab.subTab))
            {
                guildController.CloseTab(tab.subTab);
                break;
            }
        }
    }

    public override void OnUpdateEventRise(UpdateEventType type, GuildDataUpdateEventParamBase param)
    {
        base.OnUpdateEventRise(type, param);

        if (type == UpdateEventType.RequestGuildJoinRequests)
        {
            UIFrameGuildNetCapturer.ReqGuildRequestListForGuild(
                Me.CurCharData.GuildId
                , (revPacket, resList) =>
                {
                    NotifyUpdateEvent(UpdateEventType.DataRefreshed_ReceivedGuildJoinRequests);
                });
        }

        /// 연맹 정보 업데이트 요청 처리 
		if (type == UpdateEventType.RequestAllianceInfo)
		{
			var param_allianceStates = param as EventParam_ReqAllianceState;

			if (param_allianceStates == null)
			{
				ZLog.LogError(ZLogChannel.UI, "EventParam Converting Failed. Check Data Type");
			}
			else
			{
				/// EventParam Validation 
				if (param_allianceStates.States == null
					|| param_allianceStates.States.Length == 0
					|| (param_allianceStates.States.Length == 1 && param_allianceStates.States[0] == WebNet.E_GuildAllianceState.None))
				{
					ZLog.LogError(ZLogChannel.UI, "Please Check Guild AlianceInfoUpdateRequest EventParam.");
				}
				else
				{
					UIFrameGuildNetCapturer.ReqGetGuildAllianceList(Me.CurCharData.GuildId
					   , param_allianceStates.States
					   , (revPacket, resListRes) =>
					   {
						   NotifyUpdateEvent(UpdateEventType.DataRefreshed_AllianceGuildInfo);
					   });
				}
			}
		}

		/// 출석 보상 체킹 요청 처리 
		if (type == UpdateEventType.DataAllRefreshed
			|| type == UpdateEventType.RequestAttendRewardCheckingManually)
		{
			var myMemberInfo = UIFrameGuildNetCapturer.MyGuildData.MyMemberInfo;

			/// 만약 나의 마지막 출석 보상 Dt 가 오늘이 아니다 -> 새로 출석 및 보상
			if (myMemberInfo != null
			&& TimeHelper.IsGivenDtToday(myMemberInfo.attendRewardDt, 5) == false)
			{
				UIFrameGuildNetCapturer.ReqGuildAttendReward(Me.CurCharData.GuildId,
					(revPacketRec, resListRec) =>
					{
						var displayInfo = new UIFrameGuildTab_DisplayReward.EventParam_DisplayInfo();

						displayInfo.strTitle = string.Format("길드 출석 보상을 획득하였습니다.");

						/// 0 이면 에러가 날것임 . 그러므로 우선 예외처리 
						/// if (resListRec.GetItemsLength > 0)
						{
							displayInfo.items = new UIFrameGuildTab_DisplayReward.DisplaySingleItemInfo[resListRec.GetItemsLength];

							/// 출력해줄 아이템 정보 세팅 
							for (int i = 0; i < resListRec.GetItemsLength; i++)
							{
								var itemInfo = resListRec.GetItems(i).Value;
								displayInfo.items[i] = new UIFrameGuildTab_DisplayReward.DisplaySingleItemInfo();
								displayInfo.items[i].iconSprite = ZManagerUIPreset.Instance.GetSprite(DBItem.GetItemIconName(itemInfo.ItemTid));
								displayInfo.items[i].strCnt = itemInfo.ItemCnt.ToString("n0");
							}
						}

						NotifyUpdateEvent(UpdateEventType.ObtainedGuildReward, displayInfo);
					});
			}
		}
	}
	#endregion

	#region Private Methods
	// 이미 On 이라면 changed 콜백 호출안되므로 상황따라 force 업데이트 함 
	private void SetTab(FrameGuildTabType tab, bool forceUpdate = false)
    {
        var target = tabBtns.Find(t => t.subTab == tab);

        if (target == null)
        {
            target = tabBtns.Find(t => t.subTab == defaultTab);
        }

        target.toggle.isOn = true;

        if (forceUpdate)
        {
            UpdateTab();
        }
    }

    void UpdateTab()
    {
        foreach (var tab in tabBtns)
        {
            guildController.CloseTab(tab.subTab);
        }

        var target = tabBtns.Find(t => t.toggle.isOn);

        if (target == null)
        {
            target = tabBtns.Find(t => t.subTab.Equals(defaultTab));
        }

        if (target != null)
        {
            curTab = target.subTab;

            guildController.OpenTab(target.subTab);
        }
    }
    #endregion

    #region Insepctor Event (인스펙터 연결)
    public void OnSubTabToggleChanged(Toggle toggle)
    {
        if (guildController == null ||
            guildController.IsTabStateInit == false)
            return; 

        if (toggle.isOn)
            UpdateTab();
    }
    #endregion
}
