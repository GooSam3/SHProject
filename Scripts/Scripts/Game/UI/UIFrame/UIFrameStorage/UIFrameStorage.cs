using GameDB;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZDefine;
using ZNet.Data;

public class UIFrameStorage : ZUIFrameBase
{
    private enum E_StorageState
    {
        ToStorage = 1,
        ToInventory = 2
    }

    private enum E_FilterType
    {
        All = 1,
        Equip = 2,
        Stack = 3,
    }

    // 리스트 아이템 클릭시 
    // 처음누를때 : 창고상태 변경
    // 두번째부터 : 팝업노출
    // 세번째부터 : (같은아이템인가?) 대기열 1개씩 등록
    private enum E_InteractiveFlow
    {
        StateChanged = 1,
        ItemMoveReady = 2
    }

    #region # UI SerializeField

    // -- 아이템 정보 팝업 -- 
    [SerializeField] private UIPopupItemInfo infoPopup;

    // -- 스크롤러 -- 
    [SerializeField] private UIItemScrollAdapter scrollInventory;
    [SerializeField] private UIItemScrollAdapter scrollStorage;
    [SerializeField] private UIItemScrollAdapter scrollStorageQueue;// 대기열!!

    // -- 각 리스트 첫번째 라디오버튼 (초기화용) -- 
    [SerializeField] private ZToggle toggleFirstStorage;
    [SerializeField] private ZToggle toggleFirstInventory;

    // -- 대기열 부분 화살표 및 텍스트
    [SerializeField] private GameObject arrowToStorage;
    [SerializeField] private GameObject arrowToInventory;
    [SerializeField] private Text txtStorageState;

    // -- 대기열부분 인벤토리 무게 -- 
    [SerializeField] private Slider sliderInventory;
    [SerializeField] private Text txtInventoryAmount;
    [SerializeField] private Text txtWeightPenalty;

    // -- 창고 용량( n / n ) --
    [SerializeField] private Text txtStorageAmount;

    // -- 찾기 / 맡기기 버튼 텍스트-- 
    [SerializeField] private Text txtConfirm;

    // -- 찾기 가격 -- 
    [SerializeField] private GameObject objCurrency;
    [SerializeField] private Text txtCurrency;

    #endregion UI SerializedField #

    #region # Frame Data

    private List<ScrollItemData> listInventory = new List<ScrollItemData>();
    private List<ScrollItemData> listStorageQueue = new List<ScrollItemData>();
    private List<ScrollItemData> listStorage = new List<ScrollItemData>();

    private E_StorageState storageState = E_StorageState.ToStorage;

    private E_FilterType storageSortType = E_FilterType.All;
    private E_FilterType invenSortType = E_FilterType.All;

    private bool isOpenManualy = false;// 초기화시 onhide호출

    private ScrollItemData selectedData = null;

    private E_InteractiveFlow interactiveFlow = E_InteractiveFlow.StateChanged;

    // 장바구니에 완벽하게? 정상적으로? 보이는 슬롯 갯수, 읽기전용
    private int QUEUE_VIEWPORT_SLOTCOUNT = 0;

    private bool canConfirm;

    // 큐에 들어가있는 무게
    private float queueWeight = 0f;

    // 플레이어의 현재 무게
    private float playerWeight = 0f;

    private float TotalWeight
    {
        get
        {
            if (storageState == E_StorageState.ToInventory)
            {
                return (queueWeight + playerWeight);
            }
            else
                return playerWeight - queueWeight;
        }
    }

    private float RemainWeight => ZPawnManager.Instance.MyEntity.GetAbility(E_AbilityType.FINAL_MAX_WEIGH) - TotalWeight;
    public override bool IsBackable => true;
    #endregion Frame Data #

    public void Init(Action onEndInit = null)
    {
        ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIItemSlot), obj=>
        {
            Initialize();
            ZPoolManager.Instance.Return(obj);
            onEndInit?.Invoke();
        });
    }

    private void Initialize()
    {
        scrollInventory.Initialize(OnClickInvenItem, Me.CurCharData.InvenMaxCnt);
        scrollStorage.InitializeStorage(OnClickStorageItem,()=> RefreshStorageData(false), Me.CurUserData.GetStorageMaxCnt());
        // todo_ljh : DBConfig!!
        scrollStorageQueue.Initialize(OnClickStorageQueueItem, DBConfig.Max_Trade_Queue_Count);

        QUEUE_VIEWPORT_SLOTCOUNT = (int)scrollStorageQueue.GetViewportSize() / ((int)scrollStorageQueue.Parameters.DefaultItemSize + (int)scrollStorageQueue.Parameters.ContentSpacing);

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

        txtWeightPenalty.gameObject.SetActive(false);

        toggleFirstInventory.SelectToggle();
        toggleFirstStorage.SelectToggle();

        SetStorageState(E_StorageState.ToStorage);

        SetSelectedData(null);

        RefreshInventoryData();
        RefreshStorageData();

        base.OnShow(_LayerOrder);
    }

    protected override void OnHide()
    {
        // 아이템 자동사용 등 불의의 사고 대비용
        if (isOpenManualy)
        {
            ZNet.Data.Me.CurCharData.InvenUpdate -= OnInventoryUpdate;
            UIManager.Instance.Find<UIFrameHUD>().SetSubHudFrame();
            ZPawnManager.Instance.MyEntity.DoRemoveEventWeightUpdated(RefreshPlayerWeight);
        }
        isOpenManualy = false;
        base.OnHide();
    }

    private void OnInventoryUpdate(bool _setCheck = false)
    {
        // * 인벤토리 업데이트시
        // 인벤토리 먼저 갱신 후 대기열 비교
        // 대기열의 아이템 수는 아이템의 최대갯수를 넘을수 없음
        RefreshInventoryData();

        if (storageState == E_StorageState.ToStorage)
            RefreshStorageQueueData();
    }


    private void SetStorageState(E_StorageState state)
    {
        storageState = state;

        string stateTxt = string.Empty;

        switch (state)
        {
            case E_StorageState.ToStorage:
                stateTxt = DBLocale.GetText("Storage_Put_Label");
                objCurrency.gameObject.SetActive(false);
                break;

            case E_StorageState.ToInventory:
                stateTxt = DBLocale.GetText("Storage_Get_Label");
                objCurrency.gameObject.SetActive(true);
                break;
        }

        txtStorageState.text = stateTxt;

        arrowToStorage.SetActive(state == E_StorageState.ToStorage);
        arrowToInventory.SetActive(state == E_StorageState.ToInventory);

        interactiveFlow = E_InteractiveFlow.StateChanged;

        txtConfirm.text = state == E_StorageState.ToInventory ?
                                   DBLocale.GetText("Storage_Get_Label") :
                                   DBLocale.GetText("Storage_Put_Label");

        scrollStorageQueue.SetNormalizedPosition(0);
        RefreshStorageQueueData(true);

        //fx
    }

    // 실제 데이터는 건들이지않음
    private void AddStorageQueue(ScrollItemData data, long cnt)
    {
        // 실제 데이터 참조
        ScrollItemData target = listStorageQueue.Find((item) => item.Item.item_id == data.Item.item_id);

        if (target == null)
        {
            if (scrollStorageQueue.HasRemainCapacity() == false)
            {
                // 장바구니가 꽉 찼습니다
                UIMessagePopup.ShowPopupOk(DBLocale.GetText("Storage_Error_QueueOverFlow"));
                return;
            }

            ScrollItemData queueData = new ScrollItemData();
            queueData.Reset(data);

            queueData.Count = (ulong)cnt;

            // 방금들어온놈은 켜질일이없다
            queueData.IsSelected = false;

            listStorageQueue.Add(queueData);
        }
        else
        {
            long count = (long)target.Count + cnt;
            target.Count = (ulong)Mathf.Clamp(count, 0, target.Item.cnt);
        }
    }

    // 큐에서들어옴
    // 음수값도 가능(zitem의 cnt만큼)
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

    private void AddStorage(ulong id, long cnt)
    {
        ScrollItemData target = listStorage.Find((item) => item.Item.item_id == id);

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

    //---- refresh

    private void RefreshStorageQueueData(bool setEmpty = false)
    {
        if (setEmpty)
        {
            listStorageQueue.Clear();
        }
        else
        {
            int idx = 0;

            List<ScrollItemData> listData = null;
            switch (storageState)
            {
                case E_StorageState.ToStorage:
                    listData = listInventory;
                    break;
                case E_StorageState.ToInventory:
                    listData = listStorage;
                    break;
            }

            if (listData == null) return;

            while (idx < listStorageQueue.Count)
            {
                ScrollItemData data = listStorageQueue[idx];

                ScrollItemData target = listData.Find((item) => data.Item.item_id == item.Item.item_id);

                if (target == null || target.Item.cnt == 0 || data.Count == 0)// 인벤 / 큐에 존재하지 않는친구는 제거
                    listStorageQueue.Remove(data);
                else
                {
                    // 수량만 맞춰줌
                    if (data.Count > target.Item.cnt)
                        data.Count = target.Item.cnt;
                    idx++;
                }
            }
        }

        scrollStorageQueue.ResetData(listStorageQueue);

        if (scrollStorageQueue.LastIndex > QUEUE_VIEWPORT_SLOTCOUNT)
            scrollStorageQueue.SmoothScrollTo(scrollStorageQueue.LastIndex - QUEUE_VIEWPORT_SLOTCOUNT, .2f);

        RefreshQueueWeight();

        if (storageState == E_StorageState.ToInventory)
            RefreshResultCurrency();
    }

    private ScrollItemData GetStorageQueueData(ulong id)
    {
        return listStorageQueue.Find(item => item.Item.item_id == id);
    }

    private void RefreshStorageData(bool useLocalData = false)
    {
        if (useLocalData)
        {
            if (storageState == E_StorageState.ToInventory)
            {
                foreach (var iter in listStorage)
                {
                    uint queueCount = (uint)(GetStorageQueueData(iter.Item.item_id)?.Count ?? 0);
                    iter.Count = iter.Item.cnt - queueCount;
                }
            }
        }
        else
        {
            listStorage.Clear();

            foreach (var iter in Me.CurCharData.GetStorageData())
            {
                if (iter.cnt == 0)
                    continue;

                uint queueCount = (uint)(GetStorageQueueData(iter.item_id)?.Count ?? 0);
                ScrollItemData data = new ScrollItemData() { Item = iter, Count = iter.cnt - queueCount, isEmpty = false };

                if (selectedData != null && storageState == E_StorageState.ToInventory && selectedData.Item.item_id == iter.item_id)
                {
                    data.IsSelected = true;
                }

                listStorage.Add(data);
            }
        }

        txtStorageAmount.text = $"{listStorage.Count}/{Me.CurUserData.GetStorageMaxCnt()}";

        ResetScrolleByFilterType(scrollStorage, listStorage, storageSortType);
    }

    private void RefreshInventoryData(bool useLocalData = false)
    {
        if (useLocalData)// 갯수만 갱신
        {
            if (storageState == E_StorageState.ToStorage)
            {
                foreach (var iter in listInventory)
                {
                    uint queueCount = (uint)(GetStorageQueueData(iter.Item.item_id)?.Count ?? 0);
                    iter.Count = iter.Item.cnt - queueCount;
                }
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

                uint queueCount = (uint)(GetStorageQueueData(iter.item_id)?.Count ?? 0);
                long resultCnt = (long)iter.cnt - queueCount;
                if (resultCnt <= 0)
                    resultCnt = 0;

                ScrollItemData data = new ScrollItemData() { Item = iter, Count = (ulong)resultCnt, isEmpty = false };

                if (selectedData != null && storageState == E_StorageState.ToStorage && selectedData.Item.item_id == iter.item_id)
                {
                    data.IsSelected = true;
                }

                if(EnumHelper.CheckFlag(table.LimitType, E_LimitType.Storage) == false)
				{
                    data.IsInteractive = true;
                    data.InteractiveValue = true;
                }

                listInventory.Add(data);
            }
        }

        ResetScrolleByFilterType(scrollInventory, listInventory, invenSortType);
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

    //---- event

    private bool ProcFlowInteractive(ScrollItemData slot, Action<ScrollItemData> caller)
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

    private void SetSelectedData(ScrollItemData data, bool popupInteraction = false)
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
        scrollStorage.RefreshData();
        scrollStorageQueue.RefreshData();
    }

    private void OnClickStorageItem(ScrollItemData slot)
    {
        if (storageState != E_StorageState.ToInventory)
        {
            RefreshStorageQueueData(true);
            RefreshInventoryData(true);
            SetStorageState(E_StorageState.ToInventory);
        }

        if (ProcFlowInteractive(slot, OnClickStorageItem) == false)
            return;

        // 아이템이 부족하다면 리턴
        if (slot.Count <= 0)
        {
            UIMessagePopup.ShowPopupOk(DBLocale.GetText("Storage_OverCount_Action_Error_Message"));
            return;
        }

        if (DBItem.GetWeight(slot.Item.item_tid) > RemainWeight)
        {
            UIMessagePopup.ShowPopupOk(DBLocale.GetText("Storage_Error_OverFlow"));
            return;
        }

        // 큐에 넣고
        AddStorageQueue(slot, (long)slot.Count);

        // 창고에있던 아이템 0으로만들어줌(임시)
        AddStorage(slot.Item.item_id, -(long)slot.Count);

        RefreshStorageQueueData();
        RefreshStorageData(true);
    }

    private void OnClickStorageQueueItem(ScrollItemData slot)
    {
        interactiveFlow = E_InteractiveFlow.StateChanged;

        AddStorageQueue(slot, -(long)slot.Count);

        // 되돌림
        switch (storageState)
        {
            case E_StorageState.ToStorage:
                RefreshInventoryData(true);
                break;
            case E_StorageState.ToInventory:
                RefreshStorageData(true);
                break;
        }
        SetSelectedData(null);

        RefreshStorageQueueData();
    }

    private void OnClickInvenItem(ScrollItemData slot)
    {
        if (storageState != E_StorageState.ToStorage)
        {
            RefreshStorageQueueData(true);
            RefreshStorageData(true);
            SetStorageState(E_StorageState.ToStorage);
        }

        if (ProcFlowInteractive(slot, OnClickInvenItem) == false)
            return;

        // 맡길수 없는 아이템이라면 리턴
        if(DBItem.GetItem(slot.Item.item_tid, out var table) == false)
		{
            UIMessagePopup.ShowPopupOk(DBLocale.GetText("Storage_Error_Limit"));
            return;
        }
        // 맡길수 없는 아이템이라면 리턴
        if (EnumHelper.CheckFlag(table.LimitType, E_LimitType.Storage) == false)
		{
            UIMessagePopup.ShowPopupOk(DBLocale.GetText("Storage_Error_Limit"));
            return;
		}

        // 장착중이라면 리턴
        if (slot.Item.slot_idx > 0)
        {
            UIMessagePopup.ShowPopupOk(DBLocale.GetText("Storage_Error_Equiped"));
            return;
        }

        // 잠겨있다면 리턴
        if (slot.Item.IsLock)
        {
            UIMessagePopup.ShowPopupOk(DBLocale.GetText("STORAGE_EXTEND_ITEM_LOCK"));
            return;
        }

        // 아이템이 부족하다면 리턴
        if (slot.Count <= 0)
        {
            UIMessagePopup.ShowPopupOk(DBLocale.GetText("Storage_OverCount_Action_Error_Message"));
            return;
        }

        // 큐에 넣고
        AddStorageQueue(slot, (long)slot.Count);

        // 인벤토리에 있던 아이템 0으로 표시해줌
        AddInventory(slot.Item.item_id, -(long)slot.Count);

        RefreshStorageQueueData();
        RefreshInventoryData(true);
    }

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

        foreach (var iter in listStorageQueue)
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

    // 찾기시에만 들어오게
    private void RefreshResultCurrency()
    {
        ulong resultCurrency = 0;

        foreach (var iter in listStorageQueue)
        {
            if (DBItem.GetItem(iter.Item.item_tid, out Item_Table table) == false)
                continue;

            resultCurrency += table.StorageItemCount;
        }

        txtCurrency.text = resultCurrency.ToString("N0");

        ZItem currency = Me.CurCharData.GetItem(DBConfig.Gold_ID, NetItemType.TYPE_ACCOUNT_STACK);

        if (currency == null)
            canConfirm = false;
        else
            canConfirm = currency.cnt >= resultCurrency;

        txtCurrency.color = canConfirm ? Color.white : Color.red;
    }


    public void OnClickAddNum(int num)
    {
        if (selectedData == null)
        {
            // 선택된 아이템이 읎다
            UIMessagePopup.ShowPopupOk(DBLocale.GetText("Storage_Error_NotSelected"));
            return;
        }

        // 맡길수 없는 아이템이라면 리턴
        if (DBItem.GetItem(selectedData.Item.item_tid, out var table) == false ||
            EnumHelper.CheckFlag(table.LimitType, E_LimitType.Storage) == false)
        {
            UIMessagePopup.ShowPopupOk(DBLocale.GetText("Storage_Error_Limit"));
            return;
        }

        // 장착중이라면 리턴
        if (selectedData.Item.slot_idx > 0)
        {
            UIMessagePopup.ShowPopupOk(DBLocale.GetText("Storage_Error_Equiped"));
            return;
        }

        // 잠겨있다면 리턴
        if (selectedData.Item.IsLock)
        {
            UIMessagePopup.ShowPopupOk(DBLocale.GetText("STORAGE_EXTEND_ITEM_LOCK"));
            return;
        }

        // 아이템이 부족하다면 리턴
        if (selectedData.Count <= 0)
        {
            UIMessagePopup.ShowPopupOk(DBLocale.GetText("Storage_OverCount_Action_Error_Message"));
            return;
        }


        ulong count = (ulong)num;

        if (selectedData.Count < count)
            count = selectedData.Count;

        //무게 제한
        if (storageState == E_StorageState.ToInventory)
        {
            ulong existCount = 0;

            var existItem = listStorageQueue.Find(item => item.Item.item_tid == selectedData.Item.item_tid);

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

        AddStorageQueue(selectedData, (long)count);

        switch (storageState)
        {
            case E_StorageState.ToStorage:
                RefreshInventoryData(true);
                break;

            case E_StorageState.ToInventory:
                RefreshStorageData(true);
                break;
        }

        RefreshStorageQueueData();
    }

    public void OnClickAddAmountManualy()
    {
        if (selectedData == null)
        {
            // 선택된 아이템이 읎다
            UIMessagePopup.ShowPopupOk(DBLocale.GetText("Storage_Error_NotSelected"));

            return;
        }


        // 맡길수 없는 아이템이라면 리턴
        if (DBItem.GetItem(selectedData.Item.item_tid, out var table) == false ||
            EnumHelper.CheckFlag(table.LimitType, E_LimitType.Storage) == false)
        {
            UIMessagePopup.ShowPopupOk(DBLocale.GetText("Storage_Error_Limit"));
            return;
        }


        // 아이템이 부족하다면 리턴
        if (selectedData.Count <= 0)
        {
            UIMessagePopup.ShowPopupOk(DBLocale.GetText("Storage_OverCount_Action_Error_Message"));
            return;
        }


        ZPoolManager.Instance.Spawn<UIPopupInputAmount>(E_PoolType.UI, nameof(UIPopupInputAmount), (obj) =>
        {
            var maxValue = selectedData.Item.netType == NetItemType.TYPE_EQUIP?1:9999;

            obj.Initialize(transform, OnClickAddNum, maxValue);
        });
    }

    ///<seealso cref="E_FilterType"/>
    public void OnClickStorageSortType(int filterType)
    {
        storageSortType = (E_FilterType)filterType;
        RefreshStorageData(true);
    }

    public void OnClickInvenSortType(int filterType)
    {
        invenSortType = (E_FilterType)filterType;
        RefreshInventoryData(true);
    }

    public void OnClickConfirm()
    {
        if (listStorageQueue.Count <= 0)
        {
            // 찾거나 맡길 아이템 없음
            UIMessagePopup.ShowPopupOk(DBLocale.GetText("Storage_Error_EmptyQueue"));
            return;
        }

        if(storageState == E_StorageState.ToInventory&&canConfirm == false)// 돈없응~
        {
            UIMessagePopup.ShowPopupOk(DBLocale.GetText("Storage_Error_insufficient_Gold"));
            return;
        }

        if (TotalWeight >= ZPawnManager.Instance.MyEntity.GetAbility(E_AbilityType.FINAL_MAX_WEIGH))
        {
            // 이런.. 너무 무겁다..!
            UIMessagePopup.ShowPopupOk(DBLocale.GetText("Winven_Use_MaxWeight"));

            return;
        }

        switch (storageState)// 이대로 진행했을때 인벤/창고가 터지는가
        {
            case E_StorageState.ToInventory:
                if (scrollInventory.CanMerge(listStorageQueue, Me.CurCharData.GetShowInvenItems().Count) == false)
                {
                    UIMessagePopup.ShowPopupOk(DBLocale.GetText("Inven_Error_OverFlow"));
                    return;
                }
                break;

            case E_StorageState.ToStorage:
                if (scrollStorage.CanMerge(listStorageQueue, listStorage.Count) == false)
                {
                    UIMessagePopup.ShowPopupOk(DBLocale.GetText("Storage_Error_OverFlow"));
                    return;
                }
                break;
        }

        List<ZItem> listItemQueue = new List<ZItem>();

        string strItems = string.Empty;

        foreach (var iter in listStorageQueue)
        {
            ZItem item = new ZItem(iter.Item);
            item.cnt = iter.Count;
            listItemQueue.Add(item);

            if (DBItem.GetItem(item.item_tid, out var table))
                strItems += $"{UICommon.GetItemText(table)},";
        }

        if (strItems.Length > 0)
            strItems = strItems.Substring(0, strItems.Length - 1);

        SetSelectedData(null);

        // 인터렉션시 무게튀는현상 : web보다 mmo콜백이 먼저호출
        // 무게갱신은 mmo, 상점패킷은 web임
        // 갱신콜백 및 창고패킷콜백의 호출시점이 일정한 순서 및 타이밍에 오지 않는관계로
        // 일단 창고인터렉션시 장바구니 큐 비워버림
        RefreshStorageQueueData(true);

        switch (storageState)
        {
            case E_StorageState.ToStorage:
                ZWebManager.Instance.WebGame.REQ_StoragePut(listItemQueue, (recvPacket, recvMsgPacket) =>
                {
                    ZWebChatData.AddSystemMsg(DBLocale.GetText("Storage_Put_Message", strItems));

                    RefreshStorageQueueData(true);
                    RefreshStorageData();
                    RefreshInventoryData();
                });
                break;
            case E_StorageState.ToInventory:
                ZWebManager.Instance.WebGame.REQ_StorageGet(listItemQueue, (recvPacket, recvMsgPacket) =>
                {
                    ZWebChatData.AddSystemMsg(DBLocale.GetText("Storage_Get_Message", strItems));

                    RefreshStorageQueueData(true);
                    RefreshStorageData();
                    RefreshInventoryData();
                });
                break;
        }
    }
}