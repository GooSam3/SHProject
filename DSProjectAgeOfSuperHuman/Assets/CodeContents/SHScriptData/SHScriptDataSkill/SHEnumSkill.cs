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
		Party,
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
		idle,
		ready,
		attack1,
		attack2,
		skill_01,
		skill_02,
		skill_03,
		skill_04,
		skill_05,
	}

	public enum EAnimEventType
	{
		None,		
		damage,
		damage_01,
		damage_02,
		damage_03,
		damage_04,
		damage_05,
		damage_06,
		damage_07,
		damage_08,
		damage_09,
		damage_10,
		damage_11,
		damage_12,
		damage_13,
		damage_14,
		damage_15,
	}

	public enum ETaskEventType
	{
		None,
		State_Enter,
		State_Exit,
		State_ExitForce,

		Animation_Event,
		Animation_End,

		AutoCast_On,
		AutoCast_Off,

		CustomEvent,
	}

	public enum ETaskEventCustomType
	{
		None,
		NextStap,
	}

	public enum EDamageType
	{
		None,
		AttackNormal,
		AttackSkill,
		Heal,
	}

	public enum EStageEffectType
	{
		None,
		MoveForward,
		CameraJumpKick,
		CameraKnockBack,
		CameraShake,
		RageComboType1,
	}

}