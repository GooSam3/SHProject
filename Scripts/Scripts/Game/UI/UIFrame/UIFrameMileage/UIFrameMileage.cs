using Devcat;
using GameDB;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WebNet;
using ZNet;
using ZNet.Data;
using static DBMileageShop;

/*
 * E_MileageShopType
 * E_SubTapType
 * 
 * */
public class UIFrameMileage : ZUIFrameBase
{
    public override bool IsBackable => true; 

	#region SerializedField
	#region Preference Variable
	#endregion

	#region UI Variables
	/// <summary>
	/// 메인 카테고리는 인스펙터에서 미리 꽃아놓음 
	/// </summary>
	[SerializeField] private MainCategoryTab mainCategoryUI;

    [SerializeField] private UIGroup_Bottom uiGroup_bottom;

    [SerializeField] private ZToggleGroup subCategoryToggleGroup;
    [SerializeField] private RectTransform subCategoryToggleParent;
    [SerializeField] private UIMileageSubCategory subCategorySourceObj;
    [SerializeField] private ScrollRect subCategoryScrollRect;

    [SerializeField] private List<EvaluatorKeyCommonData> commonDataByEvaluatorKey;

    [SerializeField] private List<DataTypeEvaluatorKeyMapped> dataType_evaluatorKeyMapped;

    [SerializeField] private RectTransform popUpBlocker_graphic;
    [SerializeField] private RectTransform popUpObjRoot;

    /// <summary>
    /// 내 마일리지 상태 표시 팝업 
    /// </summary>
    [SerializeField] private UIMileageMyMileagePopup myMileageStatusPopUp;
    /// <summary>
    /// 강림, 펫 디테일 팝업 
    /// </summary>
    [SerializeField] private MileageChangePetPopup change_petStatusPopUp;

    [SerializeField] private MileageItemScrollAdapter ScrollAdapter;

    [SerializeField, Header("인스펙터에서 등록되면 캐싱되고, 필요하면 런타임에서 더 생성")]
    private List<UIMileageSubCategory> subCategoryUIs;
    #endregion
    #endregion

    #region System Variables

    private MileageDataWrapper dataWrapper;
    private MileageDataEvaluatorRegisterer dataEvaluatorRegister;
    private MileageCategoryDescriptor categoryDescriptor;

    private bool updateForced;
    private E_MileageShopType lastUpdatedMainCtgType;
    private uint lastUpdatedSubCtgShopID;
    private bool isScrollDataDirtyForUIUpdate;
    private bool isExchangeUpdateUIDirty;

    private List<EvaluatorKeyStringPair> evaluatorKeyStringPair;

    /// <summary>
    /// 현재 화면에 대해서 구매 대상 아이템의 정보를 가지고 있음. 
    /// 예로 현재는 구매 가능한 팝업을 출력할때 업데이트를 해주는 방식임. 
    /// </summary>
    private BuyInfoContextInfo curContextBuyInfo;

    /// <summary>
    /// 아이템 정보 팝업 
    /// </summary>
    private UIPopupItemInfo itemInfoPopUp;

    private Coroutine showCo;
    private bool initDone;
    #endregion

    #region Properties 
    public MileageDataWrapper DataWrapper { get { return dataWrapper; } }
    public MileageCategoryDescriptor CategoryDescriptor { get { return categoryDescriptor; } }
    #endregion

    #region Unity Methods
    private void Awake()
    {
        var mainTypes = (E_MileageShopType[])Enum.GetValues(typeof(E_MileageShopType));
        // GetAllToggle 작동이상함 코드뺌
        // var allToggles = mainCategoryUI.toggleGroup.GetAllToggle();

        ///// 토글 개수 체킹 
        //if (mainTypes.Length > 0 && allToggles != null)
        //{
        //    /// None 제외해야하므로 - 1
        //    /// 타입이 존재하면 토글을 추가해줘야함 ... 
        //    if (mainTypes.Length - 1 != allToggles.Count)
        //    {
        //        ZLog.LogError(ZLogChannel.UI, "MainCategory must be generated in prefab in advance");
        //    }

        //    /// Toggle 개수를 맞춰야함 
        //    if (mainCategoryUI.tabs.Count != allToggles.Count)
        //    {
        //        ZLog.LogError(ZLogChannel.UI, "Toggle Count is Different, this must match!");
        //    }
        //}

        /// 인스펙터서 미리 등록된(캐싱된) SubCategory 게임오브젝트들 off
        for (int i = 0; i < subCategoryUIs.Count; i++)
        {
            subCategoryUIs[i].gameObject.SetActive(false);
        }

        popUpBlocker_graphic.gameObject.SetActive(false);
        CloseMyMileagePopUp();
    }

    private void Update()
    {
        //if(Input.GetKeyDown(KeyCode.A))
        //{
        //    if (itemInfoPopUp != null)
        //    {
        //        itemInfoPopUp.Initialize(E_ItemPopupType.GemUnEquip, EquipGemSlotList[_slotIdx].ItemTid);
        //    }
        //    else
        //    {
        //        ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIPopupItemInfo), (_obj) =>
        //        {
        //            UIPopupItemInfo obj = _obj.GetComponent<UIPopupItemInfo>();

        //            if (obj != null)
        //            {
        //                itemInfoPopUp = obj;
        //                obj.transform.SetParent(gameObject.transform);
        //                obj.Initialize(E_ItemPopupType.GemUnEquip, 444048);
        //            }
        //        });
        //    }
        // }

        /// TEST 
        /// 
        //if (Input.GetKeyDown(KeyCode.A))
        //{
        //    subCategoryUIs[2].Toggle.isOn = true;
        //}

        //if (scheduledUpdate_data)
        //{
        //    UpdateData();
        //}

        // if (scheduledUpdate_ui)
        //   {
        //       UpdateUI();
        //   }
    }
    #endregion

    #region Public Methods
    public void CloseAllPopUps()
    {
        if (initDone == false)
            return;

        curContextBuyInfo.Reset();
        CloseItemInfoPopup();
        CloseChangePetInfoPopUp();
        CloseMyMileagePopUp();
    }

    public MileageDataEvaluatorKey ConvertStringToEvaluatorKeyType(string str)
    {
        for (int i = 0; i < evaluatorKeyStringPair.Count; i++)
        {
            if (evaluatorKeyStringPair[i].str == str)
                return evaluatorKeyStringPair[i].key;
        }

        ZLog.LogError(ZLogChannel.UI, "No EvaluatorType matched with : " + str);

        return MileageDataEvaluatorKey.None;
    }

    public bool IsMainCategoryVisible(E_MileageShopType type)
    {
        return true;
    }

    //public List<DataTypeEvaluatorKeyMapped.EvaluatorKeyGroup> GetEvaluatorKeyGroupsMappedByDataType(MileageDataEvaluateTargetDataType dataType)
    //{
    //    for (int i = 0; i < dataType_evaluatorKeyMapped.Count; i++)
    //    {
    //        if (dataType_evaluatorKeyMapped[i].dataType == dataType)
    //        {
    //            return dataType_evaluatorKeyMapped[i].evaluatorKeyGroups;
    //        }
    //    }

    //    return null;
    //}

    //public DataTypeEvaluatorKeyMapped.EvaluatorKeyGroup GetEvaluatorKeyGroupByMemberKey(MileageDataEvaluateTargetDataType dataType, MileageDataEvaluatorKey evaluatorKey)
    //{
    //    for (int i = 0; i < dataType_evaluatorKeyMapped.Count; i++)
    //    {
    //        if (dataType_evaluatorKeyMapped[i].dataType == dataType)
    //        {
    //            foreach (var group in dataType_evaluatorKeyMapped[i].evaluatorKeyGroups)
    //            {
    //                if (group.keys.Exists(t => t == evaluatorKey))
    //                {
    //                    return group;
    //                }
    //            }
    //        }
    //    }

    //    return null;
    //}

    public string GetEvaluatorKeyText(MileageDataEvaluatorKey evaluatorKey)
    {
        var target = commonDataByEvaluatorKey.Find(t => t.evaluatorKey == evaluatorKey);
        return target != null ? target.textKey : string.Empty;
    }

    public string GetEvaluatorKeyText(string evaluatorKey)
    {
        var type = ConvertStringToEvaluatorKeyType(evaluatorKey);
        if (type == MileageDataEvaluatorKey.None)
            return string.Empty;
        else
        {
            var target = commonDataByEvaluatorKey.Find(t => t.evaluatorKey == type);
            return target != null ? target.textKey : string.Empty;
        }
    }

    public List<MileageDataEvaluatorKey> GetDefaultEvaluatorKeys(MileageDataEvaluateTargetDataType dataType)
    {
        var result = new List<MileageDataEvaluatorKey>();

        var target = dataType_evaluatorKeyMapped.Find(t => t.dataType == dataType);
        if (target != null)
        {
            for (int i = 0; i < target.evaluatorKeyGroups.Count; i++)
            {
                result.Add(target.evaluatorKeyGroups[i].defaultKey);
            }
        }

        return result;
    }

    /// <summary>
    /// DataType 에 매칭되는, 현재 적용해야하는 Evaluator Key 를 가져옴 
    /// </summary>
    public List<MileageDataEvaluatorKey> GetCurSelectedEvaluatorKeys(MileageDataEvaluateTargetDataType dataType)
    {
        var list = new List<MileageDataEvaluatorKey>();

        foreach (var keyMapped in dataType_evaluatorKeyMapped)
        {
            if (keyMapped.dataType == dataType)
            {
                for (int i = 0; i < keyMapped.evaluatorKeyGroups.Count; i++)
                {
                    var curSelectedKeyOfGroup = keyMapped.evaluatorKeyGroups[i].curSelectedKey;

                    /// None 으로 선택된 옵션은 없는것 즉 무조건 통과로 간주하게끔 세팅
                    if (curSelectedKeyOfGroup != MileageDataEvaluatorKey.None)
                    {
                        list.Add(curSelectedKeyOfGroup);
                    }
                }

                break;
            }
        }

        return list;
    }

    public List<RequestEvaluateParam> GetCurrentDataRequestParam(MileageDataEvaluateTargetDataType dataType)
    {
        var result = new List<RequestEvaluateParam>();
        var evKeyList = GetCurSelectedEvaluatorKeys(dataType);

        for (int i = 0; i < evKeyList.Count; i++)
        {
            result.Add(new RequestEvaluateParam(evKeyList[i], MileageAndOrOperation.And));
        }

        return result;
    }

    public List<MileageBaseDataIdentifier> GetData(uint shopListGroupID, MileageDataEvaluateTargetDataType targetDataType, IEnumerable<RequestEvaluateParam> reqParam)
    {
        return DataWrapper.GetData(shopListGroupID, targetDataType, reqParam);
    }

    public void ExchangeItem(List<ItemBuyInfo> buyInfoList)
    {
        ZWebManager.Instance.WebGame.REQ_MileageShopBuy(
            buyInfoList
            , (revPacket, resList) =>
            {
                isExchangeUpdateUIDirty = true;
                UpdateUI();
                // TODO 
                //  OpenNotiUp("보상을 받았습니다");

                /// 아이템 획득 연출 출력
                UIManager.Instance.Open<UIFrameItemRewardShot>((str, frame) =>
                {
                    var itemsObtained = new List<GainInfo>();

                    for (int i = 0; i < resList.ResultGetItemsLength; i++)
                    {
                        var data = resList.ResultGetItems(i);

                        var target = itemsObtained.Find(t => t.ItemTid == data.Value.ItemTid);

                        if (target == null)
                        {
                            itemsObtained.Add(new GainInfo(resList.ResultGetItems(i).Value));
                        }
                        else
                        {
                            target.Cnt++;
                        }
                    }

                    frame.AddItem(itemsObtained);
                });

                CloseChangePetInfoPopUp();
                CloseItemInfoPopup();
            },
            (err, req, res) =>
            {
                CloseChangePetInfoPopUp();
            });
    }

    public void ExchangeItem_CurrentContextBuyInfo()
    {
        if (curContextBuyInfo == null)
        {
            ZLog.LogError(ZLogChannel.UI, "Current Context itembuyInfo List not set");
            return;
        }

        if (curContextBuyInfo.affordable == false)
        {
            /// TODO 
            OpenNotiUp("마일리지가 부족합니다", "알림");
            return;
        }

        /// TODO 
        OpenTwoButtonQueryPopUp("확인", "정말 교환하시겠습니까?"
            , onConfirmed: () =>
            {
                ExchangeItem(curContextBuyInfo.buyInfoList);
            });
    }

    #region Common
    //public List<ItemBuyInfo> BuildBuyInfo(ref List<ItemBuyInfo> list, MileageBaseDataIdentifier identifier)
    //{
    //    if (list == null)
    //        list = new List<ItemBuyInfo>();

    //    var curShopId = CategoryDescriptor.SelectedSubCategoryShopID;
    //    var curMileageShopData = DBMileageShop.GetDataByTID(curShopId);

    //    if (curMileageShopData == null)
    //    {
    //        ZLog.LogError(ZLogChannel.UI, "Could not find the target MileageShopData By Tid : " + curShopId);
    //        return new List<ItemBuyInfo>();
    //    }

    //    var info = new ItemBuyInfo(CategoryDescriptor.SelectedSubCategoryShopID, 1, Me.CurCharData.GetItem()
    //}

    public static string GetAttributeSpriteName(E_UnitAttributeType type)
    {
        switch (type)
        {
            case E_UnitAttributeType.Fire:
                {
                    return "icon_element_fire_s";
                }
            case E_UnitAttributeType.Water:
                {
                    return "icon_element_water_s";
                }
            case E_UnitAttributeType.Electric:
                {
                    return "icon_element_electric_s";
                }
            case E_UnitAttributeType.Light:
                {
                    return "icon_element_light_s";
                }
            case E_UnitAttributeType.Dark:
                {
                    return "icon_element_dark_s";
                }
            default:
                ZLog.LogError(ZLogChannel.UI, "No attribute type matched : " + type.ToString());
                break;
        }

        return "";
    }

    public static string GetCharacterTypeSprite(E_CharacterType type)
    {
        switch (type)
        {
            case E_CharacterType.Knight:
                return "icon_gladiator_01_m";
            case E_CharacterType.Archer:
                return "icon_archer_01_m";
            case E_CharacterType.Wizard:
                return "icon_magician_01_m";
            case E_CharacterType.Assassin:
                return "icon_assassin_01_m";
            case E_CharacterType.All:
                return "icon_class_all";
            default:
                break;
        }

        return "";
    }

    public static string GetAttributeName(E_UnitAttributeType type)
    {
        switch (type)
        {
            case E_UnitAttributeType.Fire:
                return "Attribute_Name_Fire";
            case E_UnitAttributeType.Water:
                return "Attribute_Name_Water";
            case E_UnitAttributeType.Electric:
                return "Attribute_Name_Electric";
            case E_UnitAttributeType.Light:
                return "Attribute_Name_Light";
            case E_UnitAttributeType.Dark:
                return "Attribute_Name_Dark";
            default:
                ZLog.LogError(ZLogChannel.UI, "no type matching : " + type.ToString());
                break;
        }

        return "";
    }

    static public float GetMyMileagePercentage(uint buyItemID)
    {
        var myCount = Me.GetCurrency(buyItemID);
        var requiredCount = DBMileageShop.GetBuyItemCount(buyItemID);
        return myCount / (float)requiredCount;
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

    static public Color GetColorByGrade(byte grade)
    {
        return ParseColor("#" + DBUIResouce.GetGradeTextColor(GameDB.E_UIType.Item, grade));
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
        base.OnInitialize();

        ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIMileageShopItemGroup), delegate
        {
            if (CheckValidateFatalData() == false)
            {
                UIManager.Instance.Close<UIFrameMileage>();
                return;
            }

            dataWrapper = new MileageDataWrapper();
            categoryDescriptor = new MileageCategoryDescriptor();
            subCategoryUIs = new List<UIMileageSubCategory>();
            evaluatorKeyStringPair = new List<EvaluatorKeyStringPair>();
            dataEvaluatorRegister = new MileageDataEvaluatorRegisterer();
            curContextBuyInfo = new BuyInfoContextInfo();

            dataType_evaluatorKeyMapped.ForEach(t => t.SetSelectedToDefault());

            dataWrapper.Initialize();
            dataEvaluatorRegister.Initialize(dataWrapper);
            ScrollAdapter.Initialize();
            ScrollAdapter.onClickedSlot = HandleScrollItemClicked;

            /// MainCategory Init
            InitCategory();
            InitBottomUI();
            InitPopUp();

            initDone = true;
        }, bActiveSelf: false);
    }

    protected override void OnShow(int _LayerOrder)
    {
        base.OnShow(_LayerOrder);
        UIManager.Instance.Find<UIFrameHUD>().SetSubHudFrame(E_UIStyle.FullScreen);

        if (showCo != null)
        {
            StopCoroutine(showCo);
            showCo = null;
        }

        if (initDone == false)
        {
            showCo = StartCoroutine(WaitUntilInitDone(() =>
            {
                showCo = null;
                ShowFirst();
            }));
        }
        else
        {
            ShowFirst();
        }
    }

    protected override void OnHide()
    {
        base.OnHide();
        CloseAllPopUps();
        change_petStatusPopUp.Release();
        UIManager.Instance.Find<UIFrameHUD>().SetSubHudFrame();
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// 프레임을 꺼야하는 상황 체크 
    /// </summary>
    private bool CheckValidateFatalData()
    {
        Dictionary<uint, uint> buyItemDuplicateCheck = new Dictionary<uint, uint>();
        bool result = true;

        DBMileageShop.ForeachAllData((tableData) =>
        {
            if (buyItemDuplicateCheck.ContainsKey(tableData.BuyItemID) == false)
            {
                buyItemDuplicateCheck.Add(tableData.BuyItemID, tableData.BuyItemCount);
            }
            /// 해당 buyItemID 이미 존재 . 이때부터 Duplicate 체크 가능함 
            else
            {
                if (buyItemDuplicateCheck[tableData.BuyItemID] != tableData.BuyItemCount)
                {
                    ZLog.LogError(ZLogChannel.UI, "Mileage Different BuyItemCount between duplicate buyItemID , MileageShopID : " + tableData.MileageShopID);
                    result = false;
                }
            }

            if (result == false)
                return;
        });

        return result;
    }

    private void SetPopUpBackBlockerTopMostAndActive()
    {
        popUpBlocker_graphic.gameObject.SetActive(true);
        popUpBlocker_graphic.transform.SetAsLastSibling();
    }

    private void SetEnableSubCategoryByIndex(int index)
    {
        subCategoryUIs[index].gameObject.SetActive(true);
    }

    private void SetEnableSubCategoryCount(int count, Action<UIMileageSubCategory> preCallback)
    {
        if (subCategoryUIs.Count < count)
        {
            ZLog.LogError(ZLogChannel.UI, "Requested Count To Enable SucCategory is bigger than it should be");
            subCategoryUIs.ForEach(t => t.gameObject.SetActive(false));
            return;
        }

        for (int i = 0; i < subCategoryUIs.Count; i++)
        {
            preCallback?.Invoke(subCategoryUIs[i]);
            subCategoryUIs[i].gameObject.SetActive(i < count);
        }
    }

    #region Init
    private void InitCategory()
    {
        #region Set Basis
        var evaluatorKeys = (MileageDataEvaluatorKey[])Enum.GetValues(typeof(MileageDataEvaluatorKey));

        for (int i = 0; i < evaluatorKeys.Length; i++)
        {
            evaluatorKeyStringPair.Add(new EvaluatorKeyStringPair() { str = evaluatorKeys[i].ToString(), key = evaluatorKeys[i] });
        }
        #endregion

        #region Descriptor Initialize
        List<E_MileageShopType> mainCategories = new List<E_MileageShopType>();

        for (int i = 0; i < mainCategoryUI.tabs.Count; i++)
        {
            mainCategories.Add(mainCategoryUI.tabs[i].type);
        }

        categoryDescriptor.Initialize(this, mainCategories);
        #endregion

        #region Assign Max SubCategory Count
        int maxCategoryCount = categoryDescriptor.MaxSubCategoryCount;

        if (subCategoryUIs.Count != maxCategoryCount)
        {
            /// 필요한 량보다 넘치면 삭제 
            if (subCategoryUIs.Count > maxCategoryCount)
            {
                int removeCnt = subCategoryUIs.Count - maxCategoryCount;
                for (int i = 0; i < removeCnt; i++)
                {
                    Destroy(subCategoryUIs[i + subCategoryUIs.Count].gameObject);
                }
                subCategoryUIs.RemoveRange(subCategoryUIs.Count, removeCnt);
            }
            /// 부족하면 필요한 만큼 할당 
            else
            {
                int addCnt = maxCategoryCount - subCategoryUIs.Count;
                for (int i = 0; i < addCnt; i++)
                {
                    var instance = GenerateSubCategory();
                    subCategoryToggleGroup.RegisterToggle(instance.Toggle);
                    subCategoryUIs.Add(instance);
                }
            }
        }

        for (int i = 0; i < subCategoryUIs.Count; i++)
        {
            subCategoryUIs[i].Initialize(this.subCategoryToggleGroup);
        }
        #endregion

        #region Set Default
        CategoryDescriptor.SetDefault();

        /// callBack 은 보내지않음 
        subCategoryToggleGroup.SetAllTogglesOff(false);

        SetEnableSubCategoryCount(categoryDescriptor.SelectedSubCategoryCount,
            (ui) =>
        {

        });

        if (subCategoryUIs.Count > 0)
        {
            subCategoryUIs[0].SetToggle(true);
        }
        #endregion

        //#region Set Data 
        //#endregion

        #region Set MainCategory
        var firstDisplayableMainCtg = CategoryDescriptor.GetFirstVisibleCategory();

        /// Displayable 상태따라 메인 카테고리 Active,Deactive 
        foreach (var mainCtg in mainCategoryUI.tabs)
        {
            var isDisplayable = CategoryDescriptor.IsMainCategoryDisplayable(mainCtg.type);
            mainCtg.isDisplayable = isDisplayable;

            mainCtg.toggleObject.gameObject.SetActive(isDisplayable);

            if (firstDisplayableMainCtg != null
                && firstDisplayableMainCtg.type == mainCtg.type)
            {
                mainCtg.toggleObject.isOn = true;
            }
            else
            {
                mainCtg.toggleObject.isOn = false;
            }
        }
        #endregion
    }

    private void InitBottomUI()
    {
        /// Change 
        uiGroup_bottom.group_change.characterElemental_evaluator.Initialize(this);
        uiGroup_bottom.group_change.characterType_evaluator.Initialize(this);
        uiGroup_bottom.group_change.characterElemental_evaluator.SetOnClicked(HandleEvaluatorToggleOptionClicked);
        uiGroup_bottom.group_change.characterType_evaluator.SetOnClicked(HandleEvaluatorToggleOptionClicked);

        /// Item
        uiGroup_bottom.group_item.equipmentParts_evaluator.Initialize(this);
        uiGroup_bottom.group_item.equipmentCharacterType_evaluator.Initialize(this);
        uiGroup_bottom.group_item.equipmentParts_evaluator.SetOnClicked(HandleEvaluatorToggleOptionClicked);
        uiGroup_bottom.group_item.equipmentCharacterType_evaluator.SetOnClicked(HandleEvaluatorToggleOptionClicked);
    }

    private void InitPopUp()
    {

    }

    #endregion

    private void ShowFirst()
    {
        /// 처음 보여줄때의 Data 를 직접 세팅 후 
        categoryDescriptor.SetDefault();

        var firstMainCtg = mainCategoryUI.tabs.Find(t => t.isDisplayable);

        if (firstMainCtg != null)
        {
            firstMainCtg.toggleObject.SelectToggle();
        }

        updateForced = true;
        curContextBuyInfo.Reset();

        UpdateUI();
    }

    private UIMileageSubCategory GenerateSubCategory()
    {
        var instance = Instantiate(subCategorySourceObj, subCategoryToggleParent);
        instance.gameObject.SetActive(false);
        return instance;
    }

	/// <summary>
	/// 이따구로 짜고싶지않지만 기획에서 수정하는 바람에 하씨진짜
	/// </summary>
	private uint GetCurrentListGroupTid(uint goodsTid)
	{
        return DBShopList.FindListGroupListTIDByShopListIDAndGoods(categoryDescriptor.SelectedSubCategoryShopListID, goodsTid);
    }

	#region Set
	/// <summary>
	///  DataType 의 특정 EvaluatorKeyGroup 을 검색해서 세팅해줌 
	/// </summary>
	private void SelectEvaluatorKey(MileageDataEvaluateTargetDataType dataType, MileageDataEvaluatorKey evaluatorKey)
    {
        var target = dataType_evaluatorKeyMapped.Find(t => t.dataType == dataType);

        if (target == null)
        {
            return;
        }

        if (target.HasTargetEvaluatorKey(evaluatorKey))
        {
            target.SelectTargetEvaluatorKey(evaluatorKey);
        }
    }

    #endregion

    #region Update 
    #region ----- UpdateData ------

    private void UpdateCurrentBuyInfoByCurSelectedInfo(uint targetGoodsItemID, uint listGroupTid)
    {
        uint targetGoodsShopListID = DBShopList.GetShopListIDByGoodsID(targetGoodsItemID);
        curContextBuyInfo.Reset();
        curContextBuyInfo.Set(CategoryDescriptor.SelectedSubCategoryShopID, targetGoodsShopListID, listGroupTid);
    }

    //private void UpdateData()
    //{
    //    scheduledUpdate_data = false;
    //}

    //private void UpdateData_MainCategoryVisible()
    //{

    //}

    //private void UpdateData_SubCategoryVisible()
    //{

    //}
    //  #endregion

    #region ----- UpdateUI ------
    private void UpdateUI()
    {
        //scheduledUpdate_ui = false;
        UpdateUI_MainCategory();
        UpdateUI_SubCategory();
        UpdateUI_Bottom();
        UpdateUI_SubCategoryScrollItems();

        lastUpdatedMainCtgType = CategoryDescriptor.SelectedMainCategoryType;
        lastUpdatedSubCtgShopID = CategoryDescriptor.SelectedSubCategoryShopID;
        isExchangeUpdateUIDirty = false;
        updateForced = false;
    }

    private void UpdateUI_MainCategory()
    {
        /// 선택된 메인 카테고리가 변한 경우 
        if (lastUpdatedMainCtgType != CategoryDescriptor.SelectedMainCategoryType)
        {

        }
    }

    private void UpdateUI_SubCategory()
    {
        /// TODO : 수정해야함 . 
        /// 가장 첫번째 토글만 on 시키면 알아서 되게끔 해야함 . 코드 모양세도 그렇고 

        if (updateForced || lastUpdatedMainCtgType != CategoryDescriptor.SelectedMainCategoryType)
        {
            subCategoryScrollRect.verticalNormalizedPosition = 1f;

            int subCtgEnableCnt = 0;

            CategoryDescriptor.ForeachDisplayableSubCategories((subCtgInfo) =>
            {
                var ui = subCategoryUIs[subCtgEnableCnt];

                ui.Set(subCtgInfo.CtgData.ShopID, DBLocale.GetText(subCtgInfo.CtgData.TextID));
                ui.SetToggle(CategoryDescriptor.SelectedSubCategory.CtgData.ShopID == subCtgInfo.CtgData.ShopID);
                ui.gameObject.SetActive(true);

                //  if (subCtgEnableCnt == 0)
                //  {
                //    ui.SetToggle(true);
                //    }

                if (subCtgEnableCnt == 0)
                {
                    //ui.Toggle.ZToggleGroup.SelectToggleIndex(subCtgEnableCnt);
                    //                    ui.Toggle.ZToggleGroup.SelectToggleIndex(0);
                }

                //ui.Toggle.GraphicUpdateComplete();

                subCtgEnableCnt++;
            });

            /// 나머지는 꺼줌 
            for (int i = subCtgEnableCnt; i < subCategoryUIs.Count; i++)
            {
                subCategoryUIs[i].gameObject.SetActive(false);
            }

            if (subCategoryUIs.Count > 0
                && subCategoryUIs[0].gameObject.activeSelf)
            {
                subCategoryUIs[0].Toggle.SelectToggle();
            }
        }
    }

    private void UpdateUI_Bottom()
    {
        if (updateForced || isExchangeUpdateUIDirty || lastUpdatedSubCtgShopID != CategoryDescriptor.SelectedSubCategoryShopID)
        {
            UIGroup_Bottom.MyMileage myMileage = null;

            switch (CategoryDescriptor.SelectedDataType)
            {
                case MileageDataEvaluateTargetDataType.Item:
                    {
                        myMileage = uiGroup_bottom.group_item.myMileage;
                    }
                    break;
                case MileageDataEvaluateTargetDataType.Change:
                    {
                        myMileage = uiGroup_bottom.group_change.myMileage;
                    }
                    break;
                case MileageDataEvaluateTargetDataType.Pet:
                    {
                        myMileage = uiGroup_bottom.group_pet.myMileage;
                    }
                    break;
                case MileageDataEvaluateTargetDataType.Rune:
                    {
                        myMileage = uiGroup_bottom.group_rune.myMileage;
                    }
                    break;
                default:
                    break;
            }

            if (myMileage != null)
            {
                var mileageDataTid = CategoryDescriptor.SelectedSubCategoryShopID;
                var mileageData = DBMileageShop.GetDataByTID(mileageDataTid);
                string myMileageKindTxtKey = DBItem.GetItem(mileageData.BuyItemID).ItemTextID;
                string myMileageText = DBLocale.GetText(myMileageKindTxtKey);
                uint myMileageItemID = mileageData.BuyItemID;
                uint costCount = mileageData.BuyItemCount;

                myMileage.imgIcon.sprite = ZManagerUIPreset.Instance.GetSprite(DBItem.GetItemIconName(myMileageItemID));
                myMileage.txtTitle.text = myMileageText;
                ulong myCurrency = ZNet.Data.Me.GetCurrency(myMileageItemID);
                myMileage.txtPercentage.text = (int)(((myCurrency / costCount) * 100f)) + "%";
                myMileage.affordCount.text = ((int)(myCurrency / costCount)).ToString();

                /// Item 인 경우에는 Equip && Accessory == false 일때 추가로 필터링 옵션 UI 출력 
                if (CategoryDescriptor.SelectedDataType == MileageDataEvaluateTargetDataType.Item)
                {
                    bool isEquipment = DBShopList.IsEquipmentItem(mileageData.ShopListID) && DBShopList.IsAccessoryItemByShopListID(mileageData.ShopListID) == false;
                    uiGroup_bottom.group_item.equipmentRoot.SetActive(isEquipment);

                    /// 201028 기획 컨펌사항으로 장비일때 파츠 옵션은 사용안하기로함 . 고로 꺼줌 . 하지만 지우진 않음 . 추후 대비 
                    uiGroup_bottom.group_item.equipmentParts_evaluator.gameObject.SetActive(false);

                    /// 마석일때는 꺼줘야함 
                    bool isGem = DBShopList.IsEquipGemItem(mileageData.ShopListID);
                    uiGroup_bottom.group_item.equipmentCharacterType_evaluator.gameObject.SetActive(isGem == false);
                }
                else
                {
                    uiGroup_bottom.group_item.equipmentRoot.SetActive(false);
                }
            }

            /// Active, Deactive
            uiGroup_bottom.activeByDataType.ForEach(t => t.gameobject.SetActive(t.dataType == CategoryDescriptor.SelectedDataType));
        }
    }

    private void UpdateUI_SubCategoryScrollItems()
    {
        if (updateForced || isScrollDataDirtyForUIUpdate || lastUpdatedSubCtgShopID != CategoryDescriptor.SelectedSubCategoryShopID)
        {
            isScrollDataDirtyForUIUpdate = false;
            ScrollAdapter.SetNormalizedPosition(0f);
            ScrollAdapter.RefreshData(CategoryDescriptor.CurrentDisplayData);
        }
    }

    #endregion
    #endregion

    #region Popup Related
    private void OpenItemInfoPopUp(Item_Table tableData)
    {
        if (tableData == null)
        {
            return;
        }

        if (itemInfoPopUp != null)
        {
            CloseChangePetInfoPopUp();

            UpdateCurrentBuyInfoByCurSelectedInfo(tableData.ItemID, GetCurrentListGroupTid(tableData.ItemID));
            SetPopUpBackBlockerTopMostAndActive();
            // popUpBlocker_graphic.gameObject.SetActive(false);
            itemInfoPopUp.gameObject.SetActive(true);
            itemInfoPopUp.transform.SetAsLastSibling();
            itemInfoPopUp.Initialize(E_ItemPopupType.MileageShop, tableData.ItemID);
        }
        else
        {
            ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIPopupItemInfo), (_obj) =>
            {
                UIPopupItemInfo obj = _obj.GetComponent<UIPopupItemInfo>();

                if (obj != null)
                {
					UpdateCurrentBuyInfoByCurSelectedInfo(tableData.ItemID, GetCurrentListGroupTid(tableData.ItemID));
					//popUpBlocker_graphic.gameObject.SetActive(false);
					SetPopUpBackBlockerTopMostAndActive();
                    itemInfoPopUp = obj;
                    obj.transform.SetParent(popUpObjRoot);
                    obj.transform.SetAsLastSibling();
                    obj.Initialize(E_ItemPopupType.MileageShop, tableData.ItemID);
                }
            });
        }
    }

    private void OpenChangePetInfoPopUp(MileageBaseDataIdentifier identifier)
    {
        if ((change_petStatusPopUp.gameObject.activeSelf
            && change_petStatusPopUp.CurTID == identifier.tid)
            || change_petStatusPopUp.IsLoading == true)
            return;

		UpdateCurrentBuyInfoByCurSelectedInfo(identifier.tid, GetCurrentListGroupTid(identifier.tid));
        SetPopUpBackBlockerTopMostAndActive();
        // this.popUpBlocker_graphic.gameObject.SetActive(false);
        change_petStatusPopUp.gameObject.SetActive(true);
        change_petStatusPopUp.Open(identifier, onCloseReqCallback: () => CloseChangePetInfoPopUp(), ExchangeItem_CurrentContextBuyInfo);
        change_petStatusPopUp.transform.SetAsLastSibling();
    }

    private void CloseChangePetInfoPopUp()
    {
        if (change_petStatusPopUp != null && change_petStatusPopUp.gameObject.activeSelf)
        {
            curContextBuyInfo.Reset();
            change_petStatusPopUp.gameObject.SetActive(false);
            change_petStatusPopUp.OnClose();
            popUpBlocker_graphic.gameObject.SetActive(false);
        }
    }

    public void CloseItemInfoPopup()
    {
        if (itemInfoPopUp != null && itemInfoPopUp.gameObject.activeSelf)
        {
            curContextBuyInfo.Reset();
            itemInfoPopUp.gameObject.SetActive(false);
            popUpBlocker_graphic.gameObject.SetActive(false);
        }
    }

    public void CloseMyMileagePopUp()
    {
        if (myMileageStatusPopUp == null ||
            myMileageStatusPopUp.gameObject.activeSelf == false)
            return;

        myMileageStatusPopUp.gameObject.SetActive(false);
        popUpBlocker_graphic.gameObject.SetActive(false);
    }

    #endregion

    #region ETC
    /// <summary>
    /// 필터링 옵션 클릭 핸들링 함수 
    /// </summary>
    private void HandleEvaluatorToggleOptionClicked(UIMileageDataEvaluator evaluator, UIMileageDataEvaluator.Option option)
    {
        CloseChangePetInfoPopUp();

        //   uiGroup_bottom.CloseToggle();
        evaluator.SetTitleTxt(DBLocale.GetText(GetEvaluatorKeyText(option.evaluatorKey)));

        SelectEvaluatorKey(evaluator.TargetDataType, ConvertStringToEvaluatorKeyType(option.evaluatorKey));

        isScrollDataDirtyForUIUpdate = CategoryDescriptor.UpdateSelectedSubCategoryData(GetCurrentDataRequestParam(CategoryDescriptor.SelectedDataType));

        UpdateUI();
    }

    /// <summary>
    /// 아이템 클릭 핸들링 함수 
    /// </summary>
    private void HandleScrollItemClicked(MileageBaseDataIdentifier info)
    {
        //CloseChangePetInfoPopUp();
        CloseMyMileagePopUp();
        CloseItemInfoPopup();

        switch (info.dataType)
        {
            case MileageDataEvaluateTargetDataType.None:
                ZLog.LogError(ZLogChannel.UI, "none data type");
                break;
            case MileageDataEvaluateTargetDataType.Item:
                {
                    OpenItemInfoPopUp(DBItem.GetItem(info.tid));
                }
                break;
            case MileageDataEvaluateTargetDataType.Change:
                {
                    OpenChangePetInfoPopUp(info);
                }
                break;
            case MileageDataEvaluateTargetDataType.Pet:
                {
                    OpenChangePetInfoPopUp(info);
                }
                break;
            case MileageDataEvaluateTargetDataType.Rune:
                {
                    ZLog.LogError(ZLogChannel.UI, "no implemented");
                }
                break;
            default:
                ZLog.LogError(ZLogChannel.UI, "no matching type entered. add type");
                break;
        }
    }

    IEnumerator WaitUntilInitDone(Action onFinished)
    {
        yield return null;
        while (initDone == false)
            yield return null;
        onFinished();
    }
    #endregion
    #endregion

    #region Inspector Events 
    public void OnMainCategoryToggleChanged(ZToggle toggle)
    {
        if (initDone == false)
            return;

        var ui = mainCategoryUI.tabs.Find(t => t.toggleObject == toggle);

        if (ui != null && ui.type != lastUpdatedMainCtgType && ui.isDisplayable)
        {
            categoryDescriptor.SelectMainCategory(ui.type);
            categoryDescriptor.SelectFirstSubCateogry();
            UpdateUI();
        }
    }

    public void OnSubCategoryToggleChanged(UIMileageSubCategory subCategory)
    {
        if (initDone == false)
            return;

        if (subCategory.Toggle.isOn)
        {
            categoryDescriptor.SelectSubCategory(subCategory.MileageShopID);
            CategoryDescriptor.UpdateSelectedSubCategoryData(GetCurrentDataRequestParam(CategoryDescriptor.SelectedDataType));
            UpdateUI();
        }
    }

    public void OnOnlyObtainedChangeOptionToggleChanged(Toggle toggle)
    {
        SelectEvaluatorKey(MileageDataEvaluateTargetDataType.Change, toggle.isOn ? MileageDataEvaluatorKey.Character_NotObtained : MileageDataEvaluatorKey.Character_ObtainedOrNotObtained);
        isScrollDataDirtyForUIUpdate = CategoryDescriptor.UpdateSelectedSubCategoryData(GetCurrentDataRequestParam(CategoryDescriptor.SelectedDataType));
        UpdateUI();
    }

    #region OnClick
    public void OnClickClose()
    {
        UIManager.Instance.Close<UIFrameMileage>();
    }

    public void OnClickViewMyMileageBtn()
    {
        CloseAllPopUps();

        SetPopUpBackBlockerTopMostAndActive();
        myMileageStatusPopUp.transform.SetAsLastSibling();
        myMileageStatusPopUp.Open(onCloseCallback: () => CloseMyMileagePopUp());
        myMileageStatusPopUp.gameObject.SetActive(true);
    }
    #endregion
    #endregion

    #region Define
    abstract public class MileageCategoryDisplayable
    {
        abstract public bool IsDisplayable();
    }

    /// <summary>
    /// UI
    /// </summary>
    [Serializable]
    public class MainCategoryTab
    {
        [Serializable]
        public class SingleTab
        {
            public E_MileageShopType type;
            public ZToggle toggleObject;

            [HideInInspector] public bool isDisplayable;
        }

        public ZToggleGroup toggleGroup;
        public List<SingleTab> tabs;
    }

    [Serializable]
    public class DataTypeEvaluatorKeyMapped
    {
        [Serializable]
        public class EvaluatorKeyGroup
        {
            public List<MileageDataEvaluatorKey> keys;
            public MileageDataEvaluatorKey defaultKey;

            [HideInInspector] public MileageDataEvaluatorKey curSelectedKey;
        }

        public MileageDataEvaluateTargetDataType dataType;
        public List<EvaluatorKeyGroup> evaluatorKeyGroups;

        /// <summary>
        /// 현재 그룹이 Target Key 를 가지고 있는지 체킹 
        /// </summary>
        public bool HasTargetEvaluatorKey(MileageDataEvaluatorKey key)
        {
            for (int i = 0; i < evaluatorKeyGroups.Count; i++)
            {
                if (evaluatorKeyGroups[i].keys.Exists(t => t == key))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 현재 그룹이 Target Key 를 가지고 있다면은 Select 해줌 
        /// </summary>
        public void SelectTargetEvaluatorKey(MileageDataEvaluatorKey key)
        {
            for (int i = 0; i < evaluatorKeyGroups.Count; i++)
            {
                if (evaluatorKeyGroups[i].keys.Exists(t => t == key))
                {
                    evaluatorKeyGroups[i].curSelectedKey = key;
                    return;
                }
            }

            ZLog.LogError(ZLogChannel.UI, "Target Evaluator Key not found in any evaluatorKeyGroup , key : " + key.ToString());
        }

        public void SetSelectedToDefault()
        {
            for (int i = 0; i < evaluatorKeyGroups.Count; i++)
            {
                evaluatorKeyGroups[i].curSelectedKey = evaluatorKeyGroups[i].defaultKey;
            }
        }
    }

    [Serializable]
    public class EvaluatorKeyCommonData
    {
        public MileageDataEvaluatorKey evaluatorKey;
        public string textKey;
    }

    public class EvaluatorKeyStringPair
    {
        public string str;
        public MileageDataEvaluatorKey key;
    }

    [Serializable]
    public class GameObjectDataTypePair
    {
        public GameObject gameobject;
        public MileageDataEvaluateTargetDataType dataType;
    }

    [Serializable]
    public class UIGroup_Bottom
    {
        //public interface IToggleCloser
        //{
        //    void CloseToggle();
        //}

        [Serializable]
        public class MyMileage
        {
            public Image imgIcon;
            public Text txtTitle;
            public Text txtPercentage;
            public Text affordCount;
        }

        [Serializable]
        public class Change
        {
            public UIMileageDataEvaluator characterType_evaluator;
            public UIMileageDataEvaluator characterElemental_evaluator;
            public ZToggle toggle_onlyDisplayNotObtainedChange;
            public MyMileage myMileage;
        }

        [Serializable]
        public class Item
        {
            public MyMileage myMileage;

            /// <summary>
            ///  Equipment
            /// </summary>
            public GameObject equipmentRoot;
            public UIMileageDataEvaluator equipmentParts_evaluator;
            public UIMileageDataEvaluator equipmentCharacterType_evaluator;
        }

        [Serializable]
        public class Pet
        {
            public MyMileage myMileage;
        }

        [Serializable]
        public class Rune
        {
            public MyMileage myMileage;
        }

        public List<GameObjectDataTypePair> activeByDataType;
        public Change group_change;
        public Item group_item;
        public Pet group_pet;
        public Rune group_rune;
    }

    class BuyInfoContextInfo
    {
        public bool affordable;
        public List<ItemBuyInfo> buyInfoList = new List<ItemBuyInfo>();

        public void Reset()
        {
            affordable = false;
            buyInfoList.Clear();
        }

        /// <summary>
        /// targetItemShopList 는 출력된 아이템의 ShopListTable 에서의 ShopListID 임 , 마일리지에서의 ShopListID 가 아니라 ㅡ.ㅡ
        /// </summary>
        public void Set(uint mileageShopTid, uint targetItemShopListID, uint listGroupTid)
        //, uint buyCnt
        //, ulong useItemId
        //, uint useItemTid
        //, uint selectItemTid)
        {
            buyInfoList.Clear();

            var mileageShopData = DBMileageShop.GetDataByTID(mileageShopTid);
            var currencyItemData = Me.CurCharData.GetItem(mileageShopData.BuyItemID);

            affordable = Me.GetCurrency(mileageShopData.BuyItemID) >= mileageShopData.BuyItemCount;

            buyInfoList.Add(new ItemBuyInfo(
                mileageShopTid
                , 1
                , currencyItemData != null ? currencyItemData.item_id : 0
                , mileageShopData.BuyItemID
                , targetItemShopListID
                , listGroupTid));
        }
    }
    #endregion
}
#endregion