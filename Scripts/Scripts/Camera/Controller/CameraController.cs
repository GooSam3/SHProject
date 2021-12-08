using Cinemachine;
using System.Collections.Generic;
using UnityEngine;
public class CameraController : MonoBehaviour
{
    /// <summary> ���� ī�޶� ���� </summary>
    public CameraMotorBase CurrentMotor { get; private set; }

    /// <summary> ī�޶� �����ϴ� Ŭ���� </summary>
    public CameraManager Manager { get; private set; }

    #region Moters
    /// <summary> ���� �ƹ����� �� �ϴ� ���� </summary>
    [SerializeField]
    private CameraMotorBase EmptyMotor = null;
    /// <summary> �����Ӱ� ȸ���Ǵ� ���� </summary>
    [SerializeField]
    private CameraMotorBase FreeMotor = null;
    /// <summary> ������ ������ ����ٴϴ� ���� </summary>
    [SerializeField]
    private CameraMotorBase TopMotor = null;
    /// <summary> ������ ������ ����ٴϴ� ���� </summary>
    [SerializeField]
    private CameraMotorBase QuarterMotor = null;
    /// <summary> ��� ��, ĳ���Ͱ� �̵��ϴ� �������� �ڿ������� ī�޶� ȸ�� </summary>
    [SerializeField]
    private CameraMotorBase ShoulderMotor = null;
    /// <summary> Ÿ���� ������ �� ĳ���� �ڿ��� Ÿ���� �ٶ󺻴�. ������ ���õ� �ٸ� Ÿ���� motor�� ����? </summary>
    [SerializeField]
    private CameraMotorBase LookTargetMotor = null;
    /// <summary> 1��Ī ī�޶�</summary>
    [SerializeField]
    private CameraMotorBase PovMotor = null;
    /// <summary> ���� ī�޶� (��)</summary>
    [SerializeField]
    private CameraMotorBase NorthMotor = null;
    /// <summary> ���� ī�޶� (��)</summary>
    [SerializeField]
    private CameraMotorBase EastMotor = null;
    /// <summary> ���� ī�޶� (��)</summary>
    [SerializeField]
    private CameraMotorBase SouthMotor = null;
    /// <summary> ���� ī�޶� (��)</summary>
    [SerializeField]
    private CameraMotorBase WestMotor = null;
    /// <summary> ��忬��� </summary>
    [SerializeField]
    private CameraMotorBase ModeDirectorMotor = null;
    /// <summary> ��� ī�޶� </summary>
    [SerializeField]
    private CameraMotorBase BackMotor = null;

    /// <summary> ���͵� </summary>
    private Dictionary<E_CameraMotorType, CameraMotorBase> m_dicMotor = new Dictionary<E_CameraMotorType, CameraMotorBase>();
    #endregion
    
    #region Input
    /// <summary> Editor ��. ���콺 �� �Է� </summary>
    private const string InputMouseScrollWheelKey = "Mouse ScrollWheel";
    /// <summary> Editor ��. ���콺 X �� �Է� </summary>
    private const string InputXKey = "Mouse X";
    /// <summary> Editor ��. ���콺 Y �� �Է� </summary>
    private const string InputYKey = "Mouse Y";

    /// <summary> ���� �ԷµǾ� �ִ� X�� ��ǲ </summary>
    public float InputX { get; private set; }
    /// <summary> ���� �ԷµǾ� �ִ� Y�� ��ǲ </summary>
    public float InputY { get; private set; }
    /// <summary> ���� �ԷµǾ� �ִ� �� </summary>
    public float InputZoom { get; private set; }

    public bool IsEnableInput { get; private set; }

#if UNITY_EDITOR
    private float preMouse_x = 0;
    private float preMouse_y = 0;
#endif
    #endregion

    /// <summary> ��Ʈ�ѷ� �ʱ�ȭ </summary>
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
        m_dicMotor.Add(E_CameraMotorType.Back, BackMotor.DoInitialize(this, E_CameraMotorType.Back));//�̱���

        //���� ī�޶� ���� ����
        ChangeMotor(E_CameraMotorType.Empty);
        
        //�̺�Ʈ ���
        Manager.DoAddEventChangeTarget(HandleChangedTarget);
        Manager.DoAddEventCameraActivated(HandleCameraActivated);
    }
    
    /// <summary> ���� ���� </summary>
    public void DoChangeMotor(E_CameraMotorType motor)
    {
        if(false == m_dicMotor.ContainsKey(motor))
        {
            ZLog.Log(ZLogChannel.Camera, ZLogLevel.Error, $"DoChangeMode :: {motor}�� ����.");
            return;
        }

        ZLog.Log(ZLogChannel.Camera, ZLogLevel.Info, $"DoChangeMode :: {motor}�� �����");
        ChangeMotor(motor);
    }

    /// <summary> ���� ���� </summary>
    private void ChangeMotor(E_CameraMotorType motor)
    {
        if (null != CurrentMotor)
        {
            CurrentMotor.DoEndMotor();
        }

        CurrentMotor = m_dicMotor[motor];
        CurrentMotor.DoBeginMotor();
    }
	
	/// <summary> �����Ǵ� Motor�� ����� FarClipPlane ���� </summary>
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
						ZLog.LogWarn(ZLogChannel.Camera, $"{nameof(ChangeFarClipPlane)}() | ó�� �ȵ� Motor : {motor.MotorType}", motor);
					}
					break;
			}
		}
	}

    public void DoUpdate()
    {
        if(null == CurrentMotor)
        {
            ZLog.Log(ZLogChannel.Camera, ZLogLevel.Error, $"DoUpdate :: CurrentMotor�� ���õ��� �ʾҴ�.");
            return;
        }

        UpdateInput();

        CurrentMotor.DoUpdateMotor();
        
        DoUpdateMobileDrag(Vector2.zero, false);

        //TODO :: ī�޶� �Ÿ��� ���� �׸��� ���� ����?
    }

    /// <summary> Ÿ���� ����Ǿ��� ��� </summary>
    private void HandleChangedTarget(Transform target)
    {
        CurrentMotor.DoChangedTarget();
    }

    /// <summary> live ���°� ����� �� </summary>
    private void HandleCameraActivated(ICinemachineCamera activated, ICinemachineCamera deactivated)
    {
        // ���� ���Ϳ� ���ؼ��� ó���Ѵ�. ������ ����
        if (CurrentMotor.VirtualCamera == activated as CinemachineVirtualCameraBase)
        {
            CurrentMotor.DoUpdateCameraActivated(true);
        }
        else if(CurrentMotor.VirtualCamera == deactivated as CinemachineVirtualCameraBase)
        {
            CurrentMotor.DoUpdateCameraActivated(false);
        }
    }

    /// <summary> �����Ϳ� Input ���� ó�� </summary>
    private void UpdateInput()
    {
#if UNITY_EDITOR
        InputZoom = Input.GetAxis(InputMouseScrollWheelKey);

        //�ϴ� �����Ϳ븸
        if (Input.GetKey(KeyCode.Mouse1))
        {
            IsEnableInput = true;
            InputX = Input.GetAxis(InputXKey);
            InputY = Input.GetAxis(InputYKey);

            // ���ݿ��� ������ ���콺 ������ �ȸԾ ���콺 ������ �������� ����
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
    /// <summary> ����� �巡�� ó�� </summary>
    public void DoUpdateMobileDrag(Vector2 offset, bool bEnable, float offsetFactor = 0.2f)
    {
        IsEnableInput = bEnable;
        offset *= offsetFactor;
        InputX = offset.x;
        InputY = offset.y;
        DoUpdateMobileZoom(0);
    }

    /// <summary> ����� �� ó�� </summary>
    public void DoUpdateMobileZoom(float value)
    {
        InputZoom = value;
    }
    #endregion
}
