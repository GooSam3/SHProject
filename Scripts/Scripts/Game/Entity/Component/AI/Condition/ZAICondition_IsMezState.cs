using GameDB;

namespace NodeCanvas.Tasks.Conditions
{
	public class ZAICondition_IsMezState : ZAIConditionBase
	{
		public E_ConditionControl Type;

		protected override string info
		{
			get { return $"CheckMezType : {Type}"; }
		}

		protected override bool OnCheck()
		{
			return agent.IsMezState(Type);
		}
	}
}