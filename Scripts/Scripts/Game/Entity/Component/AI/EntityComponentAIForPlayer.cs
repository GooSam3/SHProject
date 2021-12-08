using GameDB;
using NodeCanvas.Framework;
using UnityEngine;

/// <summary> ZPawn AI </summary>
public class EntityComponentAIForPlayer : EntityComponentAI
{
	public void StartAI(E_PawnAIType aiType, uint stageTid, Vector3 goalPosition, uint targetTid, bool bForce = false)
	{
		//같은 ai 타입이면 취소

		if (true == bForce)
		{
			StopAI(true);
		}
		else if (CurrentAIType == aiType)
		{
			StopAI(false);
			return;
		}

		Blackboard.AddVariable(ZBlackbloardKey.StageTid, stageTid);
		Blackboard.AddVariable(ZBlackbloardKey.GoalPosition, goalPosition);
		Blackboard.AddVariable(ZBlackbloardKey.TargetTid, targetTid);

		StartAI(aiType);
	}

	/// <summary> AI 정지 </summary>
	protected override void StopAI(bool bNext)
	{
		base.StopAI(bNext);
		//자동 전투 ai는 항상 다시 켜짐
		if (false == bNext)
		{
			Owner.StopMove();
			StartAI(E_PawnAIType.AutoBattle);
		}
	}
}
