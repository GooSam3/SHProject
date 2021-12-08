using UnityEngine;
using System.Collections;

namespace IngameEvent
{
	/// <summary> 인게임에서 사용할 이벤트 개별 기능 </summary>
	public abstract class IngameEventSequenceBase : MonoBehaviour
	{
		protected IngameEventPlayer EventPlayer { get; private set; }

		[Header("분기 처리를 위한 그룹")]
		[SerializeField]		
		public E_IngameEventGroup EventGroup = E_IngameEventGroup.None;

		[Header("시퀀스 시작 지연 시간")]
		[SerializeField]
		private float StartDelayTime = 0f;

		[Header("내 pc의 이동을 막는다.")]
		[SerializeField]
		private bool IsBlockMoveMyPcByBegin = false;

		[Header("sequence 종료시 내 pc의 이동을 풀어준다.")]
		[SerializeField]
		private bool IsEnableMoveMyPcByEnd = false;

		private bool IsEndSequence = false;

		public void BeginEvent(IngameEventPlayer owner)
		{
			EventPlayer = owner;

			if (IsBlockMoveMyPcByBegin)
			{
				ZPawnManager.Instance.MyEntity.StopMove();

				ZPawnManager.Instance.MyEntity.IsBlockMoveMyPc = true;
			}
				

			Invoke(nameof(InvokeDelayStart), StartDelayTime);
		}

		private void InvokeDelayStart()
		{
			BeginEventImpl();

			StartCoroutine(Co_Update());
		}

		/// <summary> 각 시퀀스 종료시 호출해야함 </summary>
		protected void EndEvent()
		{
			if (true == IsEndSequence)
				return;

			IsEndSequence = true;

			StopAllCoroutines();
			EndEventImpl();

			if (IsEnableMoveMyPcByEnd)
				ZPawnManager.Instance.MyEntity.IsBlockMoveMyPc = false;

			EventPlayer.PlayNextSequence();
		}

		protected virtual IEnumerator Co_Update()
		{
			yield return null;
		}

		protected abstract void BeginEventImpl();
		protected abstract void EndEventImpl();
	}
}
