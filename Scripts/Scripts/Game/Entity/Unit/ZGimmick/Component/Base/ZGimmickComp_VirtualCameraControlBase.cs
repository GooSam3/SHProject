using Cinemachine;
using UnityEngine;

using DG.Tweening;

/// <summary> 현재 활성화된 virtual camera 를 조작한다. </summary>
[RequireComponent(typeof(CinemachineVirtualCamera))]
public abstract class ZGimmickComp_VirtualCameraControlBase : MonoBehaviour
{
    [Header("위, 아래 최대 각도")]
    [SerializeField]
    [Range(20, 90)]
    protected float MaxAngleX = 90f;

    [Header("위, 아래 최소 각도")]
    [SerializeField]
    [Range(-85, 20)]
    protected float MinAngleX = 0f;

    [Header("좌, 우 최대 각도")]
    [SerializeField]
    [Range(0f, 180f)]
    protected float MaxAngleY = 60f;

    [Header("감도")]
    [SerializeField]
    private float Sensitivity = 1;

    [Header("리셋 대기 시간")]
    [SerializeField]
    private float ResetDelayTime = 2f;

    [Header("리셋 연출 시간")]
    [SerializeField]
    private float ResetDuration = 1f;

    protected CinemachineVirtualCamera VirtualCamera;

    protected bool IsEnableInput { get; private set; }

    protected bool IsReseting { get; private set; }

    /// <summary> Editor 용. 마우스 X 축 입력 </summary>
    private const string InputXKey = "Mouse X";
    /// <summary> Editor 용. 마우스 Y 축 입력 </summary>
    private const string InputYKey = "Mouse Y";

    private float mInputX;
    private float mInputY;

    protected float InputX { get { return mInputX * Sensitivity; } }
    protected float InputY { get { return mInputY * Sensitivity; } }

    private int mTouchId = 0;

    private Tweener mTweenerRot = null;
    private Tweener mTweenerPos = null;

    protected float AngleX = 0f;
    protected float AngleY = 0f;

    protected Vector3 DefaultPos = Vector3.zero;
    protected Quaternion DefaultAngle = Quaternion.identity;

    protected float Distance = 0f;


    private void Awake()
    {
        VirtualCamera = GetComponent<CinemachineVirtualCamera>();
    }

    private void Start()
    {
        AngleX = 0;
        AngleY = 0;

        DefaultPos = VirtualCamera.transform.localPosition;
        DefaultAngle = VirtualCamera.transform.localRotation;        

        StartImpl();
    }

    protected virtual void StartImpl() { }

    private void OnEnable()
    {
        ZMonoManager.Instance.AddUpdateCall(ZMonoManager.UpdateMode.LateUpdate, UpdateCamera);
    }

    private void OnDisable()
    {
        if(ZMonoManager.hasInstance)
            ZMonoManager.Instance.RemoveUpdateCall(ZMonoManager.UpdateMode.LateUpdate, UpdateCamera);
    }

    private void UpdateCamera()
    {
        if (true == IsReseting)
            return;

        if (false == CameraManager.hasInstance)
            return;

        if (CameraManager.Instance.Brain.ActiveVirtualCamera as CinemachineVirtualCamera != VirtualCamera)
            return;

        //input 처리
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {           
            // 모든 터치에 대해서 검사해줘야한다.
            if (Input.touchCount > 0)
            {    //터치가 1개 이상이면.
                for (int i = 0; i < Input.touchCount; i++)
                {
                    var tempTouch = Input.GetTouch(i);
                    if (tempTouch.phase == TouchPhase.Began)
                    {
                        // 첫 인풋시작이 UI인풋이라면 무시
                        if (UIHelper.IsPointerOverGameObject(ref tempTouch))
                            continue;

                        mTouchId = tempTouch.fingerId;
                    }
                }
            }

            UpdateMobileInput();
        }
        else
        {
            // 첫 인풋시작이 UI인풋이라면 무시
            if (UIHelper.IsPointerOverGameObject() && !IsEnableInput)
                return;

            UpdateInput();
        }

        UpdateCameraImpl();
    }
    
    private void UpdateInput()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {            
            BeginCameraControl();
        }
        if (Input.GetKey(KeyCode.Mouse1))
        {
            KillTweener();

            mInputX = Input.GetAxis(InputXKey);
            mInputY = Input.GetAxis(InputYKey);
        }
        else if(Input.GetKeyUp(KeyCode.Mouse1))
        {
            EndCameraControl();
        }
    }

    private void UpdateMobileInput()
    {
        bool bIncludeId = false;
        foreach (var touch in Input.touches)
        {
            if (mTouchId != touch.fingerId)
                continue;

            float moveDelta = Vector2.SqrMagnitude(touch.deltaPosition);
            if (false == moveDelta > 100f)
                break;

            bIncludeId = true;

            BeginCameraControl();

            mInputX = touch.deltaPosition.x;
            mInputY = touch.deltaPosition.y;
        }

        if(false == bIncludeId)
        {
            EndCameraControl();
        }
        else
        {
            KillTweener();
        }
    }

    private void BeginCameraControl()
    {        
        IsEnableInput = true;
        IsReseting = false;
        CancelInvoke(nameof(ResetCamera));

        KillTweener();
    }

    private void EndCameraControl()
    {
        mTouchId = -1;

        mInputX = 0;
        mInputY = 0;

        if (true == IsEnableInput)
        {
            Invoke(nameof(ResetCamera), ResetDelayTime);
        }
            

        IsEnableInput = false;
    }

    private void ResetCamera()
    {        
        CancelInvoke(nameof(ResetCamera));

        IsReseting = true;
        KillTweener();

        mTweenerRot = ResetCameraRotImpl(ResetDuration);
        mTweenerPos = ResetCameraPosImpl(ResetDuration);

        mTweenerRot.OnComplete(()=>
        {
            IsReseting = false;
            OnFinishResetCamera();
        });
    }

    private void KillTweener()
    {
        if (null != mTweenerPos)
            mTweenerPos.Kill(false);

        if (null != mTweenerRot)
            mTweenerRot.Kill(false);

        mTweenerPos = null;
        mTweenerRot = null;
    }

    protected virtual void OnFinishResetCamera()
    {
        AngleX = 0f;
        AngleY = 0f;
    }

    protected abstract void UpdateCameraImpl();

    protected abstract Tweener ResetCameraPosImpl(float duration);
    protected abstract Tweener ResetCameraRotImpl(float duration);
}