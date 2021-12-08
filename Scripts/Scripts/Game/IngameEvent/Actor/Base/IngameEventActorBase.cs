using UnityEngine;
using System;
using System.Collections.Generic;

namespace IngameEvent
{
	/// <summary> 인게임에서 사용할 이벤트 actor </summary>
	public abstract class IngameEventActorBase : MonoBehaviour
	{
		[Header("소환할 캐릭터/npc/monster Tid")]
		[SerializeField]
		protected uint Tid;

		[Header("캐릭터 이미지")]		
		public string ActorImageName;

		[Header("캐릭터의 이름")]
		[ReadOnly]
		public string ActorName;


		[SerializeField]
		private List<IngameEventAnimation> CustomAnimations;

		/// <summary> 생성한 pawn의 entityID </summary>
		public uint EntityId { get; protected set; }

		/// <summary> 생성된 pawn </summary>
		public ZPawn Pawn { get; protected set; }

		/// <summary> 사망 여부 </summary>
		public bool IsDead { get; private set; }

		private Action mEventSpawned;

		/// <summary> 현재 진행중인 액션 </summary>
		private EventActorActionBase CurrentAction;

		public Transform Trans { get { return Pawn?.transform ?? transform; } }

		public Vector3 Forward { get { return Trans.forward; } }
		public Vector3 Postion { get { return Pawn?.transform?.position ?? transform.position; } }

		public void Spawn(Action onSpawned)
		{
			mEventSpawned = onSpawned;
			if(null == Pawn)
			{
				SpawnImpl();

				ZPawnManager.Instance.DoAddEventCreateMyEntity(HandleCreateMyPawn);
			}
			else
			{
				HandleCreatePawn(Pawn.EntityId, Pawn);
			}
		}

		protected void HandleCreateMyPawn()
		{
			ZPawnManager.Instance.DoRemoveEventCreateMyEntity(HandleCreateMyPawn);
			ZPawnManager.Instance.DoAddEventCreateEntity(HandleCreatePawn);
			PostCreateMyPawn();
		}

		/// <summary> 실제 폰 생성! </summary>
		protected void HandleCreatePawn(uint entityId, ZPawn pawn)
		{
			if (entityId != EntityId)
				return;

			Pawn = pawn;
			ActorName = Pawn.PawnData.Name;
			Pawn.DoAddEventDie(HandleDie);
			ZPawnManager.Instance.DoRemoveEventCreateEntity(HandleCreatePawn);

			PostCreatePawn(entityId, pawn);

			mEventSpawned?.Invoke();
		}

		/// <summary> 사망 이벤트 </summary>
		protected void HandleDie(uint attackerEntityId, ZPawn pawn)
		{
			Pawn?.DoRemoveEventDie(HandleDie);
			IsDead = true;

			//mmo에서 remove 날라오지 않는다. 걍 삭제
			float delayTime = 0f;
			if (null != Pawn)
			{
				delayTime = Pawn.GetAnimLength(E_AnimStateName.Die_001);
			}
			Invoke(nameof(RemoveMonster), delayTime);
			PostDie();
		}

		protected void RemoveMonster()
		{
			if (ZPawnManager.hasInstance && null != Pawn)
				ZPawnManager.Instance.DoRemove(Pawn.EntityId);
		}

		public void PlayCustomAnimation(string animationKey)
		{
			if (null == CustomAnimations)
				return;

			if (true == string.IsNullOrEmpty(animationKey))
				return;

			var animationData = CustomAnimations.Find((data) => data.AnimationKey.Equals(animationKey));

			if (null == animationData)
				return;

			if(null != animationData.Clip)
				Pawn?.ChangeAnimationClip(animationData.AnimState, animationData.Clip);

			Pawn?.SetAnimParameter(animationData.AnimParameter);
		}

		/// <summary> 엑터에게 특수한 액션을 수행시킨다. </summary>
		public void PlayActorAction(EventActorActionBase action, Action onFinishAction)
		{
			if(null != CurrentAction)
			{
				CurrentAction.StopAction();
			}

			CurrentAction = action;

			if(null == action)
			{
				ZLog.LogError(ZLogChannel.Quest, $"action이 셋팅되어 있지 않다.");
				onFinishAction?.Invoke();
				return;
			}

			CurrentAction.BeginAction(this, onFinishAction, EndActorAction);
		}

		/// <summary> 현재 수행중인 액션을 종료한다. </summary>
		public void StopActorAction()
		{
			if (null != CurrentAction)
			{
				CurrentAction.StopAction();
			}

			CurrentAction = null;
		}

		private void EndActorAction()
		{
			CurrentAction = null;
		}

		/// <summary> 소환 요청 </summary>
		protected abstract void SpawnImpl();

		/// <summary> 내 캐릭터 생성 이후 처리 </summary>
		protected abstract void PostCreateMyPawn();

		/// <summary> 해당 pawn 생성 이후 처리 </summary>
		protected abstract void PostCreatePawn(uint entityId, ZPawn paw);

		/// <summary> 생성된 pawn이 사망시 처리 </summary>
		protected abstract void PostDie();

		private void OnDestroy()
		{
			if (false == ZPawnManager.hasInstance)
				return;

			if (null == Pawn)
				return;

			if (true == Pawn.IsMyPc)
				return;

			ZPawnManager.Instance.DoRemove(Pawn.EntityId);			
		}


#if UNITY_EDITOR
		protected virtual void OnDrawGizmos()
		{
			Gizmos.color = GetGizmosColor();
			Gizmos.DrawCube(transform.position, Vector3.one);
		}

		protected abstract Color GetGizmosColor();
#endif
	}
}
