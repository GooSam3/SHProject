using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PREquipmentContentSell : PREquipContentBase
{
    [SerializeField] private UIPREquipScrollAdapter adapterSellData;

    [SerializeField] private ZButton btnSell;

    [SerializeField] private Text txtCost;

    // 어댑터에 쓰일 데이터(빈칸포함)
    private List<PREquipItemData> listSellData = new List<PREquipItemData>();

    // {runeID : data}
    // 실제로 등록된 데이터
    private Dictionary<ulong, PREquipItemData> dicSellData = new Dictionary<ulong, PREquipItemData>();

    private ulong sellCost = 0;

    public override void Init(PREquipmentInventory owner)
    {
        base.Init(owner);

        adapterSellData.Initialize(OnClickSellSlot);
    }

    public override void Open()
    {
        base.Open();

#if UNITY_EDITOR

        if (UnityEditor.EditorApplication.isPlaying == false)
            return;
#endif
        Refresh();
        RefreshCost();
    }

    public override void Close(bool invokeAction)
    {
        base.Close(invokeAction);

        listSellData.Clear();
        dicSellData.Clear();
        RefreshSellScroll();
    }

    private void OnClickSellSlot(PREquipItemData data)
    {
        if (data.isVisible == false)
            return;
        RemoveSellSlot(data.data.RuneId);

        RefrshScrollAdapter();
    }

    private void RemoveSellSlot(ulong runeId)
    {
        if (dicSellData.TryGetValue(runeId, out var slotData) == false)
            return;

        //slotData.isVisible = true;
        slotData.isMoved = false;
        dicSellData.Remove(runeId);
    }

    public void ClearSellSlot()
    {
        var sellList = dicSellData.Values.ToList();

        foreach (var iter in sellList)
        {
            RemoveSellSlot(iter.data.RuneId);
        }

        RefrshScrollAdapter();
    }

    public override void OnInvenClick(PREquipItemData data)
    {
        if (data.isVisible == false)
            return;

        if (dicSellData.ContainsKey(data.data.RuneId) == true)
        {
            ZLog.LogError(ZLogChannel.UI, "이미 있는놈 들어와ㅣㅆㅇㅁ");
            return;
        }

        if (data.data.OwnerPetTid > 0)
        {
            UIMessagePopup.ShowPopupOk(DBLocale.GetText("WItemInfo_NoEquip"));
            return;
        }

        //data.isVisible = false;
        data.isMoved = true;

        dicSellData.Add(data.data.RuneId, data);

        RefrshScrollAdapter();
    }

    public override void Refresh()
    {
        base.Refresh();

        listSellData.Clear();

        int count = ZNet.Data.Me.CurCharData.GetRuneCountAll();

        var left = count % ZUIConstant.PR_EQUIP_SELLSCROLL_WIDTH_COUNT;
        if (count > ZUIConstant.PR_EQUIP_SELLSCROLL_COUNT_MIN)
        {
            if (left > 0)
                count += ZUIConstant.PR_EQUIP_SELLSCROLL_WIDTH_COUNT - left;
        }
        else
        {
            count = ZUIConstant.PR_EQUIP_SELLSCROLL_COUNT_MIN;
        }

        for (int i = 0; i < count; i++)
        {
            listSellData.Add(new PREquipItemData());
        }

        adapterSellData.ResetListData(listSellData);
    }

    private void RefreshSellScroll()
    {
        var sellList = dicSellData.Values.ToList();

        for (int i = 0; i < listSellData.Count; i++)
        {
            if (sellList.Count <= i)
            {
                listSellData[i].isVisible = false;
                continue;
            }

            listSellData[i].Reset(sellList[i].data);
        }

        adapterSellData.RefreshData();
    }

    private void RefrshScrollAdapter()
    {
        RefreshSellScroll();

        owner.RefreshInven();
        btnSell.interactable = dicSellData.Values.Count > 0;
        RefreshCost();
    }

    private void RefreshCost()
    {
        sellCost = 0;

        foreach (var iter in dicSellData.Values)
        {
            if (DBItem.GetItem(iter.data.RuneTid, out var table) == false)
                continue;

            sellCost += table.SellItemCount;
        }

        txtCost.text = sellCost.ToString("N0");
    }

    public void OnClickSell()
    {
        if (dicSellData.Values.Count <= 0)
            return;

        List<ulong> sellRune = new List<ulong>();

        foreach (var iter in dicSellData.Values)
        {
            sellRune.Add(iter.data.RuneId);
        }

        UIMessagePopup.ShowCostPopup(DBLocale.GetText("Sell_Text"), DBLocale.GetText("Rune_Break_Message"), DBConfig.Gold_ID, sellCost, () =>
         {
             ZWebManager.Instance.WebGame.REQ_RuneSell(sellRune, (recvPacket, recvMsgPacket) =>
             {
                 dicSellData.Clear();
                 owner.Refresh();
             }, null);
         },costDescKey:"Sell_Own_Desc");
    }

    public void OnClickAddFilteredDataAll()
    {
        foreach(var iter in owner.ListEquipData)
        {
            if (iter.isDisable)
                continue;

            if (iter.isMoved)
                continue;

            if (iter.isVisible == false)
                continue;

            if (iter.data == null)
                continue;

            if (iter.data.IsLock)
                continue;

            if (iter.data.OwnerPetTid > 0)
                continue;

            if (dicSellData.ContainsKey(iter.data.RuneId))
                continue;

            iter.isMoved = true;
            dicSellData.Add(iter.data.RuneId, iter);
        }

        RefrshScrollAdapter();
    }
}
