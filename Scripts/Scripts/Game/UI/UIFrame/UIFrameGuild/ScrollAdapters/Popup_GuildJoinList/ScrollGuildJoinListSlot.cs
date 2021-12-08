using System;
using UnityEngine;
using UnityEngine.UI;

public class ScrollGuildJoinListSlot : MonoBehaviour
{
    #region SerializedField
    #region Preference Variable
    #endregion

    #region UI Variables
    [SerializeField] private Image imgIcon;
    [SerializeField] private Text txtLevel;
    [SerializeField] private Text txtName;
    [SerializeField] private Text txtComment;
    #endregion
    #endregion

    #region System Variables
    private Action OnClickedApprove;
    private Action OnClickedReject;
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
        , uint level
        , string name
        , string comment)
    {
        imgIcon.sprite = iconSprite;
        txtLevel.text = level.ToString();
        txtName.text = name;
        txtComment.text = comment;
    }

    public void AddListener_OnClickApprove(Action callback)
    {
        OnClickedApprove += callback;
    }

    public void AddListener_OnClickReject(Action callback)
    {
        OnClickedReject += callback;
    }
    #endregion

    #region Private Methods
    public void OnClickApprove()
    {
        OnClickedApprove?.Invoke();
    }

    public void OnClickReject()
    {
        OnClickedReject?.Invoke();
    }
    #endregion
}
