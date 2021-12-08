using System.Collections.Generic;
using UnityEngine;

/// <summary> 리지드바디가 있는 오브젝트 트리거 캐싱 </summary>
public class TriggerArea : MonoBehaviour
{
    private List<Rigidbody> m_listRigidbodyInTrigger = new List<Rigidbody>();
    private List<bool> m_listGravityFlag = new List<bool>();

    [Header("내 pc만 처리할지 여부")]
    [SerializeField]
    private bool IsOnlyMyPc = false;

    [Header("중력 적용 여부")]
    [SerializeField]
    private bool IsDisableGravity = false;
        
    void OnTriggerEnter(Collider col)
    {
        var entity = Check(col);
        if (null == entity)
            return;

        if (false == m_listRigidbodyInTrigger.Contains(col.attachedRigidbody) && false == col.attachedRigidbody.isKinematic)
        {
            m_listGravityFlag.Add(col.attachedRigidbody.useGravity);
            m_listRigidbodyInTrigger.Add(col.attachedRigidbody);

            if (false == entity.IsMyPc && true == IsDisableGravity)
            {
                col.attachedRigidbody.useGravity = false;
            }
        }
    }
    
    void OnTriggerExit(Collider col)
    {
        if (false == Check(col))
            return;

        int index = m_listRigidbodyInTrigger.FindIndex((item) => item == col.attachedRigidbody);
        if(0 <= index)
        {
            col.attachedRigidbody.useGravity = m_listGravityFlag[index];

            m_listRigidbodyInTrigger.RemoveAt(index);
            m_listGravityFlag.RemoveAt(index);
        }
    }

    private EntityBase Check(Collider col)
    {
        if (null == col.attachedRigidbody)
            return null;

        var entity = col.attachedRigidbody.GetComponent<EntityBase>();

        if (null == entity)
            return null;

        //내 pc일 경우 트리거, 아닐 경우 트리거가 아닐때 처리
        if (true == entity.IsMyPc)
        {
            if (false == col.isTrigger)
                return null;
        }
        else if (true == IsOnlyMyPc)
        {
            return null;
        }
        else
        {
            if (true == col.isTrigger)
                return null;
        }

        return entity;
    }

    public List<Rigidbody> GetEnteredRigidbody()
    {
        m_listRigidbodyInTrigger.RemoveAll((rBody) =>
        {
            if (null == rBody || rBody.isKinematic)
                return true;

            return false;
        });

        return m_listRigidbodyInTrigger;
    }
}
