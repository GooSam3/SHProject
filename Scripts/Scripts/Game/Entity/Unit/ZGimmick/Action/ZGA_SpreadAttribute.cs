using GameDB;
using System.Collections.Generic;
using UnityEngine;

/// <summary> 속성을 확산 </summary>
public class ZGA_SpreadAttribute : ZGimmickActionBase
{
    [Header("발동시 이펙트")]
    [SerializeField]
    private GameObject Fx_Invoke;

    [Header("발동시 확산될 속성")]
    [SerializeField]
    private E_UnitAttributeType AttributeType = E_UnitAttributeType.Fire;

    [Header("해당 기믹에 발동시킬 속성 레벨")]
    [SerializeField]
    private E_AttributeLevel AttributeLevel = E_AttributeLevel.Level_1;

    [Header("발동시 확산되기까지 딜레이 시간")]
    [SerializeField]
    private float SpreadDelayTime = 2f;

    [Header("발동시 해당 시간후 기믹이 제거됨. 0이면 무제한")]
    [SerializeField]
    private float DestroyTime = 0f;

    [Header("발동시 해당 시간후 이펙트가 제거되고, 확산이 중지됨. 0이면 무제한")]
    [SerializeField]
    private float SpreadTime = 0f;
    
    /// <summary> 해당 기믹이 불타기 시작한 시간 </summary>
    private Dictionary<ZGimmick, float> m_dicEntryGimmickList = new Dictionary<ZGimmick, float>();

    private void Awake()
    {
        enabled = false;
        ZTempleHelper.SetActiveFx(Fx_Invoke, false);
    }

    protected override void InvokeImpl()
    {
        if (AttributeType == E_UnitAttributeType.None)
            return;

        ZGimmickManager.Instance.AddSpreadAttribute(Gimmick, AttributeType);

        if (false == enabled)
        {
            ZTempleHelper.SetActiveFx(Fx_Invoke, true);
        }
                
        CancelInvoke(nameof(DisableSpread));

        if (0 < DestroyTime)
        {
            Gimmick.DestroyGimmick(3f, DestroyTime, DestroyGimmick);
        }

        if(0 < SpreadTime)
        {
            Invoke(nameof(DisableSpread), SpreadTime);
        }

        enabled = true;
    }

    protected override void CancelImpl()
    {
        if (AttributeType == E_UnitAttributeType.None)
            return;

        enabled = false;
        DesapwnEffect();        
        CancelInvoke(nameof(DisableSpread));
    }

    /// <summary> 이펙트 제거 </summary>
    private void DesapwnEffect()
    {
        ZTempleHelper.SetActiveFx(Fx_Invoke, false);
    }

    private void DestroyGimmick()
    {
        CancelInvoke(nameof(DisableSpread));

        DesapwnEffect();
    }

    private void DisableSpread()
    {
        CancelInvoke(nameof(DisableSpread));

        DesapwnEffect();
        enabled = false;

        m_dicEntryGimmickList.Clear();

        Gimmick.SetEnable(false, AttributeLevel);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (AttributeType == E_UnitAttributeType.None)
            return;

        ZGimmick gimmick = other.GetComponent<ZGimmick>();

        if (null == gimmick)
            return;

        if (false == enabled)
            return;

        if (false == m_dicEntryGimmickList.ContainsKey(gimmick))
            m_dicEntryGimmickList.Add(gimmick, 0f);

        m_dicEntryGimmickList[gimmick] = Time.time;
    }

    private void OnTriggerStay(Collider other)
    {
        if (AttributeType == E_UnitAttributeType.None)
            return;

        ZGimmick gimmick = other.GetComponent<ZGimmick>();

        if (null == gimmick)
            return;

        if (false == enabled)
            return;
        
        if (m_dicEntryGimmickList.TryGetValue(gimmick, out var startTime))
        {
            if (startTime + SpreadDelayTime < Time.time)
            {
                gimmick.TakeAttribute(AttributeType, AttributeLevel);

                m_dicEntryGimmickList.Remove(gimmick);
            }
        }
        else
        {
            m_dicEntryGimmickList.Add(gimmick, 0f);
            m_dicEntryGimmickList[gimmick] = Time.time;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (AttributeType == E_UnitAttributeType.None)
            return;

        ZGimmick gimmick = other.GetComponent<ZGimmick>();

        if (null == gimmick)
            return;

        if (false == enabled)
            return;

        m_dicEntryGimmickList.Remove(gimmick);
    }
}