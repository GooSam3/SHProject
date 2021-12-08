using UnityEngine;
using ZNet.Data;
using UnityEngine.UI;
using static UIFrameGuildNetCapturer;

public class UIFrameGuildPopup_GuildJoin
    : UIFrameGuildOverlayPopupBase
{
    #region SerializedField
    #region Preference Variable
    #endregion

    #region UI Variables
    [SerializeField] private ScrollGuildJoinListAdapter ScrollAdapter;
    [SerializeField] private Text txtCnt;
    #endregion
    #endregion

    #region System Variables
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

        var slot = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(ScrollGuildJoinListSlot));

        ScrollAdapter.Parameters.ItemPrefab = slot.GetComponent<RectTransform>();
        var pf = ScrollAdapter.Parameters.ItemPrefab;
        pf.SetParent(transform);
        pf.localScale = Vector2.one;
        pf.localPosition = Vector3.zero;
        pf.gameObject.SetActive(false);

        ScrollAdapter.AddListener_OnClickedApprove(OnClicked_Approve);
        ScrollAdapter.AddListener_OnClickedReject(OnClicked_Reject);
        ScrollAdapter.Initialize();
    }

    public override void Open()
    {
        base.Open();
        ignoreClickEvents = false;
        ScrollAdapter.RefreshRankingList();
        UpdateUI();
    }
    #endregion

    #region Public Methods
    #endregion

    #region Private Methods
    private void UpdateUI()
    {
        txtCnt.text = string.Format(DBLocale.GetText("UI_Common_Amount_Simple"), UIFrameGuildNetCapturer.MyGuildData.receivedJoinRequestList.Count, DBConfig.Guild_Request_Max);
    }

    private void OnClicked_Approve(GuildRequestListForGuildConverted data)
    {
        if (ignoreClickEvents)
            return;

        ignoreClickEvents = true;

        UIFrameGuildNetCapturer.ReqGuildRequestAccept(
            Me.CurCharData.GuildId
            , data.char_id
            , (revPacketRec, resListRec) =>
            {
                ignoreClickEvents = false;
                ScrollAdapter.RefreshRankingList();
                UpdateUI();
            },
            (error, req, recv) =>
            {
                ignoreClickEvents = false;
                UIFrameGuildTabBase.HandleError(error, req, recv);
            });
    }

    private void OnClicked_Reject(GuildRequestListForGuildConverted data)
    {
        if (ignoreClickEvents)
            return;

        ignoreClickEvents = true;

        UIFrameGuildNetCapturer.ReqGuildRequestReject(
            Me.CurCharData.GuildId
            , data.char_id
            , (revPacketRec, resListRec) =>
            {
                ignoreClickEvents = false;
                ScrollAdapter.RefreshRankingList();
                UpdateUI();
            },
            (error, req, recv) =>
            {
                ignoreClickEvents = false;
                UIFrameGuildTabBase.HandleError(error, req, recv);
            });
    }

    #endregion

    #region OnClick Event (인스펙터 연결)

    #endregion
}
