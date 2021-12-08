using Cinemachine;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CameraManager : Zero.Singleton<CameraManager>
{
	/// <summary> 카메라 모드 변경시 알림 </summary>
	private Action<E_CameraMotorType> mEventChangeCameraMotor;

	/// <summary> 카메라의 타겟이 변경되었을 경우 알림 </summary>
	private Action<Transform> mEventChangeTarget;

	/// <summary> 카메라 업데이트 이후 호출 </summary>
	private Action mEventCameraUpdated;

	/// <summary> 시네머신 버추얼 카메라의 live 상태가 변경될때 </summary>
	private Action<ICinemachineCamera, ICinemachineCamera> mEventVirtualCameraActiveed;

	/// <summary> 메인 카메라 </summary>
	[SerializeField] private Camera mMain = null;

	/// <summary> 메인 카메라 </summary>
	public Camera Main { get { return mMain; } }

	/// <summary> PP 카메라 </summary>
	[SerializeField] private Camera mPP = null;

	/// <summary> PP 카메라 </summary>
	public Camera PP { get { return mPP; } }

	/// <summary> 시네머신 브레인 </summary>
	[SerializeField] private CinemachineBrain mBrain = null;

	/// <summary> 시네머신 브레인 </summary>
	public CinemachineBrain Brain { get { return mBrain; } }

	/// <summary> Camera Controller. Motor 관리 </summary>
	[SerializeField] private CameraController Controller = null;

	/// <summary> 현재 작동중인 모터 </summary>
	public CameraMotorBase CurrentMotor { get { return Controller.CurrentMotor; } }

	/// <summary> 현재 카메라 모드 </summary>
	public E_CameraMotorType CurrentMotorType { get { return Controller.CurrentMotor.MotorType; } }

	/// <summary> 카메라 업데이트 모드 </summary>
	[SerializeField] private E_CameraUpdateType UpdateType = E_CameraUpdateType.LateUpdate;

	/// <summary> PostProcess Controller </summary>
	[SerializeField] private PostProcessController PPController = null;

	/// <summary> 현재 카메라의 타겟 </summary>
	public Transform Target { get; private set; }

	private E_RenderType RenderType = E_RenderType.InGameDefault;

	/// <summary> 카메라쉐이크 </summary>
	[SerializeField] private CinemachineImpulseSource ImpulseSource;

	protected override void Init()
	{
		base.Init();
		Initialize();
	}

	protected void Update()
	{
		if( UpdateType != E_CameraUpdateType.Update ) {
			return;
		}

		Controller.DoUpdate();
	}

	protected void LateUpdate()
	{
		if( UpdateType != E_CameraUpdateType.LateUpdate ) {
			return;
		}

		Controller.DoUpdate();
	}

	protected void FixedUpdate()
	{
		if( UpdateType != E_CameraUpdateType.FixedUpdate ) {
			return;
		}

		Controller.DoUpdate();
	}

	#region ::======== Initialize ========::
	/// <summary> 초기화 </summary>
	private void Initialize()
	{
		DoResolveMainCamera();

		InitializeController();

		//이벤트 등록
		CinemachineCore.CameraUpdatedEvent.AddListener( HandleCameraUpdate );
		Brain.m_CameraActivatedEvent.AddListener( HandleCameraActivated );
	}

	/// <summary> MainCamera가 여러개라면, 현재 설정한 카메라만 남기고 제거 </summary>
	public void DoResolveMainCamera()
	{
		if( null == mMain )
			return;

		for( int i = 0; i < Camera.allCamerasCount; i++ ) {
			var cam = Camera.allCameras[ i ];

			if( mMain.tag == cam.tag &&
				mMain != cam ) {
				Destroy( cam.gameObject );
			}
		}
	}

	/// <summary> 카메라 컨트롤러 초기화 </summary>
	private void InitializeController()
	{
		Controller.DoInitialize( this );
	}
	#endregion

	#region ::======== Use APIs ========::
	/// <summary> 카메라의 타겟을 셋팅한다. </summary>
	public void DoSetTarget( Transform target )
	{
		Target = target;
		mEventChangeTarget?.Invoke( target );
	}

	/// <summary> 카메라 그리기만 끄기 </summary>
	public void DoSetVisible( bool visible )
	{
		if(visible) {
			SetMainCameraRender(RenderType);
		}
		else {
			SetMainCameraRender(E_RenderType.InGameScreenSaver);
		}

		//Main.enabled = visible;
	}

	/// <summary> 카메라 Culling Mask 변경 </summary>
	public void DoSetCullingMaskForUI( bool bOnlyUI )
	{
		if( bOnlyUI ) {
			Main.cullingMask = 0;
		}
		else {
			Main.cullingMask = UnityConstants.Layers.EverythingBut(
				UnityConstants.Layers.UI,				
				UnityConstants.Layers.UIModel);
		}
	}

	/// <summary> 카메라 Culling Mask 변경 </summary>
	public void DoSetCullingMask( int mask )
	{
		Main.cullingMask = mask;
	}
	#endregion

	#region ::======== Controller ========::
	/// <summary> 카메라 모터를 변경한다. </summary>
	public void DoChangeCameraMotor( 
		E_CameraMotorType motor, CinemachineBlendDefinition.Style blendStyle = CinemachineBlendDefinition.Style.EaseIn, float blendDuration = 0.5f, bool bDontSave = false )
	{
		DoSetBrainBlendStyle( blendStyle, blendDuration );

		Controller.DoChangeMotor( motor );

		mEventChangeCameraMotor?.Invoke( motor );
	}

	/// <summary> lookat motor로 셋팅한다. </summary>
	public void DoSetLookAtMotor(Transform target, CinemachineBlendDefinition.Style blendStyle = CinemachineBlendDefinition.Style.EaseIn, float blendDuration = 0.5f)
	{
		DoSetBrainBlendStyle(blendStyle, blendDuration);

		Controller.DoChangeMotor(E_CameraMotorType.LookTarget);

		if(CurrentMotor is CameraMotorLookTarget motor)
		{
			motor.DoChangeLookTarget(target);
		}

		mEventChangeCameraMotor?.Invoke(E_CameraMotorType.LookTarget);
	}

	/// <summary> 저장된 모터로 원복한다. </summary>
	public void DoResetMotor(CinemachineBlendDefinition.Style blendStyle = CinemachineBlendDefinition.Style.EaseIn, float blendDuration = 0.5f)
	{
		DoSetTarget(ZPawnManager.Instance.MyEntity?.transform);

		var motor = LoadCameraMotorType();
		DoChangeCameraMotor(motor, blendStyle, blendDuration);
	}

	public void SaveCameraMotorType()
	{
		//세이브하지 말어야할 타입 추가
		if( CurrentMotorType != E_CameraMotorType.Empty &&
			CurrentMotorType != E_CameraMotorType.ModeDirector ) {
			DeviceSaveDatas.SaveData( DeviceSaveDatas.KEY_CAMERA_MOTOR_TYPE, ( int )CurrentMotorType );
		}
	}

	public E_CameraMotorType LoadCameraMotorType()
	{
		int motorTypeNum = DeviceSaveDatas.LoadData( DeviceSaveDatas.KEY_CAMERA_MOTOR_TYPE, ( int )E_CameraMotorType.Free );
		return ( E_CameraMotorType )motorTypeNum;
	}

	/// <summary> 카메라 FarClip 설정 변경 </summary>
	public void ChangeFarClipPlane( float _farClipPlane )
	{
		_farClipPlane = Mathf.Clamp( _farClipPlane, 0.1f, float.MaxValue );

		Controller.ChangeFarClipPlane( _farClipPlane );
	}

	#endregion

	#region ::======== Brain ========::
	/// <summary> 시네머신 기본 블렌드 타입 셋팅 </summary>
	public void DoSetBrainBlendStyle( CinemachineBlendDefinition.Style style, float blendDuration = 0.5f )
	{
		if( null == mBrain ) {

			ZLog.Log( ZLogChannel.Camera, ZLogLevel.Error, "DoSetBrainBlendStyle :: CinemachineBrain이 셋팅되지 않음!" );
			return;
		}

		mBrain.m_DefaultBlend.m_Style = style;
		mBrain.m_DefaultBlend.m_Time = blendDuration;
	}

	/// <summary> 카메라 업데이트 이후 호출됨 </summary>
	private void HandleCameraUpdate( CinemachineBrain brain )
	{
		if( Brain != brain ) {
			return;
		}

		mEventCameraUpdated?.Invoke();
	}

	/// <summary> virtual camera live 상태 변경시 호출됨 </summary>
	private void HandleCameraActivated( ICinemachineCamera activated, ICinemachineCamera deactivated )
	{
		mEventVirtualCameraActiveed?.Invoke( activated, deactivated );
	}

	#endregion

	#region ::======== Delegate ========::
	/// <summary> 카메라 업데이트(Brain) 알림 추가 </summary>
	public void DoAddEventCameraUpdated( Action action )
	{
		DoRemoveEventCameraUpdated( action );
		mEventCameraUpdated += action;
	}

	/// <summary> 카메라 업데이트(Brain) 알림 제거 </summary>
	public void DoRemoveEventCameraUpdated( Action action )
	{
		mEventCameraUpdated -= action;
	}

	/// <summary> 시네머신 버추얼 카메라 활성화/비활성화 알림 추가 </summary>
	public void DoAddEventCameraActivated( Action<ICinemachineCamera, ICinemachineCamera> action )
	{
		DoRemoveEventCameraActivated( action );
		mEventVirtualCameraActiveed += action;
	}

	/// <summary> 시네머신 버추얼 카메라 활성화/비활성화 알림 제거 </summary>
	public void DoRemoveEventCameraActivated( Action<ICinemachineCamera, ICinemachineCamera> action )
	{
		mEventVirtualCameraActiveed -= action;
	}

	/// <summary> 카메라 모드 변경시 알림 추가 </summary>
	public void DoAddEventChangeCameraMotor( Action<E_CameraMotorType> action )
	{
		DoRemoveEventChangeCameraMotor( action );
		mEventChangeCameraMotor += action;
	}

	/// <summary> 카메라 모드 변경시 알림 제거 </summary>
	public void DoRemoveEventChangeCameraMotor( Action<E_CameraMotorType> action )
	{
		mEventChangeCameraMotor -= action;
	}

	/// <summary> 카메라 타겟 변경시 알림 추가 </summary>
	public void DoAddEventChangeTarget( Action<Transform> action )
	{
		DoRemoveEventChangeTarget( action );
		mEventChangeTarget += action;
	}

	/// <summary> 카메라 타겟 변경시 알림 제거 </summary>
	public void DoRemoveEventChangeTarget( Action<Transform> action )
	{
		mEventChangeTarget -= action;
	}
	#endregion

	#region ::======== Post Process ========::
	public void DoSetupPostProcess( VolumeProfile profile )
	{
		PPController.DoSetupGlobalPostProcessing( profile );
	}

	/// <summary> PostProcess Layer 및 Volume을 활성화/비활성화 한다. </summary>
	public void DoPostProcessTurn( bool bOn )
	{
		PPController.Turn( bOn );
	}
	#endregion

	#region ::======== Shake ========::
	public void DoShake( Vector3 position )
	{
		DoShake( position, Vector3.down );
	}

	public void DoShake( Vector3 position, Vector3 velocity, float amplitude = 1f, float frequency = 1f, float time = 0.2f, float impactRadius = 20f, float dissipationDistance = 50f, bool bForce = false )
	{
		if( null == ImpulseSource )
			return;

		if( !ZGameOption.Instance.bVibration && !bForce )
			return;

		//ZLog.Log( ZLogChannel.Camera, ZLogLevel.Info, $"카메라쉐이크, 시간{time},강도{amplitude}" );
		ImpulseSource.m_ImpulseDefinition.m_AmplitudeGain = amplitude;
		ImpulseSource.m_ImpulseDefinition.m_FrequencyGain = frequency;
		ImpulseSource.m_ImpulseDefinition.m_ImpactRadius = impactRadius;
		ImpulseSource.m_ImpulseDefinition.m_DissipationDistance = dissipationDistance;
		ImpulseSource.m_ImpulseDefinition.m_TimeEnvelope.m_SustainTime = time / 2f;
		ImpulseSource.m_ImpulseDefinition.m_TimeEnvelope.m_DecayTime = time / 2f;
		ImpulseSource.GenerateImpulseAt( position, velocity );
	}
	#endregion

	#region ::======== Mobile Input ========::
	/// <summary> 모바일 입력 처리 </summary>
	public void DoUpdateMobileDrag( Vector2 offset, bool bEnable, float offsetFactor = 0.2f )
	{
		Controller.DoUpdateMobileDrag( offset, bEnable, offsetFactor );
	}

	/// <summary> 모바일 줌 처리 </summary>
	public void DoUpdateMobileZoom( float value )
	{
		Controller.DoUpdateMobileZoom( value );
	}
	#endregion

	//hhj
	public void SetMainCameraRender(E_RenderType _type)
    {
		if(_type != E_RenderType.InGameScreenSaver) RenderType = _type;

		UniversalAdditionalCameraData mainCamaraData = CameraExtensions.GetUniversalAdditionalCameraData(Main);
		UniversalAdditionalCameraData ppCamaraData = CameraExtensions.GetUniversalAdditionalCameraData(PP);
		
		mainCamaraData.SetRenderer((int)_type);
		ppCamaraData.SetRenderer((int)_type);
	}
}