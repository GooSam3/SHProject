using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZNet.Data;
using static UIEnhanceElement;

public class UIEnhanceElement_IntegratedChainEffect : MonoBehaviour
{
    public class SingleSlot
    {
        public uint chainEffectLevel;
        public UIEnhanceElementChainEffectSlot slot;
    }

    #region Serialized Field
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private UIEnhanceElementChainEffectSlot slotSourceObj;
    [SerializeField] private UIEnhanceElementTitleValuePair singleTxtSourceObj;
    #endregion

    #region Properties 
    public RectTransform RectTransform { get { return rectTransform; } }
    #endregion

    #region System Variables
    private UIEnhanceElementSettingProvider SettingProvider;
    private List<UIEnhanceElementChainEffectSlot> slots;
    private bool forceUpdateLayout;
    private bool isInitialized;
    #endregion

    #region Public Methods
    public void Initialize(UIEnhanceElementSettingProvider settingProvider, List<AbilityActionTitleValuePair> data)
    {
        if (isInitialized)
            return;

        SettingProvider = settingProvider;
        slots = new List<UIEnhanceElementChainEffectSlot>();

        Generate(data);
        UpdateUI();

        forceUpdateLayout = true;
        isInitialized = true;
    }

    public void Generate(List<AbilityActionTitleValuePair> data)
    {
        foreach (var d in data)
        {
            var chainEffectData = DBAttribute.GetAttributeChainByID(d.sourceTid);
            var targetSlot = slots.Find(t => t.chainLevel == chainEffectData.ChainLevel);

            // 슬롯 생성 
            if (targetSlot == null)
            {
                targetSlot = Instantiate(slotSourceObj, rectTransform);
                targetSlot.Initialize(SettingProvider, chainEffectData.ChainLevel);
                targetSlot.gameObject.SetActive(true);

                if (targetSlot != null)
                    slots.Add(targetSlot);
            }

            targetSlot.GenerateText(singleTxtSourceObj, d);
        }
    }

    public void UpdateUI()
    {
        var colorSource = SettingProvider.chainAndEnhanceProvider.chainEffect;

        foreach (var slot in slots)
        {
            bool isObtained = Me.CurCharData.IsThisAttributeChainEffectObtained(slot.chainLevel);
            Color imgColor = isObtained ?
                colorSource.imgColorOnActive :
                colorSource.imgColorOnInactive;
            Color titleColor = isObtained ?
                colorSource.effectTitleColorOnActive :
                colorSource.effectTitleColorOnInactive;
            Color valueColor = isObtained ?
                colorSource.valueColorOnActive :
                colorSource.valueColorOnInactive;

            slot.UpdateUI(DBAttribute.GetAttributeChainByLevel(slot.chainLevel), imgColor, titleColor, valueColor);
        }
    }
    #endregion

    #region Unity Methods
    private void Update()
    {
        if(forceUpdateLayout)
        {
            forceUpdateLayout = false;

            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        }
    }
    #endregion
}
