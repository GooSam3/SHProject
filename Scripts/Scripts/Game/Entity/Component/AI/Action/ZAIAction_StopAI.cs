using GameDB;
using NodeCanvas.Framework;

namespace NodeCanvas.Tasks.Actions
{
	/// <summary> AI 종료 </summary>
	public class ZAIAction_StopAI : ZAIActionBase
	{
		public E_PawnAIType AIType = E_PawnAIType.None;
		protected override void OnExecute()
		{
			if(AIType == E_PawnAIType.None)
            {
				AIType = agent.CurrentAIType;
            }

			agent.StopAI(AIType);

			EndAction(false);
		}
	}
}