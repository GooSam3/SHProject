using GameDB;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using WebNet;
using ZNet;
using frame8.Logic.Misc.Other.Extensions;
using static SpecialShopCategoryDescriptor;
using ZDefine;
using ZNet.Data;
using NTCore;
using static ZNet.ZWebRequestClientBase;

//public enum IAPAvailabilityStatus
//{
//    None = 0,
//    Disable, // 사용안함 
//    Available, // 사용함 및 사용 가능 
//    Negative_NotConnected, // Connect 가 안되어 사용못함 
//    Negative_Restoring // 현재 복구중인 상품이 있어 사용못함 
//}

/// <summary>
/// IAP 의 Status 를 정의하는 Enum 타입 
/// 타입마다 필요하다면 비동기로 Update() 에서 실시간 체킹후 처리함 .
/// </summary>
public enum IAPStatusByFlow
{
    None = 0, 
    Disable, /// 기능을 Disable 시킨 상태 
    RequestStartSetup, /// IAP 세팅 시작해야하는 상태 

    Connecting, /// 스토어 연결중인 상태 
    ConnectDoneSuccess, /// 스토어 연결 성공한 상태 
    ConnectDoneFail, /// 스토어 연결 실패한 상태 

    RequestRefreshPurchaseInfo, /// 상품 정보 업데이트 요청해야하는 상태 
    RefreshingPurchaseInfo, /// 상품 정보 업데이트중인 상태 
    RefreshPurchaseInfoDone, /// 상품 정보 업데이트 끝난 상태 

    RequestCheckRestore, /// 복구 요청해야하는 상태 
    RestoringProducts, /// 상품 복구중인 상태 

    Available /// 구매가능 (IAP 연결됐고 복구할 상품 없음)
}

public class ProcessPaymentPurchaseParam
{
    public string billingOrderID;
    public string productID;
    public PurchaseProductInfo infoOnStore;
    public string oriJson;
    public System.Action<ZWebRecvPacket, ResPaymentPurchase> onSuccess;
    public WebRequestErrorDelegate onError;

    public ProcessPaymentPurchaseParam(string billingOrderID, string productID, PurchaseProductInfo infoOnStore, string oriJson, Action<ZWebRecvPacket, ResPaymentPurchase> onSuccess, WebRequestErrorDelegate onError)
    {
        this.billingOrderID = billingOrderID;
        this.productID = productID;
        this.infoOnStore = infoOnStore;
        this.oriJson = oriJson;
        this.onSuccess = onSuccess;
        this.onError = onError;
    }
}

public class UIFrameSpecialShop : ZUIFrameBase
{
    public enum StartUpLoadingFailReason
    {
        Reason01
    }

    public enum Mode
    {
        None = 0,
        Category,
        Archive
    }

    #region SerializedField
    #region Preference Variable
    #endregion

    #region UI Variables
    [SerializeField] private GameObject categoryRoot;
    [SerializeField] private GameObject archiveRoot;

    [SerializeField] private ZToggleGroup mainCtgToggleGroup;
    [SerializeField] private RectTransform mainCtgParent;
    [SerializeField] private UISpecialShopMainCtg mainCtgToggleSourceObj;
    [SerializeField] private ScrollRect mainCtgScrollRect;

    [SerializeField] private ZToggleGroup subCtgToggleGroup;
    [SerializeField] private RectTransform subCtgParent;
    [SerializeField] private UISpecialShopSubCtg subCtgToggleSourceObj;

    [SerializeField] private SpecialShopItemGridAdapter ScrollAdapter_Category;
    [SerializeField] private SpecialShopArchiveListAdapter ScrollAdapter_Archive;
    /// <summary> 스페셜 상점 아이템 리스트 아답터. 튜토리얼에서 사용 </summary>
    public SpecialShopItemGridAdapter ItemScrollAdapter { get { return ScrollAdapter_Category; } }

    [SerializeField] private ZToggle toggleFilterCharacterType;
    [SerializeField] private Text txtCharacterFilter_normal;
    [SerializeField] private Text txtCharacterFilter_on;
    //[SerializeField] private ZToggleGroup toggleGroup_characterTypeFilters;

    [SerializeField] private ZButton btnArchive;

    [SerializeField] private RectTransform overlayRoot;
    #endregion
    #endregion

    public override bool IsBackable => true;

	#region Public Fields
	#endregion

	#region Private Fields
	private List<UISpecialShopMainCtg> mainCtgUIs;

    /// <summary> 스페셜 상점 메인카테고리 탭 (튜토리얼에서 훔침) </summary>
    public List<UISpecialShopMainCtg> MainCtgUIs { get { return mainCtgUIs; } }
    private List<UISpecialShopSubCtg> subCtgUIs;

    #region System Variables
    private SpecialShopCategoryDescriptor CategoryDescriptor;

    private Mode curMode;

    private E_SpecialShopType lastUpdatedMainCtgType;
    private E_SpecialSubTapType lastUpdatedSubCtgType;
    private bool isFilterDirty;
    private bool forceUpdate;

    private List<PurchaseProductInfo> storeProducts;

    // private bool isIAPReady;
    /// <summary>
    /// IAP Setup 처리를 비동기로하기에 현재 상태를 보관하는 변수 
    /// </summary>
    private IAPStatusByFlow iapSetupStatus;
    private bool isIAPProductsRefreshProcessDone;

    private List<ProcessPaymentPurchaseParam> paymentPurchasePendingList;
    private bool isProcessingPurchasePayment;

    /// <summary>
    /// 복구 + 구매때도 CashMail 을 Refresh 해야하면서 스페셜 상점 처음 들어갔을때도 Refresh 해야하기때문에 
    /// Update() 에서 비동기로 처리하기 위해 플래그 추가.
    /// </summary>
    static public bool willRefreshCashMail;
    private bool isRefreshingCashMail;

    private bool isArchiveListDirty;

    /// <summary>
    /// CurFilter
    /// </summary>
    private FilterOptions filters;

    public UISpecialShopItemInfoPopUp infoPopUp { get; private set; }

    private bool resetCategoryInfoOnEnter;
    private bool didSetCategoryEntering;

    private Coroutine showCo;
    private bool scriptInitDone;
    private bool isSetupDone;

    private Action onPostInit = delegate { };
    #endregion
    #endregion

    #region Properties 
    #endregion

    #region Unity Methods
    private void Update()
    {
        ProcessIAPSetting();

        /// PurchasePayment 즉 구매(복원) 의 마지막 단계는 비동기로 하나씩 처리 
        if (paymentPurchasePendingList != null && paymentPurchasePendingList.Count > 0)
        {
            if (isProcessingPurchasePayment == false)
            {
                isProcessingPurchasePayment = true;

                var param = paymentPurchasePendingList[0];
                paymentPurchasePendingList.RemoveAt(0);
                AdvanceBillingPaymentPurchase(param);
            }
        }

        /// 캐쉬 메일 새로고침도 비동기로함. 구매완료마다 실행해야하기 때문. 
        if (isRefreshingCashMail == false && willRefreshCashMail)
        {
            willRefreshCashMail = false;
            isRefreshingCashMail = true;

            UIManager.Instance.ShowGlobalIndicator(true);

            /// 보관함 데이터 갱신 
            ZWebManager.Instance.WebGame.REQ_GetCashMailList((revPacket_, resList_) =>
            {
                UIManager.Instance.ShowGlobalIndicator(false);
                isArchiveListDirty = true;
                isRefreshingCashMail = false;
            },
            _onError: (err, req, res) =>
            {
                UIManager.Instance.ShowGlobalIndicator(false);
                isRefreshingCashMail = false;
            });
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// 파라미터로 받은 아이템을 필터링 적용하여 체킹함 
    /// </summary>
    public bool ValidateDataByFilter(E_SpecialShopType shopType, SingleDataInfo data)
    {
        /// CharacterType Checking 
        if (shopType == E_SpecialShopType.Essence && this.filters.characterType != E_CharacterType.All)
        {
            var tableData = DBItem.GetItem(data.tidForTargetGoods);

            if (tableData != null && tableData.UseCharacterType == filters.characterType)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 해당 아이템으로 한번에 이동하기 위한 함수
    /// </summary>
    public bool ShortCut(uint targetSpecialShopTid, bool openItemInfo)
    {
        if (targetSpecialShopTid == 0)
        {
            return false;
        }

        var data = DBSpecialShop.Get(targetSpecialShopTid);

        if (data == null)
        {
            return false;
        }

        MoveTab(data.SpecialShopType);

        if (openItemInfo)
        {
            OpenItemInfoPopUp(targetSpecialShopTid);
        }

        return true;
    }

    // ljh : 초기화 후 실행될 이벤트 설정
    public void SetPostInitAction(Action postAction)
    {
        onPostInit = postAction;
    }

    #region Common
    /// <summary>
    /// OnPurchasePaymentComplete() 까지 처리된 시점에서 (실제 현금으로 결제된 시점)
    /// 마지막으로 서버에 보낸후 Consume 처리 및 기타 처리해주는 공통 로직
    /// </summary>
    public void AdvanceBillingPaymentPurchase(ProcessPaymentPurchaseParam param)
    {
        UIManager.Instance.ShowGlobalIndicator(true);

        ZWebManager.Instance.Billing.REQ_PaymentPurchase(param.infoOnStore, param.oriJson, NTIcarusManager.Instance.AdjustGpsADID
            , (purchase_revPacket, purchase_resList) =>
            {
                NTIcarusManager.Instance.ConsumePurchase(param.billingOrderID, param.productID);
                NTIcarusManager.Instance.AdjustPurchaseEvent(NTIcarusManager.TOKEN_09_PU, param.billingOrderID, param.productID, param.infoOnStore.currenyCode, param.infoOnStore.price.ToDouble);

                UIManager.Instance.ShowGlobalIndicator(false);

                /// 첫 구매 이벤트 트래킹 
                ///    if (purchase_resList.IsFirstPurchase)
                ///  {
                //  NTIcarusManager.Instance.AdjustPurchaseEvent(NTIcarusManager.TOKEN_08_IAP, billingOrderID, productID, infoOnStore.currenyCode, infoOnStore.price.ToDouble);
                ///    }

                /// 구매로 아이템을 얻게된 경우 
                if (iapSetupStatus == IAPStatusByFlow.Available)
                {
                    OpenNotiUp(string.Format("{0}을 획득하였습니다. 보관함을 확인하십시오.", param.infoOnStore.title), "확인");
                }
                /// 복구로 아이템을 얻게된 경우 
				else if (iapSetupStatus == IAPStatusByFlow.RestoringProducts)
                {
                    /// 다시한번 체킹후 복구할게 남아있지 않다면 구매 가능상태 즉 Available 세팅. 
                    if (PurchaseAPI.IsExistUnconsumedReceipt() == false)
                    {
                        SetIAPSetupStatus(IAPStatusByFlow.Available);
                    }
                }

                param.onSuccess?.Invoke(purchase_revPacket, purchase_resList);

                willRefreshCashMail = true;
                isProcessingPurchasePayment = false;
            }
            , _onNetError: (err, req, res) =>
            {
                ZLog.LogError(ZLogChannel.UI, "**** IAP Fail 01 **** ");

                UIManager.Instance.ShowGlobalIndicator(false);

                /// CAUTION : 에러처리는 바뀔수있음 . 
                if (err == ZWebRequestClientBase.NetErrorType.WebRequest)
                {
                    OpenErrorPopUp(ERROR.BILLING_PAYMENT_FAIL);
                }
                else if (err == ZWebRequestClientBase.NetErrorType.Packet)
                {
                    OpenErrorPopUp(res.ErrCode);
                }

                param.onError?.Invoke(err, req, res);
            });
    }

    /// <summary>
    /// 아이템 정보 팝업이 켜져있다면 IAP 상태 업데이트 
    /// </summary>
	private void NotifyIAPStatusChangedToInfoPopUp()
	{
		if (infoPopUp != null)
		{
			if (infoPopUp.gameObject.activeInHierarchy && infoPopUp.gameObject.activeSelf)
			{
				infoPopUp.ChangeIAPStatus(iapSetupStatus);
			}
		}
	}

	public string GetMainCategoryTextKey(E_SpecialShopType type)
    {
        return string.Concat("ShopType_", type.ToString());
    }
    public string GetSubCategoryTextKey(E_SpecialSubTapType type)
    {
        return string.Concat("ShopType_", type.ToString());
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

    static public void OpenNotiUp(string content, string title = "확인", Action onConfirmed = null)
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

    static public void OpenErrorPopUp(ERROR errorCode, Action onConfirmed = null)
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

        if (CheckValidateFatalData() == false)
        {
            ZLog.LogError(ZLogChannel.UI, "Terminate SpecialShop Fatal Data Issue Found");
            UIManager.Instance.Close<UIFrameSpecialShop>();
            return;
        }

        ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(SpecialShopItemSlot), delegate
        {
            ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(SpecialShopArchiveItemSlot), delegate
            {
                InitBase();
                InitCategory();
                InitScrollAdapter();

                scriptInitDone = true;
            }, bActiveSelf: false);
        }, bActiveSelf: false);
    }

    protected override void OnShow(int _LayerOrder)
    {
        base.OnShow(_LayerOrder);

        UIManager.Instance.TopMost<UISubHUDCharacterState>(true);
        UIManager.Instance.TopMost<UISubHUDCurrency>(true);
        UIManager.Instance.Find<UISubHUDCurrency>().ShowSpecialCurrency(
            new List<uint>() { DBConfig.Diamond_ID, DBConfig.Gold_ID, DBConfig.Essence_ID });

        didSetCategoryEntering = false;

        // 처음으로 보이게끔 세팅. 
        if (scriptInitDone && resetCategoryInfoOnEnter)
        {
            didSetCategoryEntering = true;
            MoveToFirstTab();
        }

        if (showCo != null)
        {
            StopCoroutine(showCo);
            showCo = null;
        }

        #region Show 이전 처리 
        isSetupDone = false;
        #endregion

        showCo = StartCoroutine(WaitUntilReady(() =>
        {
            ZLog.Log(ZLogChannel.UI, "********* (12) ************");
            isSetupDone = true;
            UIManager.Instance.ShowGlobalIndicator(false);
            showCo = null;
            ShowOnEnter();
        }));
    }

    protected override void OnRefreshOrder(int _LayerOrder)
    {
        base.OnRefreshOrder(_LayerOrder);
        UIManager.Instance.TopMost<UISubHUDCharacterState>(true);
        UIManager.Instance.TopMost<UISubHUDCurrency>(true);
        UIManager.Instance.Find<UISubHUDCurrency>().ShowSpecialCurrency(
            new List<uint>() { DBConfig.Diamond_ID, DBConfig.Gold_ID, DBConfig.Essence_ID });
    }

    protected override void OnHide()
    {
        base.OnHide();

        if (isSetupDone == false)
        {
            UIManager.Instance.ShowGlobalIndicator(false);
        }

        /// 끌때 첨으로돌리느거 일단 제거 . 
        //if (isSetupDone && resetCategoryInfoOnEnter)
        //{
        //          forceUpdate = true;
        //          SetMode(Mode.Category);
        //          CategoryDescriptor.SelectFirstMainCategory(true);
        //          UpdateUI();
        //}

        UIManager.Instance.TopMost<UISubHUDCharacterState>(true);
        UIManager.Instance.TopMost<UISubHUDCurrency>(true);

        UIManager.Instance.Find<UISubHUDCurrency>().ShowBaseCurrency();

        ReleaseIAP();

        if (infoPopUp != null)
        {
            infoPopUp.Close(true);
        }
    }
    #endregion

    #region Private Methods
    private void MoveToFirstTab()
    {
        MoveTab(CategoryDescriptor.FirstMainTab);
        mainCtgScrollRect.horizontalNormalizedPosition = 0f;
    }

    private void MoveTab(E_SpecialShopType specialShopType)
    {
        bool mainCtgChanged = specialShopType != CategoryDescriptor.SelectedMainCategoryType;

        forceUpdate = true;
        SetMode(Mode.Category);
        CategoryDescriptor.SelectMainCategory(specialShopType);
        if (mainCtgChanged)
        {
            CategoryDescriptor.SelectFirstSubCategory();
        }
        UpdateUI();
    }

    private void SetInteractableStartUpUI(bool interactable)
    {
        toggleFilterCharacterType.interactable = interactable;
        btnArchive.interactable = interactable;

        if (mainCtgUIs != null)
        {
            for (int i = 0; i < mainCtgUIs.Count; i++)
            {
                if (mainCtgUIs[i] != null)
                {
                    mainCtgUIs[i].toggle.interactable = interactable;
                }
            }
        }
        if (subCtgUIs != null)
        {
            for (int i = 0; i < subCtgUIs.Count; i++)
            {
                if (subCtgUIs[i] != null)
                {
                    subCtgUIs[i].toggle.interactable = interactable;
                }
            }
        }
    }

    private void ReleaseIAP()
    {
        iapSetupStatus = IAPStatusByFlow.None;
        NTIcarusManager.Instance.TryConnectComplete -= OnTryConnectComplete;
        NTIcarusManager.Instance.ProductInfosRefreshed -= OnIAPProductsRefreshed;
        NTIcarusManager.Instance.RemoveListener_PurchasePaymentCompleted(OnPurchasePaymentCompleted);
    }

    #region Get 
    private UISpecialShopMainCtg GetMainCategoryUI(E_SpecialShopType type)
    {
        return mainCtgUIs.Find(t => t.Key == type);
    }

    private UISpecialShopSubCtg GetSubCategoryUI(E_SpecialSubTapType type)
    {
        return subCtgUIs.Find(t => t.Key == type);
    }
    #endregion

    private void RefreshShopItems(List<SpecialShopCategoryDescriptor.SingleDataInfo> dataList)
    {
        ScrollAdapter_Category.RefreshData(dataList);
    }

    private void RefreshArchiveItemList()
    {
        var dataList = new List<SpecialShopArchiveListItemModel>();
        for (int i = 0; i < Me.CurCharData.CashMailList.Count; i++)
        {
            var t = Me.CurCharData.CashMailList[i];
            dataList.Add(new SpecialShopArchiveListItemModel(t.MailIdx, t.ShopTid));
        }

        this.ScrollAdapter_Archive.Refresh(dataList);
    }

    private void HandleShopItemClicked(SingleDataInfo info)
    {
        OpenItemInfoPopUp(info);
    }

    private void OpenItemInfoPopUp(uint tid)
    {
        var targetData = CategoryDescriptor.FindDisplayData(tid);

        if (targetData == null)
        {
            ZLog.LogError(ZLogChannel.UI, "Could not find target SpecialShopData that is displayble , Tid : " + tid);
            return;
        }

        OpenItemInfoPopUp(targetData);
    }

    private void OpenItemInfoPopUp(SingleDataInfo info)
    {
        if (infoPopUp != null)
        {
            SetupInfoPopUp(info);
            infoPopUp.gameObject.SetActive(true);
        }
        else
        {
            ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UISpecialShopItemInfoPopUp), (obj) =>
            {
                var target = obj.GetComponent<UISpecialShopItemInfoPopUp>();

                if (target != null)
                {
                    infoPopUp = target;
                    obj.transform.SetParent(overlayRoot);
                    //obj.transform.SetAsLastSibling();
                    obj.transform.localPosition = Vector3.zero;
                    obj.transform.localScale = Vector3.one;
                    target.RectTransform.MatchParentSize(false);
                    SetupInfoPopUp(info);
                }
            });
        }
    }

    /// <summary>
    /// 보관함에서 아이템 하나를 받기 눌렀을때 처리 
    /// </summary>
    private void HandleArchiveItemReceiveBtnClicked(SpecialShopArchiveListItemModel data)
    {
        RequestReceiveArchiveItem(new List<CashMailData>() { new CashMailData(data.mailIdx, data.specialShopTid) });
    }

    private void RequestReceiveArchiveItem(List<CashMailData> itemData)
    {
        ZWebManager.Instance.WebGame.REQ_TakeCashMailItems(itemData
            , (revPacket, resList) =>
            {
                /// TODO : LOCALE
                OpenNotiUp("상품을 수령하였습니다", "알림");

                isArchiveListDirty = true;
                UpdateUI();
            });
    }

    void SetupInfoPopUp(SpecialShopCategoryDescriptor.SingleDataInfo info)
    {
        var data = DBSpecialShop.Get(info.specialShopId);
        string targetProductID = DBSpecialShop.GetStoreProductIDByPlatform(info.specialShopId);
        PurchaseProductInfo productInfo = null;

        if (Application.isMobilePlatform)
        {
            /// Cash 면 StoreProduct 정보 세팅 
            if (data.CashType == E_CashType.Cash)
            {
                productInfo = this.storeProducts.Find(t => t.productId == targetProductID);

                if (productInfo == null)
                {
                    ZLog.LogError(ZLogChannel.UI, "**** Could not get ProductID from cached list. Check if it was obtained right on OnIAPProductsRefreshed() ****");
                    return;
                }
            }
        }

        infoPopUp.SetupAuto_Cash(
            new UISpecialShopItemInfoPopUp.DisplayItemParam(info.specialShopId)
            , this.iapSetupStatus
            , productInfo
            , onPreDirecting: () =>
            {
                /// 연출후에 스페셜 상점 Frame UI 를 다시 Open 하기때문에 , 원래것으로 점프해야함. 플래그 세팅. 
                resetCategoryInfoOnEnter = false;
            }
            , onPurchased_cash: (purchasedSpecialShopTid, resList) =>
            {
                forceUpdate = true;
                UpdateUI();
            }
            , onPurchased_nonCash: (resList) =>
            {
                forceUpdate = true;
                UpdateUI();
            }
            , onClosed: () => { });

        //infoPopUp.Setup_ManualImplement(new UISpecialShopItemInfoPopUp.DisplayItemParam(info.specialShopId)
        //    , buyBtnClickedListener: (param) =>
        //    {
        //        var specialShopData = DBSpecialShop.Get(info.specialShopId);

        //        if (specialShopData.CashType == E_CashType.Cash)
        //        {

        //        }
        //        else if (specialShopData.CashType == E_CashType.None)
        //        {
        //            /// 필요 재화 체크 
        //            bool isEnoughMoney = ConditionHelper.CheckCompareCost(specialShopData.BuyItemID, specialShopData.BuyItemCount);

        //            if (isEnoughMoney == false)
        //                return;
        //        }

        //        OpenTwoButtonQueryPopUp("확인", "정말 구매하시겠습니까?",
        //            onConfirmed: () =>
        //           {
        //               infoPopUp.RequestBuy();
        //           });
        //    }
        //    , () => true
        //    , onClosed: () =>
        //    {
        //    });
    }

    //private void TryBuyItem(SpecialShopCategoryDescriptor.SingleDataInfo info)
    //{

    //}

    IEnumerator WaitUntilReady(Action onFinished)
    {
        UIManager.Instance.ShowGlobalIndicator(true);

        /// 모바일 환경에서만 IAP 세팅 진행. 
        if (Application.isMobilePlatform)
        {
            SetIAPSetupStatus(IAPStatusByFlow.RequestStartSetup);
            ZLog.Log(ZLogChannel.UI, "IAP Setup Start");
        }
        else
        {
            SetIAPSetupStatus(IAPStatusByFlow.Disable);
            ZLog.Log(ZLogChannel.UI, "IAP Setup Canceled (EditorMode)");
        }

        // isIAPReady = true;

        #region 스페셜 상점 스크립트 내부 Initialize 여부 체킹
        yield return new UnityEngine.WaitUntil(() => scriptInitDone);
        #endregion

        ZLog.Log(ZLogChannel.UI, "********* (0) ************");

        #region 처음에 Inactive 시킬 UI 들 처리 
        SetInteractableStartUpUI(false);
        #endregion

        ZLog.Log(ZLogChannel.UI, "********* (1) ************");

        if(iapSetupStatus == IAPStatusByFlow.RequestStartSetup) 
        {
            ProcessIAPSetting();
		}

        storeProducts = new List<PurchaseProductInfo>();

        #region 구매 제한 데이터 업데이트
        bool isBuyLimitCountDataRefreshDone = false;

        ZLog.Log(ZLogChannel.UI, "********* (10) ************");
        ZWebManager.Instance.WebGame.REQ_GetBuyLimitList((revPacket, resList) =>
        {
            isBuyLimitCountDataRefreshDone = true;
        }, (err, req, res) =>
        {
            isBuyLimitCountDataRefreshDone = true;
        });

        yield return new WaitUntil(() => isBuyLimitCountDataRefreshDone);
        ZLog.Log(ZLogChannel.UI, "********* (11) ************");
        #endregion

        isSetupDone = true;
        willRefreshCashMail = true;

        onFinished();
    }

    private void SetIAPSetupStatus(IAPStatusByFlow status, bool instantProcess = false)
	{
		iapSetupStatus = status;
        NotifyIAPStatusChangedToInfoPopUp();

        if (instantProcess)
			ProcessIAPSetting();
	}

	/// <summary>
	/// IAP Status 따라 비동기 처리 함수 
	/// </summary>
	private void ProcessIAPSetting()
	{
		switch (iapSetupStatus)
		{
			/// Idle 상태의 Case 들. 
			case IAPStatusByFlow.None:
            case IAPStatusByFlow.Disable:
            case IAPStatusByFlow.Connecting:
			case IAPStatusByFlow.ConnectDoneFail: 
			case IAPStatusByFlow.RefreshingPurchaseInfo:
            case IAPStatusByFlow.RestoringProducts:
				break;
            /// IAP 세팅을 시작
			case IAPStatusByFlow.RequestStartSetup:
				{
                    if(PurchaseAPI.IsConnected == false)
					{
                        SetIAPSetupStatus(IAPStatusByFlow.Connecting);

                        NTIcarusManager.Instance.TryConnectComplete -= OnTryConnectComplete;
                        NTIcarusManager.Instance.TryConnectComplete += OnTryConnectComplete;
                        PurchaseAPI.Connect();
                    }
					else
					{
                        SetIAPSetupStatus(IAPStatusByFlow.ConnectDoneSuccess);
					}
				}       
				break;
                /// 연결이 됐다면 스토어 상품 정보를 Refresh 함 
			case IAPStatusByFlow.ConnectDoneSuccess:
				{
                    SetIAPSetupStatus(IAPStatusByFlow.RequestRefreshPurchaseInfo, true);
				}
				break;
                /// 상품 정보 Refresh 
            case IAPStatusByFlow.RequestRefreshPurchaseInfo:
				{
					NTIcarusManager.Instance.RemoveListener_PurchasePaymentCompleted(OnPurchasePaymentCompleted);
					NTIcarusManager.Instance.AddListener_PurchasePaymentCompleted(OnPurchasePaymentCompleted);

                    NTIcarusManager.Instance.ProductInfosRefreshed -= OnIAPProductsRefreshed;
                    NTIcarusManager.Instance.ProductInfosRefreshed += OnIAPProductsRefreshed;

                    NTIcarusManager.Instance.InitPurchaseFlow();

                    SetIAPSetupStatus(IAPStatusByFlow.RefreshingPurchaseInfo);
                }
                break;
                /// 상품 정보 Refreh 가 끝나면 복구 처리 요청함 
            case IAPStatusByFlow.RefreshPurchaseInfoDone:
				{
                    SetIAPSetupStatus(IAPStatusByFlow.RequestCheckRestore);
				}
                break;
                /// 복구 요청 . 복구할게 없으면 바로 Available (구매가능) 상태로 세팅 
            case IAPStatusByFlow.RequestCheckRestore:
				{
                    bool requested = NTIcarusManager.Instance.TryRestoreIfExist();

					if (requested)
					{
                        SetIAPSetupStatus(IAPStatusByFlow.RestoringProducts);
					}
					else
					{
                        SetIAPSetupStatus(IAPStatusByFlow.Available);
					}
				}
                break;
			case IAPStatusByFlow.Available:
                { }
				break;
			default:
                ZLog.LogError(ZLogChannel.UI, "Type not matching. Handle this , Type : " + iapSetupStatus.ToString());
				break;
		}
	}

    private void OnTryConnectComplete(NTPurchaseConnectResult result)
    {
        NTIcarusManager.Instance.TryConnectComplete -= OnTryConnectComplete;

        ZLog.Log(ZLogChannel.UI, "********* (4) ************");

        if (result == null || result.success == false)
        {
            SetIAPSetupStatus(IAPStatusByFlow.ConnectDoneFail);
        }
        else
        {
            SetIAPSetupStatus(IAPStatusByFlow.ConnectDoneSuccess);
        }
    }


    /// <summary>
    /// 복구처리로 획득한 아이템 대상으로만 Consume 처리하기 위한 함수 . 
    /// 정상 루틴으로 구매를 할때는 들어오지 않게끔 Remove 되어야함  
    /// (result.success, BillingOrderID, ProductID, 스토어상 상품정보, OriginalJson)
    /// </summary>
    private void OnPurchasePaymentCompleted(bool result, string billingOrderID, string productID, PurchaseProductInfo infoOnStore, string oriJson)
    {
        /// 만약에, Consume 되지 않은 상태에서 구매까지만 되고 스페셜 상점을 나갔다(PendingList에 등록돼있음) -> 
        /// 다시 스페셜상점에 들어온다 -> 
        /// 미처리 복구 건에 대해 Exist 를 체킹하였더니 Consume 되지 않은 상품이 복구 프로세스를 탄다 -> 
        /// 똑같은 상품이 현재 이 콜백에 들어온다, 가 가능한 시나리오라 판단되어 
        /// 영수증의 중복 여부 체킹을 매번함. 
        if (paymentPurchasePendingList.Exists(t => t.oriJson.Equals(oriJson)) == false)
        {
            paymentPurchasePendingList.Add(new ProcessPaymentPurchaseParam(billingOrderID, productID, infoOnStore, oriJson, null, null));
        }
    }

    private void OnIAPProductsRefreshed(List<PurchaseProductInfo> products)
    {
        NTIcarusManager.Instance.ProductInfosRefreshed -= OnIAPProductsRefreshed;

        ZLog.Log(ZLogChannel.UI, "********* (8) ************");
        SetIAPSetupStatus(IAPStatusByFlow.RefreshPurchaseInfoDone);
        isIAPProductsRefreshProcessDone = true;
        this.storeProducts = products;

        if (products == null)
        {
            this.storeProducts = new List<PurchaseProductInfo>();
            ZLog.LogError(ZLogChannel.UI, "**** OnIAPProductsRefreshed() , Products is NULL ****");
        }
        else if (products.Count == 0)
        {
            this.storeProducts = new List<PurchaseProductInfo>();
            ZLog.LogWarn(ZLogChannel.UI, "**** OnIAPProductsRefreshed() , Products Count is 0 ****");
        }
        else
        {
            ZLog.Log(ZLogChannel.UI, "***** OnIAPProductsRefreshed() , Retrieved Products Below **** ");

            for (int i = 0; i < products.Count; i++)
            {
                var t = products[i];

                ZLog.Log(ZLogChannel.UI, " **** Product Title : " + t.title + " , ID : " + t.productId + " , Price ; " + t.price);
            }
        }
    }

    private void ShowOnEnter()
    {
        ZLog.Log(ZLogChannel.UI, "********* (13) ************");
        SetInteractableStartUpUI(true);

        if (didSetCategoryEntering == false && resetCategoryInfoOnEnter)
        {
            didSetCategoryEntering = true;

            MoveToFirstTab();

            this.filters.Reset();
            this.toggleFilterCharacterType.isOn = false;
        }

        resetCategoryInfoOnEnter = true;

        // ljh : 초기화 모두 끝난 후 처리할 로직
        onPostInit?.Invoke();
        onPostInit = delegate { };
    }

    private void SetMode(Mode mode)
    {
        curMode = mode;
    }

    private void InitBase()
    {
        paymentPurchasePendingList = new List<ProcessPaymentPurchaseParam>();
        storeProducts = new List<PurchaseProductInfo>();
        filters = new FilterOptions();
        resetCategoryInfoOnEnter = true;
    }

    private void InitCategory()
    {
        CategoryDescriptor = new SpecialShopCategoryDescriptor();
        CategoryDescriptor.Initialize(this);

        mainCtgUIs = new List<UISpecialShopMainCtg>();
        subCtgUIs = new List<UISpecialShopSubCtg>();

        /// 보여야 하는 MainCategory 들 순회하며 생성 
        foreach (var byMainCtg in CategoryDescriptor.MainCategories)
        {
            var mainCtgUI = Instantiate(this.mainCtgToggleSourceObj, mainCtgParent);

            if (mainCtgUI != null)
            {
                /// 메인 카테고리는 생성과 동시에 Active 
                mainCtgUI.Initialize(this.mainCtgToggleGroup, byMainCtg.Key, DBLocale.GetText(GetMainCategoryTextKey(byMainCtg.Key)));
                mainCtgUI.gameObject.SetActive(true);
                mainCtgUIs.Add(mainCtgUI);
            }
            else
            {
                ZLog.LogError(ZLogChannel.UI, "Failed to generate mainCategory");
            }
        }

        /// 보관함용 보이지 않는 Dummy 메인 카테고리 하나 생성 
        var dummyMainCtg = Instantiate(this.mainCtgToggleSourceObj, mainCtgParent);
        if (dummyMainCtg != null)
        {
            /// 메인 카테고리는 생성과 동시에 Active 
            dummyMainCtg.Initialize_Dummy(this.mainCtgToggleGroup);
            dummyMainCtg.gameObject.SetActive(true);
            dummyMainCtg.gameObject.name = "DummyCategory(ForAllOff)";
            mainCtgUIs.Add(dummyMainCtg);
        }

        for (int i = 0; i < CategoryDescriptor.MaxSubCategoryCount; i++)
        {
            var subCtgUI = Instantiate(this.subCtgToggleSourceObj, subCtgParent);

            if (subCtgUI != null)
            {
                subCtgUI.Initialize(subCtgToggleGroup);
                subCtgUI.gameObject.SetActive(false);
                subCtgUIs.Add(subCtgUI);
            }
            else
            {
                ZLog.LogError(ZLogChannel.UI, "Failed to generate subCategory");
            }
        }
    }

    private void InitScrollAdapter()
    {
        ScrollAdapter_Category.onClickedSlot = HandleShopItemClicked;
        ScrollAdapter_Category.Initialize(-1);

        ScrollAdapter_Archive.SetListenerOnClickReceive(HandleArchiveItemReceiveBtnClicked);
        ScrollAdapter_Archive.Initialize();
    }

    bool applicationQuitting = false;

    private void OnApplicationQuit()
    {
        applicationQuitting = true;
    }

    #region UpdateUI
    private bool isDestroying;

    private void UpdateUI()
    {
        if (isDestroying)
            return; 

        /// 애플리케이션 종료시 exception 간헐적 발생 일단 방지 
        /// (GameObjects can not be made active when they are being destroyed.)
        this.categoryRoot.SetActive(curMode == Mode.Category);
        this.archiveRoot.SetActive(curMode == Mode.Archive);

        if (curMode == Mode.Category)
        {
            UpdateUI_MainCategory();
            UpdateUI_SubCategory();
            UpdateUI_BottomUI();

            isFilterDirty = false;
            forceUpdate = false;
            lastUpdatedMainCtgType = CategoryDescriptor.SelectedMainCategoryType;
            lastUpdatedSubCtgType = CategoryDescriptor.SelectedSubCategoryType;
        }
        else if (curMode == Mode.Archive)
        {
            UpdateUI_Archive();
        }
    }

	private void OnDestroy()
	{
        isDestroying = true;
	}

	private void UpdateUI_MainCategory()
    {
        var curSelectedType = CategoryDescriptor.SelectedMainCategoryType;

        /// 선택된 메인 카테고리로 선택 로직 
        if (forceUpdate || lastUpdatedMainCtgType != curSelectedType)
        {
            var targetUI = GetMainCategoryUI(curSelectedType);

            if (targetUI != null
                && targetUI.toggle.isOn == false)
            {
                targetUI.toggle.SelectToggle();
            }

            var mainCtg = CategoryDescriptor.SelectedMainCategoryInfo;

            if (mainCtg != null)
            {
                /// 메인 카테고리가 바뀌었으므로 SubCategory 업데이트 로직 
                for (int i = 0; i < subCtgUIs.Count; i++)
                {
                    var ui = subCtgUIs[i];

                    if (i < mainCtg.subCtgInfoList.Count)
                    {
                        var info = mainCtg.subCtgInfoList[i];
                        ui.Set(GetSubCategoryTextKey(info.type), info.type);
                        ui.gameObject.SetActive(true);
                    }
                    else
                    {
                        ui.gameObject.SetActive(false);
                    }
                }
            }
        }
    }

    private void UpdateUI_SubCategory()
    {
        var curSelectedType = CategoryDescriptor.SelectedSubCategoryType;

        /// None 은 중복되는게 있기때문에 그냥 무조건 업데이트 
        if (forceUpdate
            || lastUpdatedMainCtgType != CategoryDescriptor.SelectedMainCategoryType
            || isFilterDirty
            || curSelectedType == E_SpecialSubTapType.None
            || lastUpdatedSubCtgType != curSelectedType)
        {
            var targetUI = GetSubCategoryUI(curSelectedType);

            /// toggle Select 
            if (targetUI != null
                && targetUI.toggle.isOn == false)
            {
                targetUI.toggle.SelectToggle();
            }

            bool isDataDirty = CategoryDescriptor.UpdateSelectedCategoryData();

            ScrollAdapter_Category.SetNormalizedPosition(0f);

            if (isDataDirty)
            {
                RefreshShopItems(CategoryDescriptor.SelectedCategoryShopItemList);
            }
        }
    }

    private void UpdateUI_BottomUI()
    {
        var curMainCtgType = CategoryDescriptor.SelectedMainCategoryType;

        if (forceUpdate || lastUpdatedMainCtgType != curMainCtgType)
        {
            toggleFilterCharacterType.gameObject.SetActive(curMainCtgType == E_SpecialShopType.Essence);
            toggleFilterCharacterType.isOn = false;
        }

        if (isFilterDirty)
        {
            string charFilterText = "All_Occupations";
            if (filters.characterType != E_CharacterType.All)
            {
                charFilterText = DBLocale.GetText("Character_" + filters.characterType.ToString());
            }

            this.txtCharacterFilter_normal.text = charFilterText;
            this.txtCharacterFilter_on.text = charFilterText;
        }
    }

    private void UpdateUI_Archive()
    {
        var dummyCtg = mainCtgUIs.Find(t => t.IsDummy);
        if (dummyCtg != null && dummyCtg.toggle.isOn == false)
        {
            dummyCtg.toggle.SelectToggle();
        }

        if (isArchiveListDirty)
        {
            isArchiveListDirty = false;
            RefreshArchiveItemList();
        }
    }
    #endregion

    #region ETC
    private bool CheckValidateFatalData()
    {
        bool isFatal = false;

        DBSpecialShop.ForeachMainCategory((mainCtgType, subTabDic) =>
        {
            bool noneSubTabExist = subTabDic.ContainsKey(E_SpecialSubTapType.None);
            if (noneSubTabExist)
            {
                /// None 타입의 SubTab 이 존재하는데 , None 이 아닌것도 존재한다 => 
                /// 기획 테이블 수정해야함 . 
                /// 스페셜 상점에서의 SubTab 에서 None 타입은 Invalid 가 아닌 
                /// 실제로 존재하는 탭이지만 보이지는 않는 탭임 . 단일로만 존재해야함 . 
                /// 기획 픽스 사항임. 
                if (subTabDic.Count > 1)
                {
                    ZLog.LogError(ZLogChannel.UI, "SpecialShop SubTab None Type must be unique, cant exist with other types in the same MainTab. MainTabType : " + mainCtgType.ToString());
                }
            }
        });

        return isFatal == false;
    }
    #endregion
    #endregion

    #region Inspector Events 
    public void OnMainCategoryToggleValueChanged(UISpecialShopMainCtg category)
    {
        if (isDestroying || applicationQuitting || isSetupDone == false)
            return;

        if (category.IsDummy)
            return;

        if (category.toggle.isOn)
        {
            SetMode(Mode.Category);

            /// 메인 카테고리 변경 
            if (category.Key != CategoryDescriptor.SelectedMainCategoryType)
            {
                CategoryDescriptor.SelectMainCategory(category.Key);
                CategoryDescriptor.SelectFirstSubCategory();
            }

            UpdateUI();
        }
    }

    public void OnSubCategoryToggleValueChanged(UISpecialShopSubCtg category)
    {
        if (isDestroying || applicationQuitting || isSetupDone == false)
            return;

        if (category.toggle.isOn)
        {
            CategoryDescriptor.SelectSubCategory(category.Key);
            /// TODO : Update Selected Sub Category 
            UpdateUI();
        }
    }

    public void OnCharacterTypeFilterChanged(int index)
    {
        E_CharacterType newType = (E_CharacterType)index;
        isFilterDirty = filters.characterType != newType;
        filters.characterType = newType;
        toggleFilterCharacterType.isOn = false;
        toggleFilterCharacterType.GraphicUpdateComplete();
        UpdateUI();
    }

    #region OnClick

    public void OnClickArchiveBtn()
    {
        if (curMode == Mode.Archive)
            return;

        SetMode(Mode.Archive);
        UpdateUI();
    }

    public void OnClickReceiveAllArchiveItems()
    {
        if (ScrollAdapter_Archive.CurDataShownCount == 0)
        {
            /// TODO : LOCALE 
            OpenNotiUp("수령할 상품이 없습니다", "알림");
            return;
        }

        RequestReceiveArchiveItem(Me.CurCharData.CashMailList);
    }

    public void OnClickCloseBtn()
    {
        Close();
    }

    #endregion
    #endregion

    #region Define
    public class FilterOptions
    {
        public E_CharacterType characterType;

        public FilterOptions()
        {
            characterType = E_CharacterType.All;
        }

        public void Reset()
        {
            characterType = E_CharacterType.All;
        }
    }
    #endregion
}
