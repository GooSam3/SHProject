using GameDB;

/// <summary> 소환수 (몬스터 기반) </summary>
public class ZPawnSummon : ZPawnMonster
{
    public override E_UnitType EntityType { get { return E_UnitType.Summon; } }

    public ZPawnDataSummon SummonData { get { return EntityData.To<ZPawnDataSummon>(); } }

    protected override void SetAttributeType()
    {
        //소환수는 소환 당시 캐릭터의 속성을 따라간다.
        if(ZPawnManager.Instance.TryGetEntity(SummonData.ParentEntityId, out var pawn))
        {
            UnitAttributeType = pawn.UnitAttributeType;
        }
        else
        {
            UnitAttributeType = E_UnitAttributeType.None;
        }
    }
}
