using System;
using UnityEngine;
using UnityEngine.UI;

public class ScrollArtifactLinkListSlot : MonoBehaviour
{
    /// <summary>
    /// active,inactive 공통 ui 그룹 
    /// </summary>
    [Serializable]
    public class CommonGroup
    {
        [Serializable]
        public class SingleSlot
        {
            [HideInInspector]
            public uint artifactID;
            public Image imgIcon;
            public Image imgGradeBoard;
        }

        public GameObject rootObj;

        public Text txtName;
        public Text txtCnt;

        public SingleSlot leftSlot;
        public SingleSlot rightSlot;
    }

    #region SerializedField
    #region Preference Variable
    #endregion

    #region UI Variables
    [SerializeField] private CommonGroup activeObj;
    [SerializeField] private CommonGroup inactiveObj;

    [SerializeField] private Image imgSelected;
    [SerializeField] private Color disableColor_Grade;
    [SerializeField] private Color disableColor_Icon;
    #endregion
    #endregion

    #region System Variables
    private Action _onClicked;
    /// <summary>
    ///  uint : TID 
    /// </summary>
    private Action<uint> _onClickedLeftMaterial;
    private Action<uint> _onClickedRightMaterial;

    private CommonGroup currentGroup;
    #endregion

    #region Properties 
    #endregion

    #region Unity Methods
    #endregion

    #region Overrides 
    #endregion

    #region Public Methods
    public void SetUI(
        uint leftMatTid
        , uint rightMatTid
        , Sprite leftArtifactSprite
        , Sprite leftArtifactGradeSprite
        , Sprite rightArtifactSprite
        , Sprite rightArtifactGradeSprite
        , Color linkTitleColor
        , string linkName
        , string cntStr
        , bool isLinkObtained
        , bool isLeftArtifactObtained
        , bool isRightArtifactObtained
        , bool isSelected)
    {
        activeObj.rootObj.SetActive(isLinkObtained);
        inactiveObj.rootObj.SetActive(isLinkObtained == false);

        currentGroup = isLinkObtained ? activeObj : inactiveObj;

        CommonGroup target = isLinkObtained ? activeObj : inactiveObj;

        target.txtName.text = linkName;
        target.txtName.color = linkTitleColor;
        target.txtCnt.gameObject.SetActive(isLinkObtained == false);

        if (isLinkObtained == false)
        {
            target.txtCnt.text = cntStr;
        }

        Color gradeColorByActive = isLeftArtifactObtained ? Color.white : disableColor_Grade;
        Color iconColorByActive = isLeftArtifactObtained ? Color.white : disableColor_Icon;

        SetArtifactSlot(leftMatTid, target.leftSlot, leftArtifactSprite, leftArtifactGradeSprite, iconColorByActive, gradeColorByActive);

        gradeColorByActive = isRightArtifactObtained ? Color.white : disableColor_Grade;
        iconColorByActive = isRightArtifactObtained ? Color.white : disableColor_Icon;

        SetArtifactSlot(rightMatTid, target.rightSlot, rightArtifactSprite, rightArtifactGradeSprite, iconColorByActive, gradeColorByActive);

        imgSelected.gameObject.SetActive(isSelected);
    }

    public void AddListener_OnClicked(Action callback)
    {
        _onClicked += callback;
    }

    public void RemoveListener_OnClicked(Action callback)
    {
        _onClicked -= callback;
    }

    public void AddListener_OnClickedLeftMat(Action<uint> callback)
    {
        _onClickedLeftMaterial += callback;
    }

    public void RemoveListener_OnClickedLeftMat(Action<uint> callback)
    {
        _onClickedLeftMaterial -= callback;
    }

    public void AddListener_OnClickedRightMat(Action<uint> callback)
    {
        _onClickedRightMaterial += callback;
    }

    public void RemoveListener_OnClickedRightMat(Action<uint> callback)
    {
        _onClickedRightMaterial -= callback;
    }

    #endregion

    #region Private Methods
    void SetArtifactSlot(
        uint artifactID
        , CommonGroup.SingleSlot slot
        , Sprite iconSprite
        , Sprite gradeSprite
        , Color iconColor
        , Color gradeColor)
    {
        slot.artifactID = artifactID;
        slot.imgIcon.sprite = iconSprite;
        slot.imgGradeBoard.sprite = gradeSprite;
        slot.imgIcon.gameObject.SetActive(true);
        slot.imgGradeBoard.gameObject.SetActive(true);

        slot.imgIcon.color = iconColor;
        slot.imgGradeBoard.color = gradeColor;
    }
    #endregion

    #region OnClick Event (인스펙터 연결)
    public void OnClicked()
    {
        _onClicked?.Invoke();
    }

    public void OnClickLeftArtifact()
    {
        if (currentGroup.leftSlot.artifactID == 0)
            return;

        _onClickedLeftMaterial?.Invoke(currentGroup.leftSlot.artifactID);
    }

    public void OnClickRightArtifact()
    {
        if (currentGroup.rightSlot.artifactID == 0)
            return;

        _onClickedRightMaterial?.Invoke(currentGroup.rightSlot.artifactID);
    }
    #endregion
}
