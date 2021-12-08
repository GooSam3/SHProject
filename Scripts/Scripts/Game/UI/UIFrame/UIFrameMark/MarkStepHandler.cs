using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using frame8.Logic.Misc.Other.Extensions;
using Devcat;
using ZNet.Data;
using GameDB;
using WebNet;
using ZNet;
using System.Text;
using static UIFrameMark;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MarkStepHandler : MonoBehaviour
{
    public enum EnhanceStatus
    {
        None = 0,
        Idle, /// 가만히 대기 상태 
        Enhancing /// 강화 연출중 
    }

    public enum ReEnhanceType
    {
        None = 0,
        Loop, /// 계속 반복 
        RemainedCount, /// 횟수만큼 실행후 종료 
        TargetStep, /// 특정 Step 까지 도달후 종료 
        ScheduleStop, /// 그만하기 
    }

    /// <summary>
    /// 강화 가능 조건 여부 타입 
    /// </summary>
    public enum EnhanceAdvanceCheckCondition
    {
        None = 0,
        Advance, // 강화 조건 충족 
        Currency_Gold, // 골드 부족 
        Currency_Assence, // Assence 아이템 부족 
        MaxStep, // 최대 레벨 도달 
    }

    #region SerializedField
    #region Preference Variable
    [Header("Mark ToolTip 의 중앙 기준 출력 Offset 값")]
    [SerializeField] private Vector2 offsetMarkDetailToolTipFromCenter;
    #endregion

    #region UI Variables
    [SerializeField] private List<GameObject> activeOnWide;
    [SerializeField] private List<GameObject> activeOnDetail;

    [SerializeField] private GameObject markGroupRoot_fireDragon;
    [SerializeField] private GameObject markGroupRoot_waterDragon;
    [SerializeField] private GameObject markGroupRoot_electricDragon;

    /// <summary>
    /// 문양 하나의 Stat 정보 ToolTip
    /// </summary>
    [SerializeField] private MarkDetailToolTip markStatToolTip;

    /// <summary>
    /// 사이즈가 커다란 문양 타입별 설명 info 팝업임 . (toolTip 과 헷갈 X)
    /// </summary>
    [SerializeField] private MarkMarkInfoPopup markInfoPopup;

    [SerializeField] private RectTransform temporaryDisplayUIcloser;

    [SerializeField] private MarkDirectionHandler directionHandler;

    #region Wide Group
    [SerializeField]
    private UIAbilityListAdapter ScrollAdapter_WideView;

    [Header("가장 하위 레벨은 포함시키지 않습니다.) 화염의 드래곤 문양 세팅(하나의 문양의 최초 Tid 가 1000, 1001, 1002 라면 StartTid 에는 1001 을 입력합니다")]
    [SerializeField] private List<MarkGroup> marks_fireDragon;

    [Header("물의 드래곤 문양 세팅")]
    [SerializeField] private List<MarkGroup> marks_waterDragon;

    [Header("번개의 드래곤 문양 세팅")]
    [SerializeField] private List<MarkGroup> marks_electricDragon;

    [Space]
    [SerializeField] private RectTransform connectorRoot;
    [SerializeField] private MarkConnectorUI connectorSourceObj;

    [SerializeField] private EnhanceProtector enhanceProtector;

    [SerializeField] private CurrencyGroup currencyUI_essence;
    [SerializeField] private CurrencyGroup currencyUI_gold;

    [SerializeField] private List<WideViewEffectGameObjectByLevel> wideViewDragonEffectObjs;
    #endregion

    #region Detail Group
    [SerializeField] private UIAbilityListAdapter ScrollAdapter_DetailView;

    [SerializeField] private MarkEnhanceIndicatorModel detailMarkModel;
    [SerializeField] private RectTransform selectedDetailMarkObj;

    [SerializeField] private List<GameObject> activeOnDisplayCost;
    [SerializeField] private Button btnEnhance;
    [SerializeField] private Button btnSerialEnhance;
    [SerializeField] private Button btnExitOnBottom;
    [SerializeField] private Slider serialEnhanceProgressBar;

    [SerializeField] private Image imgMarkStepIconBGOnDetail;
    //[SerializeField] private Image imgMarkStepIconOnDetail;
    [SerializeField] private Text txtMarkNameOnDetail;
    [SerializeField] private Text txtTypeNameOnDetail;

    [SerializeField] private Text txtWideStatTitleMarkGroupType;
    [SerializeField] private Text txtWideStatTitleStep;

    [SerializeField] private List<Selectable> selectableObjsControlByDirecting_wideView;
    [SerializeField] private List<Selectable> selectableObjsControlByDirecting_detailView;
    #endregion
    #endregion
    #endregion

    #region System Variables
    private StepTab currentTab;
    private UIFrameMark FrameMark;

    /// <summary>
    /// 인스펙터로 꽃힌 마크들을 타입별로 재분류 
    /// </summary>
    private EnumDictionary<GameDB.E_MarkAbleType, List<MarkGroup>> marksByType;

    private List<MarkConnectorUI> connectors;

    /// <summary>
    /// 어떤 Group 을 Detail 을 눌렀을떄 띄어줄지 등 현재 Context Info 캐싱 
    /// context 에 관한 info 다 싶으면 이 클래스로 통제함. 
    /// </summary>
    private ContextInfo contextInfo;

    // private SimpleTimeAnimation enhancingTimeAni;
    private EnhanceProp enhancingProp;

    // private UIUpdateContextInfo uiUpdateContextInfo;
    private ExtendedDirectionInfo directionInfo;

    private bool isGroupObtainedWithinDetailView;

    private bool isDataDirty;

    private bool initDone;
    #endregion

    #region Properties 
    List<MarkGroup> CurrentMarkGroupList { get { return marksByType[FrameMark.CurrentMainTab]; } }
    #endregion

    #region Unity Methods
    private void Awake()
    {
        if (connectorSourceObj.gameObject.activeSelf)
            connectorSourceObj.gameObject.SetActive(false);
        if (selectedDetailMarkObj.gameObject.activeSelf)
            selectedDetailMarkObj.gameObject.SetActive(false);
        CloseMarkStatToolTip();
    }

    private void Update()
    {
        if (isDataDirty)
        {
            UpdateData(false);
        }

        if (enhancingProp != null)
        {
            // UpdateEnhancingAnimation();

            /// --- 재강화 로직 ---
            /// CAUTION : Data Dirty 상태에서는 강화 재시작을 시작하지않음.
            if (isDataDirty == false
                && enhancingProp.scheduledReEnhance
                && enhancingProp.currentEnhanceStatus == EnhanceStatus.Idle)
            {
                /// 재강화 가능 여부 체킹 
                bool advanceReEnhance = UpdateAdvanceReEnhanceState();

                /// 재강화 그대로 진행함 
                if (advanceReEnhance)
                {
                    enhancingProp.scheduledReEnhance = false;

                    if (StartEnhance(true) == false)
                    {
                        enhancingProp.ResetAll();
                        FrameMark.RefreshCurrentStepTab();
                    }
                }
                else
                {
                    StopEnhance();
                }
            }
        }
    }

    private void OnDisable()
    {
        temporaryDisplayUIcloser.gameObject.SetActive(false);
        markInfoPopup.gameObject.SetActive(false);
        markStatToolTip.gameObject.SetActive(false);
    }
    #endregion

    #region Public Methods
    public bool Initialize(UIFrameMark frameMark)
    {
        if (ValidateData() == false)
        {
            return false;
        }

        FrameMark = frameMark;

        #region 문양 세팅
        marksByType = new EnumDictionary<GameDB.E_MarkAbleType, List<MarkGroup>>();
        marksByType.Add(GameDB.E_MarkAbleType.None, new List<MarkGroup>());
        marksByType.Add(GameDB.E_MarkAbleType.RecoveryMark, marks_fireDragon);
        marksByType.Add(GameDB.E_MarkAbleType.AttackMark, marks_waterDragon);
        marksByType.Add(GameDB.E_MarkAbleType.DefenseMark, marks_electricDragon);

        /// 인스펙터에 꽃혀진 문양들을 순회하며 세팅
        foreach (var groupByType in marksByType)
        {
            for (int i = 0; i < groupByType.Value.Count; i++)
            {
                var markGroup = groupByType.Value[i];
                var sharedInfo = FrameMark.GetSharedInfo(groupByType.Key);
                var subMarkCommonSprite = sharedInfo.wideViewSubMarkSprite;

                markGroup.markNode.Initialize(
                    frameMark
                    , (uint)markGroup.startTid
                    , Me.CurCharData.GetMarkStep(groupByType.Key)
                    , i
                    , subMarkCommonSprite
                    , frameMark.GetSpriteGroupCenterStep(i)
                    , sharedInfo.markGroupCircleEffectGlowOnObtained
                    , sharedInfo.markGroupEffectGlowObjOnObtained
                    , sharedInfo.markGroupSmallEffect_UnDoneNotNextEnhance
                    , sharedInfo.markGroupSmallEffect_NextEnhanceTarget
                    , sharedInfo.markGroupSmallEffect_UnDoneComplete
                    , sharedInfo.markGroupSmallEffect_DoneState
                    , HandleMarkGroupClicked);
            }
        }
        #endregion

        #region 마크 Connector 세팅
        /// Connector 그냥 미리 필요한 만큼 할당함 
        int requiredConnectorMaxCount = 0;

        foreach (var groups in marksByType)
        {
            /// Group 이 2개다 -> Connector 1개 필요 
            /// Group 이 3개다 -> Connector 2개 필요
            int reqCnt = groups.Value.Count - 1;

            if (reqCnt > requiredConnectorMaxCount)
            {
                requiredConnectorMaxCount = reqCnt;
            }
        }

        /// 요구되는 Connector 가 0 개면 하나만 할당하자 . 어차피 쓸일은 없음 .
        if (requiredConnectorMaxCount == 0)
            requiredConnectorMaxCount = 1;

        connectors = new List<MarkConnectorUI>(requiredConnectorMaxCount);

        for (int i = 0; i < requiredConnectorMaxCount; i++)
            AddConnector();
        #endregion

        #region Enhancing 관련 세팅
        enhancingProp = new EnhanceProp();
        SetEnhancingToIdle();
        #endregion

        #region Direction Handler
        directionHandler.Initialize();

        #endregion

        #region ETC

        #region ContextInfo 
        contextInfo = new ContextInfo();

        /// 혹시라도 기기에 저장해놓고 한번에 detail view shortcut 을 간다던가 하면 여기서 설정해줘야할것임.
        contextInfo.dataByType.Add(new ContextInfo.ContextInfoCacheByType() { type = GameDB.E_MarkAbleType.None });
        contextInfo.dataByType.Add(new ContextInfo.ContextInfoCacheByType() { type = GameDB.E_MarkAbleType.RecoveryMark });
        contextInfo.dataByType.Add(new ContextInfo.ContextInfoCacheByType() { type = GameDB.E_MarkAbleType.AttackMark });
        contextInfo.dataByType.Add(new ContextInfo.ContextInfoCacheByType() { type = GameDB.E_MarkAbleType.DefenseMark });

        // uiUpdateContextInfo = new UIUpdateContextInfo();
        directionInfo = new ExtendedDirectionInfo();
        #endregion

        #region Detail View
        detailMarkModel.Initialize(HandleSubMarkClickedOnDetailView);
        #endregion

        #region ScrollAdapter 
        ScrollAdapter_WideView.Initialize_SkipPrefabManualLoading();
        ScrollAdapter_DetailView.Initialize_SkipPrefabManualLoading();
        #endregion

        #endregion

        initDone = true;
        return true;
    }

    /// <summary>
    /// 지금 어떤 상태이던 , 강화중이던 뭐던 그냥 다 Release 시킴 .
    /// 이걸 언제 사용해야할지는 좀더 고려를 해봐야함 . 
    /// </summary>
    public void Release()
    {
        SetEnhancingToIdle();
        detailMarkModel.ReleaseState();
        // uiUpdateContextInfo.Reset();
        if (directionInfo != null)
            directionInfo.Reset();
        directionHandler.Release();
        isGroupObtainedWithinDetailView = false;
        selectableObjsControlByDirecting_wideView.ForEach(t => t.interactable = true);
        selectableObjsControlByDirecting_detailView.ForEach(t => t.interactable = true);
    }

    /// <summary>
    /// MainTab 이 Switch 되기 전에 필요한 세팅 
    /// </summary>
    public void OnPreSwitchMainTab(GameDB.E_MarkAbleType newMainTab, StepTab newStepTab)
    {
        if (newMainTab != GameDB.E_MarkAbleType.None || newStepTab != StepTab.None)
        {
            if (newStepTab == StepTab.Detail
                && currentTab != newStepTab)
            {
                var info = GetContextByTypeInfo(newMainTab);
                if (info != null && info.entryGroupIndex_detailView == -1)
                {
                    SetContextInfo_DetailViewEntryGroupIndexByType(
                        newMainTab
                        , GetNextEnhanceGroupIndex(newMainTab)
                        , (updatedContextInfo) =>
                        {
                            SetContextInfo_DefaultTargetMarkForDisplay(updatedContextInfo);
                        });
                }
            }
        }

        /// 각종 State Reset 처리 
        if (enhancingProp != null)
        {
            enhancingProp.ResetAll();
        }

        if (directionInfo != null)
        {
            directionInfo.Reset();
        }
    }

    public void SetTab(StepTab tab)
    {
        bool tabChanged = currentTab != tab;
        currentTab = tab;
        UpdateData(tabChanged);

        if (tabChanged)
        {
            OnTabChanged(tab);
        }
    }

    public void UpdateData(bool tabChanged, bool skipMarkModelOnDetail = false)
    {
        if (currentTab != StepTab.Detail)
        {
            if (enhancingProp != null)
            {
                enhancingProp.ResetAll();
            }

            //if (uiUpdateContextInfo != null)
            //{
            //    uiUpdateContextInfo.Reset_ShowExitBtn();
            //}
        }

        if (currentTab == StepTab.Detail && tabChanged)
        {
            if (directionInfo != null)
                directionInfo.Reset();
        }

        switch (currentTab)
        {
            case StepTab.None:
                break;
            case StepTab.Wide:
                {
                    uint fireStep = Me.CurCharData.GetMarkStep(GameDB.E_MarkAbleType.RecoveryMark);
                    uint waterStep = Me.CurCharData.GetMarkStep(GameDB.E_MarkAbleType.AttackMark);
                    uint electricStep = Me.CurCharData.GetMarkStep(GameDB.E_MarkAbleType.DefenseMark);

                    for (int i = 0; i < marks_fireDragon.Count; i++)
                    {
                        marks_fireDragon[i].markNode.UpdateInfo(fireStep);
                    }
                    for (int i = 0; i < marks_waterDragon.Count; i++)
                    {
                        marks_waterDragon[i].markNode.UpdateInfo(waterStep);
                    }
                    for (int i = 0; i < marks_electricDragon.Count; i++)
                    {
                        marks_electricDragon[i].markNode.UpdateInfo(electricStep);
                    }
                }
                break;
            case StepTab.Detail:
                {
                    if (skipMarkModelOnDetail == false)
                    {
                        var curType = FrameMark.CurrentMainTab;
                        var myData = Me.CurCharData.GetMarkDataByType(curType);
                        int groupIndex = GetContextByTypeInfo(curType).entryGroupIndex_detailView;

                        if (groupIndex != -1 && myData != null)
                        {
                            detailMarkModel.SetBaseInfo(
                                FrameMark.CurrentMainTab
                                , myData.Step
                                , (uint)GetMarkGroupByIndex(FrameMark.CurrentMainTab, groupIndex).startTid
                                , groupIndex
                                , FrameMark.GetSpriteGroupCenterStep(groupIndex) // FrameMark.GetRepresentSprite(curType)
                                , FrameMark.GetSharedInfo(curType).detailViewGaugeSprite
                                , FrameMark.GetColorByType(curType)
                                , FrameMark.InactiveColor);
                        }
                        else
                        {
                            detailMarkModel.ReleaseState();
                        }
                    }
                }
                break;
            default:
                break;
        }

        if (skipMarkModelOnDetail == false)
        {
            isDataDirty = false;
        }
    }

    public void UpdateUI(
        bool skipMarkModelOnDetail = false
        , bool skipBtnOnDetail = false
        , bool skipSelectObjOnDetail = false
        , bool skipStatTitleOnDetail = false
        , bool skipProtectorAndCostInfoOnDetail = false
        , bool skipAbilityOnDetail = false)
    {
        /// '에러' 와 동급인 수준의 valid 체킹임. 즉 
        /// false 면 UI 자체를 표시를 안하고 있음 . 
        bool valid = currentTab != StepTab.None;

        var curType = FrameMark.CurrentMainTab;
        var myKeyData = Me.CurCharData.GetMarkDataByType(curType);
        bool isMastered = Me.CurCharData.IsMarkMaxStep(curType);
        Mark_Table myMarkData = myKeyData != null ? DBMark.GetMarkData(myKeyData.MarkTid) : null;
        var curContextInfo = GetContextByTypeInfo(curType);

        if (myKeyData == null)
        {
            ZLog.LogError(ZLogChannel.UI, "my given type of mark does not exist , Type : " + curType);
        }
        else if (myMarkData == null)
        {
            ZLog.LogError(ZLogChannel.UI, "my given mark not found in table , Tid : " + myKeyData.MarkTid);
        }
        else if (curContextInfo == null)
        {
            ZLog.LogError(ZLogChannel.UI, "my contextInfo not exist , Type : " + curType);
        }
        else
        {
            /// 모든 Mark 레벨들이 라인끼리 줄지어 있는 부분 UI 업데이트 
            if (currentTab == StepTab.Wide)
            {
                var curGroupList = CurrentMarkGroupList;

                /// 만약 해당 타입의 내 Mark 데이터가 없다면 
                /// 에러 수준임 
                valid = valid && myKeyData != null && curGroupList.Count > 0;

                #region ---------- 문양 노드 업데이트 ------------

                if (valid)
                {
                    var colorForType = FrameMark.GetColorByType(curType);
                    var inactiveColor = FrameMark.InactiveColor;

                    /// 전체 Mark Group 을 순회하며 세팅
                    foreach (var groups in marksByType)
                    {
                        var root = GetMarkGroupRootByType(groups.Key);
                        bool targetMarkGroup = curType == groups.Key;

                        if (targetMarkGroup)
                        {
                            for (int i = 0; i < groups.Value.Count; i++)
                            {
                                var group = groups.Value[i];
                                group.markNode.UpdateUI(isMastered, myKeyData.Step, group.markNode.HaveGivenStep((byte)((uint)myKeyData.Step + 1)), FrameMark.GetSharedInfo(curType).markGroupStepSmallSpriteByOrder, colorForType, inactiveColor);
                            }
                        }

                        if (root != null)
                            root.SetActive(targetMarkGroup);
                    }
                }
                else
                {
                    SetMarkGroupRootActiveRespectively(false, false, false);
                }
                #endregion

                #region ---------- 문양 배경 드래곤 이펙트 관련 업데이트 -------------

                UpdateUI_WideViewDragonImpact();

                #endregion

                #region ---------- 문양 Connector 업데이트 ------------

                if (valid)
                {
                    int connectorCnt = curGroupList.Count - 1;

                    for (int i = 0; i < connectors.Count; i++)
                    {
                        if (i < connectorCnt)
                        {
                            var connector = connectors[i];
                            var markFrom = curGroupList[i].markNode;
                            var markTo = curGroupList[i + 1].markNode;
                            var dirToTarget = (markTo.RectTransform.anchoredPosition - markFrom.RectTransform.anchoredPosition).normalized;

                            float distance = (markTo.RectTransform.anchoredPosition - markFrom.RectTransform.anchoredPosition).magnitude;
                            var rt = connector.rt;
                            bool isActiveConnector = myKeyData.Step >= markFrom.TopStep && myKeyData.Step >= markTo.TopStep;

                            rt.sizeDelta = new Vector2(distance, rt.sizeDelta.y);
                            rt.anchoredPosition = markFrom.RectTransform.anchoredPosition;
                            /// 현재 x pivot 이 0 이고 width 를 늘리기 때문에 기준 방향을 오른쪽으로 설정 
                            rt.localRotation = Quaternion.FromToRotation(Vector3.right, dirToTarget);
                            connector.img.color = isActiveConnector ? connector.activeColor : FrameMark.InactiveColor;

                            connector.gameObject.SetActive(true);
                        }
                        else
                        {
                            connectors[i].gameObject.SetActive(false);
                        }
                    }
                }
                else
                {
                    connectors.ForEach(t => t.gameObject.SetActive(false));
                }
                #endregion

                #region Stat Part

                txtWideStatTitleMarkGroupType.text = DBLocale.GetText(FrameMark.GetTextKeyByType(curType));
                /// TODO : local 
                txtWideStatTitleStep.text = string.Format("{0}단계", myKeyData.Step.ToString());

                #endregion

                #region Selectable Object By Direction State

                UpdateSelectableGroup();

                #endregion

                #region Ability Scroll Data
                RefreshAbilityData(ScrollAdapter_WideView, curType, Me.CurCharData.GetMarkTidByType(curType));
                #endregion
            }
            /// 하나의 Mark 정보만 출력되는 부분 UI 업데이트 
            else if (currentTab == StepTab.Detail)
            {
                /// 화면에 보이고 있는 groupIndex 값이 -1 이 아니어야 valid 판정 
                valid = valid && curContextInfo.entryGroupIndex_detailView != -1;

                MarkGroup targetGroup = GetMarkGroupByIndex(curType, contextInfo.targetMarkInfo.CurSelectedMarkIndexInCurGroup);

                /// -- 화면에 출력되어야 하는 UI 들의 Target Mark 정보 세팅 --
                Mark_Table targetMarkInfoForDisplay = null;
                /// -- 실제 강화에 적용될 Cost Info 는 보이는 것 즉 타겟 마크의 이전 Step Mark 이기 때문에 별도로 선언 -- 
                Mark_Table markBeforeTargetMark = null;

                targetMarkInfoForDisplay = DBMark.GetMarkData(contextInfo.targetMarkInfo.CurSelectedMarkTidInCurGroup);
                markBeforeTargetMark = DBMark.GetMarkData(contextInfo.targetMarkInfo.MarkTidBeforeTargetMark);

                #region ---------- Selectable Object Setting -----------

                UpdateSelectableGroup();

                #endregion

                #region ---------- Selected Object Setting ------------

                if (skipSelectObjOnDetail == false)
                {
                    if (contextInfo.targetMarkInfo.CurSelectedMarkIndexInCurGroup != -1)
                    {
                        var sourSubMark = detailMarkModel.GetSubMarkRectTransform(contextInfo.targetMarkInfo.CurSelectedMarkIndexInCurGroup);

                        if (sourSubMark != null)
                        {
                            selectedDetailMarkObj.transform.rotation = sourSubMark.rotation;
                            selectedDetailMarkObj.transform.position = sourSubMark.position;
                            selectedDetailMarkObj.gameObject.SetActive(true);
                        }
                        else
                        {
                            selectedDetailMarkObj.gameObject.SetActive(false);
                        }
                    }
                    else
                    {
                        selectedDetailMarkObj.gameObject.SetActive(false);
                    }
                }

                #endregion

                #region ---------- Button Setting -------------

                if (skipBtnOnDetail == false)
                {
                    bool enhanceNone = enhancingProp.currentEnhanceStatus == EnhanceStatus.None;
                    bool enhanceIdle = enhancingProp.currentEnhanceStatus == EnhanceStatus.Idle;
                    bool enhancingNow = enhancingProp.currentEnhanceStatus == EnhanceStatus.Enhancing;
                    bool serialEnhancing = enhancingProp.isSerialEnhancing && enhancingProp.reEnhanceType != ReEnhanceType.ScheduleStop;

                    if (serialEnhancing)
                    {

                    }
                    else
                    {
                        bool enhanceInteractable = valid && EnhanceButtonInteractable(contextInfo.targetMarkInfo.CurSelectedMarkTidInCurGroup);
                        btnEnhance.interactable = enhanceInteractable;
                        btnSerialEnhance.interactable = enhanceInteractable;
                    }

                    btnEnhance.gameObject.SetActive(serialEnhancing == false);
                    btnSerialEnhance.gameObject.SetActive(serialEnhancing == false);
                    serialEnhanceProgressBar.gameObject.SetActive(serialEnhancing);
                    // btnExitOnBottom.gameObject.SetActive(uiUpdateContextInfo.showExitBtn);
                    btnExitOnBottom.gameObject.SetActive(isGroupObtainedWithinDetailView); // directionInfo.IsImpactTriggered());
                }
                #endregion

                #region --------- Title Info -------------

                if (skipStatTitleOnDetail == false)
                {
                    if (valid)
                    {
                        imgMarkStepIconBGOnDetail.color = Color.white;
                        //  imgMarkStepIconOnDetail.sprite = FrameMark.GetSpriteByOrderInGroup(contextInfo.targetMarkInfo.CurSelectedMarkIndexInCurGroup /*targetMarkForDisplayOrderInGroup*/);
                        //   imgMarkStepIconOnDetail.color = Color.white;

                        txtTypeNameOnDetail.text = DBLocale.GetText(targetMarkInfoForDisplay.MarkAbleType.ToString());
                        txtTypeNameOnDetail.color = FrameMark.ObtainedNodeTypeColor; // contextInfo.markForDisplayInfo.isTargetMarkObtained ? FrameMark.ObtainedNodeTypeColor : FrameMark.InactiveColor;

                        txtMarkNameOnDetail.text = DBLocale.GetText(targetMarkInfoForDisplay.MarkTextID);
                        txtMarkNameOnDetail.color = Color.white; // contextInfo.markForDisplayInfo.isTargetMarkObtained ? Color.white : FrameMark.InactiveColor;
                    }
                }

                #endregion

                #region --------- Protector & Cost Item Setting ----------

                if (skipProtectorAndCostInfoOnDetail == false)
                {
                    bool showProtectorAndCostInfo = valid; // && contextInfo.markForDisplayInfo.isTargetGroupObtained == false;

                    //-------------- Protector ------------//

                    if (showProtectorAndCostInfo)
                    {
                        uint protectorTid = DBConfig.Mark_Protect_Item;
                        ulong myProtectorItemCnt = ZNet.Data.Me.GetCurrency(protectorTid);
                        ulong requiredProtectorItemCnt = markBeforeTargetMark.FailStep > 0 ? (ulong)1 : 0;
                        bool protectorItemToggleInteractable = myProtectorItemCnt > 0 && myProtectorItemCnt >= requiredProtectorItemCnt;
                        bool protectorItemToggleForceUnCheck = myProtectorItemCnt == 0 || myProtectorItemCnt < requiredProtectorItemCnt;
                        var protectItemSpriteName = DBItem.GetItemIconName(protectorTid);

                        enhanceProtector.imgIcon.sprite = ZManagerUIPreset.Instance.GetSprite(protectItemSpriteName);
                        enhanceProtector.txtCnt.text = FrameMark.MakeCostCountString(
                            myProtectorItemCnt
                            , requiredProtectorItemCnt);

                        enhanceProtector.toggle.interactable = protectorItemToggleInteractable;

                        if (protectorItemToggleForceUnCheck)
                        {
                            enhanceProtector.toggle.isOn = false;
                        }
                    }

                    //-------------- CostItem -------------//

                    if (showProtectorAndCostInfo)
                    {
                        ulong myEssenceCnt = ZNet.Data.Me.GetCurrency(DBConfig.Essence_ID);
                        ulong requiredEssenceCnt = markBeforeTargetMark.EssenceCount;

                        ulong myGoldCnt = ZNet.Data.Me.GetCurrency(DBConfig.Gold_ID);
                        ulong requiredGoldCnt = markBeforeTargetMark.GoldCount;

                        currencyUI_essence.txtName.text = DBLocale.GetText(DBItem.GetItemName(DBConfig.Essence_ID));
                        currencyUI_essence.imgIcon.sprite = ZManagerUIPreset.Instance.GetSprite(DBItem.GetItemIconName(DBConfig.Essence_ID));
                        currencyUI_essence.txtCnt.text = FrameMark.MakeCostCountString(myEssenceCnt, requiredEssenceCnt);

                        currencyUI_gold.txtName.text = DBLocale.GetText(DBItem.GetItemName(DBConfig.Gold_ID));
                        currencyUI_gold.imgIcon.sprite = ZManagerUIPreset.Instance.GetSprite(DBItem.GetItemIconName(DBConfig.Gold_ID));
                        currencyUI_gold.txtCnt.text = FrameMark.MakeCostCountString(myGoldCnt, requiredGoldCnt);
                    }

                    activeOnDisplayCost.ForEach(t => t.SetActive(showProtectorAndCostInfo));
                }
                #endregion

                #region -------- Mark model Setting -----------

                if (skipMarkModelOnDetail == false)
                {
                    if (valid)
                    {
                        var sharedInfo = FrameMark.GetSharedInfo(curType);

                        detailMarkModel.UpdateUI_All(
                            FrameMark
                            , curType
                            , enhanceProtector.toggle.isOn && myMarkData.FailStep > 0
                            , myKeyData.Step
                            // , sharedInfo.markGroupStepBigSpriteByOrder
                            , sharedInfo.markGroupBigEffect_SingleSubMarkByStepOrder
                            , sharedInfo.markGroupBigEffect_Done);
                    }
                }

                #endregion

                #region -------- Ability Scroll Data -----------

                if (skipAbilityOnDetail == false)
                {
                    /// 우선은 Default 로 Ability 는 현재 나의 Step 으로 세팅 
                    uint abilityDisplayTid = Me.CurCharData.GetMarkTidByType(curType);

                    /// 선택된 Mark 가 나의 Step 보다 높은애라면 높은애를 넣어줌 . 
                    /// 낮으면 나의 Step 으로 세팅되게끔 
                    if (DBMark.GetMarkStep(contextInfo.targetMarkInfo.CurSelectedMarkTidInCurGroup) > Me.CurCharData.GetMarkStep(curType))
                    {
                        abilityDisplayTid = contextInfo.targetMarkInfo.CurSelectedMarkTidInCurGroup;
                    }

                    RefreshAbilityData(ScrollAdapter_DetailView, curType, abilityDisplayTid);
                }

                #endregion
            }
        }

        #region -------- Tab active Setting ---------

        activeOnWide.ForEach(t => t.SetActive(valid && currentTab == StepTab.Wide));
        activeOnDetail.ForEach(t => t.SetActive(valid && currentTab == StepTab.Detail));

        #endregion
    }

    private void UpdateSelectableGroup()
    {
        if (currentTab != StepTab.None)
        {
            if (currentTab == StepTab.Wide)
            {
                bool selectableInteractable =
                    directionHandler.IsPlaying(MarkDirectionHandler.MarkDirectionType.DragonImpact_First) == false
                    && directionHandler.IsPlaying(MarkDirectionHandler.MarkDirectionType.DragonImpact_Second) == false
                    && directionHandler.IsPlaying(MarkDirectionHandler.MarkDirectionType.DragonImpact_Third) == false;

                selectableObjsControlByDirecting_wideView.ForEach(t => t.interactable = selectableInteractable);
            }
            else if (currentTab == StepTab.Detail)
            {
                bool selectableInteractable = directionHandler.IsPlaying(MarkDirectionHandler.MarkDirectionType.GroupObtained) == false
                    && directionHandler.IsPlaying(MarkDirectionHandler.MarkDirectionType.MarkEnhance) == false
                    && enhancingProp.isSerialEnhancing == false;

                selectableObjsControlByDirecting_detailView.ForEach(t => t.interactable = selectableInteractable);
            }
        }
    }

    private void UpdateUI_WideViewDragonImpact()
    {
        var curGroupList = CurrentMarkGroupList;

        foreach (var dragonEff in wideViewDragonEffectObjs)
        {
            bool targetDragon = FrameMark.CurrentMainTab == dragonEff.markType;

            if (targetDragon)
            {
                foreach (var obj in dragonEff.objs)
                {
                    bool isDirectionPlaying = false;
                    float directionNormalizedTime = 0f;
                    MarkDirectionHandler.MarkDirectionType curDirectionType = MarkDirectionHandler.MarkDirectionType.None;

                    /// 1,2,3 번째중 몇번째거인지 체킹 
                    if (obj.order == 0)
                    {
                        curDirectionType = MarkDirectionHandler.MarkDirectionType.DragonImpact_First;
                    }
                    else if (obj.order == 1)
                    {
                        curDirectionType = MarkDirectionHandler.MarkDirectionType.DragonImpact_Second;
                    }
                    else if (obj.order == 2)
                    {
                        curDirectionType = MarkDirectionHandler.MarkDirectionType.DragonImpact_Third;
                    }
                    else
                    {
                        /// 현시점 201106 , 3개임 . 즉 0~2 만 존재해야함 . 
                        ZLog.LogError(ZLogChannel.UI, "target order not matching , Please Fix this : " + obj.order);
                    }

                    if (curDirectionType != MarkDirectionHandler.MarkDirectionType.None)
                    {
                        isDirectionPlaying = directionHandler.IsPlaying(curDirectionType);
                        directionNormalizedTime = directionHandler.GetCurrentNormalizedTime(curDirectionType);
                    }

                    if ((obj.activeIndex < curGroupList.Count
                        && curGroupList[obj.activeIndex].markNode.IsAllObtained)
                        && (isDirectionPlaying == false || (directionNormalizedTime >= obj.effectObjAppearNormalizedTimeDuringDirection)))
                    {
                        obj.effectObject.SetActive(true);
                    }
                    else
                    {
                        obj.effectObject.SetActive(false);
                    }
                }
            }
            else
            {
                dragonEff.objs.ForEach(t => t.effectObject.SetActive(false));
            }
        }
    }

    /// <summary>
    /// 문양 툴팁 끄기 
    /// </summary>
    public void CloseMarkStatToolTip()
    {
        markStatToolTip.gameObject.SetActive(false);
        temporaryDisplayUIcloser.gameObject.SetActive(false);
    }

    public void OpenMarkTypeInfoPopUp()
    {
        var type = FrameMark.CurrentMainTab;

        if (type == GameDB.E_MarkAbleType.None)
            return;

        string title = DBLocale.GetText(FrameMark.GetSharedInfo(type).commonToolTipTitleKey);
        string content = DBMark.GetToolTipTextID(FrameMark.CurrentMainTab);

        markInfoPopup.Set(title, FrameMark.GetColorByType(type), content);
        markInfoPopup.gameObject.SetActive(true);
        temporaryDisplayUIcloser.gameObject.SetActive(true);
    }

    public void CloseMarkTypeInfoPopUp()
    {
        markInfoPopup.gameObject.SetActive(false);
        temporaryDisplayUIcloser.gameObject.SetActive(false);
    }
    #endregion

    #region Private Methods
    private void OnTabChanged(StepTab tab)
    {
        isGroupObtainedWithinDetailView = false;

        if (tab == StepTab.Wide)
        {
            /// 드래곤 Impact 연출해야하는지 체킹 
            if (directionInfo.IsImpactTriggered() &&
                directionInfo.GetCurrentTriggeredMarkType() == FrameMark.CurrentMainTab)
            {
                var targetDirInfo = directionInfo.GetImpactDragonPhaseByType(FrameMark.CurrentMainTab);
                var targetDragonObjGroup = wideViewDragonEffectObjs.Find(t => t.markType == FrameMark.CurrentMainTab);
                var directionType = MarkDirectionHandler.MarkDirectionType.None;

                if (targetDirInfo == null)
                {
                    return;
                }
                if (targetDragonObjGroup == null)
                {
                    return;
                }

                Vector2 worldPos;
                float effectAppearNormalizedTime;
                int order = -1;

                if (targetDirInfo.triggerFirst)
                {
                    directionType = MarkDirectionHandler.MarkDirectionType.DragonImpact_First;
                    order = 0;
                }
                else if (targetDirInfo.triggerSecond)
                {
                    directionType = MarkDirectionHandler.MarkDirectionType.DragonImpact_Second;
                    order = 1;
                }
                else if (targetDirInfo.triggerThird)
                {
                    directionType = MarkDirectionHandler.MarkDirectionType.DragonImpact_Third;
                    order = 2;
                }
                else
                {
                    /// 왜여기에 ? 
                    return;
                }

                if (order != -1)
                {
                    worldPos = targetDragonObjGroup.GetWorldPos(order);
                    effectAppearNormalizedTime = targetDragonObjGroup.GetEffectAppearNormalizedTime(order);

                    float dummyLength;

                    directionHandler.Play(new MarkDirectionHandler.DirectionParam(directionType)
                        .SetMarkType(FrameMark.CurrentMainTab)
                        .SetWorldPos(worldPos)
                        .SetEventCallback(new MarkDirectionHandler.DirectionParam_EventWithNormalizedTime(
                            callback: () =>
                            {
                                UpdateUI_WideViewDragonImpact();
                            }, effectAppearNormalizedTime))
                        .SetBaseCallback
                        (
                            updatedCallback: (normalizedTime) =>
                            {
                            }
                            , finishedCallback: () =>
                            {
                                UpdateUI(
                                    skipMarkModelOnDetail: true
                                    , skipBtnOnDetail: true
                                    , skipSelectObjOnDetail: true
                                    , skipStatTitleOnDetail: true
                                    , skipProtectorAndCostInfoOnDetail: true
                                    , skipAbilityOnDetail: true);
                            }
                        )
                        , out dummyLength);

                    UpdateSelectableGroup();
                }
            }
        }

        directionInfo.Reset();
    }

    private MarkGroup GetMarkGroupByIndex(GameDB.E_MarkAbleType type, int index)
    {
        if (index < 0)
            return null;

        var groups = marksByType[type];
        if (index >= groups.Count)
            return null;
        return groups[index];
    }

    /// <summary>
    /// 주어진 MarkGroup 에 현재 강화해야하는 문양이 존재한다면 True 및 해당 문양 정보 세팅 
    /// </summary>
    //private bool FindNextEnhanceMarkInfoFromSpecificGroup(
    //    GameDB.E_MarkAbleType type
    //    , MarkGroup checkMarkGroup
    //    , out int groupIndex
    //    , out uint tid)
    //{
    //    groupIndex = -1;
    //    tid = 0;

    //    if (marksByType.ContainsKey(type) == false
    //        || marksByType[type].Count == 0)
    //    {
    //        return false;
    //    }


    //}

    /// Obtained 된 GroupIndex 를 가지고 연출 Trigger Flag 세팅 
    private void RefreshDragonImpactDirectionInfo(int obtainedGroupIndex)
    {
        var targetDirInfo = directionInfo.GetImpactDragonPhaseByType(FrameMark.CurrentMainTab);

        if (targetDirInfo == null)
            return;

        var targetImpactGroup = this.wideViewDragonEffectObjs.Find(t => t.markType == FrameMark.CurrentMainTab);

        if (targetImpactGroup == null)
            return;

        for (int i = 0; i < targetImpactGroup.objs.Count; i++)
        {
            /// 타겟 단계를 찾음 
            if (obtainedGroupIndex == targetImpactGroup.objs[i].activeIndex)
            {
                var order = targetImpactGroup.objs[i].order;

                /// First
                if (order == 0)
                {
                    targetDirInfo.triggerFirst = true;
                }
                /// Second
                else if (order == 1)
                {
                    targetDirInfo.triggerSecond = true;
                }
                /// Third
                else if (order == 2)
                {
                    targetDirInfo.triggerThird = true;
                }
            }
        }
    }

    /// <summary>
    /// 해당 타입의 다음 단계 문양의 marksByType 변수의 그룹 Index 를 가져옵니다
    /// </summary>
    private int GetNextEnhanceGroupIndex(GameDB.E_MarkAbleType type)
    {
        if (marksByType.ContainsKey(type) == false
            || marksByType[type].Count == 0)
        {
            return -1;
        }

        /// maxLevel 일때는 마지막 인덱스 
        if (Me.CurCharData.IsMarkMaxStep(type))
        {
            return marksByType[type].Count - 1;
        }

        var groups = marksByType[type];
        int resultIndex = -1;

        for (int i = 0; i < groups.Count; i++)
        {
            if (FrameMark.HasMyNextMark(type, groups[i].markNode.SubMarkTids))
            {
                resultIndex = i;
                break;
            }
        }

        return resultIndex;
    }

    private void OpenMarkStatToolTip(Vector2 pos, uint tid)
    {
        var data = DBMark.GetMarkData(tid);

        if (data == null)
            return;

        var type = FrameMark.CurrentMainTab;

        string title = string.Format("{0} {1}단계", DBLocale.GetText(FrameMark.GetTextKeyByType(type)), data.Step);
        StringBuilder sb = new StringBuilder();
        var abilities = new List<UIAbilityData>();

        FrameMark.StatHelper.BuildAbilityActionTexts(false, ref abilities, new uint[] { data.AbilityActionID_01, data.AbilityActionID_02 });

        for (int i = 0; i < abilities.Count; i++)
        {
            var ability = DBAbility.GetAbility(abilities[i].type);

            if (ability != null)
            {
                sb.AppendLine(string.Format("{0} +{1}{2}", DBLocale.GetText(ability.StringName), abilities[i].value, ability.MarkType == E_MarkType.Per ? "%" : string.Empty));
            }
            else
            {
                ZLog.LogError(ZLogChannel.UI, "could not find the target Ability for toolTip , Ability Type : " + abilities[i].type);
            }
        }

        markStatToolTip.Set(title, FrameMark.GetColorByType(type), sb.ToString().TrimEnd(' ', '\n'));
        markStatToolTip.transform.position = pos;
        pos = markStatToolTip.transform.localPosition;
        markStatToolTip.transform.localPosition = new Vector3(pos.x, pos.y, 0) + new Vector3(offsetMarkDetailToolTipFromCenter.x, offsetMarkDetailToolTipFromCenter.y, 0);
        markStatToolTip.RectTransform.TryClampPositionToParentBoundary();
        markStatToolTip.gameObject.SetActive(true);
        temporaryDisplayUIcloser.gameObject.SetActive(true);
    }

    private bool CheckEnoughCurrency(uint tid, uint requiredCnt)
    {
        return ZNet.Data.Me.GetCurrency(tid) >= requiredCnt;
    }

    /// <summary>
    /// Target Tid 의 Mark 로 강화를 하기위한 이전 단계의 Cost Info 를 가진 
    /// Mark 의 Tid 를 리턴함 . 
    /// **** 가장 낮은 레벨의 Mark 는 이 함수를 사용하면 안됨 . 
    /// 애초에 그럴 일이 없어야함 . 왜냐면 이 함수는 특정 Mark 로 강화를 하기 위해서 사용하는 함수이고
    /// 그 특정 Mark 에 Min Step 의 Mark 는 화면에 띄울일이 없기 때문에 들어갈일 없음. 
    /// **** (기획컨펌완료)
    /// </summary>
    private uint GetEnhanceCostMarkTidForTargetMark(uint tid)
    {
        var data = DBMark.GetMarkData(tid);

        if (data == null)
        {
            return 0;
        }

        /// Min Level 임 . 에러임 . 
        if (DBMark.GetMarkTypeNormalMinStep(data.MarkAbleType) == data.Step)
        {
            ZLog.LogError(ZLogChannel.UI, "(1) You must not try to get info for enhance of minimum step 0 , tid : " + tid);
            return 0;
        }

        /// 이전 단계의 Step 을 가져와야하는데 Step 이 0 이다 ? Min 이고 자시고 에러임 . 
        /// 그리고 - 1 를 하는 과정에서 0 이면 오버플로남 . 이 부분에서 예외 처리함 . 
        if (data.Step == 0)
        {
            ZLog.LogError(ZLogChannel.UI, "(2) You must not try to get info for enhance of minimum step 0 , tid : " + tid);
            return 0;
        }

        return DBMark.GetMarkTidByStep(data.MarkAbleType, (byte)(data.Step - 1));
    }

    /// <summary>
    ///  강화 가능 여부 조건 체크 . 
    /// </summary>
    private EnhanceAdvanceCheckCondition CanEnhance(uint targetTid)
    {
        var costMarkData = DBMark.GetMarkData(targetTid);

        if (ConditionHelper.CheckCompareCost(DBConfig.Gold_ID, costMarkData.GoldCount, onClickOK: () =>
        {
            /// 제거대상
            /// 
            CoroutineManager.Instance.NextFrame(() =>
            {
                UIManager.Instance.ChangeMainCameraStack(CManagerUIFrameFocusBase.E_UICameraStack.None);
                UIManager.Instance.ActiveUICamera(CManagerUIFrameFocusBase.E_UICamera.Front, true);
                UIManager.Instance.ActiveUICamera(CManagerUIFrameFocusBase.E_UICamera.Model3D, true);
            });
        }) == false)
        {
            return EnhanceAdvanceCheckCondition.Currency_Gold;
        }

        if (ConditionHelper.CheckCompareCost(DBConfig.Essence_ID, costMarkData.EssenceCount, onClickOK: () =>
        {
            /// 제거대상
            CoroutineManager.Instance.NextFrame(() =>
            {
                UIManager.Instance.ChangeMainCameraStack(CManagerUIFrameFocusBase.E_UICameraStack.None);
                UIManager.Instance.ActiveUICamera(CManagerUIFrameFocusBase.E_UICamera.Front, true);
                UIManager.Instance.ActiveUICamera(CManagerUIFrameFocusBase.E_UICamera.Model3D, true);
            });
        }) == false)
        {
            return EnhanceAdvanceCheckCondition.Currency_Assence;
        }

        var type = DBMark.GetMarkType(targetTid);

        if (Me.CurCharData.IsMarkMaxStep(type))
        {
            return EnhanceAdvanceCheckCondition.MaxStep;
        }

        return EnhanceAdvanceCheckCondition.Advance;
    }

    /// <summary>
    /// 현재 설정된 ReEnhance 자동 종료 세팅
    /// </summary>
    private void SetReEnhanceTypeProp()
    {
        if (enhancingProp == null)
            return;

        /// TODO : 자동 종료 옵션 세팅해줌댐 

        // 해당 강화 횟수만큼만 강화시도. 
        // enhancingProp.reEnhanceRemainedCnt = 3;
        // enhancingProp.reEnhanceType = ReEnhanceType.RemainedCount;

        // 해당 Step 까지만 강화 시도 
        // enhancingProp.reEnhanceCompleteStep = (byte)(Me.CurCharData.GetMarkStep(FrameMark.CurrentMainTab) + 1);
        // enhancingProp.reEnhanceType = ReEnhanceType.TargetStep;
    }

    /// <summary>
    /// 재강화 가능여부 관련 상태 업데이트 
    /// </summary>
    private bool UpdateAdvanceReEnhanceState()
    {
        bool advance = true;

        /// 강화 재시작전에 자동 종료 옵션이 존재한다면 
        if (enhancingProp.reEnhanceType != ReEnhanceType.Loop)
        {
            switch (enhancingProp.reEnhanceType)
            {
                /// 종료 
                case ReEnhanceType.ScheduleStop:
                    {
                        advance = false;
                    }
                    break;
                /// 남은 횟수 업데이트
                case ReEnhanceType.RemainedCount:
                    {
                        if (enhancingProp.reEnhanceRemainedCnt <= 0)
                        {
                            advance = false;
                        }
                        else
                        {
                            enhancingProp.reEnhanceRemainedCnt--;
                        }
                    }
                    break;
                /// 완료 Step 업데이트 
                case ReEnhanceType.TargetStep:
                    {
                        /// 현재 Step 이 CompleteStep 보다 낮아야함 
                        if (Me.CurCharData.GetMarkStep(FrameMark.CurrentMainTab) >= enhancingProp.reEnhanceCompleteStep)
                        {
                            advance = false;
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        return advance;
    }

    private ContextInfo.ContextInfoCacheByType GetContextByTypeInfo(GameDB.E_MarkAbleType type)
    {
        return contextInfo.dataByType.Find(t => t.type == type);
    }

    private void RefreshCurrentContextInfo()
    {
        SetContextInfo_DetailViewEntryGroupIndexByType(
            FrameMark.CurrentMainTab
            , GetContextByTypeInfo(FrameMark.CurrentMainTab).entryGroupIndex_detailView
            , (baseContextInfo) =>
            {
                SetContextInfo_DefaultTargetMarkForDisplay(baseContextInfo);
            });
    }

    /// <summary>
    /// DetailView 를 진입할때 어떤 Group 을 띄울지 세팅함 . 
    /// updateTargetMarkInfo 는 해당 함수 끝에서 같이 업데이트되어야할 TargetMarkInfo 업데이터 
    /// </summary>
    private bool SetContextInfo_DetailViewEntryGroupIndexByType(
        GameDB.E_MarkAbleType type
        , int index
        , ContextInfo.UpdateTargetMarkForDisplay updateTargetMarkInfo)
    {
        var target = GetContextByTypeInfo(type);

        if (target == null)
            return false;

        target.entryGroupIndex_detailView = index;

        MarkGroup targetGroup = GetMarkGroupByIndex(type, index);
        bool valid = index != -1 && targetGroup != null;

        // -- 주로 Detail View 에서 출력시켜줄 TargetMark 와 관련된 ContextInfo 세팅 -- 
        if (valid)
        {
            target.isTargetGroupObtained = Me.CurCharData.IsMarkObtained_ByStep(type, targetGroup.markNode.TopStep);
            
            /// 최대 레벨 도달하지 않았을때만 체킹함. 
            if (Me.CurCharData.IsMarkMaxStep(type) == false)
            {
                target.curGroupHasNextEnhanceMark = GetNextEnhanceGroupIndex(type) == index;
			}
			else
			{
                target.curGroupHasNextEnhanceMark = false;
            }

            target.curGroupIsOutOfBound = target.isTargetGroupObtained == false && target.curGroupHasNextEnhanceMark == false;
        }
        else
        {
            target.Reset();
        }

        updateTargetMarkInfo?.Invoke(target);

        return true;
    }

    /// <summary>
    /// 최신화되어 있는 baseContextInfo 를 기반으로 현재 그룹에서의 
    /// TargetMark 의 Index 를 세팅하고 부가 정보들을 업데이트함. 
    /// </summary>
    private bool SetContextInfo_TargetMarkIndexInGroupForDisplay(ContextInfo.ContextInfoCacheByType baseContextInfo, int index)
    {
        if (index == -1)
        {
            ZLog.LogError(ZLogChannel.UI, "-1 means invalid");
            contextInfo.targetMarkInfo.Reset();
            return false;
        }

        MarkGroup targetGroup = GetMarkGroupByIndex(baseContextInfo.type, baseContextInfo.entryGroupIndex_detailView);

        var curSelectedMarkTidInCurGroup = targetGroup.markNode.GetSubMarkTidByIndex(index);
        var isTargetMarkObtained = Me.CurCharData.IsMarkObtained_ByID(curSelectedMarkTidInCurGroup);
        var markTidForCostInfo = GetEnhanceCostMarkTidForTargetMark(curSelectedMarkTidInCurGroup);

        contextInfo.targetMarkInfo.Set(isTargetMarkObtained, curSelectedMarkTidInCurGroup, index, markTidForCostInfo);

        return true;
    }

    /// <summary>
    /// 첫 Entry 를 가정하고 baseContextInfo 를 기반으로 
    /// TargetMark 정보를 세팅함 
    /// </summary>
    private bool SetContextInfo_DefaultTargetMarkForDisplay(ContextInfo.ContextInfoCacheByType baseContextInfo)
    {
        MarkGroup targetGroup = GetMarkGroupByIndex(baseContextInfo.type, baseContextInfo.entryGroupIndex_detailView);

        uint curSelectedMarkTidInCurGroup = 0;

		///  **현재 MarkGroup 이 다음 강화의 Mark 를 포함하고 있다면 
		/// 해당 타겟 Mark 의 Tid 를 넣어줌.**
		if (baseContextInfo.curGroupHasNextEnhanceMark)
		{
            curSelectedMarkTidInCurGroup = targetGroup.markNode.GetFirstNotObtainedSubMarkTid();
        }
        /// 소유중이 아니고 && 현재 MarkGroup 이 다음 강화의 Mark 를 포함하고 있지 않다면 즉
        /// OutOfBound 상태 
        else if (baseContextInfo.isTargetGroupObtained == false)
        {
            curSelectedMarkTidInCurGroup = targetGroup.markNode.StartTID;
        }
        /// 소유중이라면 
        else if (baseContextInfo.isTargetGroupObtained)
        {
            /// 맨 마지막거 세팅 
            curSelectedMarkTidInCurGroup = targetGroup.markNode.TopTID;
        }

        var isTargetMarkObtained = Me.CurCharData.IsMarkObtained_ByID(curSelectedMarkTidInCurGroup);
        var curSelectedMarkIndexInCurGroup = targetGroup.markNode.GetSubMarkIndexByTid(curSelectedMarkTidInCurGroup);
        var markTidForCostInfo = GetEnhanceCostMarkTidForTargetMark(curSelectedMarkTidInCurGroup);

        if (curSelectedMarkIndexInCurGroup == -1)
        {
            ZLog.LogError(ZLogChannel.UI, "-1 means invalid");
            contextInfo.targetMarkInfo.Reset();
            return false;
        }
        else
        {
            contextInfo.targetMarkInfo.Set(isTargetMarkObtained, curSelectedMarkTidInCurGroup, curSelectedMarkIndexInCurGroup, markTidForCostInfo);
        }
        return true;
    }

    /// <summary>
    /// 현재 Target Mark Index 가 Invalid 가 아니라면 그대로 다시 Refresh 하되 
    /// invalid 라면 First 기준 세팅함 
    /// </summary>
    private void SetContextInfo_SetFirstIfInvalidOrRefresh(ContextInfo.ContextInfoCacheByType baseContextInfo)
    {
        if (contextInfo.targetMarkInfo.CurSelectedMarkIndexInCurGroup == -1)
        {
            SetContextInfo_DefaultTargetMarkForDisplay(baseContextInfo);
        }
        else
        {
            SetContextInfo_TargetMarkIndexInGroupForDisplay(baseContextInfo, contextInfo.targetMarkInfo.CurSelectedMarkIndexInCurGroup);
        }
    }

    #region JumpMarkGroup Detail View Methods

    /// <summary>
    ///  지정된 Group 으로 바로 Detail View 를 이동시키고 
    ///  후에 TargetMark 는 람다로 세팅한다. (다양한 설계가 가능하게끔)
    /// </summary>
    private void JumpDetailMarkGroup(int jumpGroupIndex, Action<ContextInfo.ContextInfoCacheByType> targetMarkContextSetter)
    {
        SetContextInfo_DetailViewEntryGroupIndexByType(
           FrameMark.CurrentMainTab
           , jumpGroupIndex
           , (updatedContextInfo) =>
           {
               targetMarkContextSetter?.Invoke(updatedContextInfo);
           });

        FrameMark.SetStepTab(StepTab.Detail, false);
    }

    /// <summary>
    /// 최신 데이터 기준으로 다음 단계의 강화 Mark 존재하는 Group 으로 Jump 함 
    /// </summary>
    private bool JumpDetailMarkGroup_NextEnhanceTarget()
    {
        int index = GetNextEnhanceGroupIndex(FrameMark.CurrentMainTab);

        if (index != detailMarkModel.ID)
        {
            JumpDetailMarkGroup(
                index
                , (updatedContextInfo) =>
                {
                    SetContextInfo_DefaultTargetMarkForDisplay(updatedContextInfo);
                });
            return true;
        }
        return false;
    }

    #endregion

    private void RefreshAbilityData(
        UIAbilityListAdapter adapter
        , GameDB.E_MarkAbleType type
        , uint destTid)
    {
        var myMarkTid = Me.CurCharData.GetMarkTidByType(type);

        if (myMarkTid == 0)
        {
            ZLog.LogError(ZLogChannel.UI, "mark tid retrieved from ChracterData represents invalid , Type : " + type);
            return;
        }

        var sourceAbilityIDs = FrameMark.StatHelper.GetAbilityActionIDsStackedList(myMarkTid);
        var destAbilityIDs = FrameMark.StatHelper.GetAbilityActionIDsStackedList(destTid);

        var mergedSourceList = new List<UIAbilityData>();
        var mergedDestList = new List<UIAbilityData>();

        FrameMark.StatHelper.BuildAbilityActionTexts(true, ref mergedSourceList, sourceAbilityIDs);
        FrameMark.StatHelper.BuildAbilityActionTexts(true, ref mergedDestList, destAbilityIDs);

        FrameMark.StatHelper.SetCompareTexts(mergedSourceList, ref mergedDestList);

        adapter.RefreshListData(mergedDestList);
    }

    private int GetMarkGroupCount(GameDB.E_MarkAbleType type)
    {
        return marksByType[type].Count;
    }

    private RectTransform AddConnector()
    {
        var instance = Instantiate(connectorSourceObj, connectorRoot);
        if (instance != null)
        {
            instance.gameObject.SetActive(false);
            connectors.Add(instance);
        }
        else
        {
            return null;
        }

        return instance.rt;
    }

    private GameObject GetMarkGroupRootByType(GameDB.E_MarkAbleType type)
    {
        switch (type)
        {
            case GameDB.E_MarkAbleType.None:
                return null;
            case GameDB.E_MarkAbleType.RecoveryMark:
                return markGroupRoot_fireDragon;
            case GameDB.E_MarkAbleType.AttackMark:
                return markGroupRoot_waterDragon;
            case GameDB.E_MarkAbleType.DefenseMark:
                return markGroupRoot_electricDragon;
            default:
                ZLog.LogError(ZLogChannel.UI, "type not added");
                break;
        }

        return null;
    }

    private void SetMarkGroupRootActiveRespectively(
        bool fireDragon, bool waterDragon, bool electricDragon)
    {
        markGroupRoot_fireDragon.SetActive(fireDragon);
        markGroupRoot_waterDragon.SetActive(waterDragon);
        markGroupRoot_electricDragon.SetActive(electricDragon);
    }

    #region 강화 관련 
    /// <summary>
    /// 강화 시작 함수 
    /// </summary>
    private bool StartEnhance(bool isSerialEnhance)
    {
        var resultCheck = CanEnhance(contextInfo.targetMarkInfo.MarkTidBeforeTargetMark);

        if (resultCheck == EnhanceAdvanceCheckCondition.Currency_Gold)
        {
            //   FrameMark.OpenNotiUp(string.Format("{0}가 부족합니다.", DBLocale.GetText(DBItem.GetItemName(DBConfig.Gold_ID))), "알림");
        }
        else if (resultCheck == EnhanceAdvanceCheckCondition.Currency_Assence)
        {
            //   FrameMark.OpenNotiUp(string.Format("{0}가 부족합니다.", DBLocale.GetText(DBItem.GetItemName(DBConfig.Essence_ID))), "알림");
        }
        else if (resultCheck == EnhanceAdvanceCheckCondition.MaxStep)
        {
        }

        if (resultCheck != EnhanceAdvanceCheckCondition.Advance)
        {
            return false;
        }

        var data = DBMark.GetMarkData(contextInfo.targetMarkInfo.MarkTidBeforeTargetMark);

        /// 우선 강화 실행 
        TryEnhance(
            contextInfo.targetMarkInfo.MarkTidBeforeTargetMark
            , enhanceProtector.toggle.isOn && data.FailStep > 0 /// 조건 추가 . failStep 0 초과일때만 protector 적용 
            , onFinished: (req, res) =>
            {
                /// 이제 연출 시작 
                bool started = StartEnhancingAnimation(
                    onUpdate: (normalizedTime) =>
                    {
                        /// normalize 시간으로 뭐 업데이트할게 있다면 이곳에서 하면댐 . 
                        detailMarkModel.SetFirstNotObtainedMarkFillAmount(normalizedTime);
                        serialEnhanceProgressBar.value = normalizedTime;
                    }
                    ,
                    /// 강화 연출 끝난 시점 . 
                    onFinished: () =>
                    {
                        /// 한 그룹 전체가 강화됐는지 ? 
                        bool isGroupObtained = this.detailMarkModel.IsAllObtained;
                        AdvanceEnhanceResult(isGroupObtained, res.IsSuccess);
                        SetEnhancingToIdle();

                        /// 현재 그룹에 하나라도 강화되지않은 상태 && 
                        /// 연속 강화 &&
                        /// 강화 타겟 문장이 현재 페이지에 있다 => 재강화 시도.
                        if (isGroupObtained == false
                            && isSerialEnhance
                            && GetNextEnhanceGroupIndex(FrameMark.CurrentMainTab) == detailMarkModel.ID)
                        {
                            /// 연속 강화하는 경우에는 
                            /// ContextInfo 를 다음 강화 Target 으로 전환해야함 
                            /// 근데 예외가 있을거같은데 ?
                            /// 나는 이거로 SubMark 들이 업데이트가 될거라 생각하지.근데 업데이트되긴해 근데 
                            /// 이 시점의 데이터로 들어가게될까 ? 
                            /// 즉 다음 강화를 이미 해버린 시점에 갑자기 이떄 업데이트되버리면 이후걸로 들어가버리게될텐데?

                            /// 201102 : 페이지 벗어나지않음 
                            //JumpDetailMarkGroup_NextEnhanceTarget();
                            /// JumpDetailMarkGroup_NextEnhanceTarget 에서 어차피 업데이트해주는데
                            /// Dirty true 하는 이유는 
                            /// Data Update 하고나서 재강화를 해야하는데 
                            /// 업데이트 시점 이슈로 재강화후에 Date Update 해버리는 경우가 있어서 dirty true 처리 
                            isDataDirty = true;
                            enhancingProp.scheduledReEnhance = true;
                        }
                        /// 강화 종료 
                        else
                        {
                            enhancingProp.ResetAll();
                            //FrameMark.RefreshCurrentStepTab();
                        }

                        FrameMark.RefreshCurrentStepTab();

                        /// 그룹이 전부 Obtain 된 경우 추가 연출 
                        if (isGroupObtained)
                        {
                            float dummy = 0f;

                            isGroupObtainedWithinDetailView = true;

                            //groupObtainedTempDirectionIndicator.gameObject.SetActive(true);
                            //   groupObtainedTempDirectionIndicator.color = new Color(1f, 1f, 1f, 1f);

                            RefreshDragonImpactDirectionInfo(this.detailMarkModel.ID);

                            /// 하나의 그룹이 전부 강화성공했을때의 강화 연출 시작 
                            directionHandler.Play(
                                new MarkDirectionHandler.DirectionParam(MarkDirectionHandler.MarkDirectionType.GroupObtained)
                                .SetMarkType(FrameMark.CurrentMainTab)
                                .SetBaseCallback(
                                    updatedCallback: (normalizedTime) =>
                                    {
                                    }
                                    ,
                                    finishedCallback: () =>
                                    {
                                        UpdateUI(
                                            skipMarkModelOnDetail: true
                                            , skipSelectObjOnDetail: true
                                            , skipStatTitleOnDetail: true
                                            , skipAbilityOnDetail: true);
                                    })
                                , out dummy);
                        }
                    });

                if (started)
                {
                    /// UpdateUI() 에서 반영될것들 세팅해줘야함 미리.
                    enhancingProp.isSerialEnhancing = isSerialEnhance;

                    /// FrameMark 를 통하지않고 바로 Data , UI 를 업데이트하는 이유는 
                    /// 부분적으로 Skip 해야할 것들이 있기 떄문임 
                    //FrameMark.RefreshCurrentStepTab();
                    /// MarkModel 업데이트를 스킵함 . 
                    /// 내부 데이터는 연출동안은 강화 이전의 데이터를 유지해야하기 때문임 
                    UpdateData(false, skipMarkModelOnDetail: true);
                    /// 강화 이후의 UI 를 보여주는 것들은 전부 skip 해야함 . 
                    UpdateUI(
                        skipMarkModelOnDetail: true
                        , skipSelectObjOnDetail: true
                        , skipStatTitleOnDetail: true
                        , skipAbilityOnDetail: true);
                }
            }
            , onError: (err, req, res) =>
            {
                FrameMark.OpenErrorPopUp(res.ErrCode, () =>
                {
                    CoroutineManager.Instance.NextFrame(() =>
                    {
                        UIManager.Instance.ChangeMainCameraStack(CManagerUIFrameFocusBase.E_UICameraStack.None);
                        UIManager.Instance.ActiveUICamera(CManagerUIFrameFocusBase.E_UICamera.Front, true);
                        UIManager.Instance.ActiveUICamera(CManagerUIFrameFocusBase.E_UICamera.Model3D, true);
                    });
                });
            });


        return true;
    }

    /// <summary>
    /// 강화 중단 
    /// </summary>
    private void StopEnhance()
    {
        enhancingProp.ResetAll();
        FrameMark.RefreshCurrentStepTab();
    }

    /// <summary>
    /// 강화 시도 함수 (프로토콜 호출) 
    /// </summary>
    private void TryEnhance(
        uint tid
        , bool useProtector
        , Action<ZWebRecvPacket, ResMarkEnchant> onFinished
        , PacketErrorCBDelegate onError = null)
    {
        ZWebManager.Instance.WebGame.REQ_EnchantMark(
            tid
            , useProtector
            , _onReceive:
            (revPacket, resList) =>
            {
                /// CAUTION : 현재 이 강화가 완료된 최신 데이터 기준으로 
                /// 한번 ContextInfo 를 업데이트함 . 
                /// 즉 중간에 강화가 종료되거나 해도 별도의 ContextInfo Update 는 필요하지않게끔 미리 세팅 
                RefreshCurrentContextInfo();

                var v = GetContextByTypeInfo(FrameMark.CurrentMainTab);

                onFinished?.Invoke(revPacket, resList);
            },
            (err, req, res) =>
            {
                onError?.Invoke(err, req, res);
            });
    }

    private void AdvanceEnhanceResult(
        bool isGroupObtained
        , bool isSuccess)
    {
        /// 성공이면 성공 연출
        /// 실패면 실패 연출 .
        /// 일단 임시 noti 출력
        /// TODO : 임시 문자열 
        string comment = "강화가 " + (isSuccess ? "성공" : "실패") + "하였습니다";
        UICommon.SetNoticeMessage(comment, Color.white, 1f, UIMessageNoticeEnum.E_MessageType.SubNotice);

        if (isSuccess)
        {
            float dummyLength = 0f;

            directionHandler.Play(new MarkDirectionHandler.DirectionParam(MarkDirectionHandler.MarkDirectionType.MarkEnhance_Success)
                .SetMarkType(FrameMark.CurrentMainTab)
                .SetWorldPos(this.detailMarkModel.GetFirstNotObtainedSubMarkWorldPosXY())
                , out dummyLength);
        }
        else
        {
            float dummyLength = 0f;

            directionHandler.Play(new MarkDirectionHandler.DirectionParam(MarkDirectionHandler.MarkDirectionType.MarkEnhance_Fail)
                .SetMarkType(FrameMark.CurrentMainTab)
                .SetWorldPos(this.detailMarkModel.GetFirstNotObtainedSubMarkWorldPosXY())
                , out dummyLength);
        }

        // uiUpdateContextInfo.showExitBtn = isGroupObtained;
    }

    private bool StartEnhancingAnimation(Action<float> onUpdate, Action onFinished)
    {
        if (enhancingProp == null)
        {
            return false;
        }

        SetEnhancingStatus(EnhanceStatus.Enhancing);

        directionHandler.Play(
            new MarkDirectionHandler.DirectionParam(MarkDirectionHandler.MarkDirectionType.MarkEnhance)
            .SetMarkType(FrameMark.CurrentMainTab)
            .SetWorldPos(detailMarkModel.GetFirstNotObtainedSubMarkWorldPosXY())
            .SetBaseCallback(onUpdate, onFinished)
            , out enhancingProp.duration);

        //enhancingProp.duration = duration;
        //enhancingProp.elapsedTime = 0f;
        //enhancingProp.elapsedTimeNormalized = 0f;
        //enhancingProp.onUpdate = onUpdate;
        //enhancingProp.onFinished = onFinished;

        onUpdate?.Invoke(0f);

        return true;
    }

    private void SetEnhancingToIdle()
    {
        if (enhancingProp != null)
        {
            enhancingProp.ResetDirectionProp();
        }
    }

    //private void SetUIUpdateContextInfo_ShowExitButton()
    //{
    //    if (uiUpdateContextInfo == null)
    //        return;

    //    uiUpdateContextInfo.showExitBtn = true;
    //}

    //private void UpdateEnhancingAnimation()
    //{
    //    if (enhancingProp.elapsedTime >= enhancingProp.duration)
    //    {
    //        return;
    //    }

    //    enhancingProp.elapsedTime += Time.unscaledDeltaTime;
    //    enhancingProp.elapsedTimeNormalized = enhancingProp.elapsedTime / enhancingProp.duration;

    //    bool finished = false;

    //    if (enhancingProp.elapsedTimeNormalized > 1f)
    //    {
    //        finished = true;
    //        enhancingProp.elapsedTime = enhancingProp.duration;
    //        enhancingProp.elapsedTimeNormalized = 1f;
    //    }

    //    enhancingProp.onUpdate?.Invoke(enhancingProp.elapsedTimeNormalized);

    //    /// 애니 끝 
    //    if (finished)
    //    {
    //        enhancingProp.onFinished?.Invoke();
    //        SetEnhancingToIdle();
    //    }
    //}

    /// <summary>
    /// 강화, 연속강화 버튼 Interactable 
    /// </summary>
    private bool EnhanceButtonInteractable(uint checkTid)
    {
        if (enhancingProp == null)
            return false;

        if (enhancingProp.currentEnhanceStatus == EnhanceStatus.Enhancing)
            return false;

        var type = FrameMark.CurrentMainTab;

        /// maxStep 이면 interactable 꺼줌 
        if (Me.CurCharData.IsMarkMaxStep(type))
        {
            return false;
        }

        /// 선택된애가 다음 단계 mark 가 아니면 꺼줌
        if (Me.CurCharData.GetMyNextMarkTID(type, out uint myNextTid))
        {
            if (checkTid != myNextTid)
                return false;
        }
        else
        {
            return false;
        }

        return true;
    }
    #endregion

    private void HandleMarkGroupClicked(int index)
    {
        if (FrameMark == null || FrameMark.CanInteract == false)
            return;

        var target = GetMarkGroupByIndex(FrameMark.CurrentMainTab, index);

        SetContextInfo_DetailViewEntryGroupIndexByType(
            FrameMark.CurrentMainTab
            , index
            , (baseContextInfo) =>
            {
                SetContextInfo_DefaultTargetMarkForDisplay(baseContextInfo);
            });

        FrameMark.InteractionHandler.TryScheduleLateCommand(
            timer: 1.5f /// TODO : 연출 시간 
            , onExecute: (param) =>
           {
               FrameMark.SetStepTab(StepTab.Detail, false);
           }
            , null);
    }

    private void SetEnhancingStatus(EnhanceStatus status)
    {
        enhancingProp.currentEnhanceStatus = status;
    }

    private void HandleSubMarkClickedOnDetailView(int index, uint tid, RectTransform rectTransform)
    {
        /// 강화 연출중이라면 클릭 무시 
        if (enhancingProp != null &&
            enhancingProp.currentEnhanceStatus == EnhanceStatus.Enhancing)
            return;

        var contextInfo = GetContextByTypeInfo(FrameMark.CurrentMainTab);
        if (contextInfo == null || currentTab != StepTab.Detail)
            return;

        OpenMarkStatToolTip(new Vector2(rectTransform.position.x, rectTransform.position.y), tid);

        SetContextInfo_TargetMarkIndexInGroupForDisplay(contextInfo, index);
        UpdateUI();
    }

    #region Inspector Data Validation
    /// <summary>
    /// 인스펙터에서 꽃힌 데이터 체킹 
    /// </summary>
    private bool ValidateData()
    {
        if (ValidateData_MarkGroupList(marks_fireDragon, GameDB.E_MarkAbleType.RecoveryMark) == false)
            return false;

        if (ValidateData_MarkGroupList(marks_waterDragon, GameDB.E_MarkAbleType.AttackMark) == false)
            return false;

        if (ValidateData_MarkGroupList(marks_electricDragon, GameDB.E_MarkAbleType.DefenseMark) == false)
            return false;

        /// 드래곤 이펙트 오브젝트들의 활성화 ActiveIndex 체킹 
        foreach (var dragonEffObjGroup in wideViewDragonEffectObjs)
        {
            if (dragonEffObjGroup.markType == GameDB.E_MarkAbleType.RecoveryMark)
            {
                if (dragonEffObjGroup.objs.Exists(t => t.activeIndex >= marks_fireDragon.Count))
                {
                    ZLog.LogError(ZLogChannel.UI, "Active Index cannot be larger than the whole group count - 01 -");
                    return false;
                }
            }
            else if (dragonEffObjGroup.markType == GameDB.E_MarkAbleType.AttackMark)
            {
                if (dragonEffObjGroup.objs.Exists(t => t.activeIndex >= marks_waterDragon.Count))
                {
                    ZLog.LogError(ZLogChannel.UI, "Active Index cannot be larger than the whole group count - 02 -");
                    return false;
                }
            }
            else if (dragonEffObjGroup.markType == GameDB.E_MarkAbleType.DefenseMark)
            {
                if (dragonEffObjGroup.objs.Exists(t => t.activeIndex >= marks_electricDragon.Count))
                {
                    ZLog.LogError(ZLogChannel.UI, "Active Index cannot be larger than the whole group count - 03 -");
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// MarkGroup List 의 데이터 유효성 체킹 
    /// </summary>
    private bool ValidateData_MarkGroupList(List<MarkGroup> list, GameDB.E_MarkAbleType type)
    {
        byte minStep = DBMark.GetMarkTypeNormalMinStep(type);

        foreach (var t in list)
        {
            for (int i = 0; i < UIFrameMark.MarkCountPerGroup; i++)
            {
                int tid = t.startTid + i;

                var data = DBMark.GetMarkData((uint)tid);

                if (data == null)
                {
                    ZLog.LogError(ZLogChannel.UI, "Target Mark Data not found , Tid : " + tid);
                    return false;
                }
                else if (data.Step == minStep)
                {
                    /// ** 기획 컨펌 완료 ** 
                    /// 가장 낮은 레벨의 데이터는 포함시키면 안됨 . 화면에 출력시키려는 mark 만 
                    /// 포함시키는데 , 가장 낮은 레벨의 마크는 화면에 출력되면 안됨 
                    ZLog.LogError(ZLogChannel.UI, "MarkGroup list must not include the very first data , tid : " + tid.ToString());
                    return false;
                }
            }
        }

        return true;
    }

    #endregion
    #endregion

    #region Inspector Events 
    public void OnProtectorToggleValueChanged()
    {
        byte myStep = Me.CurCharData.GetMarkStep(FrameMark.CurrentMainTab);
        var markData = DBMark.GetMarkDataByStep(FrameMark.CurrentMainTab, myStep);
        this.detailMarkModel.UpdateProtectionEffect(enhanceProtector.toggle.isOn && markData.FailStep > 0, myStep);
    }
    #region OnClick
    public void OnClickEnterToCurrentGroupBtn()
    {
        JumpDetailMarkGroup(
            GetNextEnhanceGroupIndex(FrameMark.CurrentMainTab)
            , targetMarkContextSetter: (baseContextInfo) =>
            {
                SetContextInfo_DefaultTargetMarkForDisplay(baseContextInfo);
            });
    }
    public void OnClickStepCloseBtn()
    {
        FrameMark.SetStepTab(StepTab.Wide, true);
    }

    public void OnClickEnhanceOnceBtn()
    {
        if (FrameMark.CanInteract == false)
            return;

        FrameMark.OpenTwoButtonQueryPopUp("확인", "강화를 진행하시겠습니까?",
            onConfirmed: () =>
            {
                StartEnhance(false);
            });
    }

    public void OnClickEnhanceSerialBtn()
    {
        if (FrameMark.CanInteract == false)
            return;

        FrameMark.OpenTwoButtonQueryPopUp("확인", "강화를 진행하시겠습니까?",
            onConfirmed: () =>
            {
                SetReEnhanceTypeProp();
                if (UpdateAdvanceReEnhanceState())
                {
                    if (StartEnhance(true) == false)
                    {
                        enhancingProp.ResetAll();
                    }
                }
            });
    }

    public void OnClickCancelSerialEnhanceBtn()
    {
        if (enhancingProp == null)
            return;

        enhancingProp.reEnhanceType = ReEnhanceType.ScheduleStop;
        UpdateUI(
            skipAbilityOnDetail: true
            , skipBtnOnDetail: false
            , skipSelectObjOnDetail: true
            , skipStatTitleOnDetail: true
            , skipProtectorAndCostInfoOnDetail: true
            , skipMarkModelOnDetail: true);
        // StopEnhance();
    }
    #endregion
    #endregion

    #region Class & Struct 
    [Serializable]
    public class SingleMarkInfo
    {
        /// <summary>
        /// 1000 이면 해당 Mark 는 1000,1001,1002 
        /// </summary>
        public uint startTid;
        public Vector2 anchoredPos;

        /// <summary>
        ///  현재 기준 한 Mark 에 3개씩 들어가게 되므로 
        ///  0 ~ 2 까지의 파라미터가 유효할것임. 
        /// </summary>
        public uint TID(uint order)
        {
            return startTid + order;
        }
    }

    [Serializable]
    public class MarkGroup
    {
        public int startTid;
        public MarkStepSingleNode markNode;
    }

    class ContextInfo
    {
        public delegate void UpdateTargetMarkForDisplay(ContextInfoCacheByType baseContextInfo);

        public class ContextInfoCacheByType
        {
            public GameDB.E_MarkAbleType type;
            /// <summary>
            /// -1 represent invalid 
            /// </summary>
            public int entryGroupIndex_detailView = -1;
            public bool isTargetGroupObtained;
            public bool curGroupHasNextEnhanceMark;
            /// 지금 소유중인 MarkGroup 도 아니고 , 
            /// 다음 강화 단계의 Mark 가 존재하는 Group 도 아니라면 True (OutOfBound)
            public bool curGroupIsOutOfBound;

            public void Reset()
            {
                entryGroupIndex_detailView = -1;
                isTargetGroupObtained = false;
                curGroupHasNextEnhanceMark = false;
                curGroupIsOutOfBound = false;
            }
        }

        public class TargetMarkForDisplay
        {
            public bool IsTargetMarkObtained { get; private set; }
            public uint CurSelectedMarkTidInCurGroup { get; private set; }
            public int CurSelectedMarkIndexInCurGroup { get; private set; }
            public uint MarkTidBeforeTargetMark { get; private set; }

            public void Set(
                bool isTargetMarkObtained
                , uint curSelectedMarkTidInCurGroup
                , int curSelectedMarkIndexInCurGroup
                , uint markTidForCostInfo)
            {
                IsTargetMarkObtained = IsTargetMarkObtained;
                CurSelectedMarkTidInCurGroup = curSelectedMarkTidInCurGroup;
                CurSelectedMarkIndexInCurGroup = curSelectedMarkIndexInCurGroup;
                MarkTidBeforeTargetMark = markTidForCostInfo;
            }

            public void Reset()
            {
                IsTargetMarkObtained = false;
                CurSelectedMarkTidInCurGroup = 0;
                MarkTidBeforeTargetMark = 0;
                CurSelectedMarkIndexInCurGroup = -1;
            }
        }

        /// <summary>
        /// 0 -> 1 로의 강화를 할떄는 Display 될때는 1 의 Ability 가 출력되고 , 하지만 
        /// 실제 Cost Info 는 1 로 강화를 하는거기 때문에 0 의 CostInfo 를 사용해야함 . 
        /// 즉 CostInfo 를 별도의 ContextInfo 로 관리함 .  
        /// </summary>
        //public class EnhanceCostMarkForTargetMark
        //{
        //    public uint markTidForCostInfo;

        //    public void Reset()
        //    {
        //        markTidForCostInfo = 0;
        //    }
        //}

        public List<ContextInfoCacheByType> dataByType = new List<ContextInfoCacheByType>();

        public TargetMarkForDisplay targetMarkInfo = new TargetMarkForDisplay();
        //   public EnhanceCostMarkForTargetMark markForEnhanceCostInfo = new EnhanceCostMarkForTargetMark();
    }

    //class SimpleTimeAnimation // <FinishParam>
    //{
    //    public float elapsedTime;
    //    public float duration;
    //    public float elapsedTimeNormalized;

    //    /// <summary>
    //    /// float : normalized 0 ~ 1 
    //    /// </summary>
    //    public Action<float> onUpdate;
    //    public Action onFinished;

    //    public void Update()
    //    {
    //        if (elapsedTime >= duration)
    //        {
    //            return;
    //        }

    //        elapsedTime += Time.unscaledDeltaTime;
    //        elapsedTimeNormalized = elapsedTime / duration;

    //        bool finished = false;

    //        if (elapsedTimeNormalized > 1f)
    //        {
    //            finished = true;
    //            elapsedTime = duration;
    //            elapsedTimeNormalized = 1f;
    //        }

    //        onUpdate?.Invoke(elapsedTimeNormalized);

    //        /// 애니 끝 
    //        if (finished)
    //        {
    //            onFinished?.Invoke();
    //            ResetAnimation();
    //        }
    //    }

    //    virtual public void ResetAnimation()
    //    {
    //        elapsedTime = 0f;
    //        duration = 0f;
    //        elapsedTimeNormalized = 0f;
    //        onUpdate = null;
    //        onFinished = null;
    //    }
    //}

    [Serializable]
    class CurrencyGroup
    {
        public Image imgIcon;
        public Text txtName;
        public Text txtCnt;
    }

    [Serializable]
    class EnhanceProtector
    {
        public Toggle toggle;
        public Image imgIcon;
        public Text txtCnt;

        [HideInInspector] public uint tid;
    }

    class EnhanceProp// : SimpleTimeAnimation
    {
        public EnhanceStatus currentEnhanceStatus;
        public ReEnhanceType reEnhanceType;

        public bool scheduledReEnhance;
        public bool isSerialEnhancing;

        public int reEnhanceRemainedCnt;
        public byte reEnhanceCompleteStep;

        public float duration;

        /// <summary>
        /// 강화 진행 X 인 경우만 
        /// </summary>
        public void ResetAll()
        {
            //  ResetAnimation();
            ResetDirectionProp();
            ResetReEnhanceProp();
        }

        public void ResetDirectionProp()
        {
            currentEnhanceStatus = EnhanceStatus.Idle;
            duration = 0f;
        }

        /*public override void ResetAnimation()
        {
            currentEnhanceStatus = EnhanceStatus.Idle;
            base.ResetAnimation();
        }*/

        public void ResetReEnhanceProp()
        {
            reEnhanceType = ReEnhanceType.None;
            duration = 0f;
            isSerialEnhancing = false;
            scheduledReEnhance = false;
            reEnhanceRemainedCnt = 0;
            reEnhanceCompleteStep = 0;
        }
    }

    #endregion

    [Serializable]
    public class WideViewEffectGameObjectByLevel
    {
        [Serializable]
        public class WideViewEffectGameObject
        {
            [Tooltip("순서 (ex 0, 1 ...)")]
            public int order;
            [Tooltip("해당 이펙트가 적용될 MarkGroupIndex (ex 0, 1, 2, 3 ... 현시점 총 그룹이 10개니 0 ~ 9 까지 존재 가능)")]
            public int activeIndex;
            [Tooltip("이펙트 씬 게임오브젝트")]
            public GameObject effectObject;
            [Tooltip("적용되기 직전 연출을 출력시킬 위치값 Transform")]
            public Transform directionPos;
            [Range(0f, 1f), Tooltip("연출 중간에 이펙트 게임오브젝트가 켜지게되는데, 이때 연출중 어느 시점에 켜줄지에 대한 연출 Duration 기준 NormalizedTime. (ex 0.5f => 연출 절반정도에서 이펙트를 켜줌)")]
            public float effectObjAppearNormalizedTimeDuringDirection = 0.7f;
        }

        public GameDB.E_MarkAbleType markType;
        public List<WideViewEffectGameObject> objs;

        public Vector2 GetWorldPos(int order)
        {
            var target = objs.Find(t => t.order == order);

            if (target == null)
                return Vector3.zero;

            Transform targetPosObj = target.directionPos != null ? target.directionPos : target.effectObject.transform;
            return new Vector2(target.directionPos.transform.position.x, target.directionPos.transform.position.y);
        }

        public float GetEffectAppearNormalizedTime(int order)
        {
            var target = objs.Find(t => t.order == order);

            if (target == null)
                return 0f;

            return target.effectObjAppearNormalizedTimeDuringDirection;
        }
    }

    class ExtendedDirectionInfo
    {
        public class DragonImpactStatePhase
        {
            public bool triggerFirst;
            public bool triggerSecond;
            public bool triggerThird;

            public void Reset()
            {
                triggerFirst = false;
                triggerSecond = false;
                triggerThird = false;
            }
            public bool AnyTiggered { get => triggerFirst || triggerSecond || triggerThird; }
        }

        public DragonImpactStatePhase impactFireDragon = new DragonImpactStatePhase();
        public DragonImpactStatePhase impactWaterDragon = new DragonImpactStatePhase();
        public DragonImpactStatePhase impactElectricDragon = new DragonImpactStatePhase();

        public void Reset()
        {
            impactFireDragon.Reset();
            impactWaterDragon.Reset();
            impactElectricDragon.Reset();
        }
        public DragonImpactStatePhase GetImpactDragonPhaseByType(GameDB.E_MarkAbleType type)
        {
            switch (type)
            {
                case GameDB.E_MarkAbleType.RecoveryMark:
                    return impactFireDragon;
                case GameDB.E_MarkAbleType.AttackMark:
                    return impactWaterDragon;
                case GameDB.E_MarkAbleType.DefenseMark:
                    return impactElectricDragon;
            }

            return null;
        }

        public bool IsImpactTriggered()
        {
            return impactFireDragon.AnyTiggered || impactWaterDragon.AnyTiggered || impactElectricDragon.AnyTiggered;
        }

        public GameDB.E_MarkAbleType GetCurrentTriggeredMarkType()
        {
            if (impactFireDragon.AnyTiggered)
                return GameDB.E_MarkAbleType.RecoveryMark;
            if (impactWaterDragon.AnyTiggered)
                return GameDB.E_MarkAbleType.AttackMark;
            if (impactElectricDragon.AnyTiggered)
                return GameDB.E_MarkAbleType.DefenseMark;
            return GameDB.E_MarkAbleType.None;
        }
    }

    //class UIUpdateContextInfo
    //{
    //    public bool showExitBtn;

    //    public void Reset()
    //    {
    //        Reset_ShowExitBtn();
    //    }

    //    public void Reset_ShowExitBtn()
    //    {
    //        showExitBtn = false;
    //    }
    //}

#if UNITY_EDITOR
    #region Editor Code
    private void OnDrawGizmos()
    {
        DrawMarkHandle(marks_fireDragon);
        DrawMarkHandle(marks_waterDragon);
        DrawMarkHandle(marks_electricDragon);
    }

    private void DrawMarkHandle(List<MarkGroup> list)
    {
        try
        {
            if (list == null)
                return;

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] != null
                    && list[i].markNode.gameObject.activeInHierarchy)
                {
                    GUIStyle style = new GUIStyle();
                    style.fontStyle = FontStyle.Bold;
                    style.normal.textColor = list[i].startTid > 0 ? Color.white : Color.red;

                    Handles.Label(list[i].markNode.transform.position, "Order : " + i.ToString() + " , Tid : " + list[i].startTid, style);

                    if (i + 1 < list.Count && list[i + 1] != null && list[i + 1].markNode.gameObject.activeInHierarchy)
                    {
                        Handles.DrawLine(list[i].markNode.transform.position, list[i + 1].markNode.transform.position);
                    }
                }

            }
        }
        catch (Exception exp)
        {

        }
    }
    #endregion
#endif
}
