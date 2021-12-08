using GameDB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using WebNet;
using ZNet;
using ZNet.Data;

public class UIFrameMark : ZUIFrameBase
{
    public enum StepTab
    {
        None = 0,
        Wide = 100,
        Detail = 200
    }

    [Serializable]
    public class SingleTab
    {
        public GameDB.E_MarkAbleType type;
        public ZToggle toggle;
        public Text txtTitle;
    }

    /// <summary>
    /// Type 별 SharedInfo 
    /// </summary>
    [Serializable]
    public class MarkElementSharedInfo
    {
        [Serializable]
        public class GameObjectList
        {
            public List<GameObject> gameObjects;
        }

        public GameDB.E_MarkAbleType type;
        public Sprite iconSprite;
        public Sprite detailViewGaugeSprite;
        public Color commonColor;
        public string commonColorDataKey;
        public string commonTextKey;
        public string commonToolTipTitleKey;

        [Header("-- (문양 Wide View) 서브 마크들 공용 Sprite Name")]
        public Sprite wideViewSubMarkSprite;
        // public string wideViewSubMarkCommonSpriteName;

        [Header("-- (문양 Wide View) 노멀 상태에서 획득하지 못했는데 다음 단계의 마크도 아닌 경우 출력되는 이펙트들 --")]
        public List<GameObject> markGroupSmallEffect_UnDoneNotNextEnhance;
        [Header("-- (문양 Wide View) 노멀 상태에서 다음 단계의 강화 마크일 경우 출력되는 이펙트들 --")]
        public List<GameObject> markGroupSmallEffect_NextEnhanceTarget;
        [Header("-- (문양 Wide View) 노멀 상태에서 완료된 마크일 경우 출력되는 이펙트들 --")]
        public List<GameObject> markGroupSmallEffect_UnDoneComplete;
        [Header("-- 문양 Wide View 에서 전부 강화가 된 상태에서 출력되는 이펙트들 --")]
        public List<GameObject> markGroupSmallEffect_DoneState;

        [Header("-- 문양 Detail View 에서 단계별 Mark 강화 여부에 따라 출력해줄 이펙트들 --")]
        [SerializeField]
        public List<GameObjectList> markGroupBigEffect_SingleSubMarkByStepOrder;
        [Header("-- 문양 Detail View 에서 전부 강화가 된 상태에서 센터에 출력되는 이펙트 --")]
        public List<GameObject> markGroupBigEffect_Done;

        [Space(20)]
        public Sprite markGroupCircleEffectGlowOnObtained;
        public GameObject markGroupEffectGlowObjOnObtained;
        public List<Sprite> markGroupStepSmallSpriteByOrder;
        public List<Sprite> markGroupStepBigSpriteByOrder;
    }

    public override bool IsBackable => true;

	#region Const 
	/// <summary>
	///  하나의 Step 노드마다 3개의 문양 데이터가 들어감 
	/// </summary>
	public const uint MarkCountPerGroup = 3;
    #endregion

    #region SerializedField
    #region Preference Variable
    [Header("재화 기본 폰트 색상")]
    [SerializeField] private Color costCntTxtBaseColor;
    [Header("재화 부족 폰트 색상")]
    [SerializeField] private Color costCntTxtNotEnoughColor;
    [Header("재화 충분 폰트 색상")]
    [SerializeField] private Color costCntTxtEnoughColor;

    [Header("노드 세부사항 UI 에서 보유중인 노드 타입의 폰트 색상")]
    [SerializeField] private Color obtainedNodeTypeColor;
    #endregion

    #region UI Variables
    [SerializeField] private List<SingleTab> tabs;
    [SerializeField] private GameObject _interactionBlocker;

    [Header("***** 타입별 공유 세팅 *****"), SerializeField] private List<MarkElementSharedInfo> sharedInfo;
    [SerializeField] private Color tabInactiveTxtColor;
    [SerializeField] private Color markInactiveColor;

    [Header("중앙에 위치하고 있는 Step Group")]
    [SerializeField] private MarkStepHandler stepHandler;

    [SerializeField] private List<Sprite> spritesGroupStep;

    [SerializeField] private List<ModelBlockObj> objsToMoveToBlock3DModel;
    [SerializeField] private string spawnedEffectsLayer = "UIFront";
    [SerializeField] private int spawnedEffectSortingOrder = 2;

    //    [Header("Step 별 아이콘 이미지")]
    //    [SerializeField] private List<Sprite> spritesByOrderInGroup;

    //   [Header("우측에 위치하고 있는 Stat Group")]
    //    [SerializeField] private MarkStatHandler statHandler;
    #endregion
    #endregion

    #region System Variables
    private bool initDone;
    private GameDB.E_MarkAbleType currentMainTab;
    private StepTab currentStepTab;
    // private StatTab currentStatTab;
    private Coroutine showCo;

    private MarkAbilityActionBuilder statHelper;

    private bool scheduledUpdateUI_stepTab;
    //private bool scheduledUpdateUI_statTab;

    private InteractionController interactionController;

    #endregion

    #region Properties 
    public GameDB.E_MarkAbleType CurrentMainTab { get { return currentMainTab; } }
    public Color InactiveColor { get { return markInactiveColor; } }
    public Color CostBaseColor { get { return costCntTxtBaseColor; } }
    public Color CostEnoughColor { get { return costCntTxtEnoughColor; } }
    public Color CostNotEnoughColor { get { return costCntTxtNotEnoughColor; } }
    public Color ObtainedNodeTypeColor { get { return obtainedNodeTypeColor; } }
    public bool CanInteract { get { return interactionController.CanInteract; } }
    public InteractionController InteractionHandler { get { return interactionController; } }
    public MarkAbilityActionBuilder StatHelper { get { return statHelper; } }
    public string RuntimeSpawnedEffectLayer => spawnedEffectsLayer;
    public int RuntimeSpawnedEffectSortingOrder => spawnedEffectSortingOrder;
    #endregion

    #region Unity Methods
    private void Update()
    {
        if (interactionController != null)
        {
            if (interactionController.CanInteract == false)
                interactionController.Update(Time.unscaledDeltaTime);
        }

        /// TODO : ForceImmedietaly 도 만들까 ? 
        if (scheduledUpdateUI_stepTab)
        {
            scheduledUpdateUI_stepTab = false;
            UpdateUI_StepTab();
        }

        //if (scheduledUpdateUI_statTab)
        //{
        //    scheduledUpdateUI_statTab = false;
        //    UpdateUI_StatTab();
        //}
    }
    #endregion

    #region Public Methods
    public void SetTab(GameDB.E_MarkAbleType tab)
    {
        var target = tabs.Find(t => t.type == tab);
        if (target != null)
        {
            target.toggle.SelectToggle();
        }
    }

    /// <param name="checkInteractionState">True : CanInteraction 체킹함, False : CanInteraction 무시하고 세팅함</param>
    public void SetStepTab(StepTab tab, bool checkInteractionState)
    {
        if (checkInteractionState && CanInteract == false)
        {
            return;
        }

        currentStepTab = tab;
        ScheduleUpdateUI_StepTab();

        //StatTab targetStatTab;
        //if (tab == StepTab.Wide)
        //    targetStatTab = StatTab.Wide;
        //else if (tab == StepTab.Detail)
        //    targetStatTab = StatTab.Detail;
        //else targetStatTab = StatTab.None;
        //SetStatTab(targetStatTab);
    }

    public void RefreshCurrentStepTab()
    {
        ScheduleUpdateUI_StepTab();
    }

    //public void SetStatTab(StatTab tab)
    //{
    //    currentStatTab = tab;
    //    ScheduleUpdateUI_StatTab();
    //}

    public void ScheduleUpdateUI_StepTab()
    {
        scheduledUpdateUI_stepTab = true;
    }

    //public void UpdateCurrentScene()
    //{
    //    SetStepTab(currentStepTab, false);
    //}

    //public void ScheduleUpdateUI_StatTab()
    //{
    //    scheduledUpdateUI_statTab = true;
    //}

    public MarkElementSharedInfo GetSharedInfo(GameDB.E_MarkAbleType type)
    {
        return sharedInfo.Find(t => t.type == type);
    }

    public Sprite GetRepresentSprite(GameDB.E_MarkAbleType type)
    {
        var target = GetSharedInfo(type);
        if (target == null)
            return null;
        return target.iconSprite;
    }

    public Sprite GetSpriteGroupCenterStep(int order)
    {
        if (order >= spritesGroupStep.Count)
        {
            ZLog.LogError(ZLogChannel.UI, "Please add targetSprite Order : " + order);
            return null;
        }

        return spritesGroupStep[order];
    }

    public Color GetColorByType(GameDB.E_MarkAbleType type)
    {
        var target = GetSharedInfo(type);
        if (target == null)
            return Color.black;
        return ResourceSetManager.Palette.GetPaletteColor(target.commonColorDataKey);
    }

    public string GetTextKeyByType(GameDB.E_MarkAbleType type)
    {
        var target = GetSharedInfo(type);
        if (target == null)
            return string.Empty;
        return target.commonTextKey;
    }

    #region Common
    public string MakeCostCountString(
        ulong myCurrencyCnt
        , ulong requiredCurrencyCnt
        , Color? enoughColor = null
        , Color? notEnoughColor = null
        , Color? baseColor = null)
    {
        if (enoughColor.HasValue == false)
        {
            enoughColor = CostEnoughColor;
        }
        if (notEnoughColor.HasValue == false)
        {
            notEnoughColor = CostNotEnoughColor;
        }
        if (baseColor.HasValue == false)
        {
            baseColor = CostBaseColor;
        }

        bool isEnough = myCurrencyCnt >= requiredCurrencyCnt;

        Color myCurrencyColor = isEnough ? enoughColor.Value : notEnoughColor.Value;

        return string.Format("<color=#{0}>{1}</color> <color=#{2}>/ {3}</color>"
            , ColorUtility.ToHtmlStringRGB(myCurrencyColor)
            , myCurrencyCnt.ToString("n0")
            , ColorUtility.ToHtmlStringRGB(baseColor.Value
            )
            , requiredCurrencyCnt.ToString("n0"));
    }

    public bool HasMyNextMark(GameDB.E_MarkAbleType type, params uint[] checkTids)
    {
        if (checkTids == null)
            return false;

        if (Me.CurCharData.GetMyNextMarkTID(type, out uint myNextTid) == false)
        {
            return false;
        }

        return checkTids.Contains(myNextTid);
    }

    public bool InitializeShaderClippingUpdater(GameObject root)
    {
        bool result = false; 
        var targets = root.GetComponentsInChildren<UIShaderClipingUpdater>(true);

        if (targets != null)
        {
            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i] != null)
                {
                    result = true;
                    targets[i].DoUIWidgetInitialize(this);
                }
            }
        }

        return result;
    }

    public void SpawnGameObject(List<GameObject> sourceList, Transform root, Action<GameObject> onCreate = null)
    {
        if (sourceList != null)
        {
            for (int i = 0; i < sourceList.Count; i++)
            {
                if (sourceList[i] != null)
                {
                    var t = Instantiate(sourceList[i], root);
                    ResetTransform(t.transform);
                    t.gameObject.SetLayersRecursively(RuntimeSpawnedEffectLayer);
                    if (InitializeShaderClippingUpdater(t))
                    {
                        var sg = t.gameObject.AddComponent<UnityEngine.Rendering.SortingGroup>();
                        sg.sortingLayerName = RuntimeSpawnedEffectLayer;
                        sg.sortingOrder = RuntimeSpawnedEffectSortingOrder;
                    }
                    onCreate?.Invoke(t);
                }
            }
        }
    }

    public void ResetTransform(Transform t)
    {
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;
        t.localScale = Vector3.one;
    }

    public void OpenTwoButtonQueryPopUp(
            string title, string content, Action onConfirmed, Action onCanceled = null
            , string cancelText = ""
            , string confirmText = "")
    {
        if (string.IsNullOrEmpty(cancelText))
        {
            cancelText = DBLocale.GetText("Cancel_Button");
        }

        if (string.IsNullOrEmpty(confirmText))
        {
            confirmText = DBLocale.GetText("OK_Button");
        }

        UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
        {
            _popup.Open(title, content, new string[] { cancelText, confirmText }, new Action[] {
                () =>
                {
                    onCanceled?.Invoke();
                    _popup.Close();
                    
                    UIManager.Instance.ChangeMainCameraStack(CManagerUIFrameFocusBase.E_UICameraStack.None);
                    UIManager.Instance.ActiveUICamera(CManagerUIFrameFocusBase.E_UICamera.Front, true);
                    UIManager.Instance.ActiveUICamera(CManagerUIFrameFocusBase.E_UICamera.Model3D, true);
                },
                () =>
                {
                     onConfirmed?.Invoke();
                    _popup.Close();
                    
                    UIManager.Instance.ChangeMainCameraStack(CManagerUIFrameFocusBase.E_UICameraStack.None);
                    UIManager.Instance.ActiveUICamera(CManagerUIFrameFocusBase.E_UICamera.Front, true);
                    UIManager.Instance.ActiveUICamera(CManagerUIFrameFocusBase.E_UICamera.Model3D, true);
                }});
        });
    }

    public void OpenNotiUp(string content, string title = "확인", Action onConfirmed = null)
    {
        DBLocale.TryGet(ZUIString.LOCALE_OK_BUTTON, out Locale_Table table);

        if (table != null)
        {
            title = DBLocale.GetText(table.Text);
        }

        //if (string.IsNullOrEmpty(title))
        //{
        //	title = ZUIString.ERROR;
        //}

        UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
        {
            _popup.Open(title, content, new string[] { title }, new Action[] { () =>
                {
                    onConfirmed?.Invoke();
                    _popup.Close();

                    UIManager.Instance.ChangeMainCameraStack(CManagerUIFrameFocusBase.E_UICameraStack.None);
                    UIManager.Instance.ActiveUICamera(CManagerUIFrameFocusBase.E_UICamera.Front, true);
                    UIManager.Instance.ActiveUICamera(CManagerUIFrameFocusBase.E_UICamera.Model3D, true);
                }});
        });
    }

    public void HandleError(ZWebCommunicator.E_ErrorType _errorType, ZWebReqPacketBase _reqPacket, ZWebRecvPacket _recvPacket)
    {
        OpenErrorPopUp(_recvPacket.ErrCode);
    }

    public void OpenErrorPopUp(ERROR errorCode, Action onConfirmed = null)
    {
        Locale_Table table;

        // 에러코드 확인누르고 특별한 처리가 필요한경우 여기서 처리함 (onConfirmed)
        // if(errorCode == e)

        DBLocale.TryGet(errorCode.ToString(), out table);

        if (table != null)
        {
            OpenNotiUp(table.Text, onConfirmed: onConfirmed);
        }
        else
        {
            OpenNotiUp("문제가 발생하였습니다.", onConfirmed: onConfirmed);
        }
    }
    #endregion
    #endregion

    #region Override
    protected override void OnInitialize()
    {
        base.OnInitialize();

        if(LayerMask.NameToLayer(spawnedEffectsLayer) == -1)
		{
            ZLog.LogError(ZLogChannel.UI, "Target Layer For Movin Objects does not exist, falllBack to UI");
            spawnedEffectsLayer = "UI";
        }

        interactionController = new InteractionController() { interactionBlocker = _interactionBlocker };
        statHelper = new MarkAbilityActionBuilder();
        _interactionBlocker.SetActive(false);

        /// 미리 박혀있는 이펙트들도있음. 
        InitializeShaderClippingUpdater(gameObject);

        if (stepHandler.Initialize(this) == false)
        {
            Close();
            return;
        }

        //if (statHandler.Initialize(this) == false)
        //{
        //    Close();
        //    return;
        //}

        initDone = true;
    }

    protected override void OnShow(int _LayerOrder)
    {
        base.OnShow(_LayerOrder);

        UIManager.Instance.Find<UIFrameHUD>().SetSubHudFrame(E_UIStyle.FullScreen);

        UIManager.Instance.ActiveUICamera(CManagerUIFrameFocusBase.E_UICamera.Model3D, true);
        UIManager.Instance.ActiveUICamera(CManagerUIFrameFocusBase.E_UICamera.Front, true);

        if (showCo != null)
        {
            StopCoroutine(showCo);
            showCo = null;
        }

		if (objsToMoveToBlock3DModel.Count > 0)
		{
			/// 캔버스를 얻어서 대이동
			var targetCanvas = UIManager.Instance.GetCanvas(CManagerUIFrameFocusBase.E_UICanvas.Front);

			if (targetCanvas != null)
			{
				objsToMoveToBlock3DModel.ForEach(t => t.key = UIManager.Instance.SetSystemObject(this, t.gameObject, targetCanvas.gameObject));
			}
		}

        if (initDone == false)
        {
            showCo = StartCoroutine(WaitUntilInitDone(() =>
            {
                showCo = null;
                ShowFirstScene(GetFirstDesireTab(), GetFirstDesireStepTab());
            }));
        }
        else
        {
            ShowFirstScene(GetFirstDesireTab(), GetFirstDesireStepTab());
        }
    }

    protected override void OnHide()
    {
        base.OnHide();
        stepHandler.Release();
        UIManager.Instance.Find<UIFrameHUD>().SetSubHudFrame();
        UIManager.Instance.ActiveUICamera(CManagerUIFrameFocusBase.E_UICamera.Model3D, false);
        UIManager.Instance.ActiveUICamera(CManagerUIFrameFocusBase.E_UICamera.Front, false);
        interactionController.Reset();

        if (objsToMoveToBlock3DModel.Count > 0)
		{
            var targetCanvas = UIManager.Instance.GetCanvas(CManagerUIFrameFocusBase.E_UICanvas.Front);

            if (targetCanvas != null)
            {
                objsToMoveToBlock3DModel.ForEach(t => UIManager.Instance.GetOutSystemObject(this, t.key, gameObject));
            }
		}
    }
    #endregion

    #region Private Methods
    IEnumerator WaitUntilInitDone(Action onFinished)
    {
        yield return null;
        while (initDone == false)
            yield return null;
        onFinished();
    }

    /// <summary>
    ///  나중에 또 기기에 저장하고 ShortCut 기능이 들어가고 이럴수 있기때문에
    ///  우선 처음 진입때 FirstTab 을 가변적으로 수정하기 용이하게 작업함 
    /// </summary>
    GameDB.E_MarkAbleType GetFirstDesireTab()
    {
        var t = GameDB.E_MarkAbleType.RecoveryMark;

        /// ShortCut 기능이 추가되면 
        /// 여기서 중간에 넣어주면 되겠지 ? 

        return t;
    }

    StepTab GetFirstDesireStepTab()
    {
        var t = StepTab.Wide;

        /// 마찬가지 Shortcut 기능 대비 

        return t;
    }

    void ShowFirstScene(GameDB.E_MarkAbleType firstMainTab, StepTab firstStepTab)
    {
        currentStepTab = firstStepTab;

        SetTab(firstMainTab);
        SetStepTab(firstStepTab, false);
    }

    void UpdateTabState() // StepTab? desireStepTab = null)
    {
        var target = tabs.Find(t => t.toggle.isOn);
        var targetTab = GameDB.E_MarkAbleType.None;

        if (target != null)
        {
            targetTab = target.type;
        }
        else
        {
            targetTab = currentMainTab;
        }

        if (currentMainTab != targetTab)
        {
            currentMainTab = targetTab;
            /// mainTab 이 변경된 경우에는 StepTab 을 Wide 로 변경
            currentStepTab = StepTab.Wide;

            for (int i = 0; i < tabs.Count; i++)
            {
                if (tabs[i].toggle.isOn)
                {
                    tabs[i].txtTitle.color = Color.white;
                }
                else
                {
                    tabs[i].txtTitle.color = tabInactiveTxtColor;
                }
            }

            stepHandler.OnPreSwitchMainTab(currentMainTab, currentStepTab);
            SetStepTab(currentStepTab, false);
        }
    }

    void UpdateUI_StepTab()
    {
        stepHandler.SetTab(currentStepTab);
        stepHandler.UpdateUI();
    }

    #endregion

    #region Inspector Events ( OnClick etc ) 
    public void OnClickCloseBtn()
    {
        UIManager.Instance.Close<UIFrameMark>();
    }

    public void OnTabValueChanged(Toggle toggle)
    {
        if (toggle.isOn == false)
            return;

        UpdateTabState();
    }
    #endregion

    #region Extra
    /// <summary>
    /// 딜레이 명령 실행 베이스 파라미터 
    /// </summary>
    public class BaseLateCommandParam { }

    /// <summary>
    /// 
    /// </summary>
    public class LateCommandParam_SwitchStepWideToDetail : BaseLateCommandParam
    {
        public int groupArrayIndex;
    }

    //public class LateCommandParam_SwitchStepDetailToWide : BaseLateCommandParam
    //{
    //    public int 
    //}

    /// <summary>
    /// 사용자의 Interaction 을 통제해주는 컨트롤러
    /// 단순 일정 시간동안 interaction 이 불가능한 상태로 State 관리를 하거나
    /// 일정 시간이 지난후에 어떤 명령을 실행해주는(그냥 람다임) 기능 구현
    /// 추후 들어갈 연출 대비하여 미리 설계 
    /// </summary>
    public class InteractionController
    {
        private float remainedSec;

        public bool isLateCommandScheduled;
        public Action<BaseLateCommandParam> onLateCommandExecute;
        public BaseLateCommandParam lateCommandParam;

        public GameObject interactionBlocker;

        public bool CanInteract { get { return remainedSec <= 0 && isLateCommandScheduled == false; } }

        public bool TryScheduleLateCommand(
            float timer
            , Action<BaseLateCommandParam> onExecute
            , BaseLateCommandParam param)
        {
            if (CanInteract == false)
            {
                return false;
            }

            SetTime(timer);
            isLateCommandScheduled = true;
            onLateCommandExecute = onExecute;
            lateCommandParam = param;
            return true;
        }

        public void Update(float timePassed)
        {
            if (CanInteract)
                return;

            remainedSec -= timePassed;
            if (remainedSec <= 0f)
            {
                if (isLateCommandScheduled)
                {
                    onLateCommandExecute?.Invoke(lateCommandParam);
                }

                Reset();
            }
        }

        public void Reset()
        {
            remainedSec = 0f;
            isLateCommandScheduled = false;
            onLateCommandExecute = null;
            lateCommandParam = null;
            if (interactionBlocker != null)
                interactionBlocker.SetActive(false);
        }

        private void SetTime(float sec)
        {
            if (sec > remainedSec)
            {
                if (interactionBlocker != null)
                {
                    interactionBlocker.SetActive(true);
                }

                remainedSec = sec;
            }
        }
    }

    [Serializable]
    public class ColorChangerByObtained
    {
        public Graphic targetGraphic;
        public Color onObtained = Color.black;
        public Color onNotObtained = Color.black;

        public void Set(bool obtained)
        {
            targetGraphic.color = obtained ? onObtained : onNotObtained;
        }
    }

    [Serializable]
    public class ModelBlockObj
	{
        public GameObject gameObject;
        [HideInInspector]
        public int key;
	}
    #endregion
}
