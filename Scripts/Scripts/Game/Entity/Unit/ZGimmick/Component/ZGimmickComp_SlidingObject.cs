using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameDB;

/// <summary> 회전에 따라 정해진 루트로 슬라이딩 된다 </summary>
public class ZGimmickComp_SlidingObject : MonoBehaviour
{
    [Header("제한된 위치")]
    [SerializeField]
    private Transform TransLimit_01;
    [Header("제한된 위치")]
    [SerializeField]
    private Transform TransLimit_02;
            
    private void Start()
    {
        ZMonoManager.Instance.AddUpdateCall(ZMonoManager.UpdateMode.LateUpdate, LateUpdateObject);
    }

    private void OnDestroy()
    {
        if(ZMonoManager.hasInstance)
            ZMonoManager.Instance.RemoveUpdateCall(ZMonoManager.UpdateMode.LateUpdate, LateUpdateObject);
    }

    private void LateUpdateObject()
    {
        Transform target = null;
        Transform otherTarget = null;

        float maxSqrtDist = (TransLimit_01.position - TransLimit_02.position).sqrMagnitude;
        float speedRate = 0f;

        var angle = Vector3.Angle(Vector3.up, (TransLimit_01.position - transform.position).normalized);
        var angle2 = Vector3.Angle(Vector3.up, (TransLimit_02.position - transform.position).normalized);

        if (95 < angle && 180 > angle)
        {
            target = TransLimit_01;
            otherTarget = TransLimit_02;

            speedRate = (angle - 95) / 85;
        }
        else if (95 < angle2 && 180 > angle2)
        {
            target = TransLimit_02;
            otherTarget = TransLimit_01;

            speedRate = (angle2 - 95) / 85;
        }           

        if (null == target)
            return;

        float gravity = 9.8f;

        var dir = (target.position - transform.position).normalized;

        transform.position += dir * gravity * Time.smoothDeltaTime * speedRate;

        var sqrtDist = (transform.position - otherTarget.position).sqrMagnitude;

        if (maxSqrtDist <= sqrtDist)
            transform.position = target.position;
    }
}