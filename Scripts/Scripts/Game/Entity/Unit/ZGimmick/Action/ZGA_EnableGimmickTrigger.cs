using UnityEngine;
using System.Collections.Generic;
using GameDB;

/// <summary> 해당 기믹을 활성화/비활성화 한다. </summary>
public class ZGA_EnableGimmickTrigger : ZGimmickActionBase
{
    //[Header("활성화/비활성화 토글")]
    //[SerializeField]
    //private bool IsEnable = true;

    [Header("활성화/비활성화 속성 레벨")]
    [SerializeField]
    private E_AttributeLevel AttributeLevel = E_AttributeLevel.Level_1;

    //[Header("현재 상태와 상관없이 강제로 처리")]
    //[SerializeField]
    //private bool IsForce = true;

    [Header("활성화/비활성화 시킬 기믹")]
    [SerializeField]
    private List<string> m_listEnableGimmickId = new List<string>();
     
    protected override void InvokeImpl()
    {
    }

    protected override void CancelImpl()
    {
    }

	private void OnTriggerEnter(Collider other)
	{
		ZPawnMyPc pc = other.GetComponent<ZPawnMyPc>();

		if (null == pc || false == other.isTrigger)
			return;

        foreach (var id in m_listEnableGimmickId)
        {
            ZTempleHelper.EnableGimmicks(id, true, AttributeLevel, true);
        }
    }

}