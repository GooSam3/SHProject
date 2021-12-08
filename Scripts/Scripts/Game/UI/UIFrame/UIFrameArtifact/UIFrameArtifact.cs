using GameDB;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WebNet;
using ZNet;
using ZNet.Data;

public class UIFrameArtifact : ZUIFrameBase
{
    public enum MainTab
    {
        None = 0,
        Manufacture, // 제작/승급탭
        Link // 링크탭
    }

    /// <summary>
    ///  링크는 별도로 캡쳐했다가 들어갈 필요 없으므로 , Manufacture 만 캡쳐링 용 
    /// </summary>
    public class LastTabInfoDataSave
    {
        public const string DataSaveKeyPrefix = "ArtifactSave_";

        public const string ArtifactSaveKey = "Artifact";
        public const string MainTabSaveKey = "MainTab";
        public const string MiddleTabSaveKey = "MiddleTab";

        public uint artifactIDCaptured;
        public MainTab lastTabCaptured;
        public UIFrameArtifactManufacture.MiddleTab manuFactureMiddleTabCaptured;

        public static string Prefix { get { return DataSaveKeyPrefix + Me.CurCharData.ID; } }
        public void Save(
            uint artifactID
            , MainTab mainTab
            , UIFrameArtifactManufacture.MiddleTab manufactureMiddleTab)
        {
            PlayerPrefs.SetInt(string.Format("{0}{1}", Prefix, ArtifactSaveKey), (int)artifactID);
            PlayerPrefs.SetInt(string.Format("{0}{1}", Prefix, MainTabSaveKey), (int)mainTab);
            PlayerPrefs.SetInt(string.Format("{0}{1}", Prefix, MiddleTabSaveKey), (int)manufactureMiddleTab);
        }

        public void ClearData()
        {
            PlayerPrefs.DeleteKey(string.Format("{0}{1}", Prefix, ArtifactSaveKey));
            PlayerPrefs.DeleteKey(string.Format("{0}{1}", Prefix, MainTabSaveKey));
            PlayerPrefs.DeleteKey(string.Format("{0}{1}", Prefix, MiddleTabSaveKey));
        }

        public bool LoadData(
            out uint artifactID
            , out MainTab mainTab
            , out UIFrameArtifactManufacture.MiddleTab manufactureMiddleTab)
        {
            artifactID = (uint)PlayerPrefs.GetInt(string.Format("{0}{1}", Prefix, ArtifactSaveKey), 0);
            mainTab = (MainTab)(PlayerPrefs.GetInt(string.Format("{0}{1}", Prefix, MainTabSaveKey), (int)MainTab.None));
            manufactureMiddleTab = (UIFrameArtifactManufacture.MiddleTab)(PlayerPrefs.GetInt(string.Format("{0}{1}", Prefix, MiddleTabSaveKey), (int)UIFrameArtifactManufacture.MiddleTab.None));
            return mainTab == MainTab.Manufacture;
        }
    }

    public class ShortCutFeature
    {
        public class ManufactureShortCut
        {
            public uint targetArtifactTid;
            public UIFrameArtifactManufacture.MiddleTab middleTab;
        }

        public class LinkShortCut
        {
        }

        public ManufactureShortCut manufacture = new ManufactureShortCut();
        public LinkShortCut link = new LinkShortCut();

        public bool reserved;
        public MainTab reservedTab;

        public void Release()
        {
            reserved = false;
            reservedTab = MainTab.None;
            manufacture.middleTab = UIFrameArtifactManufacture.MiddleTab.None;
            manufacture.targetArtifactTid = 0;
        }
    }

    [Serializable]
    public class SingleMainTab
    {
        public MainTab type;
        public ZToggle toggle;
    }

    public override bool IsBackable => true;

	#region SerializedField
	#region Preference Variable
	[SerializeField] private MainTab leftScrollDefaultTap = MainTab.Manufacture;
    #endregion

    #region UI 
    [SerializeField] private Image imgBlockerTilInit;

    [SerializeField] private UIFrameArtifactOverlayPopup_SelectMaterial popup_selectMaterial;
    [SerializeField] private UIPopupArtifactItemInfo popup_artifactInfo;
    [SerializeField] private GameObject artifactInfoCloser;

    [SerializeField] private UIFrameArtifactManufacture Manufacture;
    [SerializeField] private UIFrameArtifactLink Link;

    [SerializeField] private List<SingleMainTab> mainTabs;
    #endregion
    #endregion

    #region System Variables
    private UIFrameArtifactAbilityActionBuilder abilityActionBuildHelper;
    private MainTab curSelectedLeftScrollTab;
    Coroutine waitForInitCo;

    private static ShortCutFeature _ShortCutFeature = new ShortCutFeature();
    private static LastTabInfoDataSave _LastTabCapturer = new LastTabInfoDataSave();
    public static ShortCutFeature.ManufactureShortCut _ManufactureRuntimeShortData = new ShortCutFeature.ManufactureShortCut();

    private bool init;

    #endregion

    #region Public Variables
    public static bool Temp_IsOpening = false; 
    #endregion

    #region Properties 
    public UIFrameArtifactAbilityActionBuilder AbilityActionBuildHelper { get { return abilityActionBuildHelper; } }
    public UIFrameArtifactManufacture _Manufacture { get { return Manufacture; } }
    #endregion

    #region Unity Methods
    private void Awake()
    {
        popup_selectMaterial.gameObject.SetActive(false);
        popup_artifactInfo.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        Temp_IsOpening = false;
    }

    private void Update()
    {
        /// shortCut 실시간 체크 . 비동기 로직 대비한 코드 .
        PerformShortCutFeature();
    }

    private void OnDestroy()
    {
        SaveTabCaptureData();
    }

    private void OnApplicationPause(bool pause)
    {
        SaveTabCaptureData();
    }

    private void OnApplicationQuit()
    {
        SaveTabCaptureData();
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Shortcut 기능을 사용할 것을 사전에 알림. 
    /// Open 전에 값을 ShortCut 여부를 세팅하기 위해 static 으로 수정
    /// </summary>
    public static void ScheduleShortCut_ArtifactManufactureByID(uint artifactTID, UIFrameArtifactManufacture.MiddleTab desiredMiddleTab = UIFrameArtifactManufacture.MiddleTab.None)
    {
        _ShortCutFeature.manufacture.targetArtifactTid = artifactTID;
        _ShortCutFeature.manufacture.middleTab = desiredMiddleTab == UIFrameArtifactManufacture.MiddleTab.None ? UIFrameArtifactManufacture.MiddleTab.Look : desiredMiddleTab;
        _ShortCutFeature.reserved = true;
        _ShortCutFeature.reservedTab = MainTab.Manufacture;
    }

    public static void TryShortCutIfLastTabCaptured(bool isFromPreferenceOrRuntimeData)
    {
        if (_ShortCutFeature.reserved)
            return;

        /// PlayerPreference 에서 데이터를 가져옴 
        if (isFromPreferenceOrRuntimeData)
        {
            /// 재접시 테이블 데이터 삭제를 대비해서 예외 처리함
            if (_LastTabCapturer.LoadData(
                out var artifactID
                , out var mainTab
                , out var middleTab)
                && DBArtifact.IsArtifactIDExist(artifactID))
            {
                ScheduleShortCut_ArtifactManufactureByID(artifactID, middleTab);
            }
        }
        /// 마지막으로 저장되어 있던 런타임 데이터를 가져옴 
        else
        {
            ScheduleShortCut_ArtifactManufactureByID(_ManufactureRuntimeShortData.targetArtifactTid, _ManufactureRuntimeShortData.middleTab);
        }
    }

    public UIFrameArtifactOverlayPopup_SelectMaterial OpenSelectMaterialPopup()
    {
        popup_selectMaterial.Open();
        return popup_selectMaterial;
    }
    public UIFrameArtifactOverlayPopup_SelectMaterial GetArtifactPopup_SelectMaterial()
    {
        return popup_selectMaterial;
    }

    public UIPopupArtifactItemInfo OpenArtifactInfoPopup(uint artifactID)
    {
        if (popup_artifactInfo.gameObject.activeSelf)
            return null;

        popup_artifactInfo.Initialize(artifactID, null, false, false
            , onCloseCallback: () => artifactInfoCloser.SetActive(false));
        popup_artifactInfo.gameObject.SetActive(true);
        artifactInfoCloser.SetActive(true);
        return popup_artifactInfo;
    }

    public UIPopupArtifactItemInfo GetArtifactPopup()
    {
        return popup_artifactInfo;
    }

    public void AssignMaterialSlotObject(
        UIFrameArtifactResourceSlot sourceObj
        , RectTransform parent
        , int assignCount
        , Action<UIFrameArtifactResourceSlot> onCreated)
    {
        for (int i = 0; i < assignCount; i++)
        {
            AddMaterialSlot(onCreated, sourceObj, parent);
        }
    }

    #region Common
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
                },
                () =>
                {
                     onConfirmed?.Invoke();
                    _popup.Close();
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

    #region Overrides 
    protected override void OnInitialize()
    {
        if (init)
            return;

        try
        {
            base.OnInitialize();

            abilityActionBuildHelper = new UIFrameArtifactAbilityActionBuilder();

            List<string> prefabsToLoad = new List<string>()
        {
            nameof(ScrollArtifactManufactureItemListSlot),
            nameof(ScrollArtifactLinkListSlot)
        };

            int cntLoaded = 0;

            for (int i = 0; i < prefabsToLoad.Count; i++)
            {
                ZPoolManager.Instance.Spawn(E_PoolType.UI, prefabsToLoad[i], delegate
                {
                    cntLoaded++;

                    if (cntLoaded == prefabsToLoad.Count)
                    {
                        // 프리팹 로딩 완료 , 기타 Init 
                        Manufacture.Initialize(this);
                        Link.Initialize(this);
                        popup_selectMaterial.Initialize(this);
                        init = true;
                        imgBlockerTilInit.gameObject.SetActive(false);
                    }
                }, bActiveSelf: false);
            }
        }
        catch (Exception exp)
        {
            ZLog.LogError(ZLogChannel.UI, exp.Message);
            UIManager.Instance.Close<UIFrameArtifact>();
        }
    }

    protected override void OnShow(int _LayerOrder)
    {
        if (_ShortCutFeature.reserved == false)
        {
            /// 마지막 메인탭이 Manufacture 이었다면 
            if (curSelectedLeftScrollTab == MainTab.Manufacture)
            {
                TryShortCutIfLastTabCaptured(false);
            }
        }

        base.OnShow(_LayerOrder);
        UIManager.Instance.Find<UIFrameHUD>().SetSubHudFrame(E_UIStyle.FullScreen);
        //UIManager.Instance.TopMost<UISubHUDCharacterState>(true);

        // UIManager.Instance.Find<UISubHUDCurrency>().ShowSubCurrency();

        if (init == false)
        {
            if (waitForInitCo != null)
                StopCoroutine(waitForInitCo);

            waitForInitCo = StartCoroutine(WaitTilInitDone(() => Setup()));
        }
        else
        {
            Setup();
        }
    }

    protected override void OnHide()
    {
        // SaveTabCaptureData();
        
        artifactInfoCloser.SetActive(false);

        if (init)
        {
            popup_artifactInfo.Close();
            _ShortCutFeature.Release();
            // curSelectedLeftScrollTab = MainTab.None;
            Manufacture.Hide();
            Manufacture.Release();
            Link.Hide();
        }

        base.OnHide();
        UIManager.Instance.Find<UIFrameHUD>().SetSubHudFrame();
        // UIManager.Instance.Find<UISubHUDCurrency>().HideSubCurrency();
    }
    #endregion

    #region Private Methods
    private IEnumerator WaitTilInitDone(Action onFinished)
    {
        while (init == false)
            yield return null;
        onFinished?.Invoke();
    }

    private void Setup()
    {
        popup_selectMaterial.Close();

        if (PerformShortCutFeature() == false)
            SetMainTab_Manufacture(null, null);
        //SetMainTab(leftScrollDefaultTap);

        Temp_IsOpening = false;
    }

    private UIFrameArtifactResourceSlot AddMaterialSlot(
        Action<UIFrameArtifactResourceSlot> onCreated
        , UIFrameArtifactResourceSlot sourceObj
        , RectTransform parent)
    {
        var t = Instantiate(sourceObj, parent);
        onCreated?.Invoke(t);
        return t;
    }

    private void SetMainTab_Manufacture(uint? desiredArtifactID, UIFrameArtifactManufacture.MiddleTab? desiredMiddleTab)
    {
        SetMainTab(MainTab.Manufacture,
            open: () =>
            {
                Manufacture.Open(desiredArtifactID, desiredMiddleTab);
            });
    }

    private void SetMainTab_Link()
    {
        SetMainTab(MainTab.Link,
            open: () =>
            {
                Link.Open();
            });
    }

    private void SetMainTab(MainTab tab, Action open)
    {
        var target = mainTabs.Find(t => t.type == tab);

        if (target == null)
            return;

        target.toggle.SelectToggle();
        curSelectedLeftScrollTab = tab;

        switch (tab)
        {
            case MainTab.Manufacture:
                Link.Hide();
                //Manufacture.Open(param);
                break;
            case MainTab.Link:
                Manufacture.Hide();
                //Link.Open();
                break;
        }

        open.Invoke();
    }

    void SaveTabCaptureData()
    {
        if (this.curSelectedLeftScrollTab == MainTab.Manufacture)
        {
            _LastTabCapturer.Save(
                Manufacture.GetCurSelectedArtifactID()
                , MainTab.Manufacture
                , Manufacture.GetCurSelectedMiddleTab());
        }
        else
        {
            _LastTabCapturer.ClearData();
        }
    }

    private bool PerformShortCutFeature()
    {
        if (init == false)
            return false;

        if (_ShortCutFeature.reserved)
        {
            if (_ShortCutFeature.reservedTab == MainTab.Manufacture)
            {
                SetMainTab_Manufacture(_ShortCutFeature.manufacture.targetArtifactTid, _ShortCutFeature.manufacture.middleTab);
            }
            else if (_ShortCutFeature.reservedTab == MainTab.Link)
            {
                SetMainTab_Link();
            }

            //SetMainTab(
            //    shortCutFeature.reservedTab
            //    , new object[] { shortCutFeature.manufacture.targetArtifactTid, shortCutFeature.manufacture.middleTab });

            _ShortCutFeature.Release();
            return true;
        }

        return false;
    }

    #region Common
    public string ConvertColorToHex(Color color)
    {
        return ((int)(color.r * 255)).ToString("X2") + ((int)(color.g * 255)).ToString("X2") + ((int)(color.b * 255)).ToString("X2");
    }

    static public Color GetColorByGrade(byte grade)
    {
        return ParseColor("#" + DBUIResouce.GetGradeTextColor(GameDB.E_UIType.Item, grade));
    }
    /// <summary>
    ///  ex) #ff0fab
    /// </summary>
    static public Color ParseColor(string str)
    {
        Color result = new Color(0, 0, 0, 0);

        if (ColorUtility.TryParseHtmlString(str, out result) == false)
        {
            ZLog.LogError(ZLogChannel.UI, "Color parsing error : " + str);
        }

        return result;
    }
    #endregion
    #endregion

    #region Inspector Events 
    public void OnMainTabChanged(Toggle toggle)
    {
        if (toggle.isOn == false)
            return;

        var selected = mainTabs.Find(t => t.toggle == toggle);

        if (selected != null)
        {
            if (selected.type == MainTab.Manufacture)
            {
                SetMainTab_Manufacture(Manufacture.GetCurSelectedArtifactID(), Manufacture.GetCurSelectedMiddleTab());
            }
            else if (selected.type == MainTab.Link)
            {
                SetMainTab_Link();
            }
            //           SetMainTab(selected.type
            //            , new object[] { Manufacture.GetCurSelectedArtifactID(), Manufacture.GetPrevSelectedMiddleTab() });
        }
    }

    public void OnClickClose()
    {
        base.Close();
    }
    #endregion
}
