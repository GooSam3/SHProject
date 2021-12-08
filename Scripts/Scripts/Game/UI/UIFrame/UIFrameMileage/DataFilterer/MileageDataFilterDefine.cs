using System.Collections;
using System.Collections.Generic;
using System;
using GameDB;

public interface IMileageDataEvaluator
{
    List<MileageBaseDataIdentifier> Evaluate(uint shopListGroupID, IEnumerable<RequestEvaluateParam> reqParams);
    int GetCachedSize(uint key);
}

/// <summary>
/// 이름 및 순서 변경 주의
/// String Key 로 참조하는 곳 존재
/// (UIMileageDataEvaluator.cs)
/// 인스펙터에서도 세팅중 
/// </summary>
public enum MileageDataEvaluatorKey
{
    None = 0,

    Item_All = 100,

    Character_All = 200,
    Character_Gladiator = 300,
    Character_Archer = 400,
    Character_Wizard = 500,
    Character_Assassin = 600,

    Character_Elemental_All = 700,
    Character_Elemental_Fire = 800,
    Character_Elemental_Water = 900,
    Character_Elemental_Electric = 1000,
    Character_Elemental_Light = 1100,
    Character_Elemental_Dark = 1200,

    Character_ObtainedOrNotObtained = 1249,
    Character_NotObtained = 1250,

    Pet_All = 1300,
    Pet_PetType_Pet = 1400,
    Pet_PetType_Vehicle = 1500,

    Item_Equipment_Parts_All = 1600,
    Item_Equipment_Parts_Helmet = 1700,
    Item_Equipment_Parts_Armor = 1800,
    Item_Equipment_Parts_Gloves = 1900,
    Item_Equipment_Parts_Pants = 2000,
    Item_Equipment_Parts_Shoes = 2100,
    Item_Equipment_Parts_Cape = 2200,
    Item_Equipment_Parts_Weapon = 2300,
    Item_Equipment_Parts_SideWeapon = 2400,

    Item_Equipment_Character_All = 2500,
    Item_Equipment_Character_Gladiator = 2600,
    Item_Equipment_Character_Archer = 2700,
    Item_Equipment_Character_Wizard = 2800,
    Item_Equipment_Character_Assassin = 2900,
}

/// <summary>
/// TargetTable 관련 
/// </summary>
public enum MileageDataEvaluateTargetDataType
{
    None = 0,
    Item,
    Change,
    Pet,
    Rune
}

public enum MileageAndOrOperation
{
    None = 0,
    And,
    Or
}

public struct RequestEvaluateParam
{
    public MileageDataEvaluatorKey key;
    public MileageAndOrOperation conditionalType;

    public RequestEvaluateParam(MileageDataEvaluatorKey key, MileageAndOrOperation conditionalType)
    {
        this.key = key;
        this.conditionalType = conditionalType;
    }
}

//public struct MileageEvaluateDataRequestParam
//{
//    public List<RequestEvaluateParam> evaluatorList;
//    /// <summary>
//    ///  Item 이면 ItemGroup 으로 캐스팅 후 사용 등 용도임 
//    ///  이 값이 NULL 이면 그냥 전체 테이블로 간주. 
//    /// </summary>
//   // public object sourceDataParam;
//}

public struct MileageBaseDataIdentifier
{
    public uint tid;
    public MileageDataEvaluateTargetDataType dataType;

    public MileageBaseDataIdentifier(uint tid, MileageDataEvaluateTargetDataType dataType)
    {
        this.tid = tid;
        this.dataType = dataType;
    }
}

/// <summary>
///  마일리지 상점에서 출력할 모든 아이템들을 필터링할 수 있는 베이스 클래스
///  아이템 종류별로 이 클래스를 상속받아야함. 
/// </summary>
abstract public class BaseMileageDataEvaluator<EvaluateType>
{
    protected class DataSizeInfo
    {
        public uint key;
        public int size;
    }

    public delegate uint RetrieveTID(int index /*, List<EvaluateType> sourceDataList */);

    protected Dictionary<int, Predicate<EvaluateType>> Evaluators;
    protected MileageDataEvaluateTargetDataType DataType;

    protected List<RequestEvaluateParam> CurrentReqParamsCached;
    protected List<DataSizeInfo> SizeInfoCached;

    virtual public void Initialize(MileageDataEvaluateTargetDataType dataType)
    {
        Evaluators = new Dictionary<int, Predicate<EvaluateType>>();
        CurrentReqParamsCached = new List<RequestEvaluateParam>();
        SizeInfoCached = new List<DataSizeInfo>();
        this.DataType = dataType;
    }

    public int GetSizeIfCached(uint key)
    {
        var target = SizeInfoCached.Find(t => t.key == key);
        return target != null ? target.size : -1;
    }

    protected void CacheSize(uint key, int size)
    {
        var target = SizeInfoCached.Find(t => t.key == key);

        if (target == null)
        {
            target = new DataSizeInfo();
            SizeInfoCached.Add(target);
        }

        target.key = key;
        target.size = size;
    }

    protected bool DeleteSizeCache(uint key)
    {
        int index = SizeInfoCached.FindIndex(t => t.key == key);

        if (index == -1)
        {
            return false;
        }

        SizeInfoCached.RemoveAt(index);
        return true;
    }

    protected List<MileageBaseDataIdentifier> Evaluate(
        RetrieveTID tidRetriever
        , List<EvaluateType> sourceDataList
        , IEnumerable<RequestEvaluateParam> reqParams
        , Func<uint> sizeCacheKeyGetter = null)
    {
        List<MileageBaseDataIdentifier> result = new List<MileageBaseDataIdentifier>();
        SelectMyParams(reqParams);

        for (int i = 0; i < sourceDataList.Count; i++)
        {
            if (EvaluateData(sourceDataList[i]))
            {
                result.Add(AssignBaseDataIdentifier(tidRetriever(i /*, sourceDataList*/ )));
            }
        }

        if (sizeCacheKeyGetter != null)
        {
            CacheSize(sizeCacheKeyGetter(), result.Count);
        }

        return result;
    }

    protected virtual bool EvaluateData(EvaluateType data)
    {
        foreach (var param in CurrentReqParamsCached)
        {
            bool result = Evaluators[(int)param.key](data);

            /// And 조건인데 Evaluate 에서 조건에 맞지않으면 바로 return false 
            if (param.conditionalType == MileageAndOrOperation.And && result == false)
            {
                return false;
            }
            /// Or 조건인데 Evaluate 에서 조건에 맞으면 바로 return true 
            else if (param.conditionalType == MileageAndOrOperation.Or && result)
            {
                return true;
            }
        }

        return true;
    }

    protected MileageBaseDataIdentifier AssignBaseDataIdentifier(uint tid)
    {
        return new MileageBaseDataIdentifier(tid, DataType);
    }

    public void AddEvaluator(int key, Predicate<EvaluateType> predicate)
    {
        Evaluators.Add(key, predicate);
    }

    protected bool IsEvaluatorKeyExist(int key)
    {
        return Evaluators.ContainsKey(key);
    }

    protected List<RequestEvaluateParam> SelectMyParams(IEnumerable<RequestEvaluateParam> reqParam)
    {
        CurrentReqParamsCached.Clear();

        foreach (var t in reqParam)
        {
            if (IsEvaluatorKeyExist((int)t.key))
            {
                CurrentReqParamsCached.Add(t);
            }
            else
            {
                ZLog.LogError(ZLogChannel.UI, "Requested Data Filtering EvaluatorKey Predicate Not Added : " + t.key.ToString());
            }
        }

        return CurrentReqParamsCached;
    }
}

public class MileageDataEvaluator_Item : BaseMileageDataEvaluator<Item_Table>, IMileageDataEvaluator
{
    public List<MileageBaseDataIdentifier> Evaluate(uint shopListGroupID, IEnumerable<RequestEvaluateParam> reqParams)
    {
        //     var itemsByGroup = DBShopList.GetDataListByGroupID(shopListGroupID);
        //     var itemList = new List<Item_Table>();

        //     for (int i = 0; i < itemsByGroup.Count; i++)
        //     {
        //itemList.Add(DBItem.GetItem(itemsByGroup[i].GoodsItemID));
        //     }
        
        var shopDataList = DBShopList.GetDataListByGroupID(shopListGroupID);
        var itemList = new List<Item_Table>();

        foreach (var shopData in shopDataList)
        {
            DBListGroup.ForeachListGroupID(shopData.ListGroupID, (tableData) =>
            {
                if (tableData.ItemID!= 0)
                {
                    itemList.Add(DBItem.GetItem(tableData.ItemID));
                }
            });
        }

        return base.Evaluate(
            tidRetriever: (index) =>
            {
                return itemList[index].ItemID;
            }
            , itemList
            , reqParams
            , sizeCacheKeyGetter: () =>
            {
                return shopListGroupID;
            });
    }

    public int GetCachedSize(uint key)
    {
        return GetSizeIfCached(key);
    }
}

public class MileageDataEvaluator_Change : BaseMileageDataEvaluator<Change_Table>, IMileageDataEvaluator
{
    public List<MileageBaseDataIdentifier> Evaluate(uint shopListGroupID, IEnumerable<RequestEvaluateParam> reqParams)
    {
        //     var changesByGroup = DBShopList.GetDataListByGroupID(shopListGroupID);
        //     var changeList = new List<Change_Table>();

        //     for (int i = 0; i < changesByGroup.Count; i++)
        //     {
        //changeList.Add(DBChange.Get(changesByGroup[i].GoodsChangeID));
        //     }
        var shopDataList = DBShopList.GetDataListByGroupID(shopListGroupID);
        var changeList = new List<Change_Table>();

        foreach (var shopData in shopDataList)
        {
            DBListGroup.ForeachListGroupID(shopData.ListGroupID, (tableData) =>
			{
				if (tableData.ChangeID != 0)
				{
                    changeList.Add(DBChange.Get(tableData.ChangeID));
				}
			});
        }

        return base.Evaluate(
            tidRetriever: (index) =>
            {
                return changeList[index].ChangeID;
            }
            , changeList
            , reqParams
            , sizeCacheKeyGetter: () =>
            {
                return shopListGroupID;
            });
    }

    public int GetCachedSize(uint key)
    {
        return GetSizeIfCached(key);
    }
}

public class MileageDataEvaluator_Pet : BaseMileageDataEvaluator<Pet_Table>, IMileageDataEvaluator
{
    public List<MileageBaseDataIdentifier> Evaluate(uint shopListGroupID, IEnumerable<RequestEvaluateParam> reqParams)
    {
        //     var petsByGroup = DBShopList.GetDataListByGroupID(shopListGroupID);
        //     var petList = new List<Pet_Table>();

        //     for (int i = 0; i < petsByGroup.Count; i++)
        //     {
        //petList.Add(DBPet.GetPetData(petsByGroup[i].GoodsPetID));
        //     }

        var shopDataList = DBShopList.GetDataListByGroupID(shopListGroupID);
        var petList = new List<Pet_Table>();

		foreach (var shopData in shopDataList)
		{
			DBListGroup.ForeachListGroupID(shopData.ListGroupID, (tableData) =>
			{
				if (tableData.PetID != 0)
				{
					petList.Add(DBPet.GetPetData(tableData.PetID));
				}
			});
		}

		return base.Evaluate(
            tidRetriever: (index) =>
            {
                return petList[index].PetID;
            }
            , petList
            , reqParams
            , sizeCacheKeyGetter: () =>
            {
                return shopListGroupID;
            });
    }

    public int GetCachedSize(uint key)
    {
        return GetSizeIfCached(key);
    }
}



//-------------------------------------------------//
