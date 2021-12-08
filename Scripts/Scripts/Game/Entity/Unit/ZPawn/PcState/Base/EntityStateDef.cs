public enum E_EntityState
{
    Empty,
    /// <summary> 일반 공격 상태 </summary>
    Attack,
    /// <summary> 스킬 사용 상태 </summary>
    Skill,
    /// <summary> 채집 상태 </summary>
    Gathering,
}

public enum E_EntityAttackTargetType
{
    None = 0,           // 공격안함
    MainTarget = 1,     // 메인 공격
    SecondTarget = 2,   // 세컨 공격
}