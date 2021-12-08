using UnityEngine;

public class UIFrameGuildTab_Ranking : UIFrameGuildTabBase
{
    #region SerializedField
    #region Preference Variable
    #endregion

    #region UI Variables
    [SerializeField] private ScrollGuildRankingListAdapter ScrollAdapter;
    [SerializeField] private RectTransform mySlotParent; 
    #endregion
    #endregion

    #region System Variables
    // 나의 길드 랭킹 별도로 표시용 
    private ScrollGuildRankingListSlot myGuildSlot;
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

        var slot = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(ScrollGuildRankingListSlot));

        ScrollAdapter.Parameters.ItemPrefab = slot.GetComponent<RectTransform>();
        var pf = ScrollAdapter.Parameters.ItemPrefab;
        pf.SetParent(transform);
        pf.localScale = Vector2.one;
        pf.localPosition = Vector3.zero;
        pf.gameObject.SetActive(false);

        ScrollAdapter.SetRankSpriteGetter(GetRankSprite);
        ScrollAdapter.Initialize();

        var mySlot = Instantiate(slot, mySlotParent);

        if (mySlot != null)
        {
            // FindClone 이 타입 반환 함수가 아니라서 어쩔수없이 GetComponent 를 씀 
            myGuildSlot = mySlot.GetComponent<ScrollGuildRankingListSlot>();
            mySlot.gameObject.SetActive(true);
        }
    }

    public override void OnUpdateEventRise(UpdateEventType type, GuildDataUpdateEventParamBase param)
    {
        base.OnUpdateEventRise(type, param);

        switch (type)
        {
            case UpdateEventType.DataAllRefreshed:
            case UpdateEventType.DataRefreshed_Ranking:
                {
                    ResetScroll();
                }
                break;
        }
    }

    public override void OnOpen()
    {
        base.OnOpen();

        ResetScroll();
        UpdateMyGuildSlot();
    }
    #endregion

    #region Private Methods
    private void ResetScroll()
    {
        ScrollAdapter.SetNormalizedPosition(1d);
        ScrollAdapter.RefreshRankingList();
    }

    // 10 위 전까지는 테이블에 넣어주기만하면 되게끔 작성함 . NULL 이면 텍스트로 표시하는 방식. 
    private Sprite GetRankSprite(uint rank)
    {
        return rank < 10 ? ZManagerUIPreset.Instance.GetSprite("img_txt_rank_" + rank.ToString()) : null;
    }

    private void UpdateMyGuildSlot()
    {
        if (myGuildSlot == null)
            return;

        var t = UIFrameGuildNetCapturer.MyGuildData.myGuildInfo;

        myGuildSlot.SetData(
            ZManagerUIPreset.Instance.GetSprite(DBGuild.GetGuildMark(t.markTid))
            , GetRankSprite(t.rank)
            , t.rank
            , DBGuild.GetLevel(t.exp)
            , t.guildName
            , t.curTotalMemberCount
            , DBConfig.Guild_Max_Character
            , t.masterName
            , t.introduction);
    }
    #endregion

    #region OnClick Event (인스펙터 연결)
    #endregion
}
