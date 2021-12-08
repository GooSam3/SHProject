using GameDB;
using System.Collections.Generic;

[UnityEngine.Scripting.Preserve]
public class DBChangeCollect : IGameDBHelper
{
    static Dictionary<E_TapType, List<ChangeCollection_Table>> changecollectionTapDic = new Dictionary<E_TapType, List<ChangeCollection_Table>>();
    static Dictionary<uint, List<uint>> changeTidContainCollectDic = new Dictionary<uint, List<uint>>();

    public void OnReadyData()
    {
        changecollectionTapDic.Clear();
        changeTidContainCollectDic.Clear();

        foreach (ChangeCollection_Table tabledata in GameDBManager.Container.ChangeCollection_Table_data.Values)
        {
            if (!changecollectionTapDic.ContainsKey(tabledata.TapType))
                changecollectionTapDic.Add(tabledata.TapType,new List<ChangeCollection_Table>());
            changecollectionTapDic[tabledata.TapType].Add(tabledata);

            foreach (uint itemId in GetAllCollectItems(tabledata.ChangeCollectionID))
            {
                if (!changeTidContainCollectDic.ContainsKey(itemId))
                    changeTidContainCollectDic.Add(itemId, new List<uint>());

                if (!changeTidContainCollectDic[itemId].Contains(tabledata.ChangeCollectionID))
                    changeTidContainCollectDic[itemId].Add(tabledata.ChangeCollectionID);
            }
        }
    }

    public static Dictionary<E_TapType, List<ChangeCollection_Table>>.KeyCollection GetChangeCollectionTypes()
    {
        return changecollectionTapDic.Keys;
    }

    public static Dictionary<uint, ChangeCollection_Table>.ValueCollection GetChangeCollectionDatas()
    {
        return GameDBManager.Container.ChangeCollection_Table_data.Values;
    }

	public static bool GetCollection(uint _collectTid, out ChangeCollection_Table tableData)
	{
		return GameDBManager.Container.ChangeCollection_Table_data.TryGetValue(_collectTid, out tableData);
    }

    public static string GetCollectionName(uint _collectTid)
    {
        if (GameDBManager.Container.ChangeCollection_Table_data.ContainsKey(_collectTid))
            return GameDBManager.Container.ChangeCollection_Table_data[_collectTid].ChangeCollectionTextID;

       // ZLog.LogError("GetCollectionName - can't Find CollectTid : " + _collectTid);
        return "";
    }

    public static ChangeCollection_Table GetChangeCollection(uint CollectTid)
    {
		if (GameDBManager.Container.ChangeCollection_Table_data.TryGetValue(CollectTid, out var table))
		{
			return table;
		}

        //ZLog.LogError("GetChangeCollection - can't Find CollectTid : " + CollectTid);
        return null;
    }

    public static List<Change_Table> GetCollectChangeList(ChangeCollection_Table _table)
    {
        List<Change_Table> listChange = new List<Change_Table>();
        if (_table == null)
            return listChange;

        AddChangeList(_table.CollectionChangeID_01);
        AddChangeList(_table.CollectionChangeID_02);
        AddChangeList(_table.CollectionChangeID_03);
        AddChangeList(_table.CollectionChangeID_04);
        AddChangeList(_table.CollectionChangeID_05);
        AddChangeList(_table.CollectionChangeID_06);
        AddChangeList(_table.CollectionChangeID_07);
        AddChangeList(_table.CollectionChangeID_08);
        
        void AddChangeList(uint tid)
        {
            if (tid <= 0)
                return;

            if (DBChange.TryGet(tid, out var table) == false)
                return;

            listChange.Add(table);  
        }

        return listChange;
    }

    public static IList<ChangeCollection_Table> GetChangeCollectionsByTap(E_TapType getType)
    {
        if (changecollectionTapDic.ContainsKey(getType))
            return changecollectionTapDic[getType].AsReadOnly();

      //  ZLog.LogError("GetChangeCollectionsByTap - can't Find TapType : " + getType);
        return null;
    }

    public static uint[] GetAllCollectItems(uint CollectTid,uint[] CompleteItemSlot = null)
    {
        List<uint> retValue = new List<uint>();

        if (GameDBManager.Container.ChangeCollection_Table_data.ContainsKey(CollectTid))
        {
            for(int Slot = 0; Slot < GameDBManager.Container.ChangeCollection_Table_data[CollectTid].CollectionChangeCount;Slot++)
            {
                if (CompleteItemSlot != null && CompleteItemSlot[Slot] != 0)
                {
                    retValue.Add(CompleteItemSlot[Slot]);
                    continue;
                }

                switch (Slot)
                {
                    case 0:
                        retValue.Add(GameDBManager.Container.ChangeCollection_Table_data[CollectTid].CollectionChangeID_01);
                        break;
                    case 1:
                        retValue.Add(GameDBManager.Container.ChangeCollection_Table_data[CollectTid].CollectionChangeID_02);
                        break;
                    case 2:
                        retValue.Add(GameDBManager.Container.ChangeCollection_Table_data[CollectTid].CollectionChangeID_03);
                        break;
                    case 3:
                        retValue.Add(GameDBManager.Container.ChangeCollection_Table_data[CollectTid].CollectionChangeID_04);
                        break;
                    case 4:
                        retValue.Add(GameDBManager.Container.ChangeCollection_Table_data[CollectTid].CollectionChangeID_05);
                        break;
                    case 5:
                        retValue.Add(GameDBManager.Container.ChangeCollection_Table_data[CollectTid].CollectionChangeID_06);
                        break;
                    case 6:
                        retValue.Add(GameDBManager.Container.ChangeCollection_Table_data[CollectTid].CollectionChangeID_07);
                        break;
                    case 7:
                        retValue.Add(GameDBManager.Container.ChangeCollection_Table_data[CollectTid].CollectionChangeID_08);
                        break;
                }
            }
        }

        return retValue.ToArray();
    }

    public static E_AbilityType[] GetAllAbility(uint CollectTid)
    {
        List<E_AbilityType> retValue = new List<E_AbilityType>();

        if (GameDBManager.Container.ChangeCollection_Table_data.ContainsKey(CollectTid))
        {
            if(GameDBManager.Container.ChangeCollection_Table_data[CollectTid].AbilityActionID_01 != 0)
                retValue.AddRange(DBAbility.GetAllAbility(GameDBManager.Container.ChangeCollection_Table_data[CollectTid].AbilityActionID_01));
            if (GameDBManager.Container.ChangeCollection_Table_data[CollectTid].AbilityActionID_02 != 0)
                retValue.AddRange(DBAbility.GetAllAbility(GameDBManager.Container.ChangeCollection_Table_data[CollectTid].AbilityActionID_02));
        }

        return retValue.ToArray();
    }


    public static bool ContainsAbility(uint CollectTid, E_AbilityType type)
    {
        List<E_AbilityType> temp = new List<E_AbilityType>();

        if (GameDBManager.Container.ChangeCollection_Table_data.ContainsKey(CollectTid))
        {
            if(GameDBManager.Container.ChangeCollection_Table_data[CollectTid].AbilityActionID_01 != 0)
                temp.AddRange(DBAbility.GetAllAbility(GameDBManager.Container.ChangeCollection_Table_data[CollectTid].AbilityActionID_01));
            if (GameDBManager.Container.ChangeCollection_Table_data[CollectTid].AbilityActionID_02 != 0)
                temp.AddRange(DBAbility.GetAllAbility(GameDBManager.Container.ChangeCollection_Table_data[CollectTid].AbilityActionID_02));

            return temp.Contains(type);
        }

        return false;
    }


    public static uint[] GetAffectCollections(uint ChangeTid)
    {
        if (changeTidContainCollectDic.ContainsKey(ChangeTid))
            return changeTidContainCollectDic[ChangeTid].ToArray();

        return null;
    }

    public static int[] GetCollectionSlot(uint CollectionTid, uint ChangeTid)
    {
        if (GameDBManager.Container.ChangeCollection_Table_data.ContainsKey(CollectionTid))
        {
            List<int> slotList = new List<int>();

            for (int i = 0; i < GameDBManager.Container.ChangeCollection_Table_data[CollectionTid].CollectionChangeCount; i++)
            {
                switch (i)
                {
                    case 0:
                        if (GameDBManager.Container.ChangeCollection_Table_data[CollectionTid].CollectionChangeID_01 == ChangeTid)
                            slotList.Add(i);
                        break;
                    case 1:
                        if (GameDBManager.Container.ChangeCollection_Table_data[CollectionTid].CollectionChangeID_02 == ChangeTid)
                            slotList.Add(i);
                        break;
                    case 2:
                        if (GameDBManager.Container.ChangeCollection_Table_data[CollectionTid].CollectionChangeID_03 == ChangeTid)
                            slotList.Add(i);
                        break;
                    case 3:
                        if (GameDBManager.Container.ChangeCollection_Table_data[CollectionTid].CollectionChangeID_04 == ChangeTid)
                            slotList.Add(i);
                        break;
                    case 4:
                        if (GameDBManager.Container.ChangeCollection_Table_data[CollectionTid].CollectionChangeID_05 == ChangeTid)
                            slotList.Add(i);
                        break;
                    case 5:
                        if (GameDBManager.Container.ChangeCollection_Table_data[CollectionTid].CollectionChangeID_06 == ChangeTid)
                            slotList.Add(i);
                        break;
                    case 6:
                        if (GameDBManager.Container.ChangeCollection_Table_data[CollectionTid].CollectionChangeID_07 == ChangeTid)
                            slotList.Add(i);
                        break;
                    case 7:
                        if (GameDBManager.Container.ChangeCollection_Table_data[CollectionTid].CollectionChangeID_08 == ChangeTid)
                            slotList.Add(i);
                        break;
                }
            }

            return slotList.ToArray();
        }

        return null;
    }
}
