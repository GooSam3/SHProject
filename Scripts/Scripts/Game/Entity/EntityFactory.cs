using GameDB;
using MmoNet;
using UnityEngine;

public static class EntityFactory
{
    /// <summary> Entity object 생성 </summary>
    public static ZPawn CreatePawn(EntityDataBase data)
    {
        if(ZPawnManager.Instance.MyEntityId == data.EntityId)
        {
            return CreateEntity<ZPawnMyPc>(data);
        }
        else
        {
            switch(data.EntityType)
            {
                case E_UnitType.Character:
                    return CreateEntity<ZPawnRemotePc>(data);
                case E_UnitType.Monster:
                    return CreateEntity<ZPawnMonster>(data);
                case E_UnitType.NPC:
                    return CreateEntity<ZPawnNpc>(data);
                case E_UnitType.Summon:
                    return CreateEntity<ZPawnSummon>(data);
                case E_UnitType.Object:
                    return CreateEntity<ZObject>(data);
            }            
        }

        return null;
    }

    /// <summary> EntityData 생성 </summary>
    public static EntityDataBase CreateEntityData(S2C_AddCharInfo info)
    {
        ZPawnDataCharacter data = new ZPawnDataCharacter();
        data.DoInitialize(info);

        return data;
    }

    /// <summary> 몬스터(소환수) 데이터 생성 </summary>
    public static EntityDataBase CreateEntityData(S2C_AddMonsterInfo info)
    {
        bool bMonster = info.Parentid == 0;
        ZPawnDataMonster data = null;
        if (bMonster)
        {
            data = new ZPawnDataMonster();
        }
        else
        {
            data = new ZPawnDataSummon();            
        }

        data.DoInitialize(info);

        return data;
    }

    public static EntityDataBase CreateEntityData(S2C_AddNPCInfo info)
    {
        ZPawnDataNpc data = new ZPawnDataNpc();
        data.DoInitialize(info);

        return data;
    }

    public static EntityDataBase CreateEntityData(S2C_AddGatherObj info)
    {
        ZObjectData data = new ZObjectData();
        data.DoInitialize(info);

        return data;
    }

    private static ENTITY_TYPE CreateEntity<ENTITY_TYPE>(EntityDataBase data) where ENTITY_TYPE : EntityBase
    {
        var loadObj = Resources.Load<GameObject>($"Pawn/{typeof(ENTITY_TYPE).Name}");
        if (null == loadObj)
        {
            ZLog.LogError(ZLogChannel.Entity, $"{typeof(ENTITY_TYPE).Name} 을 찾을 수 없다.");
            return null;
        }

        GameObject go = GameObject.Instantiate(loadObj, data.Position.Value, Quaternion.Euler(0, data.Dir, 0));

#if UNITY_EDITOR
        go.name = $"{typeof(ENTITY_TYPE).Name}_{data.EntityId}";
#endif
        ENTITY_TYPE entity = go.GetComponent<ENTITY_TYPE>();
        entity.DoInitialize(data);

        return entity;
    }
}
