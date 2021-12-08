using UnityEngine;
using static UIFrameGuildNetCapturer;

public class UIFrameGuildTab_GuildSuggestion : UIFrameGuildTabBase
{
    public delegate void OnRequestJoin(ulong guildID, string comment, bool isQuickJoin);

    #region SerializedField
    #region Preference Variable
    #endregion

    #region UI Variables
    [SerializeField] public ScrollGuildRecommendListAdapter ScrollAdapter;
    #endregion
    #endregion

    #region System Variables
    #endregion

    #region Properties 
    #endregion

    #region Unity Methods
    #endregion

    #region Public Methods
    public void RefreshScrollFilteredBy(string words, out int resultSearchedGuildCount)
    {
        ScrollAdapter.RefreshFilteredBy(words, out resultSearchedGuildCount);
    }

    public void RefreshScrollSingle(GuildInfoConverted guildInfo)
    {
        ScrollAdapter.RefreshListSingle(guildInfo);
    }
    #endregion

    #region Overrides 
    public override void Initialize(UIFrameGuild guildFrame, FrameGuildTabType type)
    {
        base.Initialize(guildFrame, type);
        ScrollAdapter.Parameters.ItemPrefab = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(UIGuildRecommendListSlot)).transform as RectTransform;
        var prefab = ScrollAdapter.Parameters.ItemPrefab;
        prefab.transform.SetParent(transform);
        prefab.localPosition = Vector3.zero;
        prefab.localScale = Vector3.one;
        prefab.gameObject.SetActive(false);

        ScrollAdapter.Initialize();
        ScrollAdapter.AddListener_OnRequestJoin(GuildJoinRequested);
    }

    public override void OnOpen()
    {
        base.OnOpen();
        ResetScroll();
    }

    public override void OnUpdateEventRise(UpdateEventType type, GuildDataUpdateEventParamBase param)
    {
        base.OnUpdateEventRise(type, param);

        switch (type)
        {
            case UpdateEventType.DataAllRefreshed:
            case UpdateEventType.DataRefreshed_GuildsRecommended:
            case UpdateEventType.DataRefreshed_GuildsRecommendedAndRequestList:
                {
                    ResetScroll();
                }
                break;
            default:
                break;
        }
    }
    #endregion

    #region Private Methods
    private void ResetScroll()
    {
        ScrollAdapter.RefreshRecommendList();
        ScrollAdapter.SetNormalizedPosition(1d);
    }

    private void GuildJoinRequested(ulong guildID, string comment, bool isQuickJoin)
    {
        // UIFrameGuildNetCapturer.ReqGuildRequestJoin(guildID, comment,
        ZWebManager.Instance.WebGame.REQ_GuildRequestJoin(guildID, comment
            , (revPacket, resList, isJoined) =>
             {
                 if (isJoined)
                 {
                     OpenNotiUp("성공적으로 가입되었습니다.", "알림"
                         , onConfirmed: () =>
                        NotifyUpdateEvent(UpdateEventType.JoinedGuildOrCreated));
                 }
                 else
                 {
                     OpenNotiUp("가입 신청을 완료하였습니다.", "알림"
                         , onConfirmed: () =>
                        NotifyUpdateEvent(UpdateEventType.JoinRequested));
                 }
             }, HandleError);
        #endregion

        #region OnClick Event (인스펙터 연결)
        #endregion
    }
}
