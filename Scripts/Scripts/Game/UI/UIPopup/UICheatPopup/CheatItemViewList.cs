using GameDB;
using System;
using System.Collections.Generic;
using UnityEngine;

public class C_CheatItemData
{
    public Item_Table table;
    public int value;
}

public class C_ItemTypeSlot
{
    public E_ItemType type;
    public CheatItemViewListItem instance;

    public C_ItemTypeSlot(E_ItemType _type)
    {
        type = _type;
    }
}

public class C_MonsterTypeSlot
{
    public E_MonsterType type;
    public CheatItemViewListItem instance;

    public C_MonsterTypeSlot(E_MonsterType _type)
    {
        type = _type;
    }
}


// 극혐코드 죄송합니다, 리스트관련 변경될여지가 있는것 같아 분기태웠습니다
public class CheatItemViewList : CUGUIScrollRectListBase, CUGUIWidgetSlotItemBase.ISlotItemOwner
{
    public enum E_SlotType
    {
        ItemTypeList = 0,
        ShopList = 1,
        WishList = 2,
        MonsterTypeList = 3,
        MonsterList = 4,
    }

    private E_SlotType curSlotType;

    private C_ItemTypeSlot selectedItemTypeSlot;

    private C_MonsterTypeSlot selectedMonsterTypeSlot;

    protected override void OnUIWidgetInitializePost(CUIFrameBase _UIFrameParent)
    {
        base.OnUIWidgetInitializePost(_UIFrameParent);
    }

    protected override void OnUIScrollRectListRefreshItem(int _Index, CUGUIWidgetSlotItemBase _NewItem)
    {
        CheatItemViewListItem item = _NewItem as CheatItemViewListItem;
        item.SetSlotItemOwner(this);
        switch (curSlotType)
        {
            case E_SlotType.ItemTypeList:
                {
                    C_ItemTypeSlot data = listItemType[_Index];
                    data.instance = item;
                    item.SetItemType(data);
                    if (selectedItemTypeSlot != null)
                        item.SetColor(selectedItemTypeSlot.type == item.ItemType.type ? Color.cyan : Color.gray);
                }
                break;
            case E_SlotType.ShopList:
                {
                    Item_Table data = listShopItem[_Index];
                    item.SetShopSlot(data);
                }
                break;
            case E_SlotType.WishList:
                {
                    C_CheatItemData data = listWishList[_Index];
                    item.SetWishSlot(data);
                }
                break;
            case E_SlotType.MonsterTypeList:
                {
                    C_MonsterTypeSlot data = listMonsterType[_Index];
                    data.instance = item;
                    item.SetMonsterType(data);
                    if (selectedMonsterTypeSlot != null)
                        item.SetColor(selectedMonsterTypeSlot.type == item.MonsterType.type ? Color.cyan : Color.gray);
                }
                break;
            case E_SlotType.MonsterList:
                {
                    Monster_Table data = listMonster[_Index];
                    item.SetMonsterSlot(data);
                }
                break;
        }
    }

    public void ISlotItemSelect(CUGUIWidgetSlotItemBase _SelectItem)
    {
        CheatItemViewListItem slot = _SelectItem as CheatItemViewListItem;

        switch (curSlotType)
        {
            case E_SlotType.ItemTypeList:

                selectedItemTypeSlot?.instance?.SetColor(Color.gray);
                selectedItemTypeSlot = slot.ItemType;
                selectedItemTypeSlot.instance.SetColor(Color.cyan);

                onClickItemType?.Invoke(slot.ItemType);

                break;
            case E_SlotType.ShopList:
                onClickShopItem?.Invoke(slot.ShopItemData,false);
                break;
            case E_SlotType.WishList:
                onClickWishItem?.Invoke(slot.WishItemData,false);
                break;
            case E_SlotType.MonsterTypeList:
                selectedMonsterTypeSlot?.instance?.SetColor(Color.gray);
                selectedMonsterTypeSlot = slot.MonsterType;
                selectedMonsterTypeSlot.instance.SetColor(Color.cyan);

                onClickMonsterType?.Invoke(slot.MonsterType);

                break;
            case E_SlotType.MonsterList:
                onClickMonster?.Invoke(slot.MonsterData);
                break;
        }
    }

    public void OnRightClick(CUGUIWidgetSlotItemBase _SelectItem)
    {
        CheatItemViewListItem slot = _SelectItem as CheatItemViewListItem;

        switch (curSlotType)
        {
            case E_SlotType.ShopList:
                onClickShopItem?.Invoke(slot.ShopItemData,true);
                break;
            case E_SlotType.WishList:
                onClickWishItem?.Invoke(slot.WishItemData,true);
                break;
        }
    }

    public void SetUnselectItemTypeSlot()
    {
        selectedItemTypeSlot?.instance?.SetColor(Color.gray);
        selectedItemTypeSlot = null;
    }

    public void SetUnselectMonsterTypeSlot()
    {
        selectedMonsterTypeSlot?.instance?.SetColor(Color.gray);
        selectedMonsterTypeSlot = null;
    }

    public void DoRefresh()
    {
        int count = 0;
        switch (curSlotType)
        {
            case E_SlotType.ItemTypeList:
                count = listItemType.Count;
                break;
            case E_SlotType.ShopList:
                count = listShopItem.Count;
                break;
            case E_SlotType.WishList:
                count = listWishList.Count;
                break;
            case E_SlotType.MonsterTypeList:
                count = listMonsterType.Count;
                break;
            case E_SlotType.MonsterList:
                count = listMonster.Count;
                break;
        }

        ProtUIScrollListInitialize(count);
    }

    #region # ItemType

    private List<C_ItemTypeSlot> listItemType = new List<C_ItemTypeSlot>();
    private Action<C_ItemTypeSlot> onClickItemType;

    public List<C_ItemTypeSlot> ListType => listItemType;

    public void InitializeItemType(Action<C_ItemTypeSlot> _onClick)
    {
        listItemType = new List<C_ItemTypeSlot>();
        onClickItemType = _onClick;
        curSlotType = E_SlotType.ItemTypeList;
    }

    // 들어오지 않지만 선언해놓습니다.
    public void ClearItemTypeList() => listItemType.Clear();

    public void AddListItemType(C_ItemTypeSlot _item) => listItemType.Add(_item);

    #endregion ItemType #

    #region # Shop

    // shop
    private List<Item_Table> listShopItem = new List<Item_Table>();
    private Action<Item_Table,bool> onClickShopItem;

    public void InitializeShop(Action<Item_Table,bool> _onClick)
    {
        listShopItem = new List<Item_Table>();
        onClickShopItem = _onClick;
        curSlotType = E_SlotType.ShopList;
    }

    public void ClearShopListData() => listShopItem.Clear();

    public void AddShopListData(List<Item_Table> _item) => listShopItem = _item;
    #endregion Shop #

    #region # WishList

    // wishlist
    private List<C_CheatItemData> listWishList = new List<C_CheatItemData>();
    private Action<C_CheatItemData,bool> onClickWishItem;

    public List<C_CheatItemData> WishList => listWishList;

    public void InitializeWishlist(Action<C_CheatItemData,bool> _onClick)
    {
        listWishList = new List<C_CheatItemData>();
        onClickWishItem = _onClick;
        curSlotType = E_SlotType.WishList;
    }

    public void ClearWishListData() => listWishList.Clear();

    public void AddWishListData(Item_Table _item, int _value)
    {
        var myItem = listWishList.Find((item) => item.table.ItemID == _item.ItemID);

        if (myItem == null)
        {
            listWishList.Add(new C_CheatItemData() { table = _item, value = _value });
        }
        else
        {
            myItem.value += _value;
        }
    }

    public void RemoveWishListData(Item_Table _item, int _value)
    {
        var myItem = listWishList.Find((item) => item.table.ItemID == _item.ItemID);

        if (myItem != null)
        {
            myItem.value -= _value;
            if (myItem.value <= 0)
            {
                listWishList.Remove(myItem);
            }
        }
    }

    #endregion WishList #

    #region # MonsterType

    private List<C_MonsterTypeSlot> listMonsterType = new List<C_MonsterTypeSlot>();
    private Action<C_MonsterTypeSlot> onClickMonsterType;

    public List<C_MonsterTypeSlot> MonsterType => listMonsterType;

    public void InitializeMonsterType(Action<C_MonsterTypeSlot> _onClick)
    {
        listMonsterType = new List<C_MonsterTypeSlot>();
        onClickMonsterType = _onClick;
        curSlotType = E_SlotType.MonsterTypeList;
    }

    // 들어오지 않지만 선언해놓습니다.
    public void ClearMonsterTypeList() => listMonsterType.Clear();

    public void AddListMonsterType(C_MonsterTypeSlot _item) => listMonsterType.Add(_item);

    #endregion MonsterType #

    #region # MonsterList

    private List<Monster_Table> listMonster = new List<Monster_Table>();
    private Action<Monster_Table> onClickMonster;//소환

    public void InitializeMonsterList(Action<Monster_Table> _onClick)
    {
        listMonster = new List<Monster_Table>();
        onClickMonster = _onClick;
        curSlotType = E_SlotType.MonsterList;
    }

    public void ClearMonsterListData() => listShopItem.Clear();

    public void AddMonsterListData(List<Monster_Table> _item) => listMonster = _item;

    #endregion MonsterList #
}