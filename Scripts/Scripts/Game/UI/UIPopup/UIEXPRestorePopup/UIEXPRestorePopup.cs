using GameDB;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WebNet;
using ZDefine;
using ZNet;
using ZNet.Data;

// TODO : 
// 에러코드 처리해야함 e.g. 복구 실패 , 기간만료 등 . 
public class UIEXPRestorePopup : ZUIFrameBase
{
    // TODO : Enum 위치 프로젝트에 맞게 옮겨야할 수 있음 
    public enum CostType
    {
        Free,
        Gold,
        Diamond
    }

    #region UI Variable
    [SerializeField] private UIRestoreExpDetailSlot slotSource;
    [SerializeField] private RectTransform slotHolder;

    [SerializeField] private RectTransform btnRestoreForFree;
    [SerializeField] private RectTransform btnRestoreByGold;
    [SerializeField] private RectTransform btnRestoreByDiamond;
    [SerializeField] private RectTransform txtAlarm;
    [SerializeField] private Text txtFree;
    [SerializeField] private Text txtGoldPrice;
    [SerializeField] private Text txtDiamondPrice;
    #endregion

    #region System Variable
    private bool init = false;
    // 슬롯들 , 데이터만큼만 생성해놓고 그 뒤로 변하는 데이터들에 대해선 active 로만 관리
    private List<UIRestoreExpDetailSlot> slotsPooled;
    // 현재 선택된 슬롯 UI Element. 주의할점은 같은 Element 지만 data 가 달라질수 있음 . 
    private UIRestoreExpDetailSlot selectedSlotUIElement;
    // 슬롯은 데이터로 식별해야함 . ui 는 세팅해준 데이터따라 달라질수있기 때문.
    private RestoreExpData selectedSlotData;
    // 시간만료로 슬롯이 중간에 사라지거나 할수도있으니 별도로 req 보낼 값 restoreID 캡쳐함 
    ulong queryPopupRestoreID;
    Coroutine timeCheckCoroutine;
    uint goldPrice;
    uint diamondPrice;
    #endregion

    #region Public Method 
    public void Init()
    {
        if (init)
            return;

        slotsPooled = new List<UIRestoreExpDetailSlot>();

        if (slotSource.gameObject.activeInHierarchy)
            slotSource.gameObject.SetActive(false);

        init = true;
    }

    #endregion

    IEnumerator TimeCheck()
    {
        while (true)
        {
            bool invalidSlotFound = false;
            bool closePopUp = false;

            foreach (var slot in slotsPooled)
            {
                if (slot.gameObject.activeInHierarchy)
                {
                    slot.UpdateUI();

                    if (slot.IsValid == false)
                    {
                        if (selectedSlotData?.RestoreId == slot.data?.RestoreId)
                            closePopUp = true;

                        invalidSlotFound = true;
                    }
                }
            }

            if (invalidSlotFound)
                RefreshSlots();

            if (closePopUp)
                CloseQueryPopUp();

            yield return null;
        }
    }

    #region Overrides
    protected override void OnShow(int _LayerOrder)
    {
        base.OnShow(_LayerOrder);

        if (init == false) Init();

        UpdatePriceData();
        RefreshSlots();

        if (slotsPooled.Exists(t => t.gameObject.activeInHierarchy))
            timeCheckCoroutine = StartCoroutine(TimeCheck());
    }

    protected override void OnHide()
    {
        if (init == false)
            Init();

        base.OnHide();
        Release();
    }
    #endregion

    void Release()
    {
        if (timeCheckCoroutine != null)
            StopCoroutine(timeCheckCoroutine);

        selectedSlotUIElement = null;
        selectedSlotData = null;

        SetSlotActiveRecursively(false);
    }

    void RefreshSlotPoolByData()
    {
        var data = Me.CurCharData.GetRestoreExpData();

        if (data.Count > slotsPooled.Count)
        {
            int createCnt = data.Count - slotsPooled.Count;

            for (int i = 0; i < createCnt; i++)
            {
                GenerateSlot();
            }
        }

        for (int i = 0; i < data.Count; i++)
        {
            slotsPooled[i].Setup(data[i], SelectSlot);
        }

        /*   TODO: 소팅 여부 기획확인 필요
               slotsPooled.Sort((t1, t2) =>
               {
                   if (t1.data == null ||
                   t2.data == null)
                       return 1;
               });

               for (int i = 0; i < data.Count; i++)
               {
                   slotsPooled[i].transform.SetSiblingIndex(i);
               } */

        SetSlotActiveRecursively(true, data.Count);
    }

    // -1 = 전체
    void SetSlotActiveRecursively(bool active, int count = -1)
    {
        if (count == -1)
            slotsPooled.ForEach(t => t.gameObject.SetActive(active));
        else
        {
            for (int i = 0; i < slotsPooled.Count; i++)
            {
                if (i < count)
                {
                    slotsPooled[i].gameObject.SetActive(active);
                }
                else slotsPooled[i].gameObject.SetActive(false);
            }
        }
    }

    void RefreshSlots()
    {
        RefreshSlotPoolByData();

        if (selectedSlotData != null)
            SelectSlot(null, false);

        UpdateBtnsAndAlarm();
    }

    void GenerateSlot()
    {
        var newSlot = Instantiate<UIRestoreExpDetailSlot>(slotSource, slotHolder);
        newSlot.gameObject.SetActive(false);
        slotsPooled.Add(newSlot);
    }

    void UpdatePriceData()
    {
        // 기획안 : 사망 회수만큼 기본 비용 x 2 
        // 유료복구 회수 카운트를 사망 회수로 계산 
        int deathCnt = Me.CurCharData.RestoreExpNotFreeCnt;
        uint summedGoldPrice = DBConfig.DeathPenalty_Gold_Count;
        uint maxGold = DBConfig.DeathPenalty_Max_Gold;

        for (int i = 0; i < deathCnt; i++)
            summedGoldPrice *= 2;

        if (summedGoldPrice >= maxGold)
            summedGoldPrice = maxGold;

        goldPrice = summedGoldPrice;
        diamondPrice = DBConfig.DeathPenalty_Diamond_Count;
    }

    // 하단 위치 UI 업데이트 
    void UpdateBtnsAndAlarm()
    {
        bool forFreeActive = false;
        bool byGoldActive = false;
        bool byDiamondActive = false;
        bool alarmActive = false;

        if (selectedSlotUIElement == null)
        {
            alarmActive = true;
        }
        else
        {
            int freeCnt = Me.CurCharData.RestoreExpCnt;

            if (freeCnt > 0)
            {
                forFreeActive = true;
                txtFree.text = string.Format(DBLocale.GetText("WRecorver_Free_NowCount"), freeCnt);
            }
            else
            {
                byGoldActive = true;
                byDiamondActive = true;

                txtGoldPrice.text = goldPrice.ToString();
                // 다이아는 고정 
                txtDiamondPrice.text = diamondPrice.ToString();
            }
        }

        btnRestoreForFree.gameObject.SetActive(forFreeActive);
        btnRestoreByGold.gameObject.SetActive(byGoldActive);
        btnRestoreByDiamond.gameObject.SetActive(byDiamondActive);
        txtAlarm.gameObject.SetActive(alarmActive);
    }

    void SelectSlot(UIRestoreExpDetailSlot selectedSlot, bool updateBtn = true)
    {
        selectedSlotUIElement = selectedSlot;

        if (selectedSlot == null)
            selectedSlotData = null;
        else selectedSlotData = selectedSlot.data;

        foreach (var slot in slotsPooled)
        {
            if (slot.gameObject.activeInHierarchy)
            {
                slot.SetSelection(slot == selectedSlot);
            }
        }

        if (updateBtn)
            UpdateBtnsAndAlarm();
    }

    void OpenPopup(CostType costType, uint price)
    {
        string titleKey = string.Empty;
        ulong useItemID = 0; // 0 : 무료
        uint useItemTid = 0;
        bool open = true;

        switch (costType)
        {
            case CostType.Free:
                titleKey = "WRecorver_ExpRecorver_FreeNotice";
                useItemID = 0;
                useItemTid = 0;
                break;
            case CostType.Gold:
                {
                    titleKey = "WRecorver_ExpRecorver_PayNotice";
                    var targetItem = Me.CurCharData.GetInvenItemUsingMaterial(DBConfig.Gold_ID);

                    if (targetItem == null)
                        ZLog.LogError(ZLogChannel.Default, "Gold_ID (" + DBConfig.Gold_ID.ToString() + ") 가 ZItem 에 존재하지않습니다");

                    if (targetItem != null)
                    {
                        useItemID = targetItem.item_id;
                        useItemTid = targetItem.item_tid;
                    }
                    else open = false;
                }
                break;
            case CostType.Diamond:
                {
                    titleKey = "WRecorver_ExpRecorver_PayNotice";
                    useItemID = 0;
                    useItemTid = DBConfig.Diamond_ID;
                }
                break;
            default:
                break;
        }

        if (open)
        {
            if (selectedSlotData != null)
                queryPopupRestoreID = selectedSlotData.RestoreId;
            else queryPopupRestoreID = 0;

            UIManager.Instance.Open<UIEXPRestoreQueryPopup>(
                    (str, popup) => popup.Open(
                        DBLocale.GetText(titleKey)
                        , costType
                        , price
                        , DBLocale.GetText("OK_Button")
                        , _onConfirm:
                        () =>
                        {
                            ProgressRestore(useItemID, useItemTid);
                        }));
        }
    }

    void CloseQueryPopUp()
    {
        if (UIManager.Instance.Find<UIEXPRestoreQueryPopup>())
            UIManager.Instance.Close<UIEXPRestoreQueryPopup>();
    }

    // 재화가 충분한지 체크 
    bool CheckCurrencyEnough(CostType costType)
    {
        switch (costType)
        {
            case CostType.Free:
                return true;
            case CostType.Gold:
                return ConditionHelper.CheckCompareCost(DBConfig.Gold_ID, goldPrice);
            case CostType.Diamond:
                return ConditionHelper.CheckCompareCost(DBConfig.Diamond_ID, diamondPrice);
            default:
                ZLog.LogError(ZLogChannel.UI, "Price Type not matching");
                return false;
        }
    }

    void ProgressRestore(ulong useItemID, uint useItemTid)
    {
        ZWebManager.Instance.WebGame.REQ_Restore(queryPopupRestoreID, useItemID, useItemTid
            , _onReceive:
            (revPacket, resList) =>
        {
            // 경험치 복구 리스트를 업데이트하기 위해 한번더 보냄 
            ZWebManager.Instance.WebGame.REQ_RestoreExpList((_revPacket, _resList) =>
            {
                OnRestoreDone();
            }, (ZWebCommunicator.E_ErrorType _errorType, ZWebReqPacketBase _reqPacket, ZWebRecvPacket _recvPacket) =>
            {
                OnRestoreDone();
                HandleError(_errorType, _reqPacket, _recvPacket);
            });
        }
        , HandleError);
    }

    void OnRestoreDone()
    {
        UpdatePriceData();
        RefreshSlots();
        UIManager.Instance.Find<UIFrameHUD>().RefreshCurrency();
    }

    private void HandleError(ZWebCommunicator.E_ErrorType _errorType, ZWebReqPacketBase _reqPacket, ZWebRecvPacket _recvPacket)
    {
        OpenErrorPopUp(_recvPacket.ErrCode);
    }

    void OpenErrorPopUp(ERROR errorCode)
    {
        DBLocale.TryGet(ZUIString.LOCALE_OK_BUTTON, out Locale_Table table);
        string btnName = table.Text;
        switch (errorCode)
        {
            case ERROR.RESTORE_EXP_EXPIRE_DT:
            case ERROR.NOT_ENOUGH_GOODS:
            case ERROR.NOT_ENOUGH_GOLD:
            case ERROR.NOT_ENOUGH_DIAMOND:
                DBLocale.TryGet(errorCode.ToString(), out table);
                UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
                {
                    _popup.Open(
                        ZUIString.ERROR
                        , table.Text
                        , new string[] { btnName }
                        , new Action[] { delegate { _popup.Close(); } });
                });
                break;
            default:
                UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
                {
                    _popup.Open(ZUIString.ERROR, "문제가 발생하였습니다.", new string[] { btnName }, new Action[] { () => { _popup.Close(); } });
                });
                break;
        }
    }

    #region OnClick Event
    public void OnClickRestoreForFree()
    {
        OpenPopup(CostType.Free, 0);
    }

    public void OnClickRestoreByGold()
    {
        if (CheckCurrencyEnough(CostType.Gold) == false)
        {
            // OpenErrorPopUp(ERROR.NOT_ENOUGH_GOLD);
            return;
        }

        OpenPopup(CostType.Gold, goldPrice);
    }

    public void OnClickRestoreByDiamond()
    {
        if (CheckCurrencyEnough(CostType.Diamond) == false)
        {
            // OpenErrorPopUp(ERROR.NOT_ENOUGH_DIAMOND);
            return;
        }

        OpenPopup(CostType.Diamond, diamondPrice);
    }
    #endregion
}
