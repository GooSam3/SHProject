public enum E_GameModeEvent
{

}

/// <summary> 게임 모드 </summary>
public enum E_GameModeType
{
    Empty,
    Field,    
    Temple,
    Colosseum, //영웅의전당 : 3대3투기장
    TrialSanctuary,
    GodLand, //성지 : 비동기 시뮬레이션용
    Infinity,
    Tower, //시공의틈 : 필드형 정예던전    
    BossWar,
    GuildDungeon,
}


/// <summary> 게임 모드가 시작하기 위한 준비 단계 </summary>
[System.Flags]
public enum E_GameModePrepareStateType
{
    None = 0,
    /// <summary> 씬로드가 완료됨. </summary>
    SceneLoadComplete = 1 << 0,
    /// <summary> 맵 데이터 로드가 완료됨. </summary>
    MapDataLoadComplete = 1 << 1,
    /// <summary> MMO 에서 JoinField가 완료됨 </summary>
    MMO_JoinField = 1 << 2,
    /// <summary> MMO 에서 LoadMapOK가 완료됨 </summary>
    MMO_LoadMapOk = 1 << 3,
    /// <summary> 현재 게임모드에서 내 pc가 생성됨 </summary>
    CreateMyPc = 1 << 4,
    /// <summary> 현재 게임모드에서 필요한 UI가 사용할 준비가 됨 </summary>
    //UILoadComplete = 1 << 5,

    DefaultCheckState = SceneLoadComplete | MMO_JoinField | MMO_LoadMapOk,
}
