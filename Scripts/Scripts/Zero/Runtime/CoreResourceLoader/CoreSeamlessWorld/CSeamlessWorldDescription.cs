using System.Collections.Generic;

// CManagerLoaderSeamlessWorldBase 에 입력될 월드 정보를 표현 
// 

public class CSeamlessWorldDescription : CMonoBase
{
    public class SSeamlessWorld
    {
        public int WorldID = 0;
        public Dictionary<int, CSeamlessStageNodeBase> StageNode = new Dictionary<int, CSeamlessStageNodeBase>();
        public CSeamlessStageNodeBase FindSeamlessStageNode(int StageID) { CSeamlessStageNodeBase Result = null;
            if (StageNode.ContainsKey(StageID)) { Result = StageNode[StageID]; } return Result;}
    }
    private Dictionary<int, SSeamlessWorld> m_dicSeamlessWorld = new Dictionary<int, SSeamlessWorld>();
    //---------------------------------------------------------------
    protected override void OnUnityAwake()
    {
        base.OnUnityAwake();
        PrivSeamlessDescriptionInitilize();
    }

    protected override void OnUnityStart()
    {
        base.OnUnityStart();       
    }

    //---------------------------------------------------------------
    public void DoWorldDescriptionStageOpen(int _WorldID, int _StageID, List<CSeamlessStageBase> _OutListStageOpen)
    {
        if (m_dicSeamlessWorld.ContainsKey(_WorldID))
        {
            SSeamlessWorld SeamlessWorld = m_dicSeamlessWorld[_WorldID];
            CSeamlessStageNodeBase StageNode = SeamlessWorld.FindSeamlessStageNode(_StageID);
            if (StageNode != null)
            {
                StageNode.DoSeamlessStageOpen(SeamlessWorld, _OutListStageOpen);
            }
        }
    }

    //---------------------------------------------------------------
    private void PrivSeamlessDescriptionInitilize()
    {
        List<CSeamlessStageNodeBase> ListStageNode = new List<CSeamlessStageNodeBase>();
        GetComponentsInChildren(true, ListStageNode);

        for (int i = 0; i < ListStageNode.Count; i++)
        {
            ListStageNode[i].DoSeamlessStageInitialize();
            PrivSeamlessDescriptionInsertNode(ListStageNode[i]);
        }
    }

    private void PrivSeamlessDescriptionInsertNode(CSeamlessStageNodeBase _InsertNode)
    {
        SSeamlessWorld SeamlessWorld = null;
        if (m_dicSeamlessWorld.ContainsKey(_InsertNode.pWorldID))
        {
            SeamlessWorld = m_dicSeamlessWorld[_InsertNode.pWorldID];
        }
        else
        {
            SeamlessWorld = new SSeamlessWorld();
            m_dicSeamlessWorld.Add(_InsertNode.pWorldID, SeamlessWorld);
        }

        SeamlessWorld.StageNode.Add(_InsertNode.pStageID, _InsertNode);
    }


    //----------------------------------------------------------------
  

}
