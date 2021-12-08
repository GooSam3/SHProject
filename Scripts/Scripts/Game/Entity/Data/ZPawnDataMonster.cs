using GameDB;
using MmoNet;
using UnityEngine;
using System.Collections.Generic;

/// <summary> 몬스터 Entity 데이터 </summary>
public class ZPawnDataMonster : ZPawnDataBase
{
    public override E_UnitType EntityType { get { return E_UnitType.Monster; } }

    /// <summary> 소환수일 경우 부모의 Entity id </summary>
    public uint ParentEntityId { get; private set; }

    private Monster_Table mTable;
    public Monster_Table Table { get { return mTable; } }

    public void DoInitialize(S2C_AddMonsterInfo info)
    {
        EntityId = info.Objectid;

        TableId = info.MonsterTid;
        Dir = info.Dir;
        MoveSpeed = info.Movespeed;

        ParentEntityId = info.Parentid;

        if (DBMonster.TryGet(info.MonsterTid, out mTable))
        {
            Name = DBLocale.GetText(mTable.MonsterTextID);
        }
        else
        {
            ZLog.Log(ZLogChannel.Entity, ZLogLevel.Error, $"해당 몬스터 TID({info.MonsterTid})를 찾을 수 없다.");
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

        MaxHp = info.MaxHp;
        CurrentHp = info.CurHp;
        MaxMp = info.MaxMp;
        CurrentMp = info.CurMp;

        for (int i = 0; i < info.AbilactionsLength; ++i)
        {
            AbilActionInfo value = info.Abilactions(i).Value;
            dicAbilityAction.Add(value.AbilactionTid, new KeyValuePair<ulong, bool>(value.ExpireDt, value.NotConsume));
        }

        MezState = (E_ConditionControl)info.Mezstate;
    }
}
