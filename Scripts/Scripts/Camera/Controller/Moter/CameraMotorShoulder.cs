using Cinemachine;
using UnityEngine;

public class CameraMotorShoulder : CameraMotorBase
{
    /// <summary> 카메라 셋팅 데이터 </summary>    
    [SerializeField]
    protected CameraMotorSettingShoulder Setting;

    /// <summary> 카메라 셋팅 데이터 </summary>    
    public override CameraMotorSettingBase SettingBase { get { return Setting; } }

    /// <summary> 시네머신에서 제공되는 freelook class </summary>
    private CinemachineFreeLook FreeLook;

    protected override void OnInitializeImpl()
    {
        FreeLook = GetComponentInChildren<CinemachineFreeLook>();

        FreeLook.m_YAxisRecentering = Setting.RecenteringY;
        FreeLook.m_XAxis.m_InputAxisName = string.Empty;
        FreeLook.m_YAxis.m_InputAxisName = string.Empty;
    }

    protected override void OnBegineMotorImpl()
    {
        OnChangedTarget();
    }

    protected override void OnUpdateMotorImpl()
    {
        FreeLook.m_XAxis.m_InputAxisValue = -mController.InputX;
        FreeLook.m_YAxis.m_InputAxisValue = -mController.InputY;
    }

    protected override void OnEndMotorImpl()
    {
    }

    protected override void OnChangedTarget()
    {
        FreeLook.LookAt = Target;
        FreeLook.Follow = Target;
    }

    protected override void OnCameraActivated(bool bActivated)
    {
    }
}
