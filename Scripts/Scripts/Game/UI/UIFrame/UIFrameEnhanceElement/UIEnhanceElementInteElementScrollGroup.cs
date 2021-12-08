using UnityEngine.UI;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using static UIEnhanceElement_IntegratedElementTxt;
using static UIEnhanceElement;

public class UIEnhanceElementInteElementScrollGroup : MonoBehaviour
{
    #region SerializedField
    #region Preference Variable
    #endregion

    #region UI Variables
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private Text txtLevel;

    [SerializeField] private Image imgHighlight;
    #endregion
    #endregion

    #region System Variables
    public RectTransform RectTransform { get { return this.rectTransform; } }
    private List<UIEnhanceElementTitleValuePair> txtTitleValues = new List<UIEnhanceElementTitleValuePair>();

    private Action _onClicked;
    #endregion

    #region Properties 
    #endregion

    #region Unity Methods
    #endregion

    #region Overrides 
    #endregion

    #region Public Methods
    public void SetUI(
        uint level
        , List<AbilityActionTitleValuePair> data
        , UIEnhanceElementTitleValuePair pairSourceObj
        , bool highlight
        , Color highlightColor
        , Color levelColor
        , Color titleColor
        , Color valueColor)
    {
        txtLevel.text = string.Format(DBLocale.GetText("Attribute_Level"), level);
        txtLevel.color = levelColor;

        ///  pair 개수 맞춤 
        int dataCnt = data.Count;

        if (txtTitleValues.Count < dataCnt)
        {
            txtTitleValues.Capacity = dataCnt;
            int addCnt = dataCnt - txtTitleValues.Count;

            for (int i = 0; i < addCnt; i++)
            {
                AddValueText(pairSourceObj);
            }
        }
        // 안쓰게될 부분 active false 처리 
        else if (txtTitleValues.Count > dataCnt)
        {
            for (int i = 0; i < txtTitleValues.Count - dataCnt; i++)
            {
                txtTitleValues[i + dataCnt].gameObject.SetActive(false);
            }
        }

        for (int i = 0; i < dataCnt; i++)
        {
            txtTitleValues[i].SetText(data[i].title, data[i].value);
            txtTitleValues[i].SetColor(titleColor, valueColor);
            txtTitleValues[i].gameObject.SetActive(true);
        }

        imgHighlight.color = highlightColor;
        imgHighlight.gameObject.SetActive(highlight);
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
    private void AddValueText(UIEnhanceElementTitleValuePair sourceObj)
    {
        var instance = Instantiate(sourceObj, rectTransform);
        instance.gameObject.SetActive(false);
        txtTitleValues.Add(instance);
    }
    #endregion

    #region OnClick Event (인스펙터 연결)
    public void OnClicked()
    {
        _onClicked?.Invoke();
    }
    #endregion
}