using GameDB;
using UnityEngine;

[UnityEngine.Scripting.Preserve]
public class DBPKBuff : IGameDBHelper
{
    public void OnReadyData()
    {
        foreach(var table in GameDBManager.Container.PKBuff_Table_data)
        {
            int value = table.Value.MaxTendencyCount;
            DBConfig.MaxGoodTendency = Mathf.Max(DBConfig.MaxGoodTendency, value);
            DBConfig.MaxEvilTendency = Mathf.Min(DBConfig.MaxEvilTendency, value);
        }
    }

    public static bool TryGet(uint tid, out PKBuff_Table table)
    {
        return GameDBManager.Container.PKBuff_Table_data.TryGetValue(tid, out table);
    }

    public static bool GetTableByTendencyValue(int tendency, out PKBuff_Table table)
    {
        //얼마가 올지모르니 고정
        tendency = Mathf.Clamp(tendency, DBConfig.MaxEvilTendency, DBConfig.MaxGoodTendency);

        table = null;

        foreach(var iter in GameDBManager.Container.PKBuff_Table_data.Values)
        {
            if (iter.MinTendencyCount <= tendency && iter.MaxTendencyCount >= tendency)
            {
                table = iter;
                return true;
            }
        }

        return false;
    }
}
