using GameDB;
using NodeCanvas.Framework;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions
{
    public class ZAIAction_MoveToPosition : ZAIActionBase
    {
		protected override string info
		{
			get { return "목적지로 이동"; }
		}
		protected override void OnExecute()
        {
			if(agent.IsMoving())
            {
				EndAction(false);
				return;
            }

			Vector3 pos = blackboard.GetVariableValue<Vector3>(ZBlackbloardKey.GoalPosition);

			agent.MoveTo(pos, agent.MoveSpeed); 
			EndAction(true);
		}
	}
}
