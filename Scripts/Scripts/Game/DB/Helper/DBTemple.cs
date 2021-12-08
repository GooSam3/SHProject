using GameDB;
using System.Linq;

[UnityEngine.Scripting.Preserve]
public class DBTemple : IGameDBHelper
{

    public void OnReadyData()
    {
    }

    public static bool TryGet(uint _tid, out GameDB.Temple_Table outTable)
    {
        return GameDBManager.Container.Temple_Table_data.TryGetValue(_tid, out outTable);
    }

    public static GameDB.Temple_Table Get(uint _tid)
    {
        if (GameDBManager.Container.Temple_Table_data.TryGetValue(_tid, out var foundTable))
            return foundTable;

        ZLog.LogError(ZLogChannel.System, $"해당 TempleID[{_tid}]가 테이블에 존재하지 않습니다.");

        return null;
    }
     /// <summary>  TempleTyped에 따라 데이터 내려주기 </summary>
    public static System.Collections.Generic.List<Temple_Table> GetList(E_TempleType type)
    {
        var templeTableList = GameDBManager.Container.Temple_Table_data.Values.ToList();
        if (templeTableList == null)
        {
            ZLog.LogError(ZLogChannel.System, $"TempleTable.");
            return null;
        }
        System.Collections.Generic.List<Temple_Table> typeTempleTableList  =  templeTableList.FindAll(obj => obj.TempleType == type);
   
        return typeTempleTableList;
    }

    /// <summary> 스테이지 ID로 TempleTable 데이터를 얻어온다. </summary>
    public static Temple_Table GetTempleTableByStageTid(uint stageTid)
    {
        if(DBStage.TryGet(stageTid, out var stageTable))
        {
            foreach (var portal in GameDBManager.Container.Portal_Table_data)
            {
                if (portal.Value.StageID == stageTid)
                {
                    foreach (var temple in GameDBManager.Container.Temple_Table_data)
                    {
                        if (temple.Value.EntrancePortalID == portal.Value.PortalID)
                        {
                            return temple.Value;
                        }
                    }
                    return null;
                }
            }
        }

        return null;
    }
}
