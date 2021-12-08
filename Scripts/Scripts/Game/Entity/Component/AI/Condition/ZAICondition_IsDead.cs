namespace NodeCanvas.Tasks.Conditions
{
	public class ZAICondition_IsDead : ZAIConditionBase
	{
		protected override string info
		{
			get { return agent ? $"Dead : {agent.IsDead}" : "사망 여부 체크"; }
		}

		protected override bool OnCheck()
		{
			return agent.IsDead;
		}
	}
}