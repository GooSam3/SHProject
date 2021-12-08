using UnityEngine;
using Cinemachine;

/// <summary> 카메라 연출용 액션 </summary>
public class ZGA_VirtualCamera : ZGimmickActionBase
{

    [Header("활성화시 카메라 연출 (Optional)")]
    [SerializeField]
    protected CinemachineVirtualCamera VCamera;

    [Header("활성화시 카메라 연출 시간 (Optional)")]
    [SerializeField]
    protected float VCameraActiveDuration = 2f;

    [Header("플레이시 카메라 연출 블렌딩 시간 (Optional)")]
    [SerializeField]
    protected float VCameraBlendDuration = 0.5f;

    [Header("활성화시 카메라 연출 블렌딩 스타일 (Optional)")]
    [SerializeField]
    protected CinemachineBlendDefinition.Style VCameraBlendStyle = CinemachineBlendDefinition.Style.EaseIn;

    protected override void InitializeImpl()
    {
        DisableVirtualCamera();
    }

    protected override void InvokeImpl()
    {
        SetVirtualCamera(true);
    }

    protected override void DestroyImpl()
    {
        DisableVirtualCamera();
    }

    protected override void CancelImpl()
    {
        DisableVirtualCamera();
    }

    private void SetVirtualCamera(bool bActive)
    {
        if (null == VCamera)
        {
            return;
        }

        if (bActive)
        {
            CameraManager.Instance.DoSetBrainBlendStyle(VCameraBlendStyle, VCameraBlendDuration);
            Invoke(nameof(DisableVirtualCamera), VCameraActiveDuration);
        }

        VCamera.gameObject.SetActive(bActive);
    }

    private void DisableVirtualCamera()
    {
        SetVirtualCamera(false);
    }
}