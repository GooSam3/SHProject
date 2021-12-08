using Cinemachine;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CameraManager : Zero.Singleton<CameraManager>
{
	/// <summary> ī�޶� ��� ����� �˸� </summary>
	private Action<E_CameraMotorType> mEventChangeCameraMotor;

	/// <summary> ī�޶��� Ÿ���� ����Ǿ��� ��� �˸� </summary>
	private Action<Transform> mEventChangeTarget;

	/// <summary> ī�޶� ������Ʈ ���� ȣ�� </summary>
	private Action mEventCameraUpdated;

	/// <summary> �ó׸ӽ� ���߾� ī�޶��� live ���°� ����ɶ� </summary>
	private Action<ICinemachineCamera, ICinemachineCamera> mEventVirtualCameraActiveed;

	/// <summary> ���� ī�޶� </summary>
	[SerializeField] private Camera mMain = null;

	/// <summary> ���� ī�޶� </summary>
	public Camera Main { get { return mMain; } }

	/// <summary> PP ī�޶� </summary>
	[SerializeField] private Camera mPP = null;

	/// <summary> PP ī�޶� </summary>
	public Camera PP { get { return mPP; } }

	/// <summary> �ó׸ӽ� �극�� </summary>
	[SerializeField] private CinemachineBrain mBrain = null;

	/// <summary> �ó׸ӽ� �극�� </summary>
	public CinemachineBrain Brain { get { return mBrain; } }

	/// <summary> Camera Controller. Motor ���� </summary>
	[SerializeField] private CameraController Controller = null;

	/// <summary> ���� �۵����� ���� </summary>
	public CameraMotorBase CurrentMotor { get { return Controller.CurrentMotor; } }

	/// <summary> ���� ī�޶� ��� </summary>
	public E_CameraMotorType CurrentMotorType { get { return Controller.CurrentMotor.MotorType; } }

	/// <summary> ī�޶� ������Ʈ ��� </summary>
	[SerializeField] private E_CameraUpdateType UpdateType = E_CameraUpdateType.LateUpdate;

	/// <summary> PostProcess Controller </summary>
	[SerializeField] private PostProcessController PPController = null;

	/// <summary> ���� ī�޶��� Ÿ�� </summary>
	public Transform Target { get; private set; }

	private E_RenderType RenderType = E_RenderType.InGameDefault;

	/// <summary> ī�޶���ũ </summary>
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
	/// <summary> �ʱ�ȭ </summary>
	private void Initialize()
	{
		DoResolveMainCamera();

		InitializeController();

		//�̺�Ʈ ���
		CinemachineCore.CameraUpdatedEvent.AddListener( HandleCameraUpdate );
		Brain.m_CameraActivatedEvent.AddListener( HandleCameraActivated );
	}

	/// <summary> MainCamera�� ���������, ���� ������ ī�޶� ����� ���� </summary>
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

	/// <summary> ī�޶� ��Ʈ�ѷ� �ʱ�ȭ </summary>
	private void InitializeController()
	{
		Controller.DoInitialize( this );
	}
	#endregion

	#region ::======== Use APIs ========::
	/// <summary> ī�޶��� Ÿ���� �����Ѵ�. </summary>
	public void DoSetTarget( Transform target )
	{
		Target = target;
		mEventChangeTarget?.Invoke( target );
	}

	/// <summary> ī�޶� �׸��⸸ ���� </summary>
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

	/// <summary> ī�޶� Culling Mask ���� </summary>
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

	/// <summary> ī�޶� Culling Mask ���� </summary>
	public void DoSetCullingMask( int mask )
	{
		Main.cullingMask = mask;
	}
	#endregion

	#region ::======== Controller ========::
	/// <summary> ī�޶� ���͸� �����Ѵ�. </summary>
	public void DoChangeCameraMotor( 
		E_CameraMotorType motor, CinemachineBlendDefinition.Style blendStyle = CinemachineBlendDefinition.Style.EaseIn, float blendDuration = 0.5f, bool bDontSave = false )
	{
		DoSetBrainBlendStyle( blendStyle, blendDuration );

		Controller.DoChangeMotor( motor );

		mEventChangeCameraMotor?.Invoke( motor );
	}

	/// <summary> lookat motor�� �����Ѵ�. </summary>
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

	/// <summary> ����� ���ͷ� �����Ѵ�. </summary>
	public void DoResetMotor(CinemachineBlendDefinition.Style blendStyle = CinemachineBlendDefinition.Style.EaseIn, float blendDuration = 0.5f)
	{
		DoSetTarget(ZPawnManager.Instance.MyEntity?.transform);

		var motor = LoadCameraMotorType();
		DoChangeCameraMotor(motor, blendStyle, blendDuration);
	}

	public void SaveCameraMotorType()
	{
		//���̺����� ������� Ÿ�� �߰�
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

	/// <summary> ī�޶� FarClip ���� ���� </summary>
	public void ChangeFarClipPlane( float _farClipPlane )
	{
		_farClipPlane = Mathf.Clamp( _farClipPlane, 0.1f, float.MaxValue );

		Controller.ChangeFarClipPlane( _farClipPlane );
	}

	#endregion

	#region ::======== Brain ========::
	/// <summary> �ó׸ӽ� �⺻ ���� Ÿ�� ���� </summary>
	public void DoSetBrainBlendStyle( CinemachineBlendDefinition.Style style, float blendDuration = 0.5f )
	{
		if( null == mBrain ) {

			ZLog.Log( ZLogChannel.Camera, ZLogLevel.Error, "DoSetBrainBlendStyle :: CinemachineBrain�� ���õ��� ����!" );
			return;
		}

		mBrain.m_DefaultBlend.m_Style = style;
		mBrain.m_DefaultBlend.m_Time = blendDuration;
	}

	/// <summary> ī�޶� ������Ʈ ���� ȣ��� </summary>
	private void HandleCameraUpdate( CinemachineBrain brain )
	{
		if( Brain != brain ) {
			return;
		}

		mEventCameraUpdated?.Invoke();
	}

	/// <summary> virtual camera live ���� ����� ȣ��� </summary>
	private void HandleCameraActivated( ICinemachineCamera activated, ICinemachineCamera deactivated )
	{
		mEventVirtualCameraActiveed?.Invoke( activated, deactivated );
	}

	#endregion

	#region ::======== Delegate ========::
	/// <summary> ī�޶� ������Ʈ(Brain) �˸� �߰� </summary>
	public void DoAddEventCameraUpdated( Action action )
	{
		DoRemoveEventCameraUpdated( action );
		mEventCameraUpdated += action;
	}

	/// <summary> ī�޶� ������Ʈ(Brain) �˸� ���� </summary>
	public void DoRemoveEventCameraUpdated( Action action )
	{
		mEventCameraUpdated -= action;
	}

	/// <summary> �ó׸ӽ� ���߾� ī�޶� Ȱ��ȭ/��Ȱ��ȭ �˸� �߰� </summary>
	public void DoAddEventCameraActivated( Action<ICinemachineCamera, ICinemachineCamera> action )
	{
		DoRemoveEventCameraActivated( action );
		mEventVirtualCameraActiveed += action;
	}

	/// <summary> �ó׸ӽ� ���߾� ī�޶� Ȱ��ȭ/��Ȱ��ȭ �˸� ���� </summary>
	public void DoRemoveEventCameraActivated( Action<ICinemachineCamera, ICinemachineCamera> action )
	{
		mEventVirtualCameraActiveed -= action;
	}

	/// <summary> ī�޶� ��� ����� �˸� �߰� </summary>
	public void DoAddEventChangeCameraMotor( Action<E_CameraMotorType> action )
	{
		DoRemoveEventChangeCameraMotor( action );
		mEventChangeCameraMotor += action;
	}

	/// <summary> ī�޶� ��� ����� �˸� ���� </summary>
	public void DoRemoveEventChangeCameraMotor( Action<E_CameraMotorType> action )
	{
		mEventChangeCameraMotor -= action;
	}

	/// <summary> ī�޶� Ÿ�� ����� �˸� �߰� </summary>
	public void DoAddEventChangeTarget( Action<Transform> action )
	{
		DoRemoveEventChangeTarget( action );
		mEventChangeTarget += action;
	}

	/// <summary> ī�޶� Ÿ�� ����� �˸� ���� </summary>
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

	/// <summary> PostProcess Layer �� Volume�� Ȱ��ȭ/��Ȱ��ȭ �Ѵ�. </summary>
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

		//ZLog.Log( ZLogChannel.Camera, ZLogLevel.Info, $"ī�޶���ũ, �ð�{time},����{amplitude}" );
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
	/// <summary> ����� �Է� ó�� </summary>
	public void DoUpdateMobileDrag( Vector2 offset, bool bEnable, float offsetFactor = 0.2f )
	{
		Controller.DoUpdateMobileDrag( offset, bEnable, offsetFactor );
	}

	/// <summary> ����� �� ó�� </summary>
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