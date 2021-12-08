using GameDB;

/// <summary> 소환 Entity 데이터 (몬스터 기반)</summary>
public class ZPawnDataSummon : ZPawnDataMonster
{
    public override E_UnitType EntityType { get { return E_UnitType.Summon; } }
}
