using UnityEngine;
using FlatBuffers;
using GameDB;
using MmoNet;
using System.Collections.Generic;

namespace IngameEvent
{
	/// <summary> 인게임에서 사용할 이벤트 actor </summary>
	public class EventActorNpc : IngameEventActorBase
	{
		protected override void SpawnImpl()
		{
			EntityId = ZGimmickManager.Instance.AddNpc();
		}

		protected override void PostCreateMyPawn()
		{
			if(true == DBNpc.TryGet(Tid, out var table))
			{
				ZPawnManager.Instance.DoAdd(GetNpcInfo(table));
			}
		}

		protected override void PostCreatePawn(uint entityId, ZPawn paw)
		{			
		}

		protected override void PostDie()
		{
		}

		private S2C_AddNPCInfo GetNpcInfo(NPC_Table table)
		{			
			FlatBufferBuilder builder = new FlatBufferBuilder(1);
				//, builder.CreateString($"Single_Npc_{EntityId}")
				//, info.CharTid
			var bb = S2C_AddNPCInfo.CreateS2C_AddNPCInfo(builder
				, EntityId
				, Tid
				, ServerPos3.CreateServerPos3(builder, transform.position.x, transform.position.y, transform.position.z)
				, transform.rotation.eulerAngles.y
				, table.RunSpeed);

			builder.Finish(bb.Value);
			return S2C_AddNPCInfo.GetRootAsS2C_AddNPCInfo(builder.DataBuffer);
		}

#if UNITY_EDITOR
		protected override Color GetGizmosColor()
		{
			return Color.yellow;
		}
#endif
	}
}
