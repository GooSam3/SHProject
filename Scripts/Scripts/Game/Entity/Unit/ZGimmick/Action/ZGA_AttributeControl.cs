using GameDB;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AttributeControlItem
{
    public E_AttributeLevel Level;
    public float Time;
}

/// <summary>
/// 물 공격으로 기믹을 얼릴때 사용
/// - 얼었을시 불 공격으로 녹일수 있음
/// </summary>
public class ZGA_AttributeControl : ZGimmickActionBase
{
    [Header("발동 속성")]
    [SerializeField]
    private E_UnitAttributeType AttributeType = E_UnitAttributeType.Water;

    [Header("해당 기믹에 발동시킬 속성 레벨")]
    [SerializeField]
    private E_AttributeLevel AttributeLevel = E_AttributeLevel.Level_1;

    [Header("얼리는 시간(s) - '0' 일때 무제한")]
    [SerializeField]
    private List<AttributeControlItem> FreezingTimes;

    // 현제 얼려 져 있는지
    private bool isFreeze = false;

	protected override void InvokeImpl()
    {
        if (isFreeze)
            return;

        if(Gimmick.Meterial == E_TempleGimmickMeterial.Ston)
		{
            isFreeze = true;
            var freezingTime = GetFreezingTime();

            // 0 보다 클때만 자동으로 녹여줌
            if (0 < freezingTime)
                Invoke(nameof(CancelImpl), freezingTime);

            Gimmick.InvokeActions(E_GimmickActionInvokeType.Attribute_Water, AttributeLevel);
            Gimmick.SetAttributeMaterialColor(E_UnitAttributeType.Water);
        }
    }

    private float GetFreezingTime()
	{
        // 속성값 보다 현제 들어가 있는 프리징시간 갯수가 작을경우 무조건 '0'
        var findItem = FreezingTimes.Find(d => d.Level == AttributeLevel);
        if (null == findItem)
            return 0;

        return findItem.Time;
    }

	protected override void CancelImpl()
    {
        CancelInvoke();

        if(true == isFreeze)
		{
            if (Gimmick.Meterial == E_TempleGimmickMeterial.Ston)
            {
                isFreeze = false;
                Gimmick.SetAttributeMaterialColor(E_UnitAttributeType.None);
                Gimmick.InvokeActions(E_GimmickActionInvokeType.Attribute_WaterCancle, AttributeLevel);
            }
        }
    }

    protected override void DestroyImpl()
    {
        CancelInvoke();
    }

}
