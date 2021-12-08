using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UIFrameArtifactAbilityActionBuilder;

public class UIFrameArtifactLinkSingleAbilityAction : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private Text txtGrade;

    private List<UIFrameArtifactSingleAbilityAction> abilityTextObjs;

    [SerializeField] private Color titleActiveColor;
    [SerializeField] private Color valueActiveColor;
    [SerializeField] private Color inactiveColor;

    public RectTransform RectTransform { get { return this.rectTransform; } }
    public int CurrentTextCount
    {
        get
        {
            return abilityTextObjs != null ? abilityTextObjs.Count : 0;
        }
    }
    public bool IsMyGrade { get; private set; }

    public void Set(
        UIFrameArtifactAbilityActionBuilder builder
        , UIFrameArtifactSingleAbilityAction sourceObj
        , string titleGrade
        , Color titleGradeColor
        , bool isObtained
        , bool isMyCurrentGrade
        , List<AbilityActionTitleValuePair> txtList)
    {
        IsMyGrade = isMyCurrentGrade;

        if (abilityTextObjs == null)
            abilityTextObjs = new List<UIFrameArtifactSingleAbilityAction>();

        txtGrade.text = titleGrade;
        txtGrade.color = isMyCurrentGrade ? titleGradeColor : inactiveColor;

        builder.SetAbilityActionUITexts(false, 0, sourceObj, txtList, RectTransform, ref abilityTextObjs);

        for (int i = 0; i < abilityTextObjs.Count; i++)
        {
            if (abilityTextObjs[i].gameObject.activeSelf)
            {
                abilityTextObjs[i].SetColor(
                    isMyCurrentGrade ? titleActiveColor : inactiveColor
                    , isMyCurrentGrade ? valueActiveColor : inactiveColor);
            }
        }
    }
}