using Cinemachine;
using UnityEngine;

public abstract class CameraMotorBase : MonoBehaviour
{
    protected static RaycastHit[] mHitResults = new RaycastHit[5];

    /// <summary> �ش� ������ VirtualCamera </summary>
    [SerializeField]
    private CinemachineVirtualCameraBase mVirtualCamera = null;

    /// <summary> �ش� ������ VirtualCamera </summary>
    public CinemachineVirtualCameraBase VirtualCamera { get { return mVirtualCamera; } }

    /// <summary> ī�޶� ��Ʈ�ѷ� </summary>
    protected CameraController mController;

    /// <summary> ���� ī�޶� transform </summary>
    protected Transform MainCameraTrans { get { return mController.Manager.Main.transform; } }

    /// <summary> ���� ����Ǵ� Transform </summary>
    protected Transform mCameraTrans;

    /// <summary> ī�޶� ���� ������ </summary>    
    public abstract CameraMotorSettingBase SettingBase { get; }

    /// <summary> ī�޶��� Ÿ�� </summary>
    protected Transform Target { get { return mController.Manager.Target; } }

    public E_CameraMotorType MotorType { get; private set; }

    protected bool isFixedZoom;

    /// <summary> �ش� Type �� ���ͷ� ���� </summary>
    public T ToMoter<T>() where T : CameraMotorBase
    {
        return this as T;
    }

    /// <summary> ī�޶� ���� �ʱ�ȭ </summary>
    public CameraMotorBase DoInitialize(CameraController controller, E_CameraMotorType motorType)
    {
        mController = controller;
        mCameraTrans = mVirtualCamera.transform;
        MotorType = motorType;
        gameObject.SetActive(false);

        OnInitializeImpl();

        return this;
    }

    /// <summary> ī�޶� ��� ���۽� </summary>
    public void DoBeginMotor()
    {
        gameObject.SetActive(true);
        OnBegineMotorImpl();
    }

    /// <summary> UpdateType�� ���� ȣ�� </summary>
    public void DoUpdateMotor()
    {
        UpdateDistance();
        OnUpdateMotorImpl();
    }

    /// <summary> ī�޶� ��� ����� </summary>
    public void DoEndMotor()
    {
        gameObject.SetActive(false);

        OnEndMotorImpl();
    }

    /// <summary> Ÿ���� ����Ǿ��� ��� </summary>
    public void DoChangedTarget()
    {
        OnChangedTarget();
    }

    /// <summary> �ó׸ӽ� ī�޶� ���̺� ���°� ����Ǿ��� ��� </summary>
    public void DoUpdateCameraActivated(bool bActivated)
    {
        OnCameraActivated(bActivated);
    }

    protected Vector3 GetTargetPosition()
    {
        return Target.position + SettingBase.TargetOffset;
    }

    /// <summary> Input�� ���� ī�޶� �Ÿ� ���� </summary>
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
