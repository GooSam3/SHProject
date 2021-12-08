using System;
using UnityEngine;
using UnityEngine.UI;

public class UIGuildCreateGuildSlot : MonoBehaviour
{
    #region SerializedField
    #region Preference Variable
    #endregion

    #region UI Variables
    [SerializeField] private Image imgIcon;
    [SerializeField] private Image imgSelected;
    [SerializeField] private Button btn;
    [SerializeField] private GameObject activeOnLockedObj;
    [SerializeField] private Text txtOpenLevel;

    [SerializeField] private Color colorNotSelected;
    #endregion
    #endregion

    #region System Variables
    Action onClicked;
    #endregion

    #region Properties 
    #endregion

    #region Unity Methods
    #endregion

    #region Overrides 
    #endregion

    #region Public Methods
    public void SetOnClickHandler(Action callback)
    {
        onClicked = callback;
    }

    public void UpdateUI(Sprite iconSprite, bool isLocked, uint openLevel)
    {
        activeOnLockedObj.SetActive(isLocked);
        if (isLocked)
        {
            txtOpenLevel.text = string.Format("길드레벨 {0}", openLevel);
        }
        imgIcon.sprite = iconSprite;
    }

    public void SetSelect(bool isLocked, bool select, bool hasBeenSelected)
    {
        if (isLocked)
        {
            imgSelected.gameObject.SetActive(false);

            if (select || hasBeenSelected)
            {
                imgIcon.color = Color.white;
            }
            else if (hasBeenSelected == false)
            {
                imgIcon.color = colorNotSelected;
            }
        }
        else
        {
            imgSelected.gameObject.SetActive(select);

            if (select)
            {
                imgIcon.color = Color.white;
            }
            else
            {
                imgIcon.color = colorNotSelected;
            }
        }
    }
    #endregion

    #region Private Methods
    #endregion

    #region OnClick Event (인스펙터 연결)
    public void OnClicked()
    {
        onClicked?.Invoke();
    }
    #endregion
}
