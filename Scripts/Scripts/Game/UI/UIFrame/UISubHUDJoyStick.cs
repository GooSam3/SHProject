using CoolJoystick;
using UnityEngine;

public class UISubHUDJoyStick : ZUIFrameBase
{
    [SerializeField] private Joystick Joystick;            // Reference to Joystick
    [SerializeField] private CanvasGroup AlphaCanvas;

    //Transform TargetTrans;
    //public Action<Vector3> OnJoystickMove;
    //public Action OnJoystickEnd;
    
    private Vector2 _lastAxis;
	private bool isEnded;

    private EntityBase MyEntity = null;
    protected override void OnUnityAwake()
    {
        base.OnUnityAwake();
    } 
    private void Start()
    {
        MyEntity = ZPawnManager.Instance.MyEntity;

        CameraManager.Instance.DoAddEventCameraUpdated(UpdateJoyStick);
        ZPawnManager.Instance.DoAddEventCreateEntity(HandleCreateEntity);
        ZPawnManager.Instance.DoAddEventRemoveEntity(HandleRemoveEntity);
        ZGameOption.Instance.OnOptionChanged += UpdateOptionChanged;

        UpdateUseJoyStick();
    }

	protected override void OnHide()
	{
        Joystick.OnPointerUp(null);
    }

	private void HandleCreateEntity(uint entityId, EntityBase entity)
    {
        if(null == entity || false == entity.IsMyPc)
        {
            return;
        }

        MyEntity = entity;
    }

    private void HandleRemoveEntity(uint entityid)
    {
        if(null == MyEntity || MyEntity.EntityId != entityid)
        {
            return;
        }

        MyEntity = null;
    }

    private void OnDestroy()
    {
        ZGameOption.Instance.OnOptionChanged -= UpdateOptionChanged;
    }

    void UpdateOptionChanged(ZGameOption.OptionKey key)
    {
        if (key == ZGameOption.OptionKey.Option_Use_VirtualPad)
            UpdateUseJoyStick();
    }

    void UpdateUseJoyStick()
    {
        AlphaCanvas.alpha = ZGameOption.Instance.bUseVirtualPad ? 1.0f : 0f;

        //Joystick.gameObject.SetActive(ZGameOption.Instance.bUseVirtualPad);
	}

 //   public void SetTargetTrans(Transform _TargetTrans, System.Action<Vector3> _OnJoystickMove, System.Action _joystickEnd = null)
 //   {
 //       TargetTrans = _TargetTrans;
 //       OnJoystickMove = _OnJoystickMove;
	//	OnJoystickEnd = _joystickEnd;
	//}

    private void UpdateJoyStick()
    {
        if (null == MyEntity)
            return;

        if (!ZGameOption.Instance.bUseVirtualPad)
            return;

		if (Joystick.Pressed && Vector2.zero != Joystick.Direction) // 조이스틱 값 변화시
		{
			isEnded = false;

			float angle = VectorHelper.Axis2Angle(Joystick.Direction, true);

            if (CameraManager.Instance.Main != null)
            {

                Quaternion rot = Quaternion.Euler(
                    0,
                    CameraManager.Instance.Main.transform.rotation.eulerAngles.y + angle,
                    0);

                // 이동할 방향 구하기
                Vector3 newDir = rot * Vector3.forward;
                MyEntity.MoveToDirection(MyEntity.transform.position, newDir, MyEntity.MoveSpeed, Joystick.Direction);
            }
		}
		else
		{
			if (!isEnded && _lastAxis == Vector2.zero)
			{
				isEnded = true;

                MyEntity.StopMove(MyEntity.transform.position);
            }
		}

		_lastAxis = Joystick.Direction;
	}

	private void OnApplicationFocus(bool focus)
	{
		// 조이스틱 리셋
		if (focus && null != Joystick)
		{
			//Joystick.OnPointerUp(null);
		}
	}

	private void OnApplicationPause(bool pause)
	{
		// 조이스틱 리셋
		if (!pause && null != Joystick)
		{
			//Joystick.OnPointerUp(null);
		}
	}
#if UNITY_EDITOR
    private bool bKeyMove = false;
    private void Update()
    {
        Vector2 dir = new Vector2();
        dir.x = Input.GetAxisRaw("Horizontal");
        dir.y = Input.GetAxisRaw("Vertical");

        if (dir != Vector2.zero)
        {
            if (false == bKeyMove)
            {
                bKeyMove = true;
                Joystick.Pressed = true;
                Joystick.enabled = false;
            }
            Joystick.Direction = dir;
        }
        else if (bKeyMove)
        {
            bKeyMove = false;
            Joystick.Pressed = false;
            Joystick.enabled = true;
            Joystick.Direction = Vector2.zero;
        }
    }
#endif
}
