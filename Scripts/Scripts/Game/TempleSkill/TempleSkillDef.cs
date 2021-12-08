
/// <summary> 스킬 현재 상태 </summary>
//public enum E_TempleSkillState
//{
//	/// <summary> 스킬 시작시 </summary>
//	Start,
//	/// <summary> 스킬 캐스팅시 </summary>
//	Casting,
//	/// <summary> 스킬 액션시 </summary>
//	Action,
//	/// <summary> 스킬 루프시 </summary>
//	Loop,
//	/// <summary> 스킬 종료시 </summary>
//	End,
//}

/// <summary> 스킬 종료 이유 </summary>
public enum E_TempleSkillFinishReason
{
	SkillEnd,
	Cancel,
}

/// <summary> 스킬의 타겟 타입 </summary>
public enum E_TempleSkillTargetType
{
	/// <summary> 적군 </summary>
	Enemy,
	/// <summary> 자신 </summary>
	Self,
}