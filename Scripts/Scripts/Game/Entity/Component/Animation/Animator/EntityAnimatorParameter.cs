using System.Collections.Generic;
using UnityEngine;

/// <summary> Animator에 셋팅되어있는 Parameter들 (추가한다면 밑으로만 추가해야함.) </summary>
public enum E_AnimParameter
{
    None,
    MoveSpeedRate,
    AttackSpeedRate,
    SkillSpeedRate,

    Attack_001,
    Attack_002,
    Attack_003,

    Skill_001,
    Skill_002,

    Buff_001,
    
    Knockback_001,

    SkillCast_End_001,
    SkillCast_Start_001,
    SkillCasting_001,

    SkillRush_End_001,
    SkillRush_Start_001,
    SkillRush_001,

    SkillLeap_End_001,
    SkillLeap_Start_001,
    SkillLeap_001,

    SkillPull_001,

    Die_001,
    Stun_001,

    Hit_001,

    //===== :: 탈 것 :: ====
    Riding_001,

    //===== :: 로비용 :: ====
    Lobby_001,
    
    //===== :: 사당용 :: =====
    Lift_Start_001,
    Lift_001,
    Lift_End_001,
    PullPush_001,
    Dir,
    Sliding_Ladder_001,
    Climbing_001,
    Climbing_End_001,
    Landing_001,
    Jump_Start_001,
    Jump_001,
    Gliding_001,
    FallDown_001,

    //===== :: 기믹용 :: =====
    Idle_001,
    Start_001,
    Loop_001,
    End_001,

    //===== :: Npc용 :: =====
    Action_001,
    Action_002,
    Action_003,

    CombatMode,
    GimmickAnimSpeed,

    Throw_001,

    //==== :: Pet Taming용 :: ====
    Taming_Rodeo,
    Taming_Success,
    Taming_Drop,
    Taming_Fail,
}

/// <summary> Animator에 셋팅되어있는 State Name </summary>
public enum E_AnimStateName
{
    None,

    Attack_001,
    Attack_002,
    Attack_003,

    Buff_001,

    SkillCast_End_001,
    SkillCast_Start_001,
    SkillCasting_001,

    SkillRush_End_001,
    SkillRush_Start_001,
    SkillRush_001,

    SkillLeap_End_001,
    SkillLeap_Start_001,
    SkillLeap_001,

    SkillPull_001,

    Climbing_001,
    Climbing_End_001,

    Die_001,

    End_001,

    FallDown_001,

    Gliding_001,

    Hit_001,

    Idle_001,

    Jump_001,
    Jump_Start_001,

    Knockback_001,

    Landing_001,    
    Lift_End_001,
    Lift_Idle_001,
    Lift_Start_001,
    Lift_Walk_001,
    Loop_001,

    Pull_001,
    PullPush_Idle_001,
    Push_001,

    Run_001,

    Skill_001,
    Skill_002,
    Sliding_Ladder_001,
    Start_001,
    Stun_001,
    
    Walk_001,

	//===== :: 로비용 :: ====
	Lobby_001,

    //===== :: Npc용 :: =====
    Action_001,
    Action_002,
    Action_003,

    BattleIdle_001,

    Throw_001,

    RideIdle_001,
    Riding_001,

    //===== :: Pet Taming용 :: =====

    Taming_Rodeo,
    Taming_Success,
    Taming_Drop,
    Taming_Fail,


    Max,
}


/// <summary> Animator의 Parameter에 관한 처리를 위한 클래스 </summary>
public class EntityAnimatorParameter
{
	/// <summary> Parameter를 처리하기 위한 클래스 </summary>
	private class ZAnimatorIdAndParameterType
    {
        public int Id;
        public AnimatorControllerParameterType ParameterType;

        public bool BoolValue = false;
        public int IntValue = 0;
        public float FloatValue = 0f;        

        public ZAnimatorIdAndParameterType(E_AnimParameter parameter, bool defaultValue)
        {
            Id = Animator.StringToHash(parameter.ToString());
            ParameterType = AnimatorControllerParameterType.Bool;
            BoolValue = defaultValue;
        }

        public ZAnimatorIdAndParameterType(E_AnimParameter parameter, int defaultValue)
        {
            Id = Animator.StringToHash(parameter.ToString());
            ParameterType = AnimatorControllerParameterType.Int;
            IntValue = defaultValue;
        }

        public ZAnimatorIdAndParameterType(E_AnimParameter parameter, float defaultValue)
        {
            Id = Animator.StringToHash(parameter.ToString());
            ParameterType = AnimatorControllerParameterType.Float;
            FloatValue = defaultValue;
        }

        public ZAnimatorIdAndParameterType(E_AnimParameter parameter)
        {
            Id = Animator.StringToHash(parameter.ToString());
            ParameterType = AnimatorControllerParameterType.Trigger;
        }
    }

    private static Dictionary<E_AnimParameter, ZAnimatorIdAndParameterType> Parameters = new Dictionary<E_AnimParameter, ZAnimatorIdAndParameterType>();

    /// <summary> static 생성자. Parameter를 셋팅한다. </summary>
    static EntityAnimatorParameter()
    {
        Parameters.Clear();

        Parameters.Add(E_AnimParameter.MoveSpeedRate, new ZAnimatorIdAndParameterType(E_AnimParameter.MoveSpeedRate, 0f));
        Parameters.Add(E_AnimParameter.AttackSpeedRate, new ZAnimatorIdAndParameterType(E_AnimParameter.AttackSpeedRate, 1f));
        Parameters.Add(E_AnimParameter.SkillSpeedRate, new ZAnimatorIdAndParameterType(E_AnimParameter.SkillSpeedRate, 1f));        
        Parameters.Add(E_AnimParameter.CombatMode, new ZAnimatorIdAndParameterType(E_AnimParameter.CombatMode, 0f));

        Parameters.Add(E_AnimParameter.Attack_001, new ZAnimatorIdAndParameterType(E_AnimParameter.Attack_001));
        Parameters.Add(E_AnimParameter.Attack_002, new ZAnimatorIdAndParameterType(E_AnimParameter.Attack_002));
        Parameters.Add(E_AnimParameter.Attack_003, new ZAnimatorIdAndParameterType(E_AnimParameter.Attack_003));

        Parameters.Add(E_AnimParameter.Skill_001, new ZAnimatorIdAndParameterType(E_AnimParameter.Skill_001));
        Parameters.Add(E_AnimParameter.Skill_002, new ZAnimatorIdAndParameterType(E_AnimParameter.Skill_002));

        Parameters.Add(E_AnimParameter.Buff_001, new ZAnimatorIdAndParameterType(E_AnimParameter.Buff_001));        
        Parameters.Add(E_AnimParameter.Knockback_001, new ZAnimatorIdAndParameterType(E_AnimParameter.Knockback_001));

        Parameters.Add(E_AnimParameter.SkillCast_Start_001, new ZAnimatorIdAndParameterType(E_AnimParameter.SkillCast_Start_001));
        Parameters.Add(E_AnimParameter.SkillCasting_001, new ZAnimatorIdAndParameterType(E_AnimParameter.SkillCasting_001, false));
        Parameters.Add(E_AnimParameter.SkillCast_End_001, new ZAnimatorIdAndParameterType(E_AnimParameter.SkillCast_End_001));

        Parameters.Add(E_AnimParameter.SkillRush_Start_001, new ZAnimatorIdAndParameterType(E_AnimParameter.SkillRush_Start_001));
        Parameters.Add(E_AnimParameter.SkillRush_001, new ZAnimatorIdAndParameterType(E_AnimParameter.SkillRush_001, false));
        Parameters.Add(E_AnimParameter.SkillRush_End_001, new ZAnimatorIdAndParameterType(E_AnimParameter.SkillRush_End_001));

        Parameters.Add(E_AnimParameter.SkillLeap_Start_001, new ZAnimatorIdAndParameterType(E_AnimParameter.SkillLeap_Start_001));
        Parameters.Add(E_AnimParameter.SkillLeap_001, new ZAnimatorIdAndParameterType(E_AnimParameter.SkillLeap_001, false));
        Parameters.Add(E_AnimParameter.SkillLeap_End_001, new ZAnimatorIdAndParameterType(E_AnimParameter.SkillLeap_End_001));
                
        Parameters.Add(E_AnimParameter.SkillPull_001, new ZAnimatorIdAndParameterType(E_AnimParameter.SkillPull_001));

        Parameters.Add(E_AnimParameter.Die_001, new ZAnimatorIdAndParameterType(E_AnimParameter.Die_001, false));
        Parameters.Add(E_AnimParameter.Stun_001, new ZAnimatorIdAndParameterType(E_AnimParameter.Stun_001, false));

        Parameters.Add(E_AnimParameter.Hit_001, new ZAnimatorIdAndParameterType(E_AnimParameter.Hit_001));
        Parameters.Add(E_AnimParameter.Riding_001, new ZAnimatorIdAndParameterType(E_AnimParameter.Riding_001, false));

        //===== :: 로비용 :: =====
        Parameters.Add(E_AnimParameter.Lobby_001, new ZAnimatorIdAndParameterType(E_AnimParameter.Lobby_001));

		//===== :: 사당용 :: =====
		Parameters.Add(E_AnimParameter.Lift_Start_001, new ZAnimatorIdAndParameterType(E_AnimParameter.Lift_Start_001, false));
        Parameters.Add(E_AnimParameter.Lift_001, new ZAnimatorIdAndParameterType(E_AnimParameter.Lift_001, false));
        Parameters.Add(E_AnimParameter.Lift_End_001, new ZAnimatorIdAndParameterType(E_AnimParameter.Lift_End_001));
        Parameters.Add(E_AnimParameter.Throw_001, new ZAnimatorIdAndParameterType(E_AnimParameter.Throw_001));

        Parameters.Add(E_AnimParameter.PullPush_001, new ZAnimatorIdAndParameterType(E_AnimParameter.PullPush_001, false));

        Parameters.Add(E_AnimParameter.Dir, new ZAnimatorIdAndParameterType(E_AnimParameter.Dir, 0f));

        Parameters.Add(E_AnimParameter.Sliding_Ladder_001, new ZAnimatorIdAndParameterType(E_AnimParameter.Sliding_Ladder_001, false));

        Parameters.Add(E_AnimParameter.Climbing_001, new ZAnimatorIdAndParameterType(E_AnimParameter.Climbing_001, false));
        Parameters.Add(E_AnimParameter.Climbing_End_001, new ZAnimatorIdAndParameterType(E_AnimParameter.Climbing_End_001));

        Parameters.Add(E_AnimParameter.Landing_001, new ZAnimatorIdAndParameterType(E_AnimParameter.Landing_001));

        Parameters.Add(E_AnimParameter.Jump_Start_001, new ZAnimatorIdAndParameterType(E_AnimParameter.Jump_Start_001));
        Parameters.Add(E_AnimParameter.Jump_001, new ZAnimatorIdAndParameterType(E_AnimParameter.Jump_001, false));

        Parameters.Add(E_AnimParameter.Gliding_001, new ZAnimatorIdAndParameterType(E_AnimParameter.Gliding_001, false));
        Parameters.Add(E_AnimParameter.FallDown_001, new ZAnimatorIdAndParameterType(E_AnimParameter.FallDown_001, false));

        //===== :: 기믹용 :: =====
        Parameters.Add(E_AnimParameter.Idle_001, new ZAnimatorIdAndParameterType(E_AnimParameter.Idle_001));
        Parameters.Add(E_AnimParameter.Start_001, new ZAnimatorIdAndParameterType(E_AnimParameter.Start_001));
        Parameters.Add(E_AnimParameter.End_001, new ZAnimatorIdAndParameterType(E_AnimParameter.End_001));
        Parameters.Add(E_AnimParameter.Loop_001, new ZAnimatorIdAndParameterType(E_AnimParameter.Loop_001));
        Parameters.Add(E_AnimParameter.GimmickAnimSpeed, new ZAnimatorIdAndParameterType(E_AnimParameter.GimmickAnimSpeed, 1f));

        //===== :: Npc용 :: =====
        Parameters.Add(E_AnimParameter.Action_001, new ZAnimatorIdAndParameterType(E_AnimParameter.Action_001));
        Parameters.Add(E_AnimParameter.Action_002, new ZAnimatorIdAndParameterType(E_AnimParameter.Action_002));
        Parameters.Add(E_AnimParameter.Action_003, new ZAnimatorIdAndParameterType(E_AnimParameter.Action_003));

        //===== :: Pet Taming용 :: =====
        Parameters.Add(E_AnimParameter.Taming_Rodeo, new ZAnimatorIdAndParameterType(E_AnimParameter.Taming_Rodeo));
        Parameters.Add(E_AnimParameter.Taming_Success, new ZAnimatorIdAndParameterType(E_AnimParameter.Taming_Success));
        Parameters.Add(E_AnimParameter.Taming_Drop, new ZAnimatorIdAndParameterType(E_AnimParameter.Taming_Drop));
        Parameters.Add(E_AnimParameter.Taming_Fail, new ZAnimatorIdAndParameterType(E_AnimParameter.Taming_Fail));
    }

    public static AnimatorControllerParameterType GetParameterType(E_AnimParameter parameter)
    {
        return Parameters[parameter].ParameterType;
    }

    public static void ResetDefaultParametersValue(EntityComponentAnimation_Animator component)
    {
		foreach (var param in Parameters)
        {
            switch(param.Value.ParameterType)
            {
                case AnimatorControllerParameterType.Bool:
                    SetParameter(component, param.Key, param.Value.BoolValue);
                    break;
                case AnimatorControllerParameterType.Int:
                    SetParameter(component, param.Key, param.Value.IntValue);
                    break;
                case AnimatorControllerParameterType.Float:
                    SetParameter(component, param.Key, param.Value.FloatValue);
                    break;
            }            
        }
    }

    /// <summary> Animator의 Parameter를 셋팅한다. </summary>
    public static void SetParameter(EntityComponentAnimation_Animator component, E_AnimParameter type, bool value)
    {
        var param = Parameters[type];

        if (false == CheckType(component, param, AnimatorControllerParameterType.Bool))
        {
            return;
        }

		component.Anim.SetBool(param.Id, value);
    }

    public static void SetParameter(Animator anim, E_AnimParameter type, bool value)
    {
        var param = Parameters[type];

        anim.SetBool(param.Id, value);
    }

    /// <summary> Animator의 Parameter를 셋팅한다. </summary>
    public static void SetParameter(EntityComponentAnimation_Animator component, E_AnimParameter type, int value)
    {
        var param = Parameters[type];

        if (false == CheckType(component, param, AnimatorControllerParameterType.Int))
        {
            return;
        }

        component.Anim.SetInteger(param.Id, value);
    }

    public static void SetParameter(Animator anim, E_AnimParameter type, int value)
    {
        var param = Parameters[type];

        anim.SetInteger(param.Id, value);
    }

    /// <summary> Animator의 Parameter를 셋팅한다. </summary>
    public static void SetParameter(EntityComponentAnimation_Animator component, E_AnimParameter type, float value)
    {
        var param = Parameters[type];

        if (false == CheckType(component, param, AnimatorControllerParameterType.Float))
        {
            return;
        }

        component.Anim.SetFloat(param.Id, value);
    }

    public static void SetParameter(Animator anim, E_AnimParameter type, float value)
    {
        var param = Parameters[type];

        anim.SetFloat(param.Id, value);
    }

    /// <summary> Animator의 Parameter를 셋팅한다. </summary>
    public static void SetParameter(EntityComponentAnimation_Animator component, E_AnimParameter type)
    {
        var param = Parameters[type];

        if (false == CheckType(component, param, AnimatorControllerParameterType.Trigger))
        {
            return;
        }

        if (component.LastTriggerId > 0)
            component.Anim.ResetTrigger(component.LastTriggerId);

        component.LastTriggerId = param.Id;
        component.Anim.SetTrigger(param.Id);
    }

    public static void SetParameter(Animator anim, E_AnimParameter type)
    {
        var param = Parameters[type];

        anim.SetTrigger(param.Id);
    }

    public static bool GetBool(Animator anim, E_AnimParameter type)
    {
        if (null == anim)
        {
            ZLog.LogWarn(ZLogChannel.Entity, "Anim is null");
            return false;
        }

        var param = Parameters[type];

        if (param.ParameterType != AnimatorControllerParameterType.Bool)
        {

            ZLog.LogWarn(ZLogChannel.Entity, $"{type} is ({param.ParameterType}) type");
            return false;
        }

        return anim.GetBool(param.Id);
    }

    public static int GetInteger(Animator anim, E_AnimParameter type)
    {
        if (null == anim)
        {
            ZLog.LogWarn(ZLogChannel.Entity, "anim is null");
            return 0;
        }

        var param = Parameters[type];

        if (param.ParameterType != AnimatorControllerParameterType.Int)
        {
            ZLog.LogWarn(ZLogChannel.Entity, $"{type} is ({param.ParameterType}) type");
            return 0;
        }

        return anim.GetInteger(param.Id);
    }

    public static float GetFloat(Animator anim, E_AnimParameter type)
    {
        if (null == anim)
        {
            ZLog.LogWarn(ZLogChannel.Entity, "anim is null");
            return 0f;
        }

        var param = Parameters[type];

        if (param.ParameterType != AnimatorControllerParameterType.Float)
        {
            ZLog.LogWarn(ZLogChannel.Entity, $"{type} is ({param.ParameterType}) type");
            return 0f;
        }

        return anim.GetFloat(param.Id);
    }

    private static bool CheckType(EntityComponentAnimation_Animator component, ZAnimatorIdAndParameterType param, AnimatorControllerParameterType type)
    {
        if (null == component || null == component.Anim)
        {
			ZLog.LogWarn(ZLogChannel.Entity, $"Anim is null | {param.ParameterType}");
            return false;
        }

        if (param.ParameterType != type)
        {
            ZLog.LogWarn(ZLogChannel.Entity, $"Parameter Type ({param.ParameterType}) 체크 바람");
            return false;
        }

		// 실제 Animator에 존재하는지
		if (!component.DicAnimParams.ContainsKey(param.Id))
		{
			return false;
		}

        return true;
    }
}
