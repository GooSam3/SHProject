using GameDB;
using MmoNet;
using System.Collections.Generic;
using UnityEngine;

/// <summary> 캐릭터 Entity 데이터 </summary>
public class ZPawnDataCharacter : ZPawnDataBase
{
    public override E_UnitType EntityType { get { return E_UnitType.Character; } }

    public uint ChangeTid { get; private set; }
    public uint PetTid { get; private set; }
    public uint VehicleTid { get; private set; }
    public uint ServerIdx { get; private set; }
    public uint GuildMarkId;
    public ulong GuildId;

    public void DoInitialize(S2C_AddCharInfo info)
    {        
        EntityId = info.Objectid;
        TableId = info.CharTid;
        CharacterId = info.CharId;
        Dir = info.Dir;
        MoveSpeed = info.Movespeed;
        Name = info.Name;
        ServerIdx = info.Serverid;
        GuildId = info.GuildId;
        GuildMarkId = info.GuildmarkTid;
        Tendency = info.Tendency;
        Team = info.Team;

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

        if (info.DestPos.HasValue)
        {
            ServerPos3 destPos = info.DestPos.Value;
            DestPositions.Add(new Vector3(destPos.X, destPos.Y, destPos.Z));
        }

        MaxHp = info.MaxHp;
        CurrentHp = info.CurHp;
        MaxMp = info.MaxMp;
        CurrentMp = info.CurMp;

        for (int i = 0; i < info.AbilactionsLength; ++i)
        {
            AbilActionInfo value = info.Abilactions(i).Value;
            dicAbilityAction.Add(value.AbilactionTid, new KeyValuePair<ulong, bool>(value.ExpireDt, value.NotConsume));
        }

        ChangeTid = info.ChangeId;
        MezState = (E_ConditionControl)info.Mezstate;
        PetTid = info.Pettid;
        VehicleTid = info.Vehicletid;
    }

    public void DoChangePetTid(uint petTid)
    {
        PetTid = petTid;
    }

    public void DoChangeClassTid(uint changeTid)
    {
        ChangeTid = changeTid;
    }

    public void DoChangeVehicleTid(uint vehicleTid)
    {
        VehicleTid = vehicleTid;
    }
}
