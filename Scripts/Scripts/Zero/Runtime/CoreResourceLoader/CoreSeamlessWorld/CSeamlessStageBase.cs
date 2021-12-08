abstract public class CSeamlessStageBase : CMonoBase
{
    private string mStageAddressabeName; public string pStageName { get { return mStageAddressabeName; } }
    private int mStageID; public int pStageID { get { return mStageID; } }
    private int mWorldID; public int pWorldID { get { return mWorldID; } }


    public void DoSeamlessStageInitialize()
    {
        OnSeamlessStageInitialize();
    }

    //---------------------------------------------------------
    protected void ProtSeamlessStageInfo(string _StageName, int _StageID, int _WorldID)
    {
        mStageAddressabeName = _StageName;
        mStageID = _StageID;
        mWorldID = _WorldID;
    }

    protected virtual void OnSeamlessStageInitialize() { }
}
