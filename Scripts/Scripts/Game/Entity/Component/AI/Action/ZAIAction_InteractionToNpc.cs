using GameDB;
using NodeCanvas.Framework;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions
{
	/// <summary> npc와 대화 </summary>
	public class ZAIAction_InteractionToNpc : ZAIActionBase
	{
		protected override string info => "Npc 상호작용 후 ai 종료";
		protected override void OnExecute()
		{
			uint tid = blackboard.GetVariableValue<uint>(ZBlackbloardKey.TargetTid);
			
			if (0 == tid)
			{
				EndAction(false);
				return;
			}

			var entity = ZPawnManager.Instance.FindEntityByTid(tid, E_UnitType.NPC);

			if(null == entity)
            {
				EndAction(false);
				return;
			}

			ZPawnNpc npc = entity as ZPawnNpc;

			if (null == npc)
			{
				EndAction(false);
				return;
			}

			var touchRange = npc.NpcData.TableData.TouchRange;

			if ((touchRange * touchRange) < Vector3.SqrMagnitude(npc.Position - agent.Position))
			{
				EndAction(false);
				return;
			}


			agent.StopMove();
			npc.Interact(agent);

			//ai 멈춤
			agent.StopAI(E_PawnAIType.TalkNpc);

			EndAction(true);
		}
	}
}