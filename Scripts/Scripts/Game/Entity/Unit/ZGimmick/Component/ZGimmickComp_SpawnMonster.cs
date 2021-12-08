using UnityEngine;

/// <summary> 몬스터 소환 </summary>
public class ZGimmickComp_SpawnMonster : MonoBehaviour
{
    [Header("소환할 몬스터 Id")]
    [SerializeField]
    private uint MonsterTid;

    private bool IsSpawn;

    public bool IsDead { get; private set; }

    private uint EntityId = 0;

    public ZPawn Monster { get; private set; }

    private ZGA_SpawnMonster GimmickAction;

    public E_PawnAIType AIType = E_PawnAIType.Hostile;

    public void Spawn(ZGA_SpawnMonster gimmickAction)
    {
        GimmickAction = gimmickAction;
        //tid가 셋팅되지 않았다면 패스. 기믹 테스트 씬이라면 패스
        if (0 >= MonsterTid || (ZGameManager.Instance.StarterData is ZGimmickStarterData))
        {
            HandleDie(0, null);
            return;
        }            

        //이미 소환함
        if (IsSpawn)
            return;

        IsSpawn = true;
        EntityId = ZGimmickManager.Instance.AddMonster();
        ZPawnManager.Instance.DoAddEventCreateMyEntity(HandleCreateMyPawn);           
    }

    private void HandleCreateMyPawn()
    {
        ZPawnManager.Instance.DoRemoveEventCreateMyEntity(HandleCreateMyPawn);
        ZPawnManager.Instance.DoAddEventCreateEntity(HandleCreatePawn);
        ZMmoManager.Instance.Field.REQ_MonsterSpawnReq(MonsterTid, transform.position, transform.rotation);
    }

    private void HandleCreatePawn(uint entityId, ZPawn pawn)
    {
        if (entityId != EntityId)
            return;

        Monster = pawn;
        Monster.DoAddEventDie(HandleDie);

        Monster.StartAI(AIType);

        ZPawnManager.Instance.DoRemoveEventCreateEntity(HandleCreatePawn);
    }

    private void HandleDie(uint attackerEntityId, ZPawn pawn)
    {
        Monster?.DoRemoveEventDie(HandleDie);
        IsDead = true;
        GimmickAction?.DieMonster();

        //mmo에서 remove 날라오지 않는다. 걍 삭제
        float delayTime = 0f;
        if(null != Monster)
        {
            delayTime = Monster.GetAnimLength(E_AnimStateName.Die_001);
        }
        Invoke("RemoveMonster", delayTime);
    }

    private void RemoveMonster()
    {
        if(ZPawnManager.hasInstance && null != Monster)
            ZPawnManager.Instance.DoRemove(Monster.EntityId);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = 0 < MonsterTid ? Color.green : Color.red;
        Gizmos.DrawCube(transform.position, Vector3.one);
    }
#endif
}