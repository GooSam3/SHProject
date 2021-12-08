using Cinemachine;
using UnityEngine;

public abstract class CameraMotorBase : MonoBehaviour
{
    protected static RaycastHit[] mHitResults = new RaycastHit[5];

    /// <summary> 해당 모터의 VirtualCamera </summary>
    [SerializeField]
    private CinemachineVirtualCameraBase mVirtualCamera = null;

    /// <summary> 해당 모터의 VirtualCamera </summary>
    public CinemachineVirtualCameraBase VirtualCamera { get { return mVirtualCamera; } }

    /// <summary> 카메라 컨트롤러 </summary>
    protected CameraController mController;

    /// <summary> 메인 카메라 transform </summary>
    protected Transform MainCameraTrans { get { return mController.Manager.Main.transform; } }

    /// <summary> 실제 제어되는 Transform </summary>
    protected Transform mCameraTrans;

    /// <summary> 카메라 셋팅 데이터 </summary>    
    public abstract CameraMotorSettingBase SettingBase { get; }

    /// <summary> 카메라의 타겟 </summary>
    protected Transform Target { get { return mController.Manager.Target; } }

    public E_CameraMotorType MotorType { get; private set; }

    protected bool isFixedZoom;

    /// <summary> 해당 Type 의 모터로 리턴 </summary>
    public T ToMoter<T>() where T : CameraMotorBase
    {
        return this as T;
    }

    /// <summary> 카메라 모터 초기화 </summary>
    public CameraMotorBase DoInitialize(CameraController controller, E_CameraMotorType motorType)
    {
        mController = controller;
        mCameraTrans = mVirtualCamera.transform;
        MotorType = motorType;
        gameObject.SetActive(false);

        OnInitializeImpl();

        return this;
    }

    /// <summary> 카메라 모드 시작시 </summary>
    public void DoBeginMotor()
    {
        gameObject.SetActive(true);
        OnBegineMotorImpl();
    }

    /// <summary> UpdateType에 따라 호출 </summary>
    public void DoUpdateMotor()
    {
        UpdateDistance();
        OnUpdateMotorImpl();
    }

    /// <summary> 카메라 모드 종료시 </summary>
    public void DoEndMotor()
    {
        gameObject.SetActive(false);

        OnEndMotorImpl();
    }

    /// <summary> 타겟이 변경되었을 경우 </summary>
    public void DoChangedTarget()
    {
        OnChangedTarget();
    }

    /// <summary> 시네머신 카메라 라이브 상태가 변경되었을 경우 </summary>
    public void DoUpdateCameraActivated(bool bActivated)
    {
        OnCameraActivated(bActivated);
    }

    protected Vector3 GetTargetPosition()
    {
        return Target.position + SettingBase.TargetOffset;
    }

    /// <summary> Input에 따른 카메라 거리 변경 </summary>
    private void UpdateDistance()
    {
        if (0 == mController.InputZoom)
            return;

        if( isFixedZoom == false ) {
            float dist = SettingBase.Distance - mController.InputZoom * ( DBConfig.Zoom_Speed * 10.0f );
            SettingBase.Distance = Mathf.Clamp( dist, SettingBase.MinDistance, SettingBase.MaxDistance );
        }
    }

    protected abstract void OnInitializeImpl();
    protected abstract void OnBegineMotorImpl();
    protected abstract void OnUpdateMotorImpl();
    protected abstract void OnEndMotorImpl();
    protected abstract void OnChangedTarget();
    protected abstract void OnCameraActivated(bool bActivated);
}
