using UnityEngine;

public class ZGA_AnimSpeed : ZGimmickActionBase
{
    [Header("기믹의 애니메이션 속도를 변경한다.")]
    [SerializeField]
    private float AnimSpeed;

    protected override void InvokeImpl()
    {
        Gimmick.SetAnimParameter(E_AnimParameter.GimmickAnimSpeed, AnimSpeed);
    }

    protected override void CancelImpl()
    {
        Gimmick.SetAnimParameter(E_AnimParameter.GimmickAnimSpeed, 1f);
    }
}