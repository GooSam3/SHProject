using UnityEngine;
using System.Collections;

namespace IngameEvent
{
	/// <summary> 액터 이동 </summary>
	public class Sequence_ActorMove : Sequence_ActorBase
	{
		[Header("목적지")]
		[SerializeField]
		private Transform DestPosition;

		[Header("이동 속도")]
		[SerializeField]
		private float MoveSpeed = 5f;

		[Header("이동이 끝날때까지 대기할지 여부")]
		[SerializeField]
		private bool IsWaitMove;

		protected override void BeginEventImpl()
		{
		}

		protected override IEnumerator Co_Update()
		{
			var pawn = Pawn;

			if(null != pawn && null != DestPosition)
			{
				pawn.MoveTo(DestPosition.position, MoveSpeed);
				if(true == IsWaitMove)
				{
					while(false == pawn.IsMoving())
					{
						yield return null;
					}
				}
			}

			EndEvent();
		}

		protected override void EndEventImpl()
		{
		}

#if UNITY_EDITOR
		protected virtual void OnDrawGizmos()
		{
			if (null == DestPosition)
				return;

			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(DestPosition.position, 0.5f);
		}
#endif
	}
}
