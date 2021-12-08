using UnityEngine;

public class ZSeamlessStageAttacher : CSeamlessStageAttacherBase
{
    [SerializeField]
    private E_WorldID WorldType = E_WorldID.None;
    [SerializeField]
    private E_StageID StageID = E_StageID.None;

    //-----------------------------------------------------------------------
    protected override void OnSeamlessStageInitialize() 
    {
        ProtSeamlessStageInfo(StageID.ToString(), (int)StageID, (int)WorldType);
    }
}
