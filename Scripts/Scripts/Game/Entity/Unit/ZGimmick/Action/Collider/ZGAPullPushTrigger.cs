using System.Collections.Generic;
using UnityEngine;

/// <summary> 밀기 당기기용 트리거 </summary>
public class ZGAPullPushTrigger : MonoBehaviour
{
    private ZGA_PullPush mPullPushAction;

    private List<Collider> m_listEnteredCollider = new List<Collider>();

    private void Start()
    {
        mPullPushAction = GetComponentInParent<ZGA_PullPush>();
    }

    private void OnTriggerEnter(Collider other)
    {
        ZPawnMyPc pc = other.gameObject.GetComponent<ZPawnMyPc>();

        if (null == pc || true == pc.IsRiding)
        {
            if (null == mPullPushAction || null == mPullPushAction.Gimmick || null == other.GetComponent<ZGimmick>())
                return;

            if(false == other.isTrigger && mPullPushAction.Gimmick != other.GetComponent<ZGimmick>())
            {
                m_listEnteredCollider.Add(other);
            }
            return;
        }

        if (0 < m_listEnteredCollider.Count)
            return;

        if(false == other.isTrigger)
        {
            return;
        }

        mPullPushAction.TriggerEnter(this, other);
    }

    private void OnTriggerExit(Collider other)
    {
        ZPawnMyPc pc = other.gameObject.GetComponent<ZPawnMyPc>();

        m_listEnteredCollider.Remove(other);

        if (null == pc || false == other.isTrigger)
        {            
            return;
        }   

        mPullPushAction?.TriggerExit(this, other);
    }
}