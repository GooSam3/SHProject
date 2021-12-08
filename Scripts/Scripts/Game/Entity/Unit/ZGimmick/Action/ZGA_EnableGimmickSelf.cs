using GameDB;
using UnityEngine;

public class ZGA_EnableGimmickSelf : ZGimmickActionBase
{
    [Header("활성화/비활성화 플래그")]
    public bool IsEnableGimmick = true;

    [Header("해당 기믹에 발동시킬 속성 레벨")]
    [SerializeField]
    private E_AttributeLevel AttributeLevel = E_AttributeLevel.Level_1;

    [Header("강제 활성화 여부")]
    [SerializeField]
    private bool IsForce = true;

    protected override void InvokeImpl()
    {
        Gimmick.SetEnable(IsEnableGimmick, AttributeLevel, IsForce);
    }

    protected override void CancelImpl()
    {

    }
}