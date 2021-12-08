using GameDB;
using MmoNet;
using UnityEngine;

/// <summary> Npc Entity 데이터 </summary>
public class ZPawnDataNpc : ZPawnDataBase
{
    public override E_UnitType EntityType { get { return E_UnitType.NPC; } }

	public NPC_Table TableData;

    public void DoInitialize(S2C_AddNPCInfo info)
    {
        EntityId = info.Objectid;

        TableId = info.NpcTid;
        Dir = info.Dir;
        MoveSpeed = info.Movespeed;

        if (DBNpc.TryGet(info.NpcTid, out NPC_Table table))
        {
            Name = DBLocale.GetText(table.NPCTextID);
			TableData = table;

		}
        else
        {
            ZLog.Log(ZLogChannel.Entity, ZLogLevel.Error, $"해당 NPC TID({info.NpcTid})를 찾을 수 없다.");
        }

        if (info.Pos.HasValue)
        {
            ServerPos3 pos = info.Pos.Value;
            Position = new Vector3(pos.X, pos.Y, pos.Z);
        }
        else
        {
            Position = null;
        }

        DestPositions.Clear();

        for (int i = 0; i < info.DestPosLength; ++i)
        {
            ServerPos3 dest = info.DestPos(i).Value;

            DestPositions.Add(new Vector3(dest.X, dest.Y, dest.Z));
        }
    }
}
