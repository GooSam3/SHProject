using GameDB;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZDefine;
using ZNet.Data;

public class UIFrameTrade : ZUIFrameBase
{
    #region UI Variable
    [SerializeField] private ZToggle[] TopTab;
    [SerializeField] private ZToggle[] SubTab;
    [SerializeField] private GameObject[] TradeMenu = new GameObject[ZUIConstant.TRADE_TOP_MENU_COUNT];
    public UITradeScrollAdapter ScrollAdapter;
    [SerializeField] private UITradeSettlementAdapter SettlementScrollAdapter;
    public UITradeListUpScrollAdapter ListUpScrollAdapter;
    public UITradeListUpInvenScrollAdapter ListUpInvenScrollAdapter;
    public InputField SearchBoard;
    public uint SearchGroupId;
    public GameObject SearchSubMenu;
    [SerializeField] private GameObject SearchHistory = null;
    public UITradeSearchHistoryListItem SearchRecently = null;
    [SerializeField] private UITradeSearchHistoryListItem[] SearchHistoryArray = new UITradeSearchHistoryListItem[ZUIConstant.TRADE_SEARCH_HISTORY_COUNT];
    public ZButton SearchEnhance = null;
    [SerializeField] private GameObject ContentListObj; // Main Tab
    [SerializeField] private GameObject DetailListObj;  // Sub Tab

    // 정산 내역
    [SerializeField] private ZButton SettlementBtn = null;
    [SerializeField] private Text TotalSettlementValueLog = null;      // 정산 완료 액
    [SerializeField] private Text TotalSellValueLog = null;            // 판매 건 수

    // 정산
    [SerializeField] private Text TotalSettlementValue = null;      // 정산 완료 액
    [SerializeField] private Text TotalSellValue = null;            // 판매 건 수

    // 판매 등록
    [SerializeField] private Text RegisterSale = null;  // 판매 등록
    [SerializeField] private Text SalesFee = null;      // 판매 수수료
    [SerializeField] private Text ToalSaleValue = null; // 판매 총액

    public UITradePopupSellInfo SellInfoPopup = null;
    public UITradePopupBuyAllInfo BuyAllInfoPopup = null;

    // 판매 아이템 정보    
    [SerializeField] private UIPopupItemInfo InfoPopup = null;

    // 검색 조건
    public UITradePopupSearchInfo SearchDetailInfoPopup = null;
    #endregion

    #region System Variable
    public E_TradeMenu CurTradeMenu = E_TradeMenu.Search;
    public E_TradeTapType CurTradeSearchMainTab = E_TradeTapType.None;
    public E_TradeSubTapType CurTradeSearchSubTab = E_TradeSubTapType.None;
    public E_InvenSortType CurSearchInvenType = E_InvenSortType.All;
    public List<ExchangeItemData> MultiBuyList = new List<ExchangeItemData>();
    [SerializeField] private List<GameObject> SearchSubMenulist = new List<GameObject>();
    [SerializeField] private ScrollRect SearchSubMenuScrollRect;
    public override bool IsBackable => true;
    private bool Isinit = false;

    #endregion

    protected override void OnInitialize()
    {
        base.OnInitialize();

        ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UITradeViewsHolder), delegate
        {
            ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UITradeSettlementViewsHolder), delegate
            {
                ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UITradeListUpViewsHolder), delegate
                {
                    ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UITradeListUpInvenViewsHolder), delegate
                    {
                        ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UITradeSubMenuListItem), delegate {
                            ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UITradeBuyAllInfoListItem), delegate
                            {
                                Isinit = true;

                                TopTab[(int)E_TradeMenu.Search].SelectToggleAction((ZToggle _toggle) => {
                                    OnSelectSearchMenu((int)E_TradeMenu.Search);
                                });
                            }); 
                        });
                    });
                });
            });
        });
    }

    protected override void OnShow(int _LayerOrder)
	{
		base.OnShow(_LayerOrder);

		if (UIManager.Instance.Find(out UIFrameHUD _hud))
			_hud.SetSubHudFrame(E_UIStyle.FullScreen);

        if (!Isinit)
            return;

        TopTab[(int)E_TradeMenu.Search].SelectToggleAction((ZToggle _toggle) => {
            OnSelectSearchMenu((int)E_TradeMenu.Search);
        });
    }

    protected override void OnHide()
    {
        base.OnHide();

        if (UIManager.Instance.Find(out UIFrameHUD _hud)) 
            _hud.SetSubHudFrame();

        SearchDetailInfoPopup.Initialize(false);

        SellInfoPopup.ClickClose();
        BuyAllInfoPopup.ClickConfirm();

        SearchBoard.text = string.Empty;

        ClearMultiBuyList();

        if(InfoPopup != null)
		{
            Destroy(InfoPopup);
            InfoPopup = null;
		}
    }

	protected override void OnRemove()
	{
		base.OnRemove();

        ZPoolManager.Instance.Clear(E_PoolType.UI, nameof(UITradeViewsHolder));
        ZPoolManager.Instance.Clear(E_PoolType.UI, nameof(UITradeSettlementViewsHolder));
        ZPoolManager.Instance.Clear(E_PoolType.UI, nameof(UITradeListUpViewsHolder));
        ZPoolManager.Instance.Clear(E_PoolType.UI, nameof(UITradeListUpInvenViewsHolder));
        ZPoolManager.Instance.Clear(E_PoolType.UI, nameof(UITradeSubMenuListItem));
    }

	private void SetAdapter()
	{
        if (!Isinit)
            return;

        ScrollAdapter.gameObject.SetActive(CurTradeMenu == E_TradeMenu.Search);
        SettlementScrollAdapter.gameObject.SetActive(CurTradeMenu == E_TradeMenu.Settlement || CurTradeMenu == E_TradeMenu.SettlementLog);

        switch (CurTradeMenu)
		{
            case E_TradeMenu.Search:
                ScrollAdapter.SetData(E_TradeSearchMainType.Main);
                break;
            case E_TradeMenu.Sell:
                ClearMultiBuyList();
                OnSelectSearchInvenType((int)CurSearchInvenType);
                break;
            case E_TradeMenu.Settlement:
                SettlementScrollAdapter.SetData(CurTradeMenu == E_TradeMenu.Settlement ? UITradeSettlementAdapter.E_SettlementType.Settlement : UITradeSettlementAdapter.E_SettlementType.Settlement, delegate {
                    SettlementBtn.interactable = SettlementScrollAdapter.Data.List.Count != 0;
                });
                
                break;
            case E_TradeMenu.SettlementLog:
                SettlementScrollAdapter.SetData(CurTradeMenu == E_TradeMenu.Settlement ? UITradeSettlementAdapter.E_SettlementType.Log : UITradeSettlementAdapter.E_SettlementType.Log);
                break;
		}
    }

    public void OnClickSearch()
    {
        ScrollAdapter.SetData(ScrollAdapter.CurType, ScrollAdapter.CurType == E_TradeSearchMainType.Detail ? ScrollAdapter.CurSearchItemId : 0);
    }

    public void OnClickSearchInputField()
    {
        //SearchBoard.text = string.Empty;

        ActiveSearchHistory(true);
    }

    public void ActiveSearchHistory(bool _active)
    {
        SearchHistory.SetActive(_active);

        if(_active)
        {
            if (SearchBoard.text.Length < 1)
                return;

            DBItem.GetAllItem(out var table);
            List<uint> historyCnt = new List<uint>();
            for (int i = 0; i < table.Count; i++)
            {
                string itemName = DBLocale.GetText(table[i].ItemTextID);

                if (historyCnt.Count >= ZUIConstant.TRADE_SEARCH_HISTORY_COUNT)
                    return;

                if (itemName.Length < SearchBoard.text.Length)
                    continue;

                if (table[i].BelongType == E_BelongType.Belong)
                    continue;

                if ((table[i].TradeSubTapType == CurTradeSearchSubTab || CurTradeSearchSubTab == E_TradeSubTapType.None)&&
                   itemName.Substring(0, SearchBoard.text.Length) == SearchBoard.text &&
                    historyCnt.Find(item => item == table[i].ViewGroupID) == 0)
                {
                    historyCnt.Add(table[i].ViewGroupID);
                    SearchHistoryArray[historyCnt.Count - 1].Initialize(DBLocale.GetText(table[i].ItemTextID), table[i].GroupID);
                    continue;
                }
            }
            
            if (historyCnt.Count < ZUIConstant.TRADE_SEARCH_HISTORY_COUNT)
                for (int i = historyCnt.Count; i < ZUIConstant.TRADE_SEARCH_HISTORY_COUNT; i++)
                    SearchHistoryArray[i].gameObject.SetActive(false);
        }
        else
            for (int i = 0; i < SearchHistoryArray.Length; i++)
                SearchHistoryArray[i].gameObject.SetActive(false);
    }

    public void OnRefreshSearchInfo()
    {
        SearchBoard.text = string.Empty;

        switch(ScrollAdapter.CurType)
        {
            case E_TradeSearchMainType.Main: SearchDetailInfoPopup.OnRefresh(); break;
        }
    }

    /// <summary> 처음 목록 버튼 콜백 </summary>
    public void OnClickFirstlist()
    {
        CurTradeMenu = E_TradeMenu.Search;
        CurTradeSearchMainTab = E_TradeTapType.None;
        OnSelectTradeMenu((int)CurTradeMenu);
    }

    /// <summary> 이전 목록 버튼 콜백 </summary>
    public void OnClickPreviouslist()
    {
        ScrollAdapter.SetData(E_TradeSearchMainType.Main);
    }

    public void OnClickSearchEnhanceButton()
    {
        SearchDetailInfoPopup.Initialize(true);
    }

    public void ActiveSearchOptionButton(E_TradeSearchMainType _type)
    {
        SearchEnhance.gameObject.SetActive(_type == E_TradeSearchMainType.Detail);
    }

    /// <summary> 모두 구매 버튼 콜백 </summary>
    public void OnClickBuyAll()
    {
        uint totalPrice = 0;
        for(int i = 0; i < MultiBuyList.Count; i++)
            totalPrice += MultiBuyList[i].ItemTotalPrice;
        
        if (totalPrice > Me.CurUserData.Cash)
        {
            UICommon.OpenSystemPopup((UIPopupSystem _popup) => {
                _popup.Open(ZUIString.WARRING, DBLocale.GetText("재화가 부족합니다."), new string[] { ZUIString.LOCALE_OK_BUTTON }, new Action[] { delegate {
                    _popup.Close(); } });
            });
            return;
        }

        uint totalAllItemPrice = 0;
        for (int i = 0; i < MultiBuyList.Count; i++)
            totalAllItemPrice += MultiBuyList[i].ItemTotalPrice;

        if (MultiBuyList.Count == 0)
        {
            UICommon.OpenSystemPopup((UIPopupSystem _popup) => {
                _popup.Open(ZUIString.WARRING, DBLocale.GetText("구매 아이템을 선택해주세요."), new string[] { ZUIString.LOCALE_OK_BUTTON }, new Action[] { delegate {
                    _popup.Close(); } });
            });
            return;
        }

        UICommon.OpenSystemPopup((UIPopupSystem _popupBuy) => {
            _popupBuy.Open(ZUIString.WARRING, string.Format(DBLocale.GetText("Exchange_Purchase_Message"), DBLocale.GetText(DBItem.GetItem(MultiBuyList[0].ItemTId).ItemTextID), totalAllItemPrice), new string[] { ZUIString.LOCALE_CANCEL_BUTTON, ZUIString.LOCALE_OK_BUTTON }, new Action[] { delegate{ _popupBuy.Close(); }, delegate {

                    _popupBuy.Close();

                    BuyAllInfoPopup.Initialize(MultiBuyList);

                    ClearMultiBuyList();
            } });
        }); 
    }

    /// <summary>검색 리스트(메인) 새로고침 버튼 콜백 </summary>
    public void OnClickRefreshSearchList()
    {
        ScrollAdapter.SetData(E_TradeSearchMainType.Main);
    }

    /// <summary> 정산 버튼 콜백 </summary>
    public void OnClickSettleCost()
    {
        if (CurTradeMenu != E_TradeMenu.Settlement)
            return;

        List<ulong> idList = new List<ulong>();

        for (int i = 0; i < SettlementScrollAdapter.Data.List.Count; i++)
        {
            idList.Add(SettlementScrollAdapter.Data.List[i].Data.TransactionId);
        }

        if(idList.Count == 0)
            return;
		
		ZWebManager.Instance.WebGame.REQ_ExchangeTakeMoney(idList, (recvPacket, recvPacketMsg) =>
        {
            SetAdapter();
        });
    }

    /// <summary>검색 리스트(상세) 새로고침 버튼 콜백 </summary>
    public void OnClickRefresh()
    {
        ScrollAdapter.SetData(E_TradeSearchMainType.Detail, ScrollAdapter.CurSearchItemId);
    }

    public void OnSelectTradeMenu(int _idx)
	{
        CurTradeMenu = (E_TradeMenu)_idx;

        for (int i = 0; i < TradeMenu.Length; i++)
            TradeMenu[i].SetActive((int)CurTradeMenu == i);

        if (CurTradeMenu == E_TradeMenu.Search && CurTradeSearchMainTab == E_TradeTapType.None)
            OnSelectSearchMenu((int)CurTradeSearchMainTab);
        else
            SetAdapter();

        ActiveSearchHistory(false);
    }

    public void OnSelectSearchMenu(int _idx)
    {
        CurTradeSearchMainTab = (E_TradeTapType)_idx;

        SearchSubMenu.SetActive(CurTradeSearchMainTab != E_TradeTapType.None);

        // 전체 탭일 경우
        if (CurTradeSearchMainTab == E_TradeTapType.None)
		{
            CurTradeSearchSubTab = E_TradeSubTapType.None;
            SetAdapter();
        }

        SetSearchSubMenuList();

        ActiveSearchHistory(false);
    }

    public void SetSubMenuToggleChangeValue()
    {
        // 예외적으로 서브 메뉴는 중복 선택이 가능해야함
        for (int i = 0; i < SubTab.Length; i++)
            SubTab[i].onValueChanged.SetPersistentListenerState(0, UnityEngine.Events.UnityEventCallState.RuntimeOnly);
    }

    public void OnSelectSearchInvenType(int _idx)
    {
        CurSearchInvenType = (E_InvenSortType)_idx;

        ListUpScrollAdapter.SetData();
        ListUpInvenScrollAdapter.SetData(CurSearchInvenType, delegate { ListUpInvenScrollAdapter.RefreshData(); });
    }

    public void RefreshTextData(E_TradeMenu _mentType)
	{
        switch(_mentType)
		{
            case E_TradeMenu.Search:
                ContentListObj.SetActive(ScrollAdapter.CurType == E_TradeSearchMainType.Main);
                DetailListObj.SetActive(ScrollAdapter.CurType == E_TradeSearchMainType.Detail);
                break;

            case E_TradeMenu.Sell:
                uint priceSell = 0;
                for (int i = 0; i < ListUpScrollAdapter.Data.List.Count; i++)
                    priceSell += ListUpScrollAdapter.Data.List[i].Data.ItemTotalPrice;

                SalesFee.text = DBConfig.Exchange_Sell_Commission.ToString() + "%";
                ToalSaleValue.text = priceSell.ToString();
                RegisterSale.text = ListUpScrollAdapter.Data.Count.ToString() + "/" + DBConfig.Exchange_SellRegister_Max.ToString();
                break;

            case E_TradeMenu.Settlement:
                uint priceSettlement = 0;
                for (int i = 0; i < SettlementScrollAdapter.Data.List.Count; i++)
                    priceSettlement += SettlementScrollAdapter.Data.List[i].Data.ItemTotalPrice;

                TotalSettlementValue.text = priceSettlement.ToString(); 
                TotalSellValue.text = SettlementScrollAdapter.Data.List.Count.ToString() + "건";        
                break;

            case E_TradeMenu.SettlementLog:
                uint priceSettlementLog = 0;
                for (int i = 0; i < SettlementScrollAdapter.Data.List.Count; i++)
                    priceSettlementLog += SettlementScrollAdapter.Data.List[i].Data.ItemTotalPrice;

                TotalSettlementValueLog.text = priceSettlementLog.ToString();
                TotalSellValueLog.text = SettlementScrollAdapter.Data.List.Count.ToString() + "건";
                break;
		}
	}

    public void ActiveSubSearchMenuList(bool _active)
    {
        SearchSubMenu.SetActive(_active);

        if(!_active)
            SetSubMenuToggleChangeValue();
    }

    private void ClaerSearchSubMenuList()
	{
        for (int i = 0; i < SearchSubMenulist.Count; i++)
            Destroy(SearchSubMenulist[i].gameObject);

        SearchSubMenulist.Clear();
    }

    public void OnClickActiveHistory(bool _active)
    {
        ActiveSearchHistory(_active);
    }

    private void SetSearchSubMenuList()
	{
        ClaerSearchSubMenuList();

        for(int i = 0; i < Enum.GetNames(typeof(E_TradeSubTapType)).Length; i++)
		{
            int enumValue = (int)Enum.GetValues(typeof(E_TradeSubTapType)).GetValue(i);

            // to do : 기획에서 서브 탭 조건을 심플하게 하기 위해 메인 탭 기준 100 범위 내 처리하는 방식으로 요청
            if (enumValue > (int)CurTradeSearchMainTab * 100 &&
               enumValue < (int)CurTradeSearchMainTab * 100 + 101)
            {
                UITradeSubMenuListItem obj = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(UITradeSubMenuListItem)).GetComponent<UITradeSubMenuListItem>();

                if (obj != null)
                {
                    obj.transform.SetParent(SearchSubMenuScrollRect.content, false);
                    obj.Initialize((E_TradeSubTapType)enumValue);
                    SearchSubMenulist.Add(obj.gameObject);
                }
            }
        }
    }

    public void OnBuyItem(ExchangeItemData _item)
	{
        if (_item == null)
            return;

        ZWebManager.Instance.WebGame.REQ_BuyExchangeItem(_item, (recvPacket, recvPacketMsg) => {

            if (recvPacket.ErrCode == WebNet.ERROR.NO_ERROR)
            {
                for (int i = 0; i < ScrollAdapter.Data.List.Count; i++)
                    ScrollAdapter.Data.List[i].IsCheck = false;

                ScrollAdapter.SetData(E_TradeSearchMainType.Detail, ScrollAdapter.CurSearchItemId);
            }
        });
    }

    private void ClearMultiBuyList()
	{
        MultiBuyList.Clear();
	}

    public void CheckMultiBuyList(ExchangeItemData _data)
	{
        var data = MultiBuyList.Count != 0 ? MultiBuyList.Find(item => item.ExchangeID == _data.ExchangeID) : null;

        if(data != null)
		{
            MultiBuyList.Remove(data);
        }
        else
		{
            if(MultiBuyList.Count >= ZUIConstant.TRADE_MULTI_BUY_COUNT)
			{
                UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
                {
                    _popup.Open(ZUIString.LOCALE_OK_BUTTON, DBLocale.GetText("최대 5개까지 한 번에 구매 가능합니다."),
                    new string[] { ZUIString.LOCALE_OK_BUTTON },
                    new Action[] { delegate { _popup.Close(); } });
                });
                return;
			}

            MultiBuyList.Add(_data);
        }
	}

    public bool GetInfoPopup()
	{
        return InfoPopup != null ? true : false;
	}

    public void SetInfoPopup(UIPopupItemInfo _obj)
    {
        InfoPopup = _obj;
    }

    public void RemoveInfoPopup()
    {
        if (InfoPopup != null)
        {
            Destroy(InfoPopup.gameObject);
            InfoPopup = null;
        }
    }

    public void Close()
    {
        UIManager.Instance.Close<UIFrameTrade>();
    }
}
