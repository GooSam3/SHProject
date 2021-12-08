public enum E_PawnAIType
{
    None = 0,
    /// <summary> 비선공, 공격을 받으면 대응 </summary>
    Neutral = 1,
    /// <summary> 선공!! </summary>
    Hostile = 2,
    /// <summary> 평화. 공격을 받아도 아무 행동도 하지 않음 </summary>
    Friendly = 3,

    #region ===== :: 내 pc 전용 :: =====
    /// <summary> 내 pc 자동 전투 </summary>
    AutoBattle = 10,
    /// <summary> Quest 채집 AI </summary>
    QuestGathering,
    /// <summary> 특정 위치로 이동하는 AI </summary>
    MoveTo,
    /// <summary> Npc 위치로 이동 및 대화 </summary>
    TalkNpc,
    /// <summary> 퀘스트용 자동 전투 </summary>
    QuestBattle,
    #endregion 
}


public static class ZBlackbloardKey
{
    public const string MoveSpeed = "MoveSpeed";
    public const string Target = "Target";

    public const string SkillTid = "SkillTid";

    public const string StageTid = "StageTid";
    public const string GoalPosition = "GoalPosition";
    public const string TargetTid = "TargetTid";
}