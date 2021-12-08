using GameDB;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum E_MovingPlatformNextPositionType
{
    /// <summary> 이동 완료후 반대로 이동 </summary>
    PingPong,
    /// <summary> 반복 이동 </summary>
    Loop,    
    /// <summary> 웨이포인트 랜덤 이동 </summary>
    Random,
}

public enum E_MovingPlatformMoveType
{
    /// <summary> 다음 위치까지 한단계씩 실행 </summary>
    Step,
    /// <summary> 현재 진행방향 끝까지 실행 </summary>
    OneShot,
    /// <summary> 반복 실행 </summary>
    Loop,
}

public enum E_MovingObjectMoveType
{
    /// <summary> 이동/복귀 상태 토글 </summary>
    Toggle,
    /// <summary> 이동 후 복귀 처리 </summary>
    PingPong,
    /// <summary> 이동/복귀 상태 반복 </summary>
    Loop,
}

/// <summary> 기믹 트리거 관련 타입  </summary>
public enum E_GimmickTriggerType
{
    /// <summary> 즉시 발동 </summary>
    Immediate,
    /// <summary> UI 터치 후 발동 </summary>
    ActiveTouchUI,
}

/// <summary> 기믹 발동 횟수 타입 </summary>
public enum E_GimmickInvokeCountType
{
    /// <summary> 한번만 발동 </summary>
    OneShot,
    /// <summary> 지속 발동. 발동 카운트에 따라 예외처리? </summary>
    Loop,
}

/// <summary> 기믹의 액션 발동 타입에 관한 타입 </summary>
[System.Flags]
public enum E_GimmickActionInvokeType
{
    None = 0,
    /// <summary> 기믹이 활성화 될 때 발동 </summary>
    Enable = 1 << 0,
    /// <summary> 기믹이 비활서화 될 때 발동 </summary>
    Disable = 1 << 1,
    /// <summary> 트리거 안에 들어올 때 발동 </summary>
    Enter = 1 << 2,
    /// <summary> 트리거를 벗어났을 때 발동 </summary>
    Exit = 1 << 3,
    /// <summary> 트리거 안에 있을 때 발동 </summary>
    Stay = 1 << 4,
    /// <summary> 해당 속성(불) 공격을 받았을 때 발동 </summary>
    Attribute_Fire = 1 << 10,
    /// <summary> 해당 속성(물) 공격을 받았을 때 발동 </summary>
    Attribute_Water = 1 << 11,
    /// <summary> 해당 속성(전기) 공격을 받았을 때 발동 </summary>
    Attribute_Electric = 1 << 12,
    /// <summary> 해당 속성(빛) 공격을 받았을 때 발동 </summary>
    Attribute_Light = 1 << 13,
    /// <summary> 해당 속성(어둠) 공격을 받았을 때 발동 </summary>
    Attribute_Dark = 1 << 14,
    
    /// <summary> 무 속성 공격을 받았을 때 발동 </summary>
    Attribute_None = 1 << 20,

    /// <summary> 물 속성 얼리기 타임이 끝났을때 발동 </summary>
    Attribute_WaterCancle = 1 << 21,
}

public enum E_TempleEntranceState
{
    /// <summary> 비활성화 상태 </summary>
    Disable,
    /// <summary> 비활성화지만 활성화 가능한 상태 </summary>
    Enable,
    /// <summary> 활성화 후 오픈된 상태 </summary>
    Open,
    /// <summary> 클리어 상태 </summary>
    Clear,
}

/// <summary> 공통으로 사용할 액션 </summary>
public enum E_TemplePresetAction
{
    None,    
    /// <summary> 체크 포인트로 이동 </summary>
    WarpCheckPoint,
    /// <summary> 사망 처리 </summary>
    Die,
    /// <summary> 스턴 연출 </summary>
    Stun,
}

/// <summary> 기믹의 재질 </summary>
public enum E_TempleGimmickMeterial
{
    None,
    Wood,
    Ston,
    Metal,    
}

/// <summary> 양팔 저울에서 사용하는 타입 </summary>
public enum E_Balance
{
    First = 1,
    Second = 2,
}

/// <summary> 자석의 N극 S극 </summary>
public enum E_Magnetic
{
    N = 1,
    S = 2,
}


/// <summary> 미니게임에서 사용할 버튼 </summary>
[System.Flags]
public enum E_TempleUIType
{
    /// <summary> 모든 조작 관련 ui 가림 </summary>
    None = 0 ,
    Joystick = 1 << 0,
    Cancel = 1 << 1,
    Action = 1 << 2,

    Forward = 1 << 10,
    Backward = 1 << 11,

    /// <summary> 조이스틱과 취소 버튼 </summary>
    Joystick_CancelButton = Joystick | Cancel,
    /// <summary> 조이스틱과 액션 버튼</summary>
    Joystick_ActionButton = Joystick | Action,
    /// <summary> 조이스틱과 취소 및 액션 </summary>
    Joystick_CancelActionButton = Joystick | Cancel | Action,
    /// <summary> 조이스틱과 모든 버튼 </summary>
    Joystick_AllButton = Joystick | Cancel | Action | Forward | Backward,
    /// <summary> 모든 버튼 </summary>
    AllButton = Cancel | Action | Forward | Backward,
}

/// <summary> 기믹의 상태 </summary>
[Flags]
public enum E_TempleGimmickState
{
    None = 0,
    Die = 1 << 0,
    Ice = 1 << 2,
}

/// <summary> 해당 타입으로 기믹을 발동시키기 위한 클래스 </summary>
[Serializable]
public class GimmickActionInvokeType
{
    [SerializeField]
    private E_GimmickActionInvokeType InvokeType;

    [SerializeField]
    private E_GimmickActionInvokeType CancelInvokeType;

    [SerializeField]
    private List<string> m_listGimmickId = new List<string>();

    public void Invoke(E_AttributeLevel level)
    {
        foreach(var id in m_listGimmickId)
        {
            ZTempleHelper.InvokeGimmickActions(id, InvokeType, level);
        }        
    }

    public void Cancel()
    {
        foreach (var id in m_listGimmickId)
        {
            ZTempleHelper.InvokeGimmickActions(id, CancelInvokeType, E_AttributeLevel.Level_1);
        }
    }
}
