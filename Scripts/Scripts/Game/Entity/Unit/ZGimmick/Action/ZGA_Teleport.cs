using UnityEngine;

public class ZGA_Teleport : ZGimmickActionBase
{
    [Header("이동할 위치")]
    public Transform TargetPosition = null;

    protected override void InvokeImpl()
    {
        if (null == TargetPosition)
        {
            ZLog.LogError(ZLogChannel.Temple, $"TargetPosition 를 입력해야한다!");
            return;
        }
        
        ZPawnManager.Instance.MyEntity.Warp(TargetPosition.position);
    }

    protected override void CancelImpl()
    {
    }
}