using Cinemachine;
using UnityEngine;
using DG.Tweening;

/// <summary> 궤도 카메라 </summary>
[RequireComponent(typeof(CinemachineVirtualCamera))]
public class ZGimmickComp_VirtualCameraControlOrbit : ZGimmickComp_VirtualCameraControlBase
{
    [Header("타겟 오브젝트")]
    [SerializeField]
    protected Transform Target;

    protected override void StartImpl()
    {
        if (null != Target)
        {
            Distance = (Target.position - VirtualCamera.transform.position).magnitude;
            //VirtualCamera.transform.LookAt(Target);
        }
        else
        {
            Distance = 10f;
        }

        DefaultPos = VirtualCamera.transform.localPosition;
        DefaultAngle = VirtualCamera.transform.localRotation;
        OnFinishResetCamera();
    }

    protected override void OnFinishResetCamera()
    {
        AngleX = VirtualCamera.transform.rotation.eulerAngles.x; 
        AngleY = VirtualCamera.transform.rotation.eulerAngles.y;
    }

    protected override void UpdateCameraImpl()
    {        
        if (null == Target)
            return;

        if (true == IsReseting)
            return;

        Vector3 TargetPosition = Target.position;

        AngleX -= InputY;
        AngleY += InputX;

        AngleX = ClampAngle(AngleX, MinAngleX, MaxAngleX);

        Vector3 camforward = GetCamForward();

        //float dist = (TargetPosition - VirtualCamera.transform.position).magnitude;

        VirtualCamera.transform.position = -camforward * Distance + TargetPosition;
        VirtualCamera.transform.forward = camforward;
    }

    private Vector3 GetCamForward()
    {
        AngleX = ClampAngle(AngleX, MinAngleX, MaxAngleX);

        Quaternion quatToEuler = Quaternion.Euler(AngleX, AngleY, 0);
        Vector3 zPosition = new Vector3(0.0f, 0.0f, 1f);
        Vector3 camforward = (quatToEuler * zPosition).normalized;

        return camforward;
    }

    private float ClampAngle(float angle, float min, float max)
    {
        while (angle < -360F) { angle += 360F; }
        while (angle > 360F) { angle -= 360F; }
        return Mathf.Clamp(angle, min, max);
    }

    protected override Tweener ResetCameraPosImpl(float duration)
    {
        return VirtualCamera.transform.DOLocalMove(DefaultPos, duration);
    }

    protected override Tweener ResetCameraRotImpl(float duration)
    {
        return VirtualCamera.transform.DOLocalRotate(DefaultAngle.eulerAngles, duration);
    }
}