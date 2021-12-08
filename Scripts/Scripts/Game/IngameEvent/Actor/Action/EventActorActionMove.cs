using UnityEngine;
using System.Collections.Generic;

namespace IngameEvent
{
	/// <summary> 이벤트 actor에게 특수한 형태의 action을 수행하도록 한다. </summary>
	public class EventActorActionMove : EventActorActionBase
	{
		[Header("이동 경로")]
		[SerializeField]
		private List<Transform> Paths = new List<Transform>();

		[Header("이동 속도")]
		[SerializeField]
		private float MoveSpeed = 5f;

		[Header("이동 타입")]
		[SerializeField]
		private E_ActorActionMoveType MoveType;

		[Header("각 이동경로 도착후 대기 시간")]
		[SerializeField]
		private float WaitTime = 0f;

		private int CurrentIndex = -1;

		private bool bReverse = false;

		protected override void BeginActionImpl()
		{
			if(0 >= Paths.Count)
			{
				ZLog.LogError(ZLogChannel.Quest, "경로가 셋팅되지 않음!");
				return;
			}

			Actor.Pawn.DoAddEventMoveState(HnadleMoveState);

			MoveNextPosition();
		}

		protected override void StopActionImpl()
		{			
			Actor?.Pawn?.DoRemoveEventMoveState(HnadleMoveState);
			Actor?.Pawn?.StopMove();
		}

		private void HnadleMoveState(bool bMove)
		{
			if (true == bMove)
				return;

			Invoke(nameof(MoveNextPosition), WaitTime);
		}

		private void MoveNextPosition()
		{
			CancelInvoke(nameof(MoveNextPosition));
			if(bReverse)
				--CurrentIndex;
			else
				++CurrentIndex;

			if(0 > CurrentIndex || Paths.Count <= CurrentIndex)
			{
				switch(MoveType)
				{
					case E_ActorActionMoveType.OneShot:
						{
							StopAction();
						}
						return;
					case E_ActorActionMoveType.PingPong:
						{
							bReverse = !bReverse;
							MoveNextPosition();							
						}
						return;
					case E_ActorActionMoveType.Loop:
						{
							CurrentIndex = 0;
							bReverse = false;
						}
						break;
				}							
			}

			Actor.Pawn.MoveTo(Paths[CurrentIndex].position, MoveSpeed);
		}

#if UNITY_EDITOR
		protected void OnDrawGizmos()
		{
			if(0 < Paths.Count)
			{
				Gizmos.color = Color.green;

				foreach(var path in Paths)
				{
					if (null == path)
						continue;

					Gizmos.DrawSphere(path.position, 0.5f);
				}
			}
		}
#endif
	}
}
