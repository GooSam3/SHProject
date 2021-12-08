using UnityEngine;
using System.Collections;

namespace IngameEvent
{
	/// <summary> 카메라 원복! </summary>
	public class Sequence_ResetCamera : IngameEventSequenceBase
	{
		[Header("reset blend type")]
		[SerializeField]
		private Cinemachine.CinemachineBlendDefinition.Style BlendStyle = Cinemachine.CinemachineBlendDefinition.Style.EaseIn;

		protected override void BeginEventImpl()
		{			
			CameraManager.Instance.DoResetMotor(BlendStyle);
			EndEvent();
		}

		protected override void EndEventImpl()
		{
		}
	}
}
