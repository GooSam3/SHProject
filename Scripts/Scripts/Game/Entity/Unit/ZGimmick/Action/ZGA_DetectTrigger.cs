using GameDB;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;

/// <summary> 해당 범위안에 설정한 gimmick이 들어오면 알려준다. </summary>
public class ZGA_DetectTrigger : ZGimmickActionDetectEventBase
{
    [Header("체크할 기믹 ID")]
    [SerializeField]
    private string mCheckGimmickId;
    public string CheckGimmickId { get { return mCheckGimmickId; } }

    [Header("트리거가 작동할 경우 활성화될 Gimmick들")]
    [SerializeField]
    private List<string> m_listEnableGimmickId = new List<string>();

    [Header("해당 기믹에 발동시킬 속성 레벨")]
    [SerializeField]
    public E_AttributeLevel AttributeLevel = E_AttributeLevel.Level_1;

    [Header("트리거 안에 들어온 기믹의 상태(속도, 범위등)에 상관없이 강제로 처리할지 여부")]
    [SerializeField]
    public bool IsForceDetect = false;

    [Header("허용 오차 범위")]
    [SerializeField]
    public float ToleranceRange = 0.1f;

    [Header("허용 오차 속도")]
    [SerializeField]
    public float ToleranceVelocity = 0.1f;

    [Header("허용 오차 각속도")]
    [SerializeField]
    public float ToleranceAngularVelocity = 0.1f;

    [Header("더이상 움직일 수 없게 비화성화 할 것인지 여부")]
    [SerializeField]
    public bool IsDisableAfterDetect = false;
        
    private ZGimmick DetectedGimmick = null;

    private Rigidbody DetectedGimmickRBody = null;

    protected override void InvokeImpl()
    {
        ZMonoManager.Instance.AddUpdateCall(ZMonoManager.UpdateMode.NormalUpdate, UpdateDetect);
    }

    protected override void DestroyImpl()
    {
        if (false == ZMonoManager.hasInstance)
            return;

        ZMonoManager.Instance.RemoveUpdateCall(ZMonoManager.UpdateMode.NormalUpdate, UpdateDetect);
    }

    protected override void CancelImpl()
    {
        ZMonoManager.Instance.RemoveUpdateCall(ZMonoManager.UpdateMode.NormalUpdate, UpdateDetect);
    }

    public void OnTriggerEnter(Collider other)
    {
        ZGimmick gimmick = other.gameObject.GetComponent<ZGimmick>();

        if (null == gimmick || (false == string.IsNullOrEmpty(CheckGimmickId) && false == gimmick.GimmickId.Equals(CheckGimmickId)))
            return;

        DetectedGimmick = gimmick;

        DetectedGimmickRBody = DetectedGimmick.GetComponentInChildren<Rigidbody>();
    }

    public void OnTriggerExit(Collider other)
    {
        ZGimmick gimmick = other.gameObject.GetComponent<ZGimmick>();

        if (null == gimmick || DetectedGimmick != gimmick)
            return;

        UnDetect();
    }

    private void UpdateDetect()
    {
        if (false == IsEnableAction)
        {
            UnDetect();
            return;
        }

        if (null == DetectedGimmick || null == DetectedGimmickRBody)
        {
            UnDetect();
            return;
        }

        if (false == IsForceDetect)
        {
            if (Vector3.SqrMagnitude(DetectedGimmick.transform.position - transform.position) > (ToleranceRange * ToleranceRange))
            {
                UnDetect();
                return;
            }

            //if (DetectedGimmickRBody.velocity.magnitude > ToleranceVelocity)
            //{
            //    UnDetect();
            //    return;
            //}

            //if (DetectedGimmickRBody.angularVelocity.magnitude > ToleranceAngularVelocity)
            //{
            //    UnDetect();
            //    return;
            //}
        }

        DetectedGimmickRBody.velocity = Vector3.zero;
        DetectedGimmickRBody.angularVelocity = Vector3.zero;

        Detect();
    }

    /// <summary> 해당하는 기믹을 찾았을 경우 처리 </summary>
    private void Detect()
    {
        if (IsDetect)
            return;

        IsDetect = true;

        SetEnableGimmick();
        ZMonoManager.Instance.RemoveUpdateCall(ZMonoManager.UpdateMode.NormalUpdate, UpdateDetect);

        if(IsDisableAfterDetect)
        {
            if(null != DetectedGimmickRBody)
            {
                DetectedGimmickRBody.isKinematic = true;
                DetectedGimmickRBody.useGravity = false;

                DetectedGimmickRBody.transform.parent = transform;
                DetectedGimmickRBody.transform.DOLocalMove(Vector3.zero, 0.5f);
                GameObject.Destroy(DetectedGimmickRBody);

                DetectedGimmickRBody = null;
            }

            DetectedGimmick?.DisableActionAll();
        }
    }

    private void UnDetect()
    {
        if (false == IsDetect)
            return;

        if (IsDetect && IsDisableAfterDetect)
            return;

        IsDetect = false;

        ZMonoManager.Instance.AddUpdateCall(ZMonoManager.UpdateMode.NormalUpdate, UpdateDetect);

        if (IsDisableAfterDetect && null != DetectedGimmickRBody)
        {
            DetectedGimmickRBody.isKinematic = false;
            DetectedGimmickRBody.useGravity = true;
            DetectedGimmickRBody.transform.parent = null;
        }

        DetectedGimmick = null;
        DetectedGimmickRBody = null;

        SetEnableGimmick();
    }
        
    private void SetEnableGimmick()
    {
        foreach (var id in m_listEnableGimmickId)
        {
            ZTempleHelper.EnableGimmicks(id, IsDetect, AttributeLevel, true);
        }
    }
}