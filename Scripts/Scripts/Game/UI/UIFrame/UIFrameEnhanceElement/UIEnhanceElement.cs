using Devcat;
using frame8.Logic.Misc.Other.Extensions;
using GameDB;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZNet.Data;

public class UIEnhanceElement : ZUIFrameBase
{
    public enum TabState
    {
        None = 0,
        ChainEffect, // 연계효과탭 , (디폴트, 첫화면) 
        Enhance // 강화탭 (속성 클릭시)
    }

    public enum ToolTipType
    {
        None = 0,
        Slot,
        ChainEffect
    }

    [Flags]
    public enum UpdateUIFlag
    {
        // Everything,
        Slots = 0x1,
        Tab = 0x1 << 1,
        Selectables = 0x1 << 2
    }

    public struct AbilityActionBuilderInput
    {
        public uint tid;
        public Color color;
    }

    public struct AbilityActionTitleValuePairBuildInput
    {
        // true : attribute , false : chain
        //public bool isSourceAttributeOrChain;
        public uint sourceTid;
        public uint abilityActionTid;
    }

    public class AbilityActionTitleValuePair
    {
        // 해당 action 이 속성의 abilityAction 이라면 속성 레벨이되는거고 
        // 속성연계라면 속성연계의 레벨이 되는거임. 
        public uint sourceTid;
        public string title;
        public string value;
    }

    [Serializable]
    public class ToolTipProp
    {
        public ToolTipType type;
        public UIEnhanceElementToolTipPopUp target;
        public CanvasGroup canvasGroup;
        [Header("툴팁의 위치 출력 오프셋입니다. 기준은 타겟의 정가운데")]
        public Vector2 offset;

        [HideInInspector] public bool shouldClamp;
    }

    public override bool IsBackable => true;

	#region SerializedField
	#region UI Variables
	[SerializeField] private RectTransform canvas;
    [SerializeField] private UIEnhanceElementScrollAdapter ScrollAdapter;
    [SerializeField] private UIEnhanceElementFigureBoard figureBoard;
    [SerializeField] private UIEnhanceElementDetailWindow detailOverlayScreen;
    //    [SerializeField] private UIEnhanceElementToolTipPopUp toolTipPopUp;

    [SerializeField] private UIEnhanceElementTabTrigger tabTrigger;
    [SerializeField] private UIEnhanceElementTabWindow tabWindow;

    // 통합 정보 텍스트 그룹 관련 
    [SerializeField] private UIEnhanceElement_IntegratedElementTxt integratedElementSource;
    [SerializeField] private UIEnhanceElement_IntegratedChainEffect integratedChainEffectSource;

    [SerializeField] private List<GameObject> deactivateListOnDetailWindow;

    [SerializeField] private UIEnhanceElementSettingProvider settingInfoProvider;

    [SerializeField] private List<ToolTipProp> toolTipPopUps;

    [SerializeField] private List<Button> interactableActiveOnFrameSetupDone;
    #endregion

    [SerializeField] private ElementDirectionHandler directionHandler;

    [SerializeField] private List<Selectable> offSelectableListWhileDirectiong;
    #region Preference Variable

    //[Header("처음 탭 화면")]
    //[SerializeField] private TabState defaultTabState = TabState.ChainEffect;
    #endregion
    #endregion

    #region Public Variable

    /// <summary>
    ///  TODO :  추후에 픽스해야함 . 현재 , 광클시 GUI 프레임워크가 해당 프리팹을 두개이상을 만들어 버리는 버그 임시 수정 
    /// </summary>
    public static bool Temp_IsOpening;

    #endregion

    #region System Variable
    private bool initialized;
    private Coroutine setupCo;
    private Coroutine enhanceCo;

    private E_UnitAttributeType[] attributeTypes;

    // 속성강화창에 통합적으로 뜨는 텍스트들의 출력 데이터들만을 가지고 있는다 . 
    private EnumDictionary<E_UnitAttributeType, List<AbilityActionTitleValuePair>> integratedElementAbilityData = new EnumDictionary<E_UnitAttributeType, List<AbilityActionTitleValuePair>>();
    // 속성연계 '' 
    private List<AbilityActionTitleValuePair> integratedChainEffectAbilityData = new List<AbilityActionTitleValuePair>();

    /// <summary>
    /// ++ 출력 방식 수정 
    /// </summary>
    private EnumDictionary<E_UnitAttributeType, List<ScrollEnhanceElementIntegratedData>> integratedElementAbilityData_Ex = new EnumDictionary<E_UnitAttributeType, List<ScrollEnhanceElementIntegratedData>>();
    #endregion

    #region Properties 
    public UIEnhanceElementSettingProvider SettingProvider => settingInfoProvider;
    public ElementDirectionHandler DirectionHandler => directionHandler;
    public EnumDictionary<E_UnitAttributeType, List<AbilityActionTitleValuePair>> IntegratedElementAbilityData => integratedElementAbilityData;
    public EnumDictionary<E_UnitAttributeType, List<ScrollEnhanceElementIntegratedData>> IntegratedElementAbilityData_Ex => integratedElementAbilityData_Ex;
    public int MaxElementIntegratedTxtCnt
    {
        get
        {
            if (integratedElementAbilityData == null)
                return 0;

            int result = int.MinValue;

            foreach (var keyPair in integratedElementAbilityData)
            {
                var list = keyPair.Value;
                if (list.Count > result)
                {
                    result = list.Count;
                }
            }

            return result;
        }
    }

    public List<AbilityActionTitleValuePair> IntegratedChainEffectData { get { return integratedChainEffectAbilityData; } }
    public E_UnitAttributeType CurrentSelectedAttributeTab { get { return tabTrigger.SelectedType; } }
    #endregion

    #region Public Methods
    public void InitializeShaderClippingUpdater(GameObject root)
    {
        var targets = root.GetComponentsInChildren<UIShaderClipingUpdater>(true);

        if (targets != null)
        {
            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i] != null)
                {
                    targets[i].DoUIWidgetInitialize(this);
                }
            }
        }
    }

    /// <summary>
    /// 강화 시도시 호출 
    /// </summary>
    public void AdvanceTryEnhance(E_UnitAttributeType selectedType, uint nextTargetTid, uint enhanceAddCnt, Action onFinished)
    {
        uint preChainLevel = Me.CurCharData.GetAttributeChainEffectLevel();
        uint curTid = Me.CurCharData.GetAttributeTIDByType(selectedType);

        /// 우선 현재로 스크롤 이동 
        ScrollAdapter.ScrollToSpecificAttribute(curTid, 0.3f);
        /// 드래깅 막음 
        ScrollAdapter.SetDragEnable(false);

        ZWebManager.Instance.WebGame.REQ_EnhanceAttribute(
            curTid
            /// ** 해당 Rate 를 몇번 Add 해야 현재 CurEnhanceRawRate 가 되는건지를 계산을 해서 ㅡㅡ 보내야함 ** 
            , enhanceAddCnt
        , _onReceive:
        (revPacket, resList) =>
        {
            if (this.enhanceCo != null)
            {
                StopCoroutine(enhanceCo);
                enhanceCo = null;
            }

            enhanceCo = StartCoroutine(ProcessEnhance(resList.IsSuccess, selectedType, curTid, nextTargetTid, Me.CurCharData.GetAttributeTIDByType(selectedType), preChainLevel, onFinished));
        },
        (err, req, res) =>
        {
            onFinished?.Invoke();
            ZWebManager.Instance.ProcessErrorPacket(err, req, res, false);
        });
    }

    // AbilityAction TID 를 받아 Title,Value 로 나누어 Ability Data 를 채워줌 . 
    // 예로 속성연계레벨4 의 AbilityActionID 1339042 를 넣어주면은 
    // "방어력" 이란 문자열이 title 에 들어가게되고 
    // 해당 AbilityAciton 에 AbiltyAction Point 값 4 가 value 에 들어가게됨 . 
    static public void BuildAbilityActionStrings(List<AbilityActionTitleValuePairBuildInput> abilityActionInputData, List<AbilityActionTitleValuePair> output)
    {
        List<UIAbilityData> abilityTypeAndValue = new List<UIAbilityData>();

        for (int i = 0; i < abilityActionInputData.Count; i++)
        {
            abilityTypeAndValue.Clear();

            // ability ID 와 value 가 존재하는 abilityTable 에서의 해당 데이터를 가져옴 
            var abilityActionTableData = DBAbilityAction.Get(abilityActionInputData[i].abilityActionTid);

            if (abilityActionTableData != null)
            {
                // 리스트에 AbilityType 과 Value 를 추가함. (Type 은 MAX_HP_PER 과도 같은 
                // AbilityType 이고 Locale Table 에서도 해당 값으로 찾을수 있으며 Ability 테이블에선
                // ID 역할도함 . 
                DBAbilityAction.GetAbilityTypeList(abilityActionTableData, ref abilityTypeAndValue);

                foreach (var typeAndValue in abilityTypeAndValue)
                {
                    DBAbility.GetAbility(typeAndValue.type, out var abilityData);

                    if (abilityData != null)
                    {
                        string unit = abilityData.MarkType == E_MarkType.Per ? "%" : string.Empty;

                        output.Add(new AbilityActionTitleValuePair()
                        {
                            sourceTid = abilityActionInputData[i].sourceTid,
                            title = string.Format(DBLocale.GetText(typeAndValue.type.ToString())),
                            value = string.Format("+{0}{1}", typeAndValue.value, unit)
                        });
                    }
                }
            }
        }
    }

    /// <summary>
    /// ++ 출력 방식 수정 
    /// </summary>
    static public void BuildAbilityActionStrings_Ex(
        List<AbilityActionTitleValuePairBuildInput> abilityActionInputData
        , List<ScrollEnhanceElementIntegratedData> output)
    {
        List<UIAbilityData> abilityTypeAndValue = new List<UIAbilityData>();

        for (int i = 0; i < abilityActionInputData.Count; i++)
        {
            abilityTypeAndValue.Clear();

            // ability ID 와 value 가 존재하는 abilityTable 에서의 해당 데이터를 가져옴 
            var abilityActionTableData = DBAbilityAction.Get(abilityActionInputData[i].abilityActionTid);
            DBAttribute.GetAttributeByID(abilityActionInputData[i].sourceTid, out var attributeData);

            if (abilityActionTableData != null)
            {
                var singleInteData = new ScrollEnhanceElementIntegratedData();
                singleInteData.level = attributeData.AttributeLevel;
                singleInteData.attributeID = abilityActionInputData[i].sourceTid;

                DBAbilityAction.GetAbilityTypeList(abilityActionTableData, ref abilityTypeAndValue);

                singleInteData.txtData = new List<AbilityActionTitleValuePair>(abilityTypeAndValue.Count);

                foreach (var typeAndValue in abilityTypeAndValue)
                {
                    DBAbility.GetAbility(typeAndValue.type, out var abilityData);

                    if (abilityData != null)
                    {
                        string unit = abilityData.MarkType == E_MarkType.Per ? "%" : string.Empty;

                        singleInteData.txtData.Add(new AbilityActionTitleValuePair()
                        {
                            sourceTid = abilityActionInputData[i].sourceTid,
                            title = string.Format(DBLocale.GetText(typeAndValue.type.ToString())),
                            value = string.Format("+{0}{1}", typeAndValue.value, unit)
                        });
                    }
                }

                output.Add(singleInteData);
            }
        }
    }

    // 생성한쪽이 알아서 관리 . 
    public UIEnhanceElement_IntegratedElementTxt CreateElementIntegratedTxt(RectTransform parent)
    {
        if (integratedElementSource == null)
            return null;

        return Instantiate(integratedElementSource, parent);
    }

    public UIEnhanceElement_IntegratedChainEffect CreateChainEffectIntegratedObj(RectTransform parent)
    {
        if (integratedChainEffectSource == null)
            return null;

        return Instantiate(integratedChainEffectSource, parent);
    }

    public Rect GetElementIntegratedSourceRect()
    {
        if (integratedElementSource == null)
            return Rect.zero;

        return integratedElementSource.SourceObjRect;
    }

    // countDivideOffset 은 스크롤 렉트에서 이미 보이고있는 만큼의 영역은 
    // 인덱스로 normalizedPos 를 계산하는 과정에서 제외되어야 하므로 , 별도로 파라미터로 받음 . 
    public float GetNextElementAbilityIntegratedScrollRectNormalizedPos(E_UnitAttributeType type, bool upperOrBottom, int countDivideOffset)
    {
        if (IntegratedElementAbilityData.ContainsKey(type) == false)
            return 0;

        if (DBAttribute.GetNextAttributeID(Me.CurCharData.GetAttributeIDByType(type), out var nextID))
        {
            var data = integratedElementAbilityData[type];
            int startIndex = data.FindIndex(t => t.sourceTid == nextID);
            float dividedBy = ((float)data.Count + countDivideOffset);

            if (upperOrBottom)
            {
                return 1 - ((startIndex)) / dividedBy;
            }
            else
            {
                int endIndex = data.FindLastIndex(t => t.sourceTid == nextID);
                return (data.Count - endIndex) / dividedBy;
            }
        }
        else
        {
            return 0;
        }
    }
    #endregion

    #region Overrides
    protected override void OnInitialize()
    {
        base.OnInitialize();
        UIManager.Instance.ShowGlobalIndicator(true);

        ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIEnhanceElementSlot),
            (t) =>
            {
                attributeTypes = (E_UnitAttributeType[])Enum.GetValues(typeof(E_UnitAttributeType));

                integratedElementSource.gameObject.SetActive(false);
                integratedChainEffectSource.gameObject.SetActive(false);
                SetupElementAbilityIntegratedTextData_Ex();
                //SetupElementAbilityIntegratedTextData();
                SetupChainEffectAbilityIntegratedTextData();
                InitializeShaderClippingUpdater(gameObject);
                directionHandler.Initialize();
                settingInfoProvider.Initialize(IsThisChainEffectObtained);
                ScrollAdapter.Initialize(settingInfoProvider, HandleSlotClicked);
                figureBoard.Initialize(settingInfoProvider.chainAndEnhanceProvider, HandleChainEffectIconClicked);
                tabWindow.Initialize(this); //, OnEnhanceTried);
                detailOverlayScreen.Initialize(this);
                foreach (var tooltip in toolTipPopUps)
                {
                    tooltip.canvasGroup = tooltip.target.gameObject.AddComponent<CanvasGroup>();
                    tooltip.canvasGroup.alpha = 0;
                }

                UIManager.Instance.ShowGlobalIndicator(false);
                initialized = true;
            }, bActiveSelf: false);
    }

    IEnumerator SetupDelay()
    {
        while (initialized == false)
        {
            yield return null;
        }

        ScrollAdapter.gameObject.SetActive(true);
        ScrollAdapter.SetDragEnable(true);

        detailOverlayScreen.gameObject.SetActive(false);
        toolTipPopUps.ForEach(t => t.target?.gameObject.SetActive(false));

        SetElementTab(E_UnitAttributeType.None);

        UpdateUI(UpdateUIFlag.Tab);

        interactableActiveOnFrameSetupDone.ForEach(t => t.interactable = true);
        SetSelectableStateByDirection(true);

        directionHandler.AddListener_OnPlay(OnDirectionPlay);
        directionHandler.AddListener_OnAllTerminated(OnAllDirectionTerminated);

        Temp_IsOpening = false;

        uint maxlevel = 0;
        uint maxAttributeId = 0;

        if (Me.CurCharData.attributeDic.Count > 0)
        {
            foreach (var keyPair in Me.CurCharData.attributeDic)
            {
                if (maxlevel < keyPair.Value.Level)
                {
                    maxlevel = keyPair.Value.Level;
                    maxAttributeId = keyPair.Value.Tid;
                }
            }
        }

        if(maxAttributeId != 0)
            ScrollAdapter.ScrollToSpecificAttribute(maxAttributeId, 0.3f,true);
    }

    protected override void OnShow(int _LayerOrder)
    {
        base.OnShow(_LayerOrder);

        UIManager.Instance.Find<UIFrameHUD>().SetSubHudFrame(E_UIStyle.FullScreen);
        UIManager.Instance.Find<UISubHUDCurrency>().ShowSpecialCurrency(
            new List<uint>() { DBConfig.Diamond_ID, DBConfig.Gold_ID, DBConfig.Essence_ID });

        UIManager.Instance.Find<UIFrameHUD>().RefreshCurrency();

        if (setupCo != null)
        {
            StopCoroutine(setupCo);
            setupCo = null;
        }

        setupCo = StartCoroutine(SetupDelay());
    }

    protected override void OnHide()
    {
        base.OnHide();
        UIManager.Instance.Find<UIFrameHUD>().SetSubHudFrame();
        UIManager.Instance.Find<UISubHUDCurrency>().ShowBaseCurrency();

        if (initialized)
        {
            SetDetailWindow(false);
            UpdateUI(UpdateUIFlag.Slots | UpdateUIFlag.Tab);
            directionHandler.Release();
            this.tabWindow.Release();
        }

        if (enhanceCo != null)
        {
            StopCoroutine(enhanceCo);
            enhanceCo = null;
        }
    }
    #endregion

    #region Unity Methods
    private void Awake()
    {
        /// Adapter 의 Start() 함수에 의해서 프리팹 로딩되지 않은 시점에 세팅 시도 방지 
        ScrollAdapter.gameObject.SetActive(false);

        interactableActiveOnFrameSetupDone.ForEach(t => t.interactable = false);
    }

    private void OnDisable()
    {
        Temp_IsOpening = false;
    }
    #endregion


    #region Private Methods
    private void UpdateUI(UpdateUIFlag flag)
    {
        if (CheckUpdateFlag(UpdateUIFlag.Slots, flag))
        {
            ScrollAdapter.RefreshAll();
            //ScrollAdapter.ResetVisibleSlots();
        }

        if (CheckUpdateFlag(UpdateUIFlag.Selectables, flag))
        {
            UpdateSelectableStateByDirection();
        }

        if (CheckUpdateFlag(UpdateUIFlag.Tab, flag))
        {
            tabTrigger.UpdateUI();
            figureBoard.UpdateUI();
            tabWindow.UpdateUI();
        }
    }

    private void UpdateSelectableStateByDirection()
    {
        /// 연출중인거 있음 꺼줌 
        bool interactable = directionHandler.IsAnyPlaying == false;
        SetSelectableStateByDirection(interactable);
    }

    private void SetSelectableStateByDirection(bool interactable)
    {
        offSelectableListWhileDirectiong.ForEach(t => t.interactable = interactable);
    }

    private IEnumerator ProcessEnhance(
        bool isSuccess
        , E_UnitAttributeType selectedType
        , uint oldTid
        , uint nextTargetTid
        , uint curTid
        , uint oldChainLevel,
        Action onFinished)
    {
        /// Selectable 전부 꺼줌 . 연출중 이벤트 방지 . 
        SetSelectableStateByDirection(false);

        Vector2 oldAttributeWorldPos;
        Vector2 nextTargetAttributeWorldPos;
        bool exit = false;

        if (ScrollAdapter.GetWorldPos(oldTid, out oldAttributeWorldPos) == false)
        {
            ZLog.LogError(ZLogChannel.UI, "Could not Get Attribute Slot WorldPosition. this is not supposed to happen");
            exit = true;
        }

        if (ScrollAdapter.GetWorldPos(nextTargetTid, out nextTargetAttributeWorldPos) == false)
        {
            ZLog.LogError(ZLogChannel.UI, "Could not Get Attribute Slot WorldPosition. this is not supposed to happen");
            exit = true;
        }

        #region Start Enhance Direction

        if (exit == false)
        {
            float enhanceLength = 0f;

            /// 강화 시작 연출 
            DirectionHandler.Play(
                new ElementDirectionHandler.DirectionParam(ElementDirectionHandler.EnhanceElementDirection.ElementEnhance)
                .SetAttributeType(selectedType)
                .SetWorldPos(nextTargetAttributeWorldPos) // SetWorldPos(oldAttributeWorldPos)
                , out enhanceLength);

            yield return new WaitForSeconds(enhanceLength * 0.85f);
        }

        // this.ScrollAdapter.ScrollToSpecificAttribute(curTid, 0.3f);

        #endregion

        #region Success Or Fail Direction

        float chainReachedLength = 0f;
        float enhanceResultLength = 0f;

        if (exit == false)
        {
            Vector2 resultAttributePos;
            if (ScrollAdapter.GetWorldPos(curTid, out resultAttributePos) == false)
            {
                exit = true;
            }

            if (exit == false)
            {
                var resultDirection =
                    isSuccess ?
                    ElementDirectionHandler.EnhanceElementDirection.ElementEnhance_Success
                    : ElementDirectionHandler.EnhanceElementDirection.ElementEnhance_Fail;

                DirectionHandler.Play(
                    new ElementDirectionHandler.DirectionParam(resultDirection)
                    .SetAttributeType(selectedType)
                    .SetWorldPos(resultAttributePos)
                    , out enhanceResultLength);
            }


            #endregion

            #region Reached New Chain Level Direction

            if (exit == false)
            {
                if (isSuccess)
                {
                    if (Me.CurCharData.GetAttributeChainEffectLevel() > oldChainLevel)
                    {
                        // ZLog.LogError(ZLogChannel.UI, "TODO : Fire NewChainReached");
                        directionHandler.Play(
                            new ElementDirectionHandler.DirectionParam(ElementDirectionHandler.EnhanceElementDirection.ElementEnhance_ReachedChain)
                            .SetPositionSelectOption(ElementDirectionHandler.DirectionParam.PositionSet.X)
                            .SetWorldPos(resultAttributePos)
                            , out chainReachedLength);
                    }
                }
            }
        }

        #endregion

        //yield return new WaitForSeconds(enhanceResultLength);

        UIManager.Instance.Find<UIFrameHUD>().RefreshCurrency();
        UpdateUI(UpdateUIFlag.Slots | UpdateUIFlag.Tab);

        yield return new WaitForSeconds(Mathf.Max(enhanceResultLength, chainReachedLength));

        UICommon.SetNoticeMessage(isSuccess?DBLocale.GetText("Upgrade_Success_Message") : DBLocale.GetText("Upgrade_Failure_Message"), Color.white, 1f, UIMessageNoticeEnum.E_MessageType.SubNotice);

        onFinished?.Invoke();
        ScrollAdapter.SetDragEnable(true);
        SetSelectableStateByDirection(true);
    }

    private void OnDirectionPlay(ElementDirectionHandler.EnhanceElementDirection obj)
    {
        //  UpdateUI
        ZLog.LogError(ZLogChannel.UI, "TODO");
    }

    private void OnAllDirectionTerminated()
    {
        ZLog.LogError(ZLogChannel.UI, "TODO");
    }

    private bool CheckUpdateFlag(UpdateUIFlag source, UpdateUIFlag checkTarget)
    {
        return (source & checkTarget) != 0;
    }


    // TODO : 슬롯 Unlock 연출 
    private void UnlockSlot(UIScrollEnhanceElementSlot slot)
    {
        /*
        if (slot == null)
            return;

        // 이미 Unlock 상태 
        if (slot.IsLocked == false)
            return;
        */
    }

    // 속성 타입별로 모든 abilitAction string 데이터 세팅 
    private void SetupElementAbilityIntegratedTextData()
    {
        EnumDictionary<E_UnitAttributeType, List<AbilityActionTitleValuePairBuildInput>> abilityActionBuildInput = new EnumDictionary<E_UnitAttributeType, List<AbilityActionTitleValuePairBuildInput>>();

        // 속성 타입별 전체 레벨별 순회
        for (int i = 0; i < attributeTypes.Length; i++)
        {
            if (attributeTypes[i].Equals(E_UnitAttributeType.None))
                continue;

            E_UnitAttributeType type = attributeTypes[i];

            // 속성별로 우선 tid 리스트 쫙 세팅함 . 
            //  attributeTidList.Add(type, new List<uint>());
            var attDataList = DBAttribute.GetListByType(attributeTypes[i]);

            abilityActionBuildInput.Add(type, new List<AbilityActionTitleValuePairBuildInput>());
            var buildInputDataList = abilityActionBuildInput[type];

            // 현재 속성의 모든 데이터 순회 . 
            foreach (var attData in attDataList)
            {
                AddAbilityActionID(buildInputDataList, attData.AttributeID, attData.AbilityActionID_01, attData.AbilityActionID_02);
            }

            integratedElementAbilityData.Add(attributeTypes[i], new List<AbilityActionTitleValuePair>(buildInputDataList.Count));

            BuildAbilityActionStrings(buildInputDataList, integratedElementAbilityData[type]);
        }
    }

    /// <summary>
    ///  ++ 출력 방식 수정 
    /// </summary>
    private void SetupElementAbilityIntegratedTextData_Ex()
    {
        EnumDictionary<E_UnitAttributeType, List<AbilityActionTitleValuePairBuildInput>> abilityActionBuildInput = new EnumDictionary<E_UnitAttributeType, List<AbilityActionTitleValuePairBuildInput>>();

        // 속성 타입별 전체 레벨별 순회
        for (int i = 0; i < attributeTypes.Length; i++)
        {
            if (attributeTypes[i].Equals(E_UnitAttributeType.None))
            {
                IntegratedElementAbilityData_Ex.Add(E_UnitAttributeType.None, new List<ScrollEnhanceElementIntegratedData>());
                continue;
            }

            E_UnitAttributeType type = attributeTypes[i];

            // 속성별로 우선 tid 리스트 쫙 세팅함 . 
            //  attributeTidList.Add(type, new List<uint>());
            var attDataList = DBAttribute.GetListByType(attributeTypes[i]);

            abilityActionBuildInput.Add(type, new List<AbilityActionTitleValuePairBuildInput>());
            var buildInputDataList = abilityActionBuildInput[type];

            // 현재 속성의 모든 데이터 순회 . 
            foreach (var attData in attDataList)
            {
                AddAbilityActionID(buildInputDataList, attData.AttributeID, attData.AbilityActionID_01, attData.AbilityActionID_02);
            }

            integratedElementAbilityData_Ex.Add(attributeTypes[i], new List<ScrollEnhanceElementIntegratedData>(buildInputDataList.Count));

            BuildAbilityActionStrings_Ex(buildInputDataList, integratedElementAbilityData_Ex[type]);
        }
    }

    // 속성연계 레벨별 ability action 들 세팅 
    private void SetupChainEffectAbilityIntegratedTextData()
    {
        Dictionary<uint, List<AbilityActionTitleValuePairBuildInput>> abilityActionBuildInput = new Dictionary<uint, List<AbilityActionTitleValuePairBuildInput>>();
        uint chainMinLevel = DBAttribute.GetAttributeChainMinLevel();
        uint chainMaxLevel = DBAttribute.GetAttributeChainMaxLevel();

        for (uint level = chainMinLevel; level <= chainMaxLevel; level++)
        {
            var data = DBAttribute.GetAttributeChainByLevel(level);

            if (data == null)
                continue;

            abilityActionBuildInput.Add(level, new List<AbilityActionTitleValuePairBuildInput>());
            var buildInputDataList = abilityActionBuildInput[level];

            AddAbilityActionID(buildInputDataList, data.AttributeChainID,
                data.AbilityActionID_01
                , data.AbilityActionID_02
                , data.AbilityActionID_03
                , data.AbilityActionID_04);

            BuildAbilityActionStrings(buildInputDataList, integratedChainEffectAbilityData);
        }
    }

    // 중앙 기준 오프셋 
    void OpenToolTip(
        ToolTipType type
        , RectTransform sourceRectTransform
        , string txtTitle
        , Color titleTxtColor
        , Color abilityTxtColor
        , Action onClosed
        , List<AbilityActionTitleValuePairBuildInput> abilityIDInputData)
    {
        toolTipPopUps.ForEach(t => t.target?.gameObject.SetActive(false));

        var targetToolTip = toolTipPopUps.Find(t => t.type == type);

        if (targetToolTip == null)
        {
            ZLog.LogError(ZLogChannel.UI, "Could not find target ToolTip : " + type.ToString());
            return;
        }

        List<AbilityActionTitleValuePair> abilities = new List<AbilityActionTitleValuePair>();

        BuildAbilityActionStrings(abilityIDInputData, abilities);

        targetToolTip.canvasGroup.alpha = 0;

        targetToolTip.target.Open(txtTitle, titleTxtColor, abilityTxtColor, abilities, onClosed);

        targetToolTip.target.transform.position = sourceRectTransform.position;
        targetToolTip.target.RectTransform.ForceUpdateRectTransforms();

        if (targetToolTip.offset.y <= 0)//하단으로뜰때
        {
            targetToolTip.target.RectTransform.anchoredPosition = new Vector3(
                targetToolTip.target.RectTransform.anchoredPosition.x + targetToolTip.offset.x
                , targetToolTip.target.RectTransform.anchoredPosition.y + targetToolTip.offset.y
                , 0);
        }
        else
        {
            //상단으로 뜰때
            targetToolTip.target.RectTransform.anchoredPosition = new Vector3(
                targetToolTip.target.RectTransform.anchoredPosition.x + targetToolTip.offset.x
                , targetToolTip.target.RectTransform.anchoredPosition.y + targetToolTip.offset.y + targetToolTip.target.BoardHeight
                , 0);
        }

        // 화면 넘어가는 경우 예외처리 
        // 툴팁 layout 이 해당 프레임에 업데이트되지않아 코루틴으로 다음 프레임에 업데이트함.
        // targetToolTip.target.RectTransform.TryClampPositionToParentBoundary();

        targetToolTip.shouldClamp = true;
        StartCoroutine(ClampToolTip());
    }

    IEnumerator ClampToolTip()
    {
        foreach (var tooltip in toolTipPopUps)
        {
            if (tooltip.shouldClamp)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(tooltip.target.RectTransform);
            }
        }

        yield return null;

        foreach (var tooltip in toolTipPopUps)
        {
            if (tooltip.shouldClamp)
            {
                tooltip.shouldClamp = false;
                tooltip.canvasGroup.alpha = 1;
                tooltip.target.RectTransform.TryClampPositionToParentBoundary();
            }
        }
    }

    private void SetDetailWindow(bool open, uint skipFrame = 0)
    {
        deactivateListOnDetailWindow.ForEach(t => t.gameObject.SetActive(open == false));

        if (open)
        {
            detailOverlayScreen.Open(framesToSkip: 1);
        }
        else
        {
            detailOverlayScreen.Close();
        }
    }

    void HandleSlotClicked(ScrollEnhanceElementData scrollData, UIEnhanceElementSlot slot)
    {
        var attributeData = scrollData.TableData;
        var abilityActionIDs = new List<AbilityActionTitleValuePairBuildInput>();
        var titleKey = string.Empty;
        Color contentTxtColor = Me.CurCharData.IsThisAttributeObtained_ByID(attributeData.AttributeID) ?
            settingInfoProvider.tooltipTxtOnActive :
            settingInfoProvider.tooltipTxtOnInactive;

        // WARNING : AbilityAction 항목 늘어나면 코드 추가해야함 . 
        AddAbilityActionID(abilityActionIDs, attributeData.AttributeID, attributeData.AbilityActionID_01, attributeData.AbilityActionID_02);

        ScrollAdapter.SetActiveElementEdge(attributeData.AttributeID, true);

        OpenToolTip(
            ToolTipType.Slot
            , slot.RectTransform
            , DBLocale.GetText(SettingProvider.GetCommonTitleNameByType(attributeData.AttributeType))
            + string.Format(" " + DBLocale.GetText("Attribute_Level"), attributeData.AttributeLevel)
            , titleTxtColor: settingInfoProvider.GetAttributeToolTipTitleColor(attributeData.AttributeID)
            , abilityTxtColor: contentTxtColor
            , onClosed: () =>
			{
				ScrollAdapter.SetActiveElementEdge(attributeData.AttributeID, false);
			}
			 , abilityActionIDs);
	}

	// TODO : 누르면 해금 연출이 나온다는데 해금 연출은 어디서 가져와야하나 ? 
	private void HandleChainEffectIconClicked(UIEnhanceElementChainLevel source)
    {
        var data = DBAttribute.GetAttributeChainByLevel(source.ChainLevel);

        if (data == null)
            return;

        List<AbilityActionTitleValuePairBuildInput> abilityActionIDs = new List<AbilityActionTitleValuePairBuildInput>();
        Color contentTxtColor = settingInfoProvider.chainAndEnhanceProvider.GetChainEffectToolTipContentColor(source.ChainLevel);

        // WARNING : AbilityAction 항목 늘어나면 코드 추가해야함 . 
        AddAbilityActionID(
            abilityActionIDs
            , data.AttributeChainID
            , data.AbilityActionID_01
            , data.AbilityActionID_02
            , data.AbilityActionID_03
            , data.AbilityActionID_04);

        OpenToolTip(
            ToolTipType.ChainEffect
            , source.ButtonRectTransform
            , string.Format(DBLocale.GetText("Attribute_Chain_Title"), source.ChainLevel)
            , settingInfoProvider.chainAndEnhanceProvider.GetChainEffectTitleTxtColor(source.ChainLevel)
			, contentTxtColor
			, onClosed: () =>
			{

			}
			, abilityActionIDs);
    }

    private void AddAbilityActionID(List<AbilityActionTitleValuePairBuildInput> outputAbilityActionIDs, uint sourceID, params uint[] abilityActionIDs)
    {
        for (int i = 0; i < abilityActionIDs.Length; i++)
            outputAbilityActionIDs.Add(new AbilityActionTitleValuePairBuildInput() { sourceTid = sourceID, abilityActionTid = abilityActionIDs[i] });
    }

    private bool IsThisChainEffectObtained(uint level)
    {
        return DBAttribute.IsThisAttributeChainLevelObtained(level, Me.CurCharData.GetAttributeMinLevel());
    }

    //private void OnEnhanceTried(bool isSuccess)
    //{
    //    UIManager.Instance.Find<UIFrameHUD>().RefreshCurrency();
    //    UpdateUI(UpdateUIFlag.Slots | UpdateUIFlag.Tab);
    //}

    #region Tab 
    private void SetElementTab(E_UnitAttributeType type)
    {
        if (tabTrigger.SelectedType.Equals(type))
        {
            type = E_UnitAttributeType.None;
        }

        tabTrigger.Select(type);
        figureBoard.Select(type);

        if (tabTrigger.SelectedType.Equals(E_UnitAttributeType.None))
        {
            tabWindow.OpenChainEffectTab();
        }
        else
        {
            tabWindow.OpenEnhanceTab(type);
        }

        UpdateUI(UpdateUIFlag.Tab);
    }

    private void CloseEnhanceTab()
    {
        SetElementTab(E_UnitAttributeType.None);
    }
    #endregion

    void PrintLogError(string str)
    {
        ZLog.LogError(ZLogChannel.UI, str);
    }
    #endregion

    #region OnClick Event (인스펙터 연결)
    // WARNING : typeIndex 는 E_UnitAttributeType 의 Index 를 의미함. 
    public void OnClickTabBtn(int typeID)
    {
        if (initialized == false)
        {
            return;
        }

        if (typeID == (int)E_UnitAttributeType.None ||
            Array.Exists(attributeTypes, t => typeID == (int)t) == false)
        {
            ZLog.LogError(ZLogChannel.UI, "속성 UI 창의 탭 버튼 타입 인덱스를 확인해주세요.");
            return;
        }

        SetElementTab((E_UnitAttributeType)typeID);

        uint AttributeId = Me.CurCharData.GetAttributeIDByType((E_UnitAttributeType)typeID);

        if (AttributeId != 0)
            ScrollAdapter.ScrollToSpecificAttribute(AttributeId, 0.3f,true);
    }
    public void OnClickDetailBtn()
    {
        SetDetailWindow(true);
    }

    public void OnClickDetailCloseBtn()
    {
        SetDetailWindow(false);
    }

    public void OnClickEnhanceTabClose()
    {
        CloseEnhanceTab();
    }
    #endregion
}

// UI 의 세팅 정보들을 들고있음 . 통이 하도 커져서 따로 분류 . 
[Serializable]
public class UIEnhanceElementSettingProvider
{
    [Serializable]
    public class TypeGroupProperty
    {
        public E_UnitAttributeType type;

        public string commonTitleName;
        public Color commonColor;

        // bg 색상 
        public Color bgColorOnActive;

        public Color iconLineAndEnhanceColorOnActive;
        public Color bgInnerGlowOnActive;
        public Color bgEnhanceOnActive;
        public Color slotToolTipTitleColorOnActive;
    }

    [Header("슬롯 관련 세팅들")]
    public List<TypeGroupProperty> typeGroupProps;
    public Color bgColorOnInactive;
    public Color iconLineAndEnhanceColorOnInactive;

    public Color bgEnhanceOnInactive;
    public Color tooltipTxtOnActive;
    public Color tooltipTxtOnInactive;

    public Color tooltipTitleTxtOnInactive;

    [Header("속성연계 효과 및 강화 정보 Provider")]
    public UIEnhanceElementChainAndEnhanceSettingProvider chainAndEnhanceProvider;

    private Predicate<uint> ChainEffectLevelObtainChecker;

    #region Public Methods
    public void Initialize(Predicate<uint> chainEffectLevelObtainChecker)
    {
        ChainEffectLevelObtainChecker = chainEffectLevelObtainChecker;
        chainAndEnhanceProvider.Initialize(chainEffectLevelObtainChecker);
    }

    public Color GetColorActive_BG(E_UnitAttributeType type)
    {
        Color result = Color.black;
        var prop = FindTypeGroupProperty(type);

        if (prop != null)
        {
            result = prop.bgColorOnActive;
        }

        return result;
    }

    public Color GetColorActive_IconLineAndEnhance(E_UnitAttributeType type)
    {
        Color result = Color.black;
        var prop = FindTypeGroupProperty(type);

        if (prop != null)
        {
            result = prop.iconLineAndEnhanceColorOnActive;
        }

        return result;
    }

    public Color GetColorActive_SlotToolTipTitleText(E_UnitAttributeType type)
    {
        Color result = Color.black;
        var prop = FindTypeGroupProperty(type);

        if (prop != null)
        {
            result = prop.slotToolTipTitleColorOnActive;
        }

        return result;
    }

    public Color GetColorInactive_BG()
    {
        return bgColorOnInactive;
    }

    public Color GetColorInactive_IconLineAndEnhance()
    {
        return iconLineAndEnhanceColorOnInactive;
    }

    public Color GetColorInactive_BGEnhance()
    {
        return bgEnhanceOnInactive;
    }

    public Color GetColorActive_ToolTipText()
    {
        return tooltipTxtOnActive;
    }

    public Color GetColorInactive_ToolTipText()
    {
        return tooltipTxtOnInactive;
    }

    public string GetCommonTitleNameByType(E_UnitAttributeType type)
    {
        var target = FindTypeGroupProperty(type);
        if (target == null)
            return string.Empty;
        return target.commonTitleName;
    }

    public Color GetCommonColorByType(E_UnitAttributeType type)
    {
        var target = FindTypeGroupProperty(type);
        if (target == null)
            return Color.black;
        return target.commonColor;
    }

    #region 직접 데이터 검사해서 리턴 함수들 
    public Color GetAttributeToolTipTitleColor(uint attributeID)
    {
        Color result = tooltipTitleTxtOnInactive;
        if (Me.CurCharData.IsThisAttributeObtained_ByID(attributeID))
        {
            var group = FindTypeGroupProperty(DBAttribute.GetTypeByTID(attributeID));

            if (group != null)
                result = group.slotToolTipTitleColorOnActive;
        }
        return result;
    }
    #endregion

    public TypeGroupProperty FindTypeGroupProperty(E_UnitAttributeType type)
    {
        TypeGroupProperty target = null;

        for (int i = 0; i < typeGroupProps.Count; i++)
        {
            if (typeGroupProps[i].type.Equals(type))
            {
                target = typeGroupProps[i];
                break;
            }
        }

        return target;
    }

    #endregion

    // Inspector 서 잘못 넣어준 값이 있는지 체크함 . 
    void ValidateInspectorValues()
    {
        #region Element Group Property 체크 

        if (typeGroupProps.Exists(t => t.type.Equals(E_UnitAttributeType.None)))
            PrintLogError("None 타입 세팅하지 마십시오.");

        var elementTypes = (E_UnitAttributeType[])Enum.GetValues(typeof(E_UnitAttributeType));

        #region 타입 중복 세팅 체크 
        for (int i = 0; i < elementTypes.Length; i++)
        {
            if (elementTypes[i].Equals(E_UnitAttributeType.None))
                continue;

            var type = elementTypes[i];
            int typeCnt = 0;

            for (int j = 0; j < typeGroupProps.Count; j++)
            {
                if (type.Equals(typeGroupProps[j].type))
                {
                    typeCnt++;
                }
            }

            if (typeCnt != 1)
                PrintLogError(nameof(typeGroupProps) + " None 타입 제외한 , 나머지 속성별 반드시 설정되어있어야함 . Type : " + type.ToString());
        }
        #endregion
        #endregion
    }

    void PrintLogError(string str)
    {
        ZLog.LogError(ZLogChannel.UI, str);
    }
}

[Serializable]
public class UIEnhanceElementChainAndEnhanceSettingProvider
{
    [Serializable]
    public class ChainEffect
    {
        public Color imgColorOnActive;
        public Color imgColorOnInactive;

        public Color toolTipContentColorOnActive;
        public Color toolTipContentColorOnInactive;

        public Color effectTitleColorOnActive;
        public Color effectTitleColorOnInactive;

        public Color valueColorOnActive;
        public Color valueColorOnInactive;
    }

    [Serializable]
    public class Enhance
    {
        public Color txtTitleColorOnActive;
        public Color txtContentColorOnActive;
    }

    public ChainEffect chainEffect;
    public Enhance enhance;

    public Color enhanceTitleContentColorOnInactive;

    private Predicate<uint> ChainEffectLevelObtainChecker;

    public void Initialize(Predicate<uint> chainEffectLevelObtainChecker)
    {
        ChainEffectLevelObtainChecker = chainEffectLevelObtainChecker;
    }

    #region Public Methods
    public void GetEnhanceTextColor(E_UnitAttributeType type, uint level, out Color titleColor, out Color contentColor)
    {
        bool isObtained = Me.CurCharData.IsThisAttributeObtained_ByLevel(type, level);
        titleColor = isObtained ? enhance.txtTitleColorOnActive : enhanceTitleContentColorOnInactive;
        contentColor = isObtained ? enhance.txtContentColorOnActive : enhanceTitleContentColorOnInactive;
    }

    public Color GetChainEffectImgColor(uint level)
    {
        return ChainEffectLevelObtainChecker(level) ?
            chainEffect.imgColorOnActive : chainEffect.imgColorOnInactive;
    }

    public Color GetChainEffectToolTipContentColor(uint level)
    {
        return ChainEffectLevelObtainChecker(level)
            ? chainEffect.toolTipContentColorOnActive : chainEffect.toolTipContentColorOnInactive;
    }

    public Color GetChainEffectTitleTxtColor(uint level)
    {
        return ChainEffectLevelObtainChecker(level)
            ? chainEffect.effectTitleColorOnActive : chainEffect.effectTitleColorOnInactive;
    }
    #endregion
}