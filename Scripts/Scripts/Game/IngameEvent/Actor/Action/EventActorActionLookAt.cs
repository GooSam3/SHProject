using UnityEngine;
using System.Collections.Generic;

namespace IngameEvent
{
	/// <summary> 타겟을 바라본다. </summary>
	public class EventActorActionLookAt : EventActorActionBase
	{
		[Header("바라볼 대상")]
		[SerializeField]
		private IngameEventActorBase Target;

		[Header("회전 속도")]
		[SerializeField]
		private float RotateSpeed = 10f;

		protected override void BeginActionImpl()
		{
			if (null == Target)
			{
				ZLog.LogError(ZLogChannel.Quest, "타겟이 없다!");
				return;
			}
				

			ZMonoManager.Instance.AddUpdateCall(ZMonoManager.UpdateMode.LateUpdate, HandleLateUpdate);
		}

		protected override void StopActionImpl()
		{
			if(ZMonoManager.hasInstance)
				ZMonoManager.Instance.RemoveUpdateCall(ZMonoManager.UpdateMode.LateUpdate, HandleLateUpdate);
		}

		private void HandleLateUpdate()
		{
			Vector3 targetPos = Target.Postion;
			Vector3 ownerPos = Actor.Postion;
			targetPos.y = ownerPos.y;
			var dir = (targetPos - ownerPos).normalized;

			Actor.Trans.forward = Vector3.Lerp(Actor.Forward, dir, Time.smoothDeltaTime * RotateSpeed);
		}
	}
}
