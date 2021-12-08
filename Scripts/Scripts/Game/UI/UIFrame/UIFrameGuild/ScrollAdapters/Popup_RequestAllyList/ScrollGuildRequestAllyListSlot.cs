using System;
using UnityEngine;
using UnityEngine.UI;

public class ScrollGuildRequestAllyListSlot : MonoBehaviour
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
    #endregion
    #endregion

    #region System Variables
    private Action onClickedReject;
    private Action onClickedAccept;
    #endregion

    #region Properties 
    #endregion

    #region Unity Methods
    #endregion

    #region Overrides 
    #endregion

    #region Public Methods
    public void AddListener_OnClickedReject(Action callback)
    {
        onClickedReject += callback;
    }

    public void AddListener_OnClickedAccept(Action callback)
    {
        onClickedAccept += callback;
    }

    public void SetData(
        Sprite iconSprite
        , uint level
        , string name
        , uint curMemberCnt
        , uint maxMemberCnt
       , string masterName)
    {
        imgIcon.sprite = iconSprite;
        txtLevel.text = string.Format("Lv.{0}", level);
        txtName.text = name;
        txtMemberCnt.text = string.Format("{0}/{1}", curMemberCnt, maxMemberCnt);
        txtMasterName.text = masterName;
    }
    #endregion

    #region Private Methods
    public void OnClickedReject()
    {
        onClickedReject?.Invoke();
    }

    public void OnClickedAccept()
    {
        onClickedAccept?.Invoke();
    }
    #endregion
}
