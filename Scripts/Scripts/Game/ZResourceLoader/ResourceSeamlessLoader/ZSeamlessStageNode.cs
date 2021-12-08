using System.Collections.Generic;
using UnityEngine;

public class ZSeamlessStageNode : CSeamlessStageNodeBase
{
    [System.Serializable]
    public class SStageNodeInfo
    {
        public E_SeamlessDirection    Direction = E_SeamlessDirection.Left;
        public E_StageID             StageID   = E_StageID.None;
        public int                  Depth = 2;
    }

    [SerializeField]
    private E_WorldID WorldType = E_WorldID.None;
    [SerializeField]
    private E_StageID StageID   = E_StageID.None;
    [SerializeField]
    private List<SStageNodeInfo> LinkedNode = new List<SStageNodeInfo>();
    //---------------------------------------------------------------------------
    protected override void OnSeamlessStageInitialize()
    {
        ProtSeamlessStageInfo(StageID.ToString(), (int)StageID, (int)WorldType);
        PrivStageNodeInitialize();
    }

    //----------------------------------------------------------------------------
    private void PrivStageNodeInitialize()
    {
        for (int i = 0; i < LinkedNode.Count; i++)
        {
            SStageNodeInfo NodeInfo = LinkedNode[i];
            ProtStageNodeAddReleative((int)NodeInfo.StageID, (int)NodeInfo.Direction, NodeInfo.Depth);
        }
    }



}
