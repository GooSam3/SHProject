using UnityEngine;

public class CameraMotorFree : CameraMotorBase
{
    /// <summary> 카메라 셋팅 데이터 </summary>    
    [SerializeField]
    protected CameraMotorSettingFree Setting;

    /// <summary> 카메라 셋팅 데이터 </summary>    
    public override CameraMotorSettingBase SettingBase { get { return Setting; } }

    private float mOrbitX;
    private float mOrbitY;

    private float GoalDistance = 0f;
    private float BeginDistance = 0f;
    private float GoalDistanceElapsedTime = 0f;
    private bool GoalDistanceDirty = false;

    static int HitCheckLayerMask = UnityConstants.Layers.EverythingBut( UnityConstants.Layers.Entity, UnityConstants.Layers.Player, UnityConstants.Layers.Gimmick, UnityConstants.Layers.IgnoreCollision );

    public bool IsFixed { get; private set; }

    protected override void OnInitializeImpl()
    {
        SetFixedCamera( DeviceSaveDatas.LoadData( DeviceSaveDatas.KEY_IS_FIXED_CAM, false ) );

        mOrbitX = Setting.Angle.x;
        mOrbitY = Setting.Angle.y;

        GoalDistance = Setting.Distance;
        BeginDistance = Setting.Distance;
    }

    protected override void OnBegineMotorImpl()
    {
        //mOrbitX = MainCameraTrans.rotation.eulerAngles.x;
        //mOrbitY = MainCameraTrans.rotation.eulerAngles.y;
    }

    protected override void OnCameraActivated( bool bActivated )
    {
        if( false == bActivated ) {
            return;
        }

        if( IsFixed ) {
            mOrbitX = DeviceSaveDatas.LoadData( DeviceSaveDatas.KEY_FIXED_CAM_ORBIT_X, 0f );
            mOrbitY = DeviceSaveDatas.LoadData( DeviceSaveDatas.KEY_FIXED_CAM_ORBIT_Y, 0f );

            Setting.Distance = DeviceSaveDatas.LoadData( DeviceSaveDatas.KEY_FIXED_CAM_ORBIT_ZOOM, 0f );
            GoalDistance = Setting.Distance;
            BeginDistance = Setting.Distance;
        }
        else
        {
            //mOrbitX = MainCameraTrans.rotation.eulerAngles.x;
            mOrbitY = MainCameraTrans.rotation.eulerAngles.y;
        }
    }

	protected override void OnUpdateMotorImpl()
    {
        float distance = Setting.Distance;
        bool bHitCollision = false;

        if (null == Target)
            return;

        Vector3 TargetPosition = GetTargetPosition();

        //if (!Setting.IgnoreCollision)
        {
            Vector3 forward = mCameraTrans.position - TargetPosition;
            forward = forward.normalized;
            //int hitCount = Physics.RaycastNonAlloc(TargetPosition + forward, forward, mHitResults, distance);
			int hitCount = Physics.SphereCastNonAlloc(TargetPosition, 0.2f, forward, mHitResults, distance, HitCheckLayerMask);

			if (hitCount > 0)
            {
                System.Array.Sort(mHitResults, 0, hitCount, PhysicsHelper.RaycastHitDistanceComparer.Default);

                for (int i = 0; i < hitCount; ++i)
                {
                    RaycastHit hit = mHitResults[i];
                    int hitLayer = hit.transform.gameObject.layer;

                    // TODO :: 충돌 레이어 or 태그 설정후 처리
                    //if (false == hit.collider.CompareTag(UnityConstants.Tags.MapObject) && (hitLayer != UnityConstants.Layers.Terrain || hitLayer != UnityConstants.Layers.Wall))
                    //    continue;

                    if (hit.collider.isTrigger)
                        continue;

                    var hitDistance = hit.distance;

                    if (hitDistance < 0.5f || hitDistance > distance || float.IsNaN(hitDistance))
					{
						distance = 0.5f;
					}
					else
					{
						distance = Vector3.Distance(TargetPosition, hit.point);
					}

                    bHitCollision = true;
					break;
                }
            }
        }

        if (mController.IsEnableInput && IsFixed == false)
        {
#if UNITY_EDITOR
            // 에디터가 체감상 너무 느리때문에 배속을 늘려주자, 2d 입력값이라 회전은 x,y반대
            mOrbitX -= mController.InputY * ( DBConfig.CameraSpeedRotateX * 1.5f * distance ) / ( distance * 0.5f );
            mOrbitY += mController.InputX * ( DBConfig.CameraSpeedRotateY * 1.5f * distance ) / ( distance * 0.5f );
#else
            mOrbitX -= mController.InputY * ( DBConfig.CameraSpeedRotateX * distance ) / ( distance * 0.5f );
            mOrbitY += mController.InputX * ( DBConfig.CameraSpeedRotateY * distance ) / ( distance * 0.5f );
#endif
        }

        mOrbitX = ClampAngle( mOrbitX, Setting.MinAngleX, Setting.MaxAngleX );

        Vector3 camforward = GetCamForward();
        float dist = (TargetPosition - mCameraTrans.position).magnitude;

        SetGoalDistance(distance, dist);

        if (GoalDistanceDirty)
        {
            if (bHitCollision && GoalDistance < dist)
            {
                GoalDistance = distance;
                GoalDistanceDirty = false;
                dist = GoalDistance;
            }
            else
            {
                GoalDistanceElapsedTime += Time.smoothDeltaTime * Setting.Smoothing;

                dist = Mathf.Lerp(BeginDistance, GoalDistance, GoalDistanceElapsedTime);

                if (GoalDistanceElapsedTime >= 1f)
                {
                    GoalDistance = distance;
                    GoalDistanceDirty = false;
                    dist = GoalDistance;
                }
            }
        }
        else
        {
            GoalDistance = distance;
            GoalDistanceDirty = false;
            dist = GoalDistance;
        }

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

    private Vector3 GetCamForward()
    {
        mOrbitX = ClampAngle(mOrbitX, Setting.MinAngleX, Setting.MaxAngleX);

        Quaternion quatToEuler = Quaternion.Euler(mOrbitX, mOrbitY, 0);
        Vector3 zPosition = new Vector3(0.0f, 0.0f, 1f);
        Vector3 camforward = (quatToEuler * zPosition).normalized;

        return camforward;
    }

    private float ClampAngle(float angle, float min, float max)
    {
        while (angle < -360F) { angle += 360F; }
        while (angle > 360F) { angle -= 360F; }
        return Mathf.Clamp(angle, min, max);
    }

    public void SetFixedCamera( bool fix, bool withSave = false )
    {
        IsFixed = fix;
        isFixedZoom = fix;

        if( withSave ) {
            DeviceSaveDatas.SaveData( DeviceSaveDatas.KEY_IS_FIXED_CAM, IsFixed );
            if( IsFixed ) {
                DeviceSaveDatas.SaveData( DeviceSaveDatas.KEY_FIXED_CAM_ORBIT_X, mOrbitX );
                DeviceSaveDatas.SaveData( DeviceSaveDatas.KEY_FIXED_CAM_ORBIT_Y, mOrbitY );
                DeviceSaveDatas.SaveData( DeviceSaveDatas.KEY_FIXED_CAM_ORBIT_ZOOM, Setting.Distance );
            }
        }
    }
}