using UnityEngine;
using System.Collections;

namespace IngameEvent
{
	/// <summary> 트리거 충돌까지 대기 </summary>
	[RequireComponent(typeof(SphereCollider))]
	public class Sequence_WaitTrigger : IngameEventSequenceBase
	{
		private bool IsWait = true;

		[Header("Actor가 셋팅되어 있다면 trigger가 해당 Actor를 따라다닌다.")]
		[SerializeField]
		protected IngameEventActorBase Actor;

		[Header("트리거 밖으로 나갈경우 처리할지 여부")]
		[SerializeField]
		private bool IsExit = false;

		protected override void BeginEventImpl()
		{
			GetComponent<SphereCollider>().isTrigger = true;
			GetComponent<SphereCollider>().gameObject.layer = UnityConstants.Layers.EventTrigger;
		}

		protected override void EndEventImpl()
		{
		}

		protected override IEnumerator Co_Update()
		{			
			while(IsWait)
			{
				if (null != Actor)
					transform.position = Actor.Postion;
				yield return null;
			}			
			EndEvent();
		}

		protected void OnTriggerEnter(Collider other)
		{
			if (false == IsWait)
				return;

			if (true == IsExit)
				return;

			CheckTrigger(other);
		}

		protected void OnTriggerExit(Collider other)
		{
			if (false == IsWait)
				return;

			if (false == IsExit)
				return;

			CheckTrigger(other);
		}

		private void CheckTrigger(Collider other)
		{
			var pc = other.GetComponent<ZPawnMyPc>();

			if (null == pc)
				return;

			IsWait = false;
		}
	}
}
