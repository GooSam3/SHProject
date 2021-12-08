using NodeCanvas.Framework;

namespace NodeCanvas.Tasks.Conditions
{
	/// <summary> 스킬이 셋팅되어있을 때. 스킬 사용가능 여부 </summary>
	public class ZAICondition_IsInSkillRange : ZAIConditionBase
	{
		protected override string info
		{
			get { return $"스킬이 셋팅되어있을 때. 스킬 사용가능 여부"; }
		}

		protected override bool OnCheck()
		{
			var target = blackboard.GetVariable<EntityBase>(ZBlackbloardKey.Target);
			var skillTid = blackboard.GetVariable<uint>(ZBlackbloardKey.SkillTid);

			if (null == skillTid)
				return false;

			if (null == target)
				return false;

			var skill = agent.SkillSystem.GetSkillInfoById(skillTid.value);

			if (null == skill)
				return false;

			float distance = (target.value.Position - agent.Position).magnitude;

			return distance <= skill.Distance;
		}
	}
}