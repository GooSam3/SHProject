using Devcat;
using GameDB;
using System;
using System.Collections.Generic;

[UnityEngine.Scripting.Preserve]
public class DBMileageShop : IGameDBHelper
{
    public class ItemBuyInfo
    {
        public uint shopTid;
        public uint buyCnt;
        public ulong useItemId;
        public uint useItemTid;
        public uint selectShopListTid;
        public uint selectListGroupTid;

        public ItemBuyInfo(uint shopTid, uint buyCnt, ulong useItemId, uint useItemTid, uint selectShopListTid, uint selectListGroupTid)
        {
            this.shopTid = shopTid;
            this.buyCnt = buyCnt;
            this.useItemId = useItemId;
            this.useItemTid = useItemTid;
            this.selectShopListTid = selectShopListTid;
            this.selectListGroupTid = selectListGroupTid;
        }
    }

    public class SubCategoryData
    {
        public uint ShopID;
        public string TextID;
        public uint ShopListID;
        public E_UnusedType UnusedType;

        public SubCategoryData(uint shopID, string textID, uint shopListID, E_UnusedType unusedType)
        {
            ShopID = shopID;
            TextID = textID;
            ShopListID = shopListID;
            UnusedType = unusedType;
        }
    }

    public class BuyItemIDCountPair
    {
        public uint id;
        public uint count;
    }

    static EnumDictionary<E_MileageShopType, List<MileageShop_Table>> mileageShopTypeDic = new EnumDictionary<E_MileageShopType, List<MileageShop_Table>>();
    // static EnumDictionary<E_MileageShopType, string>
    static List<BuyItemIDCountPair> buyItemIDCountPairList = new List<BuyItemIDCountPair>();

    public void OnReadyData()
    {
        mileageShopTypeDic.Clear();
        buyItemIDCountPairList.Clear();

        foreach (var tableData in GameDBManager.Container.MileageShop_Table_data.Values)
        {
            if (!mileageShopTypeDic.ContainsKey(tableData.MileageShopType))
                mileageShopTypeDic.Add(tableData.MileageShopType, new List<MileageShop_Table>());

            mileageShopTypeDic[tableData.MileageShopType].Add(tableData);

            if (buyItemIDCountPairList.Exists(t => t.id == tableData.BuyItemID) == false)
            {
                buyItemIDCountPairList.Add(new BuyItemIDCountPair() { id = tableData.BuyItemID, count = tableData.BuyItemCount });
            }
        }

        foreach (var list in mileageShopTypeDic.Values)
        {
            /// Tab 순서대로 정렬함 
            list.Sort((t01, t02) =>
            {
                return t01.PositionNumber.CompareTo(t02.PositionNumber);
            });
        }
    }

    public static MileageShop_Table GetDataByTID(uint tid)
    {
        if (GameDBManager.Container.MileageShop_Table_data.ContainsKey(tid))
            return GameDBManager.Container.MileageShop_Table_data[tid];
        return null;
    }

    public static int GetTableDataCount()
    {
        return GameDBManager.Container.MileageShop_Table_data.Count;
    }

    public static void ForeachAllData(Action<MileageShop_Table> callback)
    {
        foreach (var data in GameDBManager.Container.MileageShop_Table_data.Values)
        {
            callback?.Invoke(data);
        }
    }

    public static void ForeachAllBuyItemID(Action<BuyItemIDCountPair> callback)
    {
        foreach (var data in buyItemIDCountPairList)
        {
            callback(data);
        }
    }

    public static uint GetBuyItemCount(uint buyItemID)
    {
        var target = buyItemIDCountPairList.Find(t => t.id == buyItemID);

        if (target == null)
            return 0;

        return target.count;
    }

    public static List<MileageShop_Table> GetTableDataByShopType(E_MileageShopType type)
    {
        if (mileageShopTypeDic.ContainsKey(type) == false)
            return null;
        return mileageShopTypeDic[type];
    }

    public static List<SubCategoryData> GetSubCategories(E_MileageShopType type)
    {
        if (mileageShopTypeDic.ContainsKey(type) == false)
            return null;

        var list = new List<SubCategoryData>(mileageShopTypeDic[type].Count);

        foreach (var t in mileageShopTypeDic[type])
        {
            list.Add(new SubCategoryData(t.MileageShopID, t.ShopTextID, t.ShopListID, t.UnusedType));
        }

        return list;
    }

    public static E_MileageShopType GetMileageShopTypeByShopTID(uint tid)
    {
        return GetDataByTID(tid).MileageShopType;
    }

    public List<BuyItemIDCountPair> GetBuyItemIDCountPair()
    {
        return buyItemIDCountPairList;
    }


    //public static bool IsMainCategoryValid(E_MileageShopType type)
    //{

    //}

    //public static bool IsSubCategoryValid()
    //{

    //}
}
