using GameDB;
using UnityEngine;

/// <summary> On/Off 전환 스위치 </summary>
public class ZGA_Switch : ZGimmickActionBase
{
    [Header("목표 기믹 Id")]
    [SerializeField]
    private string FindGimmickId = string.Empty;

    [Header("해당 기믹에 발동시킬 속성 레벨")]
    [SerializeField]
    private E_AttributeLevel AttributeLevel = E_AttributeLevel.Level_1;

    [Header("활성화시 플레이할 애니메이션")]
    public E_AnimParameter AnimParameterOn = E_AnimParameter.Start_001;

    [Header("비활성화시 플레이할 애니메이션")]
    public E_AnimParameter AnimParameterOff = E_AnimParameter.End_001;

    [Header("기믹이 활성화 됐을때 이펙트")]
    [SerializeField]
    private GameObject Fx_On;
    [Header("기믹이 비활성화 됐을때 이펙트")]
    [SerializeField]
    private GameObject Fx_Off;

    [Header("최초 활성화 비활성화 처리")]
    [SerializeField]
    private bool IsEnableSwitch = false;

    protected override void InitializeImpl()
    {
        base.InitializeImpl();
        ZTempleHelper.SetActiveFx(Fx_On, false);
        ZTempleHelper.SetActiveFx(Fx_Off, true);
        PlayAnim();
    }

    protected override void InvokeImpl()
    {
        IsEnableSwitch = !IsEnableSwitch;

        ZTempleHelper.EnableToggleGimmicks(FindGimmickId, AttributeLevel, true);

        ZTempleHelper.SetActiveFx(Fx_On, IsEnableSwitch);
        ZTempleHelper.SetActiveFx(Fx_Off, !IsEnableSwitch);

        PlayAnim();
    }

    protected override void CancelImpl()
    {        
    }

    private void PlayAnim()
    {
        if (IsEnableSwitch)
        {
            Gimmick.SetAnimParameter(AnimParameterOn);
        }
        else
        {
            Gimmick.SetAnimParameter(AnimParameterOff);
        }
    }
}