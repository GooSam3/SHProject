using UnityEngine;

namespace IngameEvent
{
	/// <summary> 인게임에서 사용할 이벤트 actor </summary>
	public class EventActorMyPc : IngameEventActorBase
	{				
		protected override void SpawnImpl()
		{			
		}

		protected override void PostCreateMyPawn()
		{
			EntityId = ZPawnManager.Instance.MyEntityId;			
			HandleCreatePawn(ZPawnManager.Instance.MyEntityId, ZPawnManager.Instance.MyEntity);			
		}

		protected override void PostCreatePawn(uint entityId, ZPawn paw)
		{			
		}

		protected override void PostDie()
		{
		}

#if UNITY_EDITOR
		protected override void OnDrawGizmos()
		{
		}

		protected override Color GetGizmosColor()
		{
			return Color.green;
		}
#endif
	}
}
