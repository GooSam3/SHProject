using GameDB;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZDefine;
using ZNet.Data;

public class UIFrameRank : ZUIFrameBase
{
    public enum ViewType
    {
        TYPE_EXP_RANK = 0,
        TYPE_PK_RANK = 1,
        TYPE_PK_HISTORY = 2
    }

    #region UI Variable
    [SerializeField] private GameObject PlayerRankListGroup;
    [SerializeField] private GameObject PvpRankListGroup;
    [SerializeField] private GameObject PvpLogListGroup;
    [SerializeField] private UIRankScrollAdapter ScrollAdapter;
    [SerializeField] private UIPvpRankScrollAdapter PvpScrollAdapter;
    [SerializeField] private UIPvpLogScrollAdapter PvpLogScrollAdapter;
    [SerializeField] private GameObject myRankSlot;
    [SerializeField] private GameObject RankingBufferInfoPopup;
    [SerializeField] private ZImage BufferInfoPopupIcon;
    [SerializeField] private ZText BufferInfoPopupTitle;
    [SerializeField] private ZText BufferInfoPopupDesc;

    // myRankSlot
    [SerializeField] private Image RankIcon;
    [SerializeField] private Image ClassIcon;
    [SerializeField] private Image GuildIcon;
    [SerializeField] private Image BufferIcon01;
    [SerializeField] private Image BufferIcon02;
    [SerializeField] private Image BufferIcon03;
    [SerializeField] private Text RankText;
    [SerializeField] private Text PlayerName;
    [SerializeField] private Text GuildName;

    // radio
    [SerializeField]ZToggle FirstTab = null;
    [SerializeField]ZToggle FirstClassTab = null;
    #endregion

    #region System Variable
    private ViewType selectViewType;
    private E_CharacterType selectClassType;

    private RankBuff_Table rewardBuffData = null;
    public override bool IsBackable => true;
    private bool IsInit = false;
    #endregion

    protected override void OnInitialize()
    {
        base.OnInitialize();

        selectViewType = ViewType.TYPE_EXP_RANK;
        selectClassType = E_CharacterType.None;
        RankingBufferInfoPopup.SetActive(false);

        ScrollAdapter.Initialize();
        PvpScrollAdapter.Initialize();
        PvpLogScrollAdapter.Initialize();

        IsInit = true;
    }

    protected override void OnShow(int _LayerOrder)
    {
        base.OnShow(_LayerOrder);
        
        if (!IsInit)
            return;

        // UIFrameHUD 예외처리.
        if (UIManager.Instance.Find(out UIFrameHUD _hud))
            _hud.SetSubHudFrame(E_UIStyle.FullScreen);

        RankingBufferInfoPopup.SetActive(false);

        // DoUIButtonClickEvent가 UpdateTab을 불러오지 않음.
        //UpdateTab();
        OnClickTab((int)ViewType.TYPE_EXP_RANK);
    }

    protected override void OnHide()
    {
        base.OnHide();
        // UIFrameHUD 예외처리.
        if (UIManager.Instance.Find(out UIFrameHUD _hud))
            _hud.SetSubHudFrame();

        RankingBufferInfoPopup.SetActive(false);
    }

    /// <summary>
    /// 탭(뷰 타입) 클릭
    /// </summary>
    public void OnClickTab(int _changedViewType)
    {
        //if (ScrollAdapter.IsInitialized && ScrollAdapter.Data.List.Count > 0)
        //    ScrollAdapter.ScrollTo(0);

        selectViewType = (ViewType)_changedViewType;

        OnClickClassTabList((int)selectClassType);
        //UpdateTab();
    }

    /// <summary>
    /// 클래스 탭 클릭
    /// </summary>
    public void OnClickClassTabList(int _changedCharType)
    {
        //if (ScrollAdapter.IsInitialized && ScrollAdapter.Data.List.Count > 0)
        //    ScrollAdapter.ScrollTo(0);

        selectClassType = (E_CharacterType)_changedCharType;

        UpdateTab();
    }

    public void UpdateTab()
    {
        PlayerRankListGroup.SetActive(false);
        PvpRankListGroup.SetActive(false);
        PvpLogListGroup.SetActive(false);
        myRankSlot.gameObject.SetActive(false);

        switch (selectViewType)
        {
            case ViewType.TYPE_EXP_RANK:
                PlayerRankListGroup.SetActive(true);

                ZWebManager.Instance.WebGame.REQ_GetExpRankList((uint)selectClassType, (recvPacket, recvPacketMsg) =>
                {
                    bool bFindMyRank = false;
                    List<RankingUser> rankList = new List<RankingUser>();

                    for (int i = 0; i < recvPacketMsg.RanksLength; i++)
                    {
                        var rankdata = recvPacketMsg.Ranks(i).Value;

                        if (Me.CurCharData.ID == rankdata.CharId)
                        {
                            // 랭킹안에 나의 정보가 있는경우.
                            myRankSlot.gameObject.SetActive(true);
                            SetUserSlot(new RankingUser(rankdata));
                            bFindMyRank = true;
                        }

                        rankList.Add(new RankingUser(rankdata));
                    }

                    // 랭크 정렬
                    rankList.Sort((x, y) => x.Rank.CompareTo(y.Rank));

                    if (!bFindMyRank)
                    {
                        // 랭킹안에 나의 정보가 없는경우.
                        myRankSlot.gameObject.SetActive(true);
                        SetUserSlot(new RankingUser() 
                        { 
                            CharId = Me.CurCharData.ID,
                            CharTid = Me.CurCharData.TID,
                            Score = Me.CurCharData.PkCnt,
                            Nick = Me.CurCharData.Nickname,
                            GuildId = Me.CurCharData.GuildId,
                            GuildName = Me.CurCharData.GuildName,
                            GuildMarkTid = Me.CurCharData.GuildMarkTid 
                        });
                    }

                    ScrollAdapter.SetScrollData(selectClassType, rankList);
                });
                break;
            case ViewType.TYPE_PK_RANK:
                PvpRankListGroup.SetActive(true);

                ZWebManager.Instance.WebGame.REQ_GetPKRankList((recvPacket, recvPacketMsg) =>
                {
                    bool bFindMyRank = false;
                    List<RankingUser> rankPKList = new List<RankingUser>();

                    for (int i = 0; i < recvPacketMsg.RanksLength; i++)
                    {
                        var rankdata = recvPacketMsg.Ranks(i).Value;

                        if (Me.CurCharData.ID == rankdata.CharId)
                        {
                            // 랭킹안에 나의 정보가 있는경우.
                            // 따로 프리팹이 없는데 물어볼것.
                            bFindMyRank = true;
                        }

                        rankPKList.Add(new RankingUser(rankdata));
                    }

                    // 랭크 정렬
                    rankPKList.Sort((x, y) => x.Rank.CompareTo(y.Rank));

                    if (!bFindMyRank)
                    {
                        // 랭킹안에 나의 정보가 없는경우.
                        // 따로 프리팹이 없는데 물어볼것.
                    }

                    PvpScrollAdapter.SetScrollData(rankPKList);
                });
                break;
            case ViewType.TYPE_PK_HISTORY:
                PvpLogListGroup.SetActive(true);

                ZWebManager.Instance.WebGame.REQ_GetPVPHistory((recvPacket, recvPacketMsg) =>
                {
                    List<PkLogData> rankLogList = new List<PkLogData>();

                    for (int i = 0; i < recvPacketMsg.PkLogsLength; i++)
                    {
                        var logdata = recvPacketMsg.PkLogs(i).Value;

                        rankLogList.Add(new PkLogData(logdata));
                    }
                    PvpLogScrollAdapter.SetScrollData(rankLogList);
                });
                break;
        }
    }

    private void SetUserSlot(RankingUser _userData)
    {
        if (_userData == null)
        {
            RankIcon.gameObject.SetActive(false);
            ClassIcon.gameObject.SetActive(false);
            GuildIcon.gameObject.SetActive(false);
            RankText.text = string.Empty;
            PlayerName.text = string.Empty;
            GuildName.text = string.Empty;
            return;
        }

        // 랭킹, 랭킹 이미지
        RankText.text = _userData.Rank == 0 ? "-" : _userData.Rank.ToString();
        RankIcon.gameObject.SetActive(_userData.Rank <= 3 && _userData.Rank > 0);
        if (_userData.Rank <= 3 && _userData.Rank > 0)
        {
            RankText.text = "";
            switch (_userData.Rank)
            {
                case 1:
                    RankIcon.sprite = ZManagerUIPreset.Instance.GetSprite("img_txt_rank_1");
                    break;
                case 2:
                    RankIcon.sprite = ZManagerUIPreset.Instance.GetSprite("img_txt_rank_2");
                    break;
                case 3:
                    RankIcon.sprite = ZManagerUIPreset.Instance.GetSprite("img_txt_rank_3");
                    break;
            }
        }

        // 클래스이미지, 유저 이름
        ClassIcon.sprite = ZManagerUIPreset.Instance.GetSprite(DBCharacter.GetClassIconName(_userData.CharTid));
        PlayerName.text = _userData.Nick;
        ClassIcon.gameObject.SetActive(true);

        // 길드이미지, 길드 이름
        if (_userData.GuildId != 0)
        {
            GuildName.text = _userData.GuildName;
            GuildIcon.sprite = ZManagerUIPreset.Instance.GetSprite(DBGuild.GetGuildMark(_userData.GuildMarkTid));
            GuildIcon.gameObject.SetActive(true);
        }
        else
        {
            GuildIcon.gameObject.SetActive(false);
            GuildName.text = "-";
        }

        // 버프
        BufferIcon01.gameObject.SetActive(false);
        BufferIcon02.gameObject.SetActive(false);
        BufferIcon03.gameObject.SetActive(false);

        if (selectClassType == GameDB.E_CharacterType.All)
            rewardBuffData = DBRank.GetExpRank(DBCharacter.GetClassTypeByTid(_userData.CharTid), _userData.Rank);
        else
            rewardBuffData = DBRank.GetExpJobRank(DBCharacter.GetClassTypeByTid(_userData.CharTid), _userData.Rank);

        if (rewardBuffData != null)
        {
            if (rewardBuffData.AbilityActionID_01 != 0)
            {
                BufferIcon01.sprite = ZManagerUIPreset.Instance.GetSprite(DBAbility.GetActionIcon(rewardBuffData.AbilityActionID_01));
                BufferIcon01.gameObject.SetActive(true);
            }

            if (rewardBuffData.AbilityActionID_02 != 0)
            {
                BufferIcon02.sprite = ZManagerUIPreset.Instance.GetSprite(DBAbility.GetActionIcon(rewardBuffData.AbilityActionID_02));
                BufferIcon02.gameObject.SetActive(true);
            }
        }
    }

    public void OnClickMyslotBufferInfoPopup(int _idx)
    {
        uint AbilityActionTid = 0;

        switch (_idx)
        {
            case 0:
                AbilityActionTid = rewardBuffData.AbilityActionID_01;
                break;
            case 1:
                AbilityActionTid = rewardBuffData.AbilityActionID_02;
                break;
        }

        string strTitle = "";
        string strDesc = "";
        strTitle = string.Format("{0}", DBLocale.GetText(DBAbility.GetActionName(AbilityActionTid)));
        strDesc += DBAbility.ParseAbilityActions(" ", "\n", AbilityActionTid);

        OpenBufferInfoPopup(ZManagerUIPreset.Instance.GetSprite(DBAbility.GetActionIcon(AbilityActionTid)), strTitle, strDesc);
    }

    public void OpenBufferInfoPopup(Sprite _sprite, string _strTitle, string _strDesc)
    {
        BufferInfoPopupIcon.sprite = _sprite;
        BufferInfoPopupTitle.text = _strTitle;
        BufferInfoPopupDesc.text = _strDesc;
        RankingBufferInfoPopup.SetActive(true);
    }

    public void CloseBufferInfoPopup()
    {
        RankingBufferInfoPopup.SetActive(false);
    }

    public void Close()
    {
        UIManager.Instance.Close<UIFrameRank>(true);
    }
}
