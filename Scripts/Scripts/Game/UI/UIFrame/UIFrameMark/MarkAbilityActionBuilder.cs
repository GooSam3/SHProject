using System.Collections.Generic;
using UnityEngine;

public class MarkAbilityActionBuilder
{
    //public class AbilityActionTitleValuePair
    //{
    //    public E_AbilityType type;
    //    public string title;
    //    public string strValue;
    //    public float value;
    //    public bool isPercentage;

    //    public AbilityActionTitleValuePair() { }
    //    public AbilityActionTitleValuePair(AbilityActionTitleValuePair source)
    //    {
    //        this.type = source.type;
    //        this.title = source.title;
    //        this.strValue = source.strValue;
    //        this.value = source.value;
    //        this.isPercentage = source.isPercentage;
    //    }
    //}

    public uint[] GetAbilityActionIDsStackedList(uint markTid)
    {
        var list = new List<uint>();
        DBMark.AddAbilityActionsStackedAscending(markTid, ref list);
        return list.ToArray();
    }

    public void BuildAbilityActionTexts(
        bool mergeValues
        , ref List<UIAbilityData> outputList
        , params uint[] abilityActionIDs)
    {
        if (outputList == null)
            outputList = new List<UIAbilityData>();

        for (int i = 0; i < abilityActionIDs.Length; i++)
        {
            DBAbilityAction.GetAbilityTypeList(abilityActionIDs[i], ref outputList);
        }

        if (mergeValues)
        {
            outputList = DBAbilityAction.GetMergedTypeList(outputList);
        }
    }

    public void SetCompareTexts(
        List<UIAbilityData> sourceAbility
        , ref List<UIAbilityData> destAbility)
    {
        foreach (var dest in destAbility)
        {
            var searchedFromSource = sourceAbility.Find(t => t.type == dest.type);

            if (searchedFromSource != null)
            {
                dest.compareValue = searchedFromSource.value;
                dest.viewType = E_UIAbilityViewType.AbilityCompare;
            }
            else
            {
                dest.viewType = E_UIAbilityViewType.Ability;
            }
        }
    }

    //public void SetAbilityActionUITexts(
    //    bool sumValue // 능력치 중복안되게 합산 여부 
    //    , int dataOffset // 데이터 시작 오프셋 값 
    //    , MarkSingleAbilityAction sourceObj
    //    , List<UIAbilityData> dataSource
    //    , RectTransform parent
    //    , ref List<UIInsatnce> instanceList)
    //{
    //    if (dataSource.Count == 0)
    //        return;

    //    List<AbilityActionTitleValuePair> data = dataSource;

    //    if (sumValue)
    //    {
    //        data = new List<AbilityActionTitleValuePair>();

    //        for (int i = dataOffset; i < dataSource.Count; i++)
    //        {
    //            var target = data.Find(t => t.type == dataSource[i].type);

    //            if (target == null)
    //            {
    //                data.Add(new AbilityActionTitleValuePair(dataSource[i]));
    //            }
    //            else
    //            {
    //                target.value += dataSource[i].value;
    //            }
    //        }
    //    }

    //    int dataCnt = data.Count - dataOffset;

    //    // 개수가 부족하면 필요한만큼 확보함. 
    //    if (instanceList.Count < dataCnt)
    //    {
    //        instanceList.Capacity = dataCnt;
    //        int addCnt = dataCnt - instanceList.Count;

    //        for (int i = 0; i < addCnt; i++)
    //        {
    //            AddAbilityActionTextInstance(instanceList, sourceObj, parent);
    //        }
    //    }
    //    else if (instanceList.Count > dataCnt)
    //    {
    //        for (int i = 0; i < instanceList.Count - dataCnt; i++)
    //        {
    //            instanceList[i + dataCnt].gameObject.SetActive(false);
    //        }
    //    }

    //    for (int i = 0; i < dataCnt; i++)
    //    {
    //        int dataIndex = dataOffset + i;
    //        instanceList[i].SetText(data[dataIndex].title, data[dataIndex].strValue);
    //        instanceList[i].gameObject.SetActive(true);
    //    }
    //}

    public MarkSingleAbilityAction AddAbilityActionTextInstance(
        List<MarkSingleAbilityAction> targetList
        , MarkSingleAbilityAction sourceObj
        , RectTransform parent)
    {
        var instance = GameObject.Instantiate(sourceObj, parent);
        targetList.Add(instance);
        return instance;
    }
}
