using GameDB;
using System;
using System.Collections.Generic;
using System.Linq;
/// <summary>
/// </summary>
[UnityEngine.Scripting.Preserve]
public class DBListGroup : IGameDBHelper
{
    static Dictionary<uint, List<ListGroup_Table>> dicListGroup = new Dictionary<uint, List<ListGroup_Table>>();

    public void OnReadyData()
    {
		dicListGroup.Clear();

		Dictionary<uint, ListGroup_Table>.Enumerator it = GameDBManager.Container.ListGroup_Table_data.GetEnumerator();
        while (it.MoveNext())
        {
            ListGroup_Table listGroup = it.Current.Value;
            uint listGroupID = listGroup.ListGroupID;

            List<ListGroup_Table> listListGroupTable = FindOrAllocListGroup(listGroupID);
            listListGroupTable.Add(listGroup);
        }
    }

    private static List<ListGroup_Table> FindOrAllocListGroup(uint _listGroupID)
    {
        List<ListGroup_Table> findList = null;
        if (dicListGroup.ContainsKey(_listGroupID))
        {
            findList = dicListGroup[_listGroupID];
        }
        else
        {
            findList = new List<ListGroup_Table>();
            dicListGroup.Add(_listGroupID, findList);
        }
        return findList;
    }

	public static List<ListGroup_Table> GetData(uint listGroupID)
	{
		if (dicListGroup.ContainsKey(listGroupID) == false)
			return new List<ListGroup_Table>();

        return dicListGroup[listGroupID];

    }

	/// <summary>
	/// ItemID 로 일치하는 ListGroupID 찾음 
	/// </summary>
	public static bool FindListGroupIDByItemID(uint targetTid, out uint resultListGroupID)
	{
		resultListGroupID = 0;

		foreach (var t in dicListGroup)
		{
			foreach (var data in t.Value)
			{
				if (data.ItemID == targetTid)
				{
					resultListGroupID = t.Key;
					return true;
				}
			}
		}

		return false;
	}

	/// <summary>
	/// ChangeID 로 일치하는 ListGroupID 찾음 
	/// </summary>
	public static bool FindListGroupIDByChangeID(uint targetTid, out uint resultListGroupID)
    {
        resultListGroupID = 0;

        foreach (var t in dicListGroup)
        {
            foreach (var data in t.Value)
            {
                if (data.ChangeID == targetTid)
                {
                    resultListGroupID = t.Key;
                    return true; 
                }
            }
        }

        return false; 
    }

    /// <summary>
    /// PetID 로 일치하는 ListGroupID 찾음 
    /// </summary>
    public static bool FindListGroupIDByPetID(uint targetTid, out uint resultListGroupID)
    {
        resultListGroupID = 0;

        foreach (var t in dicListGroup)
        {
            foreach (var data in t.Value)
            {
                if (data.PetID == targetTid)
                {
                    resultListGroupID = t.Key;
                    return true; 
                }
            }
        }

        return false; 
    }

    public static bool IsGoodsExistByListGroupID(uint listGroupID, uint goodsTid)
	{
        var data = GetData(listGroupID);

		if (data == null)
			return false;

		foreach (var t in data)
		{
			if (t.ItemID != 0)
			{
				if (t.ItemID == goodsTid)
				{
                    return true; 
				}
			}
			else if (t.ChangeID != 0)
			{
				if (t.ChangeID == goodsTid)
				{
                    return true;
                }
			}
			else if (t.PetID != 0)
			{
				if (t.PetID == goodsTid)
				{
                    return true;
                }
			}
			else if (t.RuneID != 0)
			{
				if (t.RuneID == goodsTid)
				{
                    return true;
                }
			}
		}

        return false;
	}

	/// <summary>
	/// RuneID 로 일치하는 ListGroupID 찾음 
	/// </summary>
	public static bool FindListGroupIDByRuneID(uint targetTid, out uint resultListGroupID)
	{
		resultListGroupID = 0;

		foreach (var t in dicListGroup)
		{
			foreach (var data in t.Value)
			{
				if (data.RuneID == targetTid)
				{
					resultListGroupID = t.Key;
					return true;
				}
			}
		}

		return false;
	}

	public static void ForeachListGroupID(uint id, Action<ListGroup_Table> callback)
	{
		if (dicListGroup.ContainsKey(id) == false)
			return;

		foreach (var t in dicListGroup[id])
		{
			callback(t);
		}
	}

	/// <summary>
	/// 해당 리스트 그룹의 첫 번째 아이템 기준으로 장비인지 체킹 
	/// </summary>
	public static bool IsEquippmentItem(uint listGroupID)
	{
		if (dicListGroup.ContainsKey(listGroupID) == false
            || dicListGroup[listGroupID].Count == 0)
			return false;

        uint itemID = dicListGroup[listGroupID].First().ItemID;
        return DBItem.IsEquipItem(itemID);
	}
    /// <summary>
    /// 해당 리스트 그룹의 첫 번째 아이템 기준으로 젬인지 체킹 
    /// </summary>
    public static bool IsGemItem(uint listGroupID)
	{
        if (dicListGroup.ContainsKey(listGroupID) == false
            || dicListGroup[listGroupID].Count == 0)
            return false;

        uint itemID = dicListGroup[listGroupID].First().ItemID;
        return DBItem.IsEquipGem(itemID);
    }

	public static bool IsItemByListGroupID(uint listGroupID)
	{
        if (dicListGroup.ContainsKey(listGroupID) == false
    || dicListGroup[listGroupID].Count == 0)
            return false;

        uint itemID = dicListGroup[listGroupID].First().ItemID;
        return itemID != 0;
    }

    /// <summary>
    /// 해당 리스트 그룹의 첫 번째 아이템 기준으로 악세서리인지 체킹
    /// </summary>
    public static bool IsAccessoryItem(uint listGroupID)
	{
        if (dicListGroup.ContainsKey(listGroupID) == false
            || dicListGroup[listGroupID].Count == 0)
            return false;

        uint itemID = dicListGroup[listGroupID].First().ItemID;
        return DBItem.IsAccessory(itemID);
    }

	public static bool IsChangeByListGroupID(uint listGroupID)
	{
		if (dicListGroup.ContainsKey(listGroupID) == false
            || dicListGroup[listGroupID].Count == 0)
            return false;

		uint itemID = dicListGroup[listGroupID].First().ChangeID;
		return itemID != 0;
	}

	public static bool IsPetByListGroupID(uint listGroupID)
    {
        if (dicListGroup.ContainsKey(listGroupID) == false
            || dicListGroup[listGroupID].Count == 0)
            return false;

        uint itemID = dicListGroup[listGroupID].First().PetID;
        return itemID != 0;
    }

    public static bool IsRuneByListGroupID(uint listGroupID)
    {
        if (dicListGroup.ContainsKey(listGroupID) == false
            || dicListGroup[listGroupID].Count == 0)
            return false;

        uint itemID = dicListGroup[listGroupID].First().RuneID;
        return itemID != 0;
    }

	public static uint GetListIDBySearchGoods(uint listGroupID, uint goodsTid)
	{
		if (dicListGroup.ContainsKey(listGroupID) == false)
			return 0;

		foreach (var t in dicListGroup[listGroupID])
		{
			if(HasTargetGoodsID(t, goodsTid))
			{
				return t.ListID;
			}
		}

		return 0;
	}


    public static List<ListGroup_Table> GetListGroup(uint _listGroupID)
    {   // 외부 사용시 원본의 훼손을 방지하기 위한 클론을 전달한다. GC됨을 주의.
        return FindOrAllocListGroup(_listGroupID).ToList();
    }

	public static bool HasTargetGoodsID(uint listGroupTid, uint targetQueryTid)
	{
		var data = GetData(listGroupTid);
		if (data == null)
			return false;
		return data.Exists(t => HasTargetGoodsID(t, targetQueryTid));
	}

	public static bool HasTargetGoodsID(ListGroup_Table data, uint targetQueryTid)
	{
		if (data == null)
			return false;

		return data.ItemID == targetQueryTid
			|| data.ChangeID == targetQueryTid
			|| data.PetID == targetQueryTid
			|| data.RuneID == targetQueryTid;
	}
}