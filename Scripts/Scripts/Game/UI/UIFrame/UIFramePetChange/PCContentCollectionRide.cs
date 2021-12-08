using GameDB;
using System.Collections.Generic;
using ZNet.Data;

public class PCContentCollectionRide : PCContentCollectionBase
{
    protected override int CurCollection => Me.CurCharData.GetCompleteCollectItems(CollectionType.TYPE_RIDE)?.Count ?? 0;

    protected override int MaxPCR => DBPet.GetAllRideData().Count;

    protected override int CurPCR => Me.CurCharData.GetRideDataList().Count;

    protected override List<UIAbilityData> GetCollectionAbilityList()
    {
        List<UIAbilityData> listAbility = new List<UIAbilityData>();

        var list = Me.CurCharData.GetCompleteCollectItems(CollectionType.TYPE_RIDE);

        if (list == null)
            return listAbility;

        foreach (var iter in list)
        {
            if (DBPetCollect.GetRideCollection(iter.CollectTid, out var table) == false)
                continue;

            DBAbilityAction.GetAbilityTypeList(table.AbilityActionID_01, ref listAbility);
            DBAbilityAction.GetAbilityTypeList(table.AbilityActionID_02, ref listAbility);
        }

        return DBAbilityAction.GetMergedTypeList(listAbility); ;
    }

    protected override S_PCRResourceData GetResourceData(C_PetChangeData data)
    {
        if (DBPet.TryGet(data.Tid, out var table) == false)
            return new S_PCRResourceData();

        return new S_PCRResourceData() { FileName = table.ResourceFile, ViewScale = (uint)(table.ViewScale * .8f), ViewPosY = table.ViewScaleLocY, Grade = table.Grade , ViewRotY = DBConfig.PCR_Collection_Rot_Y };
    }

    protected override List<ScrollPCRCollectionData> GetSortedData(E_CollectionSortType type)
    {
        if (type == E_CollectionSortType.All)
            return BaseContentData;

        List<ScrollPCRCollectionData> tempList = new List<ScrollPCRCollectionData>();

        switch (type)
        {
            case E_CollectionSortType.Completed:
                foreach (var iter in BaseContentData)
                {
                    var collectData = Me.CurCharData.GetCollectData(iter.Tid, CollectionType.TYPE_RIDE);

                    if (collectData == null || collectData.curState != CollectState.STATE_COMPLETE)
                        continue;

                    tempList.Add(iter);
                }
                break;
            case E_CollectionSortType.InProgress:
                foreach (var iter in BaseContentData)
                {
                    var collectData = Me.CurCharData.GetCollectData(iter.Tid, CollectionType.TYPE_RIDE);

                    if (collectData == null || collectData.curState != CollectState.STATE_COMPLETE)
                    {
                        tempList.Add(iter);
                        continue;
                    }
                }
                break;
        }
        return tempList;
    }

    protected override List<ScrollPCRCollectionData> GetSortedData(List<E_AbilityType> listType)
    {
        if (listType.Count <= 0)
            return BaseContentData;

        List<ScrollPCRCollectionData> tempList = new List<ScrollPCRCollectionData>();

        foreach (var iter in BaseContentData)
        {
            bool hasValue = true;
            foreach (var ability in listType)
            {
                hasValue = DBPetCollect.ContainsAbility(iter.Tid, ability);

                if (hasValue == false)
                    break;
            }

            if (hasValue)
                tempList.Add(iter);
        }

        return tempList;
    }

    protected override List<ScrollPCRCollectionData> GetSortedData(string str)
    {
        if (str.Length <= 0)
            return BaseContentData;

        List<ScrollPCRCollectionData> tempList = new List<ScrollPCRCollectionData>();

        foreach (var iter in BaseContentData)
        {
            if (DBPetCollect.GetRideCollection(iter.Tid, out var collectTable) == false)
                continue;

            // 컬렉션 이름 검색
            if (DBLocale.GetText(collectTable.PetCollectionTextID).Contains(str))
            {
                tempList.Add(iter);
                continue;
            }

            bool isAdded = false;

            // 능력치 검색
            foreach (var ability in DBPetCollect.GetAllAbility(iter.Tid))
            {
                if (DBAbility.IsParseAbility(ability) == false)
                    continue;

                if (DBLocale.GetText(ability.ToString()).Contains(str))
                {
                    isAdded = true;

                    tempList.Add(iter);

                    break;
                }
            }

            if (isAdded)
                continue;

            // 내부 인자 검색
            foreach (var change in DBPetCollect.GetAllCollectItems(iter.Tid))
            {
                if (DBPet.TryGet(change, out var table) == false)
                    continue;

                if (DBLocale.GetText(table.PetTextID).Contains(str))
                {
                    tempList.Add(iter);
                    break;
                }
            }

        }

        return tempList;
    }

    protected override void InitilaizeList()
    {
        BaseContentData.Clear();

        foreach (var data in DBPetCollect.GetRideCollectionDatas())
        {
            BaseContentData.Add(new ScrollPCRCollectionData(E_PetChangeViewType.Ride, data.PetCollectionID, data.Sort));
        }

        BaseContentData.Sort((x, y) => x.SortOrder.CompareTo(y.SortOrder));
    }

    protected override void SetSelectInfo(C_PetChangeData data)
    {
        if (data == null)
        {
            selectPCRInfo.text = string.Empty;
            selectPCRName.text = string.Empty;
            return;
        }

        Pet_Table tableData = data.petData;

        selectPCRName.text = DBUIResouce.GetPetGradeFormat(DBLocale.GetText(tableData.PetTextID), tableData.Grade);
    }
}
