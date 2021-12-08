using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NSkill
{
	public enum ETargetingType
	{
		None,
		Location,
		Directional,
		DirectionalLocation,
		SingleEnemy,
		SingleFriend,
	}

	public enum ERelationType
	{
		None,
		Relation_Me,
		Relation_Target,
		Relation_FriendAll,
		Relation_EnemyAll,
	}

	public enum EAnimationType
	{
		None,
		Anim_Idle,
		Anim_Walk,
		Anim_Dash,
		Anim_Attack,
		Anim_SkillNormal,
		Anim_SkillSpecial,
		Anim_Die,
	}

	public enum EAnimEventType
	{
		None,		
		AnimEvent_Attack,
		AnimEvent_Skill,
	}

	public enum ETaskEventType
	{
		None,
		TaskEvent_Enter,
		TaskEvent_Exit,
		TaskEvent_ExitForce,
		TaskEvent_Animation,
		TaskEvent_AutoCast,
		TaskEvent_AnimationEnd,
		//----------------------------
		TaskEventCustom_DamagaMain,
		TaskEventCustom_DamagaAdd1,
		TaskEventCustom_DamagaAdd2,
	}

	public enum EDamageType
	{
		None,
		Physical,
		Magical,
	}
}