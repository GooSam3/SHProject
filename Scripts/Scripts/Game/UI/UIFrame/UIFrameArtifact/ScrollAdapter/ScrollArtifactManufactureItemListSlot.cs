using System;
using UnityEngine;
using UnityEngine.UI;

public class ScrollArtifactManufactureItemListSlot : MonoBehaviour
{
    #region SerializedField
    #region Preference Variable
    [Header("해당 슬롯이 비활성화되었을때 사용할 CanvasGroup Alpha 값")]
    [SerializeField] private float InactiveSlotCanvasAlpha = 0.25f;
    #endregion

    #region UI Variables
    [SerializeField] private CanvasGroup canvasGroup;

    [SerializeField] private RectTransform equippedIndicator;

    [SerializeField] private RectTransform selectedImgObj;
    [SerializeField] private RectTransform lockObj;

    [SerializeField] private Image imgItemIcon;
    [SerializeField] private Image imgBlock;

    [SerializeField] private Image imgGradeBoard;

    // 쿨타임 
    [SerializeField] private RectTransform coolTimeObj;
    [SerializeField] private Slider sliderCoolTime;
    [SerializeField] private Text txtCoolTimeCount;

    [SerializeField] private RectTransform internalItemSelectedObj;

    [SerializeField] private RectTransform redDotObj;

    [SerializeField] private RectTransform internalItemDeactiveCover;

    [SerializeField] private Text txtArtifactName;
    [SerializeField] private Text txtArtifactItemName;
    #endregion
    #endregion

    #region System Variables
    private Action _onClicked;
    #endregion

    #region Properties 
    #endregion

    #region Unity Methods
    #endregion

    #region Overrides 
    #endregion

    #region Public Methods
    public void SetData(
        Sprite itemSprite
        , Sprite gradeSprite
        , string artifactItemName
        , Color artifactNameColor
        , bool isSelected
        , bool isObtained
        , bool isEquipped)
    {
        Set(
            itemSprite: itemSprite
            , gradeSprite
            , artifactItemName: artifactItemName
            , isSlotSelected: isSelected
            , isObtained: isObtained
            , isEquipped: isEquipped
            , artifactItemNameColor: artifactNameColor
            , isBlocked: isObtained == false);
    }

    public void AddListener_OnClicked(Action callback)
    {
        _onClicked += callback;
    }

    public void RemoveListener_OnClicked(Action callback)
    {
        _onClicked -= callback;
    }

    #endregion

    #region Private Methods
    private void Set(
        Sprite itemSprite
        , Sprite gradeSprite
        , string artifactName = null
        , string artifactItemName = null
        , Color? artifactNameColor = null
        , Color? artifactItemNameColor = null
        // , float? canvasGroupAlpha
        , float? coolTime = null
        , float? coolTimeMax = null
        , bool? isSlotSelected = null
        , bool? isInternalItemSelected = null
        , bool? isEquipped = null
        , bool? isObtained = null
        , bool? isBlocked = null
        , bool? isRedDotOn = null
        , bool? isInternalItemInactive = null)
    {
        imgItemIcon.sprite = itemSprite;
        imgGradeBoard.sprite = gradeSprite;

        if (artifactName != null)
        {
            txtArtifactName.text = artifactName;
        }

        if (artifactNameColor.HasValue)
        {
            txtArtifactName.color = artifactNameColor.Value;
        }

        if (artifactItemName != null)
        {
            txtArtifactItemName.text = artifactItemName;
        }

        if (artifactItemNameColor.HasValue)
        {
            txtArtifactItemName.color = artifactItemNameColor.Value;
        }

        if (coolTime.HasValue && coolTimeMax.HasValue)
        {
            coolTimeObj.gameObject.SetActive(coolTime.Value > 0);

            if (coolTimeObj.gameObject.activeSelf)
            {
                sliderCoolTime.value = coolTime.Value / coolTimeMax.Value;
                txtCoolTimeCount.text = ((int)coolTime).ToString();
            }
        }

        if (isSlotSelected.HasValue)
        {
            selectedImgObj.gameObject.SetActive(isSlotSelected.Value);
        }

        if (isInternalItemSelected.HasValue)
        {
            internalItemSelectedObj.gameObject.SetActive(isInternalItemSelected.Value);
        }

        if (isEquipped.HasValue)
        {
            equippedIndicator.gameObject.SetActive(isEquipped.Value);
        }

        if (isObtained.HasValue)
        {
            canvasGroup.alpha = isObtained.Value ? 1f : InactiveSlotCanvasAlpha;
        }

        if (isBlocked.HasValue)
        {
            imgBlock.gameObject.SetActive(isBlocked.Value);
        }

        if (isRedDotOn.HasValue)
        {
            redDotObj.gameObject.SetActive(isRedDotOn.Value);
        }

        if (isInternalItemInactive.HasValue)
        {
            internalItemDeactiveCover.gameObject.SetActive(isInternalItemInactive.Value);
        }
    }
    #endregion

    #region OnClick Event (인스펙터 연결)
    public void OnClicked()
    {
        _onClicked?.Invoke();
    }
    #endregion
}
