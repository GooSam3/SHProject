using Cinemachine;
using System.Collections.Generic;
using UnityEngine;
public class CameraController : MonoBehaviour
{
    /// <summary> 현재 카메라 모터 </summary>
    public CameraMotorBase CurrentMotor { get; private set; }

    /// <summary> 카메라를 관리하는 클래스 </summary>
    public CameraManager Manager { get; private set; }

    #region Moters
    /// <summary> 최초 아무짓도 안 하는 모터 </summary>
    [SerializeField]
    private CameraMotorBase EmptyMotor = null;
    /// <summary> 자유롭게 회전되는 모터 </summary>
    [SerializeField]
    private CameraMotorBase FreeMotor = null;
    /// <summary> 고정된 각도로 따라다니는 모터 </summary>
    [SerializeField]
    private CameraMotorBase TopMotor = null;
    /// <summary> 고정된 각도로 따라다니는 모터 </summary>
    [SerializeField]
    private CameraMotorBase QuarterMotor = null;
    /// <summary> 숄더 뷰, 캐릭터가 이동하는 방향으로 자연스럽게 카메라도 회전 </summary>
    [SerializeField]
    private CameraMotorBase ShoulderMotor = null;
    /// <summary> 타겟이 있으면 내 캐릭터 뒤에서 타겟을 바라본다. 없으면 셋팅된 다른 타입의 motor로 동작? </summary>
    [SerializeField]
    private CameraMotorBase LookTargetMotor = null;
    /// <summary> 1인칭 카메라</summary>
    [SerializeField]
    private CameraMotorBase PovMotor = null;
    /// <summary> 방향 카메라 (북)</summary>
    [SerializeField]
    private CameraMotorBase NorthMotor = null;
    /// <summary> 방향 카메라 (동)</summary>
    [SerializeField]
    private CameraMotorBase EastMotor = null;
    /// <summary> 방향 카메라 (남)</summary>
    [SerializeField]
    private CameraMotorBase SouthMotor = null;
    /// <summary> 방향 카메라 (서)</summary>
    [SerializeField]
    private CameraMotorBase WestMotor = null;
    /// <summary> 모드연출용 </summary>
    [SerializeField]
    private CameraMotorBase ModeDirectorMotor = null;
    /// <summary> 백뷰 카메라 </summary>
    [SerializeField]
    private CameraMotorBase BackMotor = null;

    /// <summary> 모터들 </summary>
    private Dictionary<E_CameraMotorType, CameraMotorBase> m_dicMotor = new Dictionary<E_CameraMotorType, CameraMotorBase>();
    #endregion
    
    #region Input
    /// <summary> Editor 용. 마우스 휠 입력 </summary>
    private const string InputMouseScrollWheelKey = "Mouse ScrollWheel";
    /// <summary> Editor 용. 마우스 X 축 입력 </summary>
    private const string InputXKey = "Mouse X";
    /// <summary> Editor 용. 마우스 Y 축 입력 </summary>
    private const string InputYKey = "Mouse Y";

    /// <summary> 현재 입력되어 있는 X축 인풋 </summary>
    public float InputX { get; private set; }
    /// <summary> 현재 입력되어 있는 Y축 인풋 </summary>
    public float InputY { get; private set; }
    /// <summary> 현재 입력되어 있는 휠 </summary>
    public float InputZoom { get; private set; }

    public bool IsEnableInput { get; private set; }

#if UNITY_EDITOR
    private float preMouse_x = 0;
    private float preMouse_y = 0;
#endif
    #endregion

    /// <summary> 컨트롤러 초기화 </summary>
    public void DoInitialize(CameraManager manager)
    {
        Manager = manager;

        m_dicMotor.Clear();
        m_dicMotor.Add(E_CameraMotorType.Empty, EmptyMotor.DoInitialize(this, E_CameraMotorType.Empty));
        m_dicMotor.Add(E_CameraMotorType.Free, FreeMotor.DoInitialize(this, E_CameraMotorType.Free));
        m_dicMotor.Add(E_CameraMotorType.Top, TopMotor.DoInitialize(this, E_CameraMotorType.Top));
        m_dicMotor.Add(E_CameraMotorType.Quarter, QuarterMotor.DoInitialize(this, E_CameraMotorType.Quarter));
        m_dicMotor.Add(E_CameraMotorType.Shoulder, ShoulderMotor.DoInitialize(this, E_CameraMotorType.Shoulder));        
        m_dicMotor.Add(E_CameraMotorType.LookTarget, LookTargetMotor.DoInitialize(this, E_CameraMotorType.LookTarget));
        m_dicMotor.Add(E_CameraMotorType.Pov, PovMotor.DoInitialize(this, E_CameraMotorType.Pov));
        m_dicMotor.Add(E_CameraMotorType.North, NorthMotor.DoInitialize( this, E_CameraMotorType.North ) );
        m_dicMotor.Add(E_CameraMotorType.East, EastMotor.DoInitialize( this, E_CameraMotorType.East ) );
        m_dicMotor.Add(E_CameraMotorType.South, SouthMotor.DoInitialize( this, E_CameraMotorType.South ) );
        m_dicMotor.Add(E_CameraMotorType.West, WestMotor.DoInitialize( this, E_CameraMotorType.West ) );
        m_dicMotor.Add(E_CameraMotorType.ModeDirector, ModeDirectorMotor.DoInitialize( this, E_CameraMotorType.ModeDirector ) );
        m_dicMotor.Add(E_CameraMotorType.Back, BackMotor.DoInitialize(this, E_CameraMotorType.Back));//미구현

        //최초 카메라 모터 셋팅
        ChangeMotor(E_CameraMotorType.Empty);
        
        //이벤트 등록
        Manager.DoAddEventChangeTarget(HandleChangedTarget);
        Manager.DoAddEventCameraActivated(HandleCameraActivated);
    }
    
    /// <summary> 모터 변경 </summary>
    public void DoChangeMotor(E_CameraMotorType motor)
    {
        if(false == m_dicMotor.ContainsKey(motor))
        {
            ZLog.Log(ZLogChannel.Camera, ZLogLevel.Error, $"DoChangeMode :: {motor}가 없다.");
            return;
        }

        ZLog.Log(ZLogChannel.Camera, ZLogLevel.Info, $"DoChangeMode :: {motor}로 변경됨");
        ChangeMotor(motor);
    }

    /// <summary> 모터 변경 </summary>
    private void ChangeMotor(E_CameraMotorType motor)
    {
        if (null != CurrentMotor)
        {
            CurrentMotor.DoEndMotor();
        }

        CurrentMotor = m_dicMotor[motor];
        CurrentMotor.DoBeginMotor();
    }
	
	/// <summary> 관리되는 Motor들 모두의 FarClipPlane 수정 </summary>
	public void ChangeFarClipPlane(float _farClipPlane)
	{
		foreach (var motor in m_dicMotor.Values)
		{
			if (null == motor || null == motor.VirtualCamera)
				continue;

			switch (motor.VirtualCamera)
			{
				case CinemachineVirtualCamera vcam:
					{
						vcam.m_Lens.FarClipPlane = _farClipPlane;
					}
					break;

				case CinemachineFreeLook freeLook:
					{
						freeLook.m_Lens.FarClipPlane = _farClipPlane;
					}
					break;

				default:
					{
						ZLog.LogWarn(ZLogChannel.Camera, $"{nameof(ChangeFarClipPlane)}() | 처리 안된 Motor : {motor.MotorType}", motor);
					}
					break;
			}
		}
	}

    public void DoUpdate()
    {
        if(null == CurrentMotor)
        {
            ZLog.Log(ZLogChannel.Camera, ZLogLevel.Error, $"DoUpdate :: CurrentMotor가 셋팅되지 않았다.");
            return;
        }

        UpdateInput();

        CurrentMotor.DoUpdateMotor();
        
        DoUpdateMobileDrag(Vector2.zero, false);

        //TODO :: 카메라 거리에 따른 그림자 셋팅 변경?
    }

    /// <summary> 타겟이 변경되었을 경우 </summary>
    private void HandleChangedTarget(Transform target)
    {
        CurrentMotor.DoChangedTarget();
    }

    /// <summary> live 상태가 변경될 때 </summary>
    private void HandleCameraActivated(ICinemachineCamera activated, ICinemachineCamera deactivated)
    {
        // 현재 모터에 대해서만 처리한다. 나머진 무시
        if (CurrentMotor.VirtualCamera == activated as CinemachineVirtualCameraBase)
        {
            CurrentMotor.DoUpdateCameraActivated(true);
        }
        else if(CurrentMotor.VirtualCamera == deactivated as CinemachineVirtualCameraBase)
        {
            CurrentMotor.DoUpdateCameraActivated(false);
        }
    }

    /// <summary> 에디터용 Input 관련 처리 </summary>
    private void UpdateInput()
    {
#if UNITY_EDITOR
        InputZoom = Input.GetAxis(InputMouseScrollWheelKey);

        //일단 에디터용만
        if (Input.GetKey(KeyCode.Mouse1))
        {
            IsEnableInput = true;
            InputX = Input.GetAxis(InputXKey);
            InputY = Input.GetAxis(InputYKey);

            // 원격에서 에디터 마우스 인펏이 안먹어서 마우스 움직임 수동으로 구현
            ///////////////////////////////////////////////////////////////////////
            float newMouse_x = 0;
            float newMouse_y = 0;
            if( preMouse_x != 0 ) newMouse_x = ( Input.mousePosition.x * 0.05f ) - preMouse_x;
            if( newMouse_x < 0.05f && newMouse_x > -0.05f ) newMouse_x = 0;
            preMouse_x = Input.mousePosition.x * 0.05f;

            if( InputX == 0 && newMouse_x != 0 ) {
                if( newMouse_x > 5f ) newMouse_x = 5f;
                if( newMouse_x < -5f ) newMouse_x = -5f;
                InputX = newMouse_x;
            }

            if( preMouse_y != 0 ) newMouse_y = ( Input.mousePosition.y * 0.05f ) - preMouse_y;
            if( newMouse_y < 0.05 && newMouse_y > -0.05f ) newMouse_y = 0;
            preMouse_y = Input.mousePosition.y * 0.05f;

            if( InputY == 0 && newMouse_y != 0 ) {
                if( newMouse_y > 5f ) newMouse_y = 5f;
                if( newMouse_y < -5f ) newMouse_y = -5f;
                InputY = newMouse_y;
            }
            ///////////////////////////////////////////////////////////////////////
        }
        else
        {
            IsEnableInput = false;
            InputX = 0f;
            InputY = 0f;

            preMouse_x = 0;
            preMouse_y = 0;
        }

        float input_x = 0f;
        float input_y = 0f;

        if (Input.GetKey(KeyCode.Home))
        {
            input_y += 1f;
        }

        if (Input.GetKey(KeyCode.End))
        {
            input_y -= 1f;
        }

        if (Input.GetKey(KeyCode.Delete))
        {
            input_x -= 1f;
        }
        if (Input.GetKey(KeyCode.PageDown))
        {
            input_x += 1f;
        }
        if (Input.GetKey(KeyCode.Insert))
        {
            InputZoom -= 0.1f;
        }
        if (Input.GetKey(KeyCode.PageUp))
        {
            InputZoom += 0.1f;
        }

        if (0 != input_x)
        {
            IsEnableInput = true;
            InputX = input_x;
        }

        if (0 != input_y)
        {
            IsEnableInput = true;
            InputY = input_y;
        }
#endif
    }
   

    #region Mobile Input
    /// <summary> 모바일 드래그 처리 </summary>
    public void DoUpdateMobileDrag(Vector2 offset, bool bEnable, float offsetFactor = 0.2f)
    {
        IsEnableInput = bEnable;
        offset *= offsetFactor;
        InputX = offset.x;
        InputY = offset.y;
        DoUpdateMobileZoom(0);
    }

    /// <summary> 모바이 줌 처리 </summary>
    public void DoUpdateMobileZoom(float value)
    {
        InputZoom = value;
    }
    #endregion
}
