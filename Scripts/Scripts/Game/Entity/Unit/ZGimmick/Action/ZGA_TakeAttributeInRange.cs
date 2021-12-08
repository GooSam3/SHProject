using GameDB;
using System.Collections.Generic;
using UnityEngine;

/// <summary> 범위안의 기믹에게 해당 타입의 속성 공격을 한다. </summary>
public class ZGA_TakeAttributeInRange : ZGimmickActionBase
{
    [Header("공격할 속성")]
    public E_UnitAttributeType AttributeType = E_UnitAttributeType.None;

    [Header("해당 기믹에 발동시킬 속성 레벨")]
    [SerializeField]
    private E_AttributeLevel AttributeLevel = E_AttributeLevel.Level_1;

    [Header("범위")]
    public float mRadius = 2f;

    protected override void InvokeImpl()
    {
        List<ZGimmick> gimmicks = new List<ZGimmick>();
        ZGimmickManager.Instance.TryGetTargets(Gimmick.transform, mRadius, ref gimmicks);

        foreach(var gimmick in gimmicks)
        {
            gimmick.TakeAttribute(AttributeType, AttributeLevel);
        }
    }

    protected override void CancelImpl()
    {
    }

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {        
        switch(AttributeType)
        {
            case E_UnitAttributeType.Fire:
                Gizmos.color = Color.red;
                break;
            case E_UnitAttributeType.Water:
                Gizmos.color = Color.blue;
                break;
            case E_UnitAttributeType.Electric:
                Gizmos.color = Color.yellow;
                break;
            default:
                Gizmos.color = Color.white;
                break;
                
        }
        if (null != Gimmick)
            Gizmos.DrawWireSphere(Gimmick.transform.position, mRadius);
        else
            Gizmos.DrawWireSphere(transform.position, mRadius);
    }
#endif
}