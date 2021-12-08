using Cinemachine;
using UnityEngine;
using DG.Tweening;

/// <summary> 일인칭 카메라 </summary>
[RequireComponent(typeof(CinemachineVirtualCamera))]
public class ZGimmickComp_VirtualCameraControlFirst : ZGimmickComp_VirtualCameraControlBase
{
    protected override void UpdateCameraImpl()
    {
        if (false == IsEnableInput)
            return;

        //if (Mathf.Abs(InputX) > Mathf.Abs(InputY))
        //{
        //    AngleY += (InputX);
        //    //AngleX -= (InputY);
        //}
        //else if (Mathf.Abs(InputX) < Mathf.Abs(InputY))
        //{
        //    //AngleY += (InputX);
        //    AngleX += (InputY);
        //}

        AngleY += (InputX);
        AngleX += (InputY);

        AngleY = Mathf.Max(Mathf.Min(AngleY, MaxAngleY), - MaxAngleY);
        AngleX = Mathf.Max(Mathf.Min(AngleX, MaxAngleX), MinAngleX);
        
        VirtualCamera.transform.localRotation = Quaternion.Euler(new Vector3(-AngleX, AngleY));
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