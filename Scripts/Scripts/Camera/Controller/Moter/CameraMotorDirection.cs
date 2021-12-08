using UnityEngine;

public sealed class CameraMotorDirection : CameraMotorBase
{
	/// <summary> 카메라 셋팅 데이터 </summary>    
	[SerializeField]
	protected CameraMotorSettingFree Setting;

	/// <summary> 카메라 셋팅 데이터 </summary>    
	public override CameraMotorSettingBase SettingBase { get { return Setting; } }

	private float mOrbitX;

	private float GoalDistance = 0f;
	private float BeginDistance = 0f;
	private float GoalDistanceElapsedTime = 0f;
	private bool GoalDistanceDirty = false;

	protected override void OnInitializeImpl()
	{
		mOrbitX = Setting.Angle.x;
		GoalDistance = Setting.Distance;
		BeginDistance = Setting.Distance;
	}

	protected override void OnBegineMotorImpl()
	{
	}

	static int HitCheckLayerMask = UnityConstants.Layers.EverythingBut(UnityConstants.Layers.Entity, UnityConstants.Layers.Player, UnityConstants.Layers.Gimmick, UnityConstants.Layers.IgnoreCollision);

	protected override void OnUpdateMotorImpl()
	{
		float distance = Setting.Distance;
		bool bHitCollision = false;

		if (null == Target)
			return;

		Vector3 TargetPosition = GetTargetPosition();

		Vector3 forward = mCameraTrans.position - TargetPosition;
		forward = forward.normalized;

		int hitCount = Physics.SphereCastNonAlloc(TargetPosition, 0.5f, forward, mHitResults, distance, HitCheckLayerMask);
		if (hitCount > 0) {
			System.Array.Sort(mHitResults, 0, hitCount, PhysicsHelper.RaycastHitDistanceComparer.Default);

			for (int i = 0; i < hitCount; ++i) {
				RaycastHit hit = mHitResults[i];

				if (hit.collider.isTrigger)
					continue;

				if (hit.distance < 0.5f) {
					distance = 0.5f;
				}
				else {
					distance = Vector3.Distance(TargetPosition, hit.point);
				}

				bHitCollision = true;
				break;
			}
		}

		if (mController.IsEnableInput) {
			float yInput = mController.InputY;

			// 2d 좌표 입력은 x,y값은 반대
			mOrbitX -= yInput * (DBConfig.CameraSpeedRotateX * distance) / (distance * 0.5f);
		}

		float dist = (TargetPosition - mCameraTrans.position).magnitude;

		SetGoalDistance(distance, dist);

		if (GoalDistanceDirty) {
			if (bHitCollision && GoalDistance < dist) {
				GoalDistance = distance;
				GoalDistanceDirty = false;
				dist = GoalDistance;
			}
			else {
				GoalDistanceElapsedTime += Time.smoothDeltaTime * Setting.Smoothing;

				dist = Mathf.Lerp(BeginDistance, GoalDistance, GoalDistanceElapsedTime);

				if (GoalDistanceElapsedTime >= 1f) {
					GoalDistance = distance;
					GoalDistanceDirty = false;
					dist = GoalDistance;
				}
			}
		}
		else {
			GoalDistance = distance;
			GoalDistanceDirty = false;
			dist = GoalDistance;
		}

		mOrbitX = ClampAngle(mOrbitX, Setting.MinAngleX, Setting.MaxAngleX);
		Quaternion quatToEuler = Quaternion.Euler(mOrbitX, Setting.Angle.y, 0);
		Vector3 zPosition = new Vector3(0.0f, 0.0f, 1f);
		Vector3 camforward = (quatToEuler * zPosition).normalized;

		mCameraTrans.position = -camforward * dist + TargetPosition;
		mCameraTrans.forward = camforward;
	}

	private void SetGoalDistance(float goalDist, float beginDist)
	{
		if (GoalDistance == goalDist)
			return;

		GoalDistance = goalDist;
		BeginDistance = beginDist;
		GoalDistanceElapsedTime = 0f;
		GoalDistanceDirty = true;
	}

	protected override void OnEndMotorImpl()
	{
	}

	protected override void OnChangedTarget()
	{
	}

	protected override void OnCameraActivated(bool bActivated)
	{
	}

	private float ClampAngle(float angle, float min, float max)
	{
		while (angle < -360F) { angle += 360F; }
		while (angle > 360F) { angle -= 360F; }
		return Mathf.Clamp(angle, min, max);
	}
}