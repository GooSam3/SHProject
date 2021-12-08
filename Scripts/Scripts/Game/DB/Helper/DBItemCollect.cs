using GameDB;
using System.Collections.Generic;

[UnityEngine.Scripting.Preserve]
public class DBItemCollect : IGameDBHelper
{
    static List<ItemCollection_Table> itemcollectionList = new List<ItemCollection_Table>();

    static Dictionary<uint, List<System.ValueTuple<uint, byte>>> MaterialIdByItemCollection = new Dictionary<uint, List<(uint, byte)>>();
    static Dictionary<uint, List<System.ValueTuple<uint, byte>>> RawMaterialIdByItemCollection = new Dictionary<uint, List<(uint, byte)>>();

    public void OnReadyData()
    {
        itemcollectionList.Clear();
        //MaterialIdByItemCollection.Clear();
        //RawMaterialIdByItemCollection.Clear();

        foreach (ItemCollection_Table tabledata in GameDBManager.Container.ItemCollection_Table_data.Values)
        {            
            itemcollectionList.Add(tabledata);

        //    AddCollectItemMaterialInfo(tabledata.ItemCollectionID,0, tabledata.CollectionItemID_01);
        //    AddCollectItemMaterialInfo(tabledata.ItemCollectionID,1, tabledata.CollectionItemID_02);
        //    AddCollectItemMaterialInfo(tabledata.ItemCollectionID,2, tabledata.CollectionItemID_03);
        //    AddCollectItemMaterialInfo(tabledata.ItemCollectionID,3, tabledata.CollectionItemID_04);
        //    AddCollectItemMaterialInfo(tabledata.ItemCollectionID,4, tabledata.CollectionItemID_05);
        //    AddCollectItemMaterialInfo(tabledata.ItemCollectionID,5, tabledata.CollectionItemID_06);
        //    AddCollectItemMaterialInfo(tabledata.ItemCollectionID,6, tabledata.CollectionItemID_07);
        //    AddCollectItemMaterialInfo(tabledata.ItemCollectionID,7, tabledata.CollectionItemID_08);
        }
    }

    //   static void AddCollectItemMaterialInfo(uint ItemCollectioTid,byte slot,List<uint> Materials)
    //   {
    //       foreach (var itemTid in Materials)
    //       {
    //           if (itemTid == 0)
    //               continue;

    //           if (!MaterialIdByItemCollection.ContainsKey(itemTid))
    //               MaterialIdByItemCollection.Add(itemTid,new List<(uint, byte)>());
    //           MaterialIdByItemCollection[itemTid].Add((ItemCollectioTid, slot));

    //           var enchantStep = DBItem.GetEnchantStep(itemTid);

    //           var groupIdx = DBItem.GetGroupId(itemTid);

    //           if (groupIdx == 0)
    //               continue;

    //           foreach (var data in DBItem.GetGroupList(groupIdx))
    //           {
    //               if (data.Step < enchantStep)
    //               {
    //                   if (!RawMaterialIdByItemCollection.ContainsKey(data.ItemID))
    //                       RawMaterialIdByItemCollection.Add(data.ItemID, new List<(uint, byte)>());
    //                   RawMaterialIdByItemCollection[data.ItemID].Add((ItemCollectioTid, slot));
    //               }
    //           }
    //       }
    //   }

    //   public static Dictionary<uint,ItemCollection_Table>.ValueCollection GetItemCollectionDatas()
    //   {
    //       return GameDBManager.Container.ItemCollection_Table_data.Values;
    //   }

    public static bool GetCollection(uint _collectTid, out ItemCollection_Table tableData)
    {
    	return GameDBManager.Container.ItemCollection_Table_data.TryGetValue(_collectTid, out tableData);
    }

    public static ItemCollection_Table GetItemCollection(uint CollectTid)
    {
        if (GameDBManager.Container.ItemCollection_Table_data.TryGetValue(CollectTid, out var table))
        {
            return table;
        }

        //ZLog.LogError("GetItemCollection - can't Find CollectTid : " + CollectTid);
        return null;
    }
    public static bool GetItemCollection(uint CollectTid, out ItemCollection_Table table)
    {
        return GameDBManager.Container.ItemCollection_Table_data.TryGetValue(CollectTid, out table);
    }



       public static IList<ItemCollection_Table> GetItemCollections()
       {
           return itemcollectionList.AsReadOnly(); 
       }

    //   public static IList<uint> GetNeedCollectItems(uint CollectTid,IList<uint> CompleteSlot = null)
    //   {
    //       List<uint> retValue = new List<uint>();

    //       if (GameDBManager.Container.ItemCollection_Table_data.ContainsKey(CollectTid))
    //       {
    //           for (int Slot = 0; Slot < GameDBManager.Container.ItemCollection_Table_data[CollectTid].CollectionItemCount; Slot++)
    //           {
    //               if (CompleteSlot != null && CompleteSlot[Slot] != 0)
    //               {
    //                   continue;
    //               }

    //               switch (Slot)
    //               {
    //                   case 0:
    //                       retValue.AddRange(GameDBManager.Container.ItemCollection_Table_data[CollectTid].CollectionItemID_01.ToArray());
    //                       break;
    //                   case 1:
    //                       retValue.AddRange(GameDBManager.Container.ItemCollection_Table_data[CollectTid].CollectionItemID_02.ToArray());
    //                       break;
    //                   case 2:
    //                       retValue.AddRange(GameDBManager.Container.ItemCollection_Table_data[CollectTid].CollectionItemID_03.ToArray());
    //                       break;
    //                   case 3:
    //                       retValue.AddRange(GameDBManager.Container.ItemCollection_Table_data[CollectTid].CollectionItemID_04.ToArray());
    //                       break;
    //                   case 4:
    //                       retValue.AddRange(GameDBManager.Container.ItemCollection_Table_data[CollectTid].CollectionItemID_05.ToArray());
    //                       break;
    //                   case 5:
    //                       retValue.AddRange(GameDBManager.Container.ItemCollection_Table_data[CollectTid].CollectionItemID_06.ToArray());
    //                       break;
    //                   case 6:
    //                       retValue.AddRange(GameDBManager.Container.ItemCollection_Table_data[CollectTid].CollectionItemID_07.ToArray());
    //                       break;
    //                   case 7:
    //                       retValue.AddRange(GameDBManager.Container.ItemCollection_Table_data[CollectTid].CollectionItemID_08.ToArray());
    //                       break;
    //               }
    //           }
    //       }

    //       return retValue.AsReadOnly();
    //   }

    public static uint[] GetAllCollectItems(uint CollectTid,uint[] CompleteItemSlot = null)
    {
        List<uint> retValue = new List<uint>();

        if (GameDBManager.Container.ItemCollection_Table_data.ContainsKey(CollectTid))
        {
            for(int Slot = 0; Slot < GameDBManager.Container.ItemCollection_Table_data[CollectTid].CollectionItemCount;Slot++)
            {
                if (CompleteItemSlot != null && CompleteItemSlot[Slot] != 0)
                {
                    retValue.Add(CompleteItemSlot[Slot]);
                    continue;
                }

                switch (Slot)
                {
                    case 0:
                        retValue.Add(GameDBManager.Container.ItemCollection_Table_data[CollectTid].CollectionItemID_01);
                        break;
                    case 1:
                        retValue.Add(GameDBManager.Container.ItemCollection_Table_data[CollectTid].CollectionItemID_02);
                        break;
                    case 2:
                        retValue.Add(GameDBManager.Container.ItemCollection_Table_data[CollectTid].CollectionItemID_03);
                        break;
                    case 3:
                        retValue.Add(GameDBManager.Container.ItemCollection_Table_data[CollectTid].CollectionItemID_04);
                        break;
                    case 4:
                        retValue.Add(GameDBManager.Container.ItemCollection_Table_data[CollectTid].CollectionItemID_05);
                        break;
                    case 5:
                        retValue.Add(GameDBManager.Container.ItemCollection_Table_data[CollectTid].CollectionItemID_06);
                        break;
                    case 6:
                        retValue.Add(GameDBManager.Container.ItemCollection_Table_data[CollectTid].CollectionItemID_07);
                        break;
                    case 7:
                        retValue.Add(GameDBManager.Container.ItemCollection_Table_data[CollectTid].CollectionItemID_08);
                        break;
                }
            }
        }

        return retValue.ToArray();
    }

    public static E_AbilityType[] GetAllAbility(uint CollectTid)
    {
        List<E_AbilityType> retValue = new List<E_AbilityType>();

        if (GameDBManager.Container.ItemCollection_Table_data.ContainsKey(CollectTid))
        {
            if(GameDBManager.Container.ItemCollection_Table_data[CollectTid].AbilityActionID_01 != 0)
                retValue.AddRange(DBAbility.GetAllAbility(GameDBManager.Container.ItemCollection_Table_data[CollectTid].AbilityActionID_01));
            if (GameDBManager.Container.ItemCollection_Table_data[CollectTid].AbilityActionID_02 != 0)
                retValue.AddRange(DBAbility.GetAllAbility(GameDBManager.Container.ItemCollection_Table_data[CollectTid].AbilityActionID_02));
        }

        return retValue.ToArray();
    }

    public static bool ContainsAbility(uint CollectTid, E_AbilityType type)
    {
        List<E_AbilityType> temp = new List<E_AbilityType>();

        if (GameDBManager.Container.ItemCollection_Table_data.ContainsKey(CollectTid))
        {
            if (GameDBManager.Container.ItemCollection_Table_data[CollectTid].AbilityActionID_01 != 0)
                temp.AddRange(DBAbility.GetAllAbility(GameDBManager.Container.ItemCollection_Table_data[CollectTid].AbilityActionID_01));
            if (GameDBManager.Container.ItemCollection_Table_data[CollectTid].AbilityActionID_02 != 0)
                temp.AddRange(DBAbility.GetAllAbility(GameDBManager.Container.ItemCollection_Table_data[CollectTid].AbilityActionID_02));

            return temp.Contains(type);
        }

        return false;
    }

    //   public static bool IsReplaceItem(uint CollectTid,int Slot)
    //   {
    //       if (GameDBManager.Container.ItemCollection_Table_data.ContainsKey(CollectTid))
    //       {
    //           if (GameDBManager.Container.ItemCollection_Table_data[CollectTid].CollectionItemCount > Slot)
    //           {
    //               switch (Slot)
    //               {
    //                   case 0:
    //                       return GameDBManager.Container.ItemCollection_Table_data[CollectTid].CollectionItemID_01.Count > 1;
    //                   case 1:
    //                       return GameDBManager.Container.ItemCollection_Table_data[CollectTid].CollectionItemID_02.Count > 1;
    //                   case 2:
    //                       return GameDBManager.Container.ItemCollection_Table_data[CollectTid].CollectionItemID_03.Count > 1;
    //                   case 3:
    //                       return GameDBManager.Container.ItemCollection_Table_data[CollectTid].CollectionItemID_04.Count > 1;
    //                   case 4:
    //                       return GameDBManager.Container.ItemCollection_Table_data[CollectTid].CollectionItemID_05.Count > 1;
    //                   case 5:
    //                       return GameDBManager.Container.ItemCollection_Table_data[CollectTid].CollectionItemID_06.Count > 1;
    //                   case 6:
    //                       return GameDBManager.Container.ItemCollection_Table_data[CollectTid].CollectionItemID_07.Count > 1;
    //                   case 7:
    //                       return GameDBManager.Container.ItemCollection_Table_data[CollectTid].CollectionItemID_08.Count > 1;
    //               }
    //           }
    //       }

    //       return false;
    //   }

    //   public static uint[] GetReplaceItems(uint CollectTid, int Slot)
    //   {
    //       List<uint> retValue = new List<uint>();
    //       if (GameDBManager.Container.ItemCollection_Table_data.ContainsKey(CollectTid))
    //       {
    //           if (GameDBManager.Container.ItemCollection_Table_data[CollectTid].CollectionItemCount > Slot)
    //           {
    //               switch (Slot)
    //               {
    //                   case 0:
    //                       retValue.AddRange(GameDBManager.Container.ItemCollection_Table_data[CollectTid].CollectionItemID_01.ToArray());
    //                       break;
    //                   case 1:
    //                       retValue.AddRange(GameDBManager.Container.ItemCollection_Table_data[CollectTid].CollectionItemID_02.ToArray());
    //                       break;
    //                   case 2:
    //                       retValue.AddRange(GameDBManager.Container.ItemCollection_Table_data[CollectTid].CollectionItemID_03.ToArray());
    //                       break;
    //                   case 3:
    //                       retValue.AddRange(GameDBManager.Container.ItemCollection_Table_data[CollectTid].CollectionItemID_04.ToArray());
    //                       break;
    //                   case 4:
    //                       retValue.AddRange(GameDBManager.Container.ItemCollection_Table_data[CollectTid].CollectionItemID_05.ToArray());
    //                       break;
    //                   case 5:
    //                       retValue.AddRange(GameDBManager.Container.ItemCollection_Table_data[CollectTid].CollectionItemID_06.ToArray());
    //                       break;
    //                   case 6:
    //                       retValue.AddRange(GameDBManager.Container.ItemCollection_Table_data[CollectTid].CollectionItemID_07.ToArray());
    //                       break;
    //                   case 7:
    //                       retValue.AddRange(GameDBManager.Container.ItemCollection_Table_data[CollectTid].CollectionItemID_08.ToArray());
    //                       break;
    //               }
    //           }
    //       }

    //       return retValue.ToArray();
    //   }

    public static List<System.ValueTuple<uint,byte>> GetItemCollectionIdsByMat(uint MatTid)
    {
        if (MaterialIdByItemCollection.ContainsKey(MatTid))
            return MaterialIdByItemCollection[MatTid];

        return null;
    }

    //public static List<System.ValueTuple<uint, byte>> GetItemCollectionIdsByRawMat(uint RawMatTid)
    //{
    //    if (RawMaterialIdByItemCollection.ContainsKey(RawMatTid))
    //        return RawMaterialIdByItemCollection[RawMatTid];

    //    return null;
    //}

    //public static bool IsInCollection(ItemCollection_Table table, uint itemTid)
    //{
    //    return table.CollectionItemID_01.Contains(itemTid)
    //        || table.CollectionItemID_02.Contains(itemTid)
    //        || table.CollectionItemID_03.Contains(itemTid)
    //        || table.CollectionItemID_04.Contains(itemTid)
    //        || table.CollectionItemID_05.Contains(itemTid)
    //        || table.CollectionItemID_06.Contains(itemTid)
    //        || table.CollectionItemID_07.Contains(itemTid)
    //        || table.CollectionItemID_08.Contains(itemTid);
    //}
}
