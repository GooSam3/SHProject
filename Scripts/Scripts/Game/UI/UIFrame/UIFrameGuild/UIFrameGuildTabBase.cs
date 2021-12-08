using GameDB;
using System;
using UnityEngine;
using WebNet;
using ZNet;

// TODO : Enum 타입들 옮겨야함 
public enum FrameGuildTabType
{
    None = 0,

    Main, // 절-대 꺼지지 않는 메인 화면 

    ParticipatedTab, // 가입상태에서의 타입
    NotParticipatedTab, // 미가입상태에서의 타입 

    Info, // 정보 
    Members, // 길드원 
    Ranking, // 랭킹 
    BattlePointRanking, // 배틀포인트 랭킹 
    Dungeon, // 던전 (20-09-04 기준던전이없음 .. 일단 등록안해놨음 )

    GuildSuggestion, // 추천길드 
    RequestList, // 신청목록 

    RewardDisplayer, // 보상 출력 탭 , 사실상 뭔가 팝업같은 느낌
}

public enum UpdateEventType
{
    None = 0,

    RequestGuildJoinRequests = 3, // 길드 가입 신청 데이터 새로고침 요청 
    RequestedDataRefreshed = 4, // 데이터 전체 Refresh 요청 
    DataAllRefreshed = 5, // 모든 데이터 Refreshed 되었음을 알림 
    RequestAttendRewardCheckingManually = 7, // 출석 보상 체킹 요청
    RequestAllianceInfo = 8, // 연맹 데이터 Refresh 요청 

    JoinedGuildOrCreated = 10, // 길드없는상태 -> 길드 있는상태 (Req 후)
    GuildDismissed = 11, // 길드 해산됨 (Req 후)
    GuildLeftAsMember = 13, // 길드원으로서 길드 탈퇴함. (Req 후)
    NotGuildMemberAnymore = 14, // 가만히 있다가 길드원이 더이상 아닌 상황 (broadcast)

    JoinRequested = 20, // 길드 신청한 상태 (길드는 없는 상태 그대로)  (Req 후)
    JoinRequestCanceled = 30, // 길드 신청 취소 (Req 후)

    DataRefreshed_GuildsRecommended = 40, // 데이터 업데이트 - 추천길드 리스트 
    DataRefreshed_GuildsRequestList = 50, // 데이터 업데이트 - 가입신청한 길드 리스트 
    DataRefreshed_GuildsRecommendedAndRequestList = 55, // 데이터 업데이트 - 추천길드,가입신청 길드 리스트 동시 

    DataRefreshed_AllianceGuildInfo = 60, // 데이터 업데이트 - 나의 길드의 연맹 길드 리스트
    DataRefreshed_ReceivedGuildJoinRequests = 65, // 데이터 업데이트 - 받은 길드 가입 신청 리스트
    DataRefreshed_Members = 70, // 데이터 업데이트 - 길드원 리스트 
    DataRefreshed_Ranking = 80, // 데이터 업데이트 - 길드 랭킹 리스트 

    DataRefreshed_GuildInfo = 90, // 데이터 업데이트 - 길드 정보 
    DataRefreshed_GuildInfoAndMemberInfo = 200, // 데이터 업데이트 - 길드정보 + 멤버정보 
    
    DataRefreshed_MarkTid = 300, // 데이터 업데이트 - 마크변경 

    ObtainedGuildReward = 400, // 길드 관련 Reward 획득 

    // 연맹 채팅 관련 밑에 추가 

   // CreatedAllianceChatRoom = 500, // 길드 연맹 채팅방 개설

}

public enum OverlayWindowPopUP
{
    GuildCreate,
    GuildBenefit,
    GuildJoin,
    RequestAlly,
    GuildSetting,
    GuildDonation
}

public class GuildDataUpdateEventParamBase 
{
    public bool skipGetGuildInfo = false;
}

public class EventParam_ReqAllianceState : GuildDataUpdateEventParamBase
{
    public E_GuildAllianceState[] States;
    public EventParam_ReqAllianceState(E_GuildAllianceState[] states)
    {
        this.States = states;
    }
}

[Serializable]
public class GuildOverlayPopup
{
    public OverlayWindowPopUP Type;
    public UIFrameGuildOverlayPopupBase Obj;
}

// 길드 UI 하나의 탭 베이스 
// UI Group 같은 개념 
public class UIFrameGuildTabBase : MonoBehaviour
{
    //public enum ErrorHandleLevel
    //{
    //    None = 0,
    //    CloseCurrentTab = 10,
    //    CloseGuild = 20
    //}

    #region SerializedField
    #region UI Variables

    #endregion

    #region Preference Variable
    [Header("현재 탭이 오픈되면 같이 오픈될 탭들, ** WARNING : 같은 그룹내의 탭이면 안됩니다 , 내부 시스템에 의해 자동으로 꺼짐 **")]
    [SerializeField] FrameGuildTabType[] tabsToOpenOnOpen;
    #endregion
    #endregion

    #region System Variable
    private FrameGuildTabType tabType;

    protected UIFrameGuild guildController;
    protected bool isOpen;
    #endregion

    #region Properties 
    public FrameGuildTabType TabType { get { return tabType; } }
    public bool IsOpen { get { return isOpen; } }
    #endregion

    #region Protected Methods
    protected void NotifyUpdateEvent(UpdateEventType eventType, GuildDataUpdateEventParamBase param = null)
    {
        guildController.NotifyUpdateEvent(eventType, param);
    }

    //protected void RegisterUpdateEventCallback(UpdateEventType eventType)
    //{
    //    guildController.AddListener_UpdateDataEvent(eventType, OnUpdateEventRise);
   // }

  //  protected void UnregisterUpdateEventCallback(UpdateEventType eventType)
  //  {
   //     guildController.RemoveListener_UpdateDataEvent(eventType, OnUpdateEventRise);
 //   }

    // TODO : 구현 완료후 지울 예정 
    protected void __NoImplement__(string append = "")
    {
        ZLog.LogError(ZLogChannel.UI, " GuildContent, No Implement : " + append);
    }
    #endregion

    #region Public Methods
    static public void OpenTwoButtonQueryPopUp(
        string title, string content, Action onConfirmed, Action onCanceled = null)
    {
        UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
        {
            _popup.Open(title, content, new string[] { "취소", "확인" }, new Action[] {
                () =>
                {
                     onCanceled?.Invoke();
                    _popup.Close();
                },
                () =>
                {
                   onConfirmed?.Invoke();
                    _popup.Close();
                }});
        });
    }

    static public void HandleError(ZWebCommunicator.E_ErrorType _errorType, ZWebReqPacketBase _reqPacket, ZWebRecvPacket _recvPacket)
    {
        OpenErrorPopUp(_recvPacket.ErrCode);
    }

    static public void OpenErrorPopUp(ERROR errorCode, Action onConfirmed = null)
    {
        Locale_Table table;

        // 에러코드 확인누르고 특별한 처리가 필요한경우 여기서 처리함 (onConfirmed)
        // if(errorCode == e)

        DBLocale.TryGet(errorCode.ToString(), out table);

        if (table != null)
        {
            OpenNotiUp(table.Text, onConfirmed: onConfirmed);
        }
        else
        {
            OpenNotiUp("문제가 발생하였습니다.", onConfirmed: onConfirmed);
        }
    }

    static public void OpenNotiUp(string content, string title = "확인", Action onConfirmed = null)
    {
        DBLocale.TryGet(ZUIString.LOCALE_OK_BUTTON, out Locale_Table table);

        if(table != null)
		{
            title = DBLocale.GetText(table.Text);
		}

		//if (string.IsNullOrEmpty(title))
		//{
		//	title = ZUIString.ERROR;
		//}

		UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
		{
			_popup.Open(title, content, new string[] { title }, new Action[] { () =>
				{
					onConfirmed?.Invoke();
					_popup.Close();
				}});
		});
	}

	#region Virtual Methods
	public virtual void Initialize(UIFrameGuild guildFrame, FrameGuildTabType type)
    {
        tabType = type;
        guildController = guildFrame;
        gameObject.SetActive(false);
    }

    public virtual void OnOpen()
    {
        // OnUpdateRequested();

        if (tabsToOpenOnOpen.Length > 0)
        {
            Array.ForEach(tabsToOpenOnOpen, t => guildController.OpenTab(t));
        }

        gameObject.SetActive(true);
        isOpen = true;
    }

    public virtual void OnClose()
    {
        gameObject.SetActive(false);
        isOpen = false;
    }

    // public virtual void OnUpdateRequested(object param = null) { }
    public virtual void OnUpdateEventRise(UpdateEventType type, GuildDataUpdateEventParamBase param) { }
    #endregion
    #endregion
}
