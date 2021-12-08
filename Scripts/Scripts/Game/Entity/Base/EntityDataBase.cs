using GameDB;
using MmoNet;
using System.Collections.Generic;
using UnityEngine;

/// <summary> 모든 EntityData 기본 데이터 클래스 </summary>
public abstract class EntityDataBase
{
    /// <summary> Entity Unique Id </summary>
    public uint EntityId { get; protected set; }
    public uint TableId { get; protected set; }
    public ulong CharacterId { get; protected set; }
    public abstract E_UnitType EntityType { get; }

    public string Name { get; protected set; }

    public float MaxHp { get; protected set; }
    public float CurrentHp { get; protected set; }
    public float MaxMp { get; protected set; }
    public float CurrentMp { get; protected set; }

    public Vector3? Position { get; protected set; }
    public float Dir { get; protected set; }

    public uint Team { get; protected set; }

    public bool IsMyPc { get { return ZPawnManager.Instance.MyEntityId == EntityId; } }

    public T To<T>() where T : EntityDataBase
    {
        return this as T;
    }
}

public abstract class ZPawnDataBase : EntityDataBase
{    
    public float MoveSpeed { get; protected set; }
        
    public List<Vector3> DestPositions { get; protected set; } = new List<Vector3>();

    public E_ConditionControl MezState { get; set; }

    public Dictionary<uint, KeyValuePair<ulong, bool >> dicAbilityAction = new Dictionary<uint, KeyValuePair<ulong, bool>>();

    public int Tendency { get; set; }

    public void SetMoveData(CS_MoveToDir info)
    {
        MoveSpeed = info.Speed;
        Dir = info.Dir;

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
    }

    public void SetMoveData(CS_MoveToDest info)
    {
        MoveSpeed = info.Speed;

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

        for (int i = 0; i < info.DestLength; ++i)
        {
            ServerPos3 dest = info.Dest(i).Value;
            DestPositions.Add(new Vector3(dest.X, dest.Y, dest.Z));
        }
    }

    public void SetMoveData(S2C_ForceMove info)
    {
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
    }

    public void SetMoveData(CS_MoveStop info)
    {
        Dir = info.Lookdir;

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
    }

    public void AddAbilityAction(uint abilityActionId, ulong expireDt, bool notConsume)
    {
        if (false == dicAbilityAction.ContainsKey(abilityActionId))
            dicAbilityAction.Add(abilityActionId, new KeyValuePair<ulong, bool>(expireDt, notConsume));
        else
            dicAbilityAction[abilityActionId] = new KeyValuePair<ulong, bool>(expireDt, notConsume);
    }

    public void RemoveAbilityAction(uint abilityActionId)
    {
        dicAbilityAction.Remove(abilityActionId);
    }
}

public class ZGimmickDataBase : EntityDataBase
{
    public override E_UnitType EntityType => E_UnitType.Gimmick;
}

public class ZObjectData : ZPawnDataBase
{
    public override E_UnitType EntityType => E_UnitType.Object;

    public void DoInitialize(S2C_AddGatherObj info)
    {
        EntityId = info.Objectid;
        TableId = info.GatherTid;
        Dir = info.Dir;        
        
        if (DBObject.TryGet(info.GatherTid, out Object_Table table))
        {
            Name = DBLocale.GetText(table.ObjectTextID);
        }
        else
        {
            ZLog.Log(ZLogChannel.Entity, ZLogLevel.Error, $"해당 오브젝트 TID({info.GatherTid})를 찾을 수 없다.");
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

        MaxHp = 1;
        CurrentHp = 1;
        MaxMp = 1;
        CurrentMp = 1;
    }
}
