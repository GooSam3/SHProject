using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ZGA_BlowerSetting
{
    [Header("바람 세기")]    
    public float Power;

    [Header("발동 시간. 0 이면 무제한")]    
    public float Duration;
}

public class ZGA_Blower : ZGimmickActionBase
{
    /// <summary> 바람 Collider </summary>
    [Header("바람 Trigger. 필수!!!!")]
    public CapsuleCollider BlowerCollider;

    [SerializeField]    
    private List<ZGA_BlowerSetting> m_listSetting = new List<ZGA_BlowerSetting>();

    [Header("땅에 붙어있는 경우에도 활강 적용할지 여부")]
    public bool IsGrounded = true;

    public GameObject Fx_Wind;

    private bool IsEnableBlower = false;

    private void Awake()
    {
        if(null != Fx_Wind)
            Fx_Wind.SetActive(false);
    }

    protected override void InvokeImpl()
    {
        IsEnableBlower = true;

        if (null != Fx_Wind)
            Fx_Wind.SetActive(true);

        Gimmick.SetAnimParameter(E_AnimParameter.Start_001);
        Gimmick.SetAnimParameter(E_AnimParameter.GimmickAnimSpeed, 1f);

        ChangeInvokeAttributeLevelImple();
    }

    /// <summary> 속성 레벨 변경 및 invoke 시 바람 세기와 시간을 셋팅한다. </summary>
    protected override void ChangeInvokeAttributeLevelImple()
    {
        var setting = GetSetting(ref m_listSetting, true);

        if (null != BlowerCollider)
        {
            BlowerCollider.height = setting.Power;
        }

        if (0 < setting.Duration)
            Invoke("DisableInvoke", setting.Duration);
    }

    protected override void CancelImpl()
    {        
        DisableInvoke();
    }

    private void DisableInvoke()
    {
        IsEnableBlower = false;
        if (null != Fx_Wind)
            Fx_Wind.SetActive(false);

        //애니 속도 죽임
        Gimmick.SetAnimParameter(E_AnimParameter.GimmickAnimSpeed, 0f);
    }

    private void OnTriggerStay(Collider other)
    {
        if (false == IsEnableBlower)
        {
            return;
        }   

        if (null == BlowerCollider)
        {
            ZLog.LogError(ZLogChannel.Temple, "CapsuleCollider가 셋팅되지 않았다.");
            return;
        }            

        ZPawnMyPc pc = other.GetComponent<ZPawnMyPc>();

        if (null == pc || false == other.isTrigger)
            return;

        //중점(시작점)보다 위에 있어야한다.
        float angle = Vector3.Angle(transform.forward, (pc.Position - transform.position).normalized);

        if (angle > 90)
            return;

        float height = BlowerCollider.height;
        float distance = (transform.position - pc.Position).magnitude;
        
        float power = Mathf.Clamp(height - distance, 0f, height);

        var value = transform.forward * power;

        EntityComponentMovement_Temple movement = pc.GetMovement<EntityComponentMovement_Temple>();
        if(movement.CurrentState is TempleCharacterControlState_Default defaultMovement)
        {
            //땅에 붙어있을 경우 처리
            if (false == IsGrounded && (defaultMovement.CurrentCharacterState == E_TempleCharacterState.Grounded || defaultMovement.CurrentCharacterState == E_TempleCharacterState.Jumping || defaultMovement.CurrentCharacterState == E_TempleCharacterState.Sliding))
                return;
            
            defaultMovement.DoAddMomentum(value, 1f * power / height);
        }
    }
}