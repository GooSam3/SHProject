using UnityEngine;

public class UIFrameGuildTab_RequestList : UIFrameGuildTabBase
{
    #region SerializedField
    #region Preference Variable
    #endregion

    #region UI Variables
    [SerializeField] private ScrollGuildRequestForCharListAdapter ScrollAdapter;
    #endregion
    #endregion

    #region System Variables
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

        // Adapter 세팅 
        ScrollAdapter.Parameters.ItemPrefab = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(UIGuildRequestForCharListSlot)).transform as RectTransform;
        var prefab = ScrollAdapter.Parameters.ItemPrefab;
        prefab.transform.SetParent(transform);
        prefab.localPosition = Vector3.zero;
        prefab.localScale = Vector3.one;
        prefab.gameObject.SetActive(false);

        ScrollAdapter.Initialize();
        ScrollAdapter.AddListener_OnCanceled(OnRequestCanceled);
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
            case UpdateEventType.DataRefreshed_GuildsRequestList:
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
        ScrollAdapter.RefreshRequestList();
        ScrollAdapter.SetNormalizedPosition(1d);
    }

    private void OnRequestCanceled(ulong guildID)
    {
        guildController.NotifyUpdateEvent(UpdateEventType.JoinRequestCanceled);
    }
    #endregion

    #region OnClick Event (인스펙터 연결)
    #endregion
}
