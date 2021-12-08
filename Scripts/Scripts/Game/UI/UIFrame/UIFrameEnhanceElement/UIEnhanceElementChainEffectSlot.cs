using GameDB;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UIEnhanceElement;

public class UIEnhanceElementChainEffectSlot : MonoBehaviour
{
    #region Serialized Field 
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private RectTransform txtLayout;
    [SerializeField] private Image imgLevelEdge;
    [SerializeField] private Image imgLevel;

    [SerializeField] private List<UIEnhanceElementTitleValuePair> txts;
    #endregion

    #region System Variables
    private UIEnhanceElementSettingProvider SettingProvider;
    #endregion

    #region Properties
    public RectTransform RectTransform { get { return rectTransform; } }
    public uint chainLevel { get; private set; }
    #endregion

    #region Public Methods
    public void Initialize(UIEnhanceElementSettingProvider settingProvider, uint level)
    {
        SettingProvider = settingProvider;
        chainLevel = level;
    }

    public void GenerateText(UIEnhanceElementTitleValuePair source, AbilityActionTitleValuePair abilityAction)
    {
        var newTxt = Instantiate(source, txtLayout);
        newTxt.SetText(abilityAction.title, abilityAction.value);
        newTxt.gameObject.SetActive(true);
        txts.Add(newTxt);
    }

    public void UpdateUI(AttributeChain_Table data, Color imgColor, Color titleColor, Color valueColor)
    {
        imgLevel.sprite = ZManagerUIPreset.Instance.GetSprite(data.IconID);
        imgLevelEdge.color = imgColor;
        imgLevel.color = imgColor;

        foreach (var txt in txts)
        {
            txt.SetColor(titleColor, valueColor);
        }
    }
    #endregion
}
