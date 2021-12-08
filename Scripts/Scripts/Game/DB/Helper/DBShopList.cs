using Devcat;
using GameDB;
using System.Collections.Generic;

[UnityEngine.Scripting.Preserve]
public class DBShopList : IGameDBHelper
{
    static Dictionary<uint, ShopList_Table> ShopListDic = new Dictionary<uint, ShopList_Table>();
    static Dictionary<uint, List<ShopList_Table>> ShopListDicByGroupID = new Dictionary<uint, List<ShopList_Table>>();
    public void OnReadyData()
    {
        ShopListDic.Clear();
        ShopListDicByGroupID.Clear();

        foreach (var tableData in GameDBManager.Container.ShopList_Table_data.Values)
        {
            if (!ShopListDic.ContainsKey(tableData.ShopListID))
                ShopListDic.Add(tableData.ShopListID, tableData);

            ShopListDic[tableData.ShopListID] = tableData;

            if (ShopListDicByGroupID.ContainsKey(tableData.GroupID) == false)
            {
                ShopListDicByGroupID.Add(tableData.GroupID, new List<ShopList_Table>());
            }

            var listByGroupID = ShopListDicByGroupID[tableData.GroupID];
            listByGroupID.Add(tableData);
        }
    }

    public static ShopList_Table GetDataByTID(uint tid)
    {
        if (GameDBManager.Container.ShopList_Table_data.ContainsKey(tid))
            return GameDBManager.Container.ShopList_Table_data[tid];
        return null;
    }

    public static List<ShopList_Table> GetDataListByGroupID(uint groupTid)
    {
        if (ShopListDicByGroupID.ContainsKey(groupTid) == false)
            return new List<ShopList_Table>();
        return ShopListDicByGroupID[groupTid];
    }

	public static uint GetShopListIDByGoodsID(uint goodsTid)
	{
		if (goodsTid == 0)
			return 0;

		foreach (var t in ShopListDic)
		{
			//if (t.Value.GoodsItemID != 0)
			//{
			//	if (goodsTid == t.Value.GoodsItemID)
			//	{
			//		return t.Key;
			//	}
			//}
			//else if (t.Value.GoodsChangeID != 0)
			//{
			//	if (goodsTid == t.Value.GoodsChangeID)
			//	{
			//		return t.Key;
			//	}
			//}
			//else if (t.Value.GoodsPetID != 0)
			//{
			//	if (goodsTid == t.Value.GoodsPetID)
			//	{
			//		return t.Key;
			//	}
			//}
			//else if (t.Value.GoodsRuneID != 0)
			//{
			//	if (goodsTid == t.Value.GoodsRuneID)
			//	{
			//		return t.Key;
			//	}
			//}

			if(DBListGroup.IsGoodsExistByListGroupID(t.Value.ListGroupID, goodsTid))
			{
				return t.Key;
			}
		}

		return 0;
	}

	public static bool IsItem(uint tid)
	{
		//	if (GameDBManager.Container.ShopList_Table_data.ContainsKey(tid))
		//	{
		//		var data = GameDBManager.Container.ShopList_Table_data[tid];

		//		if (data.GoodsItemID > 0)
		//			return true;
		//		else return false;
		//	}

		//	return false;

		if (ShopListDic.ContainsKey(tid) == false)
			return false;

		return DBListGroup.IsItemByListGroupID(ShopListDic[tid].ListGroupID);
	}

	public static bool IsEquipmentItem(uint tid)
	{
		//if (IsItem(tid) == false)
		//	return false;

		//var itemData = DBItem.GetItem(GameDBManager.Container.ShopList_Table_data[tid].GoodsItemID);
		//if (itemData == null)
		//return false;

		//return itemData.ItemUseType == E_ItemUseType.Equip;
		var data = GetDataByTID(tid);
		if (data == null)
			return false;
		return DBListGroup.IsEquippmentItem(data.ListGroupID);
	}

	public static bool IsEquipGemItem(uint tid)
	{
		//if (IsItem(tid) == false)
		//	return false;
		var data = GetDataByTID(tid);
		if (data == null)
			return false;
		return DBListGroup.IsGemItem(data.ListGroupID);
		//return DBItem.IsEquipGem(GameDBManager.Container.ShopList_Table_data[tid].GoodsItemID);
	}

	public static bool IsAccessoryItemByShopListID(uint tid)
	{
		//if (IsItem(tid) == false)
		//	return false;

		//return DBItem.IsAccessory(GameDBManager.Container.ShopList_Table_data[tid].GoodsItemID);
		var data = GetDataByTID(tid);
		if (data == null)
			return false;
		return DBListGroup.IsAccessoryItem(data.ListGroupID);
	}

	public static bool IsAccessoryItemByShopListGroupID(uint groupID)
	{
		//if (ShopListDicByGroupID.ContainsKey(groupID) == false
		//	|| ShopListDicByGroupID[groupID].Count == 0)
		//	return false;

		//return DBItem.IsAccessory(ShopListDicByGroupID[groupID][0].GoodsItemID);
		if (ShopListDicByGroupID.ContainsKey(groupID) == false
			|| ShopListDicByGroupID[groupID].Count == 0)
			return false;

		return DBItem.IsAccessory(ShopListDicByGroupID[groupID][0].ListGroupID);
	}

	public static bool IsMileageExist(IEnumerable<uint> tids)
    {
        foreach (var tid in tids)
        {
            var data = GetDataByTID(tid);
            if (data != null && data.MileageItemID != 0)
            {
                return true;
            }
        }

        return false;
    }

	public static uint FindListGroupListTIDByShopListIDAndGoods(uint shopListID, uint goodsTid)
	{
		if (ShopListDic.ContainsKey(shopListID) == false)
			return 0;

		return DBListGroup.GetListIDBySearchGoods(ShopListDic[shopListID].ListGroupID, goodsTid) ;
	}
}