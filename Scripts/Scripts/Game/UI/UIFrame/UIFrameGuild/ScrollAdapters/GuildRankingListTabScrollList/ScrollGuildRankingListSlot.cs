using UnityEngine;
using UnityEngine.UI;

public class ScrollGuildRankingListSlot : MonoBehaviour
{
    #region SerializedField
    #region Preference Variable
    #endregion

    #region UI Variables
    [SerializeField] private Image imgRanking;
    [SerializeField] private Image imgIcon;
    [SerializeField] private Text txtLevel;
    [SerializeField] private Text txtRanking;
    [SerializeField] private Text txtName;
    [SerializeField] private Text txtMemberCnt;
    [SerializeField] private Text txtMasterName;
    [SerializeField] private Text txtNotice;
    #endregion
    #endregion

    #region System Variables
    #endregion

    #region Properties 
    #endregion

    #region Unity Methods
    #endregion

    #region Overrides 
    #endregion

    #region Public Methods
    public void SetData(
        Sprite iconSprite
        , Sprite rankingSprite
        , uint rank
        , uint level
        , string name
        , uint curMemberCnt
        , uint maxMemberCnt
       , string masterName
        , string notice)
    {
        imgRanking.gameObject.SetActive(rankingSprite != null);
        txtRanking.gameObject.SetActive(rankingSprite == null);

        if (rankingSprite != null)
            imgRanking.sprite = rankingSprite;

        imgIcon.sprite = iconSprite;
        txtLevel.text = level.ToString();
        txtRanking.text = rank > 0 ? rank.ToString() : DBLocale.GetText("No_Rank_Text");
        txtName.text = name;
        txtMemberCnt.text = string.Format("{0}/{1}", curMemberCnt, maxMemberCnt);
        txtMasterName.text = masterName;
        txtNotice.text = notice;
    }
    #endregion

    #region Private Methods
    #endregion
}
