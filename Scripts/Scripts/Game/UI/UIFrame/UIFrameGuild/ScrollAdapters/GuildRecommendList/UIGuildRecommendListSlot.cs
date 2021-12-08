using UnityEngine;
using UnityEngine.UI;
using static UIFrameGuildTab_GuildSuggestion;

public class UIGuildRecommendListSlot : MonoBehaviour
{
    #region SerializedField
    #region Preference Variable
    #endregion

    #region UI Variables
    [SerializeField] private Image imgIcon;
    [SerializeField] private Text txtLevel;
    [SerializeField] private Text txtName;
    [SerializeField] private Text txtMemberCnt;
    [SerializeField] private Text txtMasterName;
    [SerializeField] private Text txtIntroduction;

    [SerializeField] private GameObject quickJoinObj;
    [SerializeField] private GameObject approveJoinObj;
    #endregion
    #endregion

    #region System Variables
    private ulong GuildID;
    private bool isQuickJoin;
    // GuildID(길드ID) , Comment(남기는말) , IsQuickJoin(바로가입여부)
    private OnRequestJoin OnRequestJoin;
    #endregion

    #region Properties 
    #endregion

    #region Unity Methods
    #endregion

    #region Overrides 
    #endregion

    #region Public Methods
    public void SetData(
        ulong guildID
        , Sprite iconSprite
        , uint level
        , string name
        , uint curMemberCnt
        , uint maxMemberCnt
       , string masterName
        , string introduction
        , bool quickJoin)
    {
        GuildID = guildID;
        imgIcon.sprite = iconSprite;
        txtLevel.text = level.ToString();
        txtName.text = name;
        txtMemberCnt.text = string.Format("{0}/{1}", curMemberCnt, maxMemberCnt);
        txtMasterName.text = masterName;
        txtIntroduction.text = introduction;
        quickJoinObj.SetActive(quickJoin);
        approveJoinObj.SetActive(quickJoin == false);
        isQuickJoin = quickJoin;
    }

    public void AddListener_OnRequestJoin(OnRequestJoin callback)
    {
        OnRequestJoin += callback;
    }
    #endregion

    #region Private Methods
    #endregion

    #region OnClick Event (인스펙터 연결)
    public void OnClickJoinRequestBtn()
    { 
        UIMessagePopup.ShowInputPopup("알림", "코멘트를 남겨주세요.", (comment) =>
        {
            OnRequestJoin?.Invoke(
                GuildID
                , comment
                , isQuickJoin);
        }, null, (int)DBConfig.GuildDelivery_Length_Max);
    }
    #endregion
}
