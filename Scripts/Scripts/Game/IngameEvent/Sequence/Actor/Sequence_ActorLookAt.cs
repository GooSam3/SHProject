using UnityEngine;
using System.Collections;

namespace IngameEvent
{
	/// <summary> 액터 바라보기 </summary>
	public class Sequence_ActorLookAt : Sequence_ActorBase
	{
		[Header("바라볼 위치 (Target이 Actor 타입이면 소환된 Actor를 바라본다.)")]
		[SerializeField]
		private Transform Target;

		[Header("바라보는 속도")]
		[SerializeField]
		private float RotateSpeed = 10f;

		[Header("이동이 끝날때까지 대기할지 여부")]
		[SerializeField]
		private float LookDuration = 0f;

		[Header("LookDuration 동안 대기할지 여부")]
		[SerializeField]
		private bool IsWaitLookDuration = false;

		[Header("카메라 연출 타입 (LookAt Camera) 변경 여부 (내 pc 만)")]
		[SerializeField]
		private bool IsLookAtCamera = false;

		protected override void BeginEventImpl()
		{
		}

		protected override IEnumerator Co_Update()
		{
			var pawn = Pawn;

			//일단 다음 스텝 실행!!
			if(false == IsWaitLookDuration)
				EndEvent();

			var target = Target.GetComponent<IngameEventActorBase>();

			if (null != target && null != target.Pawn)
				Target = target.Pawn.transform;

			if (null != pawn)
			{
				float time = 0f;
				while (LookDuration > time)
				{
					time += Time.smoothDeltaTime;

					Vector3 targetPos = Target.position;
					targetPos.y = pawn.transform.position.y;
					var dir = (targetPos - pawn.transform.position).normalized;

					pawn.transform.forward = Vector3.Lerp(pawn.transform.forward, dir, Time.smoothDeltaTime * RotateSpeed);

					yield return null;
				}

				pawn.LookAt(Target);
			}

			if (true == IsWaitLookDuration)
				EndEvent();
		}

		protected override void EndEventImpl()
		{
		}
	}
}
