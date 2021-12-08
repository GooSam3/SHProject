using System;
using WebNet;
using ZNet;
using ZNet.Data;

/*
 * 애는 절대 꺼지지 않는 애. 끄면안됨. 
 * */
public class UIFrameGuildTab_Main : UIFrameGuildTabBase
{
    #region SerializedField
    #region Preference Variable
    #endregion

    #region UI Variables
    #endregion
    #endregion

    #region System Variables
    // 현재 유저가 보고 있는 화면이 길드 가입된 후에 출력되는 화면이라면 
    // 해당 길드의 ID 를 여기서 보관하여 나중에 길드가입/탈퇴/강퇴 등 상황에서 
    // 중복 처리를 방지함 .
    private ulong curCapturedGuildID;
    #endregion

    #region Properties 
    #endregion

    #region Unity Methods
    #endregion

    #region Public Methods
    public void FindGuild(string guildNameToFind, Action<ZWebRecvPacket, ResFindGuildInfo> _onReceive)
	{
		ZWebManager.Instance.WebGame.REQ_FindGuildInfo(guildNameToFind,
            (revPacket, resList) =>
            {
                if (resList.GuildInfo.HasValue == false || string.IsNullOrEmpty(resList.GuildInfo.Value.MasterCharNick))
				{
					OpenNotiUp(DBLocale.GetText("Non_Existent_Guild_Message"), "알림");
				}
				else
				{
                    _onReceive?.Invoke(revPacket, resList);
				}
			});
	}
	#endregion

	#region Overrides 
	public override void OnOpen()
    {
        base.OnOpen();

        AddExternalEvents();

        // 길드 존재 여부 체킹 후 추가로 탭 Open
        bool isParticipated = Me.CurCharData.GuildId != 0;

        if (isParticipated)
        {
            OpenParticipatedTab();
        }
        else
        {
            OpenNotParticipatedTab();
        }
    }

    public override void OnClose()
    {
        base.OnClose();
        RemoveExternalEvents();

        // Me.CurCharData.GuildInfoUpdated -= GuildInfoUpdated;
    }

    #region 브로드캐스트 이벤트 함수에 의해 호출됨 

    //private void OnGuildInfoChanged(ulong prevGuildID, ulong curGuildID)
    //{
    //    if (prevGuildID != curGuildID)
    //    {

    //    }
    //    else
    //    {

    //    }
    //}

    private void OnJoinGuildUser(ulong charID, ResGetGuildInfo guildInfo)
    {
        //      UnityEngine.Debug.LogError("---- recevied broadcast Callback : OnJoinGuildUser , " + charId + " " + guildID + " " + charNick + " " + guildMarkTid);

        if (Me.CurCharData.ID == charID)
        {
            UIFrameGuildNetCapturer.MyGuildData.myGuildInfo = new UIFrameGuildNetCapturer.GuildInfoConverted(guildInfo.GuildInfo.Value);
            UIFrameGuildNetCapturer.MyGuildData.members.Clear();

            for (int i = 0; i < guildInfo.GuildMemberInfosLength; i++)
            {
                UIFrameGuildNetCapturer.MyGuildData.AddMemberInfo(guildInfo.GuildMemberInfos(i).Value);
            }

            UIFrameGuildNetCapturer.MyGuildData.SortMemberListByDisplayOrder();

            guildController.NotifyUpdateEvent(UpdateEventType.JoinedGuildOrCreated, new GuildDataUpdateEventParamBase() { skipGetGuildInfo = true });
        }
    }

    private void OnLeaveGuildUser(ulong charId, ulong guildID)
    {
        //   UnityEngine.Debug.LogError("---- recevied broadcast Callback : OnOnLeaveGuildUser , " + charId + " " + guildID);

        // 이미 강퇴 또는 나갔는데 이 브로드캐스트를 들어올수도 있음 . 그러므로 예외처리 
        if (Me.CurCharData.ID == charId)
        {
            UIFrameGuildNetCapturer.MyGuildData.ClearAll();
            guildController.NotifyUpdateEvent(UpdateEventType.NotGuildMemberAnymore);
        }
    }

    private void OnGuildMarkChange(ulong guildID, byte guildMarkTid)
    {
        //  UnityEngine.Debug.LogError("---- recevied broadcast Callback : OnGuildMarkChange , " + guildID + " " + " " + guildMarkTid);

        if (Me.CurCharData.GuildId == guildID)
        {
            UIFrameGuildNetCapturer.MyGuildData.myGuildInfo.markTid = guildMarkTid;
            guildController.NotifyUpdateEvent(UpdateEventType.DataRefreshed_MarkTid);
        }
    }
    #endregion

    public override void OnUpdateEventRise(UpdateEventType type, GuildDataUpdateEventParamBase param)
    {
        base.OnUpdateEventRise(type, param);

        switch (type)
        {
            /// 내부 또는 외부에서 현재 화면에서 데이터 전체 Refreh 를 요청받은 경우 
            case UpdateEventType.RequestedDataRefreshed:
                {
                    if (gameObject.activeInHierarchy)
                    {
                        // Data 만 Refresh 하고 오버레이 팝업은 꺼줌 
                        // 내부에서 알아서 DataAllRefreshed Notify 하고 필요한 탭에서는 
                        // 알아서 이벤트 받아서 각자 데이터 Refresh 진행 
                        guildController.OnAllServerDataRefreshRequested(() =>
                        {
                            guildController.CloseOverlayPopup();
                            // OpenNotiUp("길드 정보가 업데이트되었습니다.", "알림");
                        });
                    }
                }
                break;
            // 길드가 생긴경우 
            case UpdateEventType.JoinedGuildOrCreated:
                {
                    // 이거를 검사하는 이유는 , 혹시 지금 해당 GuildID 로 인해 켜져있는데
                    // 이미 또 켜지려고 요청이 들어온건지 체크하기 위함 
                    if (curCapturedGuildID != Me.CurCharData.GuildId)
                    {
                        bool skipGetInfo = false;

                        // GetInfo 스킵 선택 파라미터 체크 
                        if (param != null &&
                            param.skipGetGuildInfo)
                        {
                            skipGetInfo = true;
                        }

                        guildController.OnAllServerDataRefreshRequested(() =>
                        {
                            OpenParticipatedTab();
                            NotifyUpdateEvent(UpdateEventType.RequestAttendRewardCheckingManually);
                        }, skipGetInfo);
                    }
                }
                break;
            // 길드가 없어진경우
            case UpdateEventType.NotGuildMemberAnymore:
            case UpdateEventType.GuildDismissed:
            case UpdateEventType.GuildLeftAsMember:
                {
                    if (curCapturedGuildID != 0)
                    {
                        guildController.OnAllServerDataRefreshRequested(() =>
                        {
                            OpenNotParticipatedTab();
                        });
                    }
                }
                break;
        }
    }
    #endregion

    #region Private Methods
    private void AddExternalEvents()
    {
        // ZWebChatData.OnJoinGuildUser += OnJoinGuildUser;
        ZWebChatData.OnJoinGuildUser_AfterGetInfoDone += OnJoinGuildUser;
        ZWebChatData.OnLeaveGuildUser += OnLeaveGuildUser;
        ZWebChatData.OnGuildMarkChange += OnGuildMarkChange;
    }

    private void RemoveExternalEvents()
    {
        ZWebChatData.OnJoinGuildUser_AfterGetInfoDone -= OnJoinGuildUser;
        ZWebChatData.OnLeaveGuildUser -= OnLeaveGuildUser;
        ZWebChatData.OnGuildMarkChange -= OnGuildMarkChange;
    }

    private void OpenParticipatedTab()
    {
        curCapturedGuildID = Me.CurCharData.GuildId;
        guildController.OpenTab(FrameGuildTabType.ParticipatedTab);
    }

    private void OpenNotParticipatedTab()
    {
        curCapturedGuildID = 0;
        guildController.OpenTab(FrameGuildTabType.NotParticipatedTab);
    }
    #endregion

    #region OnClick Event (인스펙터 연결)
    #endregion
}
