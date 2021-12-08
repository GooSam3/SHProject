using GameDB;
using System.Collections.Generic;
using System.Linq;

[UnityEngine.Scripting.Preserve]
public class DBItem : IGameDBHelper
{
    //   static Dictionary<uint,List<string>> DicInitialSound = new Dictionary<uint, List<string>>();

    //   static Dictionary<E_TradeTapType, Dictionary<E_TradeSubTapType, List<Item_Table>>> TradeDicGroup = new Dictionary<E_TradeTapType, Dictionary<E_TradeSubTapType, List<Item_Table>>>();

    static Dictionary<uint, List<Item_Table>> ItemGroupDic = new Dictionary<uint, List<Item_Table>>();

    static Dictionary<E_RuneSetType, Item_Table> CachedRuneSetTypeDict = new Dictionary<E_RuneSetType, Item_Table>();

    //   static Dictionary<uint, uint> StepDownDic = new Dictionary<uint, uint>();

    //   static Dictionary<uint, List<Item_Table>> itemDicByGrade = new Dictionary<uint, List<Item_Table>>();

    public void OnReadyData()
    {
        //       DicInitialSound.Clear();
        //       TradeDicGroup.Clear();
        ItemGroupDic.Clear();
        CachedRuneSetTypeDict.Clear();
        //       StepDownDic.Clear();
        //       itemDicByGrade.Clear();

        foreach (var tableData in GameDBManager.Container.Item_Table_data.Values)
        {
            //if(0 < tableData.Grade)
            //{
            //    if (!itemDicByGrade.ContainsKey(tableData.Grade))
            //        itemDicByGrade.Add(tableData.Grade, new List<Item_Table>());

            //    itemDicByGrade[tableData.Grade].Add(tableData);
            //}

            //string Name = DBLocale.GetLastestLocaleText(tableData.ItemTextID);
            //tableData.ItemTextID = Name;

            if (!ItemGroupDic.ContainsKey(tableData.GroupID))
                ItemGroupDic.Add(tableData.GroupID, new List<Item_Table>());
            ItemGroupDic[tableData.GroupID].Add(tableData);

            //if (tableData.TradeTapType != E_TradeTapType.None)
            //{
            //	if (!DicInitialSound.ContainsKey(tableData.ItemID))
            //		DicInitialSound.Add(tableData.ItemID, new List<string>());

            //	if (!string.IsNullOrEmpty(Name))
            //	{
            //		for (int i = 0; i < Name.Length; i++)
            //			DicInitialSound[tableData.ItemID].Add(ZWord.FindInitialSound(Name[i]));
            //	}
            //}

            //if (tableData.TradeTapType != E_TradeTapType.None && tableData.TradeSubTapType != E_TradeSubTapType.None)
            //{
            //	if (!TradeDicGroup.ContainsKey(tableData.TradeTapType))
            //		TradeDicGroup.Add(tableData.TradeTapType, new Dictionary<E_TradeSubTapType, List<Item_Table>>());

            //	if (!TradeDicGroup.ContainsKey(E_TradeTapType.None))
            //		TradeDicGroup.Add(E_TradeTapType.None, new Dictionary<E_TradeSubTapType, List<Item_Table>>());

            //	if (!TradeDicGroup[tableData.TradeTapType].ContainsKey(tableData.TradeSubTapType))
            //		TradeDicGroup[tableData.TradeTapType].Add(tableData.TradeSubTapType, new List<Item_Table>());

            //	if (!TradeDicGroup[E_TradeTapType.None].ContainsKey(E_TradeSubTapType.None))
            //		TradeDicGroup[E_TradeTapType.None].Add(E_TradeSubTapType.None, new List<Item_Table>());

            //	if (!TradeDicGroup[tableData.TradeTapType].ContainsKey(E_TradeSubTapType.None))
            //		TradeDicGroup[tableData.TradeTapType].Add(E_TradeSubTapType.None, new List<Item_Table>());

            //	if (TradeDicGroup[tableData.TradeTapType][tableData.TradeSubTapType].Find(item => item.GroupID == tableData.GroupID) == null)
            //		TradeDicGroup[tableData.TradeTapType][tableData.TradeSubTapType].Add(tableData);

            //	if (TradeDicGroup[E_TradeTapType.None][E_TradeSubTapType.None].Find(item => item.GroupID == tableData.GroupID) == null)
            //		TradeDicGroup[E_TradeTapType.None][E_TradeSubTapType.None].Add(tableData);

            //	if (TradeDicGroup[tableData.TradeTapType][E_TradeSubTapType.None].Find(item => item.GroupID == tableData.GroupID) == null)
            //		TradeDicGroup[tableData.TradeTapType][E_TradeSubTapType.None].Add(tableData);
            //}

            if (tableData.RuneSetType != E_RuneSetType.None)
            {
            	if (tableData.RuneGradeType == E_RuneGradeType.None &&  CachedRuneSetTypeDict.ContainsKey(tableData.RuneSetType) == false)
            		CachedRuneSetTypeDict.Add(tableData.RuneSetType, tableData);
            }

            //if (tableData.StepUpID != 0)
            //{
            //	StepDownDic.Add(tableData.StepUpID, tableData.ItemID);
            //}
        }
    }

    //   //아이템 접근 시 아이템 번호 별로 생성해서 슬롯을 만들어 두겠음 - 사전 로드 안함!
    //   public class DropInfo
    //   {
    //       public uint ItemTid;
    //       public List<uint> DropMonsters = new List<uint>();
    //       public List<uint> DropStages = new List<uint>();
    //   }
    //   static Dictionary<uint, DropInfo> ItemDropDatas = new Dictionary<uint, DropInfo>();

    //   public static Dictionary<uint,Item_Table>.ValueCollection GetItems()
    //   {
    //       return GameDBManager.Container.Item_Table_data.Values;
    //   }

    public static bool GetAllItem(out List<Item_Table> table)
    {
        table = GameDBManager.Container.Item_Table_data.Values.ToList();
        return table != null;
    }

    public static bool GetItem(uint itemTid, out Item_Table tableData)
    {
        return GameDBManager.Container.Item_Table_data.TryGetValue(itemTid, out tableData);
    }

    public static bool Has(uint itemTid)
    {
        return GameDBManager.Container.Item_Table_data.ContainsKey(itemTid);
    }

    public static Item_Table GetItem(uint ItemTid)
    {
        if (!GameDBManager.Container.Item_Table_data.ContainsKey(ItemTid))
        {
            //ZLog.LogError("GetItem - Can't Find ItemTid " + ItemTid);
            return null;
        }

        return GameDBManager.Container.Item_Table_data[ItemTid];
    }

    //   // 접근함수들은 static으로 만들자~
    //   public static bool IsSameType(uint ItemTid,params E_ItemType[] checkTypes)
    //   {
    //       if (!GameDBManager.Container.Item_Table_data.ContainsKey(ItemTid))
    //       {
    //           ZLog.LogError("IsSameType - Can't Find ItemTid " + ItemTid);
    //           return false;
    //       }

    //       for (int i = 0; i < checkTypes.Length; i++)
    //           if (GameDBManager.Container.Item_Table_data[ItemTid].ItemType == checkTypes[i])
    //               return true;

    //       return false;
    //   }

    //   public static bool IsShowGrade(uint ItemTid)
    //   {
    //       if (GameDBManager.Container.Item_Table_data.ContainsKey(ItemTid))
    //       {
    //           switch (GameDBManager.Container.Item_Table_data[ItemTid].ItemType)
    //           {
    //               /*case E_ItemType.Ring:
    //               case E_ItemType.Earring:
    //               case E_ItemType.Necklace:
    //               case E_ItemType.Belt:
    //               case E_ItemType.Tshirt:
    //                   return false;*/
    //               default:
    //                   return true;
    //           }
    //       }

    //       ZLog.LogError("IsShowGrade - Can't Find ItemTid : " + ItemTid);
    //       return false;
    //   }

    public static bool IsAccessory(uint ItemTid)
    {
        if (GameDBManager.Container.Item_Table_data.ContainsKey(ItemTid))
        {
            return IsAccessory(GameDBManager.Container.Item_Table_data[ItemTid].ItemType);
        }
     //   ZLog.LogError("IsAccessory - Can't Find ItemTid : " + ItemTid);
        return false;
    }

    public static bool IsEquipGem(uint ItemTid)
    {
        var itemData = GetItem(ItemTid);
        if (itemData == null)
            return false;

        return IsEquipGem(itemData);
    }

    public static bool IsEquipGem(Item_Table data)
    {
        if (data == null)
            return false;

        return (data.ItemUseType == E_ItemUseType.Equip)
            &&
            (data.ItemType == E_ItemType.ChaosGem
            || data.ItemType == E_ItemType.EarthGem
            || data.ItemType == E_ItemType.FireGem
            || data.ItemType == E_ItemType.SeaGem
            || data.ItemType == E_ItemType.TreeGem
            || data.ItemType == E_ItemType.WindGem);
    }

    public static bool IsAccessory(E_ItemType checkType)
    {
        switch (checkType)
        {
            case E_ItemType.Ring:
            case E_ItemType.Earring:
            case E_ItemType.Necklace:
            case E_ItemType.Belt:
            case E_ItemType.Tshirt:
            case E_ItemType.Bracelet:
            case E_ItemType.Cape:
                return true;
        }

        return false;
    }

    //   /// <summary> 재련 가능한 아이템인지 여부 </summary>
    //   public static bool IsResmeltingItem(uint ItemTid)
    //   {
    //       if (false == GameDBManager.Container.Item_Table_data.ContainsKey(ItemTid))
    //           return false;

    //       return GameDBManager.Container.Item_Table_data[ItemTid].SmeltScrollUseType == E_SmeltScrollUseType.SmeltScroll;
    //   }

    /// <summary> 장비 아이템인지 여부 </summary>
    public static bool IsEquipItem(uint ItemTid)
    {
        if (GameDBManager.Container.Item_Table_data.ContainsKey(ItemTid))
        {
            return IsEquipItem(GameDBManager.Container.Item_Table_data[ItemTid].ItemType);
        }

        //ZLog.LogError("IsEquipItem - Can't Find ItemTid : " + ItemTid);
        return false;
    }

    public static bool IsEquipItem(E_ItemType type)
    {
        switch (type)
        {
            case E_ItemType.Weapon:
            case E_ItemType.SideWeapon:
            case E_ItemType.Helmet:
            case E_ItemType.Armor:
            case E_ItemType.Gloves:
            case E_ItemType.Pants:
            case E_ItemType.Shoes:
            case E_ItemType.Cape:
            case E_ItemType.Ring:
            case E_ItemType.Necklace:
            case E_ItemType.Earring:
            case E_ItemType.Belt:
            case E_ItemType.Bracelet:
            case E_ItemType.Tshirt:
            case E_ItemType.Artifact:
                return true;
            default:
                return false;
        }
    }

    public static E_CharacterType GetUseCharacterType(uint ItemTid)
    {
        if (GameDBManager.Container.Item_Table_data.ContainsKey(ItemTid))
        {
            return GameDBManager.Container.Item_Table_data[ItemTid].UseCharacterType;
        }

        //ZLog.LogError("GetUseCharacterType - Can't Find ItemTid : " + ItemTid);

        return E_CharacterType.None;
    }

    public static float GetWeight(uint ItemTid)
    {
        return GetItem(ItemTid)?.Weight ?? 0;
    }

    public static uint GetGroupId(uint ItemTid)
    {
        return GetItem(ItemTid)?.GroupID ?? 0;
    }

    public static uint GetViewGroupId(uint ItemTid)
    {
        return GetItem(ItemTid)?.ViewGroupID ?? 0;
    }

    public static uint GetSameEquipCount(uint ItemTid)
    {
        List<E_EquipSlotType> getequiptypes = GetEquipSlots(ItemTid);

        if (getequiptypes.Count > 0)
            return GetSameEquipCount(getequiptypes[0]);

        return 1;
    }

    public static uint GetSameEquipCount(E_EquipSlotType equipType)
    {
        switch (equipType)
        {
            case E_EquipSlotType.Ring:
            case E_EquipSlotType.Ring_2:
            case E_EquipSlotType.Ring_3:
            case E_EquipSlotType.Ring_4:
                return DBConfig.SameRing_EquipCount;
        }

        return 1;
    }

    static Dictionary<E_EquipSlotType, List<E_EquipSlotType>> equipslotDic = new Dictionary<E_EquipSlotType, List<E_EquipSlotType>>();

    public static List<E_EquipSlotType> GetEquipSlots(uint ItemTid)
    {
        if (equipslotDic.Count <= 0)
        {
            foreach (var type in EnumHelper.Values<E_EquipSlotType>())
            {
                if (type == E_EquipSlotType.Earring || type == E_EquipSlotType.Earring_2)
                    equipslotDic.Add(type, new List<E_EquipSlotType>() { E_EquipSlotType.Earring, E_EquipSlotType.Earring_2 });
                else if (type == E_EquipSlotType.Ring || type == E_EquipSlotType.Ring_2 || type == E_EquipSlotType.Ring_3 || type == E_EquipSlotType.Ring_4)
                    equipslotDic.Add(type, new List<E_EquipSlotType>() { E_EquipSlotType.Ring, E_EquipSlotType.Ring_2, E_EquipSlotType.Ring_3, E_EquipSlotType.Ring_4 });
                else if (type == E_EquipSlotType.Bracelet || type == E_EquipSlotType.Bracelet_2)
                    equipslotDic.Add(type, new List<E_EquipSlotType>() { E_EquipSlotType.Bracelet, E_EquipSlotType.Bracelet_2 });
                else
                    equipslotDic.Add(type, new List<E_EquipSlotType>() { type });
            }
        }

        E_EquipSlotType getslotType = GetEquipSlot(ItemTid);

        if (equipslotDic.ContainsKey(getslotType))
            return equipslotDic[getslotType];

        return null;
    }

    public static E_EquipSlotType GetEquipSlot(uint ItemTid)
    {
        if (GameDBManager.Container.Item_Table_data.ContainsKey(ItemTid))
        {
            return GameDBManager.Container.Item_Table_data[ItemTid].EquipSlotType;
        }

        //ZLog.LogError("GetEquipSlot - Can't Find ItemTid : " + ItemTid);

        return E_EquipSlotType.None;
    }

    public static string GetEquipslotImageName(E_EquipSlotType slotType)
    {
        switch (slotType)
        {
            case E_EquipSlotType.Armor:
                return "Icon_WEquip_EquipSlot4";
            case E_EquipSlotType.Necklace:
                return "Icon_EquipW_AccessorySlot2";
            case E_EquipSlotType.Cape:
                return "Icon_WEquip_EquipSlot7";
            case E_EquipSlotType.Earring:
            case E_EquipSlotType.Earring_2:
                return "Icon_EquipW_AccessorySlot1";
            case E_EquipSlotType.Gloves:
                return "Icon_WEquip_EquipSlot5";
            case E_EquipSlotType.Helmet:
                return "Icon_WEquip_EquipSlot2";
            case E_EquipSlotType.Pants:
                return "Icon_WEquip_EquipSlot6";
            case E_EquipSlotType.Ring:
            case E_EquipSlotType.Ring_2:
            case E_EquipSlotType.Ring_3:
            case E_EquipSlotType.Ring_4:
                return "Icon_EquipW_AccessorySlot4";
            case E_EquipSlotType.Shoes:
                return "Icon_WEquip_EquipSlot8";
            case E_EquipSlotType.SideWeapon:
                return "Icon_WEquip_EquipSlot3";
            case E_EquipSlotType.Bracelet:
            case E_EquipSlotType.Bracelet_2:
                return "Icon_EquipW_AccessorySlot3";
            case E_EquipSlotType.Weapon:
                return "Icon_WEquip_EquipSlot1";
            case E_EquipSlotType.Artifact:
                return "Icon_WEquip_EquipSlot10";
        }

        return "";
    }

    public static E_ItemStackType GetItemStackType(uint ItemTid)
    {
        if (GameDBManager.Container.Item_Table_data.ContainsKey(ItemTid))
        {
            return GameDBManager.Container.Item_Table_data[ItemTid].ItemStackType;
        }

        //ZLog.LogError("GetItemStackType - Can't Find Item " + ItemTid);
        return E_ItemStackType.Not;
    }

    public static string GetItemIconName(uint ItemTid)
    {
        if (GameDBManager.Container.Item_Table_data.ContainsKey(ItemTid))
        {
            return GameDBManager.Container.Item_Table_data[ItemTid].IconID;
        }

        //ZLog.LogError("GetItemIconName - Can't Find Item " + ItemTid);
        return "";
    }

    //public static uint GetItemSoundTid(uint ItemTid)
    //{
    //    if (GameDBManager.Container.Item_Table_data.ContainsKey(ItemTid))
    //    {
    //        return GameDBManager.Container.Item_Table_data[ItemTid].SoundID;
    //    }

    //    ZLog.LogError("GetItemSoundTid - Can't Find Item " + ItemTid);
    //    return 40001;
    //}

    //public static uint GetItemStoragePopCnt(uint ItemTid)
    //{
    //    if (GameDBManager.Container.Item_Table_data.ContainsKey(ItemTid))
    //    {
    //        return GameDBManager.Container.Item_Table_data[ItemTid].StorageItemCount;
    //    }

    //    ZLog.LogError("GetItemStoragePopCnt - Can't Find Item " + ItemTid);
    //    return 0;
    //}

    //public static uint GetItemSellCnt(uint ItemTid)
    //{
    //    if (GameDBManager.Container.Item_Table_data.ContainsKey(ItemTid))
    //    {
    //        return GameDBManager.Container.Item_Table_data[ItemTid].SellItemCount;
    //    }

    //    ZLog.LogError("GetItemSellCnt - Can't Find Item " + ItemTid);
    //    return 0;
    //}

    //public static uint GetItemBreakUseCnt(uint ItemTid)
    //{
    //    if (GameDBManager.Container.Item_Table_data.ContainsKey(ItemTid))
    //    {
    //        return GameDBManager.Container.Item_Table_data[ItemTid].BreakUseCount;
    //    }

    //    ZLog.LogError("GetItemBreakUseCnt - Can't Find Item " + ItemTid);
    //    return 0;
    //}

    ///*public static uint GetItemBreakCnt(uint ItemTid)
    //{
    //    if (GameDBManager.Container.Item_Table_data.ContainsKey(ItemTid))
    //    {
    //        return GameDBManager.Container.Item_Table_data[ItemTid].BreakItemCount;
    //    }

    //    ZLog.LogError("GetItemBreakCnt - Can't Find Item " + ItemTid);
    //    return 0;
    //}*/

    public static bool IsShowInven(uint ItemTid)
    {
        if (GameDBManager.Container.Item_Table_data.ContainsKey(ItemTid))
        {
            return GameDBManager.Container.Item_Table_data[ItemTid].InvenUseType == E_InvenUseType.UseInven;
        }

        //ZLog.LogError("IsShowInven - Can't Find Item " + ItemTid);
        return false;
    }

    //public static E_InvenUseType GetInvenType(uint ItemTid)
    //{
    //    if (GameDBManager.Container.Item_Table_data.ContainsKey(ItemTid))
    //    {
    //        return GameDBManager.Container.Item_Table_data[ItemTid].InvenUseType;
    //    }

    //    ZLog.LogError("GetInvenType - Can't Find Item " + ItemTid);
    //    return default;
    //}

    public static E_ItemType GetItemType(uint ItemTid)
    {
        if (GameDBManager.Container.Item_Table_data.ContainsKey(ItemTid))
            return GameDBManager.Container.Item_Table_data[ItemTid].ItemType;
        else
        {
            //ZLog.LogError("GetItemType - Can't Find Item " + ItemTid);
            return default;
        }
    }

    //public static E_LimitType GetLimitType(uint ItemTid)
    //{
    //    if (!GameDBManager.Container.Item_Table_data.ContainsKey(ItemTid))
    //    {
    //        ZLog.LogError("GetLimitType - Can't Find Item " + ItemTid);
    //        return E_LimitType.None;
    //    }

    //    return GameDBManager.Container.Item_Table_data[ItemTid].LimitType;
    //}

    public static E_BelongType GetBelongType(uint ItemTid)
    {
        if (!GameDBManager.Container.Item_Table_data.ContainsKey(ItemTid))
        {
            ZLog.LogError(ZLogChannel.System, "GetBelongType - Can't Find Item " + ItemTid);
            return E_BelongType.None;
        }

        return GameDBManager.Container.Item_Table_data[ItemTid].BelongType;
    }

    //public static byte GetLimitLevel(uint ItemTid)
    //{
    //    if (0 == ItemTid)
    //        return 0;

    //    if (!GameDBManager.Container.Item_Table_data.ContainsKey(ItemTid))
    //    {
    //        ZLog.LogError("GetLimitLevel - Can't Find Item " + ItemTid);
    //        return 0;
    //    }

    //    return GameDBManager.Container.Item_Table_data[ItemTid].LimitLevel;
    //}

    public static E_QuickSlotType GetQuickSlotType(uint ItemTid)
    {
        if (!GameDBManager.Container.Item_Table_data.ContainsKey(ItemTid))
        {
            //ZLog.LogError("GetQuickSlotType - Can't Find Item " + ItemTid);
            return E_QuickSlotType.NotQuickSlot;
        }

        return GameDBManager.Container.Item_Table_data[ItemTid].QuickSlotType;
    }

    //public static E_QuickSlotAutoType GetQuickSlotAutoType(uint ItemTid)
    //{
    //    if (!GameDBManager.Container.Item_Table_data.ContainsKey(ItemTid))
    //    {
    //        ZLog.LogError("GetQuickSlotType - Can't Find Item " + ItemTid);
    //        return E_QuickSlotAutoType.Not;
    //    }

    //    return GameDBManager.Container.Item_Table_data[ItemTid].QuickSlotAutoType;
    //}

    //public static bool IsUsableType(uint ItemTid)
    //{
    //    switch (GetUseType(ItemTid))
    //    {
    //        case E_ItemUseType.Gacha:
    //        case E_ItemUseType.Potion:
    //        case E_ItemUseType.PetSummon:
    //        case E_ItemUseType.Change:
    //        case E_ItemUseType.UseItem:
    //        case E_ItemUseType.Buff:
    //        case E_ItemUseType.Move:
    //        case E_ItemUseType.Indulgence:
    //        case E_ItemUseType.Enchant:
    //        case E_ItemUseType.ChickenCoupon:
    //        case E_ItemUseType.SkillBook:
    //        case E_ItemUseType.Restoration:
    //        case E_ItemUseType.SmeltScroll:
    //        case E_ItemUseType.Teleport:
    //            return true;
    //        case E_ItemUseType.Ticket:
    //            if (DBItem.GetItemType(ItemTid) == E_ItemType.NameChange)
    //                return true;
    //            else if (DBItem.GetItemType(ItemTid) == E_ItemType.ClassChange)
    //                return true;
    //            break;
    //    }

    //    return false;
    //}

    //public static E_ItemUseType GetUseType(uint ItemTid)
    //{
    //    if (!GameDBManager.Container.Item_Table_data.ContainsKey(ItemTid))
    //    {
    //        ZLog.LogError("GetUseType - Can't Find Item " + ItemTid);
    //        return E_ItemUseType.Goods;
    //    }

    //    return GameDBManager.Container.Item_Table_data[ItemTid].ItemUseType;
    //}

    public static string GetItemName(uint ItemTid)
    {
        if (!GameDBManager.Container.Item_Table_data.ContainsKey(ItemTid))
        {
            //ZLog.LogError("GetItemName - Can't Find Item " + ItemTid);
            return "";
        }

        return GameDBManager.Container.Item_Table_data[ItemTid].ItemTextID;
    }

    //public static string GetItemFullName(uint ItemTid,bool bUseColor = true,bool bShowBelong = true)
    //{
    //    if (!GameDBManager.Container.Item_Table_data.ContainsKey(ItemTid))
    //    {
    //        ZLog.LogError("GetItemName - Can't Find Item " + ItemTid);
    //        return "";
    //    }

    //    var data = GameDBManager.Container.Item_Table_data[ItemTid];

    //    if (bUseColor)
    //    {
    //        if(data.InvenUseType != E_InvenUseType.RuneInven)
    //            return string.Format("<color=#{0}>{1}{2}</color>", DBUIResource.GetGradeTextColor(GameDB.E_UIType.Item, data.Grade), data.Step > 0 ? string.Format("+{0} ",data.Step) : "", data.ItemTextID);
    //        else
    //            return string.Format("<color=#{0}>{1}{2}</color>", DBRune.GetRuneGradeTextColorToHex(data.RuneGradeType), data.Step > 0 ? string.Format("+{0} ", data.Step) : "", data.ItemTextID);
    //    }
    //    else
    //        return string.Format("{0}{1}", data.Step > 0 ? string.Format("+{0} ", data.Step) : "", data.ItemTextID);
    //}

    #region Grade
    public static byte GetItemGrade(uint ItemTid)
    {
        if (!GameDBManager.Container.Item_Table_data.ContainsKey(ItemTid))
        {
            //ZLog.LogError("GetItemGrade - Can't Find Item " + ItemTid);
            return 0;
        }

        return GameDBManager.Container.Item_Table_data[ItemTid].Grade;
    }
    #endregion

    //#region Enchant
    public static byte GetEnchantStep(uint ItemTid)
    {
        if (!GameDBManager.Container.Item_Table_data.ContainsKey(ItemTid))
        {
            //ZLog.LogError("GetEnchantStep - Can't Find Item " + ItemTid);
            return 0;
        }

        return GameDBManager.Container.Item_Table_data[ItemTid].Step;
    }

    /// <summary>
    /// [박윤성] 다음단계 강화가 있는지 체크 (true면 다음단계 있음)
    /// </summary>
    /// <param name="ItemTid"></param>
    /// <returns></returns>
    public static bool IsExistStepUpId(uint ItemTid)
    {
        if(GetItem(ItemTid, out var tableData))
        {
            return tableData.StepUpID != 0;
        }

        return false;
    }

    //public static E_EnchantUseType GetEnchantUseType(uint ItemTid)
    //{
    //    if (!GameDBManager.Container.Item_Table_data.ContainsKey(ItemTid))
    //    {
    //        ZLog.LogError("GetEnchantUseType - Can't Find Item " + ItemTid);
    //        return E_EnchantUseType.None;
    //    }

    //    return GameDBManager.Container.Item_Table_data[ItemTid].EnchantUseType;
    //}

    public static ItemEnchant_Table GetEnchantData(uint ItemTid)
    {
        if (!GameDBManager.Container.Item_Table_data.ContainsKey(ItemTid))
        {
            //ZLog.LogError("GetEnchantData - Can't Find Item " + ItemTid);
            return null;
        }
        if (!GameDBManager.Container.ItemEnchant_Table_data.ContainsKey(GameDBManager.Container.Item_Table_data[ItemTid].ItemEnchantID))
        {
            //ZLog.LogError("GetEnchantData - Can't Find Item " + ItemTid);
            return null;
        }

        return GameDBManager.Container.ItemEnchant_Table_data[GameDBManager.Container.Item_Table_data[ItemTid].ItemEnchantID];
    }

    public static byte GetMaxSafeEnchantStep(uint ItemTid)
    {
        if (!GameDBManager.Container.Item_Table_data.ContainsKey(ItemTid))
        {
            //ZLog.LogError("GetMaxSafeEnchantStep - Can't Find Item " + ItemTid);
            return 0;
        }

        if (!GameDBManager.Container.ItemEnchant_Table_data.ContainsKey(GameDBManager.Container.Item_Table_data[ItemTid].ItemEnchantID))
        {
            //ZLog.LogError("GetMaxSafeEnchantStep - Can't Find Enchant " + GameDBManager.Container.Item_Table_data[ItemTid].ItemEnchantID);
            return 0;
        }

        if (GameDBManager.Container.ItemEnchant_Table_data[GameDBManager.Container.Item_Table_data[ItemTid].ItemEnchantID].DestroyType == E_DestroyType.NotDestroy)
        {
            byte retValue = GetMaxSafeEnchantStep(GameDBManager.Container.Item_Table_data[ItemTid].StepUpID);
            if (retValue == 0)
            {
                return GameDBManager.Container.Item_Table_data[ItemTid].Step;
            }
            else
                return retValue;
        }

        return 0;
    }

    public static uint GetUpgradeGroupId(uint ItemTid)
    {
        ItemEnchant_Table enchantData = GetEnchantData(ItemTid);
        if (enchantData == null)
        {
            //ZLog.LogError("GetUpgradeGroupId - Can't Find EnchantData "+ ItemTid);
            return 0;
        }

        return enchantData.UpgradeGroupID;
    }

    //public static uint GetStepDownID(uint ItemTid)
    //{
    //    if (StepDownDic.TryGetValue(ItemTid, out var stepDownId))
    //        return stepDownId;

    //    return 0;
    //}
    //#endregion

    ////sort priority
    //public static int PriorityType(E_ItemType itemType)
    //{
    //    switch (itemType)
    //    {
    //        case E_ItemType.Weapon:
    //            return 0;
    //        case E_ItemType.SideWeapon:
    //            return 1;
    //        case E_ItemType.Helmet:
    //            return 2;
    //        case E_ItemType.Armor:
    //            return 3;
    //        case E_ItemType.Gloves:
    //            return 4;
    //        case E_ItemType.Pants:
    //            return 5;
    //        case E_ItemType.Shoes:
    //            return 6;
    //        case E_ItemType.Cape:
    //            return 7;
    //        case E_ItemType.Ring:
    //            return 8;
    //        case E_ItemType.Necklace://목걸이
    //            return 9;
    //        case E_ItemType.Earring:
    //            return 10;
    //        case E_ItemType.Belt:
    //            return 11;
    //        case E_ItemType.Tshirt:
    //            return 12;

    //        case E_ItemType.WeaponEnchant:
    //            return 20;
    //        case E_ItemType.DefenseEnchant:
    //            return 21;
    //        case E_ItemType.AccessoryEnchant:
    //            return 22;
    //        case E_ItemType.Upgrade:
    //            return 23;

    //        case E_ItemType.Change:
    //            return 24;
    //        case E_ItemType.PetSummon:
    //            return 25;

    //        case E_ItemType.HPPotion:
    //            return 26;
    //        case E_ItemType.MPPotion:
    //            return 27;

    //        case E_ItemType.GodTear:
    //            return 28;
    //        case E_ItemType.ZeroBless:
    //            return 29;

    //        case E_ItemType.SpeedBuff:
    //            return 30;
    //        case E_ItemType.AttackBuff:
    //            return 31;
    //        case E_ItemType.DefenseBuff:
    //            return 32;
    //        case E_ItemType.EvasionBuff:
    //            return 33;
    //        case E_ItemType.DespairTower:
    //            return 34;

    //        case E_ItemType.FixGacha:
    //            return 35;
    //        case E_ItemType.ItemGacha:
    //            return 36;
    //        case E_ItemType.ChangeGacha:
    //            return 37;
    //        case E_ItemType.PetGacha:
    //            return 38;

    //        case E_ItemType.Indulgence:
    //            return 39;

    //        case E_ItemType.InterServerTicket:
    //            return 40;
    //        case E_ItemType.GuildDungeonTicket:
    //            return 41;
    //    }
    //    return int.MaxValue;
    //}

    //public static string ParseItemInfo(uint ItemTid)
    //{
    //    var tableData = GetItem(ItemTid);

    //    if (ItemTid == 0 || tableData == null)
    //    {
    //        ZLog.LogError("ParseItemInfo - can't Find ItemTid : "+ItemTid);
    //        return "";
    //    }

    //    if (IsEquipItem(ItemTid))
    //    {
    //        string strLevelLimit = "";
    //        if (tableData.LimitLevel > 1)
    //            strLevelLimit = string.Format(DBLocale.GetLocaleText("WShop_GoodsInfo_Level"), tableData.LimitLevel);

    //        if(IsAccessory(GetItemType(ItemTid)))
    //            return string.Format("- {0}\n{1} : {2}\n{3} : {4}"//\n{5} : {6},{7}"
    //                                , GetItemFullName(ItemTid, false)
    //                                , DBLocale.GetLocaleText("ItemInfo_Group_Class")
    //                                , DBLocale.GetItemUseCharacterTypeName(ItemTid)
    //                                , DBLocale.GetLocaleText("ItemInfo_Group_Equip")
    //                                , GetParseAbilitys(ItemTid, " ", " , ")) + strLevelLimit;
    //                                //, DBLocale.GetLocaleText("ItemInfo_Group_Move")
    //                                //, EnumHelper.CheckFlag(tableData.LimitType, E_LimitType.Storage) ? DBLocale.GetLocaleText("Wshop_ItemInfo_StorageOn") : DBLocale.GetLocaleText("Wshop_ItemInfo_StorageOff")
    //                                //, EnumHelper.CheckFlag(tableData.LimitType, E_LimitType.Trade) ? DBLocale.GetLocaleText("Wshop_ItemInfo_TradeOn") : DBLocale.GetLocaleText("Wshop_ItemInfo_TradeOff"));
    //        else
    //            return string.Format("- {0}\n{1} : {2}\n{3} : {4}\n{5} : {6}"//\n{7} : {8},{9}"
    //                            , GetItemFullName(ItemTid, false)
    //                            , DBLocale.GetLocaleText("ItemInfo_Group_Tier")
    //                            , (IsShowGrade(ItemTid) && tableData.Grade > 0) ? string.Format("{0}", DBUIResource.GetTierName(E_UIType.Item, tableData.Grade)) : "없음"
    //                            , DBLocale.GetLocaleText("ItemInfo_Group_Class")
    //                            , DBLocale.GetItemUseCharacterTypeName(ItemTid)
    //                            , DBLocale.GetLocaleText("ItemInfo_Group_Equip")
    //                            , GetParseAbilitys(ItemTid, " ", " , ")) + strLevelLimit;
    //                            //, DBLocale.GetLocaleText("ItemInfo_Group_Move")
    //                            //, EnumHelper.CheckFlag(tableData.LimitType, E_LimitType.Storage) ? DBLocale.GetLocaleText("Wshop_ItemInfo_StorageOn") : DBLocale.GetLocaleText("Wshop_ItemInfo_StorageOff")
    //                            //, EnumHelper.CheckFlag(tableData.LimitType, E_LimitType.Trade) ? DBLocale.GetLocaleText("Wshop_ItemInfo_TradeOn") : DBLocale.GetLocaleText("Wshop_ItemInfo_TradeOff"));
    //    }
    //    else
    //    {
    //        string strLevelLimit = "";
    //        if (tableData.LimitLevel > 1)
    //            strLevelLimit = string.Format(DBLocale.GetLocaleText("WShop_GoodsInfo_Level"), tableData.LimitLevel);

    //        bool HasAbility = tableData.AbilityActionID_01 != 0 || tableData.AbilityActionID_02 != 0 || tableData.AbilityActionID_03 != 0;

    //        if (HasAbility)
    //            return string.Format("- {0}\n{1} : {2}\n{3} : {4}\n{5} : {6}\n{7}"//\n{8} : {9},{10}"
    //                            , GetItemFullName(ItemTid, false)
    //                            , DBLocale.GetLocaleText("ItemInfo_Group_Type")
    //                            , DBLocale.GetItemTypeName(tableData.ItemType)
    //                            , DBLocale.GetLocaleText("ItemInfo_Group_Class")
    //                            , DBLocale.GetItemUseCharacterTypeName(ItemTid)
    //                            , DBLocale.GetLocaleText("ItemInfo_Group_Use")
    //                            , DBLocale.GetLocaleText(tableData.TooltipID)
    //                            , GetParseAbilitys(ItemTid, " ", " , ")) + strLevelLimit;
    //                            //, DBLocale.GetLocaleText("ItemInfo_Group_Move")
    //                            //, EnumHelper.CheckFlag(tableData.LimitType, E_LimitType.Storage) ? DBLocale.GetLocaleText("Wshop_ItemInfo_StorageOn") : DBLocale.GetLocaleText("Wshop_ItemInfo_StorageOff")
    //                            //, EnumHelper.CheckFlag(tableData.LimitType, E_LimitType.Trade) ? DBLocale.GetLocaleText("Wshop_ItemInfo_TradeOn") : DBLocale.GetLocaleText("Wshop_ItemInfo_TradeOff"));
    //        else
    //            return string.Format("- {0}\n{1} : {2}\n{3} : {4}\n{5} : {6}"//\n{7} : {8},{9}"
    //                            , GetItemFullName(ItemTid, false)
    //                            , DBLocale.GetLocaleText("ItemInfo_Group_Type")
    //                            , DBLocale.GetItemTypeName(tableData.ItemType)
    //                            , DBLocale.GetLocaleText("ItemInfo_Group_Class")
    //                            , DBLocale.GetItemUseCharacterTypeName(ItemTid)
    //                            , DBLocale.GetLocaleText("ItemInfo_Group_Use")
    //                            , DBLocale.GetLocaleText(tableData.TooltipID)) + strLevelLimit;
    //                            //, DBLocale.GetLocaleText("ItemInfo_Group_Move")
    //                            //, EnumHelper.CheckFlag(tableData.LimitType, E_LimitType.Storage) ? DBLocale.GetLocaleText("Wshop_ItemInfo_StorageOn") : DBLocale.GetLocaleText("Wshop_ItemInfo_StorageOff")
    //                            //, EnumHelper.CheckFlag(tableData.LimitType, E_LimitType.Trade) ? DBLocale.GetLocaleText("Wshop_ItemInfo_TradeOn") : DBLocale.GetLocaleText("Wshop_ItemInfo_TradeOff"));
    //    }
    //}

    //static List<uint> GachaTempList = new List<uint>();
    //static List<uint> GachaReturnList = new List<uint>();
    //public static List<uint> GetRandomEquipGachaList(uint ItemTid, int GetCount,byte minGrade = 0,byte maxGrade = 0)
    //{
    //    GachaTempList.Clear();
    //    GachaReturnList.Clear();

    //    var tableData = GameDBManager.Container.Item_Table_data[ItemTid];

    //    if (tableData != null && tableData.ItemType == E_ItemType.ShopGacha)
    //    {
    //        foreach (var shoplistGroupId in tableData.ShopListGroupID)
    //        {
    //            foreach (var shopListData in DBShop.GetShopList(shoplistGroupId))
    //            {
    //                var itemData = DBItem.GetItem(shopListData.GoodsItemID);

    //                if (shopListData.GoodsItemID != 0 && shopListData.GetRate > 0 && IsEquipItem(itemData.ItemType) &&
    //                    (minGrade == 0 || itemData.Grade >= minGrade) && (maxGrade == 0 || itemData.Grade <= maxGrade) &&
    //                    !GachaTempList.Contains(shopListData.GoodsItemID))
    //                    GachaTempList.Add(shopListData.GoodsItemID);
    //            }
    //        }
    //    }

    //    while (GachaReturnList.Count < GetCount && GachaTempList.Count >= (GetCount - GachaReturnList.Count))
    //    {
    //        GachaReturnList.Add(GachaTempList[UnityEngine.Random.Range(0, GachaTempList.Count - 1)]);
    //    }

    //    return GachaReturnList;
    //}

    //public static int GetItemDroupInfoCount(uint ItemTid)
    //{
    //    DropInfo dropInfo = GetItemDropInfo(ItemTid);

    //    if (dropInfo != null)
    //        return dropInfo.DropMonsters.Count + dropInfo.DropStages.Count;

    //    return 0;
    //}

    public static float GetItemCoolTime(uint ItemTid)
    {
        if (!GameDBManager.Container.Item_Table_data.ContainsKey(ItemTid))
        {
            //ZLog.LogError("GetItemCoolTime - Can't Find Item " + ItemTid);
            return 1f; // 없으면 최소값이라도 주자.
        }

        return GameDBManager.Container.Item_Table_data[ItemTid].CoolTime;
    }

    //public static DropInfo GetItemDropInfo(uint ItemTid)
    //{
    //    if (ItemDropDatas.ContainsKey(ItemTid))
    //        return ItemDropDatas[ItemTid];

    //    if (!GameDBManager.Container.Item_Table_data.ContainsKey(ItemTid))
    //    {
    //        ZLog.LogError("GetItemDropMonsterTids - Can't Find Item " + ItemTid);
    //        return null;
    //    }

    //    DropInfo NewDropInfo = new DropInfo() { ItemTid = ItemTid};

    //    if (!string.IsNullOrEmpty(GameDBManager.Container.Item_Table_data[ItemTid].DropTip))
    //    {
    //        string[] splits = GameDBManager.Container.Item_Table_data[ItemTid].DropTip.Split('|');

    //        for (int i = 0; i < splits.Length; i++)
    //        {
    //            switch (i)
    //            {
    //                case 0://monster
    //                    {
    //                        string[] splitmonsters = splits[i].Split(new string[] { "," }, System.StringSplitOptions.RemoveEmptyEntries);

    //                        for (int j = 0; j < splitmonsters.Length; j++)
    //                        {
    //                            uint monsterTid = uint.Parse(splitmonsters[j]);
    //                            var stageTid = DBMonster.GetPlaceStage(monsterTid);
    //                            var stage = DBStage.Get(stageTid);
    //                            if(null == stage || stage.UnusedType == E_UnusedType.Use)
    //                                NewDropInfo.DropMonsters.Add(monsterTid);
    //                        }
    //                    }
    //                    break;
    //                case 1://stage
    //                    {
    //                        string[] splitstages = splits[i].Split(new string[] { "," }, System.StringSplitOptions.RemoveEmptyEntries);

    //                        for (int j = 0; j < splitstages.Length; j++)
    //                        {
    //                            var stageTid = uint.Parse(splitstages[j]);
    //                            var stage = DBStage.Get(stageTid);
    //                            if (null != stage && stage.UnusedType == E_UnusedType.Use)
    //                                NewDropInfo.DropStages.Add(stageTid);
    //                        }
    //                    }
    //                    break;
    //            }
    //        }

    //    }
    //    ItemDropDatas.Add(ItemTid, NewDropInfo);

    //    return ItemDropDatas[ItemTid];
    //}

    public static List<Item_Table> GetGroupList(uint GroupIdx)
    {
        return ItemGroupDic[GroupIdx];
    }

    public static Item_Table GetBaseStepItem(uint GroupIdx)
    {
        Item_Table returnData = null;
        foreach (var tableData in ItemGroupDic[GroupIdx])
        {
            if (returnData == null || returnData.Step > tableData.Step)
                returnData = tableData;
        }

        return returnData;
    }

    //public static string GetParseAbilitys(uint ItemTid,string SplitValue="",string SplitAbility="")
    //{
    //    if (GameDBManager.Container.Item_Table_data.TryGetValue(ItemTid, out var tableData))
    //    {
    //        List<uint> AbilityActionIds = new List<uint>();
    //        if (tableData.AbilityActionID_01 != 0)
    //            AbilityActionIds.Add(tableData.AbilityActionID_01);
    //        if (tableData.AbilityActionID_02 != 0)
    //            AbilityActionIds.Add(tableData.AbilityActionID_02);
    //        if (tableData.AbilityActionID_03 != 0)
    //            AbilityActionIds.Add(tableData.AbilityActionID_03);

    //        string returnStr = "";
    //        Dictionary<GameDB.E_AbilityType, System.ValueTuple<float,float>> abilitys = new Dictionary<GameDB.E_AbilityType, System.ValueTuple<float, float>>();
    //        foreach (var aid in AbilityActionIds)
    //        {
    //            var abilityActionData = DBAbility.GetAction(aid);
    //            if (abilityActionData.AbilityViewType == GameDB.E_AbilityViewType.ToolTip)
    //            {
    //                if (string.IsNullOrEmpty(returnStr))
    //                    returnStr = string.Format("{0}", DBLocale.ParseAbilityTooltip(abilityActionData, "2", "1"));
    //                else
    //                    returnStr = string.Format("{0}{1}{2}", returnStr,SplitAbility, DBLocale.ParseAbilityTooltip(abilityActionData, "2", "1"));
    //            }
    //            else
    //            {
    //                var enumer = DBAbility.GetAllAbilityData(aid).GetEnumerator();
    //                while (enumer.MoveNext())
    //                {
    //                    if (abilitys.ContainsKey(enumer.Current.Key))
    //                        abilitys[enumer.Current.Key] = (abilitys[enumer.Current.Key].Item1 + enumer.Current.Value.Item1, abilitys[enumer.Current.Key].Item2 + enumer.Current.Value.Item2);
    //                    else
    //                        abilitys.Add(enumer.Current.Key, enumer.Current.Value);
    //                }
    //            }
    //        }

    //        foreach (var key in abilitys.Keys)
    //        {
    //            if (!DBAbility.IsParseAbility(key))
    //                continue;

    //            if (string.IsNullOrEmpty(returnStr))
    //                returnStr = string.Format("{0}{1}{2}", DBLocale.GetLocaleText(DBAbility.GetAbilityName(key)),SplitValue, DBAbility.ParseAbilityValue(key, abilitys[key].Item1, abilitys[key].Item2));
    //            else
    //                returnStr = string.Format("{0}{1}{2}{3}{4}", returnStr,SplitAbility,DBLocale.GetLocaleText(DBAbility.GetAbilityName(key)), SplitValue, DBAbility.ParseAbilityValue(key, abilitys[key].Item1, abilitys[key].Item2));
    //        }

    //        return returnStr;
    //    }

    //    ZLog.LogError("GetParseAbilitys - can't find ItemTid : "+ItemTid);
    //    return "";
    //}

    //public static Item_Table GetRandomEquipItem(byte Grade)
    //{
    //    if (itemDicByGrade.ContainsKey(Grade))
    //    {
    //        int countdown = 100;
    //        while (countdown > 0)
    //        {
    //            int index = UnityEngine.Random.Range(0, itemDicByGrade[Grade].Count - 1);
    //            Item_Table table = itemDicByGrade[Grade][index];

    //            --countdown;

    //            if (false == IsEquipItem(table.ItemType))
    //                continue;

    //            return table;
    //        }
    //    }

    //    return null;
    //}

    ///// <summary> 사용시 마일리지를 주는 아이템인지 여부 </summary>
    //public static bool IsGetMileageItem(uint tid)
    //{
    //    if (GameDBManager.Container.Item_Table_data.ContainsKey(tid))
    //    {
    //        return GameDBManager.Container.Item_Table_data[tid].ItemType == E_ItemType.ShopGacha;
    //    }
    //    ZLog.LogError("IsGetMileageItem - Can't Find ItemTid : " + tid);
    //    return false;
    //}

    //#region Trade
    //public static Dictionary<E_TradeTapType, Dictionary<E_TradeSubTapType, List<Item_Table>>>.KeyCollection GetTradeTaps()
    //{
    //    return TradeDicGroup.Keys;
    //}
    //public static Dictionary<E_TradeSubTapType, List<Item_Table>>.KeyCollection GetTradeSubTaps(E_TradeTapType tap)
    //{
    //    return TradeDicGroup.ContainsKey(tap) ? TradeDicGroup[tap].Keys : null;
    //}

    //public static List<Item_Table> GetTradeTapList(E_TradeTapType tap, E_TradeSubTapType subtap)
    //{
    //    return TradeDicGroup[tap][subtap];
    //}
    //#endregion

    //#region Search
    //public static List<Item_Table> FindItem(string str,bool bUseInitialSound = false)
    //{
    //    List<Item_Table> returnList = new List<Item_Table>();
    //    List<uint> AddGroupIDList = new List<uint>();
    //    List<string> searchInitialSoundList = new List<string>();

    //    bool bSearchInitialSound = true;
    //    if (bUseInitialSound)
    //    {
    //        for (int i = 0; i < str.Length; i++)
    //        {
    //            if (str[i] < 'ㄱ' || str[i] > 'ㅎ')
    //            {
    //                bSearchInitialSound = false;
    //                break;
    //            }
    //        }

    //        for (int i = 0; i < str.Length; i++)
    //        {
    //            searchInitialSoundList.Add(ZWord.FindInitialSound(str[i]));
    //        }
    //    }

    //    if (bSearchInitialSound)
    //    {
    //        foreach (uint ItemTid in DicInitialSound.Keys)
    //        {
    //            List<string> initialSounds = DicInitialSound[ItemTid];                
    //            bool bAllSame = true;

    //            string initialSoundStr = "";
    //            string searchInitialSoundStr = "";

    //            foreach (var value in initialSounds)
    //            {
    //                initialSoundStr += value;
    //            }

    //            foreach (var value in searchInitialSoundList)
    //            {
    //                searchInitialSoundStr += value;
    //            }

    //            bAllSame = initialSoundStr.Contains(searchInitialSoundStr);

    //            if (bAllSame)
    //            {
    //                var tableData = GameDBManager.Container.Item_Table_data[ItemTid];
    //                if (!AddGroupIDList.Contains(tableData.GroupID))
    //                {
    //                    AddGroupIDList.Add(tableData.GroupID);
    //                    returnList.Add(tableData);
    //                }
    //            }
    //        }
    //    }
    //    else
    //    {
    //        foreach (Item_Table tableData in GameDBManager.Container.Item_Table_data.Values)
    //        {
    //            if (tableData.TradeTapType == E_TradeTapType.None)
    //                continue;

    //            if (tableData.ItemTextID.Contains(str))
    //            {
    //                if (!AddGroupIDList.Contains(tableData.GroupID))
    //                {
    //                    AddGroupIDList.Add(tableData.GroupID);
    //                    returnList.Add(tableData);
    //                }
    //            }
    //        }
    //    }

    //    return returnList;
    //}
    //#endregion

    public static bool GetRuneSetTable(E_RuneSetType type, out Item_Table table)
    {
        return CachedRuneSetTypeDict.TryGetValue(type, out table);
    }

    //#region Rune
    ///// <summary> tid가 해당 룬 세트 타입인지 체크한다. </summary>
    //public static bool IsCheckRuneSetType(E_RuneSetType type, uint tid)
    //{
    //    return CachedRuneSetTypeDict[type].Contains(tid);
    //}
    //#endregion

    /// <summary> subType으로 weapon type을 뽑아온다. </summary>
    public static E_WeaponType GetItemWeaponType(E_ItemSubType subType)
    {
        //TODO :: 이거 규칙 변경되면 수정해야함....
        //return (E_WeaponType)(subType - 100);

        switch (subType)
        {
            case E_ItemSubType.Sword:
                return E_WeaponType.Sword;
            case E_ItemSubType.TwoSwords:
                return E_WeaponType.TwoSwords;
            case E_ItemSubType.Wand:
                return E_WeaponType.Wand;
            case E_ItemSubType.Bow:
                return E_WeaponType.Bow;
        }

        return E_WeaponType.None;
    }
}
