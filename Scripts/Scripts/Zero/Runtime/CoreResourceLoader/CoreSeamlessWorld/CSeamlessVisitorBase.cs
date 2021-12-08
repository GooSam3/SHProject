using UnityEngine;

abstract public class CSeamlessVisitorBase : CMonoBase
{
    //-----------------------------------------------------
    protected override void OnUnityAwake()
    {
        base.OnUnityAwake();
        DontDestroyOnLoad(gameObject);
    }

    //-----------------------------------------------------
    private void OnTriggerEnter(Collider other)
    {
        CSeamlessStageAttacherBase StageAttacher = other.gameObject.GetComponentInChildren<CSeamlessStageAttacherBase>();
        if (StageAttacher != null)
        {
            ManagerSceneStreamBase.Instance.ImportVisit(StageAttacher);
            OnSeamlessVisitorEnter(StageAttacher);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        CSeamlessStageAttacherBase StageAttacher = other.gameObject.GetComponentInChildren<CSeamlessStageAttacherBase>();
        if (StageAttacher != null)
        {
            OnSeamlessVisitorExit(StageAttacher);
        }
    }

    //-----------------------------------------------------
    protected virtual void OnSeamlessVisitorEnter(CSeamlessStageAttacherBase _StageAttacher) {}
    protected virtual void OnSeamlessVisitorExit(CSeamlessStageAttacherBase _StageAttacher) {}
}
