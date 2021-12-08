using UnityEngine;

/// <summary> 부딪혔을 때 </summary>
public class ZGA_Bump : ZGimmickActionBase
{
    [Header("부딪혔을 때 발생할 기본 액션")]
    [SerializeField]
    private  E_TemplePresetAction PlayPresetAction = E_TemplePresetAction.WarpCheckPoint;

    [Header("PresetAction을 발동시키기위한 부딪히는 속도")]
    [SerializeField]
    private float BumpSpeedForPresetAction = 2f;

    protected override void InvokeImpl()
    {
        
    }

    protected override void CancelImpl()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        var pc = other.GetComponent<ZPawnMyPc>();

        if (null == pc)
            return;

        var rBody = Gimmick.GetComponent<Rigidbody>();

        if (null == rBody)
            return;

        if (rBody.velocity.magnitude < BumpSpeedForPresetAction)
            return;

        ZTempleHelper.PlayPresetAction(PlayPresetAction);
    }
}