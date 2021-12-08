
namespace NodeCanvas.Tasks.Conditions
{
	/// <summary> 스킬 사용중인지 여부 체크 </summary>
	public class ZAICondition_IsSkillAction : ZAIConditionBase
	{
		protected override string info
		{
			get { return agent ? $"SkillAction : {agent.IsSkillAction}" : "스킬 사용중인지 여부 체크"; }
		}

		protected override bool OnCheck()
		{
			return agent.IsSkillAction;
		}
	}
}