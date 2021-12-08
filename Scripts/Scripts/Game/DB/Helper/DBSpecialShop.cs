using System;
using GameDB;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

[UnityEngine.Scripting.Preserve]
public class DBSpecialShop : IGameDBHelper
{
    public class ItemBuyInfo
    {
        public uint shopTid;
        public uint buyCnt;
        public ulong useItemId;
        public uint useItemTid;
        public uint selectItemTid;

        public ItemBuyInfo() { }
        public ItemBuyInfo(uint shopTid, uint buyCnt, ulong useItemId, uint useItemTid, uint selectItemTid)
        {
            this.shopTid = shopTid;
            this.buyCnt = buyCnt;
            this.useItemId = useItemId;
            this.useItemTid = useItemTid;
            this.selectItemTid = selectItemTid;
        }

        public void Set(uint shopTid, uint buyCnt, ulong useItemId, uint useItemTid, uint selectItemTid)
        {
            this.shopTid = shopTid;
            this.buyCnt = buyCnt;
            this.useItemId = useItemId;
            this.useItemTid = useItemTid;
            this.selectItemTid = selectItemTid;
        }

        public void Reset()
        {
            shopTid = 0;
            buyCnt = 0;
            useItemId = 0;
            useItemTid = 0;
            selectItemTid = 0;
        }
    }

    /// <summary>
    /// 스페셜 상점에서 출력하는 아이템 하나의 데이터 소스
    /// 이 타입 하나로 아이템을 출력하기 위해 참조해야할 테이블을 알 수 있어야함.
    /// </summary>
    public enum E_SpecialShopDisplayGoodsTarget
    {
        None = 0,
        RateItem,
        Item,
        Change,
        Pet,
    }

    static Dictionary<E_ViewType, Dictionary<E_SpecialShopType, List<SpecialShop_Table>>> specialShopList = new Dictionary<E_ViewType, Dictionary<E_SpecialShopType, List<SpecialShop_Table>>>();
    //static Dictionary<E_SpecialShopType, List<SpecialShop_Table>> specialShopList = new Dictionary<E_SpecialShopType, List<SpecialShop_Table>>();
    static Dictionary<E_SpecialShopType, Dictionary<E_SpecialSubTapType, List<SpecialShop_Table>>> specialSubTabShopList = new Dictionary<E_SpecialShopType, Dictionary<E_SpecialSubTapType, List<SpecialShop_Table>>>();

    static Dictionary<uint, List<SpecialShop_Table>> sepcialShopLimitList = new Dictionary<uint, List<SpecialShop_Table>>();

    static List<SpecialShop_Table> recommendList = new List<SpecialShop_Table>();
    static List<SpecialShop_Table> cashGoodsList = new List<SpecialShop_Table>();

    public void OnReadyData()
    {
        specialShopList.Clear();
        specialSubTabShopList.Clear();
        recommendList.Clear();
        cashGoodsList.Clear();
        sepcialShopLimitList.Clear();

        foreach (var tableData in GameDBManager.Container.SpecialShop_Table_data.Values)
        {
            if (tableData.UnusedType == E_UnusedType.Unuse)
                continue;

            if (tableData.RecommendGoods != 0)
            {
                recommendList.Add(tableData);
            }

            if (!string.IsNullOrEmpty(tableData.GoogleID) || !string.IsNullOrEmpty(tableData.OneStoreID))
                cashGoodsList.Add(tableData);

            if (!specialShopList.ContainsKey(tableData.ViewType))
                specialShopList.Add(tableData.ViewType, new Dictionary<E_SpecialShopType, List<SpecialShop_Table>>());

            if (!specialShopList[tableData.ViewType].ContainsKey(tableData.SpecialShopType))
                specialShopList[tableData.ViewType].Add(tableData.SpecialShopType, new List<SpecialShop_Table>());
            specialShopList[tableData.ViewType][tableData.SpecialShopType].Add(tableData);


            if (!specialSubTabShopList.ContainsKey(tableData.SpecialShopType))
                specialSubTabShopList.Add(tableData.SpecialShopType, new Dictionary<E_SpecialSubTapType, List<SpecialShop_Table>>());
            if (!specialSubTabShopList[tableData.SpecialShopType].ContainsKey(tableData.SpecialSubTapType))
                specialSubTabShopList[tableData.SpecialShopType].Add(tableData.SpecialSubTapType, new List<SpecialShop_Table>());
            specialSubTabShopList[tableData.SpecialShopType][tableData.SpecialSubTapType].Add(tableData);

            if (tableData.SpecialShopCondition != 0)
            {
                if (!sepcialShopLimitList.ContainsKey(tableData.SpecialShopCondition))
                    sepcialShopLimitList.Add(tableData.SpecialShopCondition, new List<SpecialShop_Table>());

                sepcialShopLimitList[tableData.SpecialShopCondition].Add(tableData);
            }
        }

        recommendList.Sort((x, y) =>
        {
            if (x.RecommendGoods < y.RecommendGoods)
                return -1;
            else if (x.RecommendGoods > y.RecommendGoods)
                return 1;

            return 0;
        });
    }

    public static Dictionary<uint, SpecialShop_Table> DicShop
    {
        get { return GameDBManager.Container.SpecialShop_Table_data; }
    }

    public static bool TryGet(uint _tid, out SpecialShop_Table outTable)
    {
        return GameDBManager.Container.SpecialShop_Table_data.TryGetValue(_tid, out outTable);
    }

    public static SpecialShop_Table Get(uint _tid)
    {
        if (GameDBManager.Container.SpecialShop_Table_data.TryGetValue(_tid, out var foundTable))
        {
            return foundTable;
        }
        return null;
    }

    public static List<SpecialShop_Table> GetShopList(E_SpecialShopType shopType)
    {
        var tableList = GameDBManager.Container.SpecialShop_Table_data.Values.ToList();
        if (tableList == null)
        {
            return null;
        }
        return tableList.FindAll(v => v.SpecialShopType == shopType);
    }

    public static List<SpecialShop_Table> GetShopList(E_SpecialShopType shopType, E_SpecialSubTapType subTabType)
    {
        var tableList = GameDBManager.Container.SpecialShop_Table_data.Values.ToList();
        if (tableList == null)
        {
            return null;
        }
        return tableList.FindAll(v => v.SpecialShopType == shopType && v.SpecialSubTapType == subTabType);
    }

    public static List<SpecialShop_Table> GetRecommendedList()
    {
        return recommendList;
    }

    public static List<SpecialShop_Table> GetCashGoodsList()
    {
        return cashGoodsList;
    }

    public static string GetStoreProductIDByPlatform(uint tid)
    {
        var data = Get(tid);

        if (data == null)
            return string.Empty;

        string productID = string.Empty;

        switch (NTCore.CommonAPI.StoreCD)
        {
            case NTCore.StoreCD.GOOGLE_PLAY: productID = data.GoogleID; break;
            case NTCore.StoreCD.APPLE_APP_STORE: productID = data.IOSStoreID; break;
            case NTCore.StoreCD.ONESTORE: productID = data.OneStoreID; break;
            default:
                break;
        }

        return productID;
    }

    #region Util
    public static void ForeachAllCategories(Action<E_SpecialShopType, E_SpecialSubTapType, SpecialShop_Table> loopCallBack)
    {
        if (loopCallBack == null)
            return;

        foreach (var byShopType in specialSubTabShopList)
        {
            foreach (var bySubTabType in byShopType.Value)
            {
                foreach (var tableData in bySubTabType.Value)
                {
                    loopCallBack(byShopType.Key, bySubTabType.Key, tableData);
                }
            }
        }
    }

    public static void ForeachMainCategory(Action<E_SpecialShopType, Dictionary<E_SpecialSubTapType, List<SpecialShop_Table>>> loopCallBack)
	{
        if (loopCallBack == null)
            return;

        foreach (var byShopType in specialSubTabShopList)
        {
            loopCallBack(byShopType.Key, byShopType.Value);
        }
    }

    public static void ForeachMainCategoryTypes(Action<E_SpecialShopType> loopCallBack)
    {
        if (loopCallBack == null)
            return;

        foreach (var byShopType in specialSubTabShopList)
        {
            loopCallBack(byShopType.Key);
        }
    }

    /// <summary>
    /// 해당 메인 카테고리의 출력 여부 체킹 
    /// </summary>
    public static bool IsMainTabDisplayable(E_SpecialShopType shopType)
    {
        if (specialSubTabShopList.ContainsKey(shopType) == false)
            return false;

        foreach (var bySubTab in specialSubTabShopList[shopType])
        {
            foreach (var data in bySubTab.Value)
            {
                if (data.SpecialShopType != E_SpecialShopType.None && data.UnusedType == E_UnusedType.Use && data.ViewType == E_ViewType.View)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// 해당 아이템의 출력 가능 여부를 체킹함 . 
    /// </summary>
    public static bool IsItemDisplayable(uint tid)
    {
        if (GameDBManager.Container.SpecialShop_Table_data.ContainsKey(tid) == false)
            return false;

        var data = GameDBManager.Container.SpecialShop_Table_data[tid];

        if (data.SpecialShopType != E_SpecialShopType.None && data.UnusedType == E_UnusedType.Use && data.ViewType == E_ViewType.View)
        {
            return true;
        }

        return false;
    }

    //public static bool ValidateGoodsData(uint specialShopTid)
    //{
    //    var data = Get(specialShopTid);

    //    if (data == null)
    //        return false;


    //    bool isValidate = true;
    //    switch (data.GoodsListGetType)
    //    {
    //        /// None 일때는 Goods TID 로 가져옴 
    //        case E_GoodsListGetType.None:
    //            {
    //                switch (data.GoodsKindType)
    //                {
    //                    case E_GoodsKindType.None:
    //                        {
    //                            isValidate = false;
    //                            // ZLog.LogError(ZLogChannel.UI, "Target Data Type is not specified, can't resolve");
    //                        }
    //                        break;
    //                    case E_GoodsKindType.Item:
    //                        {
    //                            var target = DBItem.GetItem(data.GoodsItemID);
    //                            if (target == null)
    //                            {
    //                                isValidate = false;
    //                                // ZLog.LogError(ZLogChannel.UI, "TargetItem Not Exist in the table referenced by SpecialShopTable, ItemTid : " + data.GoodsItemID);
    //                            }
    //                            //else
    //                            //{
    //                            //    result = target.ItemTextID;
    //                            //}
    //                        }
    //                        break;
    //                    case E_GoodsKindType.Change:
    //                        {
    //                            var target = DBChange.Get(data.GoodsChangeID);
    //                            if (target == null)
    //                            {
    //                                isValidate = false;
    //                                //  ZLog.LogError(ZLogChannel.UI, "TargetChange Not Exist in the table referenced by SpecialShopTable, ItemTid : " + data.GoodsChangeID);
    //                            }
    //                            //else
    //                            //{
    //                            //    result = target.ChangeTextID;
    //                            //}
    //                        }
    //                        break;
    //                    case E_GoodsKindType.Pet:
    //                        {
    //                            var target = DBPet.GetPetData(data.GoodsPetID);
    //                            if (target == null)
    //                            {
    //                                isValidate = false;
    //                                // ZLog.LogError(ZLogChannel.UI, "TargetPet Not Exist in the table referenced by SpecialShopTable, ItemTid : " + data.GoodsPetID);
    //                            }
    //                            //else
    //                            //{
    //                            //    result = target.PetTextID;
    //                            //}
    //                        }
    //                        break;
    //                    default:
    //                        break;
    //                }
    //            }
    //            break;
    //        case E_GoodsListGetType.All:
    //            {
    //                // ZLog.LogError(ZLogChannel.UI, "Not handle yet , update please");
    //            }
    //            break;
    //        case E_GoodsListGetType.Select:
    //            {
    //                // ZLog.LogError(ZLogChannel.UI, "Not handle yet , update please");
    //            }
    //            break;
    //        case E_GoodsListGetType.Rate:
    //            {

    //            }
    //            break;
    //    }

    //    /// 현금 결제 아이템일때는 예외적으로 DiamondCount 한번더 체킹함 
    //    /// 현금 결제면은 GoodsXX 쪽에 ID 가 있거나 없을수 있음 .
    //    if (isValidate == false)
    //    {
    //        /// 현금 결제 && Diamond Count 가 0 을 넘는다 -> Valid 데이터 
    //        if (data.CashType == E_CashType.Cash && data.DiamondCount > 0)
    //        {
    //            isValidate = true;
    //        }
    //    }

    //    return isValidate;
    //}

    /// <summary>
    /// 데이터에 맞게 상점에서 출력할 아이템의 LocaleTextKey 를 가져옴 .
    /// </summary>
    public static bool GetGoodsPropsBySwitching(uint specialShopTid
        , ref E_CashType resultPaymentType
        , ref E_SpecialShopDisplayGoodsTarget resultGoodsType
        , ref string resultTextKey
        , ref string resultIconID
        , ref byte resultTargetGrade
        , ref uint resultGoodsTid)
    {
        var specialShopData = Get(specialShopTid);

        if (specialShopData == null)
            return false;

        /// ..
        /// if (ValidateGoodsData(specialShopTid) == false)
        ///   return false;

        resultPaymentType = specialShopData.CashType;

        bool result = true;

        switch (specialShopData.GoodsListGetType)
        {
            /// None 일때는 Goods TID 로 가져옴 
            case E_GoodsListGetType.None:
                {
                    switch (specialShopData.GoodsKindType)
                    {
                        case E_GoodsKindType.Item:
                            {
                                /// 예로 Item 타입인데 현금 결제인 경우는 GoodsID 가 0 일수있음 
                                var target = DBItem.GetItem(specialShopData.GoodsItemID);

                                if (target != null)
                                {
                                    resultTextKey = target.ItemTextID;
                                    resultIconID = target.IconID;
                                    resultGoodsTid = specialShopData.GoodsItemID;
                                    resultGoodsType = E_SpecialShopDisplayGoodsTarget.Item;
                                    resultTargetGrade = target.Grade;
                                }
                                else
                                {
                                    result = false;
                                }
                            }
                            break;
                        case E_GoodsKindType.Change:
                            {
                                var target = DBChange.Get(specialShopData.GoodsChangeID);
                                if (target != null)
                                {
                                    resultTextKey = target.ChangeTextID;
                                    resultIconID = target.Icon;
                                    resultGoodsTid = specialShopData.GoodsChangeID;
                                    resultGoodsType = E_SpecialShopDisplayGoodsTarget.Change;
                                    resultTargetGrade = target.Grade;
                                }
                                else
                                {
                                    result = false;
                                }
                            }
                            break;
                        case E_GoodsKindType.Pet:
                            {
                                var target = DBPet.GetPetData(specialShopData.GoodsPetID);
                                if (target != null)
                                {
                                    resultTextKey = target.PetTextID;
                                    resultIconID = target.Icon;
                                    resultGoodsTid = specialShopData.GoodsPetID;
                                    resultGoodsType = E_SpecialShopDisplayGoodsTarget.Pet;
                                    resultTargetGrade = target.Grade;
                                }
                                else
                                {
                                    result = false;
                                }
                            }
                            break;
                    }
                }
                break;
            case E_GoodsListGetType.All:
                {
                    result = false;
                    ZLog.LogError(ZLogChannel.UI, "Not handle yet , update please");
                }
                break;
            case E_GoodsListGetType.Select:
                {
                    result = false;
                    ZLog.LogError(ZLogChannel.UI, "Not handle yet , update please");
                }
                break;
            case E_GoodsListGetType.Rate:
                {
                    resultTextKey = specialShopData.ShopTextID;
                    resultIconID = specialShopData.IconID;
                    resultGoodsTid = 0;
                    resultGoodsType = E_SpecialShopDisplayGoodsTarget.RateItem;
                }
                break;
        }

        /// 추가 기획 : *SpecialShopType 이 Essence 가 아니라면* SpecialShop 에 명시된 정보로 가져옴 . 외부 X
        if (specialShopData.SpecialShopType != E_SpecialShopType.Essence)
        {
            resultTextKey = specialShopData.ShopTextID;
            resultIconID = specialShopData.IconID;
        }

        /// 특정 테이블 데이터를 가져오지 못했는데 캐쉬 타입 && 다이아 카운트가 있다 -> 다이아몬드 상품 (이상하지만 기획사항임)
        if (result == false
            && specialShopData.CashType == E_CashType.Cash
            && specialShopData.DiamondCount > 0)
        {
            result = true;
            resultTextKey = specialShopData.ShopTextID;
            resultGoodsTid = DBConfig.Diamond_ID;
            resultGoodsType = E_SpecialShopDisplayGoodsTarget.Item;
            resultIconID = specialShopData.IconID;
        }

        return result;
    }

    public static bool IsForSaleByDate(uint tid)
    {
        var data = Get(tid);

        if (data == null)
            return false;

        /// 구매에 제약이 없다 -> True 
        /// 판매 시작 시간이 0 이다 
        /// 판매 종료 시간이 0 이다 
        if (data.BuyLimitType == E_BuyLimitType.Infinite
            || data.BuyStartTime == 0
            || data.BuyFinishTime == 0)
            return true;

        return TimeManager.NowMs >= data.BuyStartTime && TimeManager.NowMs < data.BuyFinishTime;
    }

    /// <summary>
    /// 주어진 ItemGroupList 를 가지고 잠긴건지 체킹해줌 
    /// </summary>
    public static bool IsLocked(uint specialShopTid, List<uint> refItemGroupList)
    {
        var data = Get(specialShopTid);

        if (data == null)
            return false;

        /// Get 이 아니거나 , Essence 가 아니면 잠김 기능 X  
        if (data.BuyOpenType != E_BuyOpenType.Get
            || data.SpecialShopType != E_SpecialShopType.Essence)
        {
            return false;
        }

        var itemData = DBItem.GetItem(data.GoodsItemID);

        if (itemData == null)
        {
            ZLog.LogError(ZLogChannel.UI, "could not get item data referenced by SpecialShop GoodsItemID due to lock feature");
            return false;
        }

        /// 주어진 List 에 해당 아이템 GroupID 가 존재하지않는다 -> 잠김 상태로 판정
        return refItemGroupList.Contains(itemData.GroupID) == false;
    }

    //public static bool IsBuyable()
    //{

    //}

    #endregion
}
