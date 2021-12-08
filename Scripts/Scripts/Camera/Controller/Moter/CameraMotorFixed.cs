using UnityEngine;

public class CameraMotorFixed : CameraMotorBase
{
    /// <summary> 카메라 셋팅 데이터 </summary>    
    [SerializeField]
    protected CameraMotorSettingFixed Setting;

    /// <summary> 카메라 셋팅 데이터 </summary>    
    public override CameraMotorSettingBase SettingBase { get { return Setting; } }

    protected override void OnInitializeImpl()
    {
    }

    protected override void OnBegineMotorImpl()
    {
    }

    protected override void OnUpdateMotorImpl()
    {
        if (null == Target)
            return;

        Vector3 camDir = Quaternion.Euler(Setting.Angle) * Vector3.forward;
        Vector3 camPos = GetTargetPosition() - camDir * Setting.Distance;

        mCameraTrans.position = camPos;
        mCameraTrans.forward = camDir;
    }

    protected override void OnEndMotorImpl()
    {
    }

    protected override void OnChangedTarget()
    {
    }

    protected override void OnCameraActivated(bool bActivated)
    {
    }
}
