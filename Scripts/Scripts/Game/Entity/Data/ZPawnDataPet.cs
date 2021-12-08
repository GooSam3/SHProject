using GameDB;

/// <summary> Pet Entity 데이터 </summary>
public class ZPawnDataPet : ZPawnDataBase
{
    public override E_UnitType EntityType { get { return E_UnitType.Pet; } }

    public void DoInitialize(uint tableId)
    {
        TableId = tableId;
    }
}
