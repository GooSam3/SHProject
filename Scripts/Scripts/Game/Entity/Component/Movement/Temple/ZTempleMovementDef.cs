
public enum E_TempleCharacterState
{
    /// <summary> 바닥에 있는 상태 </summary>
    Grounded,
    /// <summary> 떨어지는 중 </summary>
    Falling,
    /// <summary> 올라가는 중 </summary>
    Rising,
    /// <summary> 점프 상태 </summary>
    Jumping,
    /// <summary> 슬라이딩 중 </summary>
    Sliding
}

public enum E_TempleCharacterControlState
{
    /// <summary> 조작 불가 상태 </summary>
    Empty,
    /// <summary> 기본 물리 적용되는 상태 </summary>
    Default,
    /// <summary> 상자 밀기 당기기 연출 상태 </summary>
    PullPush,
    /// <summary> 사다리 타기 상태 </summary>
    Ladder,
    /// <summary> 컨트롤 패널 연출 상태 </summary>
    ControllPanel,
    /// <summary> 물건을 옮기는 상태 </summary>
    Carry,
    /// <summary> 미니게임 상태 </summary>
    MiniGame,
}