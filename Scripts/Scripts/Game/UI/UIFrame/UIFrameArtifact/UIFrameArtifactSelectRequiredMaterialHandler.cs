using GameDB;
using System;
using System.Collections.Generic;
using static DBArtifact;

/// <summary>
/// 아티팩트 승급을 위해 필요한 재료 Scroll Item 관리 용이하게 하기위해 만든 클래스 
/// Scroll Grid OSA 쓰는애 한해서 . 
/// </summary>
public class UIFrameArtifactSelectRequiredMaterialHandler : UIFrameArtifactMaterialManagementBase
{
    Action<int, MaterialDataOnSlot> onSlotClicked;

    List<MaterialType> requiredMatList;

    public void Set(List<MaterialDataOnSlot> matsRegistered, List<MaterialType> requiredList)
    {
        CopyData(matsRegistered, Data);
        requiredMatList = requiredList;
        RefreshScroll();
    }

    ///// <summary>
    ///// 재료 선택시 이 함수로 필요 재료칸에 재료를 더함
    ///// </summary>
    //public bool TryAddData(MaterialDataOnSlot data)
    //{
    //    int availableIndex = 0;

    //    if (IsAvailableSlotExist(ref availableIndex) == false)
    //    {
    //        return false;
    //    }

    //    InsertData(data, availableIndex);
    //    return true;
    //}

    public void AddListener_OnClick(Action<int, MaterialDataOnSlot> callback)
    {
        onSlotClicked += callback;
    }

    protected override void OnSlotClicked(int index, MaterialDataOnSlot data)
    {
        onSlotClicked?.Invoke(index, data);
    }

    //public void RegisterMaterial(MaterialDataOnSlot data)
    //{
    //    MaterialDataOnSlot newData = new MaterialDataOnSlot();
    //    newData.CopyFrom(data);
    //    newData.SetSingleCount();

    //    for (int i = 0; i < Data.Count; i++)
    //    {
    //        if (Data[i] == null)
    //        {
    //            Data[i] = newData;
    //            return;
    //        }
    //    }

    //    Data.Add(newData);
    //}

    ///// <summary>
    ///// dest 리스트는 카운팅되면서 
    ///// 내거가 0 이 되면 사라지게끔 파라미터 세팅 
    ///// </summary>
    //public void MoveDataIndex(int index, List<MaterialDataOnSlot> dest)
    //{
    //    if (Data[index] == null)
    //        return;

    //    var target = Data[index];
    //    dest.Find(t => t.matType == target.matType && t.matGrade == target.matGrade).cntByContext++;
    //    Data[index] = null;
    //}

    public void MoveSpecificData_DestCounting(int itemIndex, List<MaterialDataOnSlot> dest)
    {
        if (Data[itemIndex].cntByContext == 0)
            return;

        var sourceTarget = Data[itemIndex];
        var target = dest.Find(t => t.tid == sourceTarget.tid);

        if (target == null)
            return;

        Data[itemIndex].cntByContext--;
        target.cntByContext++;

        if (Data[itemIndex].cntByContext == 0)
        {
            Data[itemIndex].tid = 0;
        }
    }

    public bool CanRegister(MaterialDataOnSlot data)
    {
        if (IsEmptySlotExist() == false)
            return false;

        if (requiredMatList == null)
            return false;

        // 현재 등록된게 요구되는 숫자보다 낮다면 등록가능함. 
        return GetCurRegisteredCnt(data.matType, data.matGrade) <
            GetCurRequiredCnt(requiredMatList, data.matType, data.matGrade);
    }

    public int GetCurRegisteredCnt(E_ArtifactMaterialType type, byte grade)
    {
        int cnt = 0;

        for (int i = 0; i < Data.Count; i++)
        {
            if (Data[i].matType == type
                && Data[i].matGrade == grade
                && Data[i].cntByContext > 0)
            {
                cnt++;
            }
        }

        return cnt;
    }
}
