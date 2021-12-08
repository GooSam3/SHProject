using NodeCanvas.Framework;
using UnityEngine;

namespace NodeCanvas.Tasks.Conditions
{
    public class ZAICondition_IsQuestPositionArrive : ZAIConditionBase
	{
		public float CheckDistance = 20f; 

        protected override string OnInit()
        {
            return base.OnInit();
        }

        protected override string info
		{
			get { return "퀘스트 목적지에 도착했는지 확인"; }
		}

		protected override bool OnCheck()
		{
			//Quest_Table questTable = DBQuest.GetQuestData(myEntity.CurrentQuestTid);

			//if(questTable == null)
			//         {
			//	return false;
			//         }

			//DestinationPosition.x = questTable.MapPos[0];
			//DestinationPosition.y = questTable.MapPos[1];
			//DestinationPosition.z = questTable.MapPos[2];

			var pos = blackboard.GetVariableValue<Vector3>(ZBlackbloardKey.GoalPosition);

			float sqrDist = (agent.Position - pos).sqrMagnitude;
			
			return sqrDist < CheckDistance * CheckDistance && !agent.IsMoving();
		}
	}
}
