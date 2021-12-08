[UnityEngine.Scripting.Preserve]
public class DBTempleObject : IGameDBHelper
{
    public void OnReadyData()
    {
    }

    public static bool TryGet(uint _tid, out GameDB.TempleObject_Table outTable)
    {
        return GameDBManager.Container.TempleObject_Table_data.TryGetValue(_tid, out outTable);
    }

    public static GameDB.TempleObject_Table Get(uint _tid)
    {
        if (GameDBManager.Container.TempleObject_Table_data.TryGetValue(_tid, out var foundTable))
            return foundTable;

        ZLog.LogError(ZLogChannel.System, $"해당 TempleID[{_tid}]가 테이블에 존재하지 않습니다.");

        return null;
    }
}
