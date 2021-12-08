using Devcat;
using GameDB;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZNet.Data;

public class UIEnhanceElementDetailWindow : MonoBehaviour
{
    [Serializable]
    public class ElementGroupProp
    {
        public E_UnitAttributeType type;

        public ScrollRect scrollRect;
        public RectTransform scrollContent;
        public RectTransform integratedTextRoot;
        public LayoutGroup layoutGroup;

        public Image imgEffect;
        public Image imgIcon;
        public Text txtTitle;
        public Text txtLevel;
    }

    [Serializable]
    public class ChainEffectGroupProp
    {
        public RectTransform integratedParent;
        public ScrollRect scrollRect;
        public Image imgBG;
        public RawImage imgEffect;
        public Image imgLevel;
        public Text txtLevel;
    }

    #region UI Variables
    [SerializeField] private RectTransform rootScrollRectRectTransform;
    [SerializeField] private ScrollRect rootScrollRect;
    [SerializeField] private List<ElementGroupProp> uiGroup_element;
    [SerializeField] private ChainEffectGroupProp uiGroup_chainEffect;
    [SerializeField] private CanvasGroup windowCanvasGroup;
    #endregion

    #region Properties 
    #endregion

    #region System Variables
    private bool isInitialized;

    private EnumDictionary<E_UnitAttributeType, UIEnhanceElement_IntegratedElementTxt> elementInteratedTxtDic = new EnumDictionary<E_UnitAttributeType, UIEnhanceElement_IntegratedElementTxt>();
    private UIEnhanceElement_IntegratedChainEffect chainEffectIntegratedObj;
    private UIEnhanceElementSettingProvider SettingProvider;

    private UIEnhanceElement ElementController;
    private bool forceLayoutUpdate;
    #endregion

    #region Public Methods
    public void Initialize(UIEnhanceElement elementController)
    {
        if (isInitialized)
            return;

        ElementController = elementController;
        SettingProvider = elementController.SettingProvider;

        // 보이는 순서대로 일단 세팅 
        SetupChainEffectObj(elementController);
        UpdateUI_ChainEffect();

        // SetupIntegratedTxts(elementController, (E_UnitAttributeType[])Enum.GetValues(typeof(E_UnitAttributeType)));
        SetupIntegratedTxts_Ex(elementController, (E_UnitAttributeType[])Enum.GetValues(typeof(E_UnitAttributeType)));
        // UpdateUI_Enhance();

        forceLayoutUpdate = true;
        isInitialized = true;
    }

    public void Open(int framesToSkip = 0)
    {
        if (framesToSkip > 0)
        {
            windowCanvasGroup.alpha = 0;
        }
        else
        {
            windowCanvasGroup.alpha = 1;
        }

        gameObject.SetActive(true);
        UpdateUI_Enhance();
        UpdateUI_ChainEffect();
        rootScrollRect.horizontalNormalizedPosition = 0;

        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(DelayOperation(framesToSkip, () => windowCanvasGroup.alpha = 1));
        }
        else
        {
            windowCanvasGroup.alpha = 1;
        }
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
    #endregion

    #region Unity Methods
    private void Update()
    {
        if (forceLayoutUpdate)
        {
            forceLayoutUpdate = false;

            foreach (var group in uiGroup_element)
                LayoutRebuilder.ForceRebuildLayoutImmediate(group.scrollContent);
        }
    }
    #endregion

    #region Private Methods
    IEnumerator DelayOperation(int framesToSkip, Action onFinished)
    {
        for (int i = 0; i < framesToSkip; i++)
            yield return null;

        onFinished.Invoke();
    }
    //private void SetupIntegratedTxts(
    //    UIEnhanceElement elementController, params E_UnitAttributeType[] elementTypes)
    //{
    //    for (int i = 0; i < elementTypes.Length; i++)
    //    {
    //        if (elementTypes[i].Equals(E_UnitAttributeType.None))
    //            continue;

    //        var elementIntegrated = elementController.CreateElementIntegratedTxt(FindElementGroup(elementTypes[i]).scrollContent);

    //        if (elementIntegrated != null)
    //        {
    //            elementIntegrated.Initialize(elementController.SettingProvider, elementController.IntegratedElementAbilityData[elementTypes[i]].Count, false);
    //            elementIntegrated.SetData(elementTypes[i], elementController.IntegratedElementAbilityData[elementTypes[i]]);
    //            elementInteratedTxtDic.Add(elementTypes[i], elementIntegrated);
    //            elementIntegrated.gameObject.SetActive(true);
    //        }
    //    }
    //}

    private void SetupIntegratedTxts_Ex(
        UIEnhanceElement elementController, params E_UnitAttributeType[] elementTypes)
    {
        for (int i = 0; i < elementTypes.Length; i++)
        {
            var type = elementTypes[i];

            if (type.Equals(E_UnitAttributeType.None))
                continue;

            var elementIntegrated = elementController.CreateElementIntegratedTxt(FindElementGroup(elementTypes[i]).integratedTextRoot);

            if (elementIntegrated != null)
            {
                elementIntegrated.Initialize_Ex(elementController.SettingProvider, false);
                elementInteratedTxtDic.Add(elementTypes[i], elementIntegrated);
                elementIntegrated.gameObject.SetActive(true);
            }
        }
    }

    private void SetupChainEffectObj(UIEnhanceElement elementController)
    {
        var integratedObj = elementController.CreateChainEffectIntegratedObj(uiGroup_chainEffect.integratedParent);

        if (integratedObj != null)
        {
            integratedObj.Initialize(elementController.SettingProvider, elementController.IntegratedChainEffectData);
            chainEffectIntegratedObj = integratedObj;
            integratedObj.gameObject.SetActive(true);
        }
    }

    private void UpdateUI_Enhance()
    {
        foreach (var e in elementInteratedTxtDic)
        {
            e.Value.SetData(e.Key, ElementController.IntegratedElementAbilityData_Ex[e.Key]);
            e.Value.SetNormalizePos(1f);
        }

        foreach (var t in uiGroup_element)
        {
            t.txtLevel.text = string.Format(DBLocale.GetText("Attribute_Level"), Me.CurCharData.GetAttributeLevelByType(t.type));
        }

        //foreach (var e in elementInteratedTxtDic)
        //{
        //    Attribute_Table firstElementData = null;
        //    DBAttribute.GetFirstByType(e.Key, out firstElementData);
        //    var group = FindElementGroup(e.Key);

        //    if (group != null)
        //    {
        //        if (firstElementData != null)
        //        {
        //            group.imgEffect.color = SettingProvider.GetColorActive_IconLineAndEnhance(e.Key);
        //            group.imgIcon.sprite = ZManagerUIPreset.Instance.GetSprite(firstElementData.AttributeIconID);
        //            group.txtTitle.text = DBLocale.GetText(firstElementData.AttributeTitle);

        //            // 일단 테이블 에러방지 
        //            try
        //            {
        //                group.txtLevel.text = string.Format(DBLocale.GetText("Attribute_Level"), Me.CurCharData.GetAttributeLevelByType(e.Key));
        //            }
        //            catch (System.Exception exp)
        //            { }
        //        }
        //        else
        //        {
        //            ZLog.LogError(ZLogChannel.UI, "Could not get first attribute Data : " + e.Key);
        //        }
        //    }

        //    e.Value.UpdateUI();
        //}

        //foreach (var group in uiGroup_element)
        //{
        //    group.scrollRect.verticalNormalizedPosition = 1;
        //}
    }

    private void UpdateUI_ChainEffect()
    {
        uint displayChainLevel = Me.CurCharData.GetAttributeChainEffectLevel();
        var data = DBAttribute.GetAttributeChainByLevel(displayChainLevel);
        bool effectActive = true;

        // 테이블에서 데이터를 못찾는 경우는 최소 레벨보다 아래로 판단하고 
        // 최소 레벨을 띄어줌 
        if (data == null)
        {
            effectActive = false;
            displayChainLevel = DBAttribute.GetAttributeChainMinLevel();
            data = DBAttribute.GetAttributeChainByLevel(displayChainLevel);
        }

        if (data == null)
            return;

        Color bgColor = SettingProvider.chainAndEnhanceProvider.GetChainEffectImgColor(displayChainLevel);
        Color levelTxtColor = SettingProvider.chainAndEnhanceProvider.GetChainEffectTitleTxtColor(displayChainLevel);

        try
        {
            uiGroup_chainEffect.imgEffect.gameObject.SetActive(effectActive);
            uiGroup_chainEffect.imgBG.color = bgColor;
            uiGroup_chainEffect.imgLevel.color = bgColor;
            uiGroup_chainEffect.imgLevel.sprite = ZManagerUIPreset.Instance.GetSprite(data.IconID);
            uiGroup_chainEffect.txtLevel.text = string.Format(DBLocale.GetText(data.ChainTitleID), displayChainLevel);
            uiGroup_chainEffect.txtLevel.color = levelTxtColor;
        }
        catch (Exception exp) { }

        chainEffectIntegratedObj.UpdateUI();

        uiGroup_chainEffect.scrollRect.verticalNormalizedPosition = 1;
    }

    private ElementGroupProp FindElementGroup(E_UnitAttributeType type)
    {
        return uiGroup_element.Find(t => t.type.Equals(type));
    }
    #endregion
}
