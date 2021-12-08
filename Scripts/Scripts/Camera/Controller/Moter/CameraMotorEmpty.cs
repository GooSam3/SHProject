using UnityEngine;

/// <summary> 현재 카메라 위치에서 아무짓도 않하는 모터 </summary>
public class CameraMotorEmpty : CameraMotorBase
{
    /// <summary> 카메라 셋팅 데이터 </summary>    
    [SerializeField]
    protected CameraMotorSettingBase Setting;

    /// <summary> 카메라 셋팅 데이터 </summary>    
    public override CameraMotorSettingBase SettingBase { get { return Setting; } }

    protected override void OnInitializeImpl()
    {
    }

    protected override void OnBegineMotorImpl()
    {
        //현재 메인카메라 위치에 동기화
        mCameraTrans.position = MainCameraTrans.position;
        mCameraTrans.rotation = MainCameraTrans.rotation;
    }

    protected override void OnUpdateMotorImpl() { }
    protected override void OnEndMotorImpl() { }
    protected override void OnChangedTarget() { }
    protected override void OnCameraActivated(bool bActivated) { }
}
