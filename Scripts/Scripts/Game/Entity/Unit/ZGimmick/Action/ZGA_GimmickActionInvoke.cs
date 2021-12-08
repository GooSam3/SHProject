using System.Collections.Generic;
using UnityEngine;

/// <summary> 해당 타입으로 기믹을 발동 </summary>
public class ZGA_GimmickActionInvoke : ZGimmickActionBase
{
    [Header("발동할 액션")]
    [SerializeField]
    private List<GimmickActionInvokeType> m_listGimmickActionInvoke = new List<GimmickActionInvokeType>();

    [Header("발동과 취소를 반대로 적용")]
    [SerializeField]
    private bool IsRevers;

    protected override void InvokeImpl()
    {
        GimmickActionInvoke(IsRevers);
    }

    protected override void CancelImpl()
    {
        GimmickActionInvoke(false == IsRevers);
    }

    private void GimmickActionInvoke(bool bCancel)
    {
        foreach (var gimmick in m_listGimmickActionInvoke)
        {
            if (false == bCancel)
            {
                gimmick.Invoke(InvokeAttributeLevel);
            }
            else
            {
                gimmick.Cancel();
            }
        }
    }
         
}