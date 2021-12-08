using UnityEngine;

namespace IngameEvent
{
	/// <summary> 인게임에서 사용할 이벤트 actor </summary>
	public class EventActorMonster : IngameEventActorBase
	{		
		protected override void SpawnImpl()
		{
			EntityId = ZGimmickManager.Instance.AddMonster();
		}

		protected override void PostCreateMyPawn()
		{
			ZMmoManager.Instance.Field.REQ_MonsterSpawnReq(Tid, transform.position, transform.rotation);
		}

		protected override void PostCreatePawn(uint entityId, ZPawn paw)
		{			
		}

		protected override void PostDie()
		{
		}

#if UNITY_EDITOR
		protected override Color GetGizmosColor()
		{
			return Color.red;
		}
#endif
	}
}
