using System;
using UnityEngine;
using UnityEngine.UI;

// 속성 메인창에 나타나는 속성 연계 레벨 UI 
public class UIEnhanceElementChainLevel : MonoBehaviour
{
    #region Serialized Field
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private RectTransform btnRectTransform;
    [SerializeField] private Image imgBG;
    [SerializeField] private Image imgLevel;
    #endregion

    #region System Variables
    private event Action<UIEnhanceElementChainLevel> onClickHandler;
    #endregion

    #region Properties
    public uint ChainLevel { get; private set; }

    public RectTransform RectTransform { get { return rectTransform; } }
    public RectTransform ButtonRectTransform { get { return btnRectTransform; } }
    #endregion

    public void Set(Action<UIEnhanceElementChainLevel> onClick, uint level, Sprite levelSprite, Color color)
    {
        onClickHandler = onClick;
        ChainLevel = level;
        imgLevel.sprite = levelSprite;
        imgBG.color = color;
        imgLevel.color = color;
    }

    #region OnClick Event
    public void OnClick()
    {
        onClickHandler?.Invoke(this);
    }
    #endregion
}
