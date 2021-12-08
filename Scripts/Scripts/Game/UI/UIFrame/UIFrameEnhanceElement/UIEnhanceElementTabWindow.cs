using GameDB;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZNet.Data;
using static UIEnhanceElement;

public class UIEnhanceElementTabWindow : MonoBehaviour
{
    // 하나의 탭 윈도우 on/off 를 제어하기 위한 타입 
    [Serializable]
    public class TabObj
    {
        public TabState type;
        public GameObject obj;
    }

    [Serializable]
    public class NextLevelProgressBar
    {
        public E_UnitAttributeType type;
        public Slider sliderBar;
        public Text txtProgres;
    }

    // 속성 연계 윈도우 탭 구성요소
    [Serializable]
    public class ChainEffectGroup
    {
        public RectTransform integratedObjParent;

        public Image imgBG;
        public RawImage imgEffect;
        // 위에 타이틀처럼 있는 부분 
        public Image imgLevel;
        public Text txtLevelTitle;
        //   public Text txtLevel;

        public List<NextLevelProgressBar> nextLevelBars;
    }

    // 강화 윈도우 탭 구성요소
    [Serializable]
    public class EnhanceGroup
    {
        public Image imgEffect;
        public Image imgElemental;
        public Text txtElementalTitle;
        public Text txtElementalLevel;

        public RectTransform enhanceScrollContent;
        public RectTransform scrollRectRectTransform;
        public ScrollRect scrollRect;

        /// <summary>
        /// ++ 출력 수정 
        /// </summary>
        public RectTransform integratedParent;

        public UIEnhanceElementEnhancing enhancingUI;
    }

    #region SerializedFields
    [SerializeField] private List<TabObj> tabObjs;

    #region ChainEffect
    [SerializeField] private ChainEffectGroup uiGroup_ChainEffect;
    #endregion

    #region Enhance
    [SerializeField] private EnhanceGroup uiGroup_Enhance;
    #endregion

    #endregion

    #region System Variables
    private bool isInitialized = false;

    private UIEnhanceElement EnhanceController;
    private UIEnhanceElementSettingProvider SettingProvider;

    private CanvasGroup elementIntegratedObjCanvasGroup;
    private float elementIntegratedSnapScrollRectNormalizedPos;
    private bool elementIntegratedSnapScrollRect;

    private Coroutine snapScrollRectCo;
    private WaitForEndOfFrame snapScrollRectWaitCached;

    private TabState tabState;
    private E_UnitAttributeType curSelectedAttributeType;

    #region ChainEffect
    private UIEnhanceElement_IntegratedChainEffect ChainEffectInteratedObj;
    #endregion

    #region Enhance
    #endregion
    // private EnumDictionary<E_UnitAttributeType, UIEnhanceElement_IntegratedElementTxt> ElementInteratedTxtDic = new EnumDictionary<E_UnitAttributeType, UIEnhanceElement_IntegratedElementTxt>();
    private UIEnhanceElement_IntegratedElementTxt ElementIntegratedTxt;
    private UIEnhanceElement_IntegratedElementTxt ElementIntegratedTxt_Ex;

    // ScrollRect 의 normalized pos 에 값을 넣엇는데 즉시 바뀌지 않는 
    // 알려진 유니티 버그때문에 최초에만 다르게 처리.. 
    private List<E_UnitAttributeType> elementScrollReadyList;
    #endregion

    #region Public Methods
    public void Initialize(UIEnhanceElement elementController)
    {
        if (isInitialized)
            return;

        EnhanceController = elementController;
        SettingProvider = elementController.SettingProvider;
        snapScrollRectWaitCached = new WaitForEndOfFrame();
        elementScrollReadyList = new List<E_UnitAttributeType>();

        // integrated Txt 세팅 
        // SetupElementIntegratedTxts(elementController);
        SetupElementIntegratedTxts_Ex(elementController);
        UpdateUI_Enhance();

        SetupChainEffectIntegratedObj(elementController);
        UpdateUI_ChainEffect();

        SetupEnhancingUI(elementController);

        LayoutRebuilder.ForceRebuildLayoutImmediate(uiGroup_Enhance.enhanceScrollContent);

        isInitialized = true;
    }

    public void Release()
    {
        uiGroup_Enhance.enhancingUI.Release();
    }

    public void OpenEnhanceTab(E_UnitAttributeType type)
    {
        tabState = TabState.Enhance;
        ResetActiveObjsByTab();

        elementIntegratedObjCanvasGroup.alpha = 0;
        SetSelectedAttributeType(type);
        SetEnhancingAttribute(type);
    }

    public void OpenChainEffectTab()
    {
        tabState = TabState.ChainEffect;
        ResetActiveObjsByTab();
    }

    public void UpdateUI()
    {
        switch (tabState)
        {
            case TabState.ChainEffect:
                {
                    UpdateUI_ChainEffect();
                }
                break;
            case TabState.Enhance:
                {
                    UpdateUI_Enhance();
                }
                break;
            default:
                ZLog.LogError(ZLogChannel.UI, "Type non matching");
                break;
        }
    }
    #endregion

    #region Unity Methods
    private void OnEnable()
    {
        if (snapScrollRectCo != null)
            StopCoroutine(snapScrollRectCo);
        snapScrollRectCo = StartCoroutine(SnapScrollRect());
    }
    #endregion

    #region Private Methods
    // 하이라이트된 부분으로 바로 이동
    private void ScrollSnapToHighlightedTxt(E_UnitAttributeType type)
    {
        // 일단 해당 데이터의 전체 배열에서의 비율을 구함 (0~1)
        // 그 다음에 이미 눈에 보이고있는 영역만큼을 인덱스로 계산후 제외시켜줌 . 

        // 위에 맞추려면 밑에 주석 코드를 쓰고 upperOrBottom 을 True 로 세팅 
        // ((int)(uiGroup_Enhance.scrollRectRectTransform.rect.height / EnhanceController.GetElementIntegratedSourceRect().height) * -1)

        float normalizedPos = EnhanceController.GetNextElementAbilityIntegratedScrollRectNormalizedPos(
            type
            , upperOrBottom: false
            , ((int)(uiGroup_Enhance.scrollRectRectTransform.rect.height / EnhanceController.GetElementIntegratedSourceRect().height) * -1) + 1);

        elementIntegratedSnapScrollRectNormalizedPos = Mathf.Clamp(normalizedPos, 0f, 1f);
        elementIntegratedSnapScrollRect = true;
    }

    /*
     * 이윤선 - 이 코드는 IntegratedText 들의 active 가 on 이 되고 layoutController 가 업데이트된 후에 
     * 바로 ScrollRect 의 normalizePos 에 포지션값을 계산해 넣었을때 ScrollRect 가 Update 되지않는
     * 괴상한 알려진 버그때문에 작성하게된 코드 . 
     * https://stackoverflow.com/questions/36198505/scrollrect-does-not-correctly-update-the-position-when-clamping-the-normalized-p
     * yield return null; 도 업데이트 시점이 안맞기에 WaitForEndOfFrame() 으로 구현 
     * */
    IEnumerator SnapScrollRect()
    {
        while (true)
        {
            yield return snapScrollRectWaitCached;

            if (elementIntegratedSnapScrollRect)
            {
                elementIntegratedSnapScrollRect = false;
                elementIntegratedObjCanvasGroup.alpha = 1;
                uiGroup_Enhance.scrollRect.verticalNormalizedPosition = elementIntegratedSnapScrollRectNormalizedPos;
            }
        }
    }

    private void ResetActiveObjsByTab()
    {
        for (int i = 0; i < tabObjs.Count; i++)
        {
            if (tabObjs[i].obj)
            {
                tabObjs[i].obj.SetActive(tabObjs[i].type.Equals(tabState));
            }
        }
    }

    private void SetupElementIntegratedTxts(
        UIEnhanceElement elementController)
    {
        ElementIntegratedTxt = elementController.CreateElementIntegratedTxt(uiGroup_Enhance.enhanceScrollContent);

        if (ElementIntegratedTxt == null)
            return;

        elementIntegratedObjCanvasGroup = ElementIntegratedTxt.gameObject.AddComponent<CanvasGroup>();
        elementIntegratedObjCanvasGroup.blocksRaycasts = true;
        elementIntegratedObjCanvasGroup.interactable = false;
        int dataCount = elementController.MaxElementIntegratedTxtCnt;
        ElementIntegratedTxt.Initialize(elementController.SettingProvider, dataCount, true);
        ElementIntegratedTxt.gameObject.SetActive(false);
    }

    /// <summary>
    /// ++ 출력 방식 수정 
    /// </summary>
    private void SetupElementIntegratedTxts_Ex(UIEnhanceElement elementController)
    {
        ElementIntegratedTxt_Ex = elementController.CreateElementIntegratedTxt(uiGroup_Enhance.integratedParent);

        if (ElementIntegratedTxt_Ex == null)
            return;

        elementIntegratedObjCanvasGroup = ElementIntegratedTxt_Ex.gameObject.AddComponent<CanvasGroup>();
        elementIntegratedObjCanvasGroup.blocksRaycasts = true;
        elementIntegratedObjCanvasGroup.interactable = false;
        ElementIntegratedTxt_Ex.Initialize_Ex(elementController.SettingProvider, true);
        //int dataCount = elementController.MaxElementIntegratedTxtCnt;
        //ElementIntegratedTxt.Initialize(elementController.SettingProvider, dataCount, true);
        ElementIntegratedTxt_Ex.gameObject.SetActive(true);
    }

    private void SetupEnhancingUI(UIEnhanceElement elementController)
    {
        uiGroup_Enhance.enhancingUI.Initialize(elementController);
    }

    private void SetupChainEffectIntegratedObj(UIEnhanceElement elementController)
    {
        var chainEffectIntegratedObj = elementController.CreateChainEffectIntegratedObj(uiGroup_ChainEffect.integratedObjParent);

        if (chainEffectIntegratedObj == null)
            return;

        chainEffectIntegratedObj.Initialize(elementController.SettingProvider, elementController.IntegratedChainEffectData);
        this.ChainEffectInteratedObj = chainEffectIntegratedObj;
        chainEffectIntegratedObj.gameObject.SetActive(true);
    }

    private void UpdateElementIntegratedTxtData()
    {
        if (curSelectedAttributeType != E_UnitAttributeType.None)
        {
            ElementIntegratedTxt.SetData(curSelectedAttributeType, EnhanceController.IntegratedElementAbilityData[curSelectedAttributeType]);
        }

        ElementIntegratedTxt.gameObject.SetActive(curSelectedAttributeType.Equals(E_UnitAttributeType.None) == false);
    }

    private void SetSelectedAttributeType(E_UnitAttributeType type)
    {
        curSelectedAttributeType = type;
    }

    private void SetEnhancingAttribute(E_UnitAttributeType currentType)
    {
        uiGroup_Enhance.enhancingUI.SetEnhanceAttributeID(Me.CurCharData.GetAttributeTIDByType(currentType), true, currentType);
    }

    private void UpdateUI_ChainEffect()
    {
        // 타이틀 및 단계별 통합 텍스트 정보 업데이트 
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

        if (data != null)
        {
            try
            {
                uiGroup_ChainEffect.imgEffect.gameObject.SetActive(effectActive);
                uiGroup_ChainEffect.imgBG.color = bgColor;
                uiGroup_ChainEffect.imgLevel.color = bgColor;
                uiGroup_ChainEffect.imgLevel.sprite = ZManagerUIPreset.Instance.GetSprite(data.IconID);
                uiGroup_ChainEffect.txtLevelTitle.text = string.Format(DBLocale.GetText(data.ChainTitleID), displayChainLevel);
                uiGroup_ChainEffect.txtLevelTitle.color = levelTxtColor;
            }
            catch (Exception exp) { }
        }

        ChainEffectInteratedObj.UpdateUI();

        UpdateUI_ChainEffectNextLevel();
    }

    private void UpdateUI_Enhance()
    {
        Attribute_Table firstElementData = null;
        DBAttribute.GetFirstByType(curSelectedAttributeType, out firstElementData);

        if (firstElementData != null)
        {
            Color levelTxtColor = SettingProvider.GetCommonColorByType(curSelectedAttributeType);

            uiGroup_Enhance.imgEffect.color = SettingProvider.GetColorActive_IconLineAndEnhance(curSelectedAttributeType);
            uiGroup_Enhance.imgElemental.sprite = ZManagerUIPreset.Instance.GetSprite(firstElementData.AttributeIconID);
            uiGroup_Enhance.txtElementalTitle.text = DBLocale.GetText(firstElementData.AttributeTitle);
            uiGroup_Enhance.txtElementalTitle.color = levelTxtColor;
            uiGroup_Enhance.txtElementalLevel.color = levelTxtColor;

            // 테이블에 local 없으면 에러날수있음 . 
            try
            {
                uiGroup_Enhance.txtElementalLevel.text = string.Format(DBLocale.GetText("Attribute_Level"), Me.CurCharData.GetAttributeLevelByType(curSelectedAttributeType));
            }
            catch (Exception exp) { }
        }

        /// ++ 기존출력제거
        //UpdateElementIntegratedTxtData();
        //ElementIntegratedTxt.UpdateUI();

        ElementIntegratedTxt_Ex.SetData(curSelectedAttributeType, EnhanceController.IntegratedElementAbilityData_Ex[curSelectedAttributeType]);
        // WARNING : OSA 의 ScrollTo 함수가 정상작동을 안함 . 일단 SetNormalize 로 대체 ElementIntegratedTxt_Ex.SnapToNextLevel(2, () => { elementIntegratedObjCanvasGroup.alpha = 1; });
        // ElementIntegratedTxt_Ex.SnapToNextLevel(2);
        ElementIntegratedTxt_Ex.SetNormalizePos(1f);
        elementIntegratedObjCanvasGroup.alpha = 1;

        uiGroup_Enhance.enhancingUI.RefreshCurrentData();

        LayoutRebuilder.ForceRebuildLayoutImmediate(uiGroup_Enhance.enhanceScrollContent);

        //ScrollSnapToHighlightedTxt(curSelectedAttributeType);
    }

    private void UpdateUI_ChainEffectNextLevel()
    {
        uint reachableElementLevel = DBAttribute.GetAttributeReachableMaxLevelAtCurrentChainLevel(Me.CurCharData.GetAttributeChainEffectLevel());

        if (reachableElementLevel == 0)
        {
            reachableElementLevel = DBAttribute.GetAttributeChainRequestLevel(DBAttribute.GetAttributeChainMinLevel());
        }

        foreach (var t in uiGroup_ChainEffect.nextLevelBars)
        {
            int curLevel = (int)Me.CurCharData.GetAttributeLevelByType(t.type);
            t.sliderBar.minValue = ((int)DBAttribute.GetAttributeChainMinLevel()) - 1;
            t.sliderBar.maxValue = reachableElementLevel;
            t.sliderBar.value = curLevel;

            t.txtProgres.text = string.Format("{0}/{1}", curLevel, reachableElementLevel);
        }
    }

    private TabObj FindTab(TabState type)
    {
        return tabObjs.Find(t => t.type.Equals(type));
    }
    #endregion
}
