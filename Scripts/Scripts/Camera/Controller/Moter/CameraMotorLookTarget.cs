using UnityEngine;

public class CameraMotorLookTarget : CameraMotorBase
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

    protected override void OnInitializeImpl()
    {
        LookTarget = null;
    }

    protected override void OnBegineMotorImpl()
    {
        //TODO :: 추후 기본 타겟 셋팅하든 하자
        DoChangeLookTarget(LookTarget);
    }

    protected override void OnUpdateMotorImpl()
    {
        if (null != LookTarget)
        {
            Vector3 camPos = new Vector3();
            Vector3 lookTargetPos = LookTarget.position + new Vector3(0f, Setting.LookTargetOffsetY, 0f);
            Vector3 dir = LookTarget.position - Target.position;
            dir.Normalize();

            Quaternion rot = Quaternion.LookRotation(dir);

            camPos = rot * Setting.TargetOffset + Target.position;

            ////if (!Setting.IgnoreCollision)
            //{
            //    Vector3 forward = camPos - lookTargetPos;

            //    int hitCount = Physics.RaycastNonAlloc(lookTargetPos, forward.normalized, mHitResults, (forward).magnitude);

            //    if (hitCount > 0)
            //    {
            //        System.Array.Sort(mHitResults, 0, hitCount, PhysicsHelper.RaycastHitDistanceComparer.Default);

            //        for (int i = 0; i < hitCount; ++i)
            //        {
            //            RaycastHit hit = mHitResults[i];

            //            //if (false == hit.collider.CompareTag(UnityConstants.Tags.MapObject) && hit.transform.gameObject.layer != UnityConstants.Layers.Wall)
            //            //    continue;

            //            if (hit.collider.isTrigger)
            //                continue;

            //            camPos = hit.point;
            //            break;
            //        }
            //    }
            //}

            mCameraTrans.position = camPos;
            mCameraTrans.rotation = Quaternion.LookRotation((lookTargetPos - mCameraTrans.position).normalized);
        }
        else
        {
            DefaultMotor.DoUpdateMotor();
        }
    }

    protected override void OnEndMotorImpl()
    {
        if (true == DefaultMotor.gameObject.activeSelf)
        {
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
        if (LookTarget == Target)
        {
            LookTarget = null;
        }   
        else
        {
            LookTarget = lookTarget;
        }   

        if (null != LookTarget)
        {
            DefaultMotor.DoEndMotor();
            VirtualCamera.gameObject.SetActive(true);
        }
        else
        {
            DefaultMotor.DoBeginMotor();
            VirtualCamera.gameObject.SetActive(false);
        }
    }
}
