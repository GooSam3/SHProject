using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using Devcat;
using GameDB;
using static MileageDataWrapper;

/// <summary>
/// 마일리지 출력 아이템 데이터 필터링 Wrapper 클래스 
/// 
/// TODO : 
/// -1 조금더 하위 클래스에게 Util 적인 권한을 줄수있는 클래스들을 만들면 어떨라나 ? 
/// 예로 현재 존재하는 Size 를 Clear 나 임의로 수정한다거나 등 .. 
/// 
/// -2 생각해보자 ...
/// 
/// </summary>
public class MileageDataWrapper
{
    private MileageDataEvaluator_Item Evaluator_Item = new MileageDataEvaluator_Item();
    private MileageDataEvaluator_Change Evaluator_Change = new MileageDataEvaluator_Change();
    private MileageDataEvaluator_Pet Evaluator_Pet = new MileageDataEvaluator_Pet();

    private IMileageDataEvaluator DataEvaluator;

    ///-------------------------//
    public void Initialize()
    {
        Evaluator_Item.Initialize(MileageDataEvaluateTargetDataType.Item);
        Evaluator_Change.Initialize(MileageDataEvaluateTargetDataType.Change);
        Evaluator_Pet.Initialize(MileageDataEvaluateTargetDataType.Pet);
    }

    public void AddEvaluatorPredicate_Item(MileageDataEvaluatorKey key, Predicate<Item_Table> predicate)
    {
        Evaluator_Item.AddEvaluator((int)key, predicate);
    }

    public void AddEvaluatorPredicate_Change(MileageDataEvaluatorKey key, Predicate<Change_Table> predicate)
    {
        Evaluator_Change.AddEvaluator((int)key, predicate);
    }

    public void AddEvaluatorPredicate_Pet(MileageDataEvaluatorKey key, Predicate<Pet_Table> predicate)
    {
        Evaluator_Pet.AddEvaluator((int)key, predicate);
    }

    public List<MileageBaseDataIdentifier> GetData(uint shopListGroupID, MileageDataEvaluateTargetDataType dataType, IEnumerable<RequestEvaluateParam> reqParams)
    {
        var t = GetEvaluator(dataType);
        DataEvaluator = t;

        if (DataEvaluator == null)
            return new List<MileageBaseDataIdentifier>();

        return DataEvaluator.Evaluate(shopListGroupID, reqParams);
    }

    public int GetSizeIfCached(MileageDataEvaluateTargetDataType dataType, uint key)
    {
        return GetEvaluator(dataType).GetCachedSize(key);
    }

    private IMileageDataEvaluator GetEvaluator(MileageDataEvaluateTargetDataType dataType)
    {
        IMileageDataEvaluator target = null;

        switch (dataType)
        {
            case MileageDataEvaluateTargetDataType.Item:
                target = Evaluator_Item;
                break;
            case MileageDataEvaluateTargetDataType.Change:
                target = Evaluator_Change;
                break;
            case MileageDataEvaluateTargetDataType.Pet:
                target = Evaluator_Pet;
                break;
            default:
                ZLog.LogError(ZLogChannel.UI, "Please add evaluator Type");
                break;
        }

        return target;
    }
}
