using GameDB;
using System.Collections.Generic;
using UnityEngine;

public class ZGA_EnableGimmick : ZGimmickActionBase
{
    [Header("활성화/비활성화할 기믹 ID")]
    public string FindGimmickId;

    [Header("활성화/비활성화할 기믹 ID들")]
    public List<string> FindGimmickIds = new List<string>();

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
        ZTempleHelper.EnableGimmicks(FindGimmickId, true, AttributeLevel, IsForce);

        foreach (var id in FindGimmickIds)
        {
            ZTempleHelper.EnableGimmicks(id, true, AttributeLevel, IsForce);
        }
    }

    protected override void CancelImpl()
    {
        ZTempleHelper.EnableGimmicks(FindGimmickId, false, AttributeLevel, IsForce);

        foreach (var id in FindGimmickIds)
        {
            ZTempleHelper.EnableGimmicks(id, false, AttributeLevel, IsForce);
        }
    }
}