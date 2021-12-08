using GameDB;
using System.Collections.Generic;

[UnityEngine.Scripting.Preserve]
class DBNormalShop : IGameDBHelper
{
    private static Dictionary<uint, NormalShop_Table> dicNormalShop = new Dictionary<uint, NormalShop_Table>();

    public void OnReadyData()
    {
        dicNormalShop = GameDBManager.Container.NormalShop_Table_data;
    }

    public static List<NormalShop_Table> GetShopDataList(E_ShopType shopType, E_NormalShopType normalshopType)
    {
        List<NormalShop_Table> listTable = new List<NormalShop_Table>();

        foreach(var iter in dicNormalShop.Values)
        {
            if (iter.ShopType != shopType)
                continue;

            if (iter.NormalShopType != normalshopType)
                continue;

            listTable.Add(iter);
        }

        return listTable;   
    }

    public static NormalShop_Table GetShopData(uint shopTid, E_ShopType shopType, E_NormalShopType normnalShopType)
    {
        foreach(var iter in dicNormalShop.Values)
        {
            if (iter.ShopType != shopType)
                continue;

            if (iter.NormalShopType != normnalShopType)
                continue;

            if (iter.NormalShopID == shopTid)
                return iter;
        }

        return null;
    }
}
