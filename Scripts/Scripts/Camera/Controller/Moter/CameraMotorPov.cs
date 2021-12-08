using UnityEngine;

public class CameraMotorPov : CameraMotorBase
{
    /// <summary> 카메라 셋팅 데이터 </summary>    
    [SerializeField]
    protected CameraMotorSettingShoulder Setting;

    /// <summary> 카메라 셋팅 데이터 </summary>    
    public override CameraMotorSettingBase SettingBase { get { return Setting; } }
    public float forwardDistance = 2.0f;
    public Vector3 cameraRotate = Vector3.zero;

    public void SetCameraRotate(Vector3 rotate)
    {
        cameraRotate = rotate;
    }

    protected override void OnUpdateMotorImpl()
    {
        if (null == Target)
            return;

        var myEntity = ZPawnManager.Instance.MyEntity;
        Vector3 camDir = Quaternion.Euler(Setting.Angle) * (myEntity.transform.forward);
        Vector3 camPos = (GetTargetPosition() - camDir) + (myEntity.transform.forward * 2.0f);
        mCameraTrans.position = camPos;
        mCameraTrans.forward = camDir;
        mCameraTrans.rotation *= Quaternion.Euler(cameraRotate);
    }

    protected override void OnCameraActivated(bool bActivated)
    {
        if (bActivated == true)
        {
            cameraRotate = Vector3.zero;
        }
    }

    protected override void OnEndMotorImpl() { }
    protected override void OnChangedTarget() { }
    protected override void OnInitializeImpl() { }
    protected override void OnBegineMotorImpl() { }
}
