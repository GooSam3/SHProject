using GameDB;

/// <summary> Monster </summary>
public class ZPawnMonster : ZPawn
{
    public override E_UnitType EntityType { get { return E_UnitType.Monster; } }

    public ZPawnDataMonster MonsterData { get { return EntityData.To<ZPawnDataMonster>(); } }

    public float ReturnRange { get { return MonsterData.Table.ReturnRange; } }
    public float RoamingRange { get { return MonsterData.Table.RoamingRange; } }
    public float SearchRange { get { return MonsterData.Table.SearchRange; } }

    public E_MonsterType MonsterType {get { return MonsterData.Table.MonsterType; } }

    protected override void SetAttributeType()
    {
        if(false == DBMonster.TryGet(TableId, out var table))
        {
            return;
        }

        UnitAttributeType = table.AttributeType;
    }
}
