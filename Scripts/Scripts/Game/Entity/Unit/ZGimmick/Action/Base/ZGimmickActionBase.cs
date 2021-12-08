using GameDB;
using System.Collections.Generic;
using UnityEngine;

/// <summary> 기믹용 컴포넌트. 기믹이 활성화/비활성화 되었을 때 처리 </summary>
public abstract class ZGimmickActionBase : MonoBehaviour
{
    /// <summary> 어느 상황에서 발동하는지 </summary>
    [Header("발동 타입. 여러개 선택 가능")]
    public E_GimmickActionInvokeType InvokeType;

    [Header("발동된 기믹을 취소 시킬때 사용. invokeType 이랑 중복되지 않게 셋팅해야함")]
    public E_GimmickActionInvokeType CancelInvokeType;

    /// <summary> Owner Gimmick </summary>
    public ZGimmick Gimmick { get; private set; }

    /// <summary> 발동 여부 </summary>
    public bool IsInvoked { get { return 0 < InvokeCount; } }

    /// <summary> 발동 횟수 </summary>    
    public int InvokeCount { get; private set; }

    /// <summary> 기믹이 활성화 된 상태인지 여부 </summary>
    public bool IsEnabled { get { return Gimmick.IsEnabled; } }

    public bool IsEnableAction { get; private set; }

    /// <summary> 발동한 속성 레벨 셋팅 </summary>
    public E_AttributeLevel InvokeAttributeLevel { get; private set; } = E_AttributeLevel.Level_1;

    /// <summary> 지연 발동 시간 </summary>
    [Header("지연 발동 시간")]
    public float DelayInvokeTime = 0f;

    [HideInInspector]
    public bool IsAwakeAttribute = false;

    /// <summary> 초기화 </summary>
    public void Initialize(ZGimmick gimmick)
    {
        Gimmick = gimmick;
        Gimmick?.DoAddEventTakeAttribute(TakeAttribute);
        InitializeImpl();
    }

    private void OnDestroy()
    {
        Gimmick?.DoRemoveEventTakeAttribute(TakeAttribute);
        DestroyImpl();
    }

    protected virtual void InitializeImpl()
    {

    }

    protected virtual void DestroyImpl()
    {

    }

    /// <summary> 해당 액션 비활성화 처리 </summary>
    public virtual void DisableAction()
    {

    }

    /// <summary> 액션 발동 </summary>
    public void Invoke(E_AttributeLevel level)
    {
        if (true == Gimmick.IsDead)
        {
            return;
        }   

        //발동한 속성 레벨
        InvokeAttributeLevel = level;

        IsEnableAction = true;

        //발동 횟수 증가
        ++InvokeCount;

        PreInvoke();

        if (0 < DelayInvokeTime)
        {
            Invoke("InvokeImpl", DelayInvokeTime);
        }
        else
        {
            InvokeImpl();
        }
    }

    /// <summary> 액션 취소 </summary>
    public void Cancel()
    {
        if (false == IsInvoked)
            return;

        IsEnableAction = false;

        CancelImpl();
    }

    /// <summary> 기믹 단계가 변경되었을 경우 처리 </summary>
    public void ChangeInvokeAttributeLevel(E_AttributeLevel level)
    {
        InvokeAttributeLevel = level;
        if (InvokeAttributeLevel < E_AttributeLevel.Level_1)
            InvokeAttributeLevel = E_AttributeLevel.Level_1;

        ChangeInvokeAttributeLevelImple();
    }

    // NOTE(JWK): 단순히 기믹의 단계가 변경되는게아닌 레벨업, 레벨다운 이 되는 경우
    /// <summary> 기믹 단계가 변경되었을 경우 처리 </summary>

    public void ChangeInvokeAttributeLevel(bool isUpgrade)
	{
        if(isUpgrade)
		{
            if (InvokeAttributeLevel == E_AttributeLevel.Level_6)
                return;
            else
                InvokeAttributeLevel = (E_AttributeLevel)((int)InvokeAttributeLevel + 1);
        }
        else
		{
            if (InvokeAttributeLevel == E_AttributeLevel.Level_1)
                return;
            else
                InvokeAttributeLevel = (E_AttributeLevel)((int)InvokeAttributeLevel - 1);
        }

        Debug.Log($"{Gimmick.name} 의 속성레벨 {InvokeAttributeLevel}");
        ChangeInvokeAttributeLevelImple();
    }

    /// <summary> 발동 직전 호출 </summary>
    protected virtual void PreInvoke()
    {

    }

    /// <summary> 발동 구현 </summary>
    protected abstract void InvokeImpl();
    /// <summary> 취소 구현 </summary>
    protected abstract void CancelImpl();
    /// <summary> 발동 속성 레벨 변경 구현 </summary>
    protected virtual void ChangeInvokeAttributeLevelImple()
    {
    }


    /// <summary> 특정 속성 공격 데미지를 입었을 경우 처리 </summary>
    protected virtual void TakeAttribute(ZGimmick gimmick, E_UnitAttributeType type)
    {
    }

    /// <summary> 해당 속성에 맞는 셋팅 데이터 가져오기 </summary>
    protected T GetSetting<T>(ref List<T> values, bool bIvokeAttribute = false) where T : new()
    {
        if(0 >= values.Count)
        {
            ZLog.LogError(ZLogChannel.Temple, $"속성 레벨 셋팅데이터가 1도 없음");
            return new T();
        }

        E_AttributeLevel attributeLevel = bIvokeAttribute ? InvokeAttributeLevel : Gimmick.InvokeAttributeLevel;

        if (values.Count < (int)attributeLevel)
        {
            ZLog.LogError(ZLogChannel.Temple, $"속성 레벨[{attributeLevel}]에 해당 셋팅데이터가 없음");
            return values[values.Count - 1];
        }

        return values[(int)attributeLevel - 1];
    }
}
