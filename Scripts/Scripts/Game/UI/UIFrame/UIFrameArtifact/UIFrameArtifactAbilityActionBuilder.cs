using GameDB;
using System.Collections.Generic;
using UnityEngine;

public class UIFrameArtifactAbilityActionBuilder
{
    public class AbilityActionTitleValuePair
    {
        public E_AbilityType type;
        public string title;
        public string strValue;
        public float value;
        public bool isPercentage;

        public AbilityActionTitleValuePair() { }
        public AbilityActionTitleValuePair(AbilityActionTitleValuePair source)
        {
            this.type = source.type;
            this.title = source.title;
            this.strValue = source.strValue;
            this.value = source.value;
            this.isPercentage = source.isPercentage;
        }
    }

    /// <summary>
    /// AbilityAction ID 를 리스트로 받아서 
    /// 해당 outputList 에 타이틀/값 문자열을 추가시켜줌.
    /// 당장 list 로 받을 필요는 없지만 추후에 기획 변경을 대비함 . 
    /// </summary>
    public void BuildAbilityActionTexts(
        ref List<AbilityActionTitleValuePair> outputList
        , List<uint> abilityActionIDs)
    {
        List<UIAbilityData> totalAbilityData = new List<UIAbilityData>();

        for (int i = 0; i < abilityActionIDs.Count; i++)
        {
            var tableData = DBAbilityAction.Get(abilityActionIDs[i]);

            if (tableData != null)
            {
                DBAbilityAction.GetAbilityTypeList(tableData, ref totalAbilityData);
            }
        }

        if (outputList.Count != totalAbilityData.Count)
        {
            if (outputList.Count < totalAbilityData.Count)
            {
                int addCnt = totalAbilityData.Count - outputList.Count;
                outputList.Capacity = totalAbilityData.Count;

                for (int i = 0; i < addCnt; i++)
                {
                    outputList.Add(new AbilityActionTitleValuePair());
                }
            }
            else
            {
                outputList.RemoveRange(totalAbilityData.Count, outputList.Count - totalAbilityData.Count);
            }
        }

        for (int i = 0; i < totalAbilityData.Count; i++)
        {
            var t = totalAbilityData[i];
            var abilityData = DBAbility.GetAbility(t.type);
            bool isPercentage = false;

            if (abilityData != null)
            {
                isPercentage = abilityData.MarkType == E_MarkType.Per;
            }

            outputList[i].type = t.type;
            outputList[i].title = DBLocale.GetText(t.type.ToString());
            outputList[i].value = t.value;
            outputList[i].strValue = string.Format("+{0}{1}", t.value, isPercentage ? "%" : string.Empty);
            outputList[i].isPercentage = isPercentage;
        }
    }

    /// <summary>
    /// AbilityAction 데이터를 받아서 해당 Instance 리스트에 생성해서 세팅해줌. 
    /// 안쓰는 부분은 알아서 꺼짐 . (OSA 안쓰면 이거 쓰면 됨)
    /// </summary>
    public void SetAbilityActionUITexts(
        bool sumValue // 능력치 중복안되게 합산 여부 
        , int dataOffset // 데이터 시작 오프셋 값 
        , UIFrameArtifactSingleAbilityAction sourceObj
        , List<AbilityActionTitleValuePair> dataSource
        , RectTransform parent
        , ref List<UIFrameArtifactSingleAbilityAction> instanceList)
    {
        if (dataSource.Count == 0)
            return;

        List<AbilityActionTitleValuePair> data = dataSource;

        if (sumValue)
        {
            data = new List<AbilityActionTitleValuePair>();

            for (int i = dataOffset; i < dataSource.Count; i++)
            {
                var target = data.Find(t => t.type == dataSource[i].type);

                if (target == null)
                {
                    data.Add(new AbilityActionTitleValuePair(dataSource[i]));
                }
                else
                {
                    target.value += dataSource[i].value;
                }
            }
        }

        int dataCnt = data.Count - dataOffset;

        // 개수가 부족하면 필요한만큼 확보함. 
        if (instanceList.Count < dataCnt)
        {
            instanceList.Capacity = dataCnt;
            int addCnt = dataCnt - instanceList.Count;

            for (int i = 0; i < addCnt; i++)
            {
                AddAbilityActionTextInstance(instanceList, sourceObj, parent);
            }
        }
        // 안쓰게될 부분 active false 처리 
        else if (instanceList.Count > dataCnt)
        {
            for (int i = 0; i < instanceList.Count - dataCnt; i++)
            {
                instanceList[i + dataCnt].gameObject.SetActive(false);
            }
        }

        for (int i = 0; i < dataCnt; i++)
        {
            int dataIndex = dataOffset + i;
            instanceList[i].SetText(data[dataIndex].title, data[dataIndex].strValue);
            instanceList[i].gameObject.SetActive(true);
        }
    }

    public UIFrameArtifactSingleAbilityAction AddAbilityActionTextInstance(
        List<UIFrameArtifactSingleAbilityAction> targetList
        , UIFrameArtifactSingleAbilityAction sourceObj
        , RectTransform parent)
    {
        var instance = GameObject.Instantiate(sourceObj, parent);
        targetList.Add(instance);
        return instance;
    }
}
