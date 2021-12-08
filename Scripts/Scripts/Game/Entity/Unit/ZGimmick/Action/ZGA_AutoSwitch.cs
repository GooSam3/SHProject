using GameDB;
using UnityEngine;

/// <summary> On/Off 전환 스위치 일정 시간마다 자동 </summary>
public class ZGA_AutoSwitch : ZGimmickActionBase
{
    [Header("목표 기믹 Id (입력하지 않는다면 자기 자신)")]
    [SerializeField]
    private string FindGimmickId = string.Empty;

    [Header("해당 스위치에 발동시킬 속성 레벨")]
    [SerializeField]
    private E_AttributeLevel AttributeLevel = E_AttributeLevel.Level_1;
    
    [Header("해당 시간마다 반복 (On/Off)")]
    [SerializeField]
    private float RepeatingTime = 3f;

    protected override void InvokeImpl()
    {
        InvokeRepeating(nameof(UpdateGimmick), 0f, RepeatingTime);
    }

    protected override void CancelImpl()
    {
        CancelInvoke(nameof(UpdateGimmick));
    }

    private void UpdateGimmick()
    {
        if (string.IsNullOrEmpty(FindGimmickId))
        {
            Gimmick.SetEnable(!IsEnabled, AttributeLevel, true);
            return;
        }

        ZTempleHelper.EnableToggleGimmicks(FindGimmickId, AttributeLevel, true);
    }
}