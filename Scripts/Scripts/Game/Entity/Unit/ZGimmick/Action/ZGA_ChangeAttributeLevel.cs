using GameDB;
using System.Collections.Generic;
using UnityEngine;

/// <summary> 작동 속성 레벨을 변경한다. </summary>
public class ZGA_ChangeAttributeLevel : ZGimmickActionBase
{
    [Header("변경할 기믹 id")]
    [SerializeField]
    private List<string> m_listGimmickId = new List<string>();

    [Header("변경할 속성 레벨")]
    [SerializeField]
    private E_AttributeLevel AttributeLevel = E_AttributeLevel.Level_1;

    protected override void InvokeImpl()
    {
        foreach(var id in m_listGimmickId)
        {
            ZTempleHelper.ChangeGimmickAttributeLevel(id, AttributeLevel);
        }
        
    }

    protected override void CancelImpl()
    {
    }
}