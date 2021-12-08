using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> 중력 조작 </summary>
public class ZGimmickComp_GravityControl : MonoBehaviour
{    
    [SerializeField]
    private TriggerArea mTriggerArea;

    [Header("중력 방향")]
    [SerializeField]
    private Vector3 GravityDir = Vector3.down;

    [Header("중력 힘")]
    [SerializeField]
    private float GravityPower = 9.8f;

    public void OnEnable()
    {
        if (null == mTriggerArea)
            return;

        StartCoroutine(Co_LateFixedUpdate());
    }

    public bool IsTest;
    private IEnumerator Co_LateFixedUpdate()
    {
        var instruction = new WaitForFixedUpdate();

        RaycastHit[] inputHitResults = new RaycastHit[15];

        while (true)
        {
            yield return instruction;

            var dir = transform.rotation * GravityDir;
            var list = mTriggerArea.GetEnteredRigidbody();

            var angle = Vector3.Dot(dir, Vector3.up);

            angle = Mathf.Abs(angle);

            var rate = 1f - angle;

            rate = Mathf.Clamp(rate * 8f, 0f, 1f);

            Vector3 slideDirection = Vector3.ProjectOnPlane(Vector3.down, -dir).normalized;

            var velocity = (slideDirection * GravityPower * rate) + (dir * GravityPower);

            for (int i = 0; i < list.Count; i++)
            {
                list[i].AddForce(velocity, ForceMode.Acceleration);
            }
        }
    }
}