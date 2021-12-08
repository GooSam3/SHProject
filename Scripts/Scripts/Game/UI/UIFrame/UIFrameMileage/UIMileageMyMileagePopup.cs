using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZNet.Data;

public class UIMileageMyMileagePopup : MonoBehaviour
{
    [SerializeField] private MileageMyMileageListItem uiSourceObj;
    [SerializeField] private RectTransform listRoot;
    [SerializeField] private ScrollRect scrollRect;

    private List<MileageMyMileageListItem> slots;

    private bool initDone;

    public Action onClose;

    public void Open(Action onCloseCallback)
    {
        if (initDone == false)
        {
            Initialize();
            initDone = true;
        }

        scrollRect.verticalNormalizedPosition = 1f;

        onClose = onCloseCallback;

        UpdateList();

        //DBMileageShop.ForeachAllBuyItemID((buyItemData) =>
        //{
        //    var targetSlot = slots.Find(t => t.MileageBuyItemID == buyItemData.id);
        //    var buyItemInfo = DBItem.GetItem(targetSlot.MileageBuyItemID);
        //    if (buyItemInfo == null)
        //    {
        //        ZLog.LogError(ZLogChannel.UI, "BuyItemID in MileageTable not exist on ItemTable Referenced , TID : " + buyItemData);
        //    }
        //else
        //{
        //ulong myMileageCount = Me.GetCurrency(targetSlot.MileageBuyItemID);
        //float myMileagePercentage = (myMileageCount / (float)buyItemData.count);
        //var affordCnt = myMileageCount / buyItemData.count;
        //string myMileagePercent = ((((uint)myMileageCount) / mileageInfo.BuyItemCount) * 100).ToString("0.00") + "%";

        /// TODO : 스프라이트 일단 NULL 
        //targetSlot.Set(
        //    null
        //    , affordCnt > 0
        //    , DBLocale.GetText(buyItemInfo.ItemTextID)
        //    , myMileagePercentage.ToString("0.00") + "%"
        //    , affordCnt);
        //}
        // });

        /// 일단 buyItemID 로 소팅 
        //slots.Sort((t01, t02) =>
        //{
        //    return t02.AffordCount.CompareTo(t01.AffordCount);
        //});

        // for (int i = 0; i < slots.Count; i++)
        //  {
        //     slots[i].transform.SetSiblingIndex(i);
        //      slots[i].gameObject.SetActive(true);
        //    }
    }

    public void Close()
    {
        slots.ForEach(t => t.gameObject.SetActive(false));
        onClose?.Invoke();
    }

    private MileageMyMileageListItem GenerateSlot()
    {
        if (uiSourceObj == null)
            return null;
        return Instantiate(uiSourceObj, listRoot);
    }

    private void Initialize()
    {
        uiSourceObj.gameObject.SetActive(false);

        int slotCnt = DBMileageShop.GetTableDataCount();
        slots = new List<MileageMyMileageListItem>(slotCnt);

        DBMileageShop.ForeachAllBuyItemID((data) =>
        {
            var target = GenerateSlot();
            target.Initialize(
                ZManagerUIPreset.Instance.GetSprite(DBItem.GetItemIconName(data.id))
                , DBLocale.GetText(DBItem.GetItemName(data.id))
                , data.id, data.count);
            slots.Add(target);
        });
    }

    private void UpdateList()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            var slot = slots[i];
            ulong myMileageCount = Me.GetCurrency(slot.MileageBuyItemID);
            var affordCnt = myMileageCount / slot.SingleItemCostCount;

            if (affordCnt > 0)
            {
                double myMileagePercentage = (myMileageCount / (double)slot.SingleItemCostCount) * 100d;
                slot.Set(((int)(myMileagePercentage)).ToString() + "%", affordCnt);
                slot.gameObject.SetActive(true);
            }
            else
            {
                slot.gameObject.SetActive(false);
            }
        }

        slots.Sort((t01, t02) =>
        {
            if (t01.gameObject.activeSelf && t02.gameObject.activeSelf == false)
            {
                return -1;
            }
            else if (t01.gameObject.activeSelf == false && t02.gameObject.activeSelf)
            {
                return 1;
            }
            else
            {
                return t02.AffordCount.CompareTo(t01.AffordCount);
            }
        });

        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].transform.SetSiblingIndex(i);
        }
    }
}