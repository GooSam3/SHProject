
namespace NodeCanvas.Tasks.Conditions
{
	/// <summary> 내 타겟이 있는지 체크 </summary>
	public class ZAICondition_IsGetTarget : ZAIConditionBase
	{
		protected override string info
		{
			get { return agent && agent.GetTarget() ? $"Target Name : {agent.GetTarget().EntityData.Name}" : "내 타겟이 있는지 체크"; }
		}

		protected override bool OnCheck()
		{
			var target = agent.GetTarget();
			return null != target && false == target.IsDead;
		}
	}
}