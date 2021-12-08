using GameDB;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions
{
	/// <summary> 타겟 위치로 이동한다. </summary>
	public class ZAIAction_MoveToTarget : ZAIActionBase
	{
		private const float MOVE_DELAY_TIME = 0.2f;
		private float mLastMoveTime = 0f;
		private Vector3? GoalPosition;
		private Vector3? CachedMyPosition;

		protected override void OnUpdate()
		{
			var target = agent.GetTarget();

			if (null == target)
			{
				EndAction(false);
				return;
			}

			if (agent.IsMezState(E_ConditionControl.NotMove))
			{
				EndAction(false);
				return;
			}

			//마지막 이동시간에서 딜레이 시간 처리
			if (mLastMoveTime + MOVE_DELAY_TIME > Time.time)
			{
				return;
			}

			if (null != GoalPosition && GoalPosition == target.Position && null != CachedMyPosition && CachedMyPosition != agent.Position)
			{
				return;
			}

			CachedMyPosition = agent.Position;
			GoalPosition = target.Position;
			mLastMoveTime = Time.time;
			agent.MoveTo(GoalPosition.Value, agent.MoveSpeed);
			EndAction(true);
		}
	}
}