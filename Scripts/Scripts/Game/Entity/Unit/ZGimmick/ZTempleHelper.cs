using GameDB;
using UnityEngine;

public class ZTempleHelper
{
    public static void SetActiveFx(GameObject fx, bool bActive)
    {
        if (null == fx)
            return;

        fx.SetActive(bActive);

        if (bActive)
        {
            foreach (var particle in fx.GetComponentsInChildren<ParticleSystem>())
            {
                particle.Play();
            }
        }
    }

    /// <summary> 해당 기믹 id를 활성화/비활성화 한다 </summary>
    public static void EnableGimmicks(string gimmickId, bool bEnable, E_AttributeLevel level, bool bForce)
    {
        if (false == ZGimmickManager.Instance.TryGetValue(gimmickId, out var gimmicks))
            return;

        foreach (var gimmick in gimmicks)
        {
            gimmick.SetEnable(bEnable, level, bForce);
        }
    }

    /// <summary> 해당 기믹 id를 활성화/비활성화 한다 </summary>
    public static void EnableToggleGimmicks(string gimmickId, E_AttributeLevel level, bool bForce)
    {
        if (false == ZGimmickManager.Instance.TryGetValue(gimmickId, out var gimmicks))
            return;

        foreach (var gimmick in gimmicks)
        {
            gimmick.SetEnable(!gimmick.IsEnabled, level, bForce);
        }
    }

    /// <summary> 해당 기믹 id의 속성 레벨을 변경한다 </summary>
    public static void ChangeGimmickAttributeLevel(string gimmickId, E_AttributeLevel level)
    {
        if (false == ZGimmickManager.Instance.TryGetValue(gimmickId, out var gimmicks))
            return;

        foreach (var gimmick in gimmicks)
        {
            gimmick.ChangeAttributeLevel(level);
        }
    }

    // NOTE(JWK): 단순히 기믹 레벨이 변경되는게아닌 up , down 될때 사용
    /// <summary> 해당 기믹 id의 속성 레벨을 변경한다 </summary>
    public static void ChangeGimmickAttributeLevel(string gimmickId, bool isUpgrade)
    {
        if (false == ZGimmickManager.Instance.TryGetValue(gimmickId, out var gimmicks))
            return;

        foreach (var gimmick in gimmicks)
        {
            gimmick.ChangeAttributeLevel(isUpgrade);
        }
    }

    /// <summary> 해당 기믹을 해당 타입으로 작동시킨다. </summary>
    public static void InvokeGimmickActions(string gimmickId, E_GimmickActionInvokeType invokeType, E_AttributeLevel level)
    {
        if (false == ZGimmickManager.Instance.TryGetValue(gimmickId, out var gimmicks))
            return;

        foreach (var gimmick in gimmicks)
        {
            gimmick.InvokeActions(invokeType, level);
        }
    }

    /// <summary> 미리 정의된 액션 사용 </summary>
    public static void PlayPresetAction(E_TemplePresetAction action)
    {
        switch(action)
        {
            case E_TemplePresetAction.Die:
                break;
            case E_TemplePresetAction.WarpCheckPoint:
                {
                    PlayCommonAction_WarpCheckPoint();
                }
                break;
            case E_TemplePresetAction.Stun:
                break;
        }
    }

    /// <summary> 체크 포인트로 이동시킨다 </summary>
    private static void PlayCommonAction_WarpCheckPoint()
    {
        Rigidbody rBody = ZPawnManager.Instance.MyEntity.GetComponent<Rigidbody>();

        if (null != rBody)
        {
            rBody.velocity = Vector3.zero;
            rBody.angularVelocity = Vector3.zero;
        }
        
        CancelCharacterControlState();

        ZPawnManager.Instance.MyEntity.Warp(ZPawnManager.Instance.TempleCheckPosition);
        ZPawnManager.Instance.MyEntity.transform.rotation = ZPawnManager.Instance.TempleCheckRotation;
    }

    /// <summary> 유적에서 기믹 or PC 가 스킬을 사용할때 사용 </summary>
    
    /// - attacker ID 가 0 인 경우 '기믹' 으로 처리하기로 함
    /// - Gimmick 이 스킬을 사용할때는, attacker 는 'pc' 로 넣고 skillId 는 기믹의 스킬 ..?
    /// 
    public static void TempleEntityAttack(EntityBase attacker, EntityBase target, uint skillTid, uint damage = 0)
	{
        ZMmoManager.Instance.Field.REQ_DamageReq(attacker.EntityId, target.EntityId, skillTid, damage);
        //attacker.ForceMove();
    }

    #region ===== :: 캐릭터 조작 상태 관련 처리 :: =====
    public static void ChangeCharacterControlState(E_TempleCharacterControlState stateType, params object[] args)
    {
        var pc = ZPawnManager.Instance.MyEntity;
        var state = pc.GetMovement<EntityComponentMovement_Temple>().CurrentState;

        //기본 상태가 아니면 패스
        if (null == state || state.StateType != E_TempleCharacterControlState.Default)
        {
            return;
        }

        var defaultState = state as TempleCharacterControlState_Default;

        //땅에 붙어있는 상태가 아니라면 패스
        if (defaultState.CurrentCharacterState != E_TempleCharacterState.Grounded)
        {
            return;
        }

        state.ChangeState(stateType, args);
    }

    public static void CancelCharacterControlState()
    {
        var pc = ZPawnManager.Instance.MyEntity;
        var state = pc.GetMovement<EntityComponentMovement_Temple>()?.CurrentState;

        //디펄트 상태면 패스
        if (null == state || state.StateType == E_TempleCharacterControlState.Default)
        {
            return;
        }

        // NOTE(JWK): push , carry 동작이 들어오면서 충돌이 들어오는 경우가 있어서 우선 강제로 처리하게 수정
        state.Cancel(true);
    }

    /// <summary>  </summary>
    public static void CancelCharacterControlState(E_TempleCharacterControlState cancelStateType = E_TempleCharacterControlState.Empty)
    {
        var pc = ZPawnManager.Instance.MyEntity;

        if (null == pc)
            return;


        var state = pc.GetMovement<EntityComponentMovement_Temple>()?.CurrentState;

        //체크할 상태가 아니면 패스
        if (null == state || state.StateType != cancelStateType)
        {
            return;
        }

        state.Cancel();
    }
    #endregion
}