using GameDB;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using ZNet.Data;

public class UIFrameGuildPopup_CreateGuild : UIFrameGuildOverlayPopupBase
{
    [Serializable]
    public class TypingInputField
    {
        public InputField inputField;
        public Text txtLength;
    }

    #region SerializedField
    #region Preference Variable
    #endregion

    #region UI Variables
    [SerializeField] private ScrollGuildCreateGridAdapter ScrollAdapter;

    [SerializeField] private Image imgCostItem;
    [SerializeField] private Text txtCostNum;

    [SerializeField] private ZToggle isQuickJoinToggle;

    [SerializeField] private TypingInputField nameInputField;
    [SerializeField] private TypingInputField introductionInputField;
    [SerializeField] private TypingInputField informationInputField;
    #endregion
    #endregion

    #region System Variables
    private byte selectedMarkTid;
    private bool ignoreClickEvents;
    #endregion

    #region Properties 
    #endregion

    #region Unity Methods
    #endregion

    #region Public Methods
    public override void Initialize(UIFrameGuild guildController)
    {
        base.Initialize(guildController);

        var slot = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(UIGuildCreateGuildSlot));
        ScrollAdapter.Parameters.Grid.CellPrefab = slot.GetComponent<RectTransform>();
        var pf = ScrollAdapter.Parameters.Grid.CellPrefab;
        pf.SetParent(transform);
        pf.localScale = Vector2.one;
        pf.localPosition = Vector3.zero;
        pf.gameObject.SetActive(false);

        Item_Table costItemData = null;

        // 소모 아이템 세팅 
        DBItem.GetItem(DBConfig.Guild_FoundingID, out costItemData);

        if (costItemData != null)
        {
            imgCostItem.sprite = ZManagerUIPreset.Instance.GetSprite(costItemData.IconID);
        }
        else
        {
            imgCostItem.sprite = null;
        }

        txtCostNum.text = DBConfig.Guild_FoundingCount.ToString("n0");

        ScrollAdapter.onClickedSlot = OnClickedMarkSlot;

        nameInputField.inputField.characterLimit = (int)DBConfig.GuildName_Length_Max;
        introductionInputField.inputField.characterLimit = (int)DBConfig.GuildIntroduce_Length_Max;
        informationInputField.inputField.characterLimit = (int)DBConfig.GuildNotice_Length_Max;

        SetInputFieldTxtLengthCount(nameInputField.txtLength, 0, nameInputField.inputField.characterLimit);
        SetInputFieldTxtLengthCount(informationInputField.txtLength, 0, informationInputField.inputField.characterLimit);
        SetInputFieldTxtLengthCount(introductionInputField.txtLength, 0, introductionInputField.inputField.characterLimit);

        nameInputField.inputField.onValueChanged.AddListener(OnNameInputChanged);
        introductionInputField.inputField.onValueChanged.AddListener(OnIntroductionInputChanged);
        informationInputField.inputField.onValueChanged.AddListener(OnInformationInputChanged);

        ScrollAdapter.Initialize();
    }

    public override void Open()
    {
        base.Open();

        ignoreClickEvents = false;

        var firstData = DBGuild.GetGuildMarkFirstData();

        if (firstData != null)
        {
            selectedMarkTid = firstData.GuildMarkID;
        }
        else
        {
            selectedMarkTid = 0;
        }

        nameInputField.inputField.text = string.Empty;
        introductionInputField.inputField.text = string.Empty;
        informationInputField.inputField.text = string.Empty;

        isQuickJoinToggle.SelectToggle();

        Refresh();
        ScrollAdapter.SetNormalizedPosition(1);
    }

    public override void Close()
    {
        isQuickJoinToggle.SelectToggle();
        base.Close();
    }
    #endregion

    #region Private Methods
    void Refresh()
    {
        ScrollAdapter.SetSelectedMarkID(selectedMarkTid, false);
        ScrollAdapter.RefreshData();
    }

    void SetInputFieldTxtLengthCount(Text txt, int cur, int max)
    {
        txt.text = string.Format("{0}/{1}", cur, max);
    }

    //bool CheckCurrency(uint useItemTID)
    //{
    //    return ZNet.Data.Me.GetCurrency(useItemTID) >= DBConfig.Guild_FoundingCount;
    //}

    private void OnNameInputChanged(string arg0)
    {
        SetInputFieldTxtLengthCount(nameInputField.txtLength, arg0.Length, nameInputField.inputField.characterLimit);
    }

    private void OnInformationInputChanged(string arg0)
    {
        SetInputFieldTxtLengthCount(informationInputField.txtLength, arg0.Length, informationInputField.inputField.characterLimit);
    }

    private void OnIntroductionInputChanged(string arg0)
    {
        SetInputFieldTxtLengthCount(introductionInputField.txtLength, arg0.Length, introductionInputField.inputField.characterLimit);
    }

    private void OnClickedMarkSlot(ScrollGuildCreateGuildGridHolder holder)
    {
        selectedMarkTid = holder.Data.tid;
        ScrollAdapter.SetSelectedMarkID(selectedMarkTid, true);
    }

    #endregion

    #region Insepctor Events (인스펙터 연결 이벤트)
    public void OnNoticeInputUpdated()
    {
        int spaceCount = informationInputField.inputField.text.Count(
            (c) =>
            {
                return c == '\n';
            });

        if (spaceCount > 4)
        {
            informationInputField.inputField.text = informationInputField.inputField.text.TrimEnd('\n');
        }
    }

    public void OnClickCreateBtn()
    {
        if (ignoreClickEvents)
            return;

        string guildName = nameInputField.inputField.text;
        string introduction = introductionInputField.inputField.text;
        string information = informationInputField.inputField.text;
        bool isQuickJoin = isQuickJoinToggle.isOn;
        var zitem = Me.CurCharData.GetInvenItemUsingMaterial(DBConfig.Guild_FoundingID);

        // TODO : LOCALE

        // 재화 부족 
        if (ConditionHelper.CheckCompareCost(DBConfig.Guild_FoundingID, DBConfig.Guild_FoundingCount) == false)
        {
            // UIFrameGuildTabBase.OpenNotiUp(content: "길드 창설에 필요한 재화가 부족합니다", title: "알림");
            return;
        }
        // 레벨 부족 
        else if (Me.CurUserData.MaxLevel < DBConfig.Guild_Application_Level)
        {
            UIFrameGuildTabBase.OpenNotiUp(string.Format("레벨이 부족합니다. (길드 창설 가능레벨 {0})", DBConfig.Guild_Application_Level));
            return;
        }
        // 길드 이름 짧음 
        else if (nameInputField.inputField.text.Length < DBConfig.GuildName_Length_Min)
        {
            UIFrameGuildTabBase.OpenNotiUp(string.Format("길드 이름은 최소 {0} 자여야 합니다.", DBConfig.GuildName_Length_Min));
            return;
        }
        // 길드 마크 찾을수없음 
        else if (DBGuild.IsGuildMarkExist(selectedMarkTid) == false)
        {
            UIFrameGuildTabBase.OpenNotiUp("길드 마크를 다시 선택해주세요.");
            return;
        }
        // 해당 길드마크는 레벨이 부족해 선택할수가 없음 
        else if (DBGuild.GetGuildMarkOpenLevel(selectedMarkTid) > 1)
        {
            UIFrameGuildTabBase.OpenNotiUp("길드 마크를 다시 선택해주세요.");
			return;
		}
        // 공지글이 너무 짧음 
		else if (informationInputField.inputField.text.Length < 2)
		{
            UIFrameGuildTabBase.OpenNotiUp("길드 공지글이 너무 짧습니다.");
            return;
        }
        // 소개글이 너무 짧음 
		else if (introductionInputField.inputField.text.Length < 2)
		{
			UIFrameGuildTabBase.OpenNotiUp("길드 소개글이 너무 짧습니다.");
            return; 
        }

		UIFrameGuildTabBase.OpenTwoButtonQueryPopUp("확인", "길드를 창설하시겠습니까?"
            , onConfirmed: () =>
            {
#if _GTEST_
        GuildController.NotifyUpdateEvent(UpdateEventType.JoinedGuildOrCreated);
#else
                ignoreClickEvents = true;

                UIFrameGuildNetCapturer.ReqCreateGuild(
                    guildName
                    , introduction
                    , information
                    , selectedMarkTid
                    , zitem.item_id
                    , isQuickJoin
                    , (revPacket, resList) =>
                    {
                        ignoreClickEvents = false;
                        GuildController.NotifyUpdateEvent(UpdateEventType.JoinedGuildOrCreated,
                            new GuildDataUpdateEventParamBase() { skipGetGuildInfo = true });
                    },
                    (error, req, res) =>
                    {
                        ignoreClickEvents = false;
                        UIFrameGuildTabBase.HandleError(error, req, res);
                    });
#endif
            }
            , onCanceled: () =>
            {
            });
    }
    #endregion
}
