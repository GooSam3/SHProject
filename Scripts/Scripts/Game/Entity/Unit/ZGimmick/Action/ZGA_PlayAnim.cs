using Cinemachine;
using UnityEngine;

public class ZGA_PlayAnim : ZGimmickActionBase
{
    [Header("플레이할 애니메이션")]
    public E_AnimParameter AnimParameter;

    [Header("플레이시 카메라 연출 (Optional)")]
    [SerializeField]
    private CinemachineVirtualCamera VCamera;

    [Header("플레이시 카메라 연출 시간 (Optional)")]
    [SerializeField]
    private float VCameraActiveDuration = 2f;

    [Header("플레이시 카메라 연출 블렌딩 스타일 (Optional)")]
    [SerializeField]
    private CinemachineBlendDefinition.Style VCameraBlendStyle = CinemachineBlendDefinition.Style.EaseIn;

    [Header("플레이시 카메라 연출 블렌딩 시간 (Optional)")]
    [SerializeField]
    private float VCameraBlendDuration = 0.5f;

    [Header("역 방향 재생 여부")]
    [SerializeField]
    private bool IsPlayReverse = false;

    private void Awake()
    {
        DisableVCamera();
    }

    protected override void InvokeImpl()
    {
        Gimmick.SetAnimParameter(E_AnimParameter.GimmickAnimSpeed, IsPlayReverse ? -1f : 1f);
        Gimmick.SetAnimParameter(AnimParameter);
    }

    protected override void CancelImpl()
    {
        DisableVCamera();
    }

    protected override void PreInvoke()
    {
        SetVirtualCamera(true);
    }

    private void SetVirtualCamera(bool bActive)
    {
        if (null == VCamera)
        {
            return;
        }

        CameraManager.Instance.DoSetBrainBlendStyle(VCameraBlendStyle, VCameraBlendDuration);
            
        VCamera.gameObject.SetActive(bActive);

        if(true == bActive)
        {
            Invoke("DisableVCamera", VCameraActiveDuration);
        }
    }

    private void DisableVCamera()
    {
        CancelInvoke("DisableVCamera");
        SetVirtualCamera(false);
    }
}