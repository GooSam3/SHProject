using System.Collections.Generic;
using UnityEngine;

// 방향 기반의 탐색 노드 시스템 제공 

abstract public class CSeamlessStageNodeBase : CSeamlessStageBase
{
    protected class SStageNode
    {
        public int Direction = 0;
        public int Depth = 0;
        public int StageID;
    }
    private Dictionary<int, SStageNode> m_dicReleativeNode = new Dictionary<int, SStageNode>(); // 방향, 스테이지 이름 (Addressable Label)
    [SerializeField]
    private Vector3 StageLocation = Vector3.zero;
    [SerializeField]
    private Vector3 StageBound = Vector3.zero;

    //-----------------------------------------------------------
    public void DoSeamlessStageOpen(CSeamlessWorldDescription.SSeamlessWorld _SeamlessWorld, List<CSeamlessStageBase> _OutListStageOpen)
    {
        PrivStageNodeInsertList(_OutListStageOpen);

        Dictionary<int, SStageNode>.Enumerator it = m_dicReleativeNode.GetEnumerator();
        while (it.MoveNext())
        {
            SStageNode StageNode = it.Current.Value;
            CSeamlessStageNodeBase DirectionStage = _SeamlessWorld.FindSeamlessStageNode(StageNode.StageID);
            if (DirectionStage != null)
            {
                DirectionStage.DoSeamlessStageVisit(StageNode.Depth - 1, _SeamlessWorld, _OutListStageOpen);
            }
        }
    }

    public void DoSeamlessStageVisit(int _Depth, CSeamlessWorldDescription.SSeamlessWorld _SeamlessWorld, List<CSeamlessStageBase> _OutListStageOpen)
    {
        PrivStageNodeInsertList(_OutListStageOpen);

        if (_Depth <= 0) return;

        Dictionary<int, SStageNode>.Enumerator it = m_dicReleativeNode.GetEnumerator();

        while (it.MoveNext())
        {
            SStageNode StageNode = it.Current.Value;

            CSeamlessStageNodeBase DirectionStage = _SeamlessWorld.FindSeamlessStageNode(StageNode.StageID);
            if (DirectionStage != null)
            {
                DirectionStage.DoSeamlessStageVisitDirection(StageNode.Direction, _Depth - 1, _SeamlessWorld, _OutListStageOpen);
            }
        }
    }

    public void DoSeamlessStageVisitDirection(int _Direction, int _Depth, CSeamlessWorldDescription.SSeamlessWorld _SeamlessWorld, List<CSeamlessStageBase> _OutListStageOpen)
    {
        PrivStageNodeInsertList(_OutListStageOpen);

        if (_Depth <= 0) return;

        Dictionary<int, SStageNode>.Enumerator it = m_dicReleativeNode.GetEnumerator();
        while (it.MoveNext())
        {
            SStageNode StageNode = it.Current.Value;
            if (StageNode.Direction == _Direction)
            {
                CSeamlessStageNodeBase DirectionStage = _SeamlessWorld.FindSeamlessStageNode(StageNode.StageID);
                if (DirectionStage != null)
                {
                    DirectionStage.DoSeamlessStageVisitDirection(_Direction, _Depth - 1, _SeamlessWorld, _OutListStageOpen);
                }
            }
        }

    }


    //------------------------------------------------------------
    protected void ProtStageNodeAddReleative(int _StageID, int _Direction, int _Depth)
    {
        SStageNode NewStageNode = new SStageNode();
        NewStageNode.Direction = _Direction;
        NewStageNode.Depth = _Depth;
        NewStageNode.StageID = _StageID;
        m_dicReleativeNode.Add(_Direction, NewStageNode);
    }

    //------------------------------------------------------------
    private void PrivStageNodeInsertList(List<CSeamlessStageBase> _OutListStageOpen)
    {
        if (_OutListStageOpen.Contains(this) == false)
        {
            _OutListStageOpen.Add(this);
        }
    }
  
}
