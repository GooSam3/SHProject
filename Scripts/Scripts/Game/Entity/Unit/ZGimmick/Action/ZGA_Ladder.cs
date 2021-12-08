using UnityEngine;

/// <summary> 사다리 타기</summary>
public class ZGA_Ladder : ZGimmickActionBase
{
    [Header("사다리 타기 시작 위치")]
    [SerializeField]
    public Transform StartPosition = null;
    public Transform EndPosition = null;

    protected override void InvokeImpl()
    {
        ZTempleHelper.ChangeCharacterControlState(E_TempleCharacterControlState.Ladder, this);
    }

    protected override void CancelImpl()
    {
        ZTempleHelper.CancelCharacterControlState(E_TempleCharacterControlState.Ladder);
    }

    /// <summary> 사다리 타기 종료 위치 </summary>
    private void OnTriggerEnter(Collider other)
    {         
        var pc = other.GetComponent<ZPawnMyPc>();

        if (null == pc)
            return;

        var state = pc.GetMovement<EntityComponentMovement_Temple>().CurrentState;

        if(state is TempleCharacterControlState_Ladder ladderState)
        {
            ladderState.EndLadder();
        }
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if(null != StartPosition)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawCube(StartPosition.position, Vector3.one * 0.5f);
        }
        
        if(null != EndPosition)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawCube(EndPosition.position, Vector3.one * 0.5f);
        }
        var endCollider = GetComponent<BoxCollider>();
        if(null != endCollider)
        {
            Gizmos.color = Color.blue;
            
            Gizmos.DrawCube(transform.position + transform.rotation * endCollider.center, endCollider.size);
        }
    }
#endif
}