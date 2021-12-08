using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using ZNet.Data;
using ZNet;
using WebNet;
using GameDB;
using NTCore;

public class UISpecialShopItemInfoPopUp : MonoBehaviour
{
	public delegate void ProcessIAP(Action<bool, ResPaymentPurchase> onFinished);
	/// <summary>
	/// 현재 팝업창을 띄우는데 외부로부터 필요한 정보들 
	/// </summary>
	public class DisplayItemParam
	{
		public uint specialShopTid;

		public DisplayItemParam(uint specialShopTid)
		{
			this.specialShopTid = specialShopTid;
		}
	}

	#region UI Variables
	[SerializeField] private RectTransform rectTransform;

	[SerializeField] private Image imgMainIcon;
	[SerializeField] private Text txtName;
	[SerializeField] private Text txtDescription;
	[SerializeField] private Image imgCostIcon;
	[SerializeField] private Text txtCost;
	[SerializeField] private Text txtItemCnt;
	[SerializeField] private Text txtItemBuyLimit;

	[SerializeField] private Button btnBuyNonCashPay;

	[SerializeField] private Button btnDecreaseCntOne;
	//[SerializeField] private Button btnDecreaseCntMultiple01;
	//[SerializeField] private Button btnDecreaseCntMultiple02;

	[SerializeField] private Button btnIncreaseCntOne;
	[SerializeField] private Button btnIncreaseCntMultiple01;
	[SerializeField] private Button btnIncreaseCntMultiple02;

	[SerializeField] private GameObject payByCashRoot;
	[SerializeField] private Text txtCashCostNum;
	[SerializeField] private Image imgCashCostIcon;
	#endregion

	#region Preference
	[SerializeField] private Color notEnoughMoneyColor;

	[SerializeField, Range(0, 100)] private int desiredCount_increaseMultiple01;
	[SerializeField, Range(0, 100)] private int desiredCount_increaseMultiple02;

	[SerializeField, Range(0, 100)] private int desiredCount_decreaseMultiple01;
	[SerializeField, Range(0, 100)] private int desiredCount_decreaseMultiple02;
	#endregion

	private uint minBuyCount;
	private uint maxBuyCount;

	private uint curBuyCnt;
	private uint singleCostCnt;

	private bool isBuyLimitExist;
	private uint buyLimitCnt;
	private uint myCurBuyLimitCnt;
	private uint buyLimitRemainedCnt;

	private ulong myCurrencyCountNonCash;

	private string cashNameTextKey;

	private E_CashType cashType;

	private DisplayItemParam Info;
	// private bool IAPValid;
	private IAPStatusByFlow IAPStatus;
	private PurchaseProductInfo CashProductInfo;
	private Action<List<DBSpecialShop.ItemBuyInfo>> OnBuyBtnClicked;
	private ProcessIAP IAPProcessor;
	private Action OnClosed;
	private Action OnPreDirecting;

	/// <summary>
	/// 구매한 스페셜상점 상품 Tid 
	/// 구매후 프로토콜 Res 값 
	/// </summary>
	private Action<uint, ResPaymentPurchase> OnPurchased_Cash;
	private Action<ResSpecialShopBuy> OnPurchased_NonCash;

	/// <summary>
	/// Auto : TRUE,  Manual : FALSE
	/// </summary>
//	private bool buyAutoOrManual;

	private List<DBSpecialShop.ItemBuyInfo> reqBuyInfo = new List<DBSpecialShop.ItemBuyInfo>();

	public RectTransform RectTransform { get { return this.rectTransform; } }
	public List<DBSpecialShop.ItemBuyInfo> ReqBuyInfo { get { return reqBuyInfo; } }
	public string CurIAPProductIDInProcess => CashProductInfo != null ? CashProductInfo.productId : string.Empty;
	private uint CurCostCount { get { return singleCostCnt * curBuyCnt; } }

	/// <summary>
	/// 구매 로직 직접 구현용 
	/// </summary>
	public void Setup_ManualImplement(
		DisplayItemParam infoParam
		, IAPStatusByFlow iapStatus
		, PurchaseProductInfo cashProductInfo
		, Action<List<DBSpecialShop.ItemBuyInfo>> buyBtnClickedListener
		, Action onPreDirecting
		, ProcessIAP iapProcessor
		, Action<uint, ResPaymentPurchase> onPurchased_cash
		, Action<ResSpecialShopBuy> onPurchased_nonCash
		, Action onClosed)
	{
		Setup(infoParam, iapStatus, cashProductInfo, buyBtnClickedListener, iapProcessor, onPreDirecting, onPurchased_cash, onPurchased_nonCash, onClosed);
	}

	/// <summary>
	/// 구매 로직 내부 로직 사용용 , 현금 결제 가능시
	/// </summary>
	public void SetupAuto_Cash(
		DisplayItemParam infoParam
		, IAPStatusByFlow iapStatus
		, PurchaseProductInfo cashProductInfo
		, Action onPreDirecting
		, Action<uint, ResPaymentPurchase> onPurchased_cash /// 캐쉬로 구매한경우 , 완료후 호출 
		, Action<ResSpecialShopBuy> onPurchased_nonCash /// 게임 머니로 구매한 경우 , 완료후 호출 
		, Action onClosed)
	{
		Setup(
			infoParam
			, iapStatus
			, cashProductInfo
			, buyBtnClickedListener: Internal_HandleBuyBtnClicked
			, iapProcessor: Internal_HandleIAPProcess
			, onPreDirecting
			, onPurchased_cash
			, onPurchased_nonCash
			, onClosed);
	}

	/// <summary>
	/// 구매 로직 내부 로직 사용용 , 게임 머니로만 결제시 
	/// </summary>
	public void SetupAuto_NonCash(
		DisplayItemParam infoParam
		, Action<ResSpecialShopBuy> onPurchased_nonCash
		, Action onPreDirecting = null
		, Action onClosed = null)
	{
		Setup(
			infoParam
			, IAPStatusByFlow.Disable
			, null
			, buyBtnClickedListener: Internal_HandleBuyBtnClicked
			, null
			, onPreDirecting
			, null
			, onPurchased_nonCash
			, onClosed);
	}

	/// <summary>
	/// 창을 세팅합니다. 
	/// </summary>
	/// <param name="infoParam"> 현재 구매하려는 상품의 정보 데이터</param>
	/// <param name="iapStatus"> 현금 결제 가능 여부와 관련된 IAP 상태 값</param>
	/// <param name="cashProductInfo"> 현금 결제 상품인경우 Product 데이터</param>
	/// <param name="buyBtnClickedListener"> 구매 버튼을 눌렀을때 호출될 함수 </param>
	/// <param name="iapProcessor"> 현금 결제일 경우 IAP 로직 실행 함수 </param>
	/// <param name="onPreDirecting"> 구매후 연출을 하는 경우에 연출하기 직전에 한번 호출될 함수 </param>
	/// <param name="onPurchased_cash"> 현금으로 결제를 완료한 경우에 호출될 함수 </param>
	/// <param name="onPurchased_nonCash"> 게임머니로 결제를 완료한 경우에 호출될 함수 </param>
	/// <param name="onClosed"> 창이 꺼질때 호출될 함수 </param>
	private void Setup(
		DisplayItemParam infoParam
		, IAPStatusByFlow iapStatus
		, PurchaseProductInfo cashProductInfo
		, Action<List<DBSpecialShop.ItemBuyInfo>> buyBtnClickedListener
		, ProcessIAP iapProcessor
		, Action onPreDirecting
		, Action<uint, ResPaymentPurchase> onPurchased_cash
		, Action<ResSpecialShopBuy> onPurchased_nonCash
		, Action onClosed)
	{
		Info = infoParam;
		IAPStatus = iapStatus;
		CashProductInfo = cashProductInfo;
		OnBuyBtnClicked = buyBtnClickedListener;
		OnClosed = onClosed;
		OnPreDirecting = onPreDirecting;
		OnPurchased_Cash = onPurchased_cash;
		OnPurchased_NonCash = onPurchased_nonCash;
		IAPProcessor = iapProcessor;

		DBSpecialShop.E_SpecialShopDisplayGoodsTarget goodsType = DBSpecialShop.E_SpecialShopDisplayGoodsTarget.None;
		string nameKey = string.Empty;
		string spriteKey = string.Empty;
		uint goodsTid = 0;
		byte grade = 0;
		var specialShopData = DBSpecialShop.Get(infoParam.specialShopTid);
		bool valid = true;

		ZLog.Log(ZLogChannel.UI, "****Setup_Auto()****");
		ZLog.Log(ZLogChannel.UI, "****Setup_Auto()**** , CashType : " + specialShopData.CashType.ToString() + " , BuyItemCount : " + specialShopData.BuyItemCount);

		if (DBSpecialShop.GetGoodsPropsBySwitching(infoParam.specialShopTid, ref cashType, ref goodsType, ref nameKey, ref spriteKey, ref grade, ref goodsTid) == false)
		{
			ZLog.LogError(ZLogChannel.UI, "Failed To get desired Goods Info, special Shop tid : " + infoParam.specialShopTid);
			valid = false;
		}

		if (specialShopData == null)
		{
			ZLog.LogError(ZLogChannel.UI, "Failed to get specialShopTableData , special Shop tid : " + infoParam.specialShopTid);
			valid = false;
		}

		if (Application.isMobilePlatform)
		{
			if (specialShopData.CashType == E_CashType.Cash && cashProductInfo == null)
			{
				ZLog.LogError(ZLogChannel.UI, "The Goods is CashType but ProductInfo is NULL");
				valid = false;
			}
		}

		if (valid == false)
		{
			Close(true);
			return;
		}

		/// 구매 제한 세팅 
		bool useBuyLimit = specialShopData.BuyLimitType != GameDB.E_BuyLimitType.Infinite;
		var myBuyLimitData = Me.CurCharData.GetBuyLimitInfo(infoParam.specialShopTid);

		txtItemBuyLimit.gameObject.SetActive(useBuyLimit);

		if (useBuyLimit)
		{
			isBuyLimitExist = true;
			buyLimitCnt = specialShopData.BuyLimitCount;
			buyLimitRemainedCnt = specialShopData.BuyLimitCount;

			if (myBuyLimitData != null)
			{
				myCurBuyLimitCnt = myBuyLimitData.BuyCnt;

				if (specialShopData.BuyLimitCount < myBuyLimitData.BuyCnt)
				{
					buyLimitRemainedCnt = 0;
				}
				else
				{
					buyLimitRemainedCnt = specialShopData.BuyLimitCount - myBuyLimitData.BuyCnt;
				}
			}
			else
			{
				myCurBuyLimitCnt = 0;
			}

			txtItemBuyLimit.text = string.Format("구매제한 <color=#ffffff>( {0} / {1} )</color>", myCurBuyLimitCnt, specialShopData.BuyLimitCount);
		}
		else
		{
			isBuyLimitExist = false;
		}

		//ulong affordCount = UICommon.GetCurrency(specialShopData.BuyItemID) / specialShopData.BuyItemCount;

		/// 최대 개수 지정 가능 카운트 세팅 
		/// Infinite 인 경우는 우선은 BuyMaxCount 값으로 세팅 , Infinite 가 아니라면 우선 1 로 세팅 
		maxBuyCount = specialShopData.BuyLimitType == E_BuyLimitType.Infinite ? specialShopData.BuyMaxCount : 1;

		/// 0 이면 1 로 일단은 맞춰줌 . 
		if (maxBuyCount == 0)
			maxBuyCount = 1;

		/// Infinite 가 아닌데, 현재 내 정보에 해당 아이템에 대한 Limit Count 정보가 존재한다면 
		/// 실제 살수있는 만큼만 지정해줌 
		if (specialShopData.BuyLimitType != E_BuyLimitType.Infinite)
		{
			maxBuyCount = specialShopData.BuyLimitCount;

			if (myBuyLimitData != null)
			{
				if (maxBuyCount < myBuyLimitData.BuyCnt)
				{
					/// 여기 들어오면 안되긴 함 .
					maxBuyCount = 1;
				}
				else
				{
					maxBuyCount -= myBuyLimitData.BuyCnt;
				}
			}
		}

		/// 최종적으로 내가 살수 있는 양을 넘어간다면 
		/// 조정해줌 => 일단 미처리 
		//   if (maxBuyCount >= affordCount)
		//    maxBuyCount = (uint)affordCount;

		/// 인벤토리에 개수제한 체킹
		if (Me.CurCharData.IsFullInven())
		{
			maxBuyCount = 1;
		}
		/// Essence 아이템인 경우는 템 한칸당 하나기땜에 AvailableSlot Count 체킹함. 
		/// (TODO : Essence 말고도 하나당 한칸 차지하는 아이템인지 알기위한 조건이 좀더 명시적으로 필요할듯? . 찜찜함)
		else if (specialShopData.SpecialShopType == E_SpecialShopType.Essence)
		{
			uint availableInvenCount = Me.CurCharData.GetAvailableInvenCount();
			if(maxBuyCount > availableInvenCount)
			{
				maxBuyCount = availableInvenCount;
			}
		}

		curBuyCnt = 1; // affordCount > 0 ? 1u : 0u;
		minBuyCount = curBuyCnt;
		singleCostCnt = specialShopData.BuyItemCount;

		cashNameTextKey = specialShopData.ShopTextID;

		/// Set UI 
		this.imgMainIcon.sprite = ZManagerUIPreset.Instance.GetSprite(spriteKey);
		this.txtName.text = DBLocale.GetText(nameKey);
		this.txtDescription.text = DBLocale.GetText(specialShopData.DescriptionID);
		this.imgCostIcon.sprite = ZManagerUIPreset.Instance.GetSprite(DBItem.GetItemIconName(specialShopData.BuyItemID));

		bool isPaymentCash = cashType == E_CashType.Cash;

		payByCashRoot.gameObject.SetActive(isPaymentCash);

		/// Cash 인 경우에는 몇가지 다르게 처리함 .
		/// 하드코딩 
		if (isPaymentCash)
		{
			txtCashCostNum.text = specialShopData.BuyItemCount.ToString("n0");
			imgCashCostIcon.sprite = ZManagerUIPreset.Instance.GetSprite("icon_cash_kr");
		}
		else
		{
			myCurrencyCountNonCash = Me.GetCurrency(specialShopData.BuyItemID);
		}

		Refresh();
	}

	public void ChangeIAPStatus(IAPStatusByFlow status)
	{
		IAPStatus = status;
	}

	private void Refresh()
	{
		UpdateReqBuyInfo();
		UpdateVariableUIs();
	}

	private void UpdateReqBuyInfo()
	{
		DBSpecialShop.ItemBuyInfo info = null;

		if (reqBuyInfo.Count > 0)
		{
			info = reqBuyInfo[0];
		}
		else
		{
			info = new DBSpecialShop.ItemBuyInfo();
			reqBuyInfo.Add(info);
		}

		var shopData = DBSpecialShop.Get(this.Info.specialShopTid);
		var zitem = Me.CurCharData.GetItem(shopData.BuyItemID);

		info.Set(
			this.Info.specialShopTid
			, this.curBuyCnt
			, zitem != null ? zitem.item_id : 0
			, shopData.BuyItemID
			, 0);
	}

	private void UpdateVariableUIs()
	{
		this.txtCost.text = CurCostCount.ToString("n0");
		this.txtItemCnt.text = curBuyCnt.ToString();

		/// cash 타입 아닐때만 . 
		if (cashType != E_CashType.Cash)
		{
			this.txtCost.color = myCurrencyCountNonCash >= CurCostCount ? Color.white : notEnoughMoneyColor;

			var specialShopData = DBSpecialShop.Get(Info.specialShopTid);
			Item_Table possibleTargetItemData = null;

			if(specialShopData.GoodsKindType == E_GoodsKindType.Item
				&& specialShopData.GoodsItemID != 0)
			{
				possibleTargetItemData = DBItem.GetItem(specialShopData.GoodsItemID);
			}

			bool isInvenFull = Me.CurCharData.IsFullInven();

			/// 구매버튼 Interactable = 재화가 부족하지 않다 && 인벤토리 슬롯을 넘어서지 않는다 
			/// TODO(Sun) : Essence 말고 다른 아이템들도 인벤에 추가되는데 , 애네도 마찬가지로 인벤 칸을 차지함
			/// 고로 애네도 체킹해야하는데 , 예외적인건 Countable 이라면 Full 이어도 ExistTargetItem() 같은거로 True 체킹해서 
			/// 그냥 구매가능하게 하게끔할수도있음 . 즉 그냥 GoodsKind 가 Item 이라면 , IsBuyableByCount(itemTid, targetBuyCount); 이런거를 만들어서
			/// True/False 로 넣어주는게 훨씬 나을듯함 . 근데 그럴려면 기준이있어야함 . Countable 인지 아닌지에 대한 .(Item일때)
			/// 또 예외적으로 , 룬같은경우는 룬 인벤이 별도로 또 있는거같음 ? 이거 상관있음 ?  
			this.btnBuyNonCashPay.interactable = myCurrencyCountNonCash >= CurCostCount
				&& (specialShopData.SpecialShopType != E_SpecialShopType.Essence || (specialShopData.SpecialShopType == E_SpecialShopType.Essence && curBuyCnt <= Me.CurCharData.GetAvailableInvenCount()));		}

		this.btnDecreaseCntOne.interactable = curBuyCnt > 1;
		//this.btnDecreaseCntMultiple01.interactable = curBuyCnt > 0;
		//this.btnDecreaseCntMultiple02.interactable = curBuyCnt > 0;

		this.btnIncreaseCntOne.interactable = curBuyCnt < maxBuyCount;
		this.btnIncreaseCntMultiple01.interactable = curBuyCnt < maxBuyCount;
		this.btnIncreaseCntMultiple02.interactable = curBuyCnt < maxBuyCount;
	}

	/// <summary>
	/// Setup_Auto 사용시 사용하는 구매 버튼 눌렀을때 실행로직 
	/// </summary>
	private void Internal_HandleBuyBtnClicked(List<DBSpecialShop.ItemBuyInfo> purchaseItemInfo)
	{
		var specialShopData = DBSpecialShop.Get(this.Info.specialShopTid);

		ZLog.Log(ZLogChannel.UI, "**** BuyButtonClickHandle **** ");

		/// Cash 일때는 IAP 로직 실행 
		if (specialShopData.CashType == E_CashType.Cash)
		{
			/// TODO : LOCALE 
			/// 기능이 Disable 됐는데 Cash 아이템을 구매하려는 시도가 발생해선 안됨. 
			if (IAPStatus == IAPStatusByFlow.Disable)
			{
				OpenNotiUp("현금 결제를 지원하지 않는 상태입니다.");
				return;
			}
			else if (IAPStatus == IAPStatusByFlow.ConnectDoneFail)
			{
				OpenNotiUp("결제 모듈 연결에 실패하였습니다. 다시 시도해주십시요.");
				return;
			}
			else if (IAPStatus == IAPStatusByFlow.RestoringProducts)
			{
				OpenNotiUp("복구 절차가 진행중입니다. 다시 시도해주십시요.");
				return;
			}
			/// 최종적으로 Available 상태가 아니라면 , 결제를 막음. 
			else if (IAPStatus != IAPStatusByFlow.Available)
			{
				OpenNotiUp("결제 시스템이 정상적으로 초기화되지 않았습니다. 다시 시도해주십시요");
				return;
			}

			ZLog.Log(ZLogChannel.UI, "**** Execute Cash Logic ****");

			if (IAPProcessor == null)
			{
				ZLog.LogError(ZLogChannel.UI, "IAP Checker is NULL");
				return;
			}

			/// IAP Process 시작 
			IAPProcessor((result, resList) =>
			{
				/// 결과. 
				ZLog.Log(ZLogChannel.UI, "IAP Final Result : " + result.ToString());

				if (result)
				{
					OnPurchased_Cash?.Invoke(Info.specialShopTid, resList);
				}

				Close(true);
			});
		}
		/// Cash 가 아니면 게임 재화로 로직 실행 
		else if (specialShopData.CashType == E_CashType.None)
		{
			ZLog.Log(ZLogChannel.UI, "**** Execute Non Cash Logic ****");

			/// 필요 재화 체크 
			bool isEnoughMoney = ConditionHelper.CheckCompareCost(specialShopData.BuyItemID, specialShopData.BuyItemCount);

			if (isEnoughMoney == false)
			{
				return;
			}

			RequestBuy_NonCash();
		}
	}

	private void Internal_HandleIAPProcess(Action<bool, ResPaymentPurchase> onFinished)
	{
		if (Application.isEditor)
		{
			ZLog.Log(ZLogChannel.UI, "SpecialShop Cash Type Payment Not Supported in EditorMode");
			onFinished?.Invoke(false, default(ResPaymentPurchase));
			return;
		}

		ZLog.Log(ZLogChannel.UI, "**** IAP Process Begin **** ");
		ZLog.Log(ZLogChannel.UI, "**** IAP Recevied Product Info , Title : " + this.CashProductInfo.title + " , ProductID : " + this.CashProductInfo.productId + " , Price : " + this.CashProductInfo.priceString.ToString());

		UIManager.Instance.ShowGlobalIndicator(true);

		var data = DBSpecialShop.Get(Info.specialShopTid);
		string productID = DBSpecialShop.GetStoreProductIDByPlatform(Info.specialShopTid);

		ZLog.Log(ZLogChannel.UI, "**** CommonAPI StoreCD : " + CommonAPI.StoreCD.ToString() + " ****");
		ZLog.Log(ZLogChannel.UI, "**** ProductID : " + productID.ToString() + " **** ");

		/// IAP 1단계 - 서버에 알림. Init 호출 
		ZWebManager.Instance.Billing.REQ_PaymentInit(Info.specialShopTid, data.BuyItemCount, CashProductInfo.currenyCode
			, (init_revPacket, init_resList) =>
			{
				ZLog.Log(ZLogChannel.UI, "**** REQ_PaymentInit Success ****");
				ZLog.Log(ZLogChannel.UI, "**** LineOrderID : " + init_resList.LineOrderId + " , " + "PayId : " + init_resList.PayId);
				
				/// IAP 2단계 - 실 결제 루틴 (Native 실행)
				NTIcarusManager.Instance.Purchase(init_resList.LineOrderId, productID, init_resList.PayId
					, (isSuccess, billingOrderID, productID_, productInfoOnStore, oriJson) =>
					{
						if (isSuccess)
						{
							ZLog.Log(ZLogChannel.UI, "**** NTSDK Purchase Routine Success **** ");

							UIManager.Instance.ShowGlobalIndicator(false);

							/// IAP 3단계 - 구매했음을 서버에 알림 및 영수증 Consume 처리 및 기타 처리는 
							/// FrameSpecialShop::Update() 에서 비동기로 순차적으로 처리함. 

							/// IAP 3단계 - 구매했음을 서버에 알림 
							/// CAUTION : Signature 부분은 oriJson 으로 우선 테스트. 
							//UIFrameSpecialShop.AdvanceBillingPaymentPurchase(billingOrderID, productID, productInfoOnStore, oriJson
							//	, onSuccess: (p_rq, p_rs) =>
							//	{
							//		/// 성공시 실행할 로직 
							//		/// /// TODO : 임시 처리 팝업 
							//		OpenNotiUp(string.Format("{0} 을 획득하였습니다. 보관함을 확인하십시오.", productInfoOnStore.title), "확인");
							//	}
							//	, onError: (p_err, p_req, p_res) =>
							//	{
							//		/// 실패시 실행할 로직 
							//	});
						}
						else
						{
							/// TODO : 실패처리 
							ZLog.LogError(ZLogChannel.UI, "**** IAP Fail 02 (Native) ****");
							UIManager.Instance.ShowGlobalIndicator(false);
							OpenNotiUp("구매에 실패하였습니다.", "확인");
						}
				});
			}
			, (init_err, init_req, init_res) =>
			{
				/// TODO : 실패처리 
				ZLog.LogError(ZLogChannel.UI, "**** IAP Fail 01 (PaymentInit) **** ");

				UIManager.Instance.ShowGlobalIndicator(false);

				if (init_err == ZWebRequestClientBase.NetErrorType.WebRequest)
				{
					OpenErrorPopUp(ERROR.BILLING_INITIALIZE_FAIL);
				}
				else if (init_err == ZWebRequestClientBase.NetErrorType.Packet)
				{
					OpenErrorPopUp(init_res.ErrCode);
				}
			});
	}

	public void Close(bool release)
	{
		if (gameObject.activeSelf == false)
			return;

		gameObject.SetActive(false);
		if (release)
			Release();

		OnClosed?.Invoke();
	}

	public void Release()
	{
		IAPStatus = IAPStatusByFlow.None;
		OnPurchased_NonCash = null;
		OnBuyBtnClicked = null;
		OnClosed = null;
		Info = null;
		CashProductInfo = null;
		IAPProcessor = null;
		if (reqBuyInfo != null)
		{
			reqBuyInfo.Clear();
		}

		myCurrencyCountNonCash = 0;
		curBuyCnt = 0;
		singleCostCnt = 0;
		maxBuyCount = 0;
	}

	/// <summary>
	/// 구매 요청 . 구매가 가능한 상태인것이 체킹 된 후 호출해야함
	/// </summary>
	public void RequestBuy_NonCash(Action<ZWebRecvPacket, ResSpecialShopBuy> _onReceive = null)
	{
		// ljh : 획득연출 꺼줌, 다시키는건 가챠일경우 가챠프레임이 해줄것임
		if (UIManager.Instance.Find<UIGainSystem>(out var gainSystem))
			gainSystem.SetPlayState(false);

		var shopId = Info.specialShopTid;

		ZWebManager.Instance.WebGame.REQ_SpecialShopBuy(reqBuyInfo
		, (revPacket, resList) =>
		{
			var specialShopData = DBSpecialShop.Get(shopId);

			if (specialShopData.GoodsListGetType == E_GoodsListGetType.Rate)
			{// 가챠인것만 연출 시도
			 //todo_ljh : -----------급하게 빌드용으로 작성, 다시짜야됨
				List<uint> listTid = UIFrameGacha.GetGachaResultTidList(resList);

				UIGachaEnum.E_TimeLineType targetTimeline = UIGachaEnum.E_TimeLineType.None;
				UIGachaEnum.E_GachaStyle targetStyle = UIGachaEnum.E_GachaStyle.None;

				if (targetTimeline == UIGachaEnum.E_TimeLineType.None && resList.ResultGetPetsLength > 0)
				{
					bool isPet = false;
					isPet = DBPet.GetPetData(resList.ResultGetPets(0).Value.PetTid).PetType == E_PetType.Pet;
					targetStyle = isPet ? UIGachaEnum.E_GachaStyle.Pet : UIGachaEnum.E_GachaStyle.Ride;
					targetTimeline = UIFrameGacha.GetPossibleGachaTimeline(targetStyle, listTid);
				}

				if (targetTimeline == UIGachaEnum.E_TimeLineType.None && resList.ResultGetChangesLength > 0)
				{
					targetStyle = UIGachaEnum.E_GachaStyle.Class;
					targetTimeline = UIFrameGacha.GetPossibleGachaTimeline(UIGachaEnum.E_GachaStyle.Class, listTid);
				}

				if (targetTimeline == UIGachaEnum.E_TimeLineType.None && resList.ResultGetItemsLength > 0)
				{
					targetStyle = UIGachaEnum.E_GachaStyle.Item;
					targetTimeline = UIFrameGacha.GetPossibleGachaTimeline(UIGachaEnum.E_GachaStyle.Item, listTid);
				}

				if (targetTimeline != UIGachaEnum.E_TimeLineType.None)
				{
					UIManager.Instance.Open<UIFrameGacha>((str, frame) =>
					{
						frame.SetGachaData(targetStyle, targetTimeline, listTid, specialShopData.SpecialShopID);
					});

					OnPreDirecting?.Invoke();
				}
				else
				{
					gainSystem?.SetPlayState(true);

					OpenNotiUp("아이템 구매를 완료하였습니다", "알림");
				}
			}
			else
			{
				gainSystem?.SetPlayState(true);

				bool isGoodsItem = (specialShopData.GoodsListGetType == E_GoodsListGetType.None
				&& specialShopData.GoodsKindType == E_GoodsKindType.Item
				&& specialShopData.GoodsItemID > 0);
				bool isGoodsOnlyDiamond = isGoodsItem == false && specialShopData.DiamondCount > 0;

				if (isGoodsItem || isGoodsOnlyDiamond)
				{
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
				}
			}

			OnPurchased_NonCash?.Invoke(resList);
			_onReceive?.Invoke(revPacket, resList);
			Close(true);
		});
	}
	#region OnClick
	public void OnClickCloseBtn()
	{
		Close(true);
	}

	public void OnClickBuyBtn()
	{
		if (this.isBuyLimitExist)
		{
			if (curBuyCnt + myCurBuyLimitCnt > buyLimitCnt)
			{
				OpenNotiUp("더 이상 구매할 수 없습니다.", "알림");
				return;
			}
		}

		OnBuyBtnClicked?.Invoke(reqBuyInfo);
	}

	public void OnClickResetCountBtn()
	{
		this.curBuyCnt = minBuyCount;
		Refresh();
	}

	public void OnClickDecreaseCountOneBtn()
	{
		if (curBuyCnt > 0)
		{
			curBuyCnt--;
			Refresh();
		}
	}

	public void OnClickDecreaseCountMultiple01Btn()
	{
		if (curBuyCnt == 0)
			return;

		if (curBuyCnt >= desiredCount_decreaseMultiple01)
		{
			curBuyCnt -= (uint)desiredCount_decreaseMultiple01;
		}
		else
		{
			curBuyCnt = 0;
		}

		Refresh();
	}

	public void OnClickDecreaseCountMultiple02Btn()
	{
		if (curBuyCnt == 0)
			return;

		if (curBuyCnt >= desiredCount_decreaseMultiple02)
		{
			curBuyCnt -= (uint)desiredCount_decreaseMultiple02;
		}
		else
		{
			curBuyCnt = 0;
		}

		Refresh();
	}

	public void OnClickIncreaseCountOneBtn()
	{
		if (curBuyCnt == uint.MaxValue)
			return;

		if (curBuyCnt < maxBuyCount)
		{
			curBuyCnt++;
			Refresh();
		}
	}

	public void OnClickIncreaseCountMultiple01Btn()
	{
		if (curBuyCnt == uint.MaxValue)
			return;

		if (curBuyCnt + desiredCount_increaseMultiple01 > maxBuyCount)
		{
			curBuyCnt = maxBuyCount;
		}
		else
		{
			curBuyCnt += (uint)desiredCount_increaseMultiple01;
		}

		Refresh();
	}

	public void OnClickIncreaseCountMultiple02Btn()
	{
		if (curBuyCnt == uint.MaxValue)
			return;

		if (curBuyCnt + desiredCount_increaseMultiple02 > maxBuyCount)
		{
			curBuyCnt = maxBuyCount;
		}
		else
		{
			curBuyCnt += (uint)desiredCount_increaseMultiple02;
		}

		Refresh();
	}

	public void OnClickDetailBtn()
	{
		ZLog.LogError(ZLogChannel.UI, "no imple");
	}
	#endregion

	#region ETC
	//void HandleError(ZWebCommunicator.E_ErrorType _errorType, ZWebReqPacketBase _reqPacket, ZWebRecvPacket _recvPacket)
	//{
	//    OpenErrorPopUp(_recvPacket.ErrCode);
	//}

	void OpenErrorPopUp(ERROR errorCode, Action onConfirmed = null)
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

	#endregion
}