using UnityEngine;
using System.Collections;

namespace IngameEvent
{
	/// <summary> 카메라 원복! </summary>
	public class Sequence_ActorLookAtCamera : Sequence_ActorBase
	{
		[Header("바라볼 대상 Actor")]
		[SerializeField]
		protected IngameEventActorBase LookAtActor;

		[Header("reset blend type")]
		[SerializeField]
		private Cinemachine.CinemachineBlendDefinition.Style BlendStyle = Cinemachine.CinemachineBlendDefinition.Style.EaseIn;

		[Header("바라보는 시간(0일 경우 무제한 - 다른 곳에서 ResetCamera 원복해야함)")]
		[SerializeField]
		private float LookAtDuration = 3f;

		[Header("카메라 대기 연출 대기")]
		[SerializeField]
		private bool IsWaitDuration = true;

		protected override void BeginEventImpl()
		{
			CameraManager.Instance.DoSetTarget(Pawn.transform);

			CameraManager.Instance.DoSetLookAtMotor(LookAtActor.Pawn.transform, BlendStyle);
		}

		protected override void EndEventImpl()
		{
		}

		protected override IEnumerator Co_Update()
		{
			if(0 < LookAtDuration && IsWaitDuration)
			{
				yield return new WaitForSeconds(LookAtDuration);

				CameraManager.Instance.DoResetMotor();

				EndEvent();
			}				
			else
			{
				EndEvent();
			}
		}
	}
}
