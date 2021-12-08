using UnityEngine;

/// <summary> ���� ī�޶� ��ġ���� �ƹ����� ���ϴ� ���� </summary>
public class CameraMotorEmpty : CameraMotorBase
{
    /// <summary> ī�޶� ���� ������ </summary>    
    [SerializeField]
    protected CameraMotorSettingBase Setting;

    /// <summary> ī�޶� ���� ������ </summary>    
    public override CameraMotorSettingBase SettingBase { get { return Setting; } }

    protected override void OnInitializeImpl()
    {
    }

    protected override void OnBegineMotorImpl()
    {
        //���� ����ī�޶� ��ġ�� ����ȭ
        mCameraTrans.position = MainCameraTrans.position;
        mCameraTrans.rotation = MainCameraTrans.rotation;
    }

    protected override void OnUpdateMotorImpl() { }
    protected override void OnEndMotorImpl() { }
    protected override void OnChangedTarget() { }
    protected override void OnCameraActivated(bool bActivated) { }
}
