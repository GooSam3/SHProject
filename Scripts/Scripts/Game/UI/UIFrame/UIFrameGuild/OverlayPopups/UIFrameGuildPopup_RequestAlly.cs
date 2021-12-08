using UnityEngine;
using static UIFrameGuildNetCapturer;

public class UIFrameGuildPopup_RequestAlly : UIFrameGuildOverlayPopupBase
{
    #region SerializedField
    #region Preference Variable
    #endregion

    #region UI Variables
    [SerializeField] private ScrollGuildRequestAllyListAdapter ScrollAdapter;
    #endregion
    #endregion

    #region System Variables
    #endregion

    #region Properties 
    #endregion

    #region Unity Methods
    #endregion

    #region Overrides 
    public override void Initialize(UIFrameGuild guildController)
    {
        base.Initialize(guildController);

        var slot = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(ScrollGuildRequestAllyListSlot));
        ScrollAdapter.Parameters.ItemPrefab = slot.GetComponent<RectTransform>();
        var pf = ScrollAdapter.Parameters.ItemPrefab;
        pf.SetParent(transform);
        pf.localScale = Vector2.one;
        pf.localPosition = Vector3.zero;
        pf.gameObject.SetActive(false);

        ScrollAdapter.AddListener_OnClickedAccept(OnClickedAccept);
        ScrollAdapter.AddListener_OnClickedReject(OnClickedReject);
        ScrollAdapter.Initialize();
    }

    public override void Open()
    {
        base.Open();

        ScrollAdapter.RefreshList();
    }
    #endregion


    #region Public Methods
    #endregion

    #region Private Methods
    private void OnClickedReject(GuildAllianceInfoConverted data)
    {

    }

    private void OnClickedAccept(GuildAllianceInfoConverted data)
    {

    }
    #endregion

    #region OnClick Event (인스펙터 연결)
    #endregion
}
