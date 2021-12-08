
/// <summary> 클라에서만 사용할 상태값 </summary>
[System.Flags]
public enum E_CustomConditionControl
{
    None = 0,
    /// <summary> Pk 중인 상태 (보라돌이 상태) </summary>
    Pk = 1 << 0,    
    /// <summary> 내 파티 멤버 </summary>
    PartyMember = 1 << 1,
    /// <summary> 내 길드 멤버 </summary>
    GuildMember = 1 << 2,
    /// <summary> 적대 유저 </summary>
    HostileUser = 1 << 3,
    /// <summary> 적대 길드 </summary>
    HostileGuild = 1 << 4,
    /// <summary> 연맹 길드 </summary>
    AllianceGuild = 1 << 5,
    /// <summary> 내 친구 </summary>
    Friend = 1 << 10,
    /// <summary> 유적에서 일반 컨트롤 상태가 아닐 경우 활성화 </summary>
    Temple_Control = 1 << 20,
}