public enum E_SkillSystemError
{
    /// <summary> 에러 없음 </summary>
    None,
    /// <summary> 쿨타임 중 </summary>
    CoolTime,
    /// <summary> 유효하지 않은 대상 </summary>
    InvalidTarget,
    /// <summary> 스턴, 공격 불가, 넉백 등 공격 불가능한 상태 </summary>
    AbnormalState,
    /// <summary> 사정거리에서 벗어남 </summary>
    OutOfRange,
    /// <summary> 마나 부족 </summary>
    NotEnoughMp,
    /// <summary> 무기 장착 필요 </summary>
    InvalidWeaponType,
    /// <summary> 사용 가능한 캐릭터가 아니다 </summary>
    InvalidCharacterType,
    /// <summary> 사당에서 일반 조작 상태가 아닐경우 </summary>
    TempleControlState,
    /// <summary> 유효하지 않은 스킬 </summary>
    Invalid,
	/// <summary> 대상이 존재하지 않습니다. </summary>
	NotExistTarget,
    /// <summary> 자동힐을 이용할 수 없다. </summary>
	CantUseAutoHeal,
    /// <summary> 마을에서는 이용할 수 없다. </summary>
    CantUseInTowen,
}
