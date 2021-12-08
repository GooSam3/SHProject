using GameDB;
using System.Collections.Generic;

[UnityEngine.Scripting.Preserve]
public class DBPetCollect : IGameDBHelper
{
    static Dictionary<E_TapType, List<PetCollection_Table>> petcollectionTapDic = new Dictionary<E_TapType, List<PetCollection_Table>>();
    static Dictionary<uint, List<uint>> petTidContainCollectDic = new Dictionary<uint, List<uint>>();

    static Dictionary<uint, List<uint>> rideTidContainCollectDic = new Dictionary<uint, List<uint>>();

    static Dictionary<uint, PetCollection_Table> dicPetCollection = new Dictionary<uint, PetCollection_Table>();
    static Dictionary<uint, PetCollection_Table> dicRideCollection = new Dictionary<uint, PetCollection_Table>();

    public void OnReadyData()
    {
        petcollectionTapDic.Clear();
        petTidContainCollectDic.Clear();
        rideTidContainCollectDic.Clear();
        dicPetCollection.Clear();
        dicRideCollection.Clear();

        foreach (PetCollection_Table tabledata in GameDBManager.Container.PetCollection_Table_data.Values)
        {
            if (!petcollectionTapDic.ContainsKey(tabledata.TapType))
                petcollectionTapDic.Add(tabledata.TapType,new List<PetCollection_Table>());
            petcollectionTapDic[tabledata.TapType].Add(tabledata);

            if(tabledata.PetType == E_PetType.Pet)
            {
                if(dicPetCollection.ContainsKey(tabledata.PetCollectionID) == false)
                    dicPetCollection.Add (tabledata.PetCollectionID, tabledata);
            }
            else if (tabledata.PetType == E_PetType.Vehicle)
            {
                if (dicRideCollection.ContainsKey(tabledata.PetCollectionID) == false)
                    dicRideCollection.Add(tabledata.PetCollectionID, tabledata);
            }


            foreach (uint itemId in GetAllCollectItems(tabledata.PetCollectionID))
            {
                if(tabledata.PetType == E_PetType.Pet)
                {
                    if (!petTidContainCollectDic.ContainsKey(itemId))
                        petTidContainCollectDic.Add(itemId, new List<uint>());

                    if (!petTidContainCollectDic[itemId].Contains(tabledata.PetCollectionID))
                        petTidContainCollectDic[itemId].Add(tabledata.PetCollectionID);

                }
                else if (tabledata.PetType == E_PetType.Vehicle)
                {
                    if (!rideTidContainCollectDic.ContainsKey(itemId))
                        rideTidContainCollectDic.Add(itemId, new List<uint>());

                    if (!rideTidContainCollectDic[itemId].Contains(tabledata.PetCollectionID))
                        rideTidContainCollectDic[itemId].Add(tabledata.PetCollectionID);
                }

            }
        }
    }

    public static bool ContainsAbility(uint CollectTid, E_AbilityType type)
    {
        List<E_AbilityType> temp = new List<E_AbilityType>();

        if (GameDBManager.Container.PetCollection_Table_data.ContainsKey(CollectTid))
        {
            if (GameDBManager.Container.PetCollection_Table_data[CollectTid].AbilityActionID_01 != 0)
                temp.AddRange(DBAbility.GetAllAbility(GameDBManager.Container.PetCollection_Table_data[CollectTid].AbilityActionID_01));
            if (GameDBManager.Container.PetCollection_Table_data[CollectTid].AbilityActionID_02 != 0)
                temp.AddRange(DBAbility.GetAllAbility(GameDBManager.Container.PetCollection_Table_data[CollectTid].AbilityActionID_02));

            return temp.Contains(type);
        }

        return false;
    }

    public static Dictionary<E_TapType, List<PetCollection_Table>>.KeyCollection GetPetCollectionTypes()
    {
        return petcollectionTapDic.Keys;
    }

    public static Dictionary<uint, PetCollection_Table>.ValueCollection GetPetCollectionDatas()
    {
        return dicPetCollection.Values;
    }

    public static Dictionary<uint, PetCollection_Table>.ValueCollection GetRideCollectionDatas()
    {
        return dicRideCollection.Values;
    }

    public static bool GetPetRideCollection(uint _collectTid, out PetCollection_Table tableData)
	{
		return GameDBManager.Container.PetCollection_Table_data.TryGetValue(_collectTid, out tableData);
	}

	public static bool GetPetCollection(uint CollectTid,out PetCollection_Table table)
    {
        return dicPetCollection.TryGetValue(CollectTid, out table);
    }

    public static bool GetRideCollection(uint CollectTid, out PetCollection_Table table)
    {
        return dicRideCollection.TryGetValue(CollectTid, out table);
    }

    public static string GetCollectionName(uint _collectTid)
    {
        if (GameDBManager.Container.PetCollection_Table_data.ContainsKey(_collectTid))
            return GameDBManager.Container.PetCollection_Table_data[_collectTid].PetCollectionTextID;

      //  ZLog.LogError("GetCollectionName - can't Find CollectTid : " + _collectTid);
        return "";
    }

    public static IList<PetCollection_Table> GetPetCollectionsByTap(E_TapType getType)
    {
        if (petcollectionTapDic.ContainsKey(getType))
            return petcollectionTapDic[getType].AsReadOnly();

        //ZLog.LogError("GetPetCollectionsByTap - can't Find TapType : " + getType);
        return null;
    }

    public static List<Pet_Table> GetCollectPetRideList(PetCollection_Table _table)
    {
        List<Pet_Table> listPetRide = new List<Pet_Table>();
        if (_table == null)
            return listPetRide;

        AddPetRideList(_table.CollectionPetID_01);
        AddPetRideList(_table.CollectionPetID_02);
        AddPetRideList(_table.CollectionPetID_03);
        AddPetRideList(_table.CollectionPetID_04);
        AddPetRideList(_table.CollectionPetID_05);
        AddPetRideList(_table.CollectionPetID_06);
        AddPetRideList(_table.CollectionPetID_07);
        AddPetRideList(_table.CollectionPetID_08);

        void AddPetRideList(uint tid)
        {
            if (tid <= 0)
                return;
            if (DBPet.TryGet(tid, out var table) == false)
                return;
            listPetRide.Add(table);
        }

        return listPetRide;
    }

    public static uint[] GetAllCollectItems(uint CollectTid,uint[] CompleteItemSlot = null)
    {
        List<uint> retValue = new List<uint>();

        if (GameDBManager.Container.PetCollection_Table_data.ContainsKey(CollectTid))
        {
            for(int Slot = 0; Slot < GameDBManager.Container.PetCollection_Table_data[CollectTid].CollectionPetCount;Slot++)
            {
                if (CompleteItemSlot != null && CompleteItemSlot[Slot] != 0)
                {
                    retValue.Add(CompleteItemSlot[Slot]);
                    continue;
                }

                switch (Slot)
                {
                    case 0:
                        retValue.Add(GameDBManager.Container.PetCollection_Table_data[CollectTid].CollectionPetID_01);
                        break;
                    case 1:
                        retValue.Add(GameDBManager.Container.PetCollection_Table_data[CollectTid].CollectionPetID_02);
                        break;
                    case 2:
                        retValue.Add(GameDBManager.Container.PetCollection_Table_data[CollectTid].CollectionPetID_03);
                        break;
                    case 3:
                        retValue.Add(GameDBManager.Container.PetCollection_Table_data[CollectTid].CollectionPetID_04);
                        break;
                    case 4:
                        retValue.Add(GameDBManager.Container.PetCollection_Table_data[CollectTid].CollectionPetID_05);
                        break;
                    case 5:
                        retValue.Add(GameDBManager.Container.PetCollection_Table_data[CollectTid].CollectionPetID_06);
                        break;
                    case 6:
                        retValue.Add(GameDBManager.Container.PetCollection_Table_data[CollectTid].CollectionPetID_07);
                        break;
                    case 7:
                        retValue.Add(GameDBManager.Container.PetCollection_Table_data[CollectTid].CollectionPetID_08);
                        break;
                }
            }
        }

        return retValue.ToArray();
    }

    public static E_AbilityType[] GetAllAbility(uint CollectTid)
    {
        List<E_AbilityType> retValue = new List<E_AbilityType>();
    
        if (GameDBManager.Container.PetCollection_Table_data.ContainsKey(CollectTid))
        {
            if(GameDBManager.Container.PetCollection_Table_data[CollectTid].AbilityActionID_01 != 0)
                retValue.AddRange(DBAbility.GetAllAbility(GameDBManager.Container.PetCollection_Table_data[CollectTid].AbilityActionID_01));
            if (GameDBManager.Container.PetCollection_Table_data[CollectTid].AbilityActionID_02 != 0)
                retValue.AddRange(DBAbility.GetAllAbility(GameDBManager.Container.PetCollection_Table_data[CollectTid].AbilityActionID_02));
        }
    
        return retValue.ToArray();
    }

    public static uint[] GetAffectPetCollections(uint PetTid)
    {
        if (petTidContainCollectDic.ContainsKey(PetTid))
            return petTidContainCollectDic[PetTid].ToArray();

        return null;
    }

    public static uint[] GetAffectRideCollections(uint RideTid)
    {
        if (rideTidContainCollectDic.ContainsKey(RideTid))
            return rideTidContainCollectDic[RideTid].ToArray();

        return null;
    }

    public static int[] GetCollectionSlot(uint CollectionTid,uint PetTid)
    {
        if (GameDBManager.Container.PetCollection_Table_data.ContainsKey(CollectionTid))
        {
            List<int> slotList = new List<int>();

            for (int i = 0; i < GameDBManager.Container.PetCollection_Table_data[CollectionTid].CollectionPetCount; i++)
            {
                switch(i)
                {
                    case 0:
                        if (GameDBManager.Container.PetCollection_Table_data[CollectionTid].CollectionPetID_01 == PetTid)
                            slotList.Add(i);
                        break;
                    case 1:
                        if (GameDBManager.Container.PetCollection_Table_data[CollectionTid].CollectionPetID_02 == PetTid)
                            slotList.Add(i);
                        break;
                    case 2:
                        if (GameDBManager.Container.PetCollection_Table_data[CollectionTid].CollectionPetID_03 == PetTid)
                            slotList.Add(i);
                        break;
                    case 3:
                        if (GameDBManager.Container.PetCollection_Table_data[CollectionTid].CollectionPetID_04 == PetTid)
                            slotList.Add(i);
                        break;
                    case 4:
                        if (GameDBManager.Container.PetCollection_Table_data[CollectionTid].CollectionPetID_05 == PetTid)
                            slotList.Add(i);
                        break;
                    case 5:
                        if (GameDBManager.Container.PetCollection_Table_data[CollectionTid].CollectionPetID_06 == PetTid)
                            slotList.Add(i);
                        break;
                    case 6:
                        if (GameDBManager.Container.PetCollection_Table_data[CollectionTid].CollectionPetID_07 == PetTid)
                            slotList.Add(i);
                        break;
                    case 7:
                        if (GameDBManager.Container.PetCollection_Table_data[CollectionTid].CollectionPetID_08 == PetTid)
                            slotList.Add(i);
                        break;
                }
            }

            return slotList.ToArray();
        }

        return null;
    }
}
