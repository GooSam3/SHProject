using Cinemachine;
using UnityEngine;

public sealed class CameraMotorModeDirector : CameraMotorBase
{
	/// <summary> 카메라 셋팅 데이터 </summary>    
	[SerializeField]
	protected CameraMotorSettingLookTarget Setting;

	/// <summary> 카메라 셋팅 데이터 </summary>    
	public override CameraMotorSettingBase SettingBase { get { return Setting; } }

	/// <summary> LookTarget이 없을 경우 동작하는 모터 </summary>
	[SerializeField]
	private CameraMotorBase DefaultMotor = null;

	/// <summary> 바라볼 타겟 </summary>
	public Transform LookTarget { get; private set; }

	[SerializeField] private CinemachineVirtualCamera cinemaCamera;
	[SerializeField] private bool isFixPos = false;
	[SerializeField] private Vector3 fixCamPosition = new Vector3(0, 2, 0);
	[SerializeField] private float rotationSpeed = 10f;
	[SerializeField] private float zoomSpeed = 0.02f;
	[SerializeField] float maxFov = 28;
	[SerializeField] float minFov = 20;

	float zoomDuration;

	protected override void OnInitializeImpl()
	{
		LookTarget = null;
	}

	protected override void OnBegineMotorImpl()
	{
		//TODO :: 추후 기본 타겟 셋팅하든 하자
		DoChangeLookTarget(LookTarget);
	}

	public void ZoomIn(float duration = 10.0f)
	{
		zoomDuration = duration;
		cinemaCamera.m_Lens.FieldOfView = maxFov;
	}

	protected override void OnUpdateMotorImpl()
	{
		if (null != LookTarget) {
			Vector3 lookTargetPos = LookTarget.position + new Vector3(0f, Setting.LookTargetOffsetY, 0f);
			Vector3 dir = LookTarget.position - Target.position;
			dir.Normalize();

			Quaternion rot = Quaternion.identity;
			if (dir != Vector3.zero) {
				rot = Quaternion.LookRotation(dir);
			}

			if (isFixPos) {
				mCameraTrans.position = fixCamPosition;
			}
			else {
				Vector3 camPos = new Vector3();
				camPos = rot * Setting.TargetOffset + Target.position;
				mCameraTrans.position = camPos;
			}

			var dirTo = (lookTargetPos - mCameraTrans.position).normalized;
			mCameraTrans.rotation = Quaternion.Lerp(mCameraTrans.rotation, Quaternion.LookRotation(dirTo), Time.deltaTime * rotationSpeed);

			if (zoomDuration > 0) {
				zoomDuration -= Time.deltaTime;
				float fov = cinemaCamera.m_Lens.FieldOfView;
				fov -= zoomSpeed;
				fov = Mathf.Clamp(fov, minFov, maxFov);
				cinemaCamera.m_Lens.FieldOfView = fov;
			}
		}
		else {
			DefaultMotor.DoUpdateMotor();
		}
	}



	protected override void OnEndMotorImpl()
	{
		if (true == DefaultMotor.gameObject.activeSelf) {
			DefaultMotor.DoEndMotor();
		}
	}

	protected override void OnChangedTarget()
	{
		DoChangeLookTarget(LookTarget);
	}

	protected override void OnCameraActivated(bool bActivated)
	{
	}

	/// <summary> 바라보는 타겟을 변경한다. </summary>
	public void DoChangeLookTarget(Transform lookTarget)
	{
		if (LookTarget == Target) {
			LookTarget = null;
		}
		else {
			LookTarget = lookTarget;
		}

		if (null != LookTarget) {
			DefaultMotor.DoEndMotor();
			VirtualCamera.gameObject.SetActive(true);
		}
		else {
			DefaultMotor.DoBeginMotor();
			VirtualCamera.gameObject.SetActive(false);
		}
	}
}
