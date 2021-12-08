using Cinemachine;
using UnityEngine;

public class CameraMotorShoulder : CameraMotorBase
{
    /// <summary> ī�޶� ���� ������ </summary>    
    [SerializeField]
    protected CameraMotorSettingShoulder Setting;

    /// <summary> ī�޶� ���� ������ </summary>    
    public override CameraMotorSettingBase SettingBase { get { return Setting; } }

    /// <summary> �ó׸ӽſ��� �����Ǵ� freelook class </summary>
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
