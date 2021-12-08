using UnityEngine;

/// <summary> 점프 트리거. 기믹과 별개로 처리 </summary>
public class ZGimmickComp_Jump : MonoBehaviour
{
    [Header("점프 파워!")]
    [SerializeField]
    private float JumpPower = 10f;

    [Header("언제든 점프!")]
    [SerializeField]
    private bool IsForce = false;
        
    private void OnTriggerEnter(Collider other)
    {
        ZPawnMyPc pc = other.GetComponent<ZPawnMyPc>();

        if (null == pc || false == other.isTrigger)
            return;

        //TODO :: UI 버튼 누를 경우에 동작해야한다면 처리 필요.
        Jump(pc);
    }

    private void OnTriggerExit(Collider other)
    {
        //TODO :: UI 버튼 누를 경우에 동작해야한다면 처리 필요.
    }

    private void Jump(ZPawnMyPc pc)
    {
        EntityComponentMovement_Temple movement = pc.GetMovement<EntityComponentMovement_Temple>();
        if (null != movement && movement.CurrentState is TempleCharacterControlState_Default defaultMovement)
        {
            if(false == IsForce)
            {
                //땅에 있을 경우에만 처리되야함.
                if (defaultMovement.CurrentCharacterState != E_TempleCharacterState.Grounded)
                    return;
            }

            defaultMovement.OnJumpStart(JumpPower, IsForce);
        }
    }
}