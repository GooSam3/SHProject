using GameDB;
using System;
using System.Collections.Generic;

public class UIFrameArtifactSelectMaterialMaterialHandler : UIFrameArtifactMaterialManagementBase
{
    Action<int, MaterialDataOnSlot> onSlotClicked;

    public override void Initialize(UIFrameArtifact artifact, bool showCount, bool disableOnCountZero)
    {
        base.Initialize(artifact, showCount, disableOnCountZero);
    }

    public void AddListener_OnClick(Action<int, MaterialDataOnSlot> callback)
    {
        onSlotClicked += callback;
    }

    public override void RefreshScroll()
    {
        for (int i = 0; i < Data.Count; i++)
        {
            if (Data[i] != null)
            {
                Data[i].isChecked = Data[i].cntByContext < Data[i].oriCnt;
            }
        }

        base.RefreshScroll();
    }

    ///// <summary>
    ///// 나의 재료 선택칸은 카운트가 0 이 되어도 사라지면 안됨 그리고 
    ///// Dest 는 데이터 카운팅하지말고 새로 슬롯에 등록대게끔 설정 
    ///// </summary>
    //public override void MoveData(List<MaterialDataOnSlot> target, E_ArtifactMaterialType type, uint grade)
    //{
    //    base.MoveData(false, false, Data, target, type, grade);
    //}

    public void MoveSpecificData(MaterialDataOnSlot materialMoved, List<MaterialDataOnSlot> destList)
    {
        if (materialMoved.cntByContext == 0)
            return;

        for (int i = 0; i < destList.Count; i++)
        {
            if (destList[i].cntByContext == 0 &&
                destList[i].matType == materialMoved.matType &&
                destList[i].matGrade == materialMoved.matGrade)
            {
                materialMoved.cntByContext--;
                destList[i].SetSingleCount();
                destList[i].tid = materialMoved.tid;
                return;
            }
        }
    }

    public void Set(List<MaterialDataOnSlot> _data)
    {
        CopyData(_data, Data);
        RefreshScroll();
    }

    public bool IsExist(E_ArtifactMaterialType type, uint grade, out MaterialDataOnSlot outputData)
    {
        outputData = null;
        var target = Data.Find(t => t.cntByContext > 0 && t.matType == type && t.matGrade == grade);
        if (target != null)
        {
            outputData = target;
            return true;
        }
        return false;
    }

    protected override void OnSlotClicked(int index, MaterialDataOnSlot data)
    {
        onSlotClicked?.Invoke(Data.IndexOf(data), data);
    }
}
