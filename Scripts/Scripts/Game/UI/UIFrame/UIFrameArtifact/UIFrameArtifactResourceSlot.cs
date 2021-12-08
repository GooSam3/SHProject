using GameDB;
using System;
using UnityEngine;
using UnityEngine.UI;
using static UIFrameArtifactMaterialManagementBase;

public class UIFrameArtifactResourceSlot : MonoBehaviour
{
    #region SerializedField
    #region Preference Variable
    #endregion

    #region UI 
    [SerializeField] private RectTransform emptyContentRoot;
    [SerializeField] private RectTransform displayContentRoot;

    [SerializeField] private Image imgElement;
    [SerializeField] private Image imgClassIcon;
    [SerializeField] private Image imgMaterialIcon;
    [SerializeField] private Image imgGrade;
    [SerializeField] private RectTransform dropShadowObj;
    [SerializeField] private RectTransform selectedObj;
    [SerializeField] private RectTransform lockObj;
    [SerializeField] private RectTransform stateIndicatorObj;
    [SerializeField] private Text txtState;
    [SerializeField] private Text txtLevel;
    [SerializeField] private Text txtCnt;
    [SerializeField] private RectTransform checkmarkObj;
    [SerializeField] private RectTransform noGainLockObj;
    #endregion
    #endregion

    #region System Variables
    /// <summary> 해당 슬롯 인덱스, 세팅된 재료의 타입 , 등급 (Grade) </summary>
    private Action OnClickedCallback;

    private int Index;
    /// <summary> 재료의 타입 (현 시점 : Pet, Vehicle)  </summary>
    private E_ArtifactMaterialType MaterialType;
    /// <summary>
    /// 재료의 ID (재료의 타입에 따라서 다른 테이블 참조함 . 
    /// 타입이 Pet 이면 Pet_Table 에서 PetType 을 Pet 으로 검색 
    /// Vehicle 이면 Vehicle_Table 에서 PetType 을 Vehicle 으로 검색 
    /// </summary>
    private uint MaterialGrade;

    private bool showCount;
    #endregion

    #region Properties 
    #endregion

    #region Unity Methods
    #endregion

    #region Public Methods
    public void ClearSlot(Sprite gradeBG)
    {
        SetOptional(
            setAllSpriteWhite: true
            , isContentEmpty: true
            , gradeBG: gradeBG
            , isSelected: false
            , dropShadow: false
            , checkMark: false
            , lockObj: false
            , stateIndicator: false
            , noGainLock: false
            , gradeImg: true
            , levelObj: false
            , countObj: false
            , stateText: string.Empty
            , level: "0"
            , count: "0");
    }

    public void SetUI(int index, bool showCount, Sprite gradeBG, MaterialDataOnSlot data)
    {
        Index = index;
        MaterialType = data.matType;
        MaterialGrade = data.matGrade;

        var petData = DBPet.GetPetData(data.tid);

        if (petData == null)
        {
            ClearSlot(gradeBG);
        }
        else
        {
            Color gradeColor;
            string colorStr = "#" + DBUIResouce.GetGradeTextColor(E_UIType.Item, data.matGrade);

            if (ColorUtility.TryParseHtmlString(colorStr, out gradeColor) == false)
            {
                ZLog.LogError(ZLogChannel.UI, "Color parsing error");
                gradeColor = Color.white;
            }

            gradeColor.a = 1;

            SetUI(
                ZManagerUIPreset.Instance.GetSprite(petData.Icon)
                , gradeBG
                , false
                , showCount
                , data.isChecked
                , data.cntByContext);
            //                , DBUIResouce.GetTierText(data.matGrade)
            //             , UIFrameArtifact.GetColorByGrade(data.matGrade));
        }
    }

    public void SetUI(
        Sprite materialSprite
        , Sprite gradeBG
        , bool isSlotEmpty
        , bool showCount
        , bool showCheck
        , uint cnt)
    {
        SetOptional(
            materialIconSprite: materialSprite
            , isContentEmpty: isSlotEmpty
            , gradeImg: true
            , gradeBG: gradeBG
            , checkMark: showCheck
            , countObj: showCount
            , count: cnt.ToString());
    }

    public void AddListener_OnClicked(Action callback)
    {
        this.OnClickedCallback += callback;
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Null 이면 해당 데이터는 세팅하지 않는것으로 간주 및 처리함 
    /// </summary>
    private void SetOptional(
        // 스프라이트 전부 NULL 처리 여부 
        // 가장 처음에 처리함으로서 clear 개념임 
        bool? setAllSpriteWhite = false

        , Sprite elementSprite = null
        , Sprite classIconSprite = null
        , Sprite materialIconSprite = null
        , Sprite gradeBG = null
        , bool? isContentEmpty = null
        , bool? isSelected = null
        , bool? dropShadow = null
        , bool? checkMark = null
        , bool? lockObj = null
        , bool? stateIndicator = null
        , bool? noGainLock = null
        , bool? gradeImg = null
        , bool? levelObj = null
        , bool? countObj = null
        , string stateText = null
        , string level = null
        , string count = null)
    {
        if (setAllSpriteWhite.HasValue)
        {
            imgElement.sprite = null;
            imgClassIcon.sprite = null;
            imgMaterialIcon.sprite = null;
        }

        if (elementSprite != null)
            imgElement.sprite = elementSprite;
        if (classIconSprite != null)
            imgClassIcon.sprite = classIconSprite;
        if (materialIconSprite != null)
            imgMaterialIcon.sprite = materialIconSprite;
        if (gradeBG != null)
            imgGrade.sprite = gradeBG;
        if (isContentEmpty.HasValue)
        {
            displayContentRoot.gameObject.SetActive(isContentEmpty.Value == false);
            emptyContentRoot.gameObject.SetActive(isContentEmpty.Value);
            // imgGrade.gameObject.SetActive(isContentEmpty.Value == false);
        }

        if (isSelected.HasValue)
            selectedObj.gameObject.SetActive(isSelected.Value);
        if (dropShadow.HasValue)
            dropShadowObj.gameObject.SetActive(dropShadow.Value);
        if (checkMark.HasValue)
            checkmarkObj.gameObject.SetActive(checkMark.Value);
        if (lockObj.HasValue)
            this.lockObj.gameObject.SetActive(lockObj.Value);
        if (stateIndicator.HasValue)
            stateIndicatorObj.gameObject.SetActive(stateIndicator.Value);
        if (noGainLock.HasValue)
            noGainLockObj.gameObject.SetActive(noGainLock.Value);
        if (gradeImg.HasValue)
            imgGrade.gameObject.SetActive(gradeImg.Value);
        if (stateIndicator.HasValue)
            stateIndicatorObj.gameObject.SetActive(stateIndicator.Value);
        if (stateText != null)
            txtState.text = stateText;
        if (levelObj.HasValue)
            txtLevel.gameObject.SetActive(levelObj.Value);
        if (level != null)
            txtLevel.text = level;
        if (countObj.HasValue)
            txtCnt.gameObject.SetActive(countObj.Value);
        if (count != null)
            txtCnt.text = count;
    }
    #region Common
    #endregion
    #endregion

    #region Inspector Events 
    public void OnClicked()
    {
        OnClickedCallback?.Invoke();//  Index, MaterialType, MaterialGrade);
    }
    #endregion
}
