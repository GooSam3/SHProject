using UnityEngine;
using System;
using System.Collections.Generic;

namespace IngameEvent
{
	/// <summary> 인게임에서 사용할 이벤트 처리 </summary>
	public class IngameEventPlayer : MonoBehaviour
	{
		[ReadOnly]
		[SerializeField]
		/// <summary> 현재 진행할 이벤트 그룹 </summary>
		protected E_IngameEventGroup CurrentEventGroup = E_IngameEventGroup.None;

		/// <summary> 이벤트 종료 </summary>
		private Action mEventFinish;

		private Queue<IngameEventSequenceBase> m_queSequence;

		[Header("종료시 생성된 모든 오브젝트를 파괴할지 여부")]
		[SerializeField]
		private bool IsDestroyOnEnd = false;

		[Header("시작 딜레이 타임")]
		[SerializeField]
		private float StartDelayTime = 0f;

		/// <summary> 현재 진행중인 시퀀스 </summary>
		public IngameEventSequenceBase Current { get; private set; }

		private void Start()
		{
			Initialize();
		}

		private void Initialize()
		{
			m_queSequence = new Queue<IngameEventSequenceBase>(GetComponentsInChildren<IngameEventSequenceBase>());

			ZPawnManager.Instance.DoAddEventCreateMyEntity(HandleCreateMyEntity);			
		}

		private void HandleCreateMyEntity()
		{
			ZPawnManager.Instance.DoRemoveEventCreateMyEntity(HandleCreateMyEntity);
			Invoke(nameof(PlayNextSequence), StartDelayTime);
		}

		/// <summary> 다음 시퀀스를 플레이한다. </summary>
		public void PlayNextSequence()
		{			
			if (0 >= m_queSequence.Count)
			{
				EndEventPlayer();
				return;
			}
			
			Current = m_queSequence.Dequeue();

			// 현재 CurrentEventGroup가 E_IngameEventGroup.None 이면 무조건 플레이. 아니라면 체크
			if (CurrentEventGroup != E_IngameEventGroup.None && false == Current.EventGroup.HasFlag(CurrentEventGroup))
			{
				PlayNextSequence();
				return;
			}

			Current.BeginEvent(this);
		}

		/// <summary> 이벤트 완료 </summary>
		private void EndEventPlayer()
		{
			//카메라 원복
			CameraManager.Instance.DoResetMotor();
			ZPawnManager.Instance.MyEntity.IsBlockMoveMyPc = false;

			mEventFinish?.Invoke();

			if(IsDestroyOnEnd)
			{
				GameObject.Destroy(gameObject);
			}
		}

		/// <summary> 이벤트가 변경된다면 처리 </summary>
		public void ChangeEventGroup(E_IngameEventGroup group)
		{
			CurrentEventGroup = group;
		}

		/// <summary> 인게임 이벤트 생성 </summary>
		public static void PlayIngameEvent(string prefabName, Action<IngameEventPlayer> action, Action onFinish)
		{
			ZResourceManager.Instance.Instantiate(prefabName, (assetName, go) =>
			{
				if(null == go)
				{
					action?.Invoke(null);
					return;
				}

				var player = go.GetComponent<IngameEventPlayer>();

				if(null != player)
				{
					player.mEventFinish = onFinish;
				}

				action?.Invoke(player);
			});
		}
	}
}
