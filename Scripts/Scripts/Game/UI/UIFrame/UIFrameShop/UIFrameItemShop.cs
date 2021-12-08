using GameDB;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZDefine;
using ZNet.Data;

// 아이템 상점
// 잡화상점과 스킬상점 이 해당 스크립트 공유함
// ? : 모든 프로세스가 같음, 상점에 진열된 아이템 타입만 다름
// 잡화상점은 구매제한 없음(주간 or 일간 등등)
public class UIFrameItemShop : ZUIFrameBase
{
    public enum E_ShopFrameType
    {
        Item = 1,   // 잡화상점
        Skill = 2   // 스킬상점
    }

    private enum E_ShopState
    {
        Buy = 1,
        Sell = 2
    }

    private enum E_FilterType
    {
        All = 1,
        Equip = 2,
        Stack = 3,
    }

    // 리스트 아이템 클릭시 
    // 처음누를때 : 상점상태 변경 / 구매, 판매, 팝업노출
    // 그다음 : (같은아이템인가?) 대기열 1개씩 등록
    private enum E_InteractiveFlow
    {
        StateChanged = 1,
        ItemMoveReady = 2
    }

    #region # UI SerializeField

    // -- 아이템 정보 팝업 -- 
    [SerializeField] private UIPopupItemInfo infoPopup;

    // -- 스크롤러 -- 
    [SerializeField] private UIItemShopScrollAdapter scrollItemShop;
    [SerializeField] private UIItemScrollAdapter scrollInventory;
    [SerializeField] private UIItemScrollAdapter scrollItemShopQueue;// 대기열!!
    [SerializeField] private UIItemShopScrollAdapter scrollAutoBuy;

    // -- 자동주문 관련 --
    [SerializeField] private GameObject objAutoBuy;
    [SerializeField] private GameObject objAutoBuyBlock;
    // 자동주문 무게제한까지 버튼텍스트
    [SerializeField] private Text txtAutoBuyMidTerm;
    [SerializeField] private Text txtAutoBuyMaxTerm;

    // -- 인벤 리스트 첫번째 라디오버튼 (초기화용) -- 
    [SerializeField] private ZToggle toggleFirstInven;

    // -- 대기열 부분 화살표 및 텍스트
    [SerializeField] private GameObject arrowSell;
    [SerializeField] private GameObject arrowBuy;
    [SerializeField] private Text txtShopState;

    // -- 대기열부분 인벤토리 무게 -- 
    [SerializeField] private Slider sliderInventory;
    [SerializeField] private Text txtInventoryAmount;
    [SerializeField] private Text txtWeightPenalty;

    // -- 판매 / 구매 버튼 텍스트-- 
    [SerializeField] private Text txtConfirm;

    // -- 판매 / 구매 가격 -- 
    [SerializeField] private Text txtCurrency;

    // -- 상점 이름 -- 
    [SerializeField] private Text txtShopTitle;

    // -- 자동주문 그룹 -- // 스킬상점시에 비활성화됨
    [SerializeField] private List<GameObject> listObjectAutoBuy = new List<GameObject>();

    // -- 잡화상점, 인터렉션 토글할 녀석들 -- 
    [SerializeField] private List<ZButton> listObjectInteractive = new List<ZButton>();
    #endregion UI SerialzieField #

    #region # FrameData
    // 0: shopid
    const string AUTOBUY_KEY_FORMAT = "AutoBuy_{0}";

    private List<ScrollShopItemData> listItemShop = new List<ScrollShopItemData>();
    private List<ScrollItemData> listInventory = new List<ScrollItemData>();
    private List<ScrollShopItemData> listItemShopQueue = new List<ScrollShopItemData>();

    private E_ShopFrameType shopType = E_ShopFrameType.Item;

    private E_ShopState shopState = E_ShopState.Buy;

    private E_FilterType invenSortType = E_FilterType.All;

    private bool isOpenManualy = false;// 초기화시 onhide호출

    private ScrollShopItemData selectedData = null;

    private E_InteractiveFlow interactiveFlow = E_InteractiveFlow.StateChanged;

    private bool isViewAutoBuy = false;

    // 사거나 팔수있는지 여부(재화비교)
    private bool canConfirm = false;

    // 장바구니에 완벽하게? 정상적으로? 보이는 슬롯 갯수, 읽기전용
    private int QUEUE_VIEWPORT_SLOTCOUNT = 0;

    // 큐에 들어가있는 무게
    private float queueWeight = 0f;

    // 플레이어의 현재 무게
    private float playerWeight = 0f;
    public override bool IsBackable => true;
    private float TotalWeight
    {
        get
        {
            if (shopState == E_ShopState.Buy)
            {
                return (queueWeight + playerWeight);
            }
            else
                return playerWeight - queueWeight;
        }
    }

    private float RemainWeight => ZPawnManager.Instance.MyEntity.GetAbility(E_AbilityType.FINAL_MAX_WEIGH) - TotalWeight;


    #endregion FrameData #

    public void Init(Action onEndInit = null)
    {
        ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIItemSlot), obj=>
        {
            ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIShopItemSlot), objShop=>
              {
                  Initialize();
                  ZPoolManager.Instance.Return(obj);
                  ZPoolManager.Instance.Return(objShop);
                  onEndInit?.Invoke();
              });
        });
    }

    private void Initialize()
    {
        scrollInventory.Initialize(OnClickInvenItem, Me.CurCharData.InvenMaxCnt);
        scrollInventory.CastListType<ScrollShopItemData>();

        scrollItemShop.Initialize(OnClickShopItem, false);
        scrollAutoBuy.Initialize(null, true);

        scrollItemShopQueue.Initialize(OnClickShopQueueItem, DBConfig.Max_Trade_Queue_Count);
        scrollItemShopQueue.CastListType<ScrollShopItemData>();

        QUEUE_VIEWPORT_SLOTCOUNT = (int)scrollItemShopQueue.GetViewportSize() / ((int)scrollItemShopQueue.Parameters.DefaultItemSize + (int)scrollItemShopQueue.Parameters.ContentSpacing);

        infoPopup.SetStorageOnClose(() => SetSelectedData(null, true));
    }

    protected override void OnShow(int _LayerOrder)
    {
        isOpenManualy = true;

        UIManager.Instance.Find<UIFrameHUD>().SetSubHudFrame(E_UIStyle.FullScreen);

        ZNet.Data.Me.CurCharData.InvenUpdate += OnInventoryUpdate;
        ZPawnManager.Instance.MyEntity.DoAddEventWeightUpdated(RefreshPlayerWeight);
        RefreshPlayerWeight();
        infoPopup.gameObject.SetActive(false);

        txtAutoBuyMidTerm.text = DBLocale.GetText("NormalShop_BuyWeightPenalty", DBConfig.Weight_Penalty_MidTerm.ToString("F1"));
        txtAutoBuyMaxTerm.text = DBLocale.GetText("NormalShop_BuyWeightPenalty", DBConfig.Weight_Penalty_MaxTerm.ToString("F1"));

        toggleFirstInven.SelectToggle(true);

        SetShopState(E_ShopState.Buy);

        SetSelectedData(null);

        SetAutoBuy(false);

        RefreshInventoryData();

        base.OnShow(_LayerOrder);
    }

    protected override void OnHide()
    {
        if (isOpenManualy)
        {
            ZNet.Data.Me.CurCharData.InvenUpdate -= OnInventoryUpdate;
            UIManager.Instance.Find<UIFrameHUD>().SetSubHudFrame();
            ZPawnManager.Instance.MyEntity.DoRemoveEventWeightUpdated(RefreshPlayerWeight);

        }
        isOpenManualy = false;
        base.OnHide();
    }

    public void SetShopType(E_ShopFrameType type)
    {
        shopType = type;

        string title = string.Empty;

        switch (shopType)
        {
            case E_ShopFrameType.Item:
                title = "잡화 상점";
                break;

            case E_ShopFrameType.Skill:
                title = "스킬북 상점";
                break;
        }

        txtShopTitle.text = title;

        listObjectAutoBuy.ForEach(item => item.SetActive(shopType == E_ShopFrameType.Item));

        SetShopData();
    }

    private void OnInventoryUpdate(bool _setCheck = false)
    {
        RefreshInventoryData();

        if (shopState == E_ShopState.Sell)
            RefreshShopQueueData();
    }

    private void SetShopState(E_ShopState state)
    {
        shopState = state;

        string stateTxt = string.Empty;

        switch (state)
        {
            case E_ShopState.Buy:
                stateTxt = DBLocale.GetText("Buy_Text");
                break;

            case E_ShopState.Sell:
                stateTxt = DBLocale.GetText("Sell_Text");
                break;
        }

        txtShopState.text = stateTxt;

        arrowBuy.SetActive(state == E_ShopState.Buy);
        arrowSell.SetActive(state == E_ShopState.Sell);

        interactiveFlow = E_InteractiveFlow.StateChanged;

        txtConfirm.text = stateTxt;

        if (shopType == E_ShopFrameType.Item)
        {
            listObjectInteractive.ForEach(item => item.interactable = state == E_ShopState.Buy);
        }

        scrollItemShopQueue.SetNormalizedPosition(0);

        RefreshShopQueueData(true);
    }

    // -- get --

    private ScrollItemData GetShopQueueData(ulong id)
    {
        return listItemShopQueue.Find(item => item.Item.item_id == id); ;
    }

    // -- set -- 
    private void AddShopQueue(ScrollShopItemData data, long cnt, ulong customClamp = 0)
    {
        // 실제 데이터 참조
        ScrollItemData target = listItemShopQueue.Find((item) => item.Item.item_id == data.Item.item_id);

        ulong maxCnt = 0;
        // clamp
        if (shopState == E_ShopState.Sell)
        {// 팔때는 내 아이템 갯수만큼만
            maxCnt = data.Item.cnt;
        }
        else
        {// 살때는 최대 구매 갯수만큼
            if (customClamp > 0)
                maxCnt = customClamp;
            else
                maxCnt = data.tableData.BuyLimitCount;
        }

        if (target == null)
        {
            if (scrollItemShopQueue.HasRemainCapacity() == false)
            {
                // 장바구니가 꽉 찼습니다
                UIMessagePopup.ShowPopupOk(DBLocale.GetText("Storage_Error_QueueOverFlow"));
                return;
            }

            ScrollShopItemData queueData = new ScrollShopItemData();
            queueData.Reset(data);

            if (shopState == E_ShopState.Buy)
            {
                queueData.shopItemTid = data.shopItemTid;
                queueData.tableData = data.tableData;
            }
            queueData.Count = (ulong)Mathf.Clamp(cnt, 0, maxCnt);

            // 방금들어온놈은 켜질일이없다
            queueData.IsSelected = false;
            queueData.isEmpty = false;

            listItemShopQueue.Add(queueData);
        }
        else
        {
            long count = (long)target.Count + cnt;

            target.Count = (ulong)Mathf.Clamp(count, 0, maxCnt);
        }
    }

    private void AddInventory(ulong id, long cnt)
    {
        // 없을수가없다

        ScrollItemData target = listInventory.Find((item) => item.Item.item_id == id);

        if (target == null)
        {
            return;
        }
        else
        {
            long count = (long)target.Count + cnt;

            target.Count = (ulong)Mathf.Clamp(count, 0, target.Item.cnt);
        }
    }

    // -- refresh -- 

    // 처음 한번만 들어온다
    // 자동구매 리스트까지 갱신
    private void SetShopData()
    {
        listItemShop.Clear();

        List<NormalShop_Table> listShopData = new List<NormalShop_Table>();

        switch (shopType)
        {
            case E_ShopFrameType.Item:
                listShopData = DBNormalShop.GetShopDataList(E_ShopType.Normal, E_NormalShopType.General);
                break;
            case E_ShopFrameType.Skill:
                listShopData = DBNormalShop.GetShopDataList(E_ShopType.Normal, E_NormalShopType.SkillBook);
                break;
        }

        foreach (var iter in listShopData)
        {
            if (iter.StateType != E_StateOutputType.Output) continue;

            if (iter.UnusedType != E_UnusedType.Use) continue;
            // 샵아이템은 zitem이 없는관계로 임시로생성
            // id == tid;
            ScrollShopItemData data = new ScrollShopItemData() { shopItemTid = iter.NormalShopID, tableData = iter, Item = new ZItem() { item_id = iter.GoodsItemID, item_tid = iter.GoodsItemID } };

            listItemShop.Add(data);
        }

        listItemShop.Sort((a, b) => a.tableData.PositionNumber.CompareTo(b.tableData.PositionNumber));

        scrollItemShop.ResetData(listItemShop);
        scrollAutoBuy.ResetData(listItemShop);
        scrollItemShop.SetNormalizedPosition(1);
        scrollAutoBuy.SetNormalizedPosition(1);
    }

    private void RefreshInventoryData(bool useLocalData = false)
    {
        if (useLocalData)
        {
            foreach (var iter in listInventory)
            {
                if (shopState == E_ShopState.Sell)
                {
                    uint queueCount = (uint)(GetShopQueueData(iter.Item.item_id)?.Count ?? 0);
                    iter.Count = iter.Item.cnt - queueCount;

                }

                if (selectedData != null && shopState == E_ShopState.Sell && selectedData.Item.item_id == iter.Item.item_id)
                {
                    iter.IsSelected = true;
                }
                else
                    iter.IsSelected = false;

                iter.IsInteractive = shopState == E_ShopState.Sell;
                iter.InteractiveValue = (EnumHelper.CheckFlag(DBItem.GetItem(iter.Item.item_tid).LimitType, E_LimitType.Store) == false);
            }
        }
        else
        {
            listInventory.Clear();

            foreach (var iter in Me.CurCharData.GetShowInvenItems())
            {
                if (DBItem.GetItem(iter.item_tid, out Item_Table table) == false)
                    continue;

                if (iter.cnt == 0)
                    continue;

                uint queueCount = (uint)(GetShopQueueData(iter.item_id)?.Count ?? 0);
                ScrollShopItemData data = new ScrollShopItemData() { Item = iter, Count = iter.cnt - queueCount, isEmpty = false };

                if (selectedData != null && shopState == E_ShopState.Sell && selectedData.Item.item_id == iter.item_id)
                {
                    data.IsSelected = true;
                }

                data.IsInteractive = shopState == E_ShopState.Sell;
                data.InteractiveValue = (EnumHelper.CheckFlag(table.LimitType, E_LimitType.Store) == false);

                listInventory.Add(data);
            }

            listInventory.Sort((a, b) =>
            {
                if (a.InteractiveValue && b.InteractiveValue)
                    return 0;

                if (!a.InteractiveValue && b.InteractiveValue)
                    return -1;

                return 1;
            });


        }

        ResetScrolleByFilterType(scrollInventory, listInventory, invenSortType);
    }

    private void RefreshShopQueueData(bool isClear = false)
    {
        if (isClear)
        {
            listItemShopQueue.Clear();
        }
        else
        {
            int idx = 0;

            // 팔기(인벤토리 참조작업)는 인벤토리와 수량 맞춰줌
            if (shopState == E_ShopState.Sell)
            {
                while (idx < listItemShopQueue.Count)
                {
                    ScrollItemData data = listItemShopQueue[idx];

                    ScrollItemData target = listInventory.Find(item => data.Item.item_id == item.Item.item_id);

                    // 인벤 / 큐에 존재하지 않는 데이터 제거
                    if (target == null || target.Item.cnt == 0 || data.Count == 0)
                        listItemShopQueue.Remove(data as ScrollShopItemData);
                    else
                    {
                        // 팔수있는 수량 맞춰줌
                        if (data.Count > target.Item.cnt)
                            data.Count = target.Item.cnt;
                        idx++;
                    }
                }
            }
            else// 샵아이템은 무제한인관계로 수량0일시 제거만해줌
            {
                while (idx < listItemShopQueue.Count)
                {
                    ScrollItemData data = listItemShopQueue[idx];

                    if (data.Count <= 0)
                        listItemShopQueue.Remove(data as ScrollShopItemData);
                    else
                        idx++;
                }

            }

        }

        scrollItemShopQueue.ResetFixedListData(listItemShopQueue);

        if (scrollItemShopQueue.LastIndex > QUEUE_VIEWPORT_SLOTCOUNT)
            scrollItemShopQueue.SmoothScrollTo((scrollItemShopQueue.LastIndex - QUEUE_VIEWPORT_SLOTCOUNT), .2f);

        RefreshQueueWeight();
        RefreshResultCurrency();
    }

    private void ResetScrolleByFilterType(UIItemScrollAdapter adapter, List<ScrollItemData> list, E_FilterType filterType)
    {

        if (filterType == E_FilterType.All)
        {
            adapter.ResetData(list);
            return;
        }

        NetItemType type = ConvertToNetType(filterType);

        if (type == NetItemType.TYPE_ACCOUNT_STACK)
        {
            adapter.ResetData(list);
            return;
        }

        adapter.ResetData(list.FindAll((item) => item.Item.netType == type));

    }

    private NetItemType ConvertToNetType(E_FilterType type)
    {
        switch (type)
        {
            case E_FilterType.Equip:
                return NetItemType.TYPE_EQUIP;
            case E_FilterType.Stack:
                return NetItemType.TYPE_STACK;
        }

        return NetItemType.TYPE_ACCOUNT_STACK;
    }

    // -- weight -- 

    private void RefreshWeight()
    {
        float weightAmount = TotalWeight / ZPawnManager.Instance.MyEntity.GetAbility(E_AbilityType.FINAL_MAX_WEIGH);

        sliderInventory.value = weightAmount;

        bool isPanelty = DBWeightPenalty.TryGetPaneltyData(weightAmount * 100f, out WeightPenalty_Table table);

        txtWeightPenalty.gameObject.SetActive(isPanelty);

        string hexColor = UICommon.HEX_COLOR_DEFAULT;

        if (isPanelty)
        {
            hexColor = table.WeightPenaltyColor;
            txtWeightPenalty.text = UICommon.GetColoredText(table.WeightPenaltyColor, DBLocale.GetText(table.SimplePenaltyTip));
        }
        txtInventoryAmount.text = UICommon.GetColoredText(hexColor, $"{(weightAmount * 100f).ToString("F2")}%");
    }

    private void RefreshQueueWeight()
    {
        float tempWeight = 0f;

        foreach (var iter in listItemShopQueue)
        {
            tempWeight += DBItem.GetWeight(iter.Item.item_tid) * iter.Count;
        }

        queueWeight = tempWeight;

        RefreshWeight();
    }

    private void RefreshPlayerWeight()
    {
        playerWeight = ZPawnManager.Instance.MyEntity.GetAbility(E_AbilityType.ITEM_WEIGH);

        RefreshWeight();
    }

    // -- event --

    public void SetAutoBuy(bool state)
    {
        isViewAutoBuy = state;

        objAutoBuy.SetActive(isViewAutoBuy);
        objAutoBuyBlock.SetActive(isViewAutoBuy);

        if (isViewAutoBuy)
        {
            scrollAutoBuy.SetNormalizedPosition(1);
            SetSelectedData(null);
        }
    }

    public void OnClickAddNum(int num)
    {
        if (selectedData == null)
        {
            // 선택된 아이템이 읎다
            UIMessagePopup.ShowPopupOk(DBLocale.GetText("Storage_Error_NotSelected"));
            return;
        }

        ulong count = (ulong)num;

        if (shopState == E_ShopState.Sell)
        {
            if (EnumHelper.CheckFlag(DBItem.GetItem(selectedData.Item.item_tid).LimitType, E_LimitType.Store) == false)
            {//못파는놈
                UIMessagePopup.ShowPopupOk(DBLocale.GetText("ITEM_CANNOT_SELL"));
                return;
            }

            // 아이템이 부족하다면 리턴
            if (selectedData.Count <= 0)
            {
                UIMessagePopup.ShowPopupOk(DBLocale.GetText("Storage_OverCount_Action_Error_Message"));
                return;
            }

            // 아이템갯수만큼
            if (selectedData.Count < count)
                count = selectedData.Count;

        }
        else// 구매
        {
            // 한번에 살수있는 수량 넘어감
            if (selectedData.Count >= selectedData.tableData.BuyLimitCount)
            {
                UIMessagePopup.ShowPopupOk(DBLocale.GetText("Storage_OverCount_Action_Error_Message"));
                return;
            }

            ulong existCount = 0;

            var existItem = listItemShopQueue.Find(item => item.Item.item_tid == selectedData.Item.item_tid);

            if (existItem != null)
            {
                existCount = existItem.Count;
            }

            var remainWeight = ZPawnManager.Instance.MyEntity.GetAbility(E_AbilityType.FINAL_MAX_WEIGH) - TotalWeight;

            float itemWeight = DBItem.GetWeight(selectedData.Item.item_tid);

            if (itemWeight > 0 && remainWeight < itemWeight * count)
            {
                float tempCount = remainWeight / itemWeight;

                count = (ulong)Mathf.FloorToInt(tempCount);

            }

            if (count <= 0)
                return;

        }
        AddShopQueue(selectedData, (long)count);

        if (shopState == E_ShopState.Sell)
        {
            RefreshInventoryData(true);
        }

        RefreshShopQueueData();
    }

    private void RefreshResultCurrency()
    {
        ulong resultCurrency = 0;

        foreach (var iter in listItemShopQueue)
        {
            if (shopState == E_ShopState.Buy)
            {
                resultCurrency += (iter as ScrollShopItemData).tableData.BuyItemCount * iter.Count;
            }
            else
            {
                if (DBItem.GetItem(iter.Item.item_tid, out Item_Table table) == false)
                    continue;

                resultCurrency += table.SellItemCount * iter.Count;
            }
        }

        txtCurrency.text = resultCurrency.ToString("N0");

        if (shopState == E_ShopState.Buy)
        {
            ZItem currency = Me.CurCharData.GetItem(DBConfig.Gold_ID, NetItemType.TYPE_ACCOUNT_STACK);

            if (currency == null)
                canConfirm = false;
            else
                canConfirm = currency.cnt >= resultCurrency;

        }
        else
            canConfirm = true;

        txtCurrency.color = canConfirm ? Color.white : Color.red;
    }

    public void OnClickAddAmountManualy()
    {
        if (selectedData == null)
        {
            // 선택된 아이템이 읎다
            UIMessagePopup.ShowPopupOk(DBLocale.GetText("Storage_Error_NotSelected"));

            return;
        }
        // 아이템이 부족하다면 리턴
        if (shopState == E_ShopState.Sell && selectedData.Count <= 0)
        {
            UIMessagePopup.ShowPopupOk(DBLocale.GetText("Storage_OverCount_Action_Error_Message"));
            return;
        }

        ZPoolManager.Instance.Spawn<UIPopupInputAmount>(E_PoolType.UI, nameof(UIPopupInputAmount), (obj) =>
        {
            obj.Initialize(transform, OnClickAddNum);
        });
    }

    public void OnClickInvenSortType(int filterType)
    {
        selectedData = null;
        invenSortType = (E_FilterType)filterType;
        RefreshInventoryData(true);
    }

    private bool ProcFlowInteractive(ScrollShopItemData slot, Action<ScrollShopItemData> caller)
    {
        switch (interactiveFlow)
        {
            case E_InteractiveFlow.StateChanged:
                infoPopup.Initialize(E_ItemPopupType.Storage, slot.Item);

                if (infoPopup.gameObject.activeSelf == false)
                {
                    infoPopup.gameObject.SetActive(true);
                }

                SetSelectedData(slot);

                interactiveFlow = E_InteractiveFlow.ItemMoveReady;
                return false;

            case E_InteractiveFlow.ItemMoveReady:
                break;
        }

        if (selectedData == null || selectedData.Item.item_id != slot.Item.item_id)
        {
            SetSelectedData(slot);
            interactiveFlow = E_InteractiveFlow.StateChanged;
            caller.Invoke(slot);
            return false;
        }

        return true;
    }

    private void SetSelectedData(ScrollShopItemData data, bool popupInteraction = false)
    {
        if (selectedData != null)
            selectedData.IsSelected = false;

        selectedData = data;

        if (selectedData == null)
        {
            if (popupInteraction == false)
            {
                infoPopup.CloseStorageInfo();
            }
        }
        else
        {
            selectedData.IsSelected = true;
        }

        scrollInventory.RefreshData();
        scrollItemShopQueue.RefreshData();
        scrollItemShop.RefreshData();
    }

    private void OnClickInvenItem(ScrollItemData data)
    {
        if (shopState != E_ShopState.Sell)
        {
            SetShopState(E_ShopState.Sell);
            RefreshInventoryData(true);
        }

        if (ProcFlowInteractive(data as ScrollShopItemData, OnClickInvenItem) == false)
            return;

        //보이긴하는데 비활성화된애들(판매불가)
        if (data.IsInteractive && data.InteractiveValue)
        {
            UIMessagePopup.ShowPopupOk(DBLocale.GetText("ITEM_CANNOT_SELL"));
            return;
        }


        // 장착중이라면 리턴
        if (data.Item.slot_idx > 0)
        {
            UIMessagePopup.ShowPopupOk(DBLocale.GetText("Storage_Error_Equiped"));
            return;
        }

        // 잠겨있다면 리턴
        if (data.Item.IsLock)
        {
            UIMessagePopup.ShowPopupOk(DBLocale.GetText("STORAGE_EXTEND_ITEM_LOCK"));
            return;
        }

        // 아이템이 부족하다면 리턴
        if (data.Count <= 0)
        {
            UIMessagePopup.ShowPopupOk(DBLocale.GetText("Storage_OverCount_Action_Error_Message"));
            return;
        }

        // 큐에 넣고
        AddShopQueue(data as ScrollShopItemData, 1);

        // 인벤토리에 있던 아이템 0으로 표시해줌
        AddInventory(data.Item.item_id, -1);

        RefreshShopQueueData();
        RefreshInventoryData(true);

    }

    private void OnClickShopItem(ScrollShopItemData data)
    {
        if (shopState != E_ShopState.Buy)
        {
            SetShopState(E_ShopState.Buy);
            RefreshInventoryData();
        }

        if (ProcFlowInteractive(data, OnClickShopItem) == false)
            return;

        //if(DBItem.GetItem(data.tid, out var table))

        //    inven

        ////예외처리~~
        //if (DBItem.GetWeight(data.Item.item_tid) > RemainWeight)
        //{
        //    UIMessagePopup.ShowPopupOk(DBLocale.GetText("Inven_Error_OverFlow"));
        //    return;
        //}

        // 일단 누르면 1개씩추가~~
        AddShopQueue(data, 1);

        RefreshShopQueueData();
    }

    private void OnClickShopQueueItem(ScrollItemData data)
    {
        interactiveFlow = E_InteractiveFlow.StateChanged;

        AddShopQueue(data as ScrollShopItemData, -(long)data.Count);

        if (shopState == E_ShopState.Sell)
        {
            RefreshInventoryData(true);
        }

        SetSelectedData(null);

        RefreshShopQueueData();
    }

    private List<ScrollShopItemData> GetAutoBuyList()
    {
        List<ScrollShopItemData> autoBuyList = new List<ScrollShopItemData>();

        foreach (var iter in listItemShop)
        {
            int value = DeviceSaveDatas.LoadCurCharData(string.Format(AUTOBUY_KEY_FORMAT, iter.tableData.NormalShopID), 0);

            if (value <= 0)
                continue;

            ScrollShopItemData data = new ScrollShopItemData();
            data.Reset(iter);
            data.Count = (ulong)value;

            autoBuyList.Add(data);
        }

        return autoBuyList;
    }

    public void OnClickAutoBuy()
    {
        scrollItemShopQueue.SetNormalizedPosition(0);
        // 큐 비워줌
        RefreshShopQueueData(true);

        List<ScrollShopItemData> autoBuyList = GetAutoBuyList();

        float maxWeight = ZPawnManager.Instance.MyEntity.GetAbility(E_AbilityType.FINAL_MAX_WEIGH);

        float curWeight = TotalWeight;

        foreach (var iter in autoBuyList)
        {
            float itemWeight = DBItem.GetWeight(iter.Item.item_tid);

            float itemTotalWeight = curWeight + itemWeight * iter.Count;

            var existItem = listItemShopQueue.Find(item => item.Item.item_tid == iter.Item.item_tid);

            // 더이상 안들어감
            if (itemTotalWeight > maxWeight)
            {
                // 가능한 갯수까지만
                int count = Mathf.FloorToInt((maxWeight - curWeight) / itemWeight);

                // 큐에 넣고
                AddShopQueue(iter, count);

                // 그만!
                break;
            }

            curWeight += itemWeight * iter.Count;
            AddShopQueue(iter, (long)iter.Count, iter.Count);
        }

        RefreshShopQueueData();
    }

    // 0 : mid
    // 1 : max
    public void OnClickWeightPanelty(int i)
    {
        if (selectedData == null)
        {
            // 선택된 아이템이 읎다
            UIMessagePopup.ShowPopupOk(DBLocale.GetText("Storage_Error_NotSelected"));
            return;
        }

        float targetValue = i == 0 ? DBConfig.Weight_Penalty_MidTerm : DBConfig.Weight_Penalty_MaxTerm;

        float maxWeight = ZPawnManager.Instance.MyEntity.GetAbility(E_AbilityType.FINAL_MAX_WEIGH);

        float targetWeight = maxWeight * (targetValue * .01f);

        targetWeight -= TotalWeight;

        if (targetWeight < 0)
            return;

        float itemWeight = DBItem.GetWeight(selectedData.tableData.GoodsItemID);
        uint count = selectedData.tableData.BuyLimitCount;// 일단 최대로 초기화

        // 무게 있는놈
        if (Mathf.Approximately(itemWeight, 0f) == false)
        {
            // 만일의 상황을 대비하여 내려버림
            count =  (uint)Mathf.FloorToInt(targetWeight / itemWeight);
        }

        AddShopQueue(selectedData, count);

        RefreshShopQueueData();
    }

    public void OnClickConfirm()
    {
        if (listItemShopQueue.Count <= 0)
        {
            // 찾거나 맡길 아이템 없음

            UIMessagePopup.ShowPopupOk(DBLocale.GetText("NormalShop_Error_Empty"));
            return;
        }

        if (shopState == E_ShopState.Buy &&
           scrollInventory.CanMerge(listItemShopQueue, listInventory.Count+1) == false)
        {
            UIMessagePopup.ShowPopupOk(DBLocale.GetText("Inven_Error_OverFlow"));
            return;
        }

        if (TotalWeight >= ZPawnManager.Instance.MyEntity.GetAbility(E_AbilityType.FINAL_MAX_WEIGH))
        {
            // 이런.. 너무 무겁다..!
            UIMessagePopup.ShowPopupOk(DBLocale.GetText("Winven_Use_MaxWeight"));

            return;
        }

        if (!canConfirm)//골드 부족
        {
            UIMessagePopup.ShowPopupOk(DBLocale.GetText("Storage_Error_insufficient_Gold"));
            return;
        }

        SetSelectedData(null);

        switch (shopState)
        {
            case E_ShopState.Sell:
                List<ZItem> listItemQueue = new List<ZItem>();

                foreach (var iter in listItemShopQueue)
                {
                    ZItem item = new ZItem(iter.Item);
                    item.cnt = iter.Count;
                    listItemQueue.Add(item);
                }

                // 인터렉션시 무게튀는현상 : web보다 mmo콜백이 먼저호출
                // 무게갱신은 mmo, 상점패킷은 web임
                // 갱신콜백 및 상점패킷콜백의 호출시점이 일정한 순서 및 타이밍에 오지 않는관계로
                // 일단 상점인터렉션시 장바구니 큐 비워버림
                RefreshShopQueueData(true);

                ZWebManager.Instance.WebGame.REQ_SellItem(listItemQueue, (recvPacket, recvMsgPacket) =>
                {
                    RefreshShopQueueData(true);
                    if (UIManager.Instance.Find(out UISubHUDQuickSlot _quick)) _quick.Initialize();
                    if (UIManager.Instance.Find(out UISubHUDCharacterAction _action)) _action.SetPotionInfo();

                    UICommon.SetNoticeMessage(DBLocale.GetText("Sale_Of_Goods"), Color.white, 1f, UIMessageNoticeEnum.E_MessageType.SubNotice);
                }, (error, a, b) => print(error));

                break;
            case E_ShopState.Buy:

                List<ScrollShopItemData> listBuyData = new List<ScrollShopItemData>();

                foreach (var iter in listItemShopQueue)
                {
                    listBuyData.Add(iter as ScrollShopItemData);
                }

                // 인터렉션시 무게튀는현상 : web보다 mmo콜백이 먼저호출
                // 무게갱신은 mmo, 상점패킷은 web임
                // 갱신콜백 및 상점패킷콜백의 호출시점이 일정한 순서 및 타이밍에 오지 않는관계로
                // 일단 상점인터렉션시 장바구니 큐 비워버림
                RefreshShopQueueData(true);

                ZWebManager.Instance.WebGame.REQ_BuyItems(false, listBuyData.ToArray(), (recvPacket, recvMsgPacket) =>
                {
                    RefreshShopQueueData(true);
                    if (UIManager.Instance.Find(out UISubHUDQuickSlot _quick)) _quick.Initialize();
                    if (UIManager.Instance.Find(out UISubHUDCharacterAction _action)) _action.SetPotionInfo();

                    UICommon.SetNoticeMessage(DBLocale.GetText("Purchase_Of_Goods"), Color.white, 1f, UIMessageNoticeEnum.E_MessageType.SubNotice);

                }, (error, a, b) => print(error));

                break;
        }
    }

}
