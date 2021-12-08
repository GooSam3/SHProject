using UnityEngine;

/// <summary> 물건 옮기기 </summary>
public class ZGA_Carry : ZGimmickActionInteractionBase<Collider>
{
    protected override E_TempleCharacterControlState ChangeControlStateType { get { return E_TempleCharacterControlState.Carry; } }

    [Header("던지기시 받을 힘 배율")]
    [SerializeField]
    private float mThrowPowerRate = 1f;

    public float ThrowPowerRate { get { return mThrowPowerRate; } }

    protected override void HandleInteraction(TempleCharacterControlStateBase state)
    {
        state.ChangeState(ChangeControlStateType, Gimmick);
    }

    public override void DisableAction()
    {
        if (false == IsEnableInteraction)
            return;

        HideInteractionUI();
        IsEnableInteraction = false;
        ZTempleHelper.CancelCharacterControlState(ChangeControlStateType);
    }
}